using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteBooking))]
	public class GteBookingWorkflowProviderTest : WorkflowProviderTest<GteBooking, GteBookingProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.GteBookingWorkflowDescriptorCode;
	}
}
