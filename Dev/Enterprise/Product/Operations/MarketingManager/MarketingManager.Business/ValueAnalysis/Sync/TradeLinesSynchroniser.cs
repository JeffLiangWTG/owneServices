using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradeLinesSynchroniser
	{
		#region Constructor

		public TradeLinesSynchroniser(ZGuid orgHeaderPk, ISalesValueAssociatedEntity parentEntity = null, bool isFullSync = false)
		{
			masterFactoryProvider = new BusinessObjectFactoryProvider();
			masterFactoryProvider.Current.RefreshEnabled = false;
			master = masterFactoryProvider.Current.Load<OrgHeader>(orgHeaderPk);
			if (master != null)
			{
				var localMasterSalesCollection = new OrgSalesCollection(master);
				localMasterSalesCollection.Load();
				masterSalesCollection = localMasterSalesCollection;

				ActualSalesLastTradedDates = new Dictionary<ZGuid, ZDate>(masterSalesCollection.Count);
			}
			this.isFullSync = isFullSync;
			this.parentEntity = parentEntity;
		}

		#endregion

		#region Properties

		readonly OrgHeader master;
		readonly OrgSalesCollection masterSalesCollection;
		readonly BusinessObjectFactoryProvider masterFactoryProvider;
		readonly bool isFullSync;
		readonly ISalesValueAssociatedEntity parentEntity;

		BusinessObjectFactory MasterFactory => masterSalesCollection.Factory;
		OrgHeader Master => master;
		OrgSalesCollection MasterSalesCollection => masterSalesCollection;
		BusinessObjectFactoryProvider MasterFactoryProvider => masterFactoryProvider;

		readonly Dictionary<ZGuid, ZDate> ActualSalesLastTradedDates;

		public bool OrganisationFound => master != null;

		#endregion

		public void Execute(TradeLinesSummaryProviderCommon summaryProvider, ZDate from, ZDate to)
		{
			if (!OrganisationFound)
			{
				throw new InvalidOperationException("Organisation was not found");
			}

			var summary = summaryProvider.GetForOrg(Master, from, to);
			Execute(summary);
		}

		public void Execute(TradeLinesSummary tradeSummary)
		{
			if (!OrganisationFound)
			{
				throw new InvalidOperationException("Organisation was not found");
			}

			TradeLaneKey tradeLaneKeyUsedToSetCCD = null;

			if (tradeSummary.IncludeActuals)
			{
				tradeLaneKeyUsedToSetCCD = SetClientCommencedDateAndGetTradeLaneKeyUsed(tradeSummary);

				var existingSalesAndTradeDetails = new OrgSalesAndTradeDictionaryBuilder(MasterFactory, MasterSalesCollection);

				var mainPartitions = TradeLinesValuePartitioner.PartitionActualValues(tradeSummary.ActualValues, PartitionSize);
				tradeSummary.ResetActualValues();

				var mainPartitionFactoryProvider = new BusinessObjectFactoryProvider();

				foreach (var tradeLinesPeriodPartition in mainPartitions.OrderBy(x => x.Period))
				{
					mainPartitionFactoryProvider.Current.RefreshEnabled = false;
					var existingPeriodAndValuePartition = new OrgTradePeriodAndValuePartitionBuilder(mainPartitionFactoryProvider.Current, Master, tradeLinesPeriodPartition.Period, tradeSummary.From, tradeSummary.To, existingSalesAndTradeDetails.TradeDetails);
					DeleteObsoleteActualValues(existingSalesAndTradeDetails, existingPeriodAndValuePartition, tradeLinesPeriodPartition);

					var subPartitionFactoryProvider = new BusinessObjectFactoryProvider();
					foreach (var subPartition in tradeLinesPeriodPartition.TradeLinesPartitions)
					{
						subPartitionFactoryProvider.Current.RefreshEnabled = false;
						CreateOrUpdateActualValues(subPartitionFactoryProvider.Current, subPartition, existingPeriodAndValuePartition, existingSalesAndTradeDetails);

						Save(subPartitionFactoryProvider);
						existingSalesAndTradeDetails.Update();
					}

					subPartitionFactoryProvider.RemoveCurrent();
					existingPeriodAndValuePartition.DeleteDuplicates();
					Save(mainPartitionFactoryProvider);
					tradeLinesPeriodPartition.ClearTradeLinesPartitions();
				}

				mainPartitionFactoryProvider.RemoveCurrent();
				existingSalesAndTradeDetails.DeleteDuplicates();
			}

			if (tradeSummary.IncludeProspect)
			{
				UpdateProspectValues(tradeSummary);
			}

			if (tradeSummary.IncludeActuals && tradeSummary.IncludeProspect)
			{
				UpdateLostValues();
			}

			AddOrUpdateTLSLogAndClientCommencedDateEditLog(tradeSummary, tradeLaneKeyUsedToSetCCD);

			Save(MasterFactoryProvider);
			MasterFactoryProvider.RemoveCurrent();
		}

		public virtual void Save(BusinessObjectFactoryProvider provider)
		{
			try
			{
				provider.SaveCurrentReclaimMemoryAndCreateNew();
			}
			catch (ZSaveException ex) when (IsForeignKeySqlException(ex))
			{
				provider.CreateNewAndReclaimMemoryWithoutSave();
			}
		}

		static bool IsForeignKeySqlException(ZSaveException ex)
		{
			if (ex.InnerException?.InnerException is SqlException sqlEx)
			{
				var dbErrorMatch = new DbErrorMatch(sqlEx);
				if (dbErrorMatch.ExceptionType == DbErrorType.DeleteConflictedWithForeignKey
					|| dbErrorMatch.ExceptionType == DbErrorType.InsertConflictedWithForeignKey
					|| dbErrorMatch.ExceptionType == DbErrorType.UpdateConflictedWithForeignKey)
				{
					return true;
				}
			}

			return false;
		}

		protected virtual int PartitionSize => TradeLinesValuePartitioner.DefaultPartitionSize;

		#region Delete Obsolete Actual Values

		void DeleteObsoleteActualValues(OrgSalesAndTradeDictionaryBuilder existingSalesAndTradeDetails, OrgTradePeriodAndValuePartitionBuilder existingPeriodAndValuePartition, TradeLinesValuePartition tradeLinesPartition)
		{
			var periodsToDelete = new List<OrgTradePeriod>(1);

			var existingDetails = existingSalesAndTradeDetails.TradeDetails.ToDictionary(x => x.PK);

			foreach (var tradePeriod in existingPeriodAndValuePartition.TradePeriods)
			{
				bool isObsolete = true;

				if (existingDetails.TryGetValue(tradePeriod.PAS_PA, out OrgTradeDetail tradeDetail))
				{
					var tradeLaneKey = TradeLinesValueHelper.CreateActualTradeLaneKey(tradeDetail.Parent);

					foreach (var actualValues in tradeLinesPartition.TradeLinesPartitions)
					{
						if (actualValues.TryGetValue(tradeLaneKey, out TradeLaneValue actualTradelane))
						{
							var tradeDetailKey = TradeLinesValueHelper.CreateTradeDetailKey(tradeDetail);
							if (actualTradelane.TradeDetails.TryGetValue(tradeDetailKey, out TradeDetailValue actualTradeDetail))
							{
								var tradePeriodKey = TradeLinesValueHelper.CreateTradePeriodKey(tradePeriod);
								isObsolete = !actualTradeDetail.TradePeriods.ContainsKey(tradePeriodKey);
							}
						}
					}
				}

				if (isObsolete)
				{
					periodsToDelete.Add(tradePeriod);
					tradePeriod.FetchStrategy.FetchForDelete();
				}
			}

			foreach (var tradePeriod in periodsToDelete)
			{
				foreach (var tradeValue in tradePeriod.TradeValues)
				{
					tradeValue.FetchStrategy.FetchForDelete();
				}
			}

			foreach (var tradePeriod in periodsToDelete)
			{
				tradePeriod.Delete();
			}
		}

		#endregion

		#region Update Actual Values

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CreateOrUpdateActualValues(BusinessObjectFactory partitionFactory, IDictionary<TradeLaneKey, TradeLaneValue> tradeLines, OrgTradePeriodAndValuePartitionBuilder existingPeriodAndValuePartition, OrgSalesAndTradeDictionaryBuilder existingSalesAndTradeDetails)
		{
			foreach (var actualTradeLane in tradeLines)
			{
				var sales = CreateOrUpdateTradeLane(partitionFactory, actualTradeLane, existingSalesAndTradeDetails);

				if (!existingSalesAndTradeDetails.TradeDetailsMap.TryGetValue(sales.PK, out Dictionary<TradeDetailKey, OrgTradeDetail> existingDetailsMap))
				{
					existingDetailsMap = new Dictionary<TradeDetailKey, OrgTradeDetail>(1);
					existingSalesAndTradeDetails.TradeDetailsMap.Add(sales.PK, existingDetailsMap);
				}

				foreach (var actualTradeDetail in actualTradeLane.Value.TradeDetails)
				{
					var tradeDetail = CreateOrFindTradeDetail(partitionFactory, sales, existingDetailsMap, actualTradeDetail);

					if (!existingPeriodAndValuePartition.TradePeriodsMap.TryGetValue(tradeDetail.PK, out Dictionary<TradePeriodKey, OrgTradePeriod> existingPeriodsMap))
					{
						existingPeriodsMap = new Dictionary<TradePeriodKey, OrgTradePeriod>(1);
						existingPeriodAndValuePartition.TradePeriodsMap.Add(tradeDetail.PK, existingPeriodsMap);
					}

					foreach (var actualTradePeriod in actualTradeDetail.Value.TradePeriods)
					{
						var tradePeriod = CreateOrUpdateTradePeriod(partitionFactory, tradeDetail, existingPeriodsMap, actualTradePeriod);

						if (!existingPeriodAndValuePartition.TradeValuesMap.TryGetValue(tradePeriod.PK, out Dictionary<TradeValueBreakdownKey, OrgTradeValue> existingValuesMap))
						{
							existingValuesMap = new Dictionary<TradeValueBreakdownKey, OrgTradeValue>(1);
							existingPeriodAndValuePartition.TradeValuesMap.Add(tradePeriod.PK, existingValuesMap);
						}

						foreach (var actualTradeValue in actualTradePeriod.Value.TradeValues)
						{
							CreateOrUpdateTradeValue(partitionFactory, tradePeriod, existingValuesMap, actualTradeValue);
						}
					}
				}
			}
		}

		OrgSales CreateOrUpdateTradeLane(BusinessObjectFactory partitionFactory, KeyValuePair<TradeLaneKey, TradeLaneValue> actualTradeLane, OrgSalesAndTradeDictionaryBuilder existingSalesAndTradeDetails)
		{
			if (!existingSalesAndTradeDetails.TradeLanesMap.TryGetValue(actualTradeLane.Key, out OrgSales sales))
			{
				//The sales colletion for the current org may not always contain matched existing trade lane - try Factory load
				var query = new ZQuery();
				query.AddToFilter(OrgSalesSchema.OW_IsTraded, ZBool.True);
				query.AddToFilter(OrgSalesSchema.OW_MP_Product, actualTradeLane.Key.ProductPk);
				query.AddToFilter(OrgSalesSchema.OW_OriginTableCode, actualTradeLane.Key.OriginTableCode);
				query.AddToFilter(OrgSalesSchema.OW_DestinationTableCode, actualTradeLane.Key.DestinationTableCode);
				query.AddToFilter(OrgSalesSchema.OW_Service, actualTradeLane.Key.Service);
				AddToFilterWithPossibleEmptyGuid(query, OrgSalesSchema.OW_OriginID, actualTradeLane.Key.OriginPk);
				AddToFilterWithPossibleEmptyGuid(query, OrgSalesSchema.OW_DestinationID, actualTradeLane.Key.DestinationPk);
				AddToFilterWithPossibleEmptyGuid(query, OrgSalesSchema.OW_OH_Supplier, actualTradeLane.Key.Supplier);
				AddToFilterWithPossibleEmptyGuid(query, OrgSalesSchema.OW_OH_Buyer, actualTradeLane.Key.Buyer);
				AddToFilterWithPossibleEmptyGuid(query, OrgSalesSchema.OW_WW, actualTradeLane.Key.WarehousePk);
				AddToFilterWithPossibleEmptyGuid(query, OrgSalesSchema.OW_GC, actualTradeLane.Key.JobCompanyPk);

				//As collection filter uses ZDBOnlyQuery, sales collection with master as Buyer or Supplier only misses an existing trade lane if it is newly added in the same batch
				if (Master.PK == actualTradeLane.Key.Supplier || Master.PK == actualTradeLane.Key.Buyer)
				{
					query.FetchOnlyFromLocalCache = true;
				}

				sales = partitionFactory.LoadTop1<OrgSales>(query);

				if (sales == null)
				{
					sales = partitionFactory.New<OrgSales>();
					using (sales.GetValidationSuspender())
					using (sales.GetDefaultPropertySuspender())
					{
						sales.OW_IsTraded = true;
						sales.OW_MP_Product = actualTradeLane.Key.ProductPk;
						sales.SetOriginIdAndTableCodeDirectly(actualTradeLane.Key.OriginPk, actualTradeLane.Key.OriginTableCode);
						sales.SetDestinationIdAndTableCodeDirectly(actualTradeLane.Key.DestinationPk, actualTradeLane.Key.DestinationTableCode);
						sales.OW_OH_Primary = ZGuid.Empty;
						sales.OW_OH_Supplier = actualTradeLane.Key.Supplier;
						sales.OW_OH_Buyer = actualTradeLane.Key.Buyer;
						sales.SetWarehousePkDirectly(actualTradeLane.Key.WarehousePk);
						sales.SetWarehouseServiceDirectly(actualTradeLane.Key.Service);
						sales.OW_GC = actualTradeLane.Key.JobCompanyPk;
					}
				}

				existingSalesAndTradeDetails.Append(sales);
			}

			return sales;
		}

		static void AddToFilterWithPossibleEmptyGuid(ZQuery query, SchemaColumn column, ZGuid guidValue)
		{
			if (guidValue.IsEmpty)
			{
				query.AddToFilter(column, null);
			}
			else
			{
				query.AddToFilter(column, guidValue);
			}
		}

		OrgTradeDetail CreateOrFindTradeDetail(BusinessObjectFactory partitionFactory, OrgSales sales, Dictionary<TradeDetailKey, OrgTradeDetail> existingDetailsMap, KeyValuePair<TradeDetailKey, TradeDetailValue> actualTradeDetail)
		{
			if (!existingDetailsMap.TryGetValue(actualTradeDetail.Key, out OrgTradeDetail tradeDetail))
			{
				tradeDetail = partitionFactory.New<OrgTradeDetail>();
				existingDetailsMap[actualTradeDetail.Key] = tradeDetail;
				using (tradeDetail.GetValidationSuspender())
				{
					tradeDetail.PA_OW = sales.PK;
					tradeDetail.SetTradeModeDirectly(actualTradeDetail.Key.Mode);
					tradeDetail.SetTradeTypeDirectly(actualTradeDetail.Key.Type);
					tradeDetail.PA_OP = actualTradeDetail.Key.SupplierPartPk;
					tradeDetail.PA_Status = actualTradeDetail.Value.StatusCode;
				}
			}
			return tradeDetail;
		}

		OrgTradePeriod CreateOrUpdateTradePeriod(BusinessObjectFactory partitionFactory, OrgTradeDetail tradeDetail, Dictionary<TradePeriodKey, OrgTradePeriod> existingPeriodsMap, KeyValuePair<TradePeriodKey, TradePeriodValue> actualTradePeriod)
		{
			bool isPeriodExisting = existingPeriodsMap.TryGetValue(actualTradePeriod.Key, out OrgTradePeriod tradePeriod);
			if (!isPeriodExisting)
			{
				tradePeriod = partitionFactory.New<OrgTradePeriod>();
				existingPeriodsMap[actualTradePeriod.Key] = tradePeriod;
			}
			using (tradePeriod.GetValidationSuspender())
			{
				if (!isPeriodExisting)
				{
					tradePeriod.PAS_IsTraded = true;
					tradePeriod.PAS_PA = tradeDetail.PK;
					tradePeriod.PAS_Period = actualTradePeriod.Key.Period;
					tradePeriod.PAS_OH_Client = actualTradePeriod.Key.OrgPk;
					tradePeriod.PAS_IsJobValue = actualTradePeriod.Key.IsJobValue;
				}
				tradePeriod.PAS_RepeatsMnth = actualTradePeriod.Value.NumberOfJobs;
				tradePeriod.PAS_PalletCount = actualTradePeriod.Value.NumberOfPallets;
				tradePeriod.PAS_LineCount = actualTradePeriod.Value.NumberOfLines;
				tradePeriod.PAS_Units = GetNotNegativeZLongFromDecimal(actualTradePeriod.Value.WeightVolume);
				tradePeriod.PAS_Weight = GetNotNegativeZDecimalFromDecimal(actualTradePeriod.Value.Weight, OrgTradePeriodSchema.PAS_Weight.Scale);
				tradePeriod.PAS_WeightUQ = Constants.Weight.Tonnes;
				tradePeriod.PAS_Volume = GetNotNegativeZDecimalFromDecimal(actualTradePeriod.Value.Volume, OrgTradePeriodSchema.PAS_Volume.Scale);
				tradePeriod.PAS_VolumeUQ = Constants.Volume.CubicMetres;
				tradePeriod.PAS_Chargeable = GetNotNegativeZDecimalFromDecimal(actualTradePeriod.Value.Chargeable, OrgTradePeriodSchema.PAS_Chargeable.Scale);
				tradePeriod.PAS_TEUQuantity = GetNotNegativeZDecimalFromDecimal(actualTradePeriod.Value.TEU, OrgTradePeriodSchema.PAS_TEUQuantity.Scale);
				tradePeriod.PAS_LastTraded = actualTradePeriod.Value.PeriodLastTrade;

				ZLong GetNotNegativeZLongFromDecimal(decimal value)
				{
					ZLong longValue = Convert.ToInt64(value);
					return longValue > 0 ? longValue : 0;
				}
			}

			return tradePeriod;
		}

		void CreateOrUpdateTradeValue(BusinessObjectFactory partitionFactory, OrgTradePeriod tradePeriod, Dictionary<TradeValueBreakdownKey, OrgTradeValue> existingValuesMap, KeyValuePair<TradeValueBreakdownKey, TradeValueBreakdown> actualTradeValue)
		{
			bool isValueExisting = existingValuesMap.TryGetValue(actualTradeValue.Key, out OrgTradeValue tradeValue);

			if (actualTradeValue.Value.Revenue == 0 && actualTradeValue.Value.Cost == 0)
			{
				if (isValueExisting)
				{
					existingValuesMap.Remove(actualTradeValue.Key);
					tradeValue.Delete();
				}
			}
			else
			{
				if (!isValueExisting)
				{
					tradeValue = partitionFactory.New<OrgTradeValue>();
					existingValuesMap[actualTradeValue.Key] = tradeValue;
				}
				using (tradeValue.GetValidationSuspender())
				{
					if (!isValueExisting)
					{
						tradeValue.PAV_PAS = tradePeriod.PK;
						tradeValue.PAV_RX_NKCurrency = actualTradeValue.Key.Currency;
						tradeValue.PAV_GC = actualTradeValue.Key.CompanyPk;
					}
					tradeValue.PAV_Revenue = decimal.Round(actualTradeValue.Value.Revenue, OrgTradeValueSchema.PAV_Revenue.Scale);
					tradeValue.PAV_Cost = -decimal.Round(actualTradeValue.Value.Cost, OrgTradeValueSchema.PAV_Cost.Scale);
				}
			}
		}

		static ZDecimal GetNotNegativeZDecimalFromDecimal(decimal value, byte scale)
		{
			var result = decimal.Round(value, scale);
			return result > 0m ? result : 0m;
		}

		#endregion

		#region Update Prospect Values

		void UpdateProspectValues(TradeLinesSummary tradeSummary)
		{
			var prospectSales = MasterSalesCollection.Where(x => !x.IsActual && !x.IsDeleted).ToList();

			foreach (var sales in prospectSales)
			{
				MasterFactory.AddFetchHint(OrgTradeDetailSchema.PA_OW, sales.PK);
			}

			foreach (var sales in prospectSales)
			{
				foreach (OrgTradeDetail tradeDetail in sales.TradeDetails)
				{
					MasterFactory.AddFetchHint(OrgTradePeriodSchema.PAS_PA, tradeDetail.PK);
					MasterFactory.AddFetchHint(OrgTradeProspectSchema.PAP_PA, tradeDetail.PK);
				}
			}

			if (tradeSummary.ProspectValues.Count > 0)
			{
				var salesLookupIgnoringOrg =
						prospectSales
						.GroupBy(x => Tuple.Create(x.OW_MP_Product, x.OW_OriginID, x.OW_DestinationID, x.OW_WW, x.OW_Service))
						.ToDictionary(x => x.Key, y => y.ToList());
				var updateItems = GetProspectUpdateItems(tradeSummary, salesLookupIgnoringOrg);
				if (updateItems.Count > 0)
				{
					UpdateProspectTradeDetails(updateItems);
				}
			}
		}

		List<ProspectTradeStatusUpdateItem> GetProspectUpdateItems(TradeLinesSummary tradeSummary, Dictionary<Tuple<ZGuid, ZGuid, ZGuid, ZGuid, ZString>, List<OrgSales>> salesLookupIgnoringOrg)
		{
			var updateItems = new List<ProspectTradeStatusUpdateItem>();

			// create prospects in the order of least specific to most specific
			// this allows prospects with buyer/supplier to be matched against ones without buyer/supplier (to avoid duplication)
			updateItems.AddRange(CreateProspectTradeStatusUpdateItems(tradeSummary.ProspectValues.Where(x => x.Key.Supplier.IsEmpty && x.Key.Buyer.IsEmpty), salesLookupIgnoringOrg));
			updateItems.AddRange(CreateProspectTradeStatusUpdateItems(tradeSummary.ProspectValues.Where(x => x.Key.Supplier.IsEmpty && !x.Key.Buyer.IsEmpty), salesLookupIgnoringOrg));
			updateItems.AddRange(CreateProspectTradeStatusUpdateItems(tradeSummary.ProspectValues.Where(x => !x.Key.Supplier.IsEmpty && x.Key.Buyer.IsEmpty), salesLookupIgnoringOrg));
			updateItems.AddRange(CreateProspectTradeStatusUpdateItems(tradeSummary.ProspectValues.Where(x => !x.Key.Supplier.IsEmpty && !x.Key.Buyer.IsEmpty), salesLookupIgnoringOrg));
			return updateItems;
		}

		IEnumerable<ProspectTradeStatusUpdateItem> CreateProspectTradeStatusUpdateItems(IEnumerable<KeyValuePair<TradeLaneKey, TradeLaneValue>> prospectValues, Dictionary<Tuple<ZGuid, ZGuid, ZGuid, ZGuid, ZString>, List<OrgSales>> salesLookupIgnoringOrg)
		{
			foreach (var prospectTradeLane in prospectValues)
			{
				var ignoreOrgKey = Tuple.Create(prospectTradeLane.Key.ProductPk, prospectTradeLane.Key.OriginPk, prospectTradeLane.Key.DestinationPk, prospectTradeLane.Key.WarehousePk, prospectTradeLane.Key.Service);

				IEnumerable<OrgSales> matchingSales = null;

				if (salesLookupIgnoringOrg.TryGetValue(ignoreOrgKey, out List<OrgSales> matchingSalesIgnoringOrg))
				{
					matchingSales = matchingSalesIgnoringOrg.Where(x =>
							prospectTradeLane.Key.MainOrg == x.OW_OH_Primary
							&& (prospectTradeLane.Key.Supplier.IsEmpty || prospectTradeLane.Key.Supplier == x.OW_OH_Supplier)
							&& (prospectTradeLane.Key.Buyer.IsEmpty || prospectTradeLane.Key.Buyer == x.OW_OH_Buyer)).ToArray();
				}
				if (matchingSales == null || !matchingSales.Any())
				{
					var sales = masterFactoryProvider.Current.New<OrgSales>();
					using (sales.GetValidationSuspender())
					using (sales.GetDefaultPropertySuspender())
					{
						sales.OW_MP_Product = prospectTradeLane.Key.ProductPk;
						sales.SetOriginIdAndTableCodeDirectly(prospectTradeLane.Key.OriginPk, prospectTradeLane.Key.OriginTableCode);
						sales.SetDestinationIdAndTableCodeDirectly(prospectTradeLane.Key.DestinationPk, prospectTradeLane.Key.DestinationTableCode);
						sales.OW_WW = prospectTradeLane.Key.WarehousePk;
						sales.OW_Service = prospectTradeLane.Key.Service;
						sales.OW_OH_Primary = prospectTradeLane.Key.MainOrg;
						sales.OW_OH_Supplier = prospectTradeLane.Key.Supplier;
						sales.OW_OH_Buyer = prospectTradeLane.Key.Buyer;
					}

					if (matchingSalesIgnoringOrg == null)
					{
						matchingSalesIgnoringOrg = new List<OrgSales>();
						salesLookupIgnoringOrg.Add(ignoreOrgKey, matchingSalesIgnoringOrg);
					}
					matchingSalesIgnoringOrg.Add(sales);
					matchingSales = TradeLinesValueHelper.AsEnumerable(sales);
				}
				else
				{
					// This is to avoid duplication when synchronizing back from quotation
					if (matchingSales.Count() > 1)
					{
						var prospectTradeDetailKeys = prospectTradeLane.Value.TradeDetails.Keys;
						OrgSales saleAssociateWithParentEntity = null;
						var salesWithSameTradeDetails = matchingSales.OrderBy(sales => sales.OW_SystemCreateTimeUtc).Where(x => x.TradeDetails.Cast<OrgTradeDetail>().Any(t => prospectTradeDetailKeys.Any(k => t.PA_TradeMode.EqualsIgnoringCase(k.Mode) && t.PA_TradeType.EqualsIgnoringCase(k.Type))));
						if (salesWithSameTradeDetails.Count() > 1 && parentEntity != null)
						{
							// All the matching criteria are the same, see which one has an existing association to the parent entity
							saleAssociateWithParentEntity = salesWithSameTradeDetails.FirstOrDefault(x => x.SalesAssociationPivotCollectionGlobal.Any(a => a.SVP_ActivityId == parentEntity.Identifier));
						}

						if (!salesWithSameTradeDetails.Any())
						{
							matchingSales = TradeLinesValueHelper.AsEnumerable(matchingSales.Last());
						}
						else if (salesWithSameTradeDetails.Count() == 1)
						{
							matchingSales = salesWithSameTradeDetails;
						}
						else
						{
							matchingSales = TradeLinesValueHelper.AsEnumerable(saleAssociateWithParentEntity ?? salesWithSameTradeDetails.First());
						}
					}
				}

				foreach (var prospectTradeDetail in prospectTradeLane.Value.TradeDetails)
				{
					yield return new ProspectTradeStatusUpdateItem(prospectTradeLane.Key, prospectTradeDetail, matchingSales);
				}
			}
		}

		void UpdateProspectTradeDetails(IEnumerable<ProspectTradeStatusUpdateItem> updateItems)
		{
			foreach (var updateItem in updateItems)
			{
				foreach (var sales in updateItem.SalesToUpdate)
				{
					MasterFactory.AddFetchHint(OrgTradeDetailSchema.PA_OW, sales.PK);
					MasterFactory.AddFetchHint(OrgSalesProductSchema.Constants.TableName, sales.OW_MP_Product);
				}
			}

			var salesValuesToAddSalesAssocation = new List<Tuple<ISalesValue, ZGuid>>();

			foreach (var updateItem in updateItems)
			{
				foreach (var sales in updateItem.SalesToUpdate)
				{
					using (sales.GetDefaultPropertySuspender())
					{
						var tradeDetail = PopulateProspectDetails(updateItem.TradeDetail, sales);
						if (tradeDetail != null && sales.Product.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgTradeDetail))
						{
							foreach (var rateEntryPk in updateItem.TradeDetail.Value.RelatedRateEntryPks)
							{
								salesValuesToAddSalesAssocation.Add(Tuple.Create<ISalesValue, ZGuid>(tradeDetail, rateEntryPk));
							}
						}

						if (sales.Product.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgSales))
						{
							foreach (var rateEntryPk in updateItem.TradeDetail.Value.RelatedRateEntryPks)
							{
								salesValuesToAddSalesAssocation.Add(Tuple.Create<ISalesValue, ZGuid>(sales, rateEntryPk));
							}
						}
					}
				}
			}

			AddToSalesValueRelatedJobsLookupCache(salesValuesToAddSalesAssocation.Select(x => x.Item1.Identifier));
			foreach (var salesValue in salesValuesToAddSalesAssocation)
			{
				CreateSalesAssociation(salesValue.Item1, salesValue.Item2);
			}
		}

		void CreateSalesAssociation(ISalesValue salesValue, ZGuid relatedJobId)
		{
			if (!HasSalesValueAssociation(salesValue, relatedJobId))
			{
				AddSalesValueAssocation(salesValue, relatedJobId);
			}
		}

		OrgTradeDetail PopulateProspectDetails(KeyValuePair<TradeDetailKey, TradeDetailValue> prospectTradeDetail, OrgSales sales)
		{
			if ((prospectTradeDetail.Key.Mode.IsEmpty && sales.Product.ModeIsMandatory)
				|| (prospectTradeDetail.Key.Type.IsEmpty && sales.Product.TypeIsMandatory))
			{
				return null;
			}
			var existingDetailsMap = sales.TradeDetails.Cast<OrgTradeDetail>()
				.GroupBy(x => TradeLinesValueHelper.CreateTradeDetailKey(x))
				.ToDictionary(x => x.Key, y => y.FirstOrDefault());
			var tradeDetail = CreateOrFindTradeDetail(MasterFactory, sales, existingDetailsMap, prospectTradeDetail);
			PopulateProspectPeriod(prospectTradeDetail, tradeDetail);
			PopulateProspectDetail(tradeDetail);

			return tradeDetail;
		}

		void PopulateProspectPeriod(KeyValuePair<TradeDetailKey, TradeDetailValue> prospectTradeDetail, OrgTradeDetail tradeDetail)
		{
			var currency = prospectTradeDetail.Value.TradePeriods.SelectMany(x => x.Value.TradeValues.Keys).Select(x => x.Currency).FirstOrDefault();

			OrgTradePeriod prospectPeriod = null;
			if (tradeDetail.IsInDatabase)
			{
				prospectPeriod = MasterFactory.LoadTop1<OrgTradePeriod>(new ZQuery(OrgTradePeriodSchema.PAS_PA, tradeDetail.PK));
			}

			var isPeriodExisting = prospectPeriod != null;
			if (!isPeriodExisting)
			{
				prospectPeriod = MasterFactory.New<OrgTradePeriod>();
			}
			using (prospectPeriod.GetValidationSuspender())
			{
				if (!isPeriodExisting)
				{
					prospectPeriod.PAS_IsTraded = false;
					prospectPeriod.PAS_PA = tradeDetail.PK;
					prospectPeriod.PAS_OH_Client = Master.PK;
				}
				if (prospectPeriod.PAS_RX_NKCurrency.IsEmpty)
				{
					prospectPeriod.PAS_RX_NKCurrency = currency ?? ZString.Empty;
				}
			}
		}

		void PopulateProspectDetail(OrgTradeDetail tradeDetail)
		{
			OrgTradeProspect prospectDetail = null;
			if (tradeDetail.IsInDatabase)
			{
				prospectDetail = MasterFactory.LoadTop1<OrgTradeProspect>(new ZQuery(OrgTradeProspectSchema.PAP_PA, tradeDetail.PK));
			}

			if (prospectDetail == null)
			{
				prospectDetail = MasterFactory.New<OrgTradeProspect>();
				using (prospectDetail.GetValidationSuspender())
				{
					prospectDetail.PAP_PA = tradeDetail.PK;
				}
			}
		}

		struct ProspectTradeStatusUpdateItem
		{
			public ProspectTradeStatusUpdateItem(TradeLaneKey tradeLaneKey, KeyValuePair<TradeDetailKey, TradeDetailValue> tradeDetail, IEnumerable<OrgSales> salesToUpdate)
			{
				TradeLaneKey = tradeLaneKey;
				TradeDetail = tradeDetail;
				SalesToUpdate = salesToUpdate;
			}

			public readonly TradeLaneKey TradeLaneKey;
			public readonly KeyValuePair<TradeDetailKey, TradeDetailValue> TradeDetail;
			public readonly IEnumerable<OrgSales> SalesToUpdate;
		}

		public class SalesValueAssociationPivotArgs : EventArgs
		{
			public SalesValueAssociationPivotArgs(OrgSalesValueAssociationPivot salesValueAssociationPivot)
			{
				SalesValueAssociationPivot = salesValueAssociationPivot;
			}

			public readonly OrgSalesValueAssociationPivot SalesValueAssociationPivot;
		}

		#region SalesValueRelatedJobsLookupCache

		Dictionary<ZGuid, HashSet<ZGuid>> salesValueRelatedJobsLookupCache;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void AddToSalesValueRelatedJobsLookupCache(IEnumerable<ZGuid> salesValueIdsToCacheIfNotYetExist)
		{
			if (salesValueRelatedJobsLookupCache == null)
			{
				salesValueRelatedJobsLookupCache = new Dictionary<ZGuid, HashSet<ZGuid>>();
			}

			var salesValueIdsToCache = new HashSet<ZGuid>(salesValueIdsToCacheIfNotYetExist.Where(x => !salesValueRelatedJobsLookupCache.ContainsKey(x)));

			var query = new ZQuery();
			query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeId, salesValueIdsToCache);
			query.FetchOnlyFromLocalCache = true;

			foreach (var localPivot in MasterFactory.Load<OrgSalesValueAssociationPivot>(query))
			{
				var relatedJobPk = localPivot.SVP_ActivityId;
				var salesValuePk = localPivot.SVP_TradeId;
				AddToDictionarySet(salesValueRelatedJobsLookupCache, salesValuePk, relatedJobPk);
			}

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT {0}, {1}
FROM {2}
WHERE
	{3} IN (SELECT VALUE FROM @IDs)
