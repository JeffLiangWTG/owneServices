using System.Linq;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using WiseRates.Constants;

namespace Enterprise.Rating.Module;

public abstract class MappedChargeCodeFilterBusinessObject : FilterStripBusinessObject
{
	protected static class Descriptions
	{
		public readonly static string ForeignCode = (NoResString)"Foreign Code";
		public readonly static string ForeignName = (NoResString)"Foreign Name";
		public readonly static string Code = (NoResString)"Code";
		public readonly static string Description = (NoResString)"Description";
	}

	protected abstract string[] VisibleFilters { get; }

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		ModuleFilterCollection filters = new ModuleFilterCollection();
		var filter = filters.AddTextFilter(Descriptions.ForeignCode, MappedChargeCodeSchema.UCC_ForeignCode);
		filter.MultilingualDescription = ResString.GetMultilingualString("Rating|UniversalChargeCodeFilter|ForeignCode", "Foreign Code");
		if (VisibleFilters.Contains(Descriptions.ForeignCode))
		{
			filter.Visibility = FilterVisibility.AlwaysVisible;
		}

		filter = filters.AddTextFilter(Descriptions.ForeignName, MappedChargeCodeSchema.UCC_ForeignName);
		filter.MultilingualDescription = ResString.GetMultilingualString("Rating|UniversalChargeCodeFilter|ForeignName", "Foreign Name");
		if (VisibleFilters.Contains(Descriptions.ForeignName))
		{
			filter.Visibility = FilterVisibility.AlwaysVisible;
		}

		filter = filters.AddTextFilter(Descriptions.Code, MappedChargeCodeSchema.UCC_Code);
		filter.MultilingualDescription = ResString.GetMultilingualString("Rating|UniversalChargeCodeFilter|Code", "Code");
		if (VisibleFilters.Contains(Descriptions.Code))
		{
			filter.Visibility = FilterVisibility.AlwaysVisible;
		}

		filter = filters.AddTextFilter(Descriptions.Description, MappedChargeCodeSchema.UCC_Description);
		filter.MultilingualDescription = ResString.GetMultilingualString("Rating|UniversalChargeCodeFilter|Description", "Description");
		if (VisibleFilters.Contains(Descriptions.Description))
		{
			filter.Visibility = FilterVisibility.AlwaysVisible;
		}

		filter = filters.AddTextFilter("Carrier", MappedChargeCodeSchema.UCC_Carrier);
		filter.MultilingualDescription = ResString.GetMultilingualString("Rating|UniversalChargeCodeFilter|Carrier", "Carrier");

		filter = filters.AddTextFilter("Rate Provider", MappedChargeCodeSchema.UCC_RateProvider, GetRateProviderList);
		filter.MultilingualDescription = ResString.GetMultilingualString("Rating|UniversalChargeCodeFilter|RateProvider", "Rate Provider");
		filter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;

		filter = filters.AddTextFilter("Global Charge Code", MappedChargeCodeSchema.UCC_GlobalChargeCode);
		filter.MultilingualDescription = ResString.GetMultilingualString("Rating|UniversalChargeCodeFilter|GlobalChargeCodeMapped", "Global Charge Code");

		filter = filters.AddTextFilter("Global Charge Code Description", MappedChargeCodeSchema.UCC_GlobalChargeCodeDescription);
		filter.MultilingualDescription = ResString.GetMultilingualString("Rating|UniversalChargeCodeFilter|GlobalChargeCodeDescription", "Global Charge Code Description");

		filter = filters.AddTextFilter("Charge Code", MappedChargeCodeSchema.UCC_LocalChargeCode);
		filter.MultilingualDescription = ResString.GetMultilingualString("Rating|UniversalChargeCodeFilter|LocalChargeCodeMapped", "Charge Code");

		filter = filters.AddTextFilter("Charge Code Description", MappedChargeCodeSchema.UCC_LocalChargeCodeDescription);
		filter.MultilingualDescription = ResString.GetMultilingualString("Rating|UniversalChargeCodeFilter|LocalChargeCodeDescription", "Charge Code Description");

		return filters;
	}

	CodeDescriptionPairList GetRateProviderList()
	{
		var providerList = new CodeDescriptionPairList();

		providerList.AddPair(
			WRConstants.RateProviders.CargoSphere,
			WRConstants.RateProviders.GetDescription(WRConstants.RateProviders.CargoSphere));
		providerList.AddPair(
			WRConstants.RateProviders.CargoGuide,
			WRConstants.RateProviders.GetDescription(WRConstants.RateProviders.CargoGuide));

		return providerList;
	}

	protected override bool ShouldAddCustomSqlFilter => false;
}
