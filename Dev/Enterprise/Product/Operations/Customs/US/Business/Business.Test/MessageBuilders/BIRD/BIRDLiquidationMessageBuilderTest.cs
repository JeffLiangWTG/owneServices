using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class BIRDLiquidationMessageBuilderTest : TestCaseWithFactory
	{
		public void TestBuildMessage()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.SetExternalBrokerForTesting();

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusLiquidation liquidation = Factory.New<CusLiquidation>();
			liquidation.B8_JE = declaration.PK;

			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.LiquidationNotice;
			message.EM_MessageNum = "";
			message.EM_MessageText = @"B018888XJ5NR                                               58                   N12222XJ5 1000241401NNN-NN-NNNN 000000137000000000563000000013700000000056301   N22222XJ5 1000241412030714BROKR-REFCBP_DOC_FILING_LOC6071407           000008247N42222XJ5 100024140000000010000000000200000000003000000000040000000000500       Y  8888XJ5NR00003";
			message.EM_Status = MQEDIMessage.Status.Received;
			message.EM_LinkedObject = liquidation;

			BIRDLiquidationMessageBuilder builder = new BIRDLiquidationMessageBuilder(declaration);
			MQEDIMessage message2 = builder.PopulateMessage();

			AssertNoExceptionThrown(() => _ = message2.MessageBlock);

			AssertContains("N12222XJ5 1000241401NNN-NN-NNNN 000000137000000000563000000013700000000056301   N22222XJ5 1000241412030714BROKR-REFCBP_DOC_FILING_LOC6071407           000008247N42222XJ5 100024140000000010000000000200000000003000000000040000000000500       ", message2.EM_MessageText);
			AssertEquals(EDIMessage.Direction.Transmit, message2.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Pending, message2.EM_Status);
		}
	}
}
