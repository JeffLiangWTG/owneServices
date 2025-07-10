using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Business.MessageProcessors.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFHeaderMessageFailureProcessorTest : MessageFailureProcessorTest<AllMessageFailureProcessor, APLA, APLB, APLY>
	{
		protected override void EndToEndCore()
		{
			ProcessAndAssert(ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling, ApplicationIdentifierCodeList.Descriptions.ImporterSecurityFiling, MessageStatusList.Codes.ErrorISFAdd, Enterprise.Customs.US.ISF.Business.EM_MessageSubTypeList.Codes.ISFAdd);
		}

		protected override IncomingMessageProcessor CreateNewIncomingMessageProcessor() => new ISFIncomingMessageProcessor();

		void ProcessAndAssert(string applicationIdentifier, string subject, string messageStatus, string messageSubType)
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			AssertNotEquals(messageStatus, header.BF_CustomsStatus);
			ProcessAndAssert(applicationIdentifier, subject, (MQEDIMessage sendigMessage) =>
			{
				sendigMessage.EM_LinkedObject = header;
				sendigMessage.EM_MessageSubType = messageSubType;
			});
			header.Reload();
			AssertEquals(messageStatus, header.BF_CustomsStatus);
		}
	}
}
