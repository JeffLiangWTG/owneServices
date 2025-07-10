using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class QuotedBookingBuilderTest : TestCaseWithFactory
	{
		public void TestInitializeFrom()
		{
			ForwardingShipment booking = Factory.New<ForwardingShipment>();
			booking.JS_IsBooking = true;
			booking.JS_IsForwardRegistered = false;
			Quote quote = Factory.New<Quote>();
			quote.TH_OneTimeQuote = true;
			IQuotedBooking quotedBooking = Builder.InitializeFrom(quote.PK, booking.PK, Factory);
			AssertEquals(quote.PK, quotedBooking.Quote.PK);
			AssertEquals(booking.PK, quotedBooking.ForwardingShipment.PK);
			quotedBooking = Builder.InitializeFrom(ZGuid.Empty, booking.PK, Factory);
			AssertEquals(booking.PK, quotedBooking.ForwardingShipment.PK);
			quotedBooking = Builder.InitializeFrom(booking.PK, Factory);
			AssertEquals(booking.PK, quotedBooking.ForwardingShipment.PK);
			AssertNull(Builder.InitializeFrom(ZGuid.Empty, ZGuid.NewZGuid(), Factory));
		}

		public void TestCreateNew()
		{
			IQuotedBooking bookingWithQuote = Builder.CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			AssertNotNull(bookingWithQuote.ForwardingShipment);
			AssertNotNull(bookingWithQuote.Quote);
			IQuotedBooking quickBooking = Builder.CreateNew(QuoteBookingType.QuickBooking, Factory);
			AssertNotNull(quickBooking.ForwardingShipment);
			AssertNull(quickBooking.Quote);
			IQuotedBooking spotQuote = Builder.CreateNew(QuoteBookingType.SpotQuote, Factory);
			AssertNull(spotQuote.ForwardingShipment);
			AssertNotNull(spotQuote.Quote);
		}

		#region TestLoad
		public void TestLoad_BookingWithQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var quotedBookingFactory2 = (QuotedBooking)Builder.Load(factory2, quotedBooking.PK);
			AssertEquals("expected the same quote", quotedBooking.Quote.PK, quotedBookingFactory2.Quote.PK);
			AssertEquals("expected the same booking", quotedBooking.Booking.PK, quotedBookingFactory2.Booking.PK);
		}

		public void TestLoad_BookingWithQuote_LoadedByBookingPK()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var quotedBookingReloaded = (QuotedBooking)Builder.Load(Factory, quotedBooking.Booking.PK);
			AssertEquals("expected the same quote", quotedBooking.Quote.PK, quotedBookingReloaded.Quote.PK);
			AssertEquals("expected the same booking", quotedBooking.Booking.PK, quotedBookingReloaded.Booking.PK);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var quotedBookingFactory2 = (QuotedBooking)Builder.Load(factory2, quotedBooking.Booking.PK);
			AssertEquals("expected the same quote", quotedBooking.Quote.PK, quotedBookingFactory2.Quote.PK);
			AssertEquals("expected the same booking", quotedBooking.Booking.PK, quotedBookingFactory2.Booking.PK);
		}

		public void TestLoad_QuickBooking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var quotedBookingFactory2 = (QuotedBooking)Builder.Load(factory2, quotedBooking.PK);
			AssertNull("expected no quote", quotedBookingFactory2.Quote);
			AssertEquals("expected the same booking", quotedBooking.Booking.PK, quotedBookingFactory2.Booking.PK);
		}

		public void TestLoad_SpotQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var quotedBookingFactory2 = (QuotedBooking)Builder.Load(factory2, quotedBooking.PK);
			AssertEquals("expected the same quote", quotedBooking.Quote.PK, quotedBookingFactory2.Quote.PK);
			AssertNull("expected no booking", quotedBookingFactory2.Booking);
		}

		#endregion
		#region TestLoadWithStrategy UsingInterface
		public void TestLoad_QuotedBooking_UsingInterface()
		{
			AssertLoad_UsingInterface(QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory));
		}

		public void TestLoad_Booking_UsingInterface()
		{
			AssertLoad_UsingInterface(QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
		}

		public void TestLoad_SpotQuote_UsingInterface()
		{
			AssertLoad_UsingInterface(QuotedBooking.New(QuoteBookingType.SpotQuote, Factory));
		}

		void AssertLoad_UsingInterface(QuotedBooking quotedBooking)
		{
			var loadedQuotedBooking = Factory.Load<IQuotedBooking>(quotedBooking.PK);
			AssertEquals("found not persisted quotedBooking", quotedBooking, loadedQuotedBooking);
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			loadedQuotedBooking = otherFactory.Load<IQuotedBooking>(quotedBooking.PK);
			AssertNotNull("found persisted quotedBooking", loadedQuotedBooking);
			AssertEquals("found persisted quotedBooking", quotedBooking.PK, ((IWorkflowProvider)loadedQuotedBooking).PK);
		}

		#endregion
		#region TestCreateAndPopulateFrom
		public void TestCreateAndPopulateFromOrder()
		{
			var order = Factory.New<Order>();
			order.BuyerPK = ZGuid.NewZGuid();
			order.SupplierPK = ZGuid.NewZGuid();
			order.JD_Packs = 3;
			order.JD_ActualVolume = 10;
			order.JD_ActualWeight = 20;
			order.JD_Waybill = "HOUSEBILL";
			order.JD_RS_NKServiceLevel_NI = "STD";
			order.JD_RL_NKPortOfLoading = "LOADP";
			order.JD_RL_NKPortOfDischarge = "DISCP";
			order.JD_OH_Carrier = Factory.NewWithValidTestData<OrgHeader>().PK;
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			order.JD_OrderGoodsDescription = "GOODS";
			var quotedBooking = (QuotedBooking)Builder.CreateAndPopulateFromOrder(order.PK, Factory);
			CombineAssertions("Booking should be populated", () =>
			{
				AssertNotNull("Booking is created", quotedBooking);
				AssertEquals(order.BuyerPK, quotedBooking.Booking.ConsigneePK);
				AssertEquals(order.SupplierPK, quotedBooking.Booking.ConsignorPK);
				AssertEquals(order.Packs, quotedBooking.Booking.JS_OuterPacks);
				AssertEquals(order.Volume, quotedBooking.Booking.JS_ActualVolume);
				AssertEquals(order.Weight, quotedBooking.Booking.JS_ActualWeight);
				AssertEquals(order.JD_Waybill, quotedBooking.Booking.JS_HouseBill);
				AssertEquals(order.JD_RS_NKServiceLevel_NI, quotedBooking.ServiceLevel);
				AssertEquals(order.JD_RL_NKPortOfLoading, quotedBooking.Origin);
				AssertEquals(order.JD_RL_NKPortOfDischarge, quotedBooking.Destination);
				AssertEquals(order.JD_OH_Carrier, quotedBooking.OH_Carrier);
				AssertEquals(Core.Constants.ContainerModes.FCL, quotedBooking.Mode);
				AssertEquals(order.JD_RL_NKPortOfLoading, quotedBooking.LoadPort);
				AssertEquals(order.JD_RL_NKPortOfDischarge, quotedBooking.DischargePort);
				AssertEquals(order.JD_OrderGoodsDescription, quotedBooking.GoodsDescription);
			}

			);
		}

		#endregion
		#region TestLoadWithStrategy UsingConcreteType
		public void TestLoad_QuotedBooking_UsingConcreteType()
		{
			AssertLoad_UsingConcreteType(QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory));
		}

		public void TestLoad_Booking_UsingConcreteType()
		{
			AssertLoad_UsingConcreteType(QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
		}

		public void TestLoad_SpotQuote_UsingConcreteType()
		{
			AssertLoad_UsingConcreteType(QuotedBooking.New(QuoteBookingType.SpotQuote, Factory));
		}

		void AssertLoad_UsingConcreteType(QuotedBooking quotedBooking)
		{
			var loadedQuotedBooking = Factory.Load<QuotedBooking>(quotedBooking.PK);
			AssertEquals("found not persisted quotedBooking", quotedBooking, loadedQuotedBooking);
			loadedQuotedBooking = Factory.Load<QuotedBooking>(quotedBooking.PK);
			AssertEquals("found not persisted quotedBooking", quotedBooking, loadedQuotedBooking);
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			loadedQuotedBooking = otherFactory.Load<QuotedBooking>(quotedBooking.PK);
			AssertNotNull("found persisted quotedBooking", loadedQuotedBooking);
			AssertEquals("found persisted quotedBooking", quotedBooking.PK, loadedQuotedBooking.PK);
			loadedQuotedBooking = otherFactory.Load<QuotedBooking>(quotedBooking.PK);
			AssertNotNull("found persisted quotedBooking", loadedQuotedBooking);
			AssertEquals("found persisted quotedBooking", quotedBooking.PK, loadedQuotedBooking.PK);
		}

		#endregion

		#region TestLoadViewQuotedBooking

		public void TestLoadViewQuotedBooking()
		{
			var booking = Factory.New<ForwardingShipment>();
			booking.JS_IsBooking = true;
			booking.JS_IsForwardRegistered = false;
			Factory.Save();

			var viewQuotedBooking = Builder.LoadViewQuotedBooking(Factory, booking.PK);

			AssertNotNull(viewQuotedBooking);
			AssertEquals("bizo should be ViewQuotedBooking", typeof(ViewQuotedBooking), viewQuotedBooking.GetType());
		}

		#endregion

		#region Implementation
		QuotedBookingBuilder Builder
		{
			get
			{
				return builder ?? (builder = new QuotedBookingBuilder());
			}
		}

		QuotedBookingBuilder builder;
		#endregion
	}
}
