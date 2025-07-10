using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCall))]
	sealed class OrgSalesCallWorkflowProviderTest : WorkflowProviderTest<OrgSalesCall, OrgSalesCallProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return OrgSalesCallWorkflowDescriptor.WorkflowTypeCode; }
		}
	}
}
