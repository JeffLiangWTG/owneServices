using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ETradeTemporaryRegistrationMessageProcessorTest : TestCaseWithFactory
	{
		public void TestUpdateCustomsStatusAndMessageModeWhenProcessMessage()
		{
			var errorMessageText = TRMessageTestHelper.GetFileText("ETradeError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var errorETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			var errorMessage = CreateETradeMessage(errorMessageText, errorETradeHeader.PK, TRMessageTypes.Codes.TRE);

			Processor.ProcessMessage(errorMessage);

			AssertEquals(CustomsStatusList.Codes.TRR, errorETradeHeader.RegistrationStatus);
			AssertEquals(TRMessageTypes.Codes.TRE, errorETradeHeader.MessageMode);
			AssertEquals(TRMessageStatusCodeList.Codes.Error, ((IMessageAttachee)errorETradeHeader).MessageStatus);

			var successMessageText = TRMessageTestHelper.GetFileText("ETradeSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var importETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			importETradeHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var importSuccessMessage = CreateETradeMessage(successMessageText, importETradeHeader.PK, TRMessageTypes.Codes.TRE);

			Processor.ProcessMessage(importSuccessMessage);

			AssertEquals(CustomsStatusList.Codes.TRS, importETradeHeader.RegistrationStatus);
			AssertEquals(TRMessageTypes.Codes.TRQ, importETradeHeader.MessageMode);
			AssertEquals(TRMessageStatusCodeList.Codes.Accepted, ((IMessageAttachee)importETradeHeader).MessageStatus);
			var expectedInterpretation2 = TRMessageTestHelper.GetFileText("ETrade.ETradeSuccess.htm");
			AssertEquals("EM_MessageInterpretation", expectedInterpretation2, importSuccessMessage.EM_MessageInterpretation);

			var exportETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			exportETradeHeader.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var exportSuccessMessage = CreateETradeMessage(successMessageText, exportETradeHeader.PK, TRMessageTypes.Codes.TRE);

			Processor.ProcessMessage(exportSuccessMessage);

			AssertEquals(CustomsStatusList.Codes.TRS, exportETradeHeader.RegistrationStatus);
			AssertEquals(TRMessageTypes.Codes.TRS, exportETradeHeader.MessageMode);
			AssertEquals(TRMessageStatusCodeList.Codes.Accepted, ((IMessageAttachee)exportETradeHeader).MessageStatus);
			var expectedInterpretation3 = TRMessageTestHelper.GetFileText("ETrade.ETradeSuccess.htm");
			AssertEquals("EM_MessageInterpretation", expectedInterpretation3, exportSuccessMessage.EM_MessageInterpretation);
		}

		public void TestCreateCusPollingTransactionForAutoResponse()
		{
			var messageText = TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.TREResponse.xml");
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETG0000001";
			var message = CreateETradeMessage(messageText, header.PK, TRMessageTypes.Codes.TRE);
			Factory.Save();

			Processor.ProcessMessage(message);

			var guid = TRMessageHelper.GetNodeValue(messageText, new ZString[] { "Envelope", "Body", "OutputMessage", "Record", "Sonuc" }, "GUID");
			var pollingTransaction = Factory.LoadTop1<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_ParentID, message.PK));
			CombineAssertions("A CusPollingTransaction for auto response should be created", () =>
			{
				AssertEquals("CPT_Type", TRMessageTypes.Codes.TRE, pollingTransaction.CPT_Type);
				AssertEquals("CPT_TransactionID", guid, pollingTransaction.CPT_TransactionID);
			});
		}

		public void TestReadTREResponseWithFormattedInterpretation()
		{
			var messageText = TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.TREResponse.xml");
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETG0000001";
			var message = CreateETradeMessage(messageText, header.PK, TRMessageTypes.Codes.TRE);

			Processor.ProcessMessage(message);

			var guid = "59d87fc7-59cb-4e85-bec3-a8bde10fbab3";

			AssertEquals("An E-Trade auto receive response message that contains the guid should be created", true, ((ETradeEDIMessage)header.Messages[0]).EM_MessageText.Contains(guid));
			Assert("EM_MessageInterpretation should contains title", message.EM_MessageInterpretation.Contains("has been cleared"));
			Assert("EM_MessageInterpretation should contains 'Query GUID'", message.EM_MessageInterpretation.Contains("<td>Query GUID:</td><td>59d87fc7-59cb-4e85-bec3-a8bde10fbab3</td>"));
		}

		public void TestUpdateCusPollingTransaction_Close()
		{
			var testData = CreateTestData(TRMessageTestHelper.GetFileText("ETradeSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade."));
			Factory.Save();

			var t1eResponseMessage = testData.t1eResponseMessage;
			Processor.ProcessMessage(t1eResponseMessage);

			var pollingTransaction = testData.pollingTransaction;
			CombineAssertions(() =>
			{
				AssertEquals("CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.CLS, pollingTransaction.CPT_Status);
				AssertEquals("CPT_StatusReason", "Close", pollingTransaction.CPT_StatusReason);
				AssertEquals("CPT_StatusTimeUtc", t1eResponseMessage.EM_SystemCreateTimeUtc, pollingTransaction.CPT_StatusTimeUtc);
			});
		}

		public void TestUpdateCusPollingTransaction_Open()
		{
			var testData = CreateTestData(ZString.Empty);
			var pollingTransaction = testData.pollingTransaction;
			pollingTransaction.CPT_NumberOfAttempts = 3;
			pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = new ZDateTime(2022, 05, 19, 10, 15, 30);
			Factory.Save();

			var t1eResponseMessage = testData.t1eResponseMessage;
			Processor.ProcessMessage(t1eResponseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("CPT_NumberOfAttempts", (ZByte)2, pollingTransaction.CPT_NumberOfAttempts);
				AssertEquals("CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN, pollingTransaction.CPT_Status);
				AssertEquals("CPT_StatusReason", "Open", pollingTransaction.CPT_StatusReason);
				AssertEquals("CPT_StatusTimeUtc", t1eResponseMessage.EM_SystemCreateTimeUtc, pollingTransaction.CPT_StatusTimeUtc);
				AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", new ZDateTime(2022, 05, 19, 10, 20, 30), pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);
			});
		}

		public void TestUpdateCusPollingTransaction_Error()
		{
			var testData = CreateTestData(ZString.Empty);
			var pollingTransaction = testData.pollingTransaction;
			pollingTransaction.CPT_NumberOfAttempts = 1;
			Factory.Save();

			var t1eResponseMessage = testData.t1eResponseMessage;
			Processor.ProcessMessage(t1eResponseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.ERR, pollingTransaction.CPT_Status);
				AssertEquals("CPT_StatusReason", "Error", pollingTransaction.CPT_StatusReason);
				AssertEquals("CPT_StatusTimeUtc", t1eResponseMessage.EM_SystemCreateTimeUtc, pollingTransaction.CPT_StatusTimeUtc);
			});
		}

		public void TestT1EErrorMessagePresentation()
		{
			var errorMessageText = TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.T1EErrorMessageResponse.xml");
			var errorETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			errorETradeHeader.AMA_JobReference = "ETR001";

			var errorMessage = CreateETradeMessage(errorMessageText, errorETradeHeader.PK, TRMessageTypes.Codes.T1E);

			Processor.ProcessMessage(errorMessage);

			CombineAssertions(() =>
			{
				var expected = TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.T1EErrorMessageResponse.htm");
				AssertEquals(expected, errorMessage.EM_MessageInterpretation);
			});
		}

		public void TestT1ESuccessMessageResponse()
		{
			var successMessageText = TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.T1ESuccessMessageResponse.xml");

			var parentNodeListSuccess = new List<ZString>() { "ETicaretSoapOut", "Record", "Sonuc" };
			ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/" };
			var xmlData = Enterprise.Customs.TR.Messaging.TRMessageHelper.GetNodeValue(successMessageText, "/A:Envelope/A:Body/B:ServisCevabiSorgulamaResponse/B:ServisCevabiSorgulamaResult", namespaceList);
			var tempRegNo = TRMessageHelper.GetNodeValue(xmlData, parentNodeListSuccess, "KayitNo");
			var tempRegDate = TRMessageHelper.GetNodeValue(xmlData, parentNodeListSuccess, "KayitTarihi");

			var successETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			successETradeHeader.AMA_JobReference = "ETR001";

			var successMessage = CreateETradeMessage(successMessageText, successETradeHeader.PK, TRMessageTypes.Codes.T1E);

			Processor.ProcessMessage(successMessage);

			CombineAssertions(() =>
			{
				AssertEquals("Temporary Registration Number", tempRegNo, successETradeHeader.TempRegNo);
				AssertEquals("Temporary Registration Date", new ZDateTime(tempRegDate).ToString("yyyyMMddhhmm"), successETradeHeader.TempRegNoDate.ToString("yyyyMMddhhmm"));
				var expected = TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.T1ESuccessMessageResponse.htm");
				AssertEquals(expected, successMessage.EM_MessageInterpretation);
			});
		}

		public void TestT1SErrorMessagePresentation()
		{
			var errorMessageText = TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.T1SErrorMessageResponse.xml");
			var errorETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			errorETradeHeader.AMA_JobReference = "ETR001";

			var errorMessage = CreateETradeMessage(errorMessageText, errorETradeHeader.PK, TRMessageTypes.Codes.T1E);

			Processor.ProcessMessage(errorMessage);

			CombineAssertions(() =>
			{
				var expected = TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.T1SErrorMessageResponse.htm");
				AssertEquals(expected, errorMessage.EM_MessageInterpretation);
			});
		}

		public void TestT1DErrorMessagePresentation()
		{
			var errorMessageText = TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.T1DErrorMessageResponse.xml");
			var errorETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			errorETradeHeader.AMA_JobReference = "ETR001";

			var errorMessage = CreateETradeMessage(errorMessageText, errorETradeHeader.PK, TRMessageTypes.Codes.T1E);

			Processor.ProcessMessage(errorMessage);

			CombineAssertions(() =>
			{
				var expected = TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.T1DErrorMessageResponse.htm");
				AssertEquals(expected, errorMessage.EM_MessageInterpretation);
			});
		}

		public void TestT2DErrorMessagePresentation()
		{
			var errorMessageText = TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.T2DErrorMessageResponse.xml");
			var errorETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			errorETradeHeader.AMA_JobReference = "ETR001";

			var errorMessage = CreateETradeMessage(errorMessageText, errorETradeHeader.PK, TRMessageTypes.Codes.T1E);

			Processor.ProcessMessage(errorMessage);

			CombineAssertions(() =>
			{
				var expected = TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.T2DErrorMessageResponse.htm");
				AssertEquals(expected, errorMessage.EM_MessageInterpretation);
			});
		}

		public void TestErrorResponseInOutputMessage()
		{
			var messageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
		<OutputMessage xmlns=""http://GumrukETApp.OutputMessageSchema"">
			<Record>
				<Sonuc>
					<Durum>Elektronik İmza ile kullanıcı kodu üzerindeki bilgiler uyumsuz.</Durum>
				</Sonuc>
			</Record>
		</OutputMessage>
	</s:Body>
</s:Envelope>";
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETG0000001";
			var message = CreateETradeMessage(messageText, header.PK, TRMessageTypes.Codes.TRE);
			Factory.Save();

			Processor.ProcessMessage(message);

			AssertContains(
				"EM_MessageInterpretation should contain the error message line.",
				"<tr><td>Error Message: </td><td>Elektronik İmza ile kullanıcı kodu &#252;zerindeki bilgiler uyumsuz.</td></tr>",
				message.EM_MessageInterpretation
			);
		}

		(CusPollingTransaction pollingTransaction, ETradeEDIMessage t1eResponseMessage) CreateTestData(ZString t1eResponseMessageText)
		{
			var factory = Factory;
			var treSessionID = new ZGuid("AF48DDBA-4CBA-44EF-B64A-CC27D240B9F8");
			var linkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			var eTradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			var treRequestInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRE, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, treSessionID, "TRE Request message");
			var treRequestMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRE, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, linkTable, eTradeHeader.PK, "TRE Request message", treRequestInterchange.PK);
			var treResponseInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRE, EDIInterchange.Direction.Receive, EDIInterchange.Status.Received, treSessionID, "TRE Response message");
			var treResponseMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRE, EDIMessage.Direction.Receive, EDIMessage.Status.ProcessedOK, linkTable, eTradeHeader.PK, "TRE Response message", treResponseInterchange.PK);
			var queryGUID = "4142285b-6b4f-4eb8-9bfa-6baa3b884b06";
			var pollingTransaction = MessageTestHelper.CreateCusPollingTransaction(factory, TRMessageTypes.Codes.TRE, Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND, 5, ZDateTime.Now, queryGUID, treResponseInterchange.PK);
			var t1eSessionID = new ZGuid("C3ACBD69-A79B-45D8-817A-6E787A9AD792");
			var t1eRequestInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.T1E, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, t1eSessionID, "TRE Request message");
			var t1eRequestMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T1E, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, linkTable, eTradeHeader.PK, "TRE Request message", t1eRequestInterchange.PK);
			t1eRequestMessage.EM_ApplicationReference = queryGUID;
			var t1eResponseInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.T1E, EDIInterchange.Direction.Receive, EDIInterchange.Status.Received, t1eSessionID, "TRE Response message");
			var t1eResponseMessage = CreateETradeMessage(t1eResponseMessageText, eTradeHeader.PK, TRMessageTypes.Codes.T1E);
			t1eResponseMessage.EM_EI = t1eResponseInterchange.PK;

			return (pollingTransaction, t1eResponseMessage);
		}

		ETradeEDIMessage CreateETradeMessage(ZString messageText, ZGuid headerPk, ZString messageType)
		{
			var message = Factory.New<ETradeEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageType = messageType;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = headerPk;
			message.EM_MessageText = messageText;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_SystemCreateUser = "YE";
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			return message;
		}

		ETradeTemporaryRegistrationMessageProcessor Processor => processor ?? (processor = new ETradeTemporaryRegistrationMessageProcessor(new LoggingInformation()));
		ETradeTemporaryRegistrationMessageProcessor processor;
	}
}
