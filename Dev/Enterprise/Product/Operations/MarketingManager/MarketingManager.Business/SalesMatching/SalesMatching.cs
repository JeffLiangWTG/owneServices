using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	enum SalesMatchingTarget
	{
		Sales,
		TradeDetail
	}

	public class SalesMatching : NonPersistentBusinessObject
	{
		public SalesMatching(OrgHeader org, EntitySalesWrapper salesForMatching, EntityTradeDetailWrapper tradeDetailForMatching, SalesMatchingOptions salesMatchingOptions)
			: base(org.Factory)
		{
			this.org = org;
			this.salesForMatching = salesForMatching ?? tradeDetailForMatching.Parent;
			this.tradeDetailForMatching = tradeDetailForMatching;
			this.entity = this.salesForMatching.Entity;
			this.MatchingTarget = tradeDetailForMatching != null ? SalesMatchingTarget.TradeDetail : SalesMatchingTarget.Sales;
			this.SalesMatchingOptions = salesMatchingOptions;

			FindMatch();
		}

		readonly OrgHeader org;
		readonly EntitySalesWrapper salesForMatching;
		readonly EntityTradeDetailWrapper tradeDetailForMatching;
		readonly ISalesValueAssociatedEntity entity;
		readonly SalesMatchingTarget MatchingTarget;
		public readonly SalesMatchingOptions SalesMatchingOptions;

		public OrgSalesProduct Product
		{
			get { return (OrgSalesProduct)salesForMatching.Product; }
		}

		public SalesMatchingDataCollection SalesToMigrate
		{
			get
			{
				if (salesToMigrate == null)
				{
					var salesData = CreateSalesMatchingData(salesForMatching, salesForMatching, tradeDetailForMatching);

					if (MatchingTarget == SalesMatchingTarget.Sales)
					{
						salesData.EstimatedValueCurrency = salesForMatching.TotalRevenueCurrencyCode;
						salesData.EstimatedValue = salesForMatching.OW_IsCustomRevenue ? salesForMatching.OW_AnnualRevenue : salesForMatching.TotalEstimatedAnnualValue;
					}
					else
					{
						salesData.EstimatedValueCurrency = tradeDetailForMatching.CurrencyCode;
						salesData.EstimatedValue = tradeDetailForMatching.PA_Calc_EstimatedAnnualValue;
					}

					salesToMigrate = new SalesMatchingDataCollection(Factory);
					salesToMigrate.Add(salesData);
				}
				return salesToMigrate;
			}
		}
		SalesMatchingDataCollection salesToMigrate;

		public SalesMatchingDataCollection MatchedSalesCollection
		{
			get
			{
				return matchedSales ?? (matchedSales = new SalesMatchingDataCollection(Factory));
			}
		}
		SalesMatchingDataCollection matchedSales;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void FindMatch()
		{
			matchedSales = new SalesMatchingDataCollection(Factory);
			var matchedAllSales = new List<SalesMatchingData>(1);

			foreach (OrgSales orgSales in org.SalesCollection)
			{
				if ((MatchingTarget != SalesMatchingTarget.Sales || salesForMatching.PK != orgSales.PK)
					&& !orgSales.IsActual
					&& Matches(orgSales))
				{
					if (orgSales.TradeDetails.Count > 0)
					{
						foreach (OrgTradeDetail tradeDetail in orgSales.TradeDetails)
						{
							if ((MatchingTarget != SalesMatchingTarget.TradeDetail || tradeDetailForMatching.PK != tradeDetail.PK)
								&& Matches(tradeDetail))
							{
								var salesData = CreateSalesMatchingData(salesForMatching, orgSales, tradeDetail);
								salesData.EstimatedValueCurrency = tradeDetail.CurrentProspectPeriod.PAS_RX_NKCurrency;
								salesData.EstimatedValue = tradeDetail.PA_Calc_EstimatedAnnualValue;
								matchedAllSales.Add(salesData);
							}
						}
					}
					else if (MatchingTarget == SalesMatchingTarget.Sales)
					{
						var salesData = CreateSalesMatchingData(salesForMatching, orgSales, null);
						var wrapper = EntitySalesWrapper.Get(orgSales, entity);
						salesData.EstimatedValueCurrency = wrapper.TotalRevenueCurrencyCode;
						salesData.EstimatedValue = wrapper.OW_IsCustomRevenue ? wrapper.OW_AnnualRevenue : wrapper.TotalEstimatedAnnualValue;
						matchedAllSales.Add(salesData);
					}
				}
			}

			if (matchedAllSales.Count > 0)
			{
				var matchedSalesWithValidStatus = (matchedAllSales.Where(x => x.TradeDetail == null || (x.TradeDetail.IsPipeline || x.TradeDetail.IsCommitted || x.TradeDetail.IsUnsuccessful)));
				var matchedSalesToAdd = matchedSalesWithValidStatus.Any() ? matchedSalesWithValidStatus : matchedAllSales;

				int rank = 1;
				foreach (SalesMatchingData data in matchedSalesToAdd.OrderBy(x => x, new SalesMatchingDataComparer(SalesMatchingOptions)))
				{
					data.Rank = rank++;
					matchedSales.Add(data);
				}
			}
		}

		public void Append()
		{
			var orgPk = (ZGuid)salesForMatching.Entity.OrgPkInfo.Value;
			salesForMatching.OW_OH_Primary = orgPk;
		}

		bool Matches(OrgSales orgSales)
		{
			if (salesForMatching.OW_MP_Product != orgSales.OW_MP_Product)
			{
				return false;
			}

			foreach (var salesPropertyName in SalesMatchingOptions.SalesPropertiesForMatching.Select(x => x.Item1))
			{
				if (salesPropertyName == OrgSalesSchema.Constants.OW_OriginID)
				{
					if (salesForMatching.OW_OriginID != orgSales.OW_OriginID && salesForMatching.Origin != null && !salesForMatching.Origin.Contains(orgSales.Origin) && orgSales.Origin != null && !orgSales.Origin.Contains(salesForMatching.Origin))
					{
						return false;
					}
				}
				else if (salesPropertyName == OrgSalesSchema.Constants.OW_DestinationID)
				{
					if (salesForMatching.OW_DestinationID != orgSales.OW_DestinationID && salesForMatching.Destination != null && !salesForMatching.Destination.Contains(orgSales.Destination) && orgSales.Destination != null && !orgSales.Destination.Contains(salesForMatching.Destination))
					{
						return false;
					}
				}
				else if (!salesForMatching[salesPropertyName].Equals(orgSales[salesPropertyName]))
				{
					return false;
				}
			}

			return true;
		}

		bool Matches(OrgTradeDetail tradeDetail)
		{
			if (tradeDetailForMatching == null)
			{
				return true;
			}

			foreach (var tradeDetailPropertyName in SalesMatchingOptions.TradeDetailPropertiesForMatching)
			{
				var isValueMatch = tradeDetailForMatching[tradeDetailPropertyName.Item1].Equals(tradeDetail[tradeDetailPropertyName.Item1]);
				var isEmptyString = tradeDetailPropertyName.Item2 == typeof(ZString) && ((ZString)tradeDetailForMatching[tradeDetailPropertyName.Item1]).IsEmpty;
				if (!isEmptyString && !isValueMatch)
				{
					return false;
				}
			}

			return true;
		}

		static SalesMatchingData CreateSalesMatchingData(OrgSales salesForMatching, OrgSales orgSales, OrgTradeDetail tradeDetail)
		{
			var salesData = new SalesMatchingData(orgSales, tradeDetail);

			if (orgSales.Buyer != null)
			{
				salesData.BuyerCode = orgSales.Buyer.OH_Code;
			}
			if (orgSales.Supplier != null)
			{
				salesData.SupplierCode = orgSales.Supplier.OH_Code;
			}

			salesData.IsOriginExactMatch = (salesForMatching.OW_OriginID == orgSales.OW_OriginID);
			salesData.IsDestinationExactMatch = (salesForMatching.OW_DestinationID == orgSales.OW_DestinationID);

			return salesData;
		}

		class SalesMatchingDataComparer : IComparer<SalesMatchingData>
		{
			public SalesMatchingDataComparer(SalesMatchingOptions salesMatchingOptions)
			{
				this.salesMatchingOptions = salesMatchingOptions;
			}

			readonly SalesMatchingOptions salesMatchingOptions;

			public int Compare(SalesMatchingData x, SalesMatchingData y)
			{
				var originDestinationLevelValueX = GetOriginDestinationRank(x);
				var originDestinationLevelValueY = GetOriginDestinationRank(y);
				if (originDestinationLevelValueX != originDestinationLevelValueY)
				{
					return originDestinationLevelValueX.CompareTo(originDestinationLevelValueY);
				}

				if (!x.Sales.OriginCode.EqualsIgnoringCase(y.Sales.OriginCode))
				{
					return x.Sales.OriginCode.ToUpper().CompareTo(y.Sales.OriginCode.ToUpper());
				}

				if (!x.Sales.DestinationCode.EqualsIgnoringCase(y.Sales.DestinationCode))
				{
					return x.Sales.DestinationCode.ToUpper().CompareTo(y.Sales.DestinationCode.ToUpper());
				}

				if (x.TradeDetail != null && y.TradeDetail != null)
				{
					if (x.TradeDetail.PA_Status != y.TradeDetail.PA_Status)
					{
						return x.TradeDetail.PA_Status.CompareTo(y.TradeDetail.PA_Status);
					}

					foreach (var tradeDetailPropertyName in salesMatchingOptions.TradeDetailPropertiesForMatching.Select(p => p.Item1))
					{
						if (!x.TradeDetail[tradeDetailPropertyName].Equals(y.TradeDetail[tradeDetailPropertyName]))
						{
							return ((IZType)x.TradeDetail[tradeDetailPropertyName]).CompareTo(y.TradeDetail[tradeDetailPropertyName]);
						}
					}
				}

				if (!x.BuyerCode.EqualsIgnoringCase(y.BuyerCode))
				{
					return x.BuyerCode.ToUpper().CompareTo(y.BuyerCode.ToUpper());
				}

				if (!x.SupplierCode.EqualsIgnoringCase(y.SupplierCode))
				{
					return x.SupplierCode.ToUpper().CompareTo(y.SupplierCode.ToUpper());
				}

				return 0;
			}

			static int GetOriginDestinationRank(SalesMatchingData data)
			{
				int originRanking = data.IsOriginExactMatch ? 0 : GetLocationRank(data.Sales.OW_OriginTableCode);
				int destinationRanking = data.IsDestinationExactMatch ? 0 : GetLocationRank(data.Sales.OW_DestinationTableCode);
				return originRanking + destinationRanking;
			}

			static int GetLocationRank(ZString locationTableCode)
			{
				switch (locationTableCode)
				{
					case RefCityTownSchema.Constants.Prefix:
						return 1;
					case RefUNLOCOSchema.Constants.Prefix:
						return 2;
					case RefCountryStatesSchema.Constants.Prefix:
						return 3;
					case RateTransportZonesSchema.Constants.Prefix:
						return 4;
					case RefCountrySchema.Constants.Prefix:
						return 5;
					case RefZoneHeaderSchema.Constants.Prefix:
						return 6;
					default:
						return 99;
				}
			}
		}

		public void UseExisting(SalesMatchingData selectedMatchingData)
		{
			var selectedEntitySales = EntitySalesWrapper.Get(selectedMatchingData.Sales, entity);
			var selectedEntityTradeDetail = selectedMatchingData.TradeDetail != null ? EntityTradeDetailWrapper.Get(selectedMatchingData.TradeDetail, entity) : null;
			var entityHeaderCollection = (SalesHeaderCollection)entity.ProspectiveSalesHeaderCollection;
			var entitySalesHeader = entityHeaderCollection.Cast<SalesHeader>().FirstOrDefault(x => x.SalesProduct.PK == Product.PK);

			if (tradeDetailForMatching != null)
			{
				if (selectedEntityTradeDetail != null)
				{
					OrgSalesValueAssociationPivot.AddPivotIfNotExist(entity, selectedEntityTradeDetail);
					OrgSalesValueAssociationPivot.AddPivotIfNotExist(entity, selectedEntitySales);
				}
				if (salesForMatching.EntityTradeDetailsCollection.Any(t => t.PK == tradeDetailForMatching.PK))
				{
					salesForMatching.EntityTradeDetailsCollection.RemoveAndDelete(tradeDetailForMatching);
				}
				DeleteIfNoTradeDetailsRemain(salesForMatching);
			}
			else
			{
				salesForMatching.Delete();
			}

			if (selectedEntityTradeDetail != null)
			{
				selectedEntitySales.EntityTradeDetailsCollection.Add(selectedEntityTradeDetail);
			}

			if (entitySalesHeader != null)
			{
				entitySalesHeader.EntitySalesCollectionProductView.Add(selectedEntitySales);
				entitySalesHeader.FilterableEntitySalesCollection.Rebuild();
			}
		}

		public void MigrateToExisting(SalesMatchingData selectedMatchingData)
		{
			var selectedEntitySales = EntitySalesWrapper.Get(selectedMatchingData.Sales, entity);
			var selectedEntityTradeDetail = selectedMatchingData.TradeDetail != null ? EntityTradeDetailWrapper.Get(selectedMatchingData.TradeDetail, entity) : null;
			var entityHeaderCollection = (SalesHeaderCollection)entity.ProspectiveSalesHeaderCollection;
			var entitySalesHeader = entityHeaderCollection.Cast<SalesHeader>().FirstOrDefault(x => x.SalesProduct.PK == Product.PK);

			if (tradeDetailForMatching != null)
			{
				if (selectedEntityTradeDetail != null)
				{
					selectedEntityTradeDetail.CopyPersistentValuesFrom(tradeDetailForMatching, new BusinessObjectCloneArgs(new[] { OrgTradeDetailSchema.Constants.PA_OW }));
					selectedEntityTradeDetail.ProspectDetail.CopyPersistentValuesFrom(tradeDetailForMatching.ProspectDetail, new BusinessObjectCloneArgs(new[] { OrgTradeProspectSchema.Constants.PAP_PA }));
					selectedEntityTradeDetail.CurrentProspectPeriod.CopyPersistentValuesFrom(tradeDetailForMatching.CurrentProspectPeriod, new BusinessObjectCloneArgs(new[] { OrgTradePeriodSchema.Constants.PAS_PA }));
					OrgSalesValueAssociationPivot.AddPivotIfNotExist(entity, selectedEntityTradeDetail);
					OrgSalesValueAssociationPivot.AddPivotIfNotExist(entity, selectedEntitySales);
				}
				if (salesForMatching.EntityTradeDetailsCollection.Any(t => t.PK == tradeDetailForMatching.PK))
				{
					salesForMatching.EntityTradeDetailsCollection.RemoveAndDelete(tradeDetailForMatching);
				}
				DeleteIfNoTradeDetailsRemain(salesForMatching);
			}
			else
			{
				MoveAllTradeDetails(salesForMatching, selectedEntitySales);
				selectedEntitySales.OW_IsCustomRevenue = salesForMatching.OW_IsCustomRevenue;
				if (salesForMatching.OW_IsCustomRevenue)
				{
					selectedEntitySales.OW_AnnualRevenue = salesForMatching.OW_AnnualRevenue;
					selectedEntitySales.OW_MonthlyRevenue = salesForMatching.OW_MonthlyRevenue;
				}
				salesForMatching.Delete();
			}

			if (selectedEntityTradeDetail != null)
			{
				selectedEntitySales.EntityTradeDetailsCollection.Add(selectedEntityTradeDetail);
			}

			if (entitySalesHeader != null)
			{
				entitySalesHeader.EntitySalesCollectionProductView.Add(selectedEntitySales);
				entitySalesHeader.FilterableEntitySalesCollection.Rebuild();
			}
		}

		public void ReplaceWithClonedExisting(SalesMatchingData selectedMatchingData, bool skipCloningDetail = false)
		{
			var selectedEntitySales = EntitySalesWrapper.Get(selectedMatchingData.Sales, entity);
			var selectedEntityTradeDetail = selectedMatchingData.TradeDetail != null ? EntityTradeDetailWrapper.Get(selectedMatchingData.TradeDetail, entity) : null;
			var entityHeaderCollection = (SalesHeaderCollection)entity.ProspectiveSalesHeaderCollection;
			var entitySalesHeader = entityHeaderCollection.Cast<SalesHeader>().FirstOrDefault(x => x.SalesProduct.PK == Product.PK);

			if (tradeDetailForMatching != null)
			{
				if (!skipCloningDetail)
				{
					CloneTradeDetail(selectedEntityTradeDetail, tradeDetailForMatching);
				}

				if (salesForMatching.PK != selectedEntitySales.PK)
				{
					using (salesForMatching.EntityTradeDetailsCollection.SuspendListChanged())
					using (selectedEntitySales.EntityTradeDetailsCollection.SuspendListChanged())
					{
						salesForMatching.EntityTradeDetailsCollection.Remove(tradeDetailForMatching);
						selectedEntitySales.EntityTradeDetailsCollection.Add(tradeDetailForMatching);
					}

					salesForMatching.TradeDetails.Load();
					using (salesForMatching.TradeDetails.SuspendListChanged())
					using (selectedEntitySales.TradeDetails.SuspendListChanged())
					{
						var tradeDetail = salesForMatching.TradeDetails.FirstOrDefault(x => x.PK == tradeDetailForMatching.PK);
						if (tradeDetail != null)
						{
							salesForMatching.TradeDetails.Remove(tradeDetail);
							selectedEntitySales.TradeDetails.Add(tradeDetail);
						}
					}
				}

				DeleteIfNoTradeDetailsRemain(salesForMatching);
			}
			else
			{
				salesForMatching.CopyPersistentValuesFrom(selectedEntitySales);
				salesForMatching.TradeDetails.RemoveAndDeleteAll();

				if (!skipCloningDetail)
				{
					foreach (var sourceTradeDetail in selectedEntitySales.EntityTradeDetails)
					{
						var targetTradeDetail = salesForMatching.EntityTradeDetailsCollection.AddNew();
						CloneTradeDetail(sourceTradeDetail, targetTradeDetail);
					}
				}
			}

			var tradeDetails = selectedMatchingData?.Sales?.TradeDetails;

			if (tradeDetails != null)
			{
				tradeDetails.Load();
				RemoveDefaultTradeDetails(tradeDetails, selectedEntitySales);
			}

			if (entitySalesHeader != null)
			{
				entitySalesHeader.EntitySalesCollectionProductView.Add(selectedEntitySales);
				entitySalesHeader.FilterableEntitySalesCollection.Rebuild();
			}

			if (tradeDetailForMatching != null)
			{
				tradeDetailForMatching.Validation.ValidateAll();
			}
		}

		static void CloneTradeDetail(EntityTradeDetailWrapper sourceTradeDetail, OrgTradeDetail targetTradeDetail)
		{
			targetTradeDetail.CopyPersistentValuesFrom(sourceTradeDetail, new BusinessObjectCloneArgs(new[] { OrgTradeDetailSchema.Constants.PA_OW, OrgTradeDetailSchema.Constants.PA_Status }));
			targetTradeDetail.ProspectDetail.CopyPersistentValuesFrom(sourceTradeDetail.ProspectDetail, new BusinessObjectCloneArgs(new[] { OrgTradeProspectSchema.Constants.PAP_PA, OrgTradeProspectSchema.Constants.PAP_ExpectedTradeStartDate }));
			targetTradeDetail.CurrentProspectPeriod.CopyPersistentValuesFrom(sourceTradeDetail.CurrentProspectPeriod, new BusinessObjectCloneArgs(new[] { OrgTradePeriodSchema.Constants.PAS_PA, OrgTradePeriodSchema.Constants.PAS_OH_Client }));
		}

		static void DeleteIfNoTradeDetailsRemain(EntitySalesWrapper sales)
		{
			if (sales.TradeDetails.Count == 0)
			{
				sales.Delete();
			}
		}

		static void RemoveDefaultTradeDetails(OrgTradeDetailCollection tradeDetails, EntitySalesWrapper selectedEntitySales)
		{
			foreach (var defaultTradeDetail in tradeDetails.OfType<OrgTradeDetail>().Where(x => !x.IsInDatabase && x.PA_TradeMode.IsEmpty && x.SalesProductCode != SystemDefinedSalesProductList.Codes.Warehouse).ToArray())
			{
				tradeDetails.RemoveAndDelete(defaultTradeDetail);
			}
			selectedEntitySales?.EntityTradeDetailsCollection?.OnDefaultTradeDetailRemovedBySalesMatching();
		}

		static void MoveAllTradeDetails(EntitySalesWrapper from, EntitySalesWrapper to)
		{
			using (from.EntityTradeDetailsCollection.SuspendListChanged())
			using (to.EntityTradeDetailsCollection.SuspendListChanged())
			{
				foreach (EntityTradeDetailWrapper tradeDetail in from.EntityTradeDetailsCollection.ToArray())
				{
					from.EntityTradeDetailsCollection.Remove(tradeDetail);
					to.EntityTradeDetailsCollection.Add(tradeDetail);
				}
			}

			from.TradeDetails.Load();
			using (from.TradeDetails.SuspendListChanged())
			using (to.TradeDetails.SuspendListChanged())
			{
				foreach (OrgTradeDetail tradeDetail in from.TradeDetails.ToArray())
				{
					from.TradeDetails.Remove(tradeDetail);
					to.TradeDetails.Add(tradeDetail);
				}
			}
		}
	}
}
