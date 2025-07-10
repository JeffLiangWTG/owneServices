using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public abstract class MessageBuilder<T> : MessageBuilder<T, MQEDIMessage>
		where T : BlockControlGenerator
	{
		protected MessageBuilder(IMessageAttachee messageAttachee)
			: base(messageAttachee)
		{
		}

		protected MessageBuilder(IMessageAttachee messageAttachee, UpdateActionCode action)
			: base(messageAttachee, action)
		{
		}
	}
}
