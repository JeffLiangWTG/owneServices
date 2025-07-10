using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	class TransitErrorMessageNotifier : INotifications, IDisposable
	{
		public TransitErrorMessageNotifier()
		{
			ErrorMessage = new List<string>();
		}
		public readonly List<string> ErrorMessage;

		void INotifications.Add(INotification notification)
		{
			if (notification.Type.EnumValueName == NotificationType.Error.EnumValueName)
			{
				ErrorMessage.Add(notification.Message);
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}
}
