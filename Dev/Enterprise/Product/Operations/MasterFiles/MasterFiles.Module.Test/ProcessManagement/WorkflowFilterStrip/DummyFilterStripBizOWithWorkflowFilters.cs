using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	public sealed class DummyFilterStripBizOWithWorkflowFilters : DummyFilterStripBusinessObject
	{
		public DummyFilterStripBizOWithWorkflowFilters(string workflowTemplateCode = "DUM", bool shouldFilterByCompanyForAnyOpenTask = true)
		{
			this.shouldFilterByCompanyForAnyOpenTask = shouldFilterByCompanyForAnyOpenTask;
			QueryObjectType = typeof(DummyBusinessObjectWithWorkflow);
			this.workflowTemplateCode = workflowTemplateCode;
		}

		readonly bool shouldFilterByCompanyForAnyOpenTask;

		readonly string workflowTemplateCode;

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(new WorkflowFilterStripsHelperForTest(typeof(DummyBusinessObjectWithWorkflow), workflowTemplateCode, Factory, shouldFilterByCompanyForAnyOpenTask));

			return helpers;
		}
	}
}
