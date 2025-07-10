using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business.Extensions;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportEventDataModelTest : TestCaseWithFactory
	{
		public void TestOriginGetter_ReturnOriginPortFromShipment()
		{
			var shipment = Factory.New<CommonShipment>();
			var transport = shipment.Transports.New(from: "UAIEV", to: "AUSYD");

			var model = new TransportEventDataModel(transport);

			AssertEquals("Property value", "UAIEV", model.Origin);
		}

		public void TestDestinationGetter_ReturnDestinationPortFromShipment()
		{
			var shipment = Factory.New<CommonShipment>();
			var transport = shipment.Transports.New(from: "UAIEV", to: "AUSYD");

			var model = new TransportEventDataModel(transport);
			AssertEquals("Property value", "AUSYD", model.Destination);
		}
	}
}
