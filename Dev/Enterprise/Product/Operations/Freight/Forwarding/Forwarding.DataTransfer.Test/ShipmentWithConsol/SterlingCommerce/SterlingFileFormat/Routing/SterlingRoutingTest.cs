using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingRouting))]
	public class SterlingRoutingTest : SterlingRecordTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SterlingRouting();
		}

		public void TestSterlingRoutingRecord()
		{
			AssertEquals("Parsed record is different from expected", ExpectedSterlingRoutingRecord1, SterlingForTest.RoutingInfo[0].Record);
		}
		const string ExpectedSterlingRoutingRecord1 = "RTN|AIR|AUSYD|2008-02-01 20:20:20 +11:00|2008-02-01 20:25:20 +11:00|USLAX|2008-02-02 20:20:20 +11:00|2008-02-02 20:25:20 +11:00|QF123||||S>\r\n";

		public void TestSeaSterlingRoutingRecord()
		{
			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.PlannedLeg routing = new Xsd.PlannedLeg();

			routing.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			routing.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "USLAX");
			routing.PortOfLoading.EstimatedDateTime = new ZDateTime(2008, 2, 2, 21, 25, 20);
			routing.PortOfLoading.ActualDateTime = new ZDateTime(2008, 2, 1, 21, 30, 20);
			routing.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "USCHI");
			routing.PortOfDischarge.EstimatedDateTime = new ZDateTime(2008, 2, 3, 20, 20, 20);
			routing.PortOfDischarge.ActualDateTime = new ZDateTime(2008, 2, 3, 20, 25, 20);
			Xsd.SailingWithVesselVoyage vesselInfo = new Xsd.SailingWithVesselVoyage();
			vesselInfo.VesselName = "Black Pearl";
			vesselInfo.LloydsNo = "LNumber";
			vesselInfo.VoyageNo = "1";
			routing.Item = vesselInfo;

			SterlingRouting routingRecord = new SterlingRouting();
			routingRecord.Source = routing;
			routingRecord.Level = "S";
			AssertEquals(routingRecord.Record, "RTN|SEA|USLAX|2008-02-02 21:25:20 +11:00|2008-02-01 21:30:20 +11:00|USCHI|2008-02-03 20:20:20 +11:00|2008-02-03 20:25:20 +11:00||Black Pearl|LNumber|1|S>\r\n");
		}

		public void TestRecordFromConsol()
		{
			Xsd.PlannedLeg routing = new Xsd.PlannedLeg();

			routing.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			routing.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "USLAX");
			routing.PortOfLoading.EstimatedDateTime = new ZDateTime(2008, 2, 2, 21, 25, 20);
			routing.PortOfLoading.ActualDateTime = new ZDateTime(2008, 2, 1, 21, 30, 20);
			routing.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "USCHI");
			routing.PortOfDischarge.EstimatedDateTime = new ZDateTime(2008, 2, 3, 20, 20, 20);
			routing.PortOfDischarge.ActualDateTime = new ZDateTime(2008, 2, 3, 20, 25, 20);
			Xsd.SailingWithVesselVoyage vesselInfo = new Xsd.SailingWithVesselVoyage();
			vesselInfo.VesselName = "Black Pearl";
			vesselInfo.LloydsNo = "LNumber";
			vesselInfo.VoyageNo = "1";
			routing.Item = vesselInfo;

			SterlingRouting routingRecord = new SterlingRouting();
			routingRecord.Source = routing;
			routingRecord.Level = "C";
			AssertEquals(routingRecord.Record, "RTN|SEA|USLAX|2008-02-02 21:25:20 +11:00|2008-02-01 21:30:20 +11:00|USCHI|2008-02-03 20:20:20 +11:00|2008-02-03 20:25:20 +11:00||Black Pearl|LNumber|1|C>\r\n");
		}
	}
}
