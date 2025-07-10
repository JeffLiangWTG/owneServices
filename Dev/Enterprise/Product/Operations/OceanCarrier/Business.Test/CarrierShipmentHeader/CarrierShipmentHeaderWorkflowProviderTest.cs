using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentHeader))]
	sealed class CarrierShipmentHeaderWorkflowProviderTest : WorkflowProviderTest<CarrierShipmentHeader, CarrierShipmentHeaderProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CarrierShipmentHeaderWorkflowDescriptorCode;
	}
}
