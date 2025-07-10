using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;

namespace Enterprise.MasterFiles.Business
{
	public class MacroNotification
	{
		public MacroNotification()
		{
		}

		public void Add(INotifications notifications)
		{
			notificationsList.Add(notifications);
		}

		readonly List<INotifications> notificationsList = new List<INotifications>();

		public string GetNotifications()
		{
			var notFoundNotifications = new List<INotifications>();
			var errorNotifications = new List<INotifications>();

			foreach (var notifications in notificationsList)
			{
				if (notifications.ToString().Contains(Res.GetString("B5CAD51F-F0D5-4F60-B3BE-383AE27E9C9D", "Cannot find property")))
				{
					notFoundNotifications.Add(notifications);
				}
				else
				{
					errorNotifications.Add(notifications);
				}
			}

			return errorNotifications.Count > 0
				? ConvertToString(errorNotifications)
				: ConvertToString(notFoundNotifications);
		}

		static string ConvertToString(List<INotifications> notificationsList)
		{
			var notificationsAsString = string.Join("\r\n", notificationsList.Select(n => n.ToString()));
			notificationsAsString = notificationsAsString.Replace("\r\n\r\n", "\r\n");
			return notificationsAsString;
		}
	}
}
