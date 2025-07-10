using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class TransportModeCodesTest : TestCase
	{
		public void TestIsContainerised()
		{
			AssertEquals("AirContainer", true, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.AirContainer));
			AssertEquals("AirNonContainer", false, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.AirNonContainer));
			AssertEquals("Auto", false, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.Auto));
			AssertEquals("BorderWaterBorne", false, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.BorderWaterBorne));
			AssertEquals("FixedTransportInstallations", false, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.FixedTransportInstallations));
			AssertEquals("Mail", false, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.Mail));
			AssertEquals("PassengerHandCarried", false, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.PassengerHandCarried));
			AssertEquals("Pedestrian", false, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.Pedestrian));
			AssertEquals("RailContainer", true, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.RailContainer));
			AssertEquals("RailNonContainer", false, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.RailNonContainer));
			AssertEquals("RoadOther", false, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.RoadOther));
			AssertEquals("TruckContainer", true, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.TruckContainer));
			AssertEquals("TruckNonContainer", false, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.TruckNonContainer));
			AssertEquals("VesselContainer", true, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.VesselContainer));
			AssertEquals("VesselNonContainer", false, TransportModeCodes.IsContainerised(TransportModeCodes.Codes.VesselNonContainer));
		}

		public void TestIsAirTransport()
		{
			AssertEquals("AirContainer", true, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.AirContainer));
			AssertEquals("AirNonContainer", true, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.AirNonContainer));
			AssertEquals("Auto", false, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.Auto));
			AssertEquals("BorderWaterBorne", false, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.BorderWaterBorne));
			AssertEquals("FixedTransportInstallations", false, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.FixedTransportInstallations));
			AssertEquals("Mail", false, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.Mail));
			AssertEquals("PassengerHandCarried", false, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.PassengerHandCarried));
			AssertEquals("Pedestrian", false, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.Pedestrian));
			AssertEquals("RailContainer", false, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.RailContainer));
			AssertEquals("RailNonContainer", false, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.RailNonContainer));
			AssertEquals("RoadOther", false, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.RoadOther));
			AssertEquals("TruckContainer", false, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.TruckContainer));
			AssertEquals("TruckNonContainer", false, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.TruckNonContainer));
			AssertEquals("VesselContainer", false, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.VesselContainer));
			AssertEquals("VesselNonContainer", false, TransportModeCodes.IsAirTransport(TransportModeCodes.Codes.VesselNonContainer));
		}

		public void TestTransportModeCodesIsSortedInCodeOrder()
		{
			TransportModeCodes codes = new TransportModeCodes();
			AssertEquals(15, codes.Count);
			AssertEquals("10", codes[0].Code);
			AssertEquals("11", codes[1].Code);
			AssertEquals("12", codes[2].Code);
			AssertEquals("20", codes[3].Code);
			AssertEquals("21", codes[4].Code);
			AssertEquals("30", codes[5].Code);
			AssertEquals("31", codes[6].Code);
			AssertEquals("32", codes[7].Code);
			AssertEquals("33", codes[8].Code);
			AssertEquals("34", codes[9].Code);
			AssertEquals("40", codes[10].Code);
			AssertEquals("41", codes[11].Code);
			AssertEquals("50", codes[12].Code);
			AssertEquals("60", codes[13].Code);
			AssertEquals("70", codes[14].Code);
		}
	}
}
