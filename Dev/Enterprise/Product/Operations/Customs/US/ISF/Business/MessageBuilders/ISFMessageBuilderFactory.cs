using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFMessageBuilderFactory : IISFMessageBuilderFactory
	{
		public IMessageBuilder<MQEDIMessage> CreateWebMessageBuilder(IImporterSecurityFiling isf, UpdateActionCode updateActionCode)
		{
			return new ImporterSecurityFilingMessageBuilder<ABIInputBlockControlGenerator, APLB, APLY>(isf, updateActionCode);
		}
	}
}
