using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging.Generators;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	public class AIMMessageBuilderTest : TestCase
	{
		public void TestBuildFRIMAWBMessage()
		{
			var mock = new Mock<IManifestMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FRI);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.CargoControlLine).Returns(AIMInterfaceTestHelper.GetCargoControlLocation("JFK", "XYZ").Object);
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345675", false).Object);
			mock.Setup(m => m.Waybill).Returns(AIMInterfaceTestHelper.GetWaybillDetails("FRA", 1, "K", 10, "TOYS", "", null).Object);
			mock.Setup(m => m.Arrival).Returns(AIMInterfaceTestHelper.GetArrival("XYZ1234A", new ZDate(2019, 10, 25), "", false, 0, "", 0).Object);
			mock.Setup(m => m.Shipper).Returns(AIMInterfaceTestHelper.GetParty("TOTLER TOYS", "FRANKFURT", "DE", "12 VIRGINIA COURT", "", "", "").Object);
			mock.Setup(m => m.Consignee).Returns(AIMInterfaceTestHelper.GetParty("TOYS R WE", "NEW YORK", "US", "8812 FUN STREET", "NY", "12345", "123-456-7890").Object);
			mock.Setup(m => m.Transfer).Returns((IAIMTransfer)null);
			mock.Setup(m => m.FDAFreightIndicator).Returns((IAIMFDAFreightIndicator)null);
			var messageHeader = mock.Object;

			const string expectedMessage = @"FRI
JFKXYZ
999-12345675
WBL/FRA/T1/K10/TOYS
ARR/XYZ1234A/25OCT
SHP/TOTLER TOYS
/12 VIRGINIA COURT
/FRANKFURT
/DE
CNE/TOYS R WE
/8812 FUN STREET
/NEW YORK/NY
/US/12345/123-456-7890";

			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory);
			AssertBuild<FRIBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		public void TestBuildFXIMAWBMessage()
		{
			var mock = new Mock<IManifestMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FXI);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.CargoControlLine).Returns(AIMInterfaceTestHelper.GetCargoControlLocation("JFK", "XYZ").Object);
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345675", false).Object);
			mock.Setup(m => m.Waybill).Returns(AIMInterfaceTestHelper.GetWaybillDetails("FRA", 1, "K", 10, "TOYS", "", null).Object);
			mock.Setup(m => m.Arrival).Returns(AIMInterfaceTestHelper.GetArrival("XYZ1234A", new ZDate(2019, 10, 25), "", false, 0, "", 0).Object);
			mock.Setup(m => m.Shipper).Returns(AIMInterfaceTestHelper.GetParty("TOTLER TOYS", "FRANKFURT", "DE", "12 VIRGINIA COURT", "", "", "").Object);
			mock.Setup(m => m.Consignee).Returns(AIMInterfaceTestHelper.GetParty("TOYS R WE", "NEW YORK", "US", "8812 FUN STREET", "NY", "12345", "123-456-7890").Object);
			mock.Setup(m => m.Transfer).Returns((IAIMTransfer)null);
			mock.Setup(m => m.FDAFreightIndicator).Returns((IAIMFDAFreightIndicator)null);
			var messageHeader = mock.Object;

			const string expectedMessage = @"FXI
JFKXYZ
999-12345675
WBL/FRA/T1/K10/TOYS
ARR/XYZ1234A/25OCT
SHP/TOTLER TOYS
/12 VIRGINIA COURT
/FRANKFURT
/DE
CNE/TOYS R WE
/8812 FUN STREET
/NEW YORK/NY
/US/12345/123-456-7890";

			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory);
			AssertBuild<FXIBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		public void TestBuildFRIHAWBMessage()
		{
			var mock = new Mock<IBillMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FRI);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345675", false).Object);
			mock.Setup(m => m.Waybill).Returns(AIMInterfaceTestHelper.GetWaybillDetails("FRA", 1, "K", 10, "TOYS", "", null).Object);
			mock.Setup(m => m.Consignee).Returns(AIMInterfaceTestHelper.GetParty("TOYS R WE", "NEW YORK", "US", "8812 FUN STREET", "NY", "12345", "123-456-7890").Object);

			var messageHeader = mock.Object;

			const string expectedMessage = @"FRI
