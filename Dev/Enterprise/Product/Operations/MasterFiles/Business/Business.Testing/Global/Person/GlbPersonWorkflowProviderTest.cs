using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPerson))]
	sealed class GlbPersonWorkflowProviderTest : WorkflowProviderTest<GlbPerson, GlbPersonProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.GlbPersonDescriptorCode; }
		}
	}
}
