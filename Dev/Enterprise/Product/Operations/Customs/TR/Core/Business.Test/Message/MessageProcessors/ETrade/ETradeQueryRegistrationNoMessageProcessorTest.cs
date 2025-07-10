using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ETradeQueryRegistrationNoMessageProcessorTest : TestCaseWithFactory
	{
		public void TestUpdateCustomsStatusAndMessageModeWhenProcessMessage()
		{
			var errorMessageText = TRMessageTestHelper.GetFileText("ETradeError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var errorETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			var errorMessage = CreateETradeMessage(errorMessageText, errorETradeHeader.PK);

			Processor.ProcessMessage(errorMessage);

			CombineAssertions("Query Registration No Error", () =>
			{
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.QRR, errorETradeHeader.RegistrationStatus);
				AssertEquals("MessageMode", TRMessageTypes.Codes.TRQ, errorETradeHeader.MessageMode);
				AssertEquals("MessageStatus", TRMessageStatusCodeList.Codes.Error, ((IMessageAttachee)errorETradeHeader).MessageStatus);
			});

			var successMessageText = TRMessageTestHelper.GetFileText("ETrade.QueryRegistrationNo.QueryRegistrationNoSuccess.xml");
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "GeciciTescildenTescilNoSorgulaResponse" };
			var queryregNo = TRMessageHelper.GetNodeValue(successMessageText, parentNodeList, "GeciciTescildenTescilNoSorgulaResult");

			var importETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			importETradeHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var importSuccessMessage = CreateETradeMessage(successMessageText, importETradeHeader.PK);

			Processor.ProcessMessage(importSuccessMessage);

			var expectedImportSuccessMessage = TRMessageTestHelper.GetFileText("ETrade.QueryRegistrationNo.QueryRegistrationNoSuccess.htm");
			CombineAssertions("Query Registration No Success", () =>
			{
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.QRS, importETradeHeader.RegistrationStatus);
				AssertEquals("MessageMode", TRMessageTypes.Codes.TRI, importETradeHeader.MessageMode);
				AssertEquals("MessageStatus", TRMessageStatusCodeList.Codes.Accepted, ((IMessageAttachee)importETradeHeader).MessageStatus);
				AssertEquals("RegistrationNumber", queryregNo, importETradeHeader.RegistrationNumber);
				AssertEquals("EM_FormattedMessageText", successMessageText, importSuccessMessage.EM_FormattedMessageText);
				AssertEquals("EM_MessageInterpretation", expectedImportSuccessMessage, importSuccessMessage.EM_MessageInterpretation);
			});

			var unSuccessMessageText = TRMessageTestHelper.GetFileText("ETrade.QueryRegistrationNo.QueryRegistrationNoUnSuccess.xml");
			var unSuccessResult = TRMessageHelper.GetNodeValue(unSuccessMessageText, parentNodeList, "GeciciTescildenTescilNoSorgulaResult");
			var oldMmessageMode = importETradeHeader.MessageMode;
			var oldRegistrationNumber = importETradeHeader.RegistrationNumber;
			var importUnSuccessMessage = CreateETradeMessage(unSuccessMessageText, importETradeHeader.PK);

			Processor.ProcessMessage(importUnSuccessMessage);

			CombineAssertions("Query Registration No UnSuccess", () =>
			{
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.QRR, importETradeHeader.RegistrationStatus);
				AssertEquals("MessageMode", oldMmessageMode, importETradeHeader.MessageMode);
				AssertEquals("RegistrationNumber", oldRegistrationNumber, importETradeHeader.RegistrationNumber);
				var expectedInterpretation = TRMessageTestHelper.GetFileText("ETrade.QueryRegistrationNo.QueryRegistrationNoUnSuccess.htm");
				AssertEquals("EM_MessageInterpretation", expectedInterpretation, importUnSuccessMessage.EM_MessageInterpretation);
			});
		}

		ETradeEDIMessage CreateETradeMessage(ZString messageText, ZGuid headerPk)
		{
			var message = Factory.New<ETradeEDIMessage>();
			message.EM_ApplicationReference = "ULU-2019/00002345";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = TRMessageTypes.Codes.TRE;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_LinkUniqueID = headerPk;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = messageText;
			return message;
		}

		ETradeQueryRegistrationNoMessageProcessor Processor => processor ?? (processor = new ETradeQueryRegistrationNoMessageProcessor(new LoggingInformation()));
		ETradeQueryRegistrationNoMessageProcessor processor;
	}
}
