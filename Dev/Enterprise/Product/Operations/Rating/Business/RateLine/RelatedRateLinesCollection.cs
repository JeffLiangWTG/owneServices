using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RelatedRateLinesCollection : BusinessObjectCollection<RelatedRateLine>
	{
		public RelatedRateLinesCollection(BusinessObjectFactory factory, RateEntry master)
			: base(factory)
		{
			Master = master;
			FactoryForLoad = Master.Parent.ShowCostingCompanyTariffFactory;
			IsManagedForDataRefresh = false;
		}

		public readonly RateEntry Master;
		readonly BusinessObjectFactory FactoryForLoad;

		protected override BusinessObject AddNewCore(Type bizoType)
		{
			var pk = Guid.NewGuid();
			using (pk.MarkAsInConstruction(RateLinesSchema.PK, Factory))
			{
				return base.AddNewCore(bizoType, pk);
			}
		}

		protected override BusinessObject CreateBusinessObjectFromRow(DataRow row)
		{
			using (row.MarkAsInConstruction(RateLinesSchema.PK, Factory))
			{
				return base.CreateBusinessObjectFromRow(row);
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			using (elementToDelete.MarkAsInDeletion())
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

		#region Loading

		public override void Load()
		{
			RemoveAll();
			if (ShowRelatedLines)
			{
				var maximumRows = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;

				var entriesFilter = GetEntriesFilter(maximumRows);
				var entries = FactoryForLoad.Load<RateEntry>(entriesFilter);

				var relatedLinesFilter = new ZQuery(RateLinesSchema.TL_TI, entries.Select(e => e.PK));

				var direction = OrgRateTariffLevel.GetOrgRateTariffLevelDirection(Master.JobDirection());
				relatedLinesFilter.AddToFilter(RateLinesSchema.TL_CompanyTariffLevel, SQLComparisonOperator.LessThanOrEqualTo, (ZByte)Master.GetCompanyTariffLevels(direction).Max());

				relatedLinesFilter.MaximumRows = maximumRows;

				var relatedRateLines = FactoryForLoad.Load<RelatedRateLine>(relatedLinesFilter);

				AddRange(relatedRateLines);
				RemoveCompanyTariffOverriddenLines();
				SetReadOnlyIncludingChildren(true);
			}
		}

		ZQuery GetEntriesFilter(int maximumRows)
		{
			var entryQuery = new ZDBOnlyQuery(typeof(RateEntry));
			AddLocationFilter(entryQuery);
			AddModeFilter(entryQuery);
			AddCommodityCodeFilter(entryQuery);
			AddServiceLevelFilter(entryQuery);
			AddDateFilter(entryQuery);
			AddRateTypeFilter(entryQuery);
			AddWarehouseFilter(entryQuery);
			AddFMCTariffIDFilter(entryQuery);
			AddContractNumberFilter(entryQuery);
			AddIsNonOperatedReeferFilter(entryQuery);
			entryQuery.MaximumRows = maximumRows;

			return entryQuery;
		}

		bool ShowRelatedLines
		{
			get
			{
				var result = !Master.Parent.IsStandardCostRate();

				if (result)
				{
					result = Master.Parent.ShowCosting || Master.Parent.ShowCompanyTariff || Master.Parent.ShowClientRates;
				}

				if (result)
				{
					if (Master.TI_RateCategory == RatingConstants.RateCategory.CST)
					{
						result = !Master.TI_Mode.IsEmpty;
					}
					else if (Master.IsWHS() || Master.IsTRW() || Master.IsTWU())
					{
						result = Master.AllWarehouses || Master.Warehouse() != null;
					}
					else
					{
						result = !Master.TI_OriginLRC.IsEmpty || !Master.TI_DestinationLRC.IsEmpty;
					}
				}

				return result;
			}
		}

		#region Locations

		void AddLocationFilter(ZDBOnlyQuery entryQuery)
		{
			var locationFilter = new ZQuery();

			if (!Master.TI_OriginLRC.IsEmpty)
			{
				locationFilter.AddToFilter(LocationHelper.GetLocationFilter(FactoryForLoad, Master.TI_OriginLRC, RateEntrySchema.TI_OriginLRC, true, typeof(RateEntry)), JoinCondition.And);
			}

			if (!Master.TI_DestinationLRC.IsEmpty)
			{
				locationFilter.AddToFilter(LocationHelper.GetLocationFilter(FactoryForLoad, Master.TI_DestinationLRC, RateEntrySchema.TI_DestinationLRC, true, typeof(RateEntry)), JoinCondition.And);
			}

			if (!Master.TI_ViaLRC.IsEmpty)
			{
				locationFilter.AddToFilter(LocationHelper.GetLocationFilter(FactoryForLoad, Master.TI_ViaLRC, RateEntrySchema.TI_ViaLRC, true, typeof(RateEntry)), JoinCondition.And);
			}

			entryQuery.AddToFilter(locationFilter);
		}

		#endregion

		#region Mode

		void AddModeFilter(ZDBOnlyQuery entryQuery)
		{
			entryQuery.AddToFilter(RateEntrySchema.TI_RateCategory, Master.TI_RateCategory);
			if (Master.IsSupplementaryEntry())
			{
				entryQuery.AddToFilter(RatingHelper.GetOriginDestinationModeExclusiveFilter(Master.FreightMode()));
			}
			else
			{
				entryQuery.AddToFilter(new ZQuery(RateEntrySchema.TI_Mode, Master.TI_Mode));
			}

			if (Master.Container != null)
			{
				var containerQuery = new ZQuery(RateEntrySchema.TI_RC, null);
				var classContainers = Master.IsFreightEntry() ? Master.Container.ContainersInSameFreightRateClass : Master.Container.ContainersInSameHandlingRateClass;
				RatingHelper.AddContainerQuery(Master.TI_RC, classContainers, containerQuery);
				entryQuery.AddToFilter(containerQuery, JoinCondition.And);
			}
		}

		#endregion

		#region Commodity Code

		void AddCommodityCodeFilter(ZDBOnlyQuery entryQuery)
		{
			if (!Master.TI_RH_NKCommodityCode.IsEmpty)
			{
				var commodity = new ZQuery(RateEntrySchema.TI_RH_NKCommodityCode, Master.TI_RH_NKCommodityCode);
				commodity.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RH_NKCommodityCode, SQLComparisonOperator.Equal, ZString.Empty);
				entryQuery.AddToFilter(commodity, JoinCondition.And);
			}
		}

		#endregion

		#region Service Level

		void AddServiceLevelFilter(ZDBOnlyQuery entryQuery)
		{
			if (!Master.TI_RS_NKServiceLevel_NI.IsEmpty)
			{
				var serviceLevel = new ZQuery(RateEntrySchema.TI_RS_NKServiceLevel_NI, Master.TI_RS_NKServiceLevel_NI);
				serviceLevel.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RS_NKServiceLevel_NI, SQLComparisonOperator.Equal, ZString.Empty);
				entryQuery.AddToFilter(serviceLevel, JoinCondition.And);
			}
		}

		#endregion

		#region Dates

		void AddDateFilter(ZDBOnlyQuery entryQuery)
		{
			var endDateFilter = new ZQuery(RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			endDateFilter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.Equal, null);
			entryQuery.AddToFilter(endDateFilter);
		}

		#endregion

		#region Warehouse

		void AddWarehouseFilter(ZDBOnlyQuery entryQuery)
		{
			if (!Master.TI_WW_Warehouse.IsEmpty)
			{
				var warehouseQuery = new ZQuery(RateEntrySchema.TI_ParentID, Master.TI_WW_Warehouse);
				warehouseQuery.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_ParentID, SQLComparisonOperator.Equal, null);
				entryQuery.AddToFilter(warehouseQuery, JoinCondition.And);
			}
		}

		#endregion

		#region FMC Tariff ID

		void AddFMCTariffIDFilter(ZDBOnlyQuery entryQuery)
		{
			if (!Master.TI_FMCTariffID.IsEmpty)
			{
				var fmcTariffIDFilter = new ZQuery(RateEntrySchema.TI_FMCTariffID, Master.TI_FMCTariffID);
				fmcTariffIDFilter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_FMCTariffID, SQLComparisonOperator.Equal, ZString.Empty);
				entryQuery.AddToFilter(fmcTariffIDFilter, JoinCondition.And);
			}
		}

		#endregion

		#region Contract Number

		void AddContractNumberFilter(ZDBOnlyQuery entryQuery)
		{
			if (!Master.TI_ContractNumber.IsEmpty)
			{
				var contractNumberFilter = new ZQuery(RateEntrySchema.TI_ContractNumber, Master.TI_ContractNumber);
				contractNumberFilter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_ContractNumber, SQLComparisonOperator.Equal, ZString.Empty);
				entryQuery.AddToFilter(contractNumberFilter, JoinCondition.And);
			}
		}

		#endregion

		#region Non Operated Reefer

		void AddIsNonOperatedReeferFilter(ZDBOnlyQuery entryQuery)
		{
			if (!Master.TI_IsNonOperatedReefer.IsEmpty)
			{
				var isNonOperatedReeferFilter = new ZQuery(RateEntrySchema.TI_IsNonOperatedReefer, Master.TI_IsNonOperatedReefer);
				isNonOperatedReeferFilter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_IsNonOperatedReefer, SQLComparisonOperator.Equal, ZString.Empty);
				entryQuery.AddToFilter(isNonOperatedReeferFilter, JoinCondition.And);
			}
		}

		#endregion

		#region Rate Type and Supplier / Carrier

		void AddRateTypeFilter(ZDBOnlyQuery entryQuery)
		{
			var rateTypeFilter = new ZQuery();

			if (Master.Parent.ShowClientRates && Master.IsQuote() && !Master.Parent.TH_OH.IsEmpty)
			{
				var clientRateFilter = new ZQuery(RatingHeaderSchema.TH_RateType, (ZString)RatingConstants.RatingHeaderTypes.ClientRate);
				clientRateFilter.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_OH, SQLComparisonOperator.Equal, Master.Parent.TH_OH);
				rateTypeFilter.AddToFilter(clientRateFilter, JoinCondition.Or);
			}

			if (Master.Parent.ShowCompanyTariff && !Master.IsCompanyTariff())
			{
				var compTariffFilter = new ZQuery(RatingHeaderSchema.TH_RateType, (ZString)RatingConstants.RatingHeaderTypes.Tariff);
				compTariffFilter.AddToFilter(RatingHeaderSchema.TH_GlobalRateLevel, (ZByte)1);
				rateTypeFilter.AddToFilter(compTariffFilter, JoinCondition.Or);
			}

			if (Master.Parent.ShowCosting)
			{
				var costingFilter = new ZQuery(RatingHeaderSchema.TH_RateType, (ZString)RatingConstants.RatingHeaderTypes.Costing);

				if (Master.IsCosting())
				{
					costingFilter.AddToFilter(RatingHeaderSchema.TH_OH, null);
				}
				else if (!Master.TI_OH_Supplier.IsEmpty)
				{
					costingFilter.AddToFilter(RatingHeaderSchema.TH_OH, Master.TI_OH_Supplier);
				}
				else if (!Master.TI_OH_TransportProvider.IsEmpty)
				{
					costingFilter.AddToFilter(RatingHeaderSchema.TH_OH, Master.TI_OH_TransportProvider);
				}

				rateTypeFilter.AddToFilter(costingFilter, JoinCondition.Or);
			}

			if (!Master.TI_OH_Supplier.IsEmpty)
			{
				var supplierFilter = new ZQuery();
				supplierFilter.AddToFilter(RateEntrySchema.TI_OH_Supplier, Master.TI_OH_Supplier);
				supplierFilter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_OH_Supplier, SQLComparisonOperator.Equal, null);
				entryQuery.AddToFilter(supplierFilter);
			}

			if (!Master.TI_OH_TransportProvider.IsEmpty)
			{
				var transportProviderFilter = new ZQuery();
				transportProviderFilter.AddToFilter(RateEntrySchema.TI_OH_TransportProvider, Master.TI_OH_TransportProvider);
				transportProviderFilter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_OH_TransportProvider, SQLComparisonOperator.Equal, null);
				entryQuery.AddToFilter(transportProviderFilter);
			}

			var headerQuery = new ZDBOnlySubQuery(typeof(RatingHeader), RateEntrySchema.TI_TH);

			var company = Master.Company() ?? GlbCompany.CurrentCompany;
			var companyFilter = new ZQuery(RatingHeaderSchema.TH_GC, company.PK);
			companyFilter.AddToFilter(new ZQuery(RatingHeaderSchema.TH_GC, null), JoinCondition.Or);

			headerQuery.AddToFilter(companyFilter, JoinCondition.And);
			headerQuery.AddToFilter(rateTypeFilter, JoinCondition.And);

			entryQuery.AddSubQuery(headerQuery, JoinCondition.And);
		}

		#endregion

		#region Company Tariff Overridden Lines

		void RemoveCompanyTariffOverriddenLines()
		{
			for (var i = Count - 1; i >= 0; i--)
			{
				if (this[i].IsTariff &&
					this[i].TL_CompanyTariffLevel < Master.GetCompanyTariffLevels().Max())
				{
					for (var j = 0; j < Count; j++)
					{
						if (this[j].IsTariff &&
							this[i].TL_AC == this[j].TL_AC &&
							this[i].TL_CompanyTariffLevel < this[j].TL_CompanyTariffLevel)
						{
							Remove(this[i]);
							break;
						}
					}
				}
			}
		}

		#endregion

		#endregion

		#region Allow New

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}

