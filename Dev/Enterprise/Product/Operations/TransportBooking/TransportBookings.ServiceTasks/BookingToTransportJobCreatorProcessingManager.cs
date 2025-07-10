using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Integration;
using Enterprise.TransportBookings.Business;

namespace Enterprise.TransportBookings.ServiceTasks
{
	class BookingToTransportJobCreatorProcessingManager
	{
		public BookingToTransportJobCreatorProcessingManager(ILogger logger)
		{
			Logger = logger;
		}

		readonly ILogger Logger;

		public void CreateTransportJobsFromTransportBookings()
		{
			var processingManager = new BookingToTransportJobCommonCreator(new WrappedNotifications(Logger));

			processingManager.CreateTransportJobsFromTransportBookings();
		}

		class WrappedNotifications : INotifications
		{
			public WrappedNotifications(ILogger logger)
			{
				Logger = Argument.NotNull(logger, "logger");
			}

			readonly ILogger Logger;

			void INotifications.Add(INotification notification)
			{
				if (notification.Type == NotificationType.Error)
				{
					Logger.Error(notification.Message);
				}
				else if (notification.Type == NotificationType.Warning)
				{
					Logger.Information(notification.Message);
				}
			}
		}
	}
}
