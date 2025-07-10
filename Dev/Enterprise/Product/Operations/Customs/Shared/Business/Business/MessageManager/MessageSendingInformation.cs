using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public sealed class MessageSendingInformation : MessageSendingNotification
	{
		public MessageSendingInformation(ZString message) : base(message)
		{
		}

		protected internal override ZString MessagePrefix
		{
			get { return Res.GetString("1A980E66-4E35-4B79-85C7-EF3C4D69E173", "Information"); }
		}
	}
}
