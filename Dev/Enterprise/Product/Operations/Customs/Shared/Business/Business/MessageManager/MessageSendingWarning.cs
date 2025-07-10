using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public sealed class MessageSendingWarning : MessageSendingNotification
	{
		public MessageSendingWarning(ZString message) : base(message)
		{
		}

		public override bool IsWarning
		{
			get { return true; }
		}

		protected internal override ZString MessagePrefix
		{
			get { return Res.GetString("27d79a59-df45-4cb5-a9da-3195c8f02d2f", "Warning"); }
		}
	}
}
