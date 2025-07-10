using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed class DeliveryOrderProviderFactoryTest : TestCaseWithFactory
	{
		public void TestCreateDeliveryOrderProvider()
		{
			AssertType<ForwardingShipmentDeliveryOrderProvider>(Factory.New<ForwardingShipment>().DeliveryOrderProvider);
		}
	}
}
