using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.DataTransfer
{
	public class HVLVPreScreeningProcessor : IProcessor
	{
		public HVLVPreScreeningProcessor(ForwardingShipment shipment)
		{
			screeningProvider = new ETailPreScreeningProvider(shipment.GetHVLVConsignmentHeader());
		}

		public HVLVPreScreeningProcessor(HVLVBookingHeader bookingHeader)
		{
			screeningProvider = new ETailPreScreeningProvider(bookingHeader);
		}

		readonly ETailPreScreeningProvider screeningProvider;

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			screeningProvider.Screen();
			screeningProvider.SyncScreeningResult();
			screeningProvider.AddLog();
			screeningProvider.SendPreScreeningNotificationEmail();
			if (screeningProvider.ScreeningResult.ErrorMessage != null)
			{
				notifications.AddError(screeningProvider.ScreeningResult.ErrorMessage);
			}
		}
	}
}
