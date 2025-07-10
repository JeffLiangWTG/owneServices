using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	internal abstract class MovementOrderHelperTest : BaseFreightTest
	{
		public void TestFirstImportTransportByLoadAndDischarge()
		{
			var shipment = Factory.New<CommonShipment>();
			var leg0 = NewLeg(shipment, OverseasPort, OverseasPort2, 1);
			var helper = NewOrderHelper(shipment);
			AssertNull(null, helper.FirstImportTransportByLoadAndDischarge());

			leg0.JW_RL_NKDiscPort = "";
			AssertLegEquals(leg0, helper.FirstImportTransportByLoadAndDischarge());

			leg0.JW_RL_NKDiscPort = HomePort;
			AssertLegEquals(leg0, helper.FirstImportTransportByLoadAndDischarge());

			var leg1 = NewLeg(shipment, AlternateHomePort, HomePort, 2);
			helper = NewOrderHelper(shipment);
			AssertLegEquals(leg0, helper.FirstImportTransportByLoadAndDischarge());

			leg0.JW_RL_NKDiscPort = "";
			AssertLegEquals(leg0, helper.FirstImportTransportByLoadAndDischarge());

			leg1.JW_RL_NKLoadPort = OverseasPort2;
			AssertLegEquals(leg1, helper.FirstImportTransportByLoadAndDischarge());
		}

		public void TestFirstLeg()
		{
			SetupLegs();
			AssertLegEquals(Leg0, Helper.FirstLeg);
		}

		public void TestLastLeg()
		{
			SetupLegs();
			AssertLegEquals(Leg4, Helper.LastLeg);
		}

		public void TestImportLeg()
		{
			SetupLegs();
			AssertLegEquals(Leg1, Helper.ImportLeg);
		}

		public void TestExportLeg()
		{
			SetupLegs();
			AssertLegEquals(Leg3, Helper.ExportLeg);
		}

		public void TestFirstLastLegMatching()
		{
			SetupLegs();

			string local = HomePort.Left(2);

			AssertLegEquals(Leg1, Helper.FirstLegMatching((t) => t.JW_RL_NKDiscPort.StartsWith(local)));
			AssertLegEquals(Leg2, Helper.FirstLegMatching((t) => t.JW_RL_NKLoadPort.StartsWith(local)));
			AssertLegEquals(Leg2, Helper.LastLegMatching((t) => t.JW_RL_NKDiscPort.StartsWith(local)));
			AssertLegEquals(Leg3, Helper.LastLegMatching((t) => t.JW_RL_NKLoadPort.StartsWith(local)));
		}

		public void TestLastLegWithTransportMode()
		{
			SetupLegs();
			Leg0.JW_TransportMode = "AIR";
			Leg1.JW_TransportMode = "AIR";
			Leg2.JW_TransportMode = "AIR";
			Leg3.JW_TransportMode = "SEA";
			Leg4.JW_TransportMode = "SEA";
			AssertLegEquals(Leg2, Helper.LastLegWithTransportMode("AIR", null, ZString.Empty));
		}

		public void TestEnumerableOrder()
		{
			SetupLegs();
			List<Transport> list = new List<Transport>(Helper);

			CombineAssertions(delegate
			{
				AssertLegEquals("list[0]", Leg0, list[0]);
				AssertLegEquals("list[1]", Leg1, list[1]);
				AssertLegEquals("list[2]", Leg2, list[2]);
				AssertLegEquals("list[3]", Leg3, list[3]);
				AssertLegEquals("list[4]", Leg4, list[4]);
			});
		}

		#region Implementation

		string TransportAsString(Transport transport)
		{
			return transport.JW_RL_NKLoadPort + "->" + transport.JW_RL_NKDiscPort;
		}

		void AssertLegEquals(Transport transport1, Transport transport2)
		{
			AssertLegEquals("", transport1, transport2);
		}
		void AssertLegEquals(string message, Transport transport1, Transport transport2)
		{
			AssertEquals(message + "\r\nLeg1: " + TransportAsString(transport1) + "  Leg2: " + TransportAsString(transport2), transport1, transport2);
		}

		void SetupLegs()
		{
			Helper = NewOrderHelper(SetupShipmentWithLegs());
		}

		CommonShipment SetupShipmentWithLegs()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			Leg0 = NewLeg(shipment, OverseasPort, OverseasPort2, 1);
			Leg1 = NewLeg(shipment, OverseasPort2, HomePort, 2);
			Leg2 = NewLeg(shipment, HomePort, AlternateHomePort, 3);
			Leg3 = NewLeg(shipment, AlternateHomePort, OverseasPort3, 4);
			Leg4 = NewLeg(shipment, OverseasPort3, OverseasPort4, 5);
			return shipment;
		}

		protected virtual Transport NewLeg(CommonShipment shipment, ZString load, ZString discharge, byte order)
		{
			Transport transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = load;
			transport.JW_RL_NKDiscPort = discharge;
			return transport;
		}

		protected abstract TransportOrderHelper NewOrderHelper(CommonShipment shipment);

		Transport Leg0;
		Transport Leg1;
		Transport Leg2;
		Transport Leg3;
		Transport Leg4;

		TransportOrderHelper Helper;

		#endregion
	}
}
