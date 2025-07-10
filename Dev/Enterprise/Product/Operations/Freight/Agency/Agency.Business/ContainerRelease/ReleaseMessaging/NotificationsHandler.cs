using System.Collections.Generic;
using CargoWise.ComponentModel;

namespace Enterprise.Freight.Agency.Business
{
	public class NotificationsHandler : INotifications
	{
		public List<INotification> Notifications
		{
			get { return notifications ?? (notifications = new List<INotification>()); }
		}

		List<INotification> notifications;

		void INotifications.Add(INotification notification)
		{
			Notifications.Add(notification);
		}
	}
}
