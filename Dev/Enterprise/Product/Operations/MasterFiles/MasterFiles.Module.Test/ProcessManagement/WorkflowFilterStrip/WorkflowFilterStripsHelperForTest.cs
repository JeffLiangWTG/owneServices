using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	class WorkflowFilterStripsHelperForTest : WorkflowFilterStripsHelper
	{
		public WorkflowFilterStripsHelperForTest(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory, bool shouldFilterByCompanyForAnyOpenTask)
			: base(businessObjectType, templateCode, factory, shouldFilterByCompanyForAnyOpenTask)
		{
		}

		public WorkflowFilterStripsHelperForTest(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory)
			: base(businessObjectType, templateCode, factory, true)
		{
		}

		public void AddMilestonesFilters_Exposed(ModuleFilterCollection filters)
		{
			AddMilestonesFilters(filters);
		}

		public void AddRelatedMilestoneFilters_Exposed(ModuleFilterCollection filters, params ZDBOnlySubQuery[] relatedParentJoiningQueries)
		{
			AddRelatedMilestoneFilters(filters, relatedParentJoiningQueries);
		}
	}
}
