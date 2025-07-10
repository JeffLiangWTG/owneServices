namespace Enterprise.Rating.Module
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Rating.Business;
	using Enterprise.Rating.GUI;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Schema;

	public class CompanyTariffsFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddFiltersForTranslatableText("Company Tariff Description", RatingHeaderSchema.TH_GlobalRateDescription, typeof(CompanyTariff), ResString.GetMultilingualString("2d80c867-f4d2-4a64-8de3-fa8348512f63", "Company Tariff Description"));

			var contractNumberFilter = filters.AddTextFilter("Client Contract Number", RateFilterHelper.GetContractNumberQuery);
			contractNumberFilter.MultilingualDescription = ResString.GetMultilingualString("0c05eeeb-d974-4837-a5c1-b0b6d996d8b5", "Client Contract Number");
			contractNumberFilter.MaxLength = RateEntrySchema.TI_ContractNumber.MaxLength;

			var fmcTariffIDFilter = RateEntryFilterUtility.AddFMCTariffIDFilter(filters);
			fmcTariffIDFilter.SubGroup = RateFilterHelper.RateEntryFilterProcessor;

			var commodityCodeFilter = RateEntryFilterUtility.AddCommodityCodeFilter(filters, CommodityCodesList);
			commodityCodeFilter.SubGroup = RateFilterHelper.RateEntryFilterProcessor;

			var descriptions = new string[]
			{
				ResString.GetMultilingualString("8b626495-63a5-4f66-9521-6bbed9d5f9e1", "Globally"),
				ResString.GetMultilingualString("7478e1b9-4ec3-4e8a-986e-9de20933af9b", "Locally")
			};
			var flags = new GetFlagsQuery[]
			{
				GetShowGlobalQuery,
				GetShowLocalQuery
			};
			var globalRateFilter = filters.AddFlagsFilter(RateFilterHelper.Constants.GlobalRateFilter, descriptions, flags, JoinCondition.Or);
			globalRateFilter.MultilingualDescription = ResString.GetMultilingualString("7efd97e2-46ff-41ba-8ccf-042e80952bcd", "Published");
			globalRateFilter.Category = FilterCategories.ModesAndTypes;

			var locationList = new RatingLocationCollection(Factory);

			RateEntryFilterUtility.AddFirstLoadFilter(
				filters,
				(locationCode) => RateFilterHelper.GetLocationQuery(locationCode, RateEntrySchema.TI_FirstLoadLRC),
				locationList);

			RateEntryFilterUtility.AddLastDischargeFilter(
				filters,
				(locationCode) => RateFilterHelper.GetLocationQuery(locationCode, RateEntrySchema.TI_LastDischargeLRC),
				locationList);

			RateEntryFilterUtility.AddFirstRouteSetLoadFilter(
				filters,
				(locationCode) => RateFilterHelper.GetLocationQuery(locationCode, RateEntrySchema.TI_FirstRouteSetLoadPortLRC),
				locationList);

			RateEntryFilterUtility.AddLastRouteSetDischargeFilter(
				filters,
				(locationCode) => RateFilterHelper.GetLocationQuery(locationCode, RateEntrySchema.TI_LastRouteSetDischargePortLRC),
				locationList);

			return filters;
		}

		ZQuery GetShowGlobalQuery(ZBool showGlobal)
		{
			var result = new ZDBOnlyQuery(typeof(RatingHeader));

			if (showGlobal)
			{
				result.AddToFilter(RatingHeaderSchema.TH_GC, null);
			}

			return result;
		}

		ZQuery GetShowLocalQuery(ZBool showLocal)
		{
			var result = new ZDBOnlyQuery(typeof(RatingHeader));

			if (showLocal)
			{
				result.AddToFilter(RatingHeaderSchema.TH_GC, SQLComparisonOperator.NotEqual, null);
			}

			return result;
		}

		RateFilterHelper RateFilterHelper
		{
			get { return fRateFilterHelper ?? (fRateFilterHelper = new RateFilterHelper(Factory)); }
		}

		RateFilterHelper fRateFilterHelper;

		RefCommodityCodeCollection CommodityCodesList => new RefCommodityCodeCollection(Factory);
	}
}

