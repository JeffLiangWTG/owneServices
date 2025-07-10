using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class UNDGStandardCalculatorTest : TestCaseWithFactory
	{
		public void TestCorrespondingStandardIsReturned_JTT()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			transport.JW_RL_NKLoadPort = "CNBHO";
			transport.JW_RL_NKDiscPort = "AUBNE";

			AssertEquals("Shipment with China Road leg should default JTT", UNDGSubstanceStandardTypes.JTT, DGStandardCalculator.GetCorrespondingStandardForShipment(shipment));
		}

		public void TestCorrespondingStandardIsReturned_CFR()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			shipment.JS_RL_NKOrigin = "US3VA";
			shipment.JS_RL_NKDestination = "GBLON";

			AssertEquals("Shipment with no legs originating in US should default CFR", UNDGSubstanceStandardTypes.CFR, DGStandardCalculator.GetCorrespondingStandardForShipment(shipment));
		}

		public void TestCorrespondingStandardsArePopulatedFromShipment()
		{
			var airShipment = Factory.New<ForwardingShipment>();
			airShipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("Air shipments should correspond to IATA", UNDGSubstanceStandardTypes.IATA, DGStandardCalculator.GetCorrespondingStandardForShipmentMode(airShipment));

			var seaShipment = Factory.New<ForwardingShipment>();
			seaShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals("Sea shipments should correspond to IMO", UNDGSubstanceStandardTypes.IMO, DGStandardCalculator.GetCorrespondingStandardForShipmentMode(seaShipment));

			var roadShipment = Factory.New<ForwardingShipment>();
			roadShipment.JS_TransportMode = Core.Constants.TransportModes.Road;

			AssertEquals("Road shipments should correspond to ADR", UNDGSubstanceStandardTypes.ADR, DGStandardCalculator.GetCorrespondingStandardForShipmentMode(roadShipment));

			var railShipment = Factory.New<ForwardingShipment>();
			railShipment.JS_TransportMode = Core.Constants.TransportModes.Rail;

			AssertEquals("Rail shipments should correspond to RID", UNDGSubstanceStandardTypes.RID, DGStandardCalculator.GetCorrespondingStandardForShipmentMode(railShipment));
		}

		public void TestCorrespondingStandardsArePopulatedFromTransportMode()
		{
			AssertEquals("Air transport mode should correspond to IATA", UNDGSubstanceStandardTypes.IATA, DGStandardCalculator.GetCorrespondingStandard(Core.Constants.TransportModes.Air));
			AssertEquals("Sea transport mode should correspond to IMO", UNDGSubstanceStandardTypes.IMO, DGStandardCalculator.GetCorrespondingStandard(Core.Constants.TransportModes.Sea));
			AssertEquals("Road transport mode should correspond to ADR", UNDGSubstanceStandardTypes.ADR, DGStandardCalculator.GetCorrespondingStandard(Core.Constants.TransportModes.Road));
			AssertEquals("Rail transport mode should correspond to RID", UNDGSubstanceStandardTypes.RID, DGStandardCalculator.GetCorrespondingStandard(Core.Constants.TransportModes.Rail));
		}

		public void TestStandardsWithCorrespondingModes()
		{
			Assert("IMO corresponds to SEA", DGStandardCalculator.StandardsWithCorrespondingModes.Contains(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO));
			Assert("IATA corresponds to AIR", DGStandardCalculator.StandardsWithCorrespondingModes.Contains(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA));
			Assert("ADR corresponds to ROAD", DGStandardCalculator.StandardsWithCorrespondingModes.Contains(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADR));
			Assert("RID corresponds to RAIL", DGStandardCalculator.StandardsWithCorrespondingModes.Contains(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.RID));

			Assert("ADN does not have corresponding Mode", !DGStandardCalculator.StandardsWithCorrespondingModes.Contains(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADN));
		}
	}
}
