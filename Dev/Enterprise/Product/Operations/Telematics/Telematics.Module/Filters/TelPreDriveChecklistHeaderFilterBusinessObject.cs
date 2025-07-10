using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.Module.Filters
{
	public class TelPreDriveChecklistHeaderFilterBusinessObject : FilterStripBusinessObject
	{
		#region FilterStripBusinessObject

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var allFieldsCompletedMutilingualString = ResString.GetMultilingualString("Telematics|TelPreDriveChecklistHeaderFilter|AllFieldsCompleted", "All Fields Completed");
			var allFieldsCompletedFilter = filters.AddFlagsFilter("All Fields Completed", new string[] { allFieldsCompletedMutilingualString }, new GetFlagsQuery[] { GetAllFieldsCompletedQuery });
			allFieldsCompletedFilter.MultilingualDescription = allFieldsCompletedMutilingualString;
			allFieldsCompletedFilter.Category = FilterCategories.StatusAndFlags;

			var driverFilterMultilingualString = ResString.GetMultilingualString("Telematics|TelPreDriveChecklistHeaderFilter|Driver", "Driver");
			var driverCompletedFilter = filters.AddNkFilter("Driver", TelPreDriveChecklistHeaderSchema.TPH_GS_NKDriver, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
			driverCompletedFilter.MultilingualDescription = driverFilterMultilingualString;
			driverCompletedFilter.Category = FilterCategories.RelationshipOrgAndStaff;

			var notesFilterMultilingualString = ResString.GetMultilingualString("Telematics|TelPreDriveChecklistHeaderFilter|Notes", "Notes");
			var notesFilter = filters.AddTextFilter("Notes", TelPreDriveChecklistHeaderSchema.TPH_Notes);
			notesFilter.MultilingualDescription = notesFilterMultilingualString;
			notesFilter.Category = FilterCategories.TextSearch;

			var typeFilterMultilingualString = ResString.GetMultilingualString("Telematics|TelPreDriveChecklistHeaderFilter|Type", "Type");
			var typeFilter = filters.AddTextFilter("Type", TelPreDriveChecklistHeaderSchema.TPH_Type);
			typeFilter.MultilingualDescription = typeFilterMultilingualString;
			typeFilter.Category = FilterCategories.TextSearch;

			return filters;
		}

		#endregion

		#region SuppressResourceStringsCheckRegion
		public ZQuery GetAllFieldsCompletedQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(TelPreDriveChecklistHeader));

			var subQuery = new ZDBOnlySubQuery(typeof(TelPreDriveChecklistEntry), TelPreDriveChecklistEntrySchema.TPE_TPH_ChecklistHeader, value);
			subQuery.AddToFilter(TelPreDriveChecklistEntrySchema.TPE_IsAgreed, SQLComparisonOperator.NotEqual, TelPreDriveChecklistEntryValueTypes.Codes.Yes);

			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		#endregion
	}
}