999-12345675
WBL/FRA/T1/K10/TOYS
CNE/TOYS R WE
/8812 FUN STREET
/NEW YORK/NY
/US/12345/123-456-7890";

			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory, true);
			AssertBuild<FRIBillBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		public void TestBuildFXIHAWBMessage()
		{
			var mock = new Mock<IBillMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FXI);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345675", false).Object);
			mock.Setup(m => m.Waybill).Returns(AIMInterfaceTestHelper.GetWaybillDetails("FRA", 1, "K", 10, "TOYS", "", null).Object);
			mock.Setup(m => m.Consignee).Returns(AIMInterfaceTestHelper.GetParty("TOYS R WE", "NEW YORK", "US", "8812 FUN STREET", "NY", "12345", "123-456-7890").Object);

			var messageHeader = mock.Object;

			const string expectedMessage = @"FXI
999-12345675
WBL/FRA/T1/K10/TOYS
CNE/TOYS R WE
/8812 FUN STREET
/NEW YORK/NY
/US/12345/123-456-7890";

			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory, true);
			AssertBuild<FXIBillBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		public void TestBuildFRCMAWBMessage()
		{
			var mock = new Mock<IManifestMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FRC);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.CargoControlLine).Returns(AIMInterfaceTestHelper.GetCargoControlLocation("ORD", "KLM").Object);
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345678", true).Object);
			mock.Setup(m => m.Waybill).Returns(AIMInterfaceTestHelper.GetWaybillDetails("AMS", 1, "L", 50, "SEAT BELT RETRACTOR", "", null).Object);
			mock.Setup(m => m.Arrival).Returns(AIMInterfaceTestHelper.GetArrival("KLM1234S", new ZDate(2019, 6, 17), "", false, 0, "", 0).Object);
			mock.Setup(m => m.Agent).Returns(AIMInterfaceTestHelper.GetAgent("BCBT336").Object);
			mock.Setup(m => m.Consignee).Returns(AIMInterfaceTestHelper.GetParty("NIPPON CAR DEALER", "CHICAGO", "US", "345 N. INDUSTRIAL LANE", "IL", "60606", "123-456-7890").Object);
			mock.Setup(m => m.Transfer).Returns(AIMInterfaceTestHelper.GetTransfer("LAX", "D", "13-150279800", "1234").Object);
			mock.Setup(m => m.ReasonForAmendment).Returns(AIMInterfaceTestHelper.GetReasonForAmendment("03", "").Object);
			var messageHeader = mock.Object;

			const string expectedMessage = @"FRC
ORDKLM
999-12345678-M
WBL/AMS/T1/L50/SEAT BELT RETRACTOR
ARR/KLM1234S/17JUN
AGT/BCBT336
CNE/NIPPON CAR DEALER
/345 N. INDUSTRIAL LANE
/CHICAGO/IL
/US/60606/123-456-7890
TRN/LAX-D/13-150279800/1234
RFA/03";

			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory);
			AssertBuild<FRCBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		public void TestBuildFXCMAWBMessage()
		{
			var mock = new Mock<IManifestMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FXC);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.CargoControlLine).Returns(AIMInterfaceTestHelper.GetCargoControlLocation("ORD", "KLM").Object);
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345678", true).Object);
			mock.Setup(m => m.Waybill).Returns(AIMInterfaceTestHelper.GetWaybillDetails("AMS", 1, "L", 50, "SEAT BELT RETRACTOR", "", null).Object);
			mock.Setup(m => m.Arrival).Returns(AIMInterfaceTestHelper.GetArrival("KLM1234S", new ZDate(2019, 6, 17), "", false, 0, "", 0).Object);
			mock.Setup(m => m.Agent).Returns(AIMInterfaceTestHelper.GetAgent("BCBT336").Object);
			mock.Setup(m => m.Consignee).Returns(AIMInterfaceTestHelper.GetParty("NIPPON CAR DEALER", "CHICAGO", "US", "345 N. INDUSTRIAL LANE", "IL", "60606", "123-456-7890").Object);
			mock.Setup(m => m.Transfer).Returns(AIMInterfaceTestHelper.GetTransfer("LAX", "D", "13-150279800", "1234").Object);
			mock.Setup(m => m.ReasonForAmendment).Returns(AIMInterfaceTestHelper.GetReasonForAmendment("03", "").Object);
			var messageHeader = mock.Object;

			const string expectedMessage = @"FXC
