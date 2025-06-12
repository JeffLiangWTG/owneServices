using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Documents;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;
using OcmPoc.Mapping.Interface;
using OcmPoc.Mapping.Interface.Helpers;

namespace OcmPoc.Mapping.Mapper
{
	class MessageForSendMapper : IMessageMapper
	{
		readonly IMessageRepository messageRepository;
		readonly IMappingProvider mappingProvider;

		public MessageForSendMapper(IMessageRepository messageRepository, IMappingProvider mappingProvider)
		{
			this.messageRepository = messageRepository;
			this.mappingProvider = mappingProvider;
		}
	
		public async Task<QueueItem> MapMessageAsync(QueueItem item)
		{
			var command = await CreateCommandAsync(item);
			var mapping = mappingProvider.GetMapping($"Provider.{item.Recipient}");

			var result = await mapping.MapAsync(command);

			return await HandleResultAsync(item, result);
		}

		async Task<MapForSendCommand> CreateCommandAsync(QueueItem item)
		{
			var message = await messageRepository.RetrieveAsync(item.MessageId);
			var commonMessage = message.Body.DeserializeFromJson<CommonMessage>();

			return new MapForSendCommand(commonMessage);
		}

		async Task<QueueItem> HandleResultAsync(QueueItem item, MapForSendResult result)
		{
			var message = new Message
			{
				From = item.Sender,
				To = item.Recipient,
				Name = result.FileName,
				Body = result.Content
			};

			var messageId = await messageRepository.StoreAsync(message);

			return new QueueItem(item)
			{
				MessageId = messageId,
				FileName = result.FileName
			};
		}
	}
}
