using CargoWise.ComponentModel;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class ZAEDIMessageValidation : EDIMessageValidation
	{
		public ZAEDIMessageValidation(AutoEDIMessage parent) : base(parent)
		{
		}

		ZAMessage message => Parent as ZAMessage;

		protected override void CheckEM_MessageNum()
		{
			base.CheckEM_MessageNum();
			var helper = CUSRESMessageHelper.New(message);
			var warningMessage = Res.GetString("4939B019-CF1B-4D11-87D4-40ADD576D933", "Unable to link this CUSRES back to entry. CUSRES has been linked via LRN. This message has been discarded.");
			if (message.EM_MessageType == SARSEDIMessage.MessageTypes.CUSRES && message.EM_Status == ZAMessage.Status.Discarded && helper.OutgoingMessageNumber != message.EM_MessageNum)
			{
				Parent.RemoveRowWarning(warningMessage);
				Parent.AddRowNotification(new Notification(CargoWise.EntityFramework.NotificationType.Warning, warningMessage));
			}
		}
	}
}
