using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerMovement))]
	internal class ContainerMovementWorkflowTest : WorkflowProviderTest<ContainerMovement, ContainerMovementProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get
			{
				return WorkflowDescriptors.ContainerMovementWorkflowDescriptorCode;
			}
		}
	}
}
