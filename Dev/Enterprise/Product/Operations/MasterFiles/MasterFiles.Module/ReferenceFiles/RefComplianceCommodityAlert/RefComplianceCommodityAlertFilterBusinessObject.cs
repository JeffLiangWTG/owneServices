using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefComplianceCommodityAlertFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddTextFilter("AlertName", RefComplianceCommodityAlertSchema.RCR_AlertName).MultilingualDescription = ResString.GetMultilingualString("RefComplianceCommodityAlertFilterBusinessObject|AlertName", "Name");

			filters.AddTextFilter("TradeDirection", RefComplianceCommodityAlertSchema.RCR_TradeDirection, new RefComplianceCommodityAlertDirectionList()).MultilingualDescription = ResString.GetMultilingualString("RefComplianceCommodityAlertFilterBusinessObject|TradeDirection", "Direction");

			filters.AddTextFilter("AlertType", RefComplianceCommodityAlertSchema.RCR_AlertType, new RefComplianceCommodityAlertTypeList()).MultilingualDescription = ResString.GetMultilingualString("RefComplianceCommodityAlertFilterBusinessObject|AlertType", "Alert Type");

			filters.AddTextFilter("RiskStatus", RefComplianceCommodityAlertSchema.RCR_CommodityRiskStatus, RefComplianceCommodityAlertLookups.GetCommodityRiskStatus()).MultilingualDescription = ResString.GetMultilingualString("RefComplianceCommodityAlertFilterBusinessObject|RiskStatus", "Risk Status");

			var countryFilter = filters.AddNkFilter("CountryRegion", RefComplianceCommodityAlertSchema.RCR_CountryRegion, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			countryFilter.Category = FilterCategories.Locations;
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("RefComplianceCommodityAlertFilterBusinessObject|CountryRegion", "Country/Region");
			countryFilter.SuspendValidation(); // allow query for EU

			return filters;
		}
	}
}
