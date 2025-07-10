using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.Forwarding.Business.Test
{
	public class BookingDataTest : TestCaseWithFactory
	{
		public void TestBothQuoteAndShipmentCodesCanBeHandled()
		{
			Assert(((ViewQuotedBookingCollection)new BookingData().GetBusinessObjectCollection(new BusinessObjectFactory())).CanHandleBothQuoteAndShipmentCodes);
		}

		public void TestFindBoxListProvider()
		{
			Quote quote1 = Factory.NewWithValidTestData<Quote>();
			quote1.TH_OneTimeQuote = true;
			quote1.TH_QuoteNumber = "111";
			Quote quote2 = Factory.NewWithValidTestData<Quote>();
			quote2.TH_OneTimeQuote = true;
			quote2.TH_QuoteNumber = "222";
			Quote quote3 = Factory.NewWithValidTestData<Quote>();
			quote3.TH_OneTimeQuote = true;
			quote3.TH_QuoteNumber = "333";
			ForwardingShipment shp1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shp2 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shp3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shp1.JS_UniqueConsignRef = "B00001001";
			shp2.JS_UniqueConsignRef = "B00001002";
			shp3.JS_UniqueConsignRef = "B00001003";
			ViewQuotedBooking qb1 = Factory.New<ViewQuotedBooking>();
			qb1.VB_TH = quote1.PK;
			qb1.VB_JS = shp1.PK;
			ViewQuotedBooking qb2 = Factory.New<ViewQuotedBooking>();
			qb2.VB_TH = quote2.PK;
			qb2.VB_JS = shp2.PK;
			ViewQuotedBooking qb3 = Factory.New<ViewQuotedBooking>();
			qb3.VB_TH = quote3.PK;
			qb3.VB_JS = shp3.PK;
			BookingData bd = new BookingData();
			ViewQuotedBookingCollection list = (ViewQuotedBookingCollection)bd.GetBusinessObjectCollection(Factory);
			list.Add(qb1);
			list.Add(qb2);
			list.Add(qb3);
			AssertEquals(qb1, ((IFindBoxListProvider)list).GetBusinessObjectFromCode(quote1.TH_QuoteNumber));
			AssertEquals(qb2, ((IFindBoxListProvider)list).GetBusinessObjectFromCode(quote2.TH_QuoteNumber));
			AssertEquals(qb3, ((IFindBoxListProvider)list).GetBusinessObjectFromCode(quote3.TH_QuoteNumber));
			AssertEquals(qb1, ((IFindBoxListProvider)list).GetBusinessObjectFromCode("B00001001"));
			AssertEquals(qb2, ((IFindBoxListProvider)list).GetBusinessObjectFromCode("B00001002"));
			AssertEquals(qb3, ((IFindBoxListProvider)list).GetBusinessObjectFromCode("B00001003"));
		}
	}
}
