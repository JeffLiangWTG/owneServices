using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class QuotedBookingRateLineConditionsSupporterTest : RateLineConditionsSupporterTest<QuotedBooking, QuotedBookingRateLineConditionsSupporter>
	{
		protected override QuotedBooking GetInterfacedObject()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment shipment = QuotedBooking.CreateNewBooking(Factory);
			return QuotedBooking.New(quote.PK, shipment.PK, Factory);
		}

		protected override OrgHeader SetExportBroker(QuotedBooking adapter)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			adapter.Booking.JS_OH_ExportBroker = org.PK;
			return adapter.Booking.ExportBroker;
		}

		protected override OrgHeader SetImportBroker(QuotedBooking adapter)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			adapter.Booking.JS_OH_ImportBroker = org.PK;
			return org;
		}

		protected override OrgHeader SetSendingAgent(QuotedBooking adapter)
		{
			return null;
		}

		protected override OrgHeader SetReceivingAgent(QuotedBooking adapter)
		{
			return null;
		}

		protected override OrgHeader SetControllingAgent(QuotedBooking adapter)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			adapter.Booking.ControllingAgentDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			return org;
		}

		protected override OrgHeader SetDepartureCFS(QuotedBooking adapter)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			adapter.Booking.JS_OA_ExportReceivingDepot = org.MainAddress.PK;
			AssertEquals(org.MainAddress.PK, adapter.ExportReceivingDepot);
			return org;
		}

		protected override OrgHeader SetArrivalCFS(QuotedBooking adapter)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			adapter.Booking.JS_OA_ImportReleaseDepot = org.MainAddress.PK;
			AssertEquals(org.MainAddress.PK, adapter.ImportReleaseDepot);
			return org;
		}

		public void TestArrivalCFSAndDepartureCFSWhenBookingIsNull()
		{
			var quotePK = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted).PK;
			var spotQuote = QuotedBooking.New(quotePK, ZGuid.Empty, Factory);
			spotQuote.Mode = Core.Constants.RateMode.LSE;
			spotQuote.PaymentTerms = ZString.Empty;
			spotQuote.Origin = "AUSYD";
			spotQuote.Destination = "CNSHA";

			var supporter = new QuotedBookingRateLineConditionsSupporter(spotQuote);
			AssertNoExceptionThrown(delegate()
			{
				var arrivalCFS = supporter.ArrivalCFS;
				var departureCFS = supporter.DepartureCFS;
			});
		}

		public override void TestHasDangerousGoods()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var booking = QuotedBooking.New(quote.PK, shipment.PK, Factory);

			var supporter = new QuotedBookingRateLineConditionsSupporter(booking);

			Assert(!supporter.HasDangerousGoods);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.UNDGs.AddNew();

			Assert("At this point only shipments can have dg", supporter.HasDangerousGoods);
		}

		public override void TestMeetsCondition()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.New(quote.PK, new Guid(), Factory);

			var supporter = new QuotedBookingRateLineConditionsSupporter(booking);
			Assert(supporter.MeetsCondition(RateLineConditions.ForwardingAndBrokerage, null, null, false, false));
			Assert(supporter.MeetsCondition(RateLineConditions.HandOver, null, null, false, false));
			Assert(supporter.MeetsCondition(RateLineConditions.OwnBrokerage, null, null, false, false));
			Assert(supporter.MeetsCondition(RateLineConditions.OwnCFS, null, null, false, false));

			Assert(!supporter.MeetsCondition(RateLineConditions.OwnGateway, null, null, false, false));
			Assert(supporter.MeetsCondition(RateLineConditions.UserDefined, "test", (c, s) => { return !c.IsEmpty && s != null; }, false, false));
		}

		public void TestMeetsCondition_BookingWithQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var booking = QuotedBooking.New(quote.PK, shipment.PK, Factory);
			var supporter = new QuotedBookingRateLineConditionsSupporter(booking);

			// Test the four overridden conditions don't pass
			Assert(!supporter.MeetsCondition(RateLineConditions.ForwardingAndBrokerage, null, null, false, false));
			Assert(!supporter.MeetsCondition(RateLineConditions.HandOver, null, null, false, false));
			Assert(!supporter.MeetsCondition(RateLineConditions.OwnBrokerage, null, null, false, false));
			Assert(!supporter.MeetsCondition(RateLineConditions.OwnCFS, null, null, false, false));

			// Test a few of the override conditions for the base case just to see they return correctly.
			// All possible values are tested in the base test for MeetsCondition();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.UNDGs.AddNew();
			Assert(supporter.MeetsCondition(RateLineConditions.DangerousGoods, null, null, false, false));

			var org = Factory.NewWithValidTestData<OrgHeader>();
			booking.ControllingAgentDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			Assert(!supporter.MeetsCondition(RateLineConditions.OwnControllingAgent, null, null, false, false));
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			Assert(supporter.MeetsCondition(RateLineConditions.OwnControllingAgent, null, null, false, false));
			Assert(!supporter.MeetsCondition(RateLineConditions.OwnGateway, null, null, false, false));
			Assert(supporter.MeetsCondition(RateLineConditions.UserDefined, "test", (c, s) => { return !c.IsEmpty && s != null; }, false, false));

			Factory.Save();

			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;

			shipment.JS_OH_ExportBroker = new Guid();
			shipment.JS_OH_ImportBroker = new Guid();
			Assert("Shipment is not OwnBroker when current export company OrgProxy is not the Broker", !supporter.MeetsCondition(RateLineConditions.OwnBrokerage, null, null, true, true));
			Assert("Shipment is HandOver when current export company OrgProxy is not the Broker", supporter.MeetsCondition(RateLineConditions.HandOver, null, null, true, true));

			shipment.JS_OH_ExportBroker = orgProxy.PK;
			shipment.JS_OH_ImportBroker = new Guid();
			Assert("Shipment is OwnBroker when current export company OrgProxy is the Broker", supporter.MeetsCondition(RateLineConditions.OwnBrokerage, null, null, true, true));
			Assert("Shipment is HandOver when current export company OrgProxy is the Broker", supporter.MeetsCondition(RateLineConditions.HandOver, null, null, true, true));

			shipment.JS_OH_ExportBroker = new Guid();
			shipment.JS_OH_ImportBroker = orgProxy.PK;
			Assert("Shipment is OwnBroker when current import company OrgProxy is the Broker", supporter.MeetsCondition(RateLineConditions.OwnBrokerage, null, null, true, true));
			Assert("Shipment is HandOver when current import company OrgProxy is the Broker", supporter.MeetsCondition(RateLineConditions.HandOver, null, null, true, true));

			shipment.JS_OH_ExportBroker = orgProxy.PK;
			shipment.JS_OH_ImportBroker = orgProxy.PK;
			Assert("Shipment is OwnBroker when both current import and export company OrgProxy is the Broker", supporter.MeetsCondition(RateLineConditions.OwnBrokerage, null, null, true, true));
			Assert("Shipment is not HandOver when both current import and export company OrgProxy is the Broker", !supporter.MeetsCondition(RateLineConditions.HandOver, null, null, true, true));
		}
	}
}
