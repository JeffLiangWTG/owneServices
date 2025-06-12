using System;
using System.Threading.Tasks;
using AutoMapper;
using OcmPoc.Infrastructure.MessageInterfaces.Documents;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using OcmPoc.Infrastructure.MessageInterfaces.Enums;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;
using OcmPoc.Infrastructure.MessageInterfaces.Tracking;

namespace OcmPoc.Core.Services
{
	public class MessageFlowService : IMessageFlowService
	{
		readonly IMessageRepository messageRepository;
		readonly IMessageFlowRepository messageFlowRepository;
		readonly IQueueClient queueClient;
		readonly IEventLogger eventLogger;
		readonly IMapper mapper;

		public MessageFlowService(IMessageRepository messageRepository, IMessageFlowRepository messageFlowRepository,
			IQueueClient queueClient, IEventLogger eventLogger, IMapper mapper)
		{
			this.messageRepository = messageRepository;
			this.messageFlowRepository = messageFlowRepository;
			this.queueClient = queueClient;
			this.eventLogger = eventLogger;
			this.mapper = mapper;
		}

		public async Task<MessageFlow> CreateNewFlowAsync(Message message)
		{
			var messageId = message.Id = Guid.NewGuid();
			Log($"In CreateNewFlowAsync:  id = {messageId}; name = {message.Name}; from = {message.From}; to = {message.To}; content-length = {message.Body.Length}");

			var messageFlow = new MessageFlow
			{
				Sender = message.From,
				Recipient = message.To,
				InitialMessageId = messageId,
				Status = MessageFlowStatus.Created
			};

			await messageFlowRepository.AddAsync(messageFlow);
			//Log($"Stored new MessageFlow with Id {messageFlow.Id}");

			try
			{
				await messageRepository.StoreAsync(message);

				Log($"Stored message {messageId}");

				using (var queueWriter = queueClient.GetQueueWriter())
				{
					var queueItem = mapper.Map<QueueItem>(messageFlow);
					queueItem.FileName = message.Name;
					queueWriter.Publish(queueItem);
					//Log("Published message to queue");
				}

				var eventId = await eventLogger.LogEventAsync(MessageEvent.New(messageFlow.Id, messageId, MessageEventType.Created));
				//Log($"Logged message event {eventId}");

			}
			catch (Exception ex)
			{
				Log($"Failure: {ex}");
				await eventLogger.LogEventAsync(MessageEvent.Exception(messageFlow.Id, messageId, ex));
			}

			return messageFlow;
		}

		public async Task<MessageFlow> GetFlowAsync(int id)
		{
			return await messageFlowRepository.GetByIdAsync(id);
		}

		private void Log(string message)
		{
			//Console.WriteLine($"{DateTime.Now} : {message}");
		}
	}
}