ORDKLM
999-12345678-M
WBL/AMS/T1/L50/SEAT BELT RETRACTOR
ARR/KLM1234S/17JUN
AGT/BCBT336
CNE/NIPPON CAR DEALER
/345 N. INDUSTRIAL LANE
/CHICAGO/IL
/US/60606/123-456-7890
TRN/LAX-D/13-150279800/1234
RFA/03";

			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory);
			AssertBuild<FXCBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		public void TestBuildFRCHAWBMessage()
		{
			var mock = new Mock<IBillMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FRC);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345678", true).Object);
			mock.Setup(m => m.Waybill).Returns(AIMInterfaceTestHelper.GetWaybillDetails("AMS", 1, "L", 50, "SEAT BELT RETRACTOR", "", null).Object);
			mock.Setup(m => m.Consignee).Returns(AIMInterfaceTestHelper.GetParty("NIPPON CAR DEALER", "CHICAGO", "US", "345 N. INDUSTRIAL LANE", "IL", "60606", "123-456-7890").Object);
			mock.Setup(m => m.ReasonForAmendment).Returns(AIMInterfaceTestHelper.GetReasonForAmendment("03", "").Object);
			var messageHeader = mock.Object;

			const string expectedMessage = @"FRC
999-12345678-M
WBL/AMS/T1/L50/SEAT BELT RETRACTOR
CNE/NIPPON CAR DEALER
/345 N. INDUSTRIAL LANE
/CHICAGO/IL
/US/60606/123-456-7890
RFA/03";

			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory, true);
			AssertBuild<FRCBillBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		public void TestBuildFXCHAWBMessage()
		{
			var mock = new Mock<IBillMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FXC);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345678", true).Object);
			mock.Setup(m => m.Waybill).Returns(AIMInterfaceTestHelper.GetWaybillDetails("AMS", 1, "L", 50, "SEAT BELT RETRACTOR", "", null).Object);
			mock.Setup(m => m.Consignee).Returns(AIMInterfaceTestHelper.GetParty("NIPPON CAR DEALER", "CHICAGO", "US", "345 N. INDUSTRIAL LANE", "IL", "60606", "123-456-7890").Object);
			mock.Setup(m => m.ReasonForAmendment).Returns(AIMInterfaceTestHelper.GetReasonForAmendment("03", "").Object);
			var messageHeader = mock.Object;

			const string expectedMessage = @"FXC
999-12345678-M
WBL/AMS/T1/L50/SEAT BELT RETRACTOR
CNE/NIPPON CAR DEALER
/345 N. INDUSTRIAL LANE
/CHICAGO/IL
/US/60606/123-456-7890
RFA/03";

			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory, true);
			AssertBuild<FXCBillBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		public void TestBuildFRXMAWBMessage()
		{
			var mock = new Mock<IManifestMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FRX);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.CargoControlLine).Returns(AIMInterfaceTestHelper.GetCargoControlLocation("LHR", "PAA").Object);
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345678", false).Object);
			mock.Setup(m => m.Arrival).Returns(AIMInterfaceTestHelper.GetArrival("PAA7089K", new ZDate(2019, 11, 13), "", false, 0, "", 0).Object);
			mock.Setup(m => m.ReasonForAmendment).Returns(AIMInterfaceTestHelper.GetReasonForAmendment("01", "REMARKS").Object);
			var messageHeader = mock.Object;

			const string expectedMessage = @"FRX
LHRPAA
999-12345678
ARR/PAA7089K/13NOV
RFA/01/REMARKS";

			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory);
			AssertBuild<FRXBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		public void TestBuildFXXMAWBMessage()
		{
			var mock = new Mock<IManifestMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FXX);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.CargoControlLine).Returns(AIMInterfaceTestHelper.GetCargoControlLocation("LHR", "PAA").Object);
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345678", false).Object);
			mock.Setup(m => m.Arrival).Returns(AIMInterfaceTestHelper.GetArrival("PAA7089K", new ZDate(2019, 11, 13), "", false, 0, "", 0).Object);
			mock.Setup(m => m.ReasonForAmendment).Returns(AIMInterfaceTestHelper.GetReasonForAmendment("01", "REMARKS").Object);
			var messageHeader = mock.Object;

			const string expectedMessage = @"FXX
