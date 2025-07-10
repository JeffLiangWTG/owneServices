using CargoWise.ComponentModel;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI.DocumentSending
{
	public class MessageNotificationCollector : UserNotification, IMessageNotificationCollector
	{
		public MessageNotificationCollector()
		{
		}

		MessageSendingNotificationCollection IMessageNotificationCollector.Notifications => notificationCollection ?? (notificationCollection = new MessageSendingNotificationCollection());
		MessageSendingNotificationCollection notificationCollection;

		public void Add(INotification notification)
		{
			var notifications = ((IMessageNotificationCollector)this).Notifications;

			if (notification.Type == NotificationType.Error)
			{
				notifications.AddError(notification.Message);
			}
			else if (notification.Type == NotificationType.Warning)
			{
				notifications.AddWarning(notification.Message);
			}
			else
			{
				notifications.AddInformation(notification.Message);
			}
		}
	}
}
