using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaff))]
	sealed class GlbStaffWorkflowProviderTest : WorkflowProviderTest<GlbStaff, GlbStaffProcessTasksCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return GlbStaffWorkflowDescriptor.WorkflowTypeCode; }
		}
	}
}
