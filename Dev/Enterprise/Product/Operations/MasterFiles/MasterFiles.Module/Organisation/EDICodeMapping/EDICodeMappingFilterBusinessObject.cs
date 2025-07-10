using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class EDICodeMappingFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddGuidFilter(FilterDescription.Organization, ModuleIDs.Organisation, OrgPatternMatchOverrideSchema.OO_OH, OrgHeaderList)
				.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EDICodeMappingFilter|Organization", FilterDescription.Organization);

			filters.AddTextFilter(FilterDescription.ForeignCode, OrgPatternMatchOverrideSchema.OO_ForeignCode)
				.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EDICodeMappingFilter|ForeignCode", FilterDescription.ForeignCode);

			filters.AddTextFilter(FilterDescription.Context, OrgPatternMatchOverrideSchema.OO_Context, PatternMatchOverride.Lookups.OO_Context_List)
				.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EDICodeMappingFilter|Context", FilterDescription.Context);

			filters.AddCustomFilter(new EDICodeMappingRelationshipLocalCodeModuleFilter(FilterDescription.RelationshipLocalCode, PatternMatchOverride)
			{
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EDICodeMappingFilter|RelationshipLocalCode", FilterDescription.RelationshipLocalCode)
			});

			return filters;
		}

		ReadOnlyBusinessObjectFactory ReadOnlyFactory => readOnlyFactory ?? (readOnlyFactory = new ReadOnlyBusinessObjectFactory());
		ReadOnlyBusinessObjectFactory readOnlyFactory;

		OrganisationsFindBoxCollection OrgHeaderList => orgHeaderList ?? (orgHeaderList = new OrganisationsFindBoxCollection(Factory));
		OrganisationsFindBoxCollection orgHeaderList;

		OrgPatternMatchOverride PatternMatchOverride => patternMatchOverride ?? (patternMatchOverride = ReadOnlyFactory.New<OrgPatternMatchOverride>());
		OrgPatternMatchOverride patternMatchOverride;
	}

	static class FilterDescription
	{
		#region SuppressResourceStringsCheckRegion

		public const string Organization = "Organization";
		public const string ForeignCode = "Foreign Code";
		public const string Context = "Context";
		public const string RelationshipLocalCode = "Relationship & Local Code";

		#endregion
	}
}
