using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteVehicleMovement))]
	public class GteVehicleMovementWorkflowProviderTest : WorkflowProviderTest<GteVehicleMovement, GteVehicleMovementProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.GteVehicleMovementWorkflowDescriptorCode;
	}
}
