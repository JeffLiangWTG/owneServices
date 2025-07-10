using CargoWise.ComponentModel;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using MessagePreSendingValidation = Enterprise.Customs.PL.NCTS.Business.MessagePreSendingValidation;
using NctsHeader = Enterprise.Customs.PL.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.PL.NCTS.GUI;

public class MessagingMenuProvider(NctsHeader header) : EU.NCTS.GUI.Phase5MessagingMenuProvider(header)
{
	protected override void SendToCustomsCore(ZMenuItem menuItem)
	{
		var notifications = new NotificationsDecorator(GlobalNotificationsWrapper.Instance);
		new MessagePreSendingValidation((NctsHeader)Header).Validate(notifications);
		if (!notifications.ReportedFatalError)
		{
			base.SendToCustomsCore(menuItem);
		}
	}

	protected override EU.NCTS.GUI.MessageSendingForm GetMessageSendingFormCore(NctsHeaderMessageSendingObjectParent messageSendingObjectParent) => new MessageSendingForm(messageSendingObjectParent);
}
