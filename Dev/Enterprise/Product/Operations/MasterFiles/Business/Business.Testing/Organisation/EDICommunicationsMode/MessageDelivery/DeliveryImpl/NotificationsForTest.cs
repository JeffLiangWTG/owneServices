using System.Collections.Generic;
using CargoWise.ComponentModel;

namespace Enterprise.MasterFiles.Business.MessageDelivery.Testing
{
	sealed class NotificationsForTest : INotifications
	{
		readonly List<string> allNotifications = new List<string>();
		readonly List<string> warningNotifications = new List<string>();

		void INotifications.Add(INotification notification)
		{
			if (notification.Type == NotificationType.Warning)
			{
				warningNotifications.Add(notification.Message);
			}

			allNotifications.Add(notification.Message);
		}

		public string GetWarnings()
		{
			return string.Join("\r\n", warningNotifications.ToArray());
		}

		public override string ToString()
		{
			return string.Join("\r\n", allNotifications.ToArray());
		}
	}
}
