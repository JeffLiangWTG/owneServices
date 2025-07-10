using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.SARSEDIMessage;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(EDIMessageCollection))]
	sealed class EDIMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestEDIMessageCollection()
		{
			AssertEquals(typeof(EDIMessageCollection), EntryHeader.Messages.GetType());
		}

		public void TestGetMatchingMessages()
		{
			var messages = EntryHeader.Messages;
			AssertEquals(2, messages.GetMatchingMessages(MessageTypes.CUSDEC, Direction.Transmit, null).Count());
			AssertEquals(0, messages.GetMatchingMessages(MessageTypes.CUSRES, Direction.Transmit, null).Count());
			AssertEquals(1, messages.GetMatchingMessages(MessageTypes.CUSRES, Direction.Receive, null).Count());
			AssertEquals(1, messages.GetMatchingMessages(MessageTypes.CONTRL, Direction.Receive, null).Count());
			AssertEquals(1, messages.GetMatchingMessages(MessageTypes.CUSDEC, Direction.Transmit, (x) => x.EM_MessageSubType == "CHG").Count());
			AssertEquals(2, messages.GetMatchingMessages(MessageTypes.CUSDEC, Direction.Transmit, (x) => x.EM_MessageSubType != "").Count());
			AssertEquals(0, messages.GetMatchingMessages(MessageTypes.CUSDEC, Direction.Transmit, (x) => x.EM_MessageSubType == "XXX").Count());
		}

		public void TestGetIncomingMessages()
		{
			var messages = EntryHeader.Messages;
			AssertEquals("Only CUSRES message", 1, messages.GetIncomingMessages(MessageTypes.CUSRES).Length);
			AssertEquals("CUSRES and CONTRL messages", 2, messages.GetIncomingMessages(MessageTypes.CUSRES, MessageTypes.CONTRL).Length);
			AssertEquals("Outgoing messages are excluded", 0, messages.GetIncomingMessages(MessageTypes.CUSDEC).Length);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<ZAMessage>();

		protected override BusinessObjectCollection GetCollectionToTest() => new EDIMessageCollection(EntryHeader);

		CusEntryHeader EntryHeader => entryHeader ??= CreateCusEntryHeader();

		CusEntryHeader entryHeader;

		CusEntryHeader CreateCusEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var msg1 = header.Messages.AddNew();
			msg1.EM_ApplicationCode = ApplicationCodes.SouthAfricanCustoms;
			msg1.EM_MessageType = MessageTypes.CUSDEC;
			msg1.EM_ReceiveTransmit = Direction.Transmit;
			msg1.EM_MessageSubType = "CHG";
			var msg2 = header.Messages.AddNew();
			msg1.EM_ApplicationCode = ApplicationCodes.SouthAfricanCustoms;
			msg2.EM_MessageType = MessageTypes.CUSDEC;
			msg2.EM_ReceiveTransmit = Direction.Transmit;
			msg2.EM_MessageSubType = "CNL";
			var msg3 = header.Messages.AddNew();
			msg3.EM_ApplicationCode = ApplicationCodes.SouthAfricanCustoms;
			msg3.EM_MessageType = MessageTypes.CUSRES;
			msg3.EM_ReceiveTransmit = Direction.Receive;
			msg3.EM_MessageSubType = "XXX";
			var msg4 = header.Messages.AddNew();
			msg4.EM_ApplicationCode = ApplicationCodes.SouthAfricanCustoms;
			msg4.EM_MessageType = MessageTypes.CONTRL;
			msg4.EM_ReceiveTransmit = Direction.Receive;
			msg4.EM_MessageSubType = "XXX";
			return header;
		}
	}
}
