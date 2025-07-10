using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE;

namespace Enterprise.Freight.Forwarding.Documents.DE.Testing
{
	class AdvancedLogisticsPortOrderHelperTest : TestCaseWithFactory
	{
		public void TestGetMainTransport()
		{
			AssertNoExceptionThrown(() => AdvancedLogisticsPortOrderHelper.GetMainTransport(null));

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertNull("Transport should be null.", AdvancedLogisticsPortOrderHelper.GetMainTransport(consol));

			Freight.Business.Transport transport1 = consol.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertNull("Transport should be null.", AdvancedLogisticsPortOrderHelper.GetMainTransport(consol));

			Freight.Business.Transport transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			AssertNull("Transport should be null.", AdvancedLogisticsPortOrderHelper.GetMainTransport(consol));

			transport1.JW_RL_NKLoadPort = "DEBRV";
			AssertEquals("Transport 1 from DEBRV should be found.", transport1, AdvancedLogisticsPortOrderHelper.GetMainTransport(consol));

			transport2.JW_RL_NKLoadPort = "DEBRE";
			AssertEquals("Transport 1 from DEHAM should be found.", transport1, AdvancedLogisticsPortOrderHelper.GetMainTransport(consol));

			transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			AssertEquals("Transport 2 from DEBRE of MAIN type should be found.", transport2, AdvancedLogisticsPortOrderHelper.GetMainTransport(consol));
		}

		public void TestGetALPOPort()
		{
			AssertNoExceptionThrown(() => AdvancedLogisticsPortOrderHelper.GetALPOPort(null));

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertNull("ALPO Port should be null.", AdvancedLogisticsPortOrderHelper.GetALPOPort(consol));

			consol.JK_RL_NKLoadPort = "DEBRV";
			AssertEquals("ALPO Port DEBRV should be found from consol.", "BRV", AdvancedLogisticsPortOrderHelper.GetALPOPort(consol).RL_IATA);

			Freight.Business.Transport transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			AssertNull("ALPO Port should be null.", AdvancedLogisticsPortOrderHelper.GetALPOPort(consol));

			transport.JW_RL_NKLoadPort = "DEBRE";
			AssertEquals("ALPO Port DEBRE should be found from transport.", "BRE", AdvancedLogisticsPortOrderHelper.GetALPOPort(consol).RL_IATA);

			transport.JW_RL_NKLoadPort = "AUSYD";
			AssertNull("ALPO Port should be null.", AdvancedLogisticsPortOrderHelper.GetALPOPort(consol));

			transport.JW_RL_NKLoadPort = "DEBRV";
			AssertEquals("ALPO Port DEBRV should be found from transport.", "BRV", AdvancedLogisticsPortOrderHelper.GetALPOPort(consol).RL_IATA);

			transport.JW_RL_NKLoadPort = "AUSYD";
			AssertNull("ALPO Port should be null.", AdvancedLogisticsPortOrderHelper.GetALPOPort(consol));

			transport.JW_RL_NKLoadPort = "DECUX";
			AssertEquals("ALPO Port DECUX should be found from transport.", "FCN", AdvancedLogisticsPortOrderHelper.GetALPOPort(consol).RL_IATA);

			transport.JW_RL_NKLoadPort = "AUSYD";
			AssertNull("ALPO Port should be null.", AdvancedLogisticsPortOrderHelper.GetALPOPort(consol));

			transport.JW_RL_NKLoadPort = "DEWVN";
			AssertEquals("ALPO Port DEWVN should be found from transport.", "WVN", AdvancedLogisticsPortOrderHelper.GetALPOPort(consol).RL_IATA);
		}

		public void TestGetOrigin()
		{
			AssertNoExceptionThrown(() => AdvancedLogisticsPortOrderHelper.GetOrigin(null));

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertNull("Origin should be null.", AdvancedLogisticsPortOrderHelper.GetOrigin(consol));

			consol.JK_RL_NKLoadPort = "AUSYD";
			AssertEquals("Origin AUSYD should be found from consol.", "SYD", AdvancedLogisticsPortOrderHelper.GetOrigin(consol).RL_IATA);

			Freight.Business.Transport transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			AssertEquals("Origin AUSYD should be found from consol.", "SYD", AdvancedLogisticsPortOrderHelper.GetOrigin(consol).RL_IATA);

			transport.JW_RL_NKLoadPort = "AUMEL";
			AssertEquals("Origin AUSYD should be found from consol.", "SYD", AdvancedLogisticsPortOrderHelper.GetOrigin(consol).RL_IATA);

			transport.JW_RL_NKLoadPort = "DEBRV";
			AssertEquals("Origin DEBRV should be found from transport.", "BRV", AdvancedLogisticsPortOrderHelper.GetOrigin(consol).RL_IATA);
		}

		public void TestGetDestination()
		{
			AssertNoExceptionThrown(() => AdvancedLogisticsPortOrderHelper.GetDestination(null));

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertNull("Destination should be null.", AdvancedLogisticsPortOrderHelper.GetDestination(consol));

			consol.JK_RL_NKDischargePort = "AUSYD";
			AssertEquals("Destination AUSYD should be found from consol.", "SYD", AdvancedLogisticsPortOrderHelper.GetDestination(consol).RL_IATA);

			Freight.Business.Transport transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			AssertEquals("Destination AUSYD should be found from consol.", "SYD", AdvancedLogisticsPortOrderHelper.GetDestination(consol).RL_IATA);

			transport.JW_RL_NKDiscPort = "AUMEL";
			AssertEquals("Destination AUSYD should be found from consol.", "SYD", AdvancedLogisticsPortOrderHelper.GetDestination(consol).RL_IATA);

			transport.JW_RL_NKDiscPort = "DEBRV";
			AssertEquals("Destination DEBRV should be found from transport.", "BRV", AdvancedLogisticsPortOrderHelper.GetDestination(consol).RL_IATA);
		}
	}
}
