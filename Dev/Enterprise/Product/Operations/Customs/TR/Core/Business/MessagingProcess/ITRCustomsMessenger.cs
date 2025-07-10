using Enterprise.Customs.Business.MessagingProcess;

namespace Enterprise.Customs.TR.Business.MessagingProcess
{
	public interface ITRCustomsMessenger
	{
		ICustomsMessenger Messenger { get; }
		bool IsMessageSigningRequired { get; }
	}
}
