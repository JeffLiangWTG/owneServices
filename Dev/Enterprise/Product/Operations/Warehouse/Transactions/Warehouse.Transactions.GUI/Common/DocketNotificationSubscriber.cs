using System;

using CargoWise.ComponentModel;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.GUI
{
	class DocketNotificationSubscriber : IDisposable
	{
		public DocketNotificationSubscriber(WhsDocket docket, INotifications subscriber)
		{
			this.docket = docket;
			this.subscriber = subscriber;

			if (docket != null && subscriber != null)
			{
				docket.NotificationManager.Push(subscriber);
			}
		}

		public virtual void Dispose()
		{
			if (docket != null && subscriber != null)
			{
				docket.NotificationManager.Pop();
			}
		}

		readonly WhsDocket docket;
		readonly INotifications subscriber;
	}
}
