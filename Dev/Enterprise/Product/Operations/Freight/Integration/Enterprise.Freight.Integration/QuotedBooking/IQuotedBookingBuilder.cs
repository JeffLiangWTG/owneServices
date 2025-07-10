using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Integration.QuotedBooking
{
	public interface IQuotedBookingBuilder : IBusinessObjectLoadStrategy
	{
		IQuotedBooking CreateNew(QuoteBookingType quoteBookingType, BusinessObjectFactory factory);
		IQuotedBooking InitializeFrom(ZGuid bookingPK, BusinessObjectFactory factory);
		IQuotedBooking InitializeFrom(ZGuid quotePK, ZGuid bookingPK, BusinessObjectFactory factory);
		IQuotedBooking CreateAndPopulateFromOrder(ZGuid orderPK, BusinessObjectFactory factory);
		BusinessObject LoadViewQuotedBooking(BusinessObjectFactory factory, ZGuid shipmentPK);
	}
}
