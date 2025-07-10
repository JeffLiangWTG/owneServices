using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class TestLogger : INotifications
	{
		readonly List<string> logs = new List<string>();

		public void Add(INotification notification)
		{
			logs.Add(notification.Type.NotificationTypeName(CargoWise.ResourceStrings.Grammar.PluralState.NonPlural) + " - " + notification.Message);
		}

		public string GetAllLogsAsString()
		{
			return string.Join("\r\n", logs.ToArray());
		}
	}
}