",
OrgSalesValueAssociationPivotSchema.Constants.SVP_ActivityId, // 0
OrgSalesValueAssociationPivotSchema.Constants.SVP_TradeId, // 1
OrgSalesValueAssociationPivotSchema.Constants.TableName, // 2
OrgSalesValueAssociationPivotSchema.Constants.SVP_TradeId // 3
				);

			IDbConnected dbConnected = MasterFactory;
			using (var command = dbConnected.Connection.Command(sql))
			{
				command.AddTableValuedParameter("@IDs", OrgSalesValueAssociationPivotSchema.SVP_TradeId, salesValueIdsToCache);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var relatedJobPk = (Guid)reader[OrgSalesValueAssociationPivotSchema.Constants.SVP_ActivityId];
						var salesValuePk = (Guid)reader[OrgSalesValueAssociationPivotSchema.Constants.SVP_TradeId];
						AddToDictionarySet(salesValueRelatedJobsLookupCache, salesValuePk, relatedJobPk);
					}
				}
			}

			foreach (var salesValueId in salesValueIdsToCache)
			{
				if (!salesValueRelatedJobsLookupCache.ContainsKey(salesValueId))
				{
					salesValueRelatedJobsLookupCache.Add(salesValueId, null);
				}
			}
		}

		static void AddToDictionarySet(Dictionary<ZGuid, HashSet<ZGuid>> dictionarySet, ZGuid key, ZGuid value)
		{
			if (!dictionarySet.TryGetValue(key, out HashSet<ZGuid> valueSet) || valueSet == null)
			{
				valueSet = new HashSet<ZGuid>();
				dictionarySet[key] = valueSet;
			}
			valueSet.Add(value);
		}

		bool HasSalesValueAssociation(ZGuid pk)
		{
			if (!salesValueRelatedJobsLookupCache.TryGetValue(pk, out HashSet<ZGuid> salesValueRelatedJobsSet))
			{
				return false;
			}

			return salesValueRelatedJobsSet != null;
		}

		bool HasSalesValueAssociation(ISalesValue salesValue, ZGuid relatedJobId)
		{
			if (!salesValueRelatedJobsLookupCache.TryGetValue(salesValue.Identifier, out HashSet<ZGuid> salesValueRelatedJobsSet))
			{
				return false;
			}

			return salesValueRelatedJobsSet != null && salesValueRelatedJobsSet.Contains(relatedJobId);
		}

		void AddSalesValueAssocation(ISalesValue salesValue, ZGuid relatedJobId)
		{
			var pivot = MasterFactory.New<OrgSalesValueAssociationPivot>();
			pivot.SVP_ActivityTableCode = RateEntrySchema.Constants.Prefix;
			pivot.SVP_ActivityId = relatedJobId;
			pivot.SVP_TradeTableCode = salesValue.TablePrefix;
			pivot.SVP_TradeId = salesValue.Identifier;
			AddToDictionarySet(salesValueRelatedJobsLookupCache, salesValue.Identifier, relatedJobId);

			SalesValueAssociationCreated?.Invoke(this, new SalesValueAssociationPivotArgs(pivot));
		}

		public event EventHandler<SalesValueAssociationPivotArgs> SalesValueAssociationCreated;

		#endregion

		#endregion

		#region Update Lost Values

		void UpdateLostValues()
		{
			MasterSalesCollection.Reload(reLoadExistingRows: false);
			PopulateActualSalesLastTradedDates();

			var lostCutoffDate = OrgSales.GetLostCutoffDate();
			var currentSales = MasterSalesCollection.Cast<OrgSales>().Where(x => !x.IsDeleted).Select(x => GetCurrentlyTradingMatchingKey(x));
			var currentActualSalesKeys = new HashSet<CurrentlyTradingMatchingKey>(currentSales.Where(x => x.IsActual && x.LastTraded >= lostCutoffDate));
			AddProspectTradeDetailsForLostActuals(currentActualSalesKeys, currentSales, lostCutoffDate);
			RemoveProspectTradeDetailsWithNoAssociationsThatAreNowTrading(currentActualSalesKeys, currentSales);
		}

		void AddProspectTradeDetailsForLostActuals(HashSet<CurrentlyTradingMatchingKey> currentActualSalesKeys, IEnumerable<CurrentlyTradingMatchingKey> currentSales, ZDate lostCutoffDate)
		{
			var prospectSalesKeys = new HashSet<CurrentlyTradingMatchingKey>(currentSales.Where(x => !x.IsActual));

			var oldActualSalesKeys = currentSales.Where(x => x.IsActual && (x.LastTraded.IsEmpty || x.LastTraded < lostCutoffDate));
			var oldActualSalesCurrentlyNotTradingKeys = oldActualSalesKeys.Where(x => !currentActualSalesKeys.Contains(x)).ToList();
			foreach (var prospectKeyToAdd in oldActualSalesCurrentlyNotTradingKeys)
			{
				if (!prospectSalesKeys.Contains(prospectKeyToAdd))
				{
					var sales = masterFactoryProvider.Current.New<OrgSales>();
					using (sales.GetValidationSuspender())
					using (sales.GetDefaultPropertySuspender())
					{
						sales.OW_OH_Primary = Master.PK;
						sales.OW_MP_Product = prospectKeyToAdd.ProductPk;
						sales.SetOriginIdAndTableCodeDirectly(prospectKeyToAdd.OriginID, prospectKeyToAdd.OriginTableCode);
						sales.SetDestinationIdAndTableCodeDirectly(prospectKeyToAdd.DestinationID, prospectKeyToAdd.DestinationTableCode);
					}

					prospectSalesKeys.Add(prospectKeyToAdd);
				}
			}
		}

		void RemoveProspectTradeDetailsWithNoAssociationsThatAreNowTrading(HashSet<CurrentlyTradingMatchingKey> currentActualSalesKeys, IEnumerable<CurrentlyTradingMatchingKey> currentSales)
		{
			if (currentActualSalesKeys.Count > 0)
			{
				var prospectSales = currentSales.Where(x => !x.IsActual && x.IsInDatabase).ToArray();
				AddToSalesValueRelatedJobsLookupCache(prospectSales.Select(x => x.SalesPk));

				var prospectSalesWithoutAssociations = prospectSales.Where(x => !HasSalesValueAssociation(x.SalesPk));
				var prospectSalesWithoutAssociationsButCurrentlyTrading = prospectSalesWithoutAssociations.Where(x => currentActualSalesKeys.Contains(x)).ToList();

				var existingSalesDictionary = MasterSalesCollection.OfType<OrgSales>().Where(x => !x.IsDeleted).ToDictionary(x => x.PK);

				var newKeys = prospectSalesWithoutAssociationsButCurrentlyTrading.Where(x => !existingSalesDictionary.ContainsKey(x.SalesPk));
				var newOrgSales = masterFactoryProvider.Current.Load<OrgSales>(new ZQuery(OrgSalesSchema.PK, newKeys)).ToDictionary(x => x.PK);

				foreach (var prospectSalesKey in prospectSalesWithoutAssociationsButCurrentlyTrading)
				{
					if (!existingSalesDictionary.TryGetValue(prospectSalesKey.SalesPk, out OrgSales prospectSalesToDelete))
					{
						prospectSalesToDelete = newOrgSales[prospectSalesKey.SalesPk];
					}
					if (prospectSalesToDelete.OW_OH_Primary.IsEmpty)
					{
						// a legacy OrgSales - which may have no associations even though user has edited it. Instead of deleting it, we will hide it
						using (prospectSalesToDelete.GetValidationSuspender())
						using (prospectSalesToDelete.GetDefaultPropertySuspender())
						{
							prospectSalesToDelete.OW_OH_Primary = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
						}
					}
					else
					{
						MasterSalesCollection.RemoveAndDelete(prospectSalesToDelete);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Cannot be expressed with ZQuery")]
		void PopulateActualSalesLastTradedDates()
		{
			ActualSalesLastTradedDates.Clear();

			var sql = $@"
SELECT {OrgSalesSchema.Constants.PK}, MAX({OrgTradePeriodSchema.Constants.PAS_Period}) AS LastTraded
FROM {OrgSalesSchema.Constants.SqlSchemaName}.{OrgSalesSchema.Constants.TableName}
LEFT JOIN {OrgTradeDetailSchema.Constants.SqlSchemaName}.{OrgTradeDetailSchema.Constants.TableName} ON {OrgTradeDetailSchema.Constants.PA_OW} = {OrgSalesSchema.Constants.PK}
LEFT JOIN {OrgTradePeriodSchema.Constants.SqlSchemaName}.{OrgTradePeriodSchema.Constants.TableName} ON {OrgTradePeriodSchema.Constants.PAS_PA} = {OrgTradeDetailSchema.Constants.PK}
WHERE
	({OrgSalesSchema.Constants.OW_IsTraded} = 1) AND ({MasterSalesCollection.CompleteFilter.FilterString})
GROUP BY {OrgSalesSchema.Constants.PK}
";

			IDbConnected dbConnected = MasterFactory;
			using (var command = dbConnected.Connection.Command(sql))
			{
				foreach (var parameter in MasterSalesCollection.CompleteFilter.Params)
				{
					command.AddParameter(parameter);
				}
				
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						ZGuid salesPk = reader.GetGuid(0);
						var lastTradedDate = !reader.IsDBNull(1) ? new ZDateTime(reader.GetDateTime(1)).Date : ZDate.Empty;
						ActualSalesLastTradedDates.Add(salesPk, lastTradedDate);
					}
				}
			}
		}

		ZGuid CustomsBrokerageProductPk
		{
			get
			{
				if (customsBrokerageProductPk == null)
				{
					var customsBrokerageProduct = MasterFactory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);
					customsBrokerageProductPk = customsBrokerageProduct != null ? customsBrokerageProduct.Identifier : ZGuid.Empty;
				}

				return customsBrokerageProductPk.Value;
			}
		}
		ZGuid? customsBrokerageProductPk;

		CurrentlyTradingMatchingKey GetCurrentlyTradingMatchingKey(OrgSales sales)
		{
			var productPk = sales.OW_MP_Product;
			ZGuid originID;
			ZString originTableCode;
			ZGuid destinationID;
			ZString destinationTableCode;

			if (sales.IsActual && productPk == CustomsBrokerageProductPk)
			{
				if (IsImpBrokerage(sales))
				{
					originID = sales.OW_DestinationID;
					originTableCode = sales.OW_DestinationTableCode;
				}
				else
				{
					originID = sales.OW_OriginID;
					originTableCode = sales.OW_OriginTableCode;
				}

				destinationID = ZGuid.Empty;
				destinationTableCode = ZString.Empty;
			}
			else
			{
				originID = sales.OW_OriginID;
				originTableCode = sales.OW_OriginTableCode;
				destinationID = sales.OW_DestinationID;
				destinationTableCode = sales.OW_DestinationTableCode;
			}

			ZDate lastTraded = ZDate.Empty;
			ActualSalesLastTradedDates.TryGetValue(sales.PK, out lastTraded);

			return new CurrentlyTradingMatchingKey(sales.PK, sales.IsActual, sales.IsInDatabase, productPk, originID, originTableCode, destinationID, destinationTableCode, lastTraded);
		}

		static bool IsImpBrokerage(OrgSales sales)
		{
			return sales.TradeDetails.Cast<OrgTradeDetail>().Any(x => x.PA_TradeType == OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Import);
		}

		struct CurrentlyTradingMatchingKey
		{
			public CurrentlyTradingMatchingKey(ZGuid salesPk, bool isActual, bool isInDatabase, ZGuid productPk, ZGuid originID, ZString originTableCode, ZGuid destinationID, ZString destinationTableCode, ZDate lastTraded)
			{
				SalesPk = salesPk;
				IsActual = isActual;
				IsInDatabase = isInDatabase;
				ProductPk = productPk;
				OriginID = originID;
				OriginTableCode = originTableCode;
				DestinationID = destinationID;
				DestinationTableCode = destinationTableCode;
				LastTraded = lastTraded;
			}

			public readonly ZGuid SalesPk;
			public readonly bool IsActual;
			public readonly bool IsInDatabase;
			public readonly ZGuid ProductPk;
			public readonly ZGuid OriginID;
			public readonly ZString OriginTableCode;
			public readonly ZGuid DestinationID;
			public readonly ZString DestinationTableCode;
			public readonly ZDate LastTraded;

			public override int GetHashCode()
			{
				return ProductPk.GetHashCode() ^
					OriginID.GetHashCode() ^
					OriginTableCode.GetHashCode() ^
					DestinationID.GetHashCode() ^
					DestinationTableCode.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var local = (CurrentlyTradingMatchingKey)obj;
				return local.ProductPk == ProductPk
					&& local.OriginID == OriginID
					&& local.OriginTableCode == OriginTableCode
					&& local.DestinationID == DestinationID
					&& local.DestinationTableCode == DestinationTableCode;
			}
		}

		#endregion

		#region AddOrUpdateTLSLogAndClientCommencedDateEditLog

		void AddOrUpdateTLSLogAndClientCommencedDateEditLog(TradeLinesSummary tradeSummary, TradeLaneKey tradeLaneKey)
		{
			var now = ZDateTime.UtcNow;
			var referencePrefix = isFullSync ? SynchronisedAllReference : SynchronisedPartialReference;

			var monthCompletionReferenceString = tradeSummary.From.Year + ((ZString)tradeSummary.From.Month.ToString(CultureInfo.InvariantCulture)).PadLeft(2, '0');

			var tlsLog = LoadExistingLog(referencePrefix);
			if (tlsLog != null)
			{
				using (((IUpdateFieldsLock)tlsLog).LockForUpdatingKeyFields())
				{
					tlsLog.SL_EventTime = now;
					tlsLog.SL_Reference = GetReferenceString(referencePrefix, monthCompletionReferenceString, now, tlsLog.SL_PostedTimeUtc);
				}
			}
			else
			{
				Master.Logs.CreateRecreateOrUpdateEventLog(Events.SalesTradeLanesSynchronised, EstimateActual.Actual, ZDateTimeOffset.UtcNow, GetReferenceString(referencePrefix, monthCompletionReferenceString, now, now));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "The reference is not translatable, shouldnt res string it")]
		static string GetReferenceString(string referencePrefix, string monthReferenceString, ZDateTime thisRun, ZDateTime firstRun)
		{
			return string.Format(Culture.Current, "{0} {1}, Last run: {2}, First run: {3}", referencePrefix, monthReferenceString, thisRun.ToShortDateString(), firstRun.ToShortDateString());
		}

		// Not using MostRecentLogByEventTime because it loads ALL logs instead of just SalesTradeLanesSynchronised
		StmALog LoadExistingLog(string referencePrefix)
		{
			var filter = new ZQuery(StmALogSchema.SL_Parent, Master.PK);
			filter.AddToFilter(StmALogSchema.SL_IsCancelled, "N");
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.SalesTradeLanesSynchronisedCode);

			var refFilter = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, referencePrefix);
			if (referencePrefix == SynchronisedPartialReference)
			{
				refFilter.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, string.Empty);
			}

			filter.AddToFilter(refFilter);
			filter.OrderBy = StmALogSchema.Constants.SL_EventTime + OrderByClause.Descending;

			return MasterFactory.LoadTop1<StmALog>(filter);
		}

		public const string SynchronisedPartialReference = "PARTIAL";
		public const string SynchronisedAllReference = "ALL";

		#endregion

		#region Set Client Commenced Date

		TradeLaneKey SetClientCommencedDateAndGetTradeLaneKeyUsed(TradeLinesSummary tradeSummary)
		{
			var firstTradedPeriodDate = tradeSummary.ActualValues.SelectMany(
												a => a.Value.TradeDetails.Values.SelectMany(
													b => b.TradePeriods.Keys.Select(c => c.Period)))
											.Where(d => !d.IsEmpty).DefaultIfEmpty(ZDate.Empty).Min();

			TradeLaneKey laneKey = null;

			if (!firstTradedPeriodDate.IsEmpty)
			{
				if (Master.MiscServ.OM_CMClientCommenced.IsEmpty)
				{
					Master.MiscServ.OM_CMClientCommenced = firstTradedPeriodDate;
				}
				else if (OrganisationRegistry.Instance.UpdateExistingClientCommenceDateFromTradeLaneSync.Value && firstTradedPeriodDate < Master.MiscServ.OM_CMClientCommenced.Date)
				{
					Master.MiscServ.OM_CMClientCommenced = firstTradedPeriodDate;
				}
			}

			if (Master.MiscServ.OM_CMClientCommencedInfo.HasChanges)
			{
				var lane = tradeSummary.ActualValues.First(e =>
				{
					var values = e.Value.TradeDetails.Values;
					var keys = values.SelectMany(a => a.TradePeriods.Keys);
					return keys.Any(a => !a.Period.IsEmpty && a.Period.Equals(firstTradedPeriodDate));
				});

				laneKey = lane.Key;
			}

			return laneKey;
		}

		#endregion
	}
}
