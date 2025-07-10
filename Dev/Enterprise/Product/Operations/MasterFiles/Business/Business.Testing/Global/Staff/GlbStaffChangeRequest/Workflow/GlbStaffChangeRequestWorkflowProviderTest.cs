using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffChangeRequest))]
	class GlbStaffChangeRequestWorkflowProviderTest : WorkflowProviderTest<GlbStaffChangeRequest, GlbStaffChangeRequestProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.GlbStaffChangeRequestWorkflowDescriptorCode;
	}
}