LHRPAA
999-12345678
ARR/PAA7089K/13NOV
CED/000
RFA/01/REMARKS";

			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory);
			AssertBuild<FXXBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		public void TestBuildFRXHAWBMessage()
		{
			var mock = new Mock<IBillMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FRX);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345678", false).Object);
			mock.Setup(m => m.ReasonForAmendment).Returns(AIMInterfaceTestHelper.GetReasonForAmendment("01", "REMARKS").Object);
			var messageHeader = mock.Object;

			const string expectedMessage = @"FRX
999-12345678
RFA/01/REMARKS";

			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory, true);
			AssertBuild<FRXBillBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		public void TestBuildFXXHAWBMessage()
		{
			var mock = new Mock<IBillMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FXX);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345678", false).Object);
			mock.Setup(m => m.ReasonForAmendment).Returns(AIMInterfaceTestHelper.GetReasonForAmendment("01", "REMARKS").Object);
			var messageHeader = mock.Object;

			const string expectedMessage = @"FXX
999-12345678
CED/000
RFA/01/REMARKS";

			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory, true);
			AssertBuild<FXXBillBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		public void TestBuildFDM()
		{
			var mock = new Mock<IDepartureMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FDM);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.Departure).Returns(AIMInterfaceTestHelper.GetDeparture("KLM325", new ZDate(2019, 12, 25), new ZDate(2019, 12, 24), "1430", "", "").Object);
			var messageHeader = mock.Object;

			const string expectedMessage = @"FDM
DEP/KLM325/25DEC/24DEC1430";

			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory);
			AssertBuild<FDMBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		public void TestBuildFSN()
		{
			var mock = new Mock<IArrivalMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FSN);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.CargoControlLine).Returns(AIMInterfaceTestHelper.GetCargoControlLocation("MIA", "KLM").Object);
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "23234376", false).Object);
			mock.Setup(m => m.Arrival).Returns(AIMInterfaceTestHelper.GetArrival("KLM3258A", new ZDate(2019, 4, 1), "A", false, 0, "", 0).Object);
			mock.Setup(m => m.AirlineStatusNotification).Returns(AIMInterfaceTestHelper.GetAirlineStatusNotification("3", "").Object);
			var messageHeader = mock.Object;

			const string expectedMessage = @"FSN
MIAKLM
999-23234376
ARR/KLM3258A/01APR-A
ASN3";
			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory);
			AssertBuild<FSNBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		public void TestBuildFSQ()
		{
			var mock = new Mock<IFreightStatusQueryMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(Constants.AIMMessageSubTypes.FSQ);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.CargoControlLine).Returns(AIMInterfaceTestHelper.GetCargoControlLocation("JFK", "PA").Object);
			mock.Setup(m => m.FreightStatusQuery).Returns(AIMInterfaceTestHelper.GetFreightStatusQuery("05").Object);
			var aimAirWayBillMock = new Mock<IAIMAirWaybill>();
			mock.Setup(m => m.AirWaybill).Returns(aimAirWayBillMock.Object);
			aimAirWayBillMock.Setup(m => m.AirWaybillPrefix).Returns("999");
			aimAirWayBillMock.Setup(m => m.AWBSerialNumber).Returns("12345675");
			aimAirWayBillMock.Setup(m => m.PartArrivalReference).Returns("A");

			var messageHeader = mock.Object;

			const string expectedMessage = @"FSQ
JFKPA
999-12345675-A
FSQ/05";
			var factory = new BusinessObjectFactory();
			var builder = new AIMMessageBuilder(messageHeader, factory);
			AssertBuild<FSQBlockGenerator>(messageHeader, builder, expectedMessage);

			mock.VerifyAll();
		}

		void AssertBuild<T>(IAIMMessageHeader header, AIMMessageBuilder builder, string expectedMessage) where T : AIMBlockGenerator
		{
			var message = builder.Build();
			AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.USAMA, message.EM_ApplicationCode);
			AssertEquals("message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", header.MessageType, message.EM_MessageSubType);
			AssertEquals("message.EM_ApplicationReference", header.Reference, message.EM_ApplicationReference);

			AssertMultilineASCIIEquals("message.EM_MessageText", expectedMessage, message.EM_MessageText);
		}
	}
}
