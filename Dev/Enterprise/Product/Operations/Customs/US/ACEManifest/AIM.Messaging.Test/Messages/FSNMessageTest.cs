using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	[TestedType(typeof(FSNMessage))]
	sealed class FSNMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadFSNMessage()
		{
			var message = Factory.New<FSNMessage>();
			AssertEquals(ApplicationCodeList.Codes.USAMA, message.EM_ApplicationCode);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals(Constants.AIMMessageSubTypes.FSN, message.EM_MessageSubType);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			Factory.Save();

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertType<FSNMessage>(reloadedMessage);

			AssertType<FSNMessage>(GetNewBusinessObject());
		}

		[TestDate(2020, 12, 27)]
		public void TestFSNMessageProperties()
		{
			var messageText = @"FSN
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/24DEC-A
CSN/1C-20/12DEC1430/0158232873876/HLD 1234 ";

			var message = Factory.New<FSNMessage>();
			message.EM_MessageText = messageText;

			CombineAssertions(() =>
			{
				AssertEquals("message.ComponentIdentifier", "FSN", message.ComponentIdentifier);

				AssertEquals("message.AirportOfArrival", "MIA", message.AirportOfArrival);
				AssertEquals("message.CargoTerminalOperator", "KLM", message.CargoTerminalOperator);

				AssertEquals("message.AirWaybillPrefix", "081", message.AirWaybillPrefix);
				AssertEquals("message.AirWaybillSerialNumber", "11223344", message.AirWaybillSerialNumber);
				AssertEquals("message.HAWBNumber", "HAWB123", message.HAWBNumber);
				AssertEquals("message.PackageTrackingIdentifier", "123456", message.PackageTrackingIdentifier);

				AssertEquals("message.FlightNumber", "KL325", message.FlightNumber);
				AssertEquals("message.ScheduledArrivalDate", new ZDate(2020, 12, 24), message.CalculatedScheduledArrivalDate);
				AssertEquals("message.PartArrivalReference", "A", message.PartArrivalReference);

				AssertEquals("message.ActionCode", "1C", message.ActionCode);
				AssertEquals("message.Remarks", "HLD 1234", message.Remarks);
				AssertEquals("message.NumberOfPieces", 20, message.NumberOfPieces);
				AssertEquals("message.EntryType", "01", message.EntryType);
				AssertEquals("message.EntryNumber", "58232873876", message.EntryNumber);
			});
		}

		[TestDate(2020, 12, 27)]
		public void TestScheduledArrivalDate_NextYear()
		{
			var messageText = @"FSN
MIAKLM
081-11223344-HAWB123/123456
ARR/KM325/02JAN-A
CSN/1C-20/12DEC1430/0158232873876/HLD1234
";

			var message = Factory.New<FSNMessage>();
			message.EM_MessageText = messageText;
			AssertEquals(new ZDate(2021, 1, 2), message.CalculatedScheduledArrivalDate);
		}

		[TestDate(2021, 08, 23)]
		public void TestEM_MessageInterpretation()
		{
			var message = Factory.New<FSNMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			message.EM_MessageText = @"FSN
LAXCR
666-67664100-166676653
ARR/ABC0826/26AUG
CSN/1C-1/26AUG1237/86SEC 321
";

			var expectedMessageInterpretation = @"Masterbill: 666-67664100
Housebill: 166676653

Flight No.: ABC0826
Scheduled Arrival Date: 26-Aug
Arrival Airport: LAX

CBP Status Notification
1C - Entry processed: CBP general examination. - for 1 piece

ARR/ABC0826/26AUG
CSN/1C-1/26AUG1237/86SEC 321


FSN
LAXCR
666-67664100-166676653
ARR/ABC0826/26AUG
CSN/1C-1/26AUG1237/86SEC 321";

			AssertMultilineASCIIEquals("EM_MessageInterpretation should be output as expected.", expectedMessageInterpretation, message.EM_MessageInterpretation);
		}

		[TestDate(2021, 08, 23)]
		public void TestMessageInterpretation1PieceNoHawb()
		{
			var message = Factory.New<FSNMessage>();
			message.EM_MessageText = @"FSN
JFKZI
439-51222231
ARR/ZI001/02AUG-A
CSN/1S-1/17AUG0230
";

			var expectedMessageInterpretation = @"Arrival Message Sent

Masterbill: 439-51222231

Flight No.: ZI001
Scheduled Arrival Date: 02-Aug
Arrival Airport: JFK

CBP Status Notification
1S - CBP Eligible for General Order. - for 1 piece

ARR/ZI001/02AUG-A
CSN/1S-1/17AUG0230


FSN
JFKZI
439-51222231
ARR/ZI001/02AUG-A
CSN/1S-1/17AUG0230";

			AssertMultilineASCIIEquals("EM_MessageInterpretation should be output as expected.", expectedMessageInterpretation, message.EM_MessageInterpretation);
		}

		[TestDate(2021, 08, 23)]
		public void TestMessageInterpretationWithRemarks()
		{
			var message = Factory.New<FSNMessage>();

			message.EM_MessageText = @"FSN
JFKZI
439-51222301
ARR/ZI031/06AUG-B
CSN/12-2/06AUG0712/63770000206/BILL ARRIVED ON 2108
";

			var expectedMessageInterpretation = @"Arrival Message Sent

Masterbill: 439-51222301

Flight No.: ZI031
Scheduled Arrival Date: 06-Aug
Arrival Airport: JFK

CBP Status Notification
12 - In-bond Arrival at Dest by BOL Received (specific BOL number-such as via ASN3) - for 2 pieces
Remarks: BILL ARRIVED ON 2108

ARR/ZI031/06AUG-B
CSN/12-2/06AUG0712/63770000206/BILL ARRIVED ON 2108


FSN
JFKZI
439-51222301
ARR/ZI031/06AUG-B
CSN/12-2/06AUG0712/63770000206/BILL ARRIVED ON 2108";

			AssertMultilineASCIIEquals("EM_MessageInterpretation should be output as expected.", expectedMessageInterpretation, message.EM_MessageInterpretation);
		}

		[TestDate(2021, 08, 23)]
		public void TestMessageInterpretationWithTrackingID()
		{
			var message = Factory.New<FSNMessage>();
			message.EM_MessageText = @"FSN
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/24DEC-A
CSN/1C-20/12DEC1430/0158232873876/HLD1234
";

			var expectedMessageInterpretation = @"Arrival Message Sent

Masterbill: 081-11223344
Housebill: HAWB123
Package Tracking ID: 123456

Flight No.: KL325
Scheduled Arrival Date: 24-Dec
Arrival Airport: MIA

CBP Status Notification
1C - Entry processed: CBP general examination. - for 20 pieces
Remarks: HLD1234

ARR/KL325/24DEC-A
CSN/1C-20/12DEC1430/0158232873876/HLD1234


FSN
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/24DEC-A
CSN/1C-20/12DEC1430/0158232873876/HLD1234";

			AssertMultilineASCIIEquals("EM_MessageInterpretation should be output as expected.", expectedMessageInterpretation, message.EM_MessageInterpretation);
		}

		public void TestMessageInterpretationForIncomingMessage()
		{
			var message = Factory.New<FSNMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageText = @"FSN
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/24DEC-A
CSN/1C-20/12DEC1430/0158232873876/HLD1234
";

			var expectedMessageInterpretation = @"Masterbill: 081-11223344
Housebill: HAWB123
Package Tracking ID: 123456

Flight No.: KL325
Scheduled Arrival Date: 24-Dec
Arrival Airport: MIA

CBP Status Notification
1C - Entry processed: CBP general examination. - for 20 pieces
Remarks: HLD1234

ARR/KL325/24DEC-A
CSN/1C-20/12DEC1430/0158232873876/HLD1234


FSN
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/24DEC-A
CSN/1C-20/12DEC1430/0158232873876/HLD1234";

			AssertMultilineASCIIEquals("EM_MessageInterpretation should be output as expected.", expectedMessageInterpretation, message.EM_MessageInterpretation);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSAirDispositionCode, "AMSAD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1C", "Entry processed: CBP general examination.", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1S", "CBP Eligible for General Order.", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "12", "In-bond Arrival at Dest by BOL Received (specific BOL number-such as via ASN3)", startDate, endDate);
			Factory.Save();
		}
	}
}
