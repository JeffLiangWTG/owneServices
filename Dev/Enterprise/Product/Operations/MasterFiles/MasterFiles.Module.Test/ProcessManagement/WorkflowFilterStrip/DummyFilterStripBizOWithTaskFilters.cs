using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DummyFilterStripBizOWithTaskFilters : DummyFilterStripBusinessObject
	{
		public DummyFilterStripBizOWithTaskFilters(string workflowTemplateCode)
		{
			this.workflowTemplateCode = workflowTemplateCode;
		}

		readonly string workflowTemplateCode;

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(new WorkflowWithExtraTasksFilterStripsHelper(typeof(DummyBusinessObjectWithWorkflow), workflowTemplateCode, Factory));

			return helpers;
		}
	}
}
