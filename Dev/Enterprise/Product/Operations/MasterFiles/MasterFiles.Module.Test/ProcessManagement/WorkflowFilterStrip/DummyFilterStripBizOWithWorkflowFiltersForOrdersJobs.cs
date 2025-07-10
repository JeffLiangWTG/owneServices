using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DummyFilterStripBizOWithWorkflowFiltersForOrdersJobs : DummyFilterStripBusinessObject
	{
		public DummyFilterStripBizOWithWorkflowFiltersForOrdersJobs()
		{
			QueryObjectType = typeof(DummyBusinessObjectWithWorkflow);
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(new WorkflowFilterStripsHelperForTest(typeof(DummyBusinessObjectWithWorkflow), WorkflowDescriptors.OrderWorkflowDescriptorCode, Factory));

			return helpers;
		}
	}
}
