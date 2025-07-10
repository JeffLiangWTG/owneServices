using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	[TestedType(typeof(FERMessage))]
	sealed class FERMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadFERMessage()
		{
			var message = Factory.New<FERMessage>();
			AssertEquals(ApplicationCodeList.Codes.USAMA, message.EM_ApplicationCode);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals(Constants.AIMMessageSubTypes.FER, message.EM_MessageSubType);
			Factory.Save();

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertType<FERMessage>(reloadedMessage);

			AssertType<FERMessage>(GetNewBusinessObject());
		}

		[TestDate(2020, 12, 27)]
		public void TestFERMessageProperties()
		{
			var ferMessageText = @"FER
QF101/24DEC
081-11223344-HAWB123/123456
ERR/001ERRORDESCRIPTION
ERR/002ANOTHERERROR";

			var ferMessage = Factory.New<FERMessage>();
			ferMessage.EM_MessageText = ferMessageText;

			CombineAssertions(() =>
			{
				AssertEquals("FER", ferMessage.ComponentIdentifier);

				AssertEquals("QF101", ferMessage.FlightNumber);
				AssertEquals(new ZDate(2020, 12, 24), ferMessage.CalculatedArrivalDate);

				AssertEquals("081", ferMessage.AirWaybillPrefix);
				AssertEquals("11223344", ferMessage.AirWaybillSerialNumber);
				AssertEquals("HAWB123", ferMessage.HAWBNumber);
				AssertEquals("123456", ferMessage.PackageTrackingIdentifier);

				var errorMessages = ferMessage.Errors;
				AssertEquals("001", errorMessages[0].ErrorCode);
				AssertEquals("ERRORDESCRIPTION", errorMessages[0].ErrorMessageText);
				AssertEquals("002", errorMessages[1].ErrorCode);
				AssertEquals("ANOTHERERROR", errorMessages[1].ErrorMessageText);
			});
		}

		public void TestFERDoesNotShowEmptyLabels()
		{
			var message = Factory.New<FERMessage>();
			message.EM_MessageText = @"FER
ZI001/15MAR
439-21031614
ERR/000COMPLETE TRANSACTION REJECTED
ERR/040BILL DATA ON FILE MUST BE AMENDED BY FRC
";
			var expected = @"Masterbill: 439-21031614

000 - COMPLETE TRANSACTION REJECTED
040 - BILL DATA ON FILE MUST BE AMENDED BY FRC


FER
ZI001/15MAR
439-21031614
ERR/000COMPLETE TRANSACTION REJECTED
ERR/040BILL DATA ON FILE MUST BE AMENDED BY FRC
";
			AssertMultilineASCIIEquals("EM_MessageInterpretation should not include empty HAWB & Tracking ID.\r\nHousebill: and Tracking ID: labels should only be visible if they have an actual value.", expected, message.EM_MessageInterpretation);

			// only Housebill empty
			message = Factory.New<FERMessage>();
			message.EM_MessageText = @"FER
ZI001/15MAR
439-21031614- /0000010
ERR/000COMPLETE TRANSACTION REJECTED
ERR/040BILL DATA ON FILE MUST BE AMENDED BY FRC
";
			expected = @"Masterbill: 439-21031614
Tracking ID: 0000010

000 - COMPLETE TRANSACTION REJECTED
040 - BILL DATA ON FILE MUST BE AMENDED BY FRC


FER
ZI001/15MAR
439-21031614- /0000010
ERR/000COMPLETE TRANSACTION REJECTED
ERR/040BILL DATA ON FILE MUST BE AMENDED BY FRC
";
			AssertMultilineASCIIEquals("EM_MessageInterpretation should not have Housebill.", expected, message.EM_MessageInterpretation);

			// only Tracking ID empty
			message = Factory.New<FERMessage>();
			message.EM_MessageText = @"FER
ZI001/15MAR
439-21031614-BKG210316HB1
ERR/000COMPLETE TRANSACTION REJECTED
ERR/040BILL DATA ON FILE MUST BE AMENDED BY FRC
";
			expected = @"Masterbill: 439-21031614
Housebill: BKG210316HB1

000 - COMPLETE TRANSACTION REJECTED
040 - BILL DATA ON FILE MUST BE AMENDED BY FRC


FER
ZI001/15MAR
439-21031614-BKG210316HB1
ERR/000COMPLETE TRANSACTION REJECTED
ERR/040BILL DATA ON FILE MUST BE AMENDED BY FRC
";
			AssertMultilineASCIIEquals("EM_MessageInterpretation should not have Tracking ID.", expected, message.EM_MessageInterpretation);
		}

		[TestDate(2020, 12, 27)]
		public void TestFERMessageProperties_MinimumMessage()
		{
			var ferMessageText = @"FER
/24DEC
--/
ERR/001ERRORDESCRIPTION";

			var ferMessage = Factory.New<FERMessage>();
			ferMessage.EM_MessageText = ferMessageText;

			CombineAssertions(() =>
			{
				AssertEquals("FER", ferMessage.ComponentIdentifier);

				AssertEquals("", ferMessage.FlightNumber);
				AssertEquals(new ZDate(2020, 12, 24), ferMessage.CalculatedArrivalDate);

				AssertEquals("", ferMessage.AirWaybillPrefix);
				AssertEquals("", ferMessage.AirWaybillSerialNumber);
				AssertEquals("", ferMessage.HAWBNumber);
				AssertEquals("", ferMessage.PackageTrackingIdentifier);

				var errorMessages = ferMessage.Errors;
				AssertEquals("001", errorMessages[0].ErrorCode);
				AssertEquals("ERRORDESCRIPTION", errorMessages[0].ErrorMessageText);
				AssertEquals("Error Message Count", 1, errorMessages.Count);
			});
		}

		[TestDate(2020, 12, 27)]
		public void TestArrivalDate_NextYear()
		{
			var ferMessageText = @"FER
QF101/02JAN
081-11223344-HAWB123/123456
ERR/001ERRORDESCRIPTION
ERR/002ANOTHERERROR
";

			var ferMessage = Factory.New<FERMessage>();
			ferMessage.EM_MessageText = ferMessageText;
			AssertEquals(new ZDate(2021, 1, 2), ferMessage.CalculatedArrivalDate);
		}

		public void TestFERMessageProperties_ConsolidationIdentifier()
		{
			var ferMessageText = @"FER
QF101/24DEC
081-11223344-M/123456
ERR/001ERRORDESCRIPTION
ERR/002ANOTHERERROR
";

			var ferMessage = Factory.New<FERMessage>();
			ferMessage.EM_MessageText = ferMessageText;

			CombineAssertions(() =>
			{
				AssertEquals("081", ferMessage.AirWaybillPrefix);
				AssertEquals("11223344", ferMessage.AirWaybillSerialNumber);
				AssertEquals("", ferMessage.HAWBNumber);
				AssertEquals("123456", ferMessage.PackageTrackingIdentifier);
			});
		}

		public void TestEM_MessageInterpretation()
		{
			var message = Factory.New<FERMessage>();
			message.EM_MessageText = @"FER
ZI001/15MAR
439-21031614-BKG210316HB1/0000010
ERR/000COMPLETE TRANSACTION REJECTED
ERR/040BILL DATA ON FILE MUST BE AMENDED BY FRC
";
			var expected = @"Masterbill: 439-21031614
Housebill: BKG210316HB1
Tracking ID: 0000010

000 - COMPLETE TRANSACTION REJECTED
040 - BILL DATA ON FILE MUST BE AMENDED BY FRC


FER
ZI001/15MAR
439-21031614-BKG210316HB1/0000010
ERR/000COMPLETE TRANSACTION REJECTED
ERR/040BILL DATA ON FILE MUST BE AMENDED BY FRC
";
			AssertMultilineASCIIEquals("EM_MessageInterpretation should be output as expected.", expected, message.EM_MessageInterpretation);
		}
	}
}
