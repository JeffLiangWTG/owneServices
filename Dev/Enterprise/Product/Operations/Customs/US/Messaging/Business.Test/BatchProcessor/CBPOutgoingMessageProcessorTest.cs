using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class OutgoingMessageProcessorTest : TestCaseWithFactory
	{
		[TestDate(2009, 12, 1)]
		public void TestOutgoingMessagesPackageIntoInterchanges()
		{
			ZString bData = new ZZZB() { StringB = CBPMessageForTesting.MessageNumberPlaceHolder }.Serialise();
			ZString yData = new ZZZY().Serialise();
			ZString cData1 = new ZZZC() { StringC = "C1" }.Serialise();
			ZString cData2 = new ZZZC() { StringC = "C2" }.Serialise();
			ZString cData3 = new ZZZC() { StringC = "C3" }.Serialise();
			ZString cData4 = new ZZZC() { StringC = "C4" }.Serialise();
			CBPMessageForTesting message1 = Factory.New<CBPMessageForTesting>();
			message1.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message1.EM_Status = CBPMessageForTesting.Status.Queued;
			message1.EM_MessageText = bData + cData1 + yData;
			message1.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			AssertNull(message1.Interchange);

			CBPMessageForTesting message2 = Factory.New<CBPMessageForTesting>();
			message2.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message2.EM_Status = CBPMessageForTesting.Status.Queued;
			message2.EM_MessageText = bData + cData2 + yData;
			message2.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			AssertNull(message2.Interchange);

			CBPMessageForTesting message3 = Factory.New<CBPMessageForTesting>();
			message3.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message3.EM_Status = CBPMessageForTesting.Status.Queued;
			message3.EM_MessageText = bData + cData3 + yData;
			message3.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			AssertNull(message3.Interchange);

			CBPMessageForTesting message4 = Factory.New<CBPMessageForTesting>();
			message4.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message4.EM_Status = CBPMessageForTesting.Status.Queued;
			message4.EM_MessageText = bData + cData4 + yData;
			message4.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			AssertNull(message4.Interchange);
			Factory.Save();

			message1.EM_MessageNum = "MSG1";
			message2.EM_MessageNum = "MSG2";
			message3.EM_MessageNum = "MSG3";
			message4.EM_MessageNum = "MSG4";
			Factory.Save();

			OutgoingMessageProcessorForTesting processor = new OutgoingMessageProcessorForTesting(new LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			AssertMessage(message1, "1");
			AssertMessage(message2, "2");
			AssertMessage(message3, "3");
			AssertMessage(message4, "4");
		}

		[TestDate(2010, 08, 30)]
		public void TestOutgoingMessagesSortIntoAppropriateInterchangeOrder()
		{
			ZString bData = new ZZZB() { StringB = CBPMessageForTesting.MessageNumberPlaceHolder }.Serialise();
			ZString yData = new ZZZY().Serialise();
			ZString cData1 = new ZZZC() { StringC = "C1" }.Serialise();
			ZString cData2 = new ZZZC() { StringC = "C2" }.Serialise();
			ZString cData3 = new ZZZC() { StringC = "C3" }.Serialise();
			ZString cData4 = new ZZZC() { StringC = "C4" }.Serialise();

			CBPMessageForTesting msg1 = Factory.New<CBPMessageForTesting>();
			msg1.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			msg1.EM_Status = CBPMessageForTesting.Status.Queued;
			msg1.EM_MessageText = bData + cData1 + cData2 + yData;
			msg1.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			AssertNull(msg1.Interchange);

			CBPMessageForTesting msg2 = Factory.New<CBPMessageForTesting>();
			msg2.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			msg2.EM_Status = CBPMessageForTesting.Status.Queued;
			msg2.EM_MessageText = bData + cData3 + cData4 + yData;
			msg2.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting2;
			AssertNull(msg2.Interchange);

			Factory.Save();

			msg1.EM_MessageNum = "MSG1";
			msg2.EM_MessageNum = "MSG2";
			Factory.Save();

			OutgoingMessageProcessorForTesting processor = new OutgoingMessageProcessorForTesting(new LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			AssertMessage(msg1, "1");
			AssertMessage(msg2, "2");
		}

		public void TestOutgoingMessagesSortIntoAppropriateInterchangeOrderComplex()
		{
			ZString bData = new ZZZB() { StringB = CBPMessageForTesting.MessageNumberPlaceHolder }.Serialise();
			ZString yData = new ZZZY().Serialise();
			ZString cData1 = new ZZZC() { StringC = "C1" }.Serialise();
			ZString cData2 = new ZZZC() { StringC = "C2" }.Serialise();
			ZString cData3 = new ZZZC() { StringC = "C3" }.Serialise();
			ZString cData4 = new ZZZC() { StringC = "C4" }.Serialise();
			var date = ZDateTime.Now;
			CBPMessageForTesting message1 = Factory.New<CBPMessageForTesting>();
			message1.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message1.EM_Status = CBPMessageForTesting.Status.Queued;
			message1.EM_MessageText = bData + cData1 + yData;
			message1.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			message1.EM_SystemCreateTimeUtc = date;
			AssertNull(message1.Interchange);

			CBPMessageForTesting message2 = Factory.New<CBPMessageForTesting>();
			message2.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message2.EM_Status = CBPMessageForTesting.Status.Queued;
			message2.EM_MessageText = bData + cData2 + yData;
			message2.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting2;
			message2.EM_SystemCreateTimeUtc = date;
			AssertNull(message2.Interchange);

			CBPMessageForTesting message3 = Factory.New<CBPMessageForTesting>();
			message3.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message3.EM_Status = CBPMessageForTesting.Status.Queued;
			message3.EM_MessageText = bData + cData3 + yData;
			message3.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			message3.EM_SystemCreateTimeUtc = date;
			AssertNull(message3.Interchange);

			date = date.AddSeconds(1);
			CBPMessageForTesting message4 = Factory.New<CBPMessageForTesting>();
			message4.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message4.EM_Status = CBPMessageForTesting.Status.Queued;
			message4.EM_MessageText = bData + cData4 + yData;
			message4.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting2;
			message4.EM_SystemCreateTimeUtc = date;
			AssertNull(message4.Interchange);

			CBPMessageForTesting message5 = Factory.New<CBPMessageForTesting>();
			message5.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message5.EM_Status = CBPMessageForTesting.Status.Queued;
			message5.EM_MessageText = bData + cData1 + cData1 + yData;
			message5.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			message5.EM_SystemCreateTimeUtc = date;
			AssertNull(message5.Interchange);

			date = date.AddSeconds(1);
			CBPMessageForTesting message6 = Factory.New<CBPMessageForTesting>();
			message6.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message6.EM_Status = CBPMessageForTesting.Status.Queued;
			message6.EM_MessageText = bData + cData1 + cData2 + yData;
			message6.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting2;
			message6.EM_SystemCreateTimeUtc = date;
			AssertNull(message6.Interchange);

			date = date.AddSeconds(1);
			CBPMessageForTesting message7 = Factory.New<CBPMessageForTesting>();
			message7.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message7.EM_Status = CBPMessageForTesting.Status.Queued;
			message7.EM_MessageText = bData + cData1 + cData3 + yData;
			message7.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			message7.EM_SystemCreateTimeUtc = date;
			AssertNull(message7.Interchange);

			CBPMessageForTesting message8 = Factory.New<CBPMessageForTesting>();
			message8.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message8.EM_Status = CBPMessageForTesting.Status.Queued;
			message8.EM_MessageText = bData + cData1 + cData4 + yData;
			message8.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting2;
			message8.EM_SystemCreateTimeUtc = date;
			AssertNull(message8.Interchange);

			date = date.AddSeconds(1);
			CBPMessageForTesting message9 = Factory.New<CBPMessageForTesting>();
			message9.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message9.EM_Status = CBPMessageForTesting.Status.Queued;
			message9.EM_MessageText = bData + cData2 + cData1 + yData;
			message9.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			message9.EM_SystemCreateTimeUtc = date;
			AssertNull(message9.Interchange);

			date = date.AddSeconds(1);
			CBPMessageForTesting message10 = Factory.New<CBPMessageForTesting>();
			message10.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message10.EM_Status = CBPMessageForTesting.Status.Queued;
			message10.EM_MessageText = bData + cData2 + cData2 + yData;
			message10.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting2;
			message10.EM_SystemCreateTimeUtc = date;
			AssertNull(message10.Interchange);

			date = date.AddSeconds(1);
			CBPMessageForTesting message11 = Factory.New<CBPMessageForTesting>();
			message11.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			message11.EM_Status = CBPMessageForTesting.Status.Queued;
			message11.EM_MessageText = bData + cData2 + cData3 + yData;
			message11.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			message11.EM_SystemCreateTimeUtc = date;
			AssertNull(message11.Interchange);
			Factory.Save();

			message1.EM_MessageNum = "MSG1";
			message2.EM_MessageNum = "MSG2";
			message3.EM_MessageNum = "MSG3";
			message4.EM_MessageNum = "MSG4";
			message5.EM_MessageNum = "MSG5";
			message6.EM_MessageNum = "MSG6";
			message7.EM_MessageNum = "MSG7";
			message8.EM_MessageNum = "MSG8";
			message9.EM_MessageNum = "MSG9";
			message10.EM_MessageNum = "MSG10";
			message11.EM_MessageNum = "MSG11";
			Factory.Save();

			OutgoingMessageProcessorForTesting processor = new OutgoingMessageProcessorForTesting(new LoggingInformation());
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

		void AssertMessage(CBPMessageForTesting message, ZString interchangeNum)
		{
			message.Reload();
			var interchange = message.Interchange;
			AssertNotNull(interchange);
			AssertEquals(message.PK, interchange.ContainedMessages[0].PK);
			AssertEquals(1, interchange.ContainedMessages.Count);
			AssertEquals(message.EM_MessageType, interchange.EI_InterchangeType);
			AssertEquals("Interchanges should be created in the required sort order", interchangeNum, interchange.EI_InterchangeNum);
		}

		sealed class OutgoingMessageProcessorForTesting : CBPOutgoingMessageProcessor
		{
			public OutgoingMessageProcessorForTesting(LoggingInformation logger)
				: base(logger)
			{
			}

			protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, CBPEDIInterchange.ApplicationCodeForTesting));
			ZQuery messageFilter;
		}
	}
}
