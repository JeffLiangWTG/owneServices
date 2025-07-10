using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.DataTransfer;

namespace Enterprise.Customs.NO.NCTS.GUI;

class Phase5MessagingMenuProvider : EU.NCTS.GUI.Phase5MessagingMenuProvider
{
	public Phase5MessagingMenuProvider(NctsHeader header) : base(header)
	{
	}

	public Phase5MessagingMenuProvider(NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper) : base(header, ntcsHeaderUniversalMessagingHelper)
	{
	}

	protected override EU.NCTS.GUI.MessageSendingForm GetMessageSendingFormCore(NctsHeaderMessageSendingObjectParent messageSendingObjectParent)
		=> new MessageSendingForm(messageSendingObjectParent as Business.NctsHeaderMessageSendingObjectParent);
}
