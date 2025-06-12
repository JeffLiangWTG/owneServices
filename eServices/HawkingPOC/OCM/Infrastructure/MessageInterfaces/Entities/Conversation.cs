using System;
using System.Collections.Generic;
using OcmPoc.Infrastructure.MessageInterfaces.Enums;

namespace OcmPoc.Infrastructure.MessageInterfaces.Entities
{
	public class Conversation : BaseEntity
	{
		List<MessageFlow> _flows;

		public Conversation()
		{
			_flows = new List<MessageFlow>();
		}

		public Guid InitialMessageId { get; set; }
		public string Initiator { get; set; }
		public string Responder { get; set; }
		public ConversationStatus Status { get; set; }

		public IReadOnlyCollection<MessageFlow> Flows => _flows.AsReadOnly();

		public void AddFlow(MessageFlow messageFlow)
		{
			_flows.Add(messageFlow);
		}
	}
}
