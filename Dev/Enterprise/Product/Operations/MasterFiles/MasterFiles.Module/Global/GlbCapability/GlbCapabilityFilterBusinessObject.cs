using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.MasterFiles.Module
{
	public class GlbCapabilityFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddFilters(result);
			return result;
		}

		void AddFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", GlbCapabilitySchema.G4_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbCapabilityFilter|Code", "Code");
			filters.AddTextFilter("Description", GlbCapabilitySchema.G4_Description).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbCapabilityFilter|Description", "Description");
			filters.AddTextFilter("Capability Membership Requirements", GlbCapabilitySchema.G4_MembershipRequirements).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbCapabilityFilter|CapabilityMembershipRequirements", "Capability Membership Requirements");

			AddCapacityScopeFilter(filters);

			filters.AddFlagsFilter("Allow Task Auto Assignment", new[] { Res.GetString("fbc6d4e3-7456-4b23-8b8e-a3fa8a7a57e8", "Allow Task Auto Assignment") }, new GetFlagsQuery[] { GetAutoAssignQuery }).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbCapabilityFilter|AllowTaskAutoAssignment", "Allow Task Auto Assignment");

			filters.AddFilter(new ModuleGuidPivotFilter("Resources with Capability", ModuleIDs.GlbStaff, GlbResourceCapabilityPivotSchema.G5_GS_Resource, GlbResourceCapabilityPivotSchema.G5_G4_Capability, () => new GlbStaffCollection(Factory), typeof(GlbCapability), typeof(GlbResourceCapabilityPivot))
			{
				MultilingualDescription = ResString.GetMultilingualString("GlbCapability|Filter|Resources with Capability", "Resources with Capability")
			});
		}

		void AddCapacityScopeFilter(ModuleFilterCollection filters)
		{
			var scopeFilter = filters.AddTextFilter("Capacity Scope", GlbCapabilitySchema.G4_CapacityScope, new GlbCapabilityScopeList());
			scopeFilter.Category = FilterCategories.StatusAndFlags;
			scopeFilter.DefaultProperty = GlbCapabilityScopeList.Codes.GlobalScope;
			scopeFilter.MultilingualDescription = ScopeFilterDescription;
		}

		ResourceString ScopeFilterDescription => ResString.GetMultilingualString("MasterFiles|GlbCapabilityFilter|CapacityScope", "Capacity Scope");

		ZQuery GetAutoAssignQuery(ZBool value)
		{
			return new ZQuery(GlbCapabilitySchema.G4_AllowTaskAutoAssignment, value);
		}

		#region Index Search Filters

		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			var fieldNameUpper = searchField.FieldName.ToUpperInvariant();
			switch (fieldNameUpper)
			{
				case SearchFieldConstants.CapacityScope:
					var scopeFilter = new IndexSearchModuleTextFilter(searchField, new GlbCapabilityScopeList(), FilterCategories.StatusAndFlags);
					scopeFilter.DefaultProperty = GlbCapabilityScopeList.Codes.GlobalScope;
					scopeFilter.MultilingualDescription = ScopeFilterDescription;
					return new SearchFieldOverride(searchField, scopeFilter);
				default:
					return base.ResolveSearchField(searchField);
			}
		}

		static class SearchFieldConstants
		{
			public const string CapacityScope = "CAPACITYSCOPE";
		}

		#endregion
	}
}
