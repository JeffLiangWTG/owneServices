using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class CustomsRulesFilterStripBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter strings")]
		public static class FilterConstants
		{
			public const string Organization = "Organization";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var moduleFilterCollection = new ModuleFilterCollection();
			var organizationFilter = moduleFilterCollection.AddGuidFilter(FilterConstants.Organization, ModuleIDs.Organisation, CusPermitHeaderSchema.CPH_OH_PermitHolder, new OrganisationsFindBoxCollection(Factory));
			organizationFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CustomsRuleFilter|Organization", FilterConstants.Organization);
			return moduleFilterCollection;
		}
	}
}
