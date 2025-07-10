using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ABIOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		[TestDate(2009, 12, 1)]
		public void TestOutgoingMessagesPackageIntoInterchanges()
		{
			DeclarationTestHelper.SetupForSendMessage();
			APLA a = new APLA();
			a.ReceiverFilerCode = "XJ5";
			APLB b = new APLB();
			b.UserData = MQEDIMessage.MessageNumberPlaceHolder;
			ZString messageText = a.Serialise() + b.Serialise() + " B";
			MQEDIMessage message1 = Factory.New<MQEDIMessage>();
			message1.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message1.EM_Status = MQEDIMessage.Status.Queued;
			message1.EM_MessageText = messageText;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			AssertNull(message1.Interchange);

			MQEDIMessage message2 = Factory.New<MQEDIMessage>();
			message2.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message2.EM_Status = MQEDIMessage.Status.Queued;
			message2.EM_MessageText = messageText;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			AssertNull(message2.Interchange);

			MQEDIMessage message3 = Factory.New<MQEDIMessage>();
			message3.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message3.EM_Status = MQEDIMessage.Status.Queued;
			message3.EM_MessageText = messageText;
			message3.EM_MessageType = ApplicationIdentifierCodeList.Codes.ElectronicInvoice;
			AssertNull(message3.Interchange);

			MQEDIMessage message4 = Factory.New<MQEDIMessage>();
			message4.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message4.EM_Status = MQEDIMessage.Status.Queued;
			message4.EM_MessageText = messageText;
			message4.EM_MessageType = ApplicationIdentifierCodeList.Codes.BorderCargoRelease;
			AssertNull(message4.Interchange);

			Factory.Save();

			ABIOutgoingMessageProcessor processor = new ABIOutgoingMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			AssertMessage(message1, "1");
			AssertMessage(message2, "2");
			AssertMessage(message3, "3");
			AssertMessage(message4, "4");
		}

		[TestDate(2010, 08, 30)]
		public void TestOutgoingMessagesSortIntoAppropriateInterchangeOrder()
		{
			DeclarationTestHelper.SetupForSendMessage();
			APLA a = new APLA();
			a.ReceiverFilerCode = "XJ5";
			APLB b = new APLB();
			b.UserData = MQEDIMessage.MessageNumberPlaceHolder;
			ZString messageText = a.Serialise() + b.Serialise() + " B";

			MQEDIMessage aiiMsg = Factory.New<MQEDIMessage>();
			aiiMsg.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			aiiMsg.EM_Status = MQEDIMessage.Status.Queued;
			aiiMsg.EM_MessageText = messageText;
			aiiMsg.EM_MessageType = ApplicationIdentifierCodeList.Codes.ElectronicInvoice;
			AssertNull(aiiMsg.Interchange);

			MQEDIMessage ensMsg = Factory.New<MQEDIMessage>();
			ensMsg.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			ensMsg.EM_Status = MQEDIMessage.Status.Queued;
			ensMsg.EM_MessageText = messageText;
			ensMsg.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			AssertNull(ensMsg.Interchange);

			Factory.Save();

			ABIOutgoingMessageProcessor processor = new ABIOutgoingMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			AssertMessage(aiiMsg, "1");
			AssertMessage(ensMsg, "2");
		}

		[TestDate(2010, 1, 2, 3, 4, 5)]
		public void TestOutgoingMessagesSortIntoAppropriateInterchangeOrderComplex()
		{
			DeclarationTestHelper.SetupForSendMessage();
			APLA a = new APLA();
			a.ReceiverFilerCode = "XJ5";
			APLB b = new APLB();
			b.UserData = MQEDIMessage.MessageNumberPlaceHolder;
			ZString messageText = a.Serialise() + b.Serialise() + " B";
			MQEDIMessage message1 = Factory.New<MQEDIMessage>();
			message1.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message1.EM_Status = MQEDIMessage.Status.Queued;
			message1.EM_MessageText = messageText;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			AssertNull(message1.Interchange);

			MQEDIMessage message2 = Factory.New<MQEDIMessage>();
			message2.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message2.EM_Status = MQEDIMessage.Status.Queued;
			message2.EM_MessageText = messageText;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			AssertNull(message2.Interchange);

			MQEDIMessage message3 = Factory.New<MQEDIMessage>();
			message3.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message3.EM_Status = MQEDIMessage.Status.Queued;
			message3.EM_MessageText = messageText;
			message3.EM_MessageType = ApplicationIdentifierCodeList.Codes.ElectronicInvoice;
			AssertNull(message3.Interchange);
			Factory.Save();

			MQEDIMessage message4 = Factory.New<MQEDIMessage>();
			message4.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message4.EM_Status = MQEDIMessage.Status.Queued;
			message4.EM_MessageText = messageText;
			message4.EM_MessageType = ApplicationIdentifierCodeList.Codes.BorderCargoRelease;
			AssertNull(message4.Interchange);

			MQEDIMessage message5 = Factory.New<MQEDIMessage>();
			message5.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message5.EM_Status = MQEDIMessage.Status.Queued;
			message5.EM_MessageText = messageText;
			message5.EM_MessageType = ApplicationIdentifierCodeList.Codes.ElectronicInvoice;
			AssertNull(message5.Interchange);
			Factory.Save();

			MQEDIMessage message6 = Factory.New<MQEDIMessage>();
			message6.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message6.EM_Status = MQEDIMessage.Status.Queued;
			message6.EM_MessageText = messageText;
			message6.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			AssertNull(message6.Interchange);
			Factory.Save();

			MQEDIMessage message7 = Factory.New<MQEDIMessage>();
			message7.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message7.EM_Status = MQEDIMessage.Status.Queued;
			message7.EM_MessageText = messageText;
			message7.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions;
			AssertNull(message7.Interchange);

			MQEDIMessage message8 = Factory.New<MQEDIMessage>();
			message8.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message8.EM_Status = MQEDIMessage.Status.Queued;
			message8.EM_MessageText = messageText;
			message8.EM_MessageType = ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQuery;
			AssertNull(message8.Interchange);
			Factory.Save();

			MQEDIMessage message9 = Factory.New<MQEDIMessage>();
			message9.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message9.EM_Status = MQEDIMessage.Status.Queued;
			message9.EM_MessageText = messageText;
			message9.EM_MessageType = ApplicationIdentifierCodeList.Codes.ElectronicInvoice;
			AssertNull(message9.Interchange);
			Factory.Save();

			MQEDIMessage message10 = Factory.New<MQEDIMessage>();
			message10.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message10.EM_Status = MQEDIMessage.Status.Queued;
			message10.EM_MessageText = messageText;
			message10.EM_MessageType = ApplicationIdentifierCodeList.Codes.ElectronicInvoice;
			AssertNull(message10.Interchange);
			Factory.Save();

			MQEDIMessage message11 = Factory.New<MQEDIMessage>();
			message11.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message11.EM_Status = MQEDIMessage.Status.Queued;
			message11.EM_MessageText = messageText;
			message11.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			AssertNull(message11.Interchange);
			Factory.Save();

			message1.EM_MessageNum = "MSG01";
			message2.EM_MessageNum = "MSG02";
			message3.EM_MessageNum = "MSG03";
			message4.EM_MessageNum = "MSG04";
			message5.EM_MessageNum = "MSG05";
			message6.EM_MessageNum = "MSG06";
			message7.EM_MessageNum = "MSG07";
			message8.EM_MessageNum = "MSG08";
			message9.EM_MessageNum = "MSG09";
			message10.EM_MessageNum = "MSG10";
			message11.EM_MessageNum = "MSG11";
			Factory.Save();

			ABIOutgoingMessageProcessor processor = new ABIOutgoingMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			// Order of interchanges generated should be Create time first then by Message Number order
			AssertMessage(message1, "1");
			AssertMessage(message2, "2");
			AssertMessage(message3, "3");
			AssertMessage(message4, "4");
			AssertMessage(message5, "5");
			AssertMessage(message6, "6");
			AssertMessage(message7, "7");
			AssertMessage(message8, "8");
			AssertMessage(message9, "9");
			AssertMessage(message10, "10");
			AssertMessage(message11, "11");
		}

		[TestDate(2009, 12, 1)]
		public void TestISFMessagesAreExcluded()
		{
			DeclarationTestHelper.SetupForSendMessage();
			APLA a = new APLA();
			a.ReceiverFilerCode = "XJ5";
			APLB b = new APLB();
			b.UserData = MQEDIMessage.MessageNumberPlaceHolder;
			ZString messageText = a.Serialise() + b.Serialise() + " B";
			MQEDIMessage message1 = Factory.New<MQEDIMessage>();
			message1.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message1.EM_Status = MQEDIMessage.Status.Queued;
			message1.EM_MessageText = messageText;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			AssertNull(message1.Interchange);
			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_Status = MQEDIMessage.Status.Queued;
			message2.EM_MessageText = messageText;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			AssertNull(message2.Interchange);
			Factory.Save();
			var processor = new ABIOutgoingMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			AssertMessage(message1, "1");
			AssertNull(message2.Interchange);
			AssertEquals(MQEDIMessage.Status.Queued, message2.EM_Status);
		}

		[TestDate(2009, 12, 1)]
		public void TestEDIMessageAreDiscardedForEmptyFilerID()
		{
			MQEDIMessage message1 = Factory.New<MQEDIMessage>();
			message1.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message1.EM_Status = MQEDIMessage.Status.Queued;
			message1.EM_MessageText = "";
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			AssertNull(message1.Interchange);
			Factory.Save();
			var processor = new ABIOutgoingMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);
			message1.Reload();

			AssertNull(message1.Interchange);
			AssertEquals(MQEDIMessage.Status.Discarded, message1.EM_Status);
			AssertContains("Message EDIEDIDAT_1 will be discarded for the following reason:\r\nThere is no Customs Interchange Sender ID set up\r\nfor Company - EDI (OrgProxy:EDICUS), Branch - BNE", processor.Logger.UserLogStrings[0]);
		}

		void AssertMessage(MQEDIMessage message, ZString interchangeNum)
		{
			message.Reload();
			var interchange = (CBPEDIInterchange)message.Interchange;
			AssertNotNull(interchange);
			AssertEquals(message, interchange.ContainedMessages[0]);
			AssertEquals(1, interchange.ContainedMessages.Count);
			AssertEquals(message.EM_MessageType, interchange.EI_InterchangeType);
			AssertEquals("Interchanges should be created in the required sort order with AII messages first", interchangeNum, interchange.EI_InterchangeNum);
		}
	}
}
