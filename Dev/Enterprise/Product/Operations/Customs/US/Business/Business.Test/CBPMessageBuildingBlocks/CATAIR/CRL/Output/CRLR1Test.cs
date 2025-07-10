using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Output.Testing
{
	sealed class CRLR1Test : TestCaseWithFactory
	{
		public void TestForwardedCargoReleaseProcessingResult()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.SetExternalBrokerForTesting();

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			MQEDIMessage incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "~15000";
			incomingMessage.EM_Status = EDIMessage.Status.Sent;
			incomingMessage.EM_MessageText = "B018888XJ5RR                                                                    R18888XJ5 010000480191-013199000B00001004FTRDADMIRALENGRACHT     1356 061307    R4            13465865424                         00001000KG   AAAD             R5062207100306ENTRY DOCUMENTS REQUIRED                                          Y018888XJ5RR00003";
			incomingMessage.EM_LinkedObject = declaration;

			declaration.Messages.Load();

			MQEDIMessage forwardedMessage = new BIRDStatusMessageBuilder(declaration).BuildTheLatestCargoProcessingResult();

			AssertEquals(2, declaration.Messages.Count);

			AssertNoExceptionThrown(() => _ = forwardedMessage.MessageBlock);
			AssertEquals(EDIMessage.Status.Pending, forwardedMessage.EM_Status);
		}
	}
}
