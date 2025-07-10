using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public class DashboardAttachDetachResult
	{
		public DashboardAttachDetachResult(IEnumerable<ForwardingShipment> requestedShipments, IEnumerable<ForwardingShipment> acceptedShipments, IEnumerable<INotification> notifications, ForwardingContainerCollection containers = null)
		{
			RequestedShipments = requestedShipments ?? System.Array.Empty<ForwardingShipment>();
			AcceptedShipments = acceptedShipments ?? System.Array.Empty<ForwardingShipment>();

			RejectedShipments = requestedShipments
				.Where(shipment => !acceptedShipments.Contains(shipment));

			Notifications = notifications?.ToArray() ?? System.Array.Empty<INotification>();

			Containers = containers;
		}

		public readonly IEnumerable<ForwardingShipment> RequestedShipments;
		public readonly IEnumerable<ForwardingShipment> AcceptedShipments;
		public readonly IEnumerable<ForwardingShipment> RejectedShipments;
		public readonly IEnumerable<INotification> Notifications;
		public readonly ForwardingContainerCollection Containers;

		public bool HasErrors => !Errors.IsEmpty;
		public bool HasWarnings => !Warnings.IsEmpty;

		public ZString Errors
		{
			get
			{
				if (!errors.HasValue)
				{
					errors = string.Join(System.Environment.NewLine, GetAllNotificationMessagesWithType(NotificationType.Error));
				}

				return errors.Value;
			}
		}
		ZString? errors;

		public ZString Warnings
		{
			get
			{
				if (!warnings.HasValue)
				{
					warnings = string.Join(System.Environment.NewLine, GetAllNotificationMessagesWithType(NotificationType.Warning));
				}

				return warnings.Value;
			}
		}
		ZString? warnings;

		#region Implementation

		IEnumerable<string> GetAllNotificationMessagesWithType(INotificationType notificationType)
		{
			return Notifications
				.Where(notification => notification.Type == notificationType)
				.Select(notification => notification.Message);
		}

		#endregion
	}
}
