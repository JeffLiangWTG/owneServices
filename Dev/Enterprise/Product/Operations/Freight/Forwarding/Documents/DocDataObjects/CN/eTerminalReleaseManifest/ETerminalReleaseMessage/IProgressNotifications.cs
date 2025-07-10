using System.Collections.Generic;
using CargoWise.ComponentModel;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	public interface IProgressNotifications : INotifications
	{
		void SetProgressMax(int max);
		void BumpProgress();
		void Notify(INotificationType notificationType, string message);
		void NotifyFormat(INotificationType notificationType, string format, params object[] args);
		ICollection<INotification> Notifications { get; }
	}
}
