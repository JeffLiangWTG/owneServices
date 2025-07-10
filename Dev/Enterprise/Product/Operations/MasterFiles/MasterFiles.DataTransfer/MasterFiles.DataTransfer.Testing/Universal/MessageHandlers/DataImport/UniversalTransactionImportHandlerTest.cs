using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Testing.Universal.MessageHandlers
{
	class UniversalTransactionImportHandlerTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			var messagesQuery = new ZQuery();
			messagesQuery.OrderBy = EDIMessageSchema.EM_ApplicationCode.Name + ", " + EDIMessageSchema.EM_ReceiveTransmit.Name;

			var messages = Factory.Load<EDIMessage>(messagesQuery);
			AssertEquals("Precondition: No existing messages", 0, messages.Length);

			const string inboundXml = @"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
  </TransactionInfo>
</UniversalTransaction>";

			var handler = new UniversalTransactionImportHandler(new XmlSessionTracker(new SimpleLogger()));
			var request = handler.CreateRequestMessage();
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(inboundXml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			using ((request as BusinessObject).Factory.AddDisposableService())
			using (var processingResult = handler.Process(request))
			{
				request.Save();
				AssertNotNull(processingResult);
				AssertEquals("PRS", processingResult.Status);

				messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("Should be one message for request", 1, messages.Length);

				CombineAssertions("Message 1", () =>
				{
					var message = messages[0];
					AssertEquals(ApplicationCodeList.Codes.UniversalDataQuery, message.EM_ApplicationCode);
					AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
				});
			}
		}
	}
}
