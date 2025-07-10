using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed class ILElectronicMessageProviderFactoryTest : TestCaseWithFactory
	{
		public void TestCreateILElectronicMessageProvider()
		{
			var factory = Factory;
			var forwardingShipment = factory.New<ForwardingShipment>();
			var forwardingConsol = factory.New<ForwardingConsol>();
			var gatePassMovementProvider = forwardingShipment.GatePassMovementProvider;
			var deliveryOrderProvider = forwardingShipment.DeliveryOrderProvider;

			AssertType<ForwardingShipmentGatePassMovementProvider>(ILElectronicMessageProviderFactory.CreateILElectronicMessageProvider(gatePassMovementProvider, forwardingShipment));
			AssertType<ForwardingConsolGatePassMovementProvider>(ILElectronicMessageProviderFactory.CreateILElectronicMessageProvider(gatePassMovementProvider, forwardingConsol));

			AssertType<ForwardingShipmentDeliveryOrderProvider>(ILElectronicMessageProviderFactory.CreateILElectronicMessageProvider(deliveryOrderProvider, forwardingShipment));
		}
	}
}
