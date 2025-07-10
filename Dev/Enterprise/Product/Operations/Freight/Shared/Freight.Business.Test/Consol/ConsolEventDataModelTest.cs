using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	public class ConsolEventDataModelTest : TestCaseWithFactory
	{
		#region Origin

		public void TestOriginGetter_ReturnOriginPortFromconsol()
		{
			var consol = GetConsolInstance();
			consol.JK_RL_NKLoadPort = "UAIEV";

			var model = new ConsolEventDataModel(consol);

			AssertEquals("Property value", "UAIEV", model.Origin);
		}

		#endregion

		#region Destination

		public void TestDestinationGetter_ReturnDestinationPortFromconsol()
		{
			var consol = GetConsolInstance();
			consol.JK_RL_NKDischargePort = "UAIEV";

			var model = new ConsolEventDataModel(consol);
			AssertEquals("Property value", "UAIEV", model.Destination);
		}

		#endregion

		#region FirstLeg

		public void TestFirstLegGetter_SomeLegsExist_ReturnModelForconsolFirstLeg()
		{
			var consol = GetConsolInstance();
			var defaultTransport = consol.Transports[0];

			var transport1 = consol.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = consol.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = consol.Transports.New(from: "USNYC", to: "UAIEV");

			defaultTransport.Delete();

			var model = new ConsolEventDataModel(consol);
			AssertEquals("Wrapped transport", transport1, model.FirstLeg.Parent_DebugOnly);
		}

		public void TestFirstSeaLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 2, Constants.TransportModes.Sea);
			var model = new ConsolEventDataModel(consol);
			AssertNull("No Sea Leg Exists, Null expected", model.FirstSeaLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Sea);
			model = new ConsolEventDataModel(consol);
			AssertEquals("First transport should be our custom transport", transport, model.FirstSeaLeg.Parent_DebugOnly);
		}

		public void TestFirstAirLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 2, Constants.TransportModes.Air);
			var model = new ConsolEventDataModel(consol);
			AssertNull("No Air Leg Exists, Null expected", model.FirstAirLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = Constants.TransportModes.Air;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Air);
			model = new ConsolEventDataModel(consol);
			AssertEquals("First transport should be our custom transport", transport, model.FirstAirLeg.Parent_DebugOnly);
		}

		public void TestFirstRoadLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 2, Constants.TransportModes.Road);
			var model = new ConsolEventDataModel(consol);
			AssertNull("No Road Leg Exists, Null expected", model.FirstRoadLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = Constants.TransportModes.Road;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Road);
			model = new ConsolEventDataModel(consol);
			AssertEquals("First transport should be our custom transport", transport, model.FirstRoadLeg.Parent_DebugOnly);
		}

		public void TestFirstRailLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 2, Constants.TransportModes.Rail);
			var model = new ConsolEventDataModel(consol);
			AssertNull("No Rail Leg Exists, Null expected", model.FirstRailLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = Constants.TransportModes.Rail;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Rail);
			model = new ConsolEventDataModel(consol);
			AssertEquals("First transport should be our custom transport", transport, model.FirstRailLeg.Parent_DebugOnly);
		}

		#endregion

		#region SecondLeg

		public void TestSecondLegGetter_MoreThanTwoLegsExist_ReturnModelForconsolSecondLeg()
		{
			var consol = GetConsolInstance();
			var defaultTransport = consol.Transports[0];

			var transport1 = consol.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = consol.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = consol.Transports.New(from: "USNYC", to: "UAIEV");

			defaultTransport.Delete();

			var model = new ConsolEventDataModel(consol);
			AssertEquals("Wrapped transport", transport2, model.SecondLeg.Parent_DebugOnly);
		}

		public void TestSecondSeaLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 2, Constants.TransportModes.Sea);
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Sea);
			var model = new ConsolEventDataModel(consol);
			AssertNull("Only One Sea Leg Exists, Null expected", model.SecondSeaLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Sea);
			model = new ConsolEventDataModel(consol);
			AssertEquals("Second transport should be our custom transport", transport, model.SecondSeaLeg.Parent_DebugOnly);
		}

		public void TestSecondAirLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 2, Constants.TransportModes.Air);
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Air);
			var model = new ConsolEventDataModel(consol);
			AssertNull("Only One Air Leg Exists, Null expected", model.SecondAirLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = Constants.TransportModes.Air;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Air);
			model = new ConsolEventDataModel(consol);
			AssertEquals("Second transport should be our custom transport", transport, model.SecondAirLeg.Parent_DebugOnly);
		}

		public void TestSecondRoadLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 2, Constants.TransportModes.Road);
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Road);
			var model = new ConsolEventDataModel(consol);
			AssertNull("Only One Road Leg Exists, Null expected", model.SecondRoadLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = Constants.TransportModes.Road;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Road);
			model = new ConsolEventDataModel(consol);
			AssertEquals("Second transport should be our custom transport", transport, model.SecondRoadLeg.Parent_DebugOnly);
		}

		public void TestSecondRailLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 2, Constants.TransportModes.Rail);
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Rail);
			var model = new ConsolEventDataModel(consol);
			AssertNull("Only One Rail Leg Exists, Null expected", model.SecondRailLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = Constants.TransportModes.Rail;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Rail);
			model = new ConsolEventDataModel(consol);
			AssertEquals("Second transport should be our custom transport", transport, model.SecondRailLeg.Parent_DebugOnly);
		}

		#endregion

		#region ThirdLeg

		public void TestThirdLegGetter_MoreThanThreeLegsExist_ReturnModelForconsolThirdLeg()
		{
			var consol = GetConsolInstance();
			var defaultTransport = consol.Transports[0];

			var transport1 = consol.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = consol.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = consol.Transports.New(from: "USNYC", to: "UAIEV");
			var transport4 = consol.Transports.New(from: "UAIEV", to: "AUMEL");

			defaultTransport.Delete();

			var model = new ConsolEventDataModel(consol);
			AssertEquals("Wrapped transport", transport3, model.ThirdLeg.Parent_DebugOnly);
		}

		public void TestThirdSeaLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 5, Constants.TransportModes.Sea);
			PopulateConsolWithTransportsOfType(consol, 2, Constants.TransportModes.Sea);
			var model = new ConsolEventDataModel(consol);
			AssertNull("Only two Sea legs exist, null expected", model.ThirdSeaLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Sea);
			model = new ConsolEventDataModel(consol);
			AssertEquals("Third transport should be our custom transport", transport, model.ThirdSeaLeg.Parent_DebugOnly);
		}

		public void TestThirdAirLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 5, Constants.TransportModes.Air);
			PopulateConsolWithTransportsOfType(consol, 2, Constants.TransportModes.Air);
			var model = new ConsolEventDataModel(consol);
			AssertNull("Only two Air legs exist, null expected", model.ThirdAirLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = Constants.TransportModes.Air;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Air);
			model = new ConsolEventDataModel(consol);
			AssertEquals("Third transport should be our custom transport", transport, model.ThirdAirLeg.Parent_DebugOnly);
		}

		public void TestThirdRoadLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 5, Constants.TransportModes.Road);
			PopulateConsolWithTransportsOfType(consol, 2, Constants.TransportModes.Road);
			var model = new ConsolEventDataModel(consol);
			AssertNull("Only two Road legs exist, null expected", model.ThirdRoadLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = Constants.TransportModes.Road;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Road);
			model = new ConsolEventDataModel(consol);
			AssertEquals("Third transport should be our custom transport", transport, model.ThirdRoadLeg.Parent_DebugOnly);
		}

		public void TestThirdRailLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 5, Constants.TransportModes.Rail);
			PopulateConsolWithTransportsOfType(consol, 2, Constants.TransportModes.Rail);
			var model = new ConsolEventDataModel(consol);
			AssertNull("Only two Rail legs exist, null expected", model.ThirdRailLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = Constants.TransportModes.Rail;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Rail);
			model = new ConsolEventDataModel(consol);
			AssertEquals("Third transport should be our custom transport", transport, model.ThirdRailLeg.Parent_DebugOnly);
		}

		#endregion

		#region FourthLeg

		public void TestFourthLegGetter_MoreThanFourLegsExist_ReturnModelForconsolFourthLeg()
		{
			var consol = GetConsolInstance();
			var defaultTransport = consol.Transports[0];

			var transport1 = consol.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = consol.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = consol.Transports.New(from: "USNYC", to: "UAIEV");
			var transport4 = consol.Transports.New(from: "UAIEV", to: "AUMEL");
			var transport5 = consol.Transports.New(from: "AUMEL", to: "NZAKL");

			defaultTransport.Delete();

			var model = new ConsolEventDataModel(consol);
			AssertEquals("Wrapped transport", transport4, model.FourthLeg.Parent_DebugOnly);
		}

		public void TestFourthSeaLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 5, Constants.TransportModes.Sea);
			PopulateConsolWithTransportsOfType(consol, 3, Constants.TransportModes.Sea);
			var model = new ConsolEventDataModel(consol);
			AssertNull("Only three Sea legs exist, null expected", model.FourthSeaLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Sea);
			model = new ConsolEventDataModel(consol);
			AssertEquals("Fourth transport should be our custom transport", transport, model.FourthSeaLeg.Parent_DebugOnly);
		}

		public void TestFourthAirLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 5, Constants.TransportModes.Air);
			PopulateConsolWithTransportsOfType(consol, 3, Constants.TransportModes.Air);
			var model = new ConsolEventDataModel(consol);
			AssertNull("Only three Air legs exist, null expected", model.FourthAirLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = Constants.TransportModes.Air;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Air);
			model = new ConsolEventDataModel(consol);
			AssertEquals("Fourth transport should be our custom transport", transport, model.FourthAirLeg.Parent_DebugOnly);
		}

		public void TestFourthRoadLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 5, Constants.TransportModes.Road);
			PopulateConsolWithTransportsOfType(consol, 3, Constants.TransportModes.Road);
			var model = new ConsolEventDataModel(consol);
			AssertNull("Only three Road legs exist, null expected", model.FourthRoadLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = Constants.TransportModes.Road;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Road);
			model = new ConsolEventDataModel(consol);
			AssertEquals("Fourth transport should be our custom transport", transport, model.FourthRoadLeg.Parent_DebugOnly);
		}

		public void TestFourthRailLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 5, Constants.TransportModes.Rail);
			PopulateConsolWithTransportsOfType(consol, 3, Constants.TransportModes.Rail);
			var model = new ConsolEventDataModel(consol);
			AssertNull("Only three Sea legs exist, null expected", model.FourthRailLeg);

			var transport = consol.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = Constants.TransportModes.Rail;
			PopulateConsolWithTransportsOfType(consol, 1, Constants.TransportModes.Rail);
			model = new ConsolEventDataModel(consol);
			AssertEquals("Fourth transport should be our custom transport", transport, model.FourthRailLeg.Parent_DebugOnly);
		}

		#endregion

		#region LastLeg

		public void TestLastLegGetter_SomeLegsExist_ReturnModelForconsolLastLeg()
		{
			var consol = GetConsolInstance();
			var defaultTransport = consol.Transports[0];

			var transport1 = consol.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = consol.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = consol.Transports.New(from: "USNYC", to: "UAIEV");

			defaultTransport.Delete();

			var model = new ConsolEventDataModel(consol);
			AssertEquals("Wrapped transport", transport3, model.LastLeg.Parent_DebugOnly);
		}

		public void TestLastSeaLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 5, Constants.TransportModes.Sea);
			var model = new ConsolEventDataModel(consol);
			AssertNull("No Sea legs exist, null expected", model.LastSeaLeg);

			var transport1 = consol.Transports.New(from: "AUSYD", to: "HKHKG");
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			model = new ConsolEventDataModel(consol);
			AssertEquals("Last transport should be our custom transport", transport1, model.LastSeaLeg.Parent_DebugOnly);

			var transport2 = consol.Transports.New(from: "HKHKG", to: "UAIEV");
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			model = new ConsolEventDataModel(consol);
			AssertEquals("Last transport should be our updated transport", transport2, model.LastSeaLeg.Parent_DebugOnly);
		}

		public void TestLastAirLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 5, Constants.TransportModes.Air);
			var model = new ConsolEventDataModel(consol);
			AssertNull("No Air legs exist, null expected", model.LastAirLeg);

			var transport1 = consol.Transports.New(from: "AUSYD", to: "HKHKG");
			transport1.JW_TransportMode = Constants.TransportModes.Air;
			model = new ConsolEventDataModel(consol);
			AssertEquals("Last transport should be our custom transport", transport1, model.LastAirLeg.Parent_DebugOnly);

			var transport2 = consol.Transports.New(from: "HKHKG", to: "UAIEV");
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			model = new ConsolEventDataModel(consol);
			AssertEquals("Last transport should be our updated transport", transport2, model.LastAirLeg.Parent_DebugOnly);
		}

		public void TestLastRoadLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 5, Constants.TransportModes.Road);
			var model = new ConsolEventDataModel(consol);
			AssertNull("No Road legs exist, null expected", model.LastRoadLeg);

			var transport1 = consol.Transports.New(from: "AUSYD", to: "AUADE");
			transport1.JW_TransportMode = Constants.TransportModes.Road;
			model = new ConsolEventDataModel(consol);
			AssertEquals("Last transport should be our custom transport", transport1, model.LastRoadLeg.Parent_DebugOnly);

			var transport2 = consol.Transports.New(from: "AUADE", to: "AUPER");
			transport2.JW_TransportMode = Constants.TransportModes.Road;
			model = new ConsolEventDataModel(consol);
			AssertEquals("Last transport should be our updated transport", transport2, model.LastRoadLeg.Parent_DebugOnly);
		}

		public void TestLastRailLeg()
		{
			var consol = PopulateConsolWithTransports(GetConsolInstance(), 5, Constants.TransportModes.Rail);
			var model = new ConsolEventDataModel(consol);
			AssertNull("No Rail legs exist, null expected", model.LastRailLeg);

			var transport1 = consol.Transports.New(from: "AUSYD", to: "AUADE");
			transport1.JW_TransportMode = Constants.TransportModes.Rail;
			model = new ConsolEventDataModel(consol);
			AssertEquals("Last transport should be our custom transport", transport1, model.LastRailLeg.Parent_DebugOnly);

			var transport2 = consol.Transports.New(from: "AUADE", to: "AUPER");
			transport2.JW_TransportMode = Constants.TransportModes.Rail;
			model = new ConsolEventDataModel(consol);
			AssertEquals("Last transport should be our updated transport", transport2, model.LastRailLeg.Parent_DebugOnly);
		}

		#endregion

		#region IsGateway

		public void TestIsGateway_IsGateway()
		{
			var consol = GetConsolInstance();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			Assert("Precondition: Consol is a gateway consol.", consol.IsGatewayConsol);

			var model = new ConsolEventDataModel(consol);

			Assert("Property value", model.IsGateway);
		}

		public void TestIsGateway_IsNonGateway()
		{
			var consol = GetConsolInstance();
			consol.JK_AgentType = Constants.AgentType.Direct;
			Assert("Precondition: Consol is not a gateway consol.", !consol.IsGatewayConsol);

			var model = new ConsolEventDataModel(consol);

			Assert("Property value", !model.IsGateway);
		}

		#endregion

		#region SetTransports

		CommonConsol PopulateConsolWithTransports(CommonConsol consol, int maxOfEachType, string avoidMode)
		{
			consol.Transports.RemoveAll();
			if (avoidMode != Constants.TransportModes.Air)
			{
				PopulateConsolWithTransportsOfType(consol, maxOfEachType, Constants.TransportModes.Air);
			}
			if (avoidMode != Constants.TransportModes.Sea)
			{
				PopulateConsolWithTransportsOfType(consol, maxOfEachType, Constants.TransportModes.Sea);
			}
			if (avoidMode != Constants.TransportModes.Road)
			{
				PopulateConsolWithTransportsOfType(consol, maxOfEachType, Constants.TransportModes.Road);
			}
			if (avoidMode != Constants.TransportModes.Rail)
			{
				PopulateConsolWithTransportsOfType(consol, maxOfEachType, Constants.TransportModes.Rail);
			}
			return consol;
		}

		CommonConsol PopulateConsolWithTransportsOfType(CommonConsol consol, int maxOfEachType, string transportMode)
		{
			for (int i = 0; i < maxOfEachType; i++)
			{
				var transport = consol.Transports.New(from: "AUSYD", to: "AUMEL");
				transport.JW_TransportMode = transportMode;
			}
			return consol;
		}

		#endregion

		protected virtual CommonConsol GetConsolInstance()
		{
			return Factory.New<CommonConsol>();
		}
	}
}
