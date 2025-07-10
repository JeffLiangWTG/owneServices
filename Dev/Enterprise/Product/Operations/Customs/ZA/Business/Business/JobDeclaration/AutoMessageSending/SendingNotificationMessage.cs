using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class SendingNotificationMessage : MessageSendingNotification
	{
		public SendingNotificationMessage(ZString message) : base(message)
		{
		}

		protected override ZString MessagePrefix => null;
	}
}
