using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Freight.QuotedBookings.Business.QuotedBookingToShipmentConverter;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Freight.QuotedBookings.DataTransfer
{
	class ConvertToShipmentProcessor : IProcessor
	{
		public ConvertToShipmentProcessor(QuotedBooking quotedBooking)
		{
			QuotedBooking = Argument.NotNull(quotedBooking, nameof(quotedBooking));
		}

		#region IProcessor Members

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			var converter = new QuotedBookingToShipmentConverter(QuotedBooking, BookingToShipmentConversionSource.WorkflowTrigger);

			if (converter.HasAnyErrors(out string errorMessage))
			{
				LogErrorAndIgnoreTrigger(notifications, errorMessage);
			}
			else
			{
				converter.ConvertBookingToShipment(QuotedBooking.Booking);
			}
		}

		#endregion

		void LogErrorAndIgnoreTrigger(INotifications notifications, ZString error)
		{
			if (!error.IsEmpty)
			{
				error = Res.GetString("798a6ab5-0044-4f15-a942-faf1e5f9990b", "Unable to convert Booking to Shipment: {0}", error);
				notifications.Add(NotificationType.Error, error);
			}
		}

		readonly QuotedBooking QuotedBooking;
	}
}
