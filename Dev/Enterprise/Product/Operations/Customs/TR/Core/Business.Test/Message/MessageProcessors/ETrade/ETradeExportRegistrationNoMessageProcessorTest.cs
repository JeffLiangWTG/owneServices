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
	class ETradeExportRegistrationNoMessageProcessorTest : TestCaseWithFactory
	{
		public void TestAutoReceiveResponseMessageCreation()
		{
			var messageText = TRMessageTestHelper.GetFileText("ResponseWithGuidValue.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Common.");
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETG0000001";
			var message = CreateETradeMessage(TRMessageTypes.Codes.TRS, messageText, header.PK);
			Factory.Save();

			Processor.ProcessMessage(message);

			var guid = TRMessageHelper.GetNodeValue(messageText, "//x:Root/Response/Guid", "http://schemas.microsoft.com/BizTalk/2003/Any");
			AssertEquals("An E-Trade auto receive response message that contains the guid should be created", true, ((ETradeEDIMessage)header.Messages[0]).EM_MessageText.Contains(guid));
			var expectedInterpretation = TRMessageTestHelper.GetFileText("ETrade.ExportRegistrationNo.ExportRegistrationNoResponse.htm");
			AssertEquals("EM_MessageInterpretation", expectedInterpretation, message.EM_MessageInterpretation);
		}

		public void TestUpdateCustomsStatusAndMessageModeWhenProcessMessage()
		{
			var errorMessageText = TRMessageTestHelper.GetFileText("ETrade.ExportRegistrationNo.ExportRegistrationNoError.xml");
			var errorETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			errorETradeHeader.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var errorMessage = CreateETradeMessage(TRMessageTypes.Codes.TRE, errorMessageText, errorETradeHeader.PK);

			Processor.ProcessMessage(errorMessage);

			AssertEquals(CustomsStatusList.Codes.RNR, errorETradeHeader.RegistrationStatus);
			AssertEquals(TRMessageTypes.Codes.TRS, errorETradeHeader.MessageMode);
			AssertEquals(TRMessageStatusCodeList.Codes.Error, ((IMessageAttachee)errorETradeHeader).MessageStatus);

			var successMessageText = TRMessageTestHelper.GetFileText("ETrade.ExportRegistrationNo.ExportRegistrationNoSuccess.xml");
			var exportETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			exportETradeHeader.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var exportSuccessMessage = CreateETradeMessage(TRMessageTypes.Codes.TRE, successMessageText, exportETradeHeader.PK);

			Processor.ProcessMessage(exportSuccessMessage);

			AssertEquals(CustomsStatusList.Codes.RNS, exportETradeHeader.RegistrationStatus);
			AssertEquals(TRMessageTypes.Codes.TRI, exportETradeHeader.MessageMode);
			AssertEquals(TRMessageStatusCodeList.Codes.Accepted, ((IMessageAttachee)exportETradeHeader).MessageStatus);
			var expectedInterpretation = TRMessageTestHelper.GetFileText("ETrade.ExportRegistrationNo.ExportRegistrationNoSuccess.htm");
			AssertEquals("EM_MessageInterpretation", expectedInterpretation, exportSuccessMessage.EM_MessageInterpretation);
		}

		ETradeEDIMessage CreateETradeMessage(ZString messageType, ZString messageText, ZGuid headerPk)
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
			return message;
		}

		ETradeExportRegistrationNoMessageProcessor Processor => processor ?? (processor = new ETradeExportRegistrationNoMessageProcessor(new LoggingInformation()));
		ETradeExportRegistrationNoMessageProcessor processor;
	}
}
