using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentTransportValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJW_RL_NKLoadPort()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NLAMS";

			Transport transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "NLAMS";
			transport.JW_OA_DepartureLocation = Factory.New<OrgHeader>().MainAddress.PK;
			AssertHasError(transport.JW_RL_NKLoadPortInfo, "A routing leg cannot load at the shipment's destination.");

			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			shipment.JS_RL_NKDestination = "NLRTM";
			transport.JW_RL_NKDiscPort = "NLRTM";
			transport.JW_RL_NKLoadPort = "NLRTM";
			AssertNoErrors(transport.JW_RL_NKLoadPortInfo);

			transport.JW_TransportMode = Core.Constants.TransportModes.Rail;
			shipment.JS_RL_NKDestination = "NLAMS";
			transport.JW_RL_NKDiscPort = "NLAMS";
			transport.JW_RL_NKLoadPort = "NLAMS";
			AssertNoErrors(transport.JW_RL_NKLoadPortInfo);

			transport.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			shipment.JS_RL_NKDestination = "NLRTM";
			transport.JW_RL_NKDiscPort = "NLRTM";
			transport.JW_RL_NKLoadPort = "NLRTM";
			AssertNoErrors(transport.JW_RL_NKLoadPortInfo);

			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKDestination = "NLAMS";
			transport.JW_RL_NKDiscPort = "NLAMS";
			transport.JW_RL_NKLoadPort = "NLAMS";
			transport.JW_OA_ArrivalLocation = Factory.New<OrgHeader>().MainAddress.PK;
			AssertNoErrors(transport.JW_RL_NKLoadPortInfo);

			transport.JW_OA_DepartureLocation = ZGuid.Empty;
			AssertHasError(transport.JW_RL_NKLoadPortInfo, "The Load and Discharge cannot be the same.");

			transport.JW_RL_NKLoadPort = "GBLON";
			AssertNoErrors(transport.JW_RL_NKLoadPortInfo);
		}

		public void TestSameLoadDiscPort()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NLAMS";

			Transport transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "AUBNE";
			AssertHasError(transport.JW_RL_NKLoadPortInfo, "The Load and Discharge cannot be the same.");
			AssertHasError(transport.JW_RL_NKDiscPortInfo, "The Load and Discharge cannot be the same.");

			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			AssertNoErrors(transport.JW_RL_NKLoadPortInfo);
			AssertNoErrors(transport.JW_RL_NKDiscPortInfo);

			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertHasError(transport.JW_RL_NKLoadPortInfo, "The Load and Discharge cannot be the same.");
			AssertHasError(transport.JW_RL_NKDiscPortInfo, "The Load and Discharge cannot be the same.");

			transport.JW_TransportMode = Core.Constants.TransportModes.Rail;
			AssertNoErrors(transport.JW_RL_NKLoadPortInfo);
			AssertNoErrors(transport.JW_RL_NKDiscPortInfo);

			transport.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			AssertNoErrors(transport.JW_RL_NKLoadPortInfo);
			AssertNoErrors(transport.JW_RL_NKDiscPortInfo);
		}

		public void TestJW_RL_NKDiscPort()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NLAMS";

			Transport transport = shipment.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "AUBNE";
			transport.JW_OA_DepartureLocation = Factory.New<OrgHeader>().MainAddress.PK;
			AssertHasError(transport.JW_RL_NKDiscPortInfo, "A routing leg cannot discharge at the shipment's origin.");

			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			shipment.JS_RL_NKOrigin = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_RL_NKLoadPort = "AUSYD";
			AssertNoErrors(transport.JW_RL_NKDiscPortInfo);

			transport.JW_TransportMode = Core.Constants.TransportModes.Rail;
			shipment.JS_RL_NKOrigin = "AUBNE";
			transport.JW_RL_NKDiscPort = "AUBNE";
			transport.JW_RL_NKLoadPort = "AUBNE";
			AssertNoErrors(transport.JW_RL_NKDiscPortInfo);

			transport.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			shipment.JS_RL_NKOrigin = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_RL_NKLoadPort = "AUSYD";
			AssertNoErrors(transport.JW_RL_NKDiscPortInfo);

			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUBNE";
			transport.JW_RL_NKDiscPort = "AUBNE";
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_OA_ArrivalLocation = Factory.New<OrgHeader>().MainAddress.PK;
			AssertNoErrors(transport.JW_RL_NKDiscPortInfo);

			transport.JW_OA_ArrivalLocation = ZGuid.Empty;
			AssertHasError(transport.JW_RL_NKDiscPortInfo, "The Load and Discharge cannot be the same.");

			transport.JW_RL_NKDiscPort = "GBLON";
			AssertNoErrors(transport.JW_RL_NKDiscPortInfo);
		}
	}
}
