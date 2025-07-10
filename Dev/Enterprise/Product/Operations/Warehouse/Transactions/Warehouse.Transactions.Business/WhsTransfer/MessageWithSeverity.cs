using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	class MessageWithSeverity
	{
		public MessageWithSeverity(MessageTypes messageType, ZString message)
		{
			MessageType = messageType;
			Message = message;
		}

		public readonly MessageTypes MessageType;
		public readonly ZString Message;

		public bool IsNone
		{
			get { return MessageType == MessageTypes.None; }
		}

		public enum MessageTypes
		{
			Error,
			Warning,
			None
		}
	}
}
