using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class BIRDEntrySummaryQueryResponseMessageBuilderTest : TestCaseWithFactory
	{
		public void TestBuildMessage()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryQuery;
			message.EM_MessageNum = "";
			message.EM_MessageText = "B018888XJ5JR                                               58                   " +
				"J18888XJ5 0000007191-013199000010000000000000000000000            051407002     " +
				"J2XJ5 00000071      0000000000000000000000                               808001 " +
				"J3XJ5 00000071                                  891300107150000000000008        " +
				"J5XJ5 00000113071120                                                            " +
				"J98888XJ5 00000071BILLING DATA NOT ON FILE                                      " +
				"J98888XJ5 00000071COLLECTION DATA NOT ON FILE                                   " +
				"Y  8888XJ5JR00006";
			message.EM_Status = MQEDIMessage.Status.Received;
			message.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			BIRDEntrySummaryQueryResponseMessageBuilder builder = new BIRDEntrySummaryQueryResponseMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			MQEDIMessage message2 = builder.PopulateMessage();

			AssertNoExceptionThrown(() => _ = message2.MessageBlock);

			AssertContains("J18888XJ5 0000007191-01319900001                                  051407002  0  ", message2.EM_MessageText);
			AssertContains("J2XJ5 00000071                                                           808001 ", message2.EM_MessageText);
			AssertContains("J3XJ5 00000071                                  891300107150           8        ", message2.EM_MessageText);
			AssertContains("J5XJ5 00000113071120                                                            ", message2.EM_MessageText);
			AssertContains("J98888XJ5 00000071BILLING DATA NOT ON FILE                                      ", message2.EM_MessageText);
			AssertContains("J98888XJ5 00000071COLLECTION DATA NOT ON FILE                                   ", message2.EM_MessageText);

			AssertEquals(EDIMessage.Status.Acknowledged, message2.EM_Status);
		}
	}
}
