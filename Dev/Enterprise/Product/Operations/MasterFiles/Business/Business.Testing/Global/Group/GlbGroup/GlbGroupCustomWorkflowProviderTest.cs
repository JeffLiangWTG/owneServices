using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroup))]
	sealed class GlbGroupCustomWorkflowProviderTest : WorkflowProviderTest<GlbGroup, GlbGroupProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.GlbGroupWorkflowDescriptorCode;
	}
}
