using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Module
{
	public class TrackingMAWBFilterBusinessObject : FilterStripBusinessObject
	{
		#region GetModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			AddNumberFilters(result);
			AddTextFilters(result);
			AddDateFilters(result);
			AddStatusFilters(result);
			AddBranchFilter(result);
			AddAlwaysAppliedFilters(result);

			return result;
		}

		#endregion

		#region Branch Filters

		void AddBranchFilter(ModuleFilterCollection filters)
		{
			if (LoggedInWebUser != null && !LoggedInWebUser.AreSecurityRightsGranted(WebSecurityRightsList.WebMAWBAdmin))
			{
				var branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, ExportAWBHeaderSchema.EH_GB_UserBranch, BranchList);
				branchFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ConsolExportAWBHeaderFilter|UserBranch", "Branch");
				branchFilter.Property = GlbBranch.CurrentBranch.PK;
				branchFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
				branchFilter.IsPublishedOnWeb = false;
			}
		}

		GlbBranchCollection BranchList
		{
			get
			{
				return branchList ?? (branchList = new GlbBranchCollection(Factory));
			}
		}
		GlbBranchCollection branchList;

		#endregion

		#region Always Applied Filter

		void AddAlwaysAppliedFilters(ModuleFilterCollection filters)
		{
			var awbType = filters.AddTextFilter("AWB Type", ExportAWBHeaderSchema.EH_AWBType);
			awbType.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ConsolExportAWBHeaderFilter|AWBType", "AWB Type");
			awbType.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			awbType.Property = AWBTypeList.Codes.AgentMaster;
		}

		#endregion

		#region Status Filters

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("FWB Status", ExportAWBHeaderSchema.EH_AWBStatus, new AWBMessagingStatusList());
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ConsolExportAWBHeaderFilter|FWBStatus", "FWB Status");
			filter.Category = FilterCategories.ModesAndTypes;
		}

		#endregion

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Issue Date", ExportAWBHeaderSchema.EH_AWBIssueDate).MultilingualDescription = ResString.GetMultilingualString("Forwarding|ConsolExportAWBHeaderFilter|AWBIssueDate", "Issue Date");
		}

		#endregion

		#region Text Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Origin IATA", ExportAWBHeaderSchema.EH_AWBOriginCode).MultilingualDescription = ResString.GetMultilingualString("Forwarding|ConsolExportAWBHeaderFilter|AWBOriginCode", "Origin IATA");
			filters.AddTextFilter("Destination IATA", ExportAWBHeaderSchema.EH_AirportOfDestinationCode).MultilingualDescription = ResString.GetMultilingualString("Forwarding|ConsolExportAWBHeaderFilter|AirportOfDestinationCode", "Destination IATA");
			filters.AddTextFilter("Issue Place", ExportAWBHeaderSchema.EH_AWBIssuePlace).MultilingualDescription = ResString.GetMultilingualString("Forwarding|ConsolExportAWBHeaderFilter|AWBIssuePlace", "Issue Place");
		}

		#endregion

		#region Number Filters

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter("MAWB #", ExportAWBHeaderSchema.EH_WayBillNumber).MultilingualDescription = ResString.GetMultilingualString("Forwarding|ConsolExportAWBHeaderFilter|ReferenceNumber", "MAWB #");
		}

		#endregion

		#region LoggedInWebUsersOrg

		public OrgHeader LoggedInWebUsersOrg
		{
			get
			{
				return LoggedInWebUser != null ? LoggedInWebUser.LoggedInOrganisation : null;
			}
		}

		#endregion

		#region Logged In Web User

		public OrgContactWebUser LoggedInWebUser { get; set; }

		#endregion

	}
}
