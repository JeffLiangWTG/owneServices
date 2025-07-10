using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ALPOHelperTest : TestCaseWithFactory
	{
		public void TestGetShipmentTransport()
		{
			AssertNoExceptionThrown(() => ALPOHelper.GetShipmentTransport(null));

			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertNull("Transport should be null.", ALPOHelper.GetShipmentTransport(shipment));

			Transport transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Air;
			AssertNull("Transport should be null.", ALPOHelper.GetShipmentTransport(shipment));

			Transport transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_TransportType = Constants.TransportPlanningType.Other;
			AssertNull("Transport should be null.", ALPOHelper.GetShipmentTransport(shipment));

			transport1.JW_RL_NKLoadPort = "DEHAM";
			AssertEquals("Transport 1 from DEHAM should be found.", transport1, ALPOHelper.GetShipmentTransport(shipment));

			transport2.JW_RL_NKLoadPort = "DEBRE";
			AssertEquals("Transport 1 from DEHAM should be found.", transport1, ALPOHelper.GetShipmentTransport(shipment));

			transport2.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			AssertEquals("Transport 2 from DEBRE of MAIN type should be found.", transport2, ALPOHelper.GetShipmentTransport(shipment));
		}

		public void TestGetShipmentTransport_InlandWaterwayTransport()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertNull("Transport should be null.", ALPOHelper.GetShipmentTransport(shipment));

			Transport transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			transport1.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
			transport1.JW_RL_NKLoadPort = "DEHAM";

			AssertEquals("Transport 1 Inland Waterway should be found.", transport1, ALPOHelper.GetShipmentTransport(shipment));

			Transport transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport2.JW_RL_NKLoadPort = "DEBRE";
			AssertEquals("Transport 2 Main Sea leg should be found.", transport2, ALPOHelper.GetShipmentTransport(shipment));
		}

		public void TestGetShipmentTransport_TransportComparerWillNotCrashInInfiniteLoops()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			Transport transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = "DEHAM";

			Transport transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "DEHAM";

			AssertEquals("Precondition", true, ALPOHelper.IsALPOPort(transport1.LoadPort));
			AssertEquals("Precondition", true, ALPOHelper.IsALPOPort(transport2.LoadPort));

			AssertNoExceptionThrown(() => ALPOHelper.GetShipmentTransport(shipment));

			transport1.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport2.JW_TransportType = Constants.TransportPlanningType.MainVessel;

			AssertNoExceptionThrown(() => ALPOHelper.GetShipmentTransport(shipment));
		}

		public void TestGetConsol()
		{
			AssertNoExceptionThrown(() => ALPOHelper.GetConsol(null));

			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertNull("Consol should be null.", ALPOHelper.GetConsol(shipment));

			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertNull("Consol should be null.", ALPOHelper.GetConsol(shipment));

			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertNull("Consol should be null.", ALPOHelper.GetConsol(shipment));

			consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertNull("Consol should be null.", ALPOHelper.GetConsol(shipment));

			consol.JK_RL_NKLoadPort = "DEHAM";
			AssertEquals("Consol from DEHAM should be found.", consol, ALPOHelper.GetConsol(shipment));
		}

		public void TestGetALPOPort()
		{
			AssertNoExceptionThrown(() => ALPOHelper.GetALPOPort(null));

			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertNull("ALPO Port should be null.", ALPOHelper.GetALPOPort(shipment));

			shipment.JS_RL_NKOrigin = "DEHAM";
			AssertEquals("ALPO Port DEHAM should be found from shipment.", "HAM", ALPOHelper.GetALPOPort(shipment).RL_IATA);

			Transport transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertNull("ALPO Port should be null.", ALPOHelper.GetALPOPort(shipment));

			transport.JW_RL_NKLoadPort = "DEBRE";
			AssertEquals("ALPO Port DEBRE should be found from transport.", "BRE", ALPOHelper.GetALPOPort(shipment).RL_IATA);
		}

		public void TestGetOrigin()
		{
			AssertNoExceptionThrown(() => ALPOHelper.GetOrigin(null));

			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertNull("Origin should be null.", ALPOHelper.GetOrigin(shipment));

			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertEquals("Origin AUSYD should be found from shipment.", "SYD", ALPOHelper.GetOrigin(shipment).RL_IATA);

			Transport transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.Other;
			AssertEquals("Origin AUSYD should be found from shipment.", "SYD", ALPOHelper.GetOrigin(shipment).RL_IATA);

			transport.JW_RL_NKLoadPort = "AUMEL";
			AssertEquals("Origin AUSYD should be found from shipment.", "SYD", ALPOHelper.GetOrigin(shipment).RL_IATA);

			transport.JW_RL_NKLoadPort = "DEHAM";
			AssertEquals("Origin DEHAM should be found from transport.", "HAM", ALPOHelper.GetOrigin(shipment).RL_IATA);
		}

		public void TestGetDestination()
		{
			AssertNoExceptionThrown(() => ALPOHelper.GetDestination(null));

			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertNull("Destination should be null.", ALPOHelper.GetDestination(shipment));

			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("Destination AUSYD should be found from shipment.", "SYD", ALPOHelper.GetDestination(shipment).RL_IATA);

			Transport transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.Other;
			AssertEquals("Destination AUSYD should be found from shipment.", "SYD", ALPOHelper.GetDestination(shipment).RL_IATA);

			transport.JW_RL_NKDiscPort = "AUMEL";
			AssertEquals("Destination AUSYD should be found from shipment.", "SYD", ALPOHelper.GetDestination(shipment).RL_IATA);

			transport.JW_RL_NKDiscPort = "DEHAM";
			AssertEquals("Destination DEHAM should be found from transport.", "HAM", ALPOHelper.GetDestination(shipment).RL_IATA);
		}
	}
}
