using System.Collections.Generic;
using CargoWise.ComponentModel;

namespace Enterprise.Services.ServiceHost
{
	sealed class NotificationsHandler : INotifications
	{
		public IReadOnlyCollection<INotification> Notifications => notifications;
		List<INotification> notifications;

		void INotifications.Add(INotification notification)
		{
			if (notification == null)
			{
				return;
			}

			if (notifications == null)
			{
				notifications = new List<INotification>();
			}

			notifications.Add(notification);
		}
	}
}
