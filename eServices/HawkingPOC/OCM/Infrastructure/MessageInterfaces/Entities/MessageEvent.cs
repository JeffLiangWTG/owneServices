using System;
using System.Runtime.CompilerServices;
using OcmPoc.Infrastructure.MessageInterfaces.Enums;

namespace OcmPoc.Infrastructure.MessageInterfaces.Entities
{
	public class MessageEvent : BaseEntity
	{
		public MessageEvent()
		{
		}

		public Guid MessageId { get; set; }
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;
		public MessageEventType Type { get; set; }
		public string Component { get; set; }
		public string Detail { get; set; }

		public int MessageFlowId { get; set; }

		public Guid MessageFlowGuid { get; set; }

		public static MessageEvent New(int messageFlowId, Guid messsageId, MessageEventType type, [CallerMemberName] string member = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
		{
			return new MessageEvent
			{
				MessageFlowId = messageFlowId,
				MessageId = messsageId,
				Type = type,
				Component = $"{member} in {file}:line {line}"
			};
		}

		public static MessageEvent Exception(int messageFlowId, Guid messsageId, Exception ex, [CallerMemberName] string member = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
		{
			return new MessageEvent
			{
				MessageFlowId = messageFlowId,
				MessageId = messsageId,
				Type = MessageEventType.Exception,
				Detail = ex.ToString(),
				Component = $"{member} in {file}:line {line}"
			};
		}
	}
}
