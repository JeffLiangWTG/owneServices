using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public interface IISFMessageBuilderFactory
	{
		IMessageBuilder<MQEDIMessage> CreateWebMessageBuilder(IImporterSecurityFiling isf, UpdateActionCode updateActionCode);
	}
}
