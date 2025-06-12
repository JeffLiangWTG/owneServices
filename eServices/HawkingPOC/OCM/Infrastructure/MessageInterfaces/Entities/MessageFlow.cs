using System;
using System.Collections.Generic;
using System.Linq;
using OcmPoc.Infrastructure.MessageInterfaces.Enums;

namespace OcmPoc.Infrastructure.MessageInterfaces.Entities
{
	public class MessageFlow : BaseEntity
	{
		static Dictionary<(MessageFlowStatus, MessageEventType), MessageFlowStatus> statusTransitions =
			new Dictionary<(MessageFlowStatus, MessageEventType), MessageFlowStatus>
			{
				[(MessageFlowStatus.Created, MessageEventType.Created)] = MessageFlowStatus.Initiated,
				[(MessageFlowStatus.Created, MessageEventType.MappingFrom)] = MessageFlowStatus.Initiated,
				[(MessageFlowStatus.Created, MessageEventType.MappedTo)] = MessageFlowStatus.Mapped,
				[(MessageFlowStatus.Created, MessageEventType.Correlated)] = MessageFlowStatus.Mapped,
				[(MessageFlowStatus.Created, MessageEventType.Sent)] = MessageFlowStatus.Delivered,
				[(MessageFlowStatus.Created, MessageEventType.Exception)] = MessageFlowStatus.Failed,
				[(MessageFlowStatus.Created, MessageEventType.Failed)] = MessageFlowStatus.Failed,
				[(MessageFlowStatus.Created, MessageEventType.MappingFailed)] = MessageFlowStatus.Failed,
				[(MessageFlowStatus.Initiated, MessageEventType.MappedTo)] = MessageFlowStatus.Mapped,
				[(MessageFlowStatus.Initiated, MessageEventType.Correlated)] = MessageFlowStatus.Mapped,
				[(MessageFlowStatus.Initiated, MessageEventType.Sent)] = MessageFlowStatus.Delivered,
				[(MessageFlowStatus.Initiated, MessageEventType.Exception)] = MessageFlowStatus.Failed,
				[(MessageFlowStatus.Initiated, MessageEventType.Failed)] = MessageFlowStatus.Failed,
				[(MessageFlowStatus.Initiated, MessageEventType.MappingFailed)] = MessageFlowStatus.Failed,
				[(MessageFlowStatus.Mapped, MessageEventType.Sent)] = MessageFlowStatus.Delivered,
				[(MessageFlowStatus.Mapped, MessageEventType.Exception)] = MessageFlowStatus.Failed,
				[(MessageFlowStatus.Mapped, MessageEventType.Failed)] = MessageFlowStatus.Failed,
				[(MessageFlowStatus.Mapped, MessageEventType.MappingFailed)] = MessageFlowStatus.Failed,
			};

		List<MessageEvent> _events;

		public MessageFlow()
		{
			_events = new List<MessageEvent>();
		}

		public Guid InitialMessageId { get; set; }
		public string Sender { get; set; }
		public string Recipient { get; set; }
		public MessageFlowStatus Status { get; set; }

		public Guid ConversationGuid { get; set; }

		public Conversation Conversation { get; set; }

		public IEnumerable<MessageEvent> Events => _events.AsReadOnly().OrderBy(e => e.Timestamp);

		public void AddEvent(MessageEvent messageEvent)
		{
			messageEvent.MessageFlowGuid = Guid;
			_events.Add(messageEvent);

			if (statusTransitions.TryGetValue((Status, messageEvent.Type), out MessageFlowStatus newStatus))
			{
				Status = newStatus;
			}

			if (Recipient == null && messageEvent.Type == MessageEventType.MappedTo)
			{
				Recipient = messageEvent.Detail;
			}

		}

		public void InitiateConversation()
		{
			Conversation = new Conversation
			{
				InitialMessageId = InitialMessageId,
				Initiator = Sender,
				Responder = Recipient,
				Status = ConversationStatus.Created
			};
		}
	}
}
