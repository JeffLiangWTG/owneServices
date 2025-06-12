using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Documents;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;
using OcmPoc.Mapping.Interface;

namespace OcmPoc.Mapping.Mapper
{
	class ReceivedMessageMapper : IMessageMapper
	{
		readonly IMessageRepository messageRepository;
		readonly IMappingProvider mappingProvider;

		public ReceivedMessageMapper(IMessageRepository messageRepository, IMappingProvider mappingProvider)
		{
			this.messageRepository = messageRepository;
			this.mappingProvider = mappingProvider;
		}

		public async Task<QueueItem> MapMessageAsync(QueueItem item)
		{
			var command = await CreateCommandAsync(item);
			var mapping = mappingProvider.GetMapping($"Provider.{item.Sender}");

			var result = await mapping.MapAsync(command);

			return await HandleResultAsync(item, result);
		}

		async Task<MapReceivedCommand> CreateCommandAsync(QueueItem item)
		{
			var message = await messageRepository.RetrieveAsync(item.MessageId);

			return new MapReceivedCommand(item.FileName, item.Sender, message.Body);
		}

		async Task<QueueItem> HandleResultAsync(QueueItem item, MapReceivedResult result)
		{
			var message = new Message
			{
				From = item.Sender,
				To = result.Recipient,
				Name = item.FileName,
				Body = result.Content
			};

			var messageId = await messageRepository.StoreAsync(message);

			return new QueueItem(item)
			{
				MessageId = messageId,
				CorrelationId = result.CorrelationId,
				FileName = null,
				Recipient = result.Recipient
			};
		}
	}
}
