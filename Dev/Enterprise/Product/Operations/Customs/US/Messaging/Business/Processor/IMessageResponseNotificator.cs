using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business
{
	public interface IMessageResponseNotificator
	{
		ZString GetFallbackEmailAddressRecipient();
	}
}
