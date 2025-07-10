using CargoWise.Types;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(GateBooking))]
	sealed class GateBookingWorkflowProviderTest : WorkflowProviderTest<GateBooking, GateBookingProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.FacilityGateWorkflowDescriptorCode;
	}
}
