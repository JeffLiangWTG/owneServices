using Enterprise.Customs.TR.Business;

namespace Enterprise.Customs.TR
{
	public interface IESignatureHelper
	{
		bool CanSignMessage(MessageSendAcknowledgeAndSign signInData);
	}
}
