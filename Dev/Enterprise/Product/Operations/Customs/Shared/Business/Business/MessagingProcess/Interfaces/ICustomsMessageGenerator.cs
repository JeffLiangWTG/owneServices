using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public interface ICustomsMessageGenerator
	{
		EDIMessage GenerateMessage();
	}

	public interface ICustomsMessageWithPlaceholdersGenerator : ICustomsMessageGenerator
	{
		void UpdateMessagePlaceholders(EDIMessage message);
	}
}
