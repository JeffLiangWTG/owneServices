using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.FilterStrips;

namespace Enterprise.Tracking.Business
{
	public class LinerAndAgencyContainerFilterStripBusinessObject : BillContainersFilterStrip
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var containerTypeFilter = filters.AddTextFilter("Container Type", GetTypeQuery);
			containerTypeFilter.MultilingualDescription = ResString.GetMultilingualString("c04305bb-7d70-4dce-bc8b-d6e1485509b6", "Container Type");
			containerTypeFilter.Category = FilterCategories.ModesAndTypes;

			return filters;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(new WorkflowFilterStripsHelper(typeof(LinerAndAgencyContainer), WorkflowDescriptors.AgencyContainerWorkflowDescriptorCode, Factory));
			return helpers;
		}

		ZQuery GetTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var filter = new ZDBOnlyQuery(typeof(LinerAndAgencyContainer));
			var subQuery = new ZDBOnlySubQuery(typeof(RefContainer), JobContainerSchema.JC_RC);
			subQuery.AddToFilter(RefContainerSchema.RC_Code, comparisonOperator, value);
			filter.AddSubQuery(subQuery, JoinCondition.And);
			return filter;
		}

		public OrgContact LoggedInUser { get; set; }

		protected override FilterStripLayoutsHelper GetNewLayoutsHelper()
		{
			return new FilterStripLayoutsHelperForWeb(this, LoggedInUser);
		}
	}
}
