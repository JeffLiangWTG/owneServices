using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public sealed class MessageSendingError : MessageSendingNotification
	{
		public MessageSendingError(ZString message) : base(message)
		{
		}

		public override bool IsError
		{
			get { return true; }
		}

		protected internal override ZString MessagePrefix
		{
			get { return Res.GetString("6f947d2b-329a-4d51-8ebc-aae0124b4b6b", "Error"); }
		}
	}
}
