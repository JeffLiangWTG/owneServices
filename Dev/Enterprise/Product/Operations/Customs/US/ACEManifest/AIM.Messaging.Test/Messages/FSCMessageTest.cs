using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	[TestedType(typeof(FSCMessage))]
	sealed class FSCMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadFSCMessage()
		{
			var message = Factory.New<FSCMessage>();
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.USAMA, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", Constants.AIMMessageSubTypes.FSC, message.EM_MessageSubType);
			AssertEquals("MessageTypeCode", Constants.AIMMessageSubTypes.FSC, message.MessageTypeCode);
			AssertEquals("MessageTypeDescription", "Freight Status Condition", message.MessageTypeDescription);
			Factory.Save();

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertType<FSCMessage>(reloadedMessage);

			AssertType<FSCMessage>(GetNewBusinessObject());
		}

		[TestDate(2020, 12, 27)]
		public void TestFSCMessageProperties_BillIsSplit()
		{
			var messageText = @"FSC
MIAKLM
081-11223344-HAWB123-B
ARR/KL888/24DEC-A
FSC/02
TXT/BILL IS SPLIT
/A-KL325/23DEC
/B-PA014/24DEC";

			var fscMessage = Factory.New<FSCMessage>();
			fscMessage.EM_MessageText = messageText;

			CombineAssertions(() =>
			{
				AssertEquals("ComponentIdentifier", "FSC", fscMessage.ComponentIdentifier);

				// inherited
				AssertEquals("AirportOfArrival", "MIA", fscMessage.AirportOfArrival);
				AssertEquals("CargoTerminalOperator", "KLM", fscMessage.CargoTerminalOperator);

				AssertEquals("AirWaybillPrefix", "081", fscMessage.AirWaybillPrefix);
				AssertEquals("AirWaybillSerialNumber", "11223344", fscMessage.AirWaybillSerialNumber);
				AssertEquals("HAWBNumber", "HAWB123", fscMessage.HAWBNumber);
				AssertEquals("PackageTrackingIdentifier", "", fscMessage.PackageTrackingIdentifier);

				AssertEquals("FlightNumber", "KL888", fscMessage.FlightNumber);
				AssertEquals("PartArrivalReference", "A", fscMessage.PartArrivalReference);
				AssertEquals("ScheduledArrivalDate", new ZDate(2020, 12, 24), fscMessage.CalculatedScheduledArrivalDate);

				AssertEquals("ActionCode", ZString.Empty, fscMessage.ActionCode);
				AssertEquals("Remarks", ZString.Empty, fscMessage.Remarks);

				// instance
				AssertEquals("StatusAnswerCode", AIMFreightStatusCodes.Codes.BillIsSplit, fscMessage.StatusAnswerCode);

				AssertEquals("Information", "BILL IS SPLIT", fscMessage.Information);
				var textContinuation = fscMessage.TextContinuation.ToArray();
				AssertEquals("textContinuation[0]", "/A-KL325/23DEC\r\n", textContinuation[0]);
				AssertEquals("textContinuation[1]", "/B-PA014/24DEC\r\n", textContinuation[1]);

				var waybill = fscMessage.WayBill;
				AssertEquals("AirportOfOrigin", ZString.Empty, waybill.AirportOfOrigin);
				AssertEquals("PermitToProceedDestinationAirport", ZString.Empty, waybill.PermitToProceedDestinationAirport);
				AssertEquals("NumberOfPieces", 0m, waybill.NumberOfPieces);
				AssertEquals("WeightCode", ZString.Empty, waybill.WeightCode);
				AssertEquals("Weight", 0m, waybill.Weight);
				AssertEquals("CargoDescription", ZString.Empty, waybill.CargoDescription);
				AssertEquals("DateOfArrivalAtThePermitToProceedDestinationAirport", ZDate.Empty, waybill.DateOfArrivalAtThePermitToProceedDestinationAirport);

				var fscWaybill = fscMessage.WayBillFSC;
				AssertEquals("WayBillFSC AirWaybillPrefix", "081", fscWaybill.AirWaybillPrefix);
				AssertEquals("WayBillFSC AWBSerialNumber", "11223344", fscWaybill.AWBSerialNumber);
				AssertEquals("WayBillFSC HAWBNumber", "HAWB123", fscWaybill.HAWBNumber);
				AssertEquals("WayBillFSC PartArrivalReference", "B", fscWaybill.PartArrivalReference);

				var splitBillArrivals = fscMessage.SplitBillArrivals.ToArray();
				AssertEquals("splitBillArrivals[0] FlightNumber", "KL325", splitBillArrivals[0].FlightNumber);
				AssertEquals("splitBillArrivals[0] PartArrivalReference", "A", splitBillArrivals[0].PartArrivalReference);
				AssertEquals("splitBillArrivals[0] ScheduledArrivalDate", new ZDate(2020, 12, 23), splitBillArrivals[0].EstimateScheduledArrivalDate);
				AssertEquals("splitBillArrivals[1] FlightNumber", "PA014", splitBillArrivals[1].FlightNumber);
				AssertEquals("splitBillArrivals[1] PartArrivalReference", "B", splitBillArrivals[1].PartArrivalReference);
				AssertEquals("splitBillArrivals[1] ScheduledArrivalDate", new ZDate(2020, 12, 24), splitBillArrivals[1].EstimateScheduledArrivalDate);

				var routingInformation = fscMessage.RoutingInformation;
				AssertEquals("AirportOfArrival", ZString.Empty, routingInformation.AirportOfArrival);
				AssertEquals("PermitToProceedDestinationAirport", ZString.Empty, routingInformation.PermitToProceedDestinationAirport);
				AssertEquals("NumberOfPieces", 0m, routingInformation.NumberOfPieces);
				AssertEquals("InBondOriginAirport", ZString.Empty, routingInformation.InBondOriginAirport);
				AssertEquals("InBondDestinationAirport", ZString.Empty, routingInformation.InBondDestinationAirport);
				AssertEquals("InBondStatus", ZString.Empty, routingInformation.InBondStatus);
			});
		}

		[TestDate(2020, 12, 27)]
		public void TestFSCMessageProperties_RoutingInformation()
		{
			var messageText = @"FSC
MIAKLM
081-11223344-HAWB123/123456
ARR/KL888/24DEC-A
FSC/08
TXT/ARR-ANC PTP-LAX INB-ORDJFK-A T32";

			var fscMessage = Factory.New<FSCMessage>();
			fscMessage.EM_MessageText = messageText;

			CombineAssertions(() =>
			{
				AssertEquals("ComponentIdentifier", "FSC", fscMessage.ComponentIdentifier);

				// instance
				AssertEquals("StatusAnswerCode", AIMFreightStatusCodes.Codes.RoutingInformationFollows, fscMessage.StatusAnswerCode);
				AssertEquals("Information", "ARR-ANC PTP-LAX INB-ORDJFK-A T32", fscMessage.Information);
				AssertEquals("TextContinuation", false, fscMessage.TextContinuation.Any());

				var waybill = fscMessage.WayBill;
				AssertEquals("AirportOfOrigin", ZString.Empty, waybill.AirportOfOrigin);
				AssertEquals("PermitToProceedDestinationAirport", ZString.Empty, waybill.PermitToProceedDestinationAirport);
				AssertEquals("NumberOfPieces", 0m, waybill.NumberOfPieces);
				AssertEquals("WeightCode", ZString.Empty, waybill.WeightCode);
				AssertEquals("Weight", 0m, waybill.Weight);
				AssertEquals("CargoDescription", ZString.Empty, waybill.CargoDescription);
				AssertEquals("DateOfArrivalAtThePermitToProceedDestinationAirport", ZDate.Empty, waybill.DateOfArrivalAtThePermitToProceedDestinationAirport);

				AssertEquals("SplitBillArrivals", false, fscMessage.SplitBillArrivals.Any());

				var routingInformation = fscMessage.RoutingInformation;
				AssertEquals("AirportOfArrival", "ANC", routingInformation.AirportOfArrival);
				AssertEquals("PermitToProceedDestinationAirport", "LAX", routingInformation.PermitToProceedDestinationAirport);
				AssertEquals("InBondOriginAirport", "ORD", routingInformation.InBondOriginAirport);
				AssertEquals("InBondDestinationAirport", "JFK", routingInformation.InBondDestinationAirport);
				AssertEquals("InBondStatus", "A", routingInformation.InBondStatus);
				AssertEquals("NumberOfPieces", 32.0m, routingInformation.NumberOfPieces);
			});
		}

		[TestDate(2020, 12, 27)]
		public void TestFSCMessageProperties_RoutingInformationWithoutDischargePort()
		{
			var messageText = @"FSC
MIAKLM
081-11223344-HAWB123/123456
ARR/KL888/24DEC-A
FSC/08
TXT/ARR-ANC INB-ORDJFK-A T32";

			var fscMessage = Factory.New<FSCMessage>();
			fscMessage.EM_MessageText = messageText;

			CombineAssertions(() =>
			{
				AssertEquals("ComponentIdentifier", "FSC", fscMessage.ComponentIdentifier);
				AssertEquals("StatusAnswerCode", AIMFreightStatusCodes.Codes.RoutingInformationFollows, fscMessage.StatusAnswerCode);
				AssertEquals("Information", "ARR-ANC INB-ORDJFK-A T32", fscMessage.Information);
				AssertEquals("TextContinuation", false, fscMessage.TextContinuation.Any());

				var routingInformation = fscMessage.RoutingInformation;
				AssertEquals("AirportOfArrival", "ANC", routingInformation.AirportOfArrival);
				AssertEquals("PermitToProceedDestinationAirport", ZString.Empty, routingInformation.PermitToProceedDestinationAirport);
				AssertEquals("InBondOriginAirport", "ORD", routingInformation.InBondOriginAirport);
				AssertEquals("InBondDestinationAirport", "JFK", routingInformation.InBondDestinationAirport);
				AssertEquals("InBondStatus", "A", routingInformation.InBondStatus);
				AssertEquals("NumberOfPieces", 32.0m, routingInformation.NumberOfPieces);
			});
		}

		[TestDate(2020, 12, 27)]
		public void TestFSCMessageProperties_CurrentAirWaybillInformationOnFile()
		{
			var messageText = @"FSC
MIAKLM
081-11223344-HAWB123/123456
ARR/KL888/24DEC-A
FSC/10
WBL/KHHSFO/T50/K680.0/SHOES/24OCT
/SOCKS
ARR/ZZ0100/24OCT-A/B10/K50.0
TRN/ORD-D//97000006
TRN/ORD-D//J999";

			var fscMessage = Factory.New<FSCMessage>();
			fscMessage.EM_MessageText = messageText;

			CombineAssertions(() =>
			{
				AssertEquals("ComponentIdentifier", "FSC", fscMessage.ComponentIdentifier);

				// instance
				AssertEquals("StatusAnswerCode", AIMFreightStatusCodes.Codes.CurrentAirWaybillInformationOnFileFollows, fscMessage.StatusAnswerCode);
				AssertEquals("Information", ZString.Empty, fscMessage.Information);
				AssertEquals("TextContinuation", false, fscMessage.TextContinuation.Any());

				var waybill = fscMessage.WayBill;
				AssertEquals("AirportOfOrigin", "KHH", waybill.AirportOfOrigin);
				AssertEquals("PermitToProceedDestinationAirport", "SFO", waybill.PermitToProceedDestinationAirport);
				AssertEquals("NumberOfPieces", 50m, waybill.NumberOfPieces);
				AssertEquals("WeightCode", "K", waybill.WeightCode);
				AssertEquals("Weight", 680.0m, waybill.Weight);
				AssertEquals("CargoDescription", "SHOES", waybill.CargoDescription);
				AssertEquals("DateOfArrivalAtThePermitToProceedDestinationAirport", new ZDate(2020, 10, 24), waybill.DateOfArrivalAtThePermitToProceedDestinationAirport);

				AssertEquals("SplitBillArrivals", false, fscMessage.SplitBillArrivals.Any());

				var routingInformation = fscMessage.RoutingInformation;
				AssertEquals("AirportOfArrival", ZString.Empty, routingInformation.AirportOfArrival);
				AssertEquals("PermitToProceedDestinationAirport", ZString.Empty, routingInformation.PermitToProceedDestinationAirport);
				AssertEquals("NumberOfPieces", 0m, routingInformation.NumberOfPieces);
				AssertEquals("InBondOriginAirport", ZString.Empty, routingInformation.InBondOriginAirport);
				AssertEquals("InBondDestinationAirport", ZString.Empty, routingInformation.InBondDestinationAirport);
				AssertEquals("InBondStatus", ZString.Empty, routingInformation.InBondStatus);
			});
		}

		public void TestEM_MessageInterpretation()
		{
			var message = Factory.New<FSCMessage>();
			message.EM_MessageText = @"FSC
LAXZI
439-21031614-BKG210316HB1
ARR/ZI001/15MAR
FSC/10
TXT/BILL IS SPLIT
/A-KL325/23DEC
/B-PA014/23DEC
/C-KL325/24DEC
WBL/SYD/T1/L1/THINGS
ARR/ZI001/15MAR
TRN/ORD-D//97000006
TRN/ORD-D//J999
";

			var expectedMessageInterpretation = @"Masterbill: 439-21031614
Housebill: BKG210316HB1

10 - Current Air Waybill information on file follows.
BILL IS SPLIT
A-KL325/23DEC
B-PA014/23DEC
C-KL325/24DEC
WBL/SYD/T1/L1/THINGS
ARR/ZI001/15MAR
TRN/ORD-D//97000006
TRN/ORD-D//J999


FSC
LAXZI
439-21031614-BKG210316HB1
ARR/ZI001/15MAR
FSC/10
TXT/BILL IS SPLIT
/A-KL325/23DEC
/B-PA014/23DEC
/C-KL325/24DEC
WBL/SYD/T1/L1/THINGS
ARR/ZI001/15MAR
TRN/ORD-D//97000006
TRN/ORD-D//J999";

			AssertMultilineASCIIEquals("EM_MessageInterpretation should be output as expected.", expectedMessageInterpretation, message.EM_MessageInterpretation);
		}

		public void TestFERDoesNotShowEmptyLabels()
		{
			var message = Factory.New<FSCMessage>();
			message.EM_MessageText = @"FSC
LAXZI
439-21031614
ARR/ZI001/15MAR
FSC/10
TXT/BILL IS SPLIT
/A-KL325/23DEC
/B-PA014/23DEC
/C-KL325/24DEC
WBL/SYD/T1/L1/THINGS
ARR/ZI001/15MAR
TRN/ORD-D//97000006
TRN/ORD-D//J999
";

			var expectedMessageInterpretation = @"Masterbill: 439-21031614

10 - Current Air Waybill information on file follows.
BILL IS SPLIT
A-KL325/23DEC
B-PA014/23DEC
C-KL325/24DEC
WBL/SYD/T1/L1/THINGS
ARR/ZI001/15MAR
TRN/ORD-D//97000006
TRN/ORD-D//J999


FSC
LAXZI
439-21031614
ARR/ZI001/15MAR
FSC/10
TXT/BILL IS SPLIT
/A-KL325/23DEC
/B-PA014/23DEC
/C-KL325/24DEC
WBL/SYD/T1/L1/THINGS
ARR/ZI001/15MAR
TRN/ORD-D//97000006
TRN/ORD-D//J999";

			AssertMultilineASCIIEquals("EM_MessageInterpretation should not include blank Housebill.", expectedMessageInterpretation, message.EM_MessageInterpretation);
		}
	}
}
