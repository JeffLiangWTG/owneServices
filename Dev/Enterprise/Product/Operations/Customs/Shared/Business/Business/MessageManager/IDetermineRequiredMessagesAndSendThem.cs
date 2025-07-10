using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public interface ISendMessagesToCustomsExtraMembersAndDetermineRequiredMessagesAndSendThem : ISendsMessagesToCustomsExtraMembers
	{
		ContinueWithSave DetermineRequiredMessagesAndSendThem(IMessageManager messageManager);
	}
}
