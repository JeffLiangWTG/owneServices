using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteGateMovementBooking))]
	public class GteGateMovementBookingWorkflowProviderTest : WorkflowProviderTest<GteGateMovementBooking, GteGateMovementBookingProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.GteGateMovementBookingWorkflowDescriptorCode;
	}
}
