using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.QuotedBookings.Business
{
	[ModuleID(ModuleId.OneOffQuotes)]
	public class ViewOneOffQuoteCollection : ViewQuotedBookingCollection, IViewOneOffQuoteCollection
	{
		public ViewOneOffQuoteCollection(BusinessObjectFactory factory)
			: base(factory) { }
	}
}
