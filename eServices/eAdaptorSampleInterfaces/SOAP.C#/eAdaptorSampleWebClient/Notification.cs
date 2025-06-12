
namespace CargoWise.eAdaptorSampleWebClient
{
	public class Notification
	{
		public string Message { get; private set; }
		public MessageType NotificationType { get; private set; }

		public Notification(string message, MessageType type)
		{
			Message = message;
			NotificationType = type;
		}

		public override string ToString()
		{
			return Message;
		}

		public enum MessageType
		{
			Information,
			Warning,
			Error,
			Confirmation
		}
	}

}
