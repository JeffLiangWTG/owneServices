using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public abstract class MessageSendingNotification
	{
		protected MessageSendingNotification(ZString message)
		{
			this.message = message;
		}

		readonly ZString message;

		public virtual bool IsError
		{
			get { return false; }
		}

		public virtual bool IsWarning
		{
			get { return false; }
		}

		public bool IsInformation
		{
			get { return !IsError && !IsWarning; }
		}

		public ZString Message
		{
			get { return message; }
		}

		public ZString MessageIncludingPrefix
		{
			get { return MessagePrefix + ": " + message; }
		}

		protected internal abstract ZString MessagePrefix
		{
			get;
		}
	}
}
