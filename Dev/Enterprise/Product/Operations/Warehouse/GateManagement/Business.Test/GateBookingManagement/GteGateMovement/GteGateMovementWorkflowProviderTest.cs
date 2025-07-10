using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteGateMovement))]
	public class GteGateMovementWorkflowProviderTest : WorkflowProviderTest<GteGateMovement, GteGateMovementProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.GteGateMovementWorkflowDescriptorCode;
	}
}
