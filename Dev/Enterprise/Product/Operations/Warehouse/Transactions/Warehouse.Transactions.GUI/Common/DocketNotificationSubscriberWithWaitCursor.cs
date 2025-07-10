
using CargoWise.ComponentModel;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	class DocketNotificationSubscriberWithWaitCursor : ZWaitCursorChanger
	{
		public DocketNotificationSubscriberWithWaitCursor(WhsDocket docket, INotifications subscriber)
		{
			this.docket = docket;
			this.subscriber = subscriber;

			if (docket != null && subscriber != null)
			{
				docket.NotificationManager.Push(subscriber);
			}
		}

		public override void Dispose()
		{
			base.Dispose();

			if (docket != null && subscriber != null)
			{
				docket.NotificationManager.Pop();
			}
		}

		readonly WhsDocket docket;
		readonly INotifications subscriber;
	}
}
