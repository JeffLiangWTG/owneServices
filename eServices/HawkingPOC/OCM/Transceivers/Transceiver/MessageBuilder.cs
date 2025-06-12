using OcmPoc.Infrastructure.MessageInterfaces.Documents;

namespace OcmPoc.Transceivers
{
	class MessageBuilder : IMessageBuilder
	{
		readonly string providerName;

		public MessageBuilder(string providerName)
		{
			this.providerName = providerName;
		}

		public Message Build(string name, byte[] content)
		{
			return new Message
			{
				Name = name,
				From = providerName,
				Body = content
			};
		}
	}
}
