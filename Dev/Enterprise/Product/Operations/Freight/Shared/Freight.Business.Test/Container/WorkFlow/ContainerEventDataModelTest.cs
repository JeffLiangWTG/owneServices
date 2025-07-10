using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Business.Extensions;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ContainerEventDataModelTest : TestCaseWithFactory
	{
		#region Origin

		public void TestOriginGetter_ReturnOriginPortFromConsol()
		{
			Consol.JK_RL_NKLoadPort = "UAIEV";

			var model = new ContainerEventDataModel(Container);
			AssertEquals("Property value", "UAIEV", model.Origin);
		}

		#endregion

		#region Destination

		public void TestDestinationGetter_ReturnDestinationPortFromConsol()
		{
			Consol.JK_RL_NKDischargePort = "UAIEV";

			var model = new ContainerEventDataModel(Container);
			AssertEquals("Property value", "UAIEV", model.Destination);
		}

		#endregion

		#region AllLegs

		#region FirstLeg

		public void TestFirstLegGetter_SomeLegsExist_ReturnModelForConsolFirstLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var transport1 = Consol.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = Consol.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = Consol.Transports.New(from: "USNYC", to: "UAIEV");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", transport1, model.FirstLeg.Parent_DebugOnly);
		}

		#endregion

		#region SecondLeg

		public void TestSecondLegGetter_MoreThanTwoLegsExist_ReturnModelForConsolSecondLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var transport1 = Consol.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = Consol.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = Consol.Transports.New(from: "USNYC", to: "UAIEV");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", transport2, model.SecondLeg.Parent_DebugOnly);
		}

		public void TestSecondLegGetter_LessThanTwoLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New(from: "AUSYD", to: "USLAX");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", null, model.SecondLeg);
		}

		#endregion

		#region ThirdLeg

		public void TestThirdLegGetter_MoreThanThreeLegsExist_ReturnModelForConsolThirdLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var transport1 = Consol.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = Consol.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = Consol.Transports.New(from: "USNYC", to: "UAIEV");
			var transport4 = Consol.Transports.New(from: "UAIEV", to: "AUMEL");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", transport3, model.ThirdLeg.Parent_DebugOnly);
		}

		public void TestThirdLegGetter_LessThanThreeLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New(from: "AUSYD", to: "USLAX");
			Consol.Transports.New(from: "USLAX", to: "USNYC");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", null, model.ThirdLeg);
		}

		#endregion

		#region FourthLeg

		public void TestFourthLegGetter_MoreThanFourLegsExist_ReturnModelForConsolFourthLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var transport1 = Consol.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = Consol.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = Consol.Transports.New(from: "USNYC", to: "UAIEV");
			var transport4 = Consol.Transports.New(from: "UAIEV", to: "AUMEL");
			var transport5 = Consol.Transports.New(from: "AUMEL", to: "NZAKL");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", transport4, model.FourthLeg.Parent_DebugOnly);
		}

		public void TestFourthLegGetter_LessThanFourLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New(from: "AUSYD", to: "USLAX");
			Consol.Transports.New(from: "USLAX", to: "USNYC");
			Consol.Transports.New(from: "USNYC", to: "UAIEV");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", null, model.FourthLeg);
		}

		#endregion

		#region LastLeg

		public void TestLastLegGetter_SomeLegsExist_ReturnModelForConsolLastLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var transport1 = Consol.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = Consol.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = Consol.Transports.New(from: "USNYC", to: "UAIEV");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", transport3, model.LastLeg.Parent_DebugOnly);
		}

		#endregion

		#endregion

		#region SeaLegs

		#region FirstSeaLeg

		public void TestFirstSeaLegGetter_SomeLegsExist_ReturnModelForConsolFirstLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport1 = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var seaTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Sea);
			var seaTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Sea);
			var seaTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Sea);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(seaTransport1, model.FirstSeaLeg.Parent_DebugOnly);
		}

		#endregion

		#region SecondSeaLeg

		public void TestSecondSeaLegGetter_MoreThanTwoLegsExist_ReturnModelForConsolSecondLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport1 = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var seaTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Sea);
			var seaTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Sea);
			var seaTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Sea);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(seaTransport2, model.SecondSeaLeg.Parent_DebugOnly);
		}

		public void TestSecondSeaLegGetter_LessThanTwoLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Sea);

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(null, model.SecondSeaLeg);
		}

		#endregion

		#region ThirdSeaLeg

		public void TestThirdSeaLegGetter_MoreThanThreeLegsExist_ReturnModelForConsolThirdLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport1 = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var seaTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Sea);
			var seaTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Sea);
			var seaTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Sea);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(seaTransport3, model.ThirdSeaLeg.Parent_DebugOnly);
		}

		public void TestThirdSeaLegGetter_LessThanThreeLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Sea);
			Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(null, model.ThirdSeaLeg);
		}

		#endregion

		#region FourthSeaLeg

		public void TestFourthSeaLegGetter_MoreThanFourLegsExist_ReturnModelForConsolFourthLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport1 = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var seaTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Sea);
			var seaTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Sea);
			var seaTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Sea);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(seaTransport4, model.FourthSeaLeg.Parent_DebugOnly);
		}

		public void TestFourthSeaLegGetter_LessThanFourLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Sea);
			Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Sea);

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(null, model.FourthSeaLeg);
		}

		#endregion

		#region LastSeaLeg

		public void TestLastSeaLegGetter_SomeLegsExist_ReturnModelForConsolLastLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport1 = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var seaTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Sea);
			var seaTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Sea);
			var seaTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Sea);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(seaTransport4, model.LastSeaLeg.Parent_DebugOnly);
		}

		#endregion

		#endregion

		#region AirLegs

		#region FirstAirLeg

		public void TestFirstAirLegGetter_SomeLegsExist_ReturnModelForConsolFirstLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport1 = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var airTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Air);
			var airTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Air);
			var airTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Air);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(airTransport1, model.FirstAirLeg.Parent_DebugOnly);
		}

		#endregion

		#region SecondAirLeg

		public void TestSecondAirLegGetter_MoreThanTwoLegsExist_ReturnModelForConsolSecondLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport1 = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var airTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Air);
			var airTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Air);
			var airTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Air);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(airTransport2, model.SecondAirLeg.Parent_DebugOnly);
		}

		public void TestSecondAirLegGetter_LessThanTwoLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(null, model.SecondAirLeg);
		}

		#endregion

		#region ThirdAirLeg

		public void TestThirdAirLegGetter_MoreThanThreeLegsExist_ReturnModelForConsolThirdLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport1 = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var airTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Air);
			var airTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Air);
			var airTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Air);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(airTransport3, model.ThirdAirLeg.Parent_DebugOnly);
		}

		public void TestThirdAirLegGetter_LessThanThreeLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Air);

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(null, model.ThirdAirLeg);
		}

		#endregion

		#region FourthAirLeg

		public void TestFourthAirLegGetter_MoreThanFourLegsExist_ReturnModelForConsolFourthLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport1 = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var airTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Air);
			var airTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Air);
			var airTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Air);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(airTransport4, model.FourthAirLeg.Parent_DebugOnly);
		}

		public void TestFourthAirLegGetter_LessThanFourLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Air);
			Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Air);

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(null, model.FourthAirLeg);
		}

		#endregion

		#region LastAirLeg

		public void TestLastAirLegGetter_SomeLegsExist_ReturnModelForConsolLastLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport1 = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var airTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Air);
			var airTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Air);
			var airTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Air);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(airTransport4, model.LastAirLeg.Parent_DebugOnly);
		}

		#endregion

		#endregion

		#region RoadLegs

		#region FirstRoadLeg

		public void TestFirstRoadLegGetter_SomeLegsExist_ReturnModelForConsolFirstLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport1 = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var roadTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Road);
			var roadTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Road);
			var roadTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Road);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(roadTransport1, model.FirstRoadLeg.Parent_DebugOnly);
		}

		#endregion

		#region SecondRoadLeg

		public void TestSecondRoadLegGetter_MoreThanTwoLegsExist_ReturnModelForConsolSecondLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport1 = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var roadTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Road);
			var roadTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Road);
			var roadTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Road);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(roadTransport2, model.SecondRoadLeg.Parent_DebugOnly);
		}

		public void TestSecondRoadLegGetter_LessThanTwoLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Road);

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(null, model.SecondRoadLeg);
		}

		#endregion

		#region ThirdRoadLeg

		public void TestThirdRoadLegGetter_MoreThanThreeLegsExist_ReturnModelForConsolThirdLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport1 = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var roadTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Road);
			var roadTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Road);
			var roadTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Road);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(roadTransport3, model.ThirdRoadLeg.Parent_DebugOnly);
		}

		public void TestThirdRoadLegGetter_LessThanThreeLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Road);
			Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Road);

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(null, model.ThirdRoadLeg);
		}

		#endregion

		#region FourthRoadLeg

		public void TestFourthRoadLegGetter_MoreThanFourLegsExist_ReturnModelForConsolFourthLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport1 = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var roadTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Road);
			var roadTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Road);
			var roadTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Road);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(roadTransport4, model.FourthRoadLeg.Parent_DebugOnly);
		}

		public void TestFourthRoadLegGetter_LessThanFourLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Road);
			Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Road);
			Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(null, model.FourthRoadLeg);
		}

		#endregion

		#region LastRoadLeg

		public void TestLastRoadLegGetter_SomeLegsExist_ReturnModelForConsolLastLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport1 = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var roadTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Road);
			var roadTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Road);
			var roadTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Road);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(roadTransport4, model.LastRoadLeg.Parent_DebugOnly);
		}

		#endregion

		#endregion

		#region RailLegs

		#region FirstRailLeg

		public void TestFirstRailLegGetter_SomeLegsExist_ReturnModelForConsolFirstLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport1 = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var railTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Rail);
			var railTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Rail);
			var railTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Rail);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(railTransport1, model.FirstRailLeg.Parent_DebugOnly);
		}

		#endregion

		#region SecondRailLeg

		public void TestSecondRailLegGetter_MoreThanTwoLegsExist_ReturnModelForConsolSecondLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport1 = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var railTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Rail);
			var railTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Rail);
			var railTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Rail);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(railTransport2, model.SecondRailLeg.Parent_DebugOnly);
		}

		public void TestSecondRailLegGetter_LessThanTwoLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Rail);

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(null, model.SecondRailLeg);
		}

		#endregion

		#region ThirdRailLeg

		public void TestThirdRailLegGetter_MoreThanThreeLegsExist_ReturnModelForConsolThirdLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport1 = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var railTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Rail);
			var railTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Rail);
			var railTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Rail);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(railTransport3, model.ThirdRailLeg.Parent_DebugOnly);
		}

		public void TestThirdRailLegGetter_LessThanThreeLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Rail);
			Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Rail);

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(null, model.ThirdRailLeg);
		}

		#endregion

		#region FourthRailLeg

		public void TestFourthRailLegGetter_MoreThanFourLegsExist_ReturnModelForConsolFourthLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport1 = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var railTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Rail);
			var railTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Rail);
			var railTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Rail);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(railTransport4, model.FourthRailLeg.Parent_DebugOnly);
		}

		public void TestFourthRailLegGetter_LessThanFourLegsExist_ReturnNull()
		{
			var defaultTransport = Consol.Transports[0];

			Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Rail);
			Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Rail);
			Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Rail);

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(null, model.FourthRailLeg);
		}

		#endregion

		#region LastRailLeg

		public void TestLastRailLegGetter_SomeLegsExist_ReturnModelForConsolLastLeg()
		{
			var defaultTransport = Consol.Transports[0];

			var airTransport = Consol.Transports.New("AUSYD", "USLAX", Constants.TransportModes.Air);
			var seaTransport = Consol.Transports.New("USLAX", "USNYC", Constants.TransportModes.Sea);
			var roadTransport = Consol.Transports.New("USNYC", "UAIEV", Constants.TransportModes.Road);
			var railTransport1 = Consol.Transports.New("UAIEV", "AEDHF", Constants.TransportModes.Rail);
			var railTransport2 = Consol.Transports.New("AEDHF", "AEJYH", Constants.TransportModes.Rail);
			var railTransport3 = Consol.Transports.New("AEJYH", "USSP2", Constants.TransportModes.Rail);
			var railTransport4 = Consol.Transports.New("USSP2", "USSP8", Constants.TransportModes.Rail);
			var transport = Consol.Transports.New("USSP8", "USSPB");

			defaultTransport.Delete();

			var model = new ContainerEventDataModel(Container);
			AssertEquals(railTransport4, model.LastRailLeg.Parent_DebugOnly);
		}

		#endregion

		#endregion

		#region Implementation

		CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<CommonConsol>();
				}

				return consol;
			}
		}
		CommonConsol consol;

		CommonContainer Container
		{
			get
			{
				if (container == null)
				{
					container = Consol.Containers.AddNew();
				}

				return container;
			}
		}
		CommonContainer container;

		#endregion
	}
}
