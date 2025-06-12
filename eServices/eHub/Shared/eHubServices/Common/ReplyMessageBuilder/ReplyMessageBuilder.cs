namespace CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder
{
	public abstract class ReplyMessageBuilder
	{
		protected MessageContext.MessageContext Context { get; private set; }

		protected ReplyMessageBuilder(MessageContext.MessageContext context)
		{
			Context = context;
		}

		public abstract string GetReply();
	}

	public class ServiceReply
	{
		public ServiceReply(Action action, string description)
		{
			ReplyAction = action;
			ReplyDescription = description;
		}

		public ServiceReply(Action action)
			: this(action, string.Empty)
		{
		}

		public Action ReplyAction { get; private set; }

		public string ReplyDescription { get; private set; }

		public enum Action
		{
			Success,
			Error
		}
	}


}