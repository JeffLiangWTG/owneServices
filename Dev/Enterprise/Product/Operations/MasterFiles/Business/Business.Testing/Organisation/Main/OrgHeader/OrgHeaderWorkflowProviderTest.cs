using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgHeader))]
	sealed class OrgHeaderWorkflowProviderTest : WorkflowProviderTest<OrgHeader, OrgHeaderProcessTasksCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return OrgHeaderWorkflowDescriptor.WorkflowTypeCode; }
		}
	}
}
