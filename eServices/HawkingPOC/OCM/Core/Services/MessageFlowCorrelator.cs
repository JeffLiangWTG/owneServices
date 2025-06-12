using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using OcmPoc.Infrastructure.MessageInterfaces.Enums;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;
using OcmPoc.Infrastructure.MessageInterfaces.Tracking;
using OcmPoc.Utils;

namespace OcmPoc.Core.Services
{
	public class MessageFlowCorrelator : IMessageFlowCorrelator
	{
		readonly IMessageFlowRepository messageFlowRepository;
		readonly IEventLogger eventLogger;

		public MessageFlowCorrelator(IMessageFlowRepository messageFlowRepository, IEventLogger eventLogger)
		{
			this.messageFlowRepository = messageFlowRepository;
			this.eventLogger = eventLogger;
		}

		public async Task CorrelateFlowAsync(int messageFlowId, string sender, string recipient, string correlationId)
		{
			Guid.TryParse(correlationId, out Guid initialMessageId);

			var (currentFlow, previousFlow) =
				await Task.WhenAll(
					messageFlowRepository.GetByIdAsync(messageFlowId),
					messageFlowRepository.GetMessageFlowAsync(recipient, sender, initialMessageId));

			var toBeUpdated = new List<MessageFlow> { currentFlow };

			if (previousFlow != null)
			{
				if (previousFlow.Conversation == null)
				{
					previousFlow.InitiateConversation();
					toBeUpdated.Add(previousFlow);
				}

				currentFlow.Conversation = previousFlow.Conversation;
			}
			else
			{
				currentFlow.InitiateConversation();
			}

			await messageFlowRepository.UpdateAsync(toBeUpdated);
			await eventLogger.LogEventAsync(new MessageEvent
			{
				MessageFlowId = messageFlowId,
				Timestamp = DateTime.UtcNow,
				Type = MessageEventType.Correlated
			});
		}
	}
}
