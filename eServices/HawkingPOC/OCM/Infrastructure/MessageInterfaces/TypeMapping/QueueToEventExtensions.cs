using System;
using System.Runtime.CompilerServices;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using OcmPoc.Infrastructure.MessageInterfaces.Enums;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;

namespace OcmPoc.Infrastructure.MessageInterfaces.TypeMapping
{
	public static class QueueToEventExtensions
    {
		public static MessageEvent ToMessageEvent(this QueueItem item, Exception ex, [CallerMemberName] string member = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
		{
			return item.ToMessageEvent(MessageEventType.Exception, ex.ToString(), member, file, line);
		}

		public static MessageEvent ToMessageEvent(this QueueItem item, MessageEventType eventType, string detail = null, [CallerMemberName] string member = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
		{
			if (eventType == MessageEventType.MappedTo && detail == null)
			{
				detail = item.Recipient;
			}

			return new MessageEvent
			{
				MessageFlowId = item.MessageFlowId,
				MessageId = item.MessageId,
				Type = eventType,
				Detail = detail,
				Component = $"{member} in {file}:line {line}"
			};
		}
	}
}
