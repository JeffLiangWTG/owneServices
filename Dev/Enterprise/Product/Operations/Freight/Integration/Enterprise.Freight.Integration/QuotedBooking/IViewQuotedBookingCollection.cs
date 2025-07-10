using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Integration.QuotedBooking
{
	public interface IViewQuotedBookingCollection : IBusinessObjectCollection
	{
		bool CanHandleBothQuoteAndShipmentCodes { get; set; }

		event System.Func<BusinessObject, INotification> GetExtraNotificationHanlder;
	}
}
