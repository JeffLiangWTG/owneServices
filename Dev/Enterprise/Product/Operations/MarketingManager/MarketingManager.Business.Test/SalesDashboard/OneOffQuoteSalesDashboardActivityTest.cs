using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OneOffQuoteSalesDashboardActivity))]
	sealed class OneOffQuoteSalesDashboardActivityTest : SalesDashboardActivityTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var builder = ObjectFactory.Get<IQuotedBookingBuilder>();
			var quotedBooking = builder.CreateNew(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			var parent = Factory.LoadTop1<RateOneOffShipment>(new ZQuery(RateOneOffShipmentSchema.TT_TH, quotedBooking.ViewPK));
			var activity = Factory.New<OneOffQuoteSalesDashboardActivity>();
			activity.VSA_ParentId = parent.PK;
			return activity;
		}
	}
}
