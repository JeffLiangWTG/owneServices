using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public abstract class ReceiveActionProcessor : IProcessor
	{
		protected ReceiveActionProcessor(WhsReceive receive)
		{
			Argument.NotNull(receive, nameof(receive));

			this.Receive = receive;
		}

		WhsReceive Receive { get; }

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			Argument.NotNull(notifications, nameof(notifications));

			if (PerformAction(Receive, notifications))
			{
				var receiveNotifications = Receive.NotificationSubscriber as NotificationBuffer;
				if (receiveNotifications == null)
				{
					notifications.AddWarning(Res.GetString("80F1A90B-603B-4B66-AF48-AB6582BBB229", "Unable to {0} for receive {1}.", ActionName, Receive.WD_DocketID));
				}
				else if (receiveNotifications.HasErrors)
				{
					notifications.AddError(Res.GetString("4CD6A5F5-BCF1-4AB7-B21D-591FBF0CE16C", "Unable to {0} for receive {1} because it is finalized or canceled, or at least one of its lines has a putaway transfer.", ActionName, Receive.WD_DocketID));
				}
				else
				{
					notifications.Add(NotificationType.Information, Res.GetString("D8EE9D1C-4683-44E2-8740-C813072026A2", "Operation {0} for receive {1} completed.", ActionName, Receive.WD_DocketID));
				}
			}
		}

		protected abstract ZString ActionName { get; }

		protected abstract bool PerformAction(WhsReceive receive, INotifications notifications);
	}
}
