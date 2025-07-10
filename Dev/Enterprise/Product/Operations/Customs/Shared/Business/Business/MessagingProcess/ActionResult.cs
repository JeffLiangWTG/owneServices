using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public class ActionResult
	{
		public ActionResult() : this(false) { }
		public ActionResult(bool success) : this(success, Enumerable.Empty<MessageSendingNotification>()) { }
		public ActionResult(bool success, IEnumerable<MessageSendingNotification> notifications)
		{
			Success = success;
			AppendNotifications(Argument.NotNull(notifications, nameof(notifications)));
		}

		public bool Success { get; set; }
		public MessageSendingNotificationCollection Notifications { get; set; } = new MessageSendingNotificationCollection();
		public MessageSendingNotificationCollection PreviousNotifications { get; set; } = new MessageSendingNotificationCollection();
		public List<EDIMessage> EDIMessages { get; set; }
		public IBusiness DataSource { get; set; }
		public object PassThroughData { get; set; }

		public void AppendErrorNotification(ZString notification)
		{
			Notifications.AddError(notification);
		}
		public void AppendWarningNotification(ZString notification)
		{
			Notifications.AddWarning(notification);
		}
		public void AppendInformationNotification(ZString notification)
		{
			Notifications.AddInformation(notification);
		}
		public void AppendNotification(MessageSendingNotification newNotification)
		{
			Notifications.Add(newNotification);
		}

		public void AppendNotifications(IEnumerable<MessageSendingNotification> notifications)
		{
			foreach (var notification in notifications)
			{
				Notifications.Add(notification);
			}
		}

		public void UpdateSuccess()
		{
			Success = Success && (!Notifications?.Any(x => x.IsError) ?? true);
		}

		public bool ContainsWarning() => PreviousNotifications.ContainsWarning() || Notifications.ContainsWarning();
	}
}
