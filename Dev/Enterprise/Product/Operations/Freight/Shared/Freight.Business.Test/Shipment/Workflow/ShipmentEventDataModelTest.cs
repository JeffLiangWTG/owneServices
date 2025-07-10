using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business.Extensions;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	public class ShipmentEventDataModelTest : TestCaseWithFactory
	{
		#region Origin

		public void TestOriginGetter_ReturnOriginPortFromShipment()
		{
			var shipment = GetShipmentInstance();
			shipment.JS_RL_NKOrigin = "UAIEV";

			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Property value", "UAIEV", model.Origin);
		}

		#endregion

		#region Destination

		public void TestDestinationGetter_ReturnDestinationPortFromShipment()
		{
			var shipment = GetShipmentInstance();
			shipment.JS_RL_NKDestination = "UAIEV";

			var model = new ShipmentEventDataModel<CommonShipment>(shipment);
			AssertEquals("Property value", "UAIEV", model.Destination);
		}

		#endregion

		#region FirstLeg

		public void TestFirstLegGetter_SomeLegsExist_ReturnModelForShipmentFirstLeg()
		{
			var shipment = GetShipmentInstance();
			var transport1 = shipment.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = shipment.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = shipment.Transports.New(from: "USNYC", to: "UAIEV");

			var model = new ShipmentEventDataModel<CommonShipment>(shipment);
			AssertEquals("Wrapped transport", transport1, model.FirstLeg.Parent_DebugOnly);
		}

		public void TestFirstLegGetter_NoLegsExist_ReturnNull()
		{
			var shipment = GetShipmentInstance();
			shipment.Transports.RemoveAndDeleteAll();

			var model = new ShipmentEventDataModel<CommonShipment>(shipment);
			AssertEquals("Model", null, model.FirstLeg);
		}

		public void TestFirstSeaLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(2, TransportModes.Air);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("No Sea Leg Exists, Null expected", model.FirstSeaLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = TransportModes.Sea;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("First transport should be our custom transport", transport, model.FirstSeaLeg.Parent_DebugOnly);
		}

		public void TestFirstAirLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(2, TransportModes.Sea);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("No Air Leg Exists, Null expected", model.FirstAirLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = TransportModes.Air;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("First transport should be our custom transport", transport, model.FirstAirLeg.Parent_DebugOnly);
		}

		public void TestFirstRoadLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(2, TransportModes.Sea);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("No Road Leg Exists, Null expected", model.FirstRoadLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = TransportModes.Road;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("First transport should be our custom transport", transport, model.FirstRoadLeg.Parent_DebugOnly);
		}

		public void TestFirstRailLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(2, TransportModes.Sea);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("No Rail Leg Exists, Null expected", model.FirstRailLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = TransportModes.Rail;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("First transport should be our custom transport", transport, model.FirstRailLeg.Parent_DebugOnly);
		}

		#endregion

		#region SecondLeg

		public void TestSecondLegGetter_MoreThanTwoLegsExist_ReturnModelForShipmentSecondLeg()
		{
			var shipment = GetShipmentInstance();
			var transport1 = shipment.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = shipment.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = shipment.Transports.New(from: "USNYC", to: "UAIEV");

			var model = new ShipmentEventDataModel<CommonShipment>(shipment);
			AssertEquals("Wrapped transport", transport2, model.SecondLeg.Parent_DebugOnly);
		}

		public void TestSecondLegGetter_LessThanTwoLegsExist_ReturnNull()
		{
			var shipment = GetShipmentInstance();
			shipment.Transports.New(from: "AUSYD", to: "USLAX");

			var model = new ShipmentEventDataModel<CommonShipment>(shipment);
			AssertEquals("Wrapped transport", null, model.SecondLeg);
		}

		public void TestSecondSeaLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(1, TransportModes.Sea);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("Only One Sea Leg Exists, Null expected", model.SecondSeaLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = TransportModes.Sea;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Second transport should be our custom transport", transport, model.SecondSeaLeg.Parent_DebugOnly);
		}

		public void TestSecondAirLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(1, TransportModes.Air);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("Only One Air Leg Exists, Null expected", model.SecondAirLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = TransportModes.Air;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Second transport should be our custom transport", transport, model.SecondAirLeg.Parent_DebugOnly);
		}

		public void TestSecondRoadLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(1, TransportModes.Road);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("Only One Road Leg Exists, Null expected", model.SecondRoadLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = TransportModes.Road;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Second transport should be our custom transport", transport, model.SecondRoadLeg.Parent_DebugOnly);
		}

		public void TestSecondRailLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(1, TransportModes.Rail);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("Only One Rail Leg Exists, Null expected", model.SecondRailLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = TransportModes.Rail;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Second transport should be our custom transport", transport, model.SecondRailLeg.Parent_DebugOnly);
		}

		#endregion

		#region ThirdLeg

		public void TestThirdLegGetter_MoreThanThreeLegsExist_ReturnModelForShipmentThirdLeg()
		{
			var shipment = GetShipmentInstance();
			var transport1 = shipment.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = shipment.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = shipment.Transports.New(from: "USNYC", to: "UAIEV");
			var transport4 = shipment.Transports.New(from: "UAIEV", to: "AUMEL");

			var model = new ShipmentEventDataModel<CommonShipment>(shipment);
			AssertEquals("Wrapped transport", transport3, model.ThirdLeg.Parent_DebugOnly);
		}

		public void TestThirdLegGetter_LessThanThreeLegsExist_ReturnNull()
		{
			var shipment = GetShipmentInstance();
			shipment.Transports.New(from: "AUSYD", to: "USLAX");
			shipment.Transports.New(from: "USLAX", to: "USNYC");

			var model = new ShipmentEventDataModel<CommonShipment>(shipment);
			AssertEquals("Wrapped transport", null, model.ThirdLeg);
		}

		public void TestThirdSeaLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(2, TransportModes.Sea);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("Only two Sea legs exist, null expected", model.ThirdSeaLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = TransportModes.Sea;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Third transport should be our custom transport", transport, model.ThirdSeaLeg.Parent_DebugOnly);
		}

		public void TestThirdAirLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(2, TransportModes.Air);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("Only two Air legs exist, null expected", model.ThirdAirLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = TransportModes.Air;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Third transport should be our custom transport", transport, model.ThirdAirLeg.Parent_DebugOnly);
		}

		public void TestThirdRoadLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(2, TransportModes.Road);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("Only two Road legs exist, null expected", model.ThirdRoadLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = TransportModes.Road;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Third transport should be our custom transport", transport, model.ThirdRoadLeg.Parent_DebugOnly);
		}

		public void TestThirdRailLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(2, TransportModes.Rail);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("Only two Rail legs exist, null expected", model.ThirdRailLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = TransportModes.Rail;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Third transport should be our custom transport", transport, model.ThirdRailLeg.Parent_DebugOnly);
		}

		#endregion

		#region FourthLeg

		public void TestFourthLegGetter_MoreThanFourLegsExist_ReturnModelForShipmentFourthLeg()
		{
			var shipment = GetShipmentInstance();
			var transport1 = shipment.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = shipment.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = shipment.Transports.New(from: "USNYC", to: "UAIEV");
			var transport4 = shipment.Transports.New(from: "UAIEV", to: "AUMEL");
			var transport5 = shipment.Transports.New(from: "AUMEL", to: "NZAKL");

			var model = new ShipmentEventDataModel<CommonShipment>(shipment);
			AssertEquals("Wrapped transport", transport4, model.FourthLeg.Parent_DebugOnly);
		}

		public void TestFourthLegGetter_LessThanFourLegsExist_ReturnNull()
		{
			var shipment = GetShipmentInstance();
			shipment.Transports.New(from: "AUSYD", to: "USLAX");
			shipment.Transports.New(from: "USLAX", to: "USNYC");
			shipment.Transports.New(from: "USNYC", to: "UAIEV");

			var model = new ShipmentEventDataModel<CommonShipment>(shipment);
			AssertEquals("Wrapped transport", null, model.FourthLeg);
		}

		public void TestFourthSeaLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(3, TransportModes.Sea);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("Only three Sea legs exist, null expected", model.FourthSeaLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = TransportModes.Sea;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Fourth transport should be our custom transport", transport, model.FourthSeaLeg.Parent_DebugOnly);
		}

		public void TestFourthAirLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(3, TransportModes.Air);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("Only three Air legs exist, null expected", model.FourthAirLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "HKHKG");
			transport.JW_TransportMode = TransportModes.Air;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Fourth transport should be our custom transport", transport, model.FourthAirLeg.Parent_DebugOnly);
		}

		public void TestFourthRoadLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(3, TransportModes.Road);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("Only three Road legs exist, null expected", model.FourthRoadLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = TransportModes.Road;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Fourth transport should be our custom transport", transport, model.FourthRoadLeg.Parent_DebugOnly);
		}

		public void TestFourthRailLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(3, TransportModes.Rail);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);
			AssertNull("Only three Sea legs exist, null expected", model.FourthRailLeg);

			var transport = shipment.Transports.New(from: "AUSYD", to: "AUBNE");
			transport.JW_TransportMode = TransportModes.Rail;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Fourth transport should be our custom transport", transport, model.FourthRailLeg.Parent_DebugOnly);
		}

		#endregion

		#region LastLeg

		public void TestLastLegGetter_SomeLegsExist_ReturnModelForShipmentLastLeg()
		{
			var shipment = GetShipmentInstance();
			var transport1 = shipment.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = shipment.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = shipment.Transports.New(from: "USNYC", to: "UAIEV");

			var model = new ShipmentEventDataModel<CommonShipment>(shipment);
			AssertEquals("Wrapped transport", transport3, model.LastLeg.Parent_DebugOnly);
		}

		public void TestLastLegGetter_NoLegsExist_ReturnNull()
		{
			var shipment = GetShipmentInstance();
			shipment.Transports.RemoveAndDeleteAll();

			var model = new ShipmentEventDataModel<CommonShipment>(shipment);
			AssertEquals("Model", null, model.LastLeg);
		}

		public void TestLastSeaLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(1, TransportModes.Air);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("No Sea legs exist, null expected", model.LastSeaLeg);

			var transport1 = shipment.Transports.New(from: "AUSYD", to: "HKHKG");
			transport1.JW_TransportMode = TransportModes.Sea;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Last transport should be our custom transport", transport1, model.LastSeaLeg.Parent_DebugOnly);

			var transport2 = shipment.Transports.New(from: "HKHKG", to: "UAIEV");
			transport2.JW_TransportMode = TransportModes.Sea;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Last transport should be our updated transport", transport2, model.LastSeaLeg.Parent_DebugOnly);
		}

		public void TestLastAirLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(1, TransportModes.Sea);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("No Air legs exist, null expected", model.LastAirLeg);

			var transport1 = shipment.Transports.New(from: "AUSYD", to: "HKHKG");
			transport1.JW_TransportMode = TransportModes.Air;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Last transport should be our custom transport", transport1, model.LastAirLeg.Parent_DebugOnly);

			var transport2 = shipment.Transports.New(from: "HKHKG", to: "UAIEV");
			transport2.JW_TransportMode = TransportModes.Air;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Last transport should be our updated transport", transport2, model.LastAirLeg.Parent_DebugOnly);
		}

		public void TestLastRoadLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(1, TransportModes.Sea);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("No Road legs exist, null expected", model.LastRoadLeg);

			var transport1 = shipment.Transports.New(from: "AUSYD", to: "AUADE");
			transport1.JW_TransportMode = TransportModes.Road;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Last transport should be our custom transport", transport1, model.LastRoadLeg.Parent_DebugOnly);

			var transport2 = shipment.Transports.New(from: "AUADE", to: "AUPER");
			transport2.JW_TransportMode = TransportModes.Road;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Last transport should be our updated transport", transport2, model.LastRoadLeg.Parent_DebugOnly);
		}

		public void TestLastRailLeg()
		{
			var shipment = GetShipmentInstanceWithTransportsOfType(1, TransportModes.Sea);
			var model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertNull("No Rail legs exist, null expected", model.LastRailLeg);

			var transport1 = shipment.Transports.New(from: "AUSYD", to: "AUADE");
			transport1.JW_TransportMode = TransportModes.Rail;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Last transport should be our custom transport", transport1, model.LastRailLeg.Parent_DebugOnly);

			var transport2 = shipment.Transports.New(from: "AUADE", to: "AUPER");
			transport2.JW_TransportMode = TransportModes.Rail;
			model = new ShipmentEventDataModel<CommonShipment>(shipment);

			AssertEquals("Last transport should be our updated transport", transport2, model.LastRailLeg.Parent_DebugOnly);
		}

		#endregion

		#region SetTransports

		CommonShipment GetShipmentInstanceWithTransportsOfType(int maxOfEachType, string transportMode)
		{
			var shipment = GetShipmentInstance();

			for (var i = 0; i < maxOfEachType; i++)
			{
				var transport = shipment.Transports.New(from: "AUSYD", to: "AUMEL");
				transport.JW_TransportMode = transportMode;
			}
			return shipment;
		}

		#endregion

		protected virtual CommonShipment GetShipmentInstance()
		{
			return Factory.New<CommonShipment>();
		}
	}
}
