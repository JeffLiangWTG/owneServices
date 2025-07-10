using CargoWise.ComponentModel;
using Enterprise.Customs.Business.MessageManagers.Testing;

namespace Enterprise.Customs.Business.MessageManagers.DocumentSending.Testing
{
	public sealed class MessageNotificationCollector_ForTest : TestUserNotification, IMessageNotificationCollector
	{
		MessageSendingNotificationCollection IMessageNotificationCollector.Notifications => notificationCollection ?? (notificationCollection = new MessageSendingNotificationCollection());
		MessageSendingNotificationCollection notificationCollection;

		public void Add(INotification notification)
		{
			var notifications = (this as IMessageNotificationCollector).Notifications;
			if (notification.Type == CargoWise.ComponentModel.NotificationType.Error)
			{
				notifications.AddError(notification.Message);
			}
			else if (notification.Type == CargoWise.ComponentModel.NotificationType.Warning)
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
