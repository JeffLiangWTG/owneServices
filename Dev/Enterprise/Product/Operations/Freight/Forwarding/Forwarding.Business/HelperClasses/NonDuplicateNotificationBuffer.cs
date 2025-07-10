using System.Collections.Generic;

using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.Business
{
	public class NonDuplicateNotificationBuffer : NotificationBuffer
	{
		public NonDuplicateNotificationBuffer(INotifications inner) : base(inner)
		{
			uniqueEvents = new Dictionary<INotification, INotification>(new NotificationEqualityComparer());
		}

		public override void Notify(INotification eventObj)
		{
			if (eventObj != null && !this.uniqueEvents.ContainsKey(eventObj))
			{
				this.uniqueEvents.Add(eventObj, eventObj);
				base.Notify(eventObj);
			}
		}

		readonly Dictionary<INotification, INotification> uniqueEvents;

		class NotificationEqualityComparer : IEqualityComparer<INotification>
		{
			#region IEqualityComparer<INotification> Members

			public bool Equals(INotification x, INotification y)
			{
				if (x.Message != y.Message)
				{
					return false;
				}

				return Equals(x.Type, y.Type);
			}

			public int GetHashCode(INotification obj)
			{
				return obj.Message.GetHashCode();
			}

			#endregion

			bool Equals(INotificationType x, INotificationType y)
			{
				return x == y ||
						(x != null && y != null && x.IsFatal == y.IsFatal && x.Severity == y.Severity);
			}
		}
	}
}
