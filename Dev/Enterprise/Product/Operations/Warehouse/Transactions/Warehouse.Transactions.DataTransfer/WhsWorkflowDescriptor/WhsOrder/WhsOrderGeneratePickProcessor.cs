using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsOrderGeneratePickProcessor

	public class WhsOrderGeneratePickProcessor : IProcessor
	{
		public WhsOrderGeneratePickProcessor(WhsPickableDocket pickableDocket)
		{
			PickableDocket = Argument.NotNull(pickableDocket, nameof(pickableDocket));
		}
		readonly WhsPickableDocket PickableDocket;

		void IProcessor.Process(INotifications notifications, CancellationToken unused)
		{
			var logger = new GeneratePickNotificationLogger(notifications);
			GeneratePickManager.GeneratePick(PickableDocket, logger, saveFactory: false);
		}

		#region GeneratePickNotificationLogger

		class GeneratePickNotificationLogger : IGeneratePickLogger
		{
			public GeneratePickNotificationLogger(INotifications notifications)
			{
				Notifications = Argument.NotNull(notifications, nameof(notifications));
			}
			readonly INotifications Notifications;

			#region IGeneratePickLogger

			void IGeneratePickLogger.LogWarning(string message) => Notifications.AddWarning(message);

			void IGeneratePickLogger.LogError(string message) => Notifications.AddError(message);

			void IGeneratePickLogger.LogError(WhsPick pick, string message) => Notifications.AddError(message);

			void IGeneratePickLogger.LogSaveConcurrencyException(WhsPick pick, string docketID)
				=> Notifications.AddError(DataTransfer.Res.GetString("5e91fcd3-d957-494c-8938-92a12b702699", "Order: {0} was modified by another user while attempting to automatically generate a Pick. Create the Pick manually.", docketID));

			#endregion
		}

		#endregion
	}

	#endregion
}
