using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuoteInvoicingSupporter))]
	public class QuoteInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public override void TestCustomsEntryNumberType()
		{
			var quote = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quote.Booking.CustomsEntryNumberType = "TF";
			var supporter = new QuoteInvoicingSupporter(quote);
			AssertEquals("CustomsEntryNumberType", "TF", supporter.CustomsEntryNumberType);
		}

		public override void TestCommunityTransitStatus()
		{
			var quote = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quote.Booking.JS_CommunityTransitStatus = "TF";
			var supporter = new QuoteInvoicingSupporter(quote);
			AssertEquals("CommunityTransitStatus", "TF", supporter.CommunityTransitStatus);
		}

		public void TestDefaultCreditor()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			var quote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			quote.OH_Carrier = carrier.PK;
			quote.Creditor = creditor.PK;
			var pChargeCode = Factory.New<AccChargeCode>();
			pChargeCode.AC_Code = "_PC";

			var supporter = new QuoteInvoicingSupporter(quote);
			var defaultCreditor = supporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, "FRT", carrier.PK));

			AssertEquals("Quote Carrier Matched Service Provider, Quote Creditor as the default creditor", creditor.OH_Code, defaultCreditor.OH_Code);

			quote.Creditor = ZGuid.Empty;
			supporter = new QuoteInvoicingSupporter(quote);
			defaultCreditor = supporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, "FRT", carrier.PK));
			AssertEquals("Quote Carrier Matched Service Provider and Creditor is Empty, Quote Creditor as the default creditor", carrier.OH_Code, defaultCreditor.OH_Code);

			quote.OH_Carrier = ZGuid.Empty;
			supporter = new QuoteInvoicingSupporter(quote);
			defaultCreditor = supporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, "FRT", carrier.PK));
			AssertNull(defaultCreditor);

			var possibleCarrier = quote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier.TTC_OH_Carrier = carrier.PK;
			supporter = new QuoteInvoicingSupporter(quote);
			defaultCreditor = supporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, "FRT", carrier.PK));
			AssertEquals("Quote Possible Carrier Matched Service Provider and Possible Creditor is Empty, Possible Carrier as the default creditor", carrier.OH_Code, defaultCreditor.OH_Code);

			possibleCarrier.TTC_OH_Creditor = creditor.PK;
			supporter = new QuoteInvoicingSupporter(quote);
			defaultCreditor = supporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, "FRT", carrier.PK));
			AssertEquals("Quote Possible Carrier Matched Service Provider, Quote Possible Carrier as the default creditor", creditor.OH_Code, defaultCreditor.OH_Code);

			defaultCreditor = supporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, "FRT", creditor.PK));
			AssertEquals("Quote Possible Creditor Matched Service Provider, Quote Possible Creditor as the default creditor", creditor.OH_Code, defaultCreditor.OH_Code);
		}

		public void TestDefaultCreditor_WhenNoCarrierInPossibleCarrier_ShouldNotThrowException()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var pChargeCode = Factory.New<AccChargeCode>();
			pChargeCode.AC_Code = "_PC";

			var quote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			var possibleCarrier = quote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();

			possibleCarrier.TTC_OH_Creditor = creditor.PK;
			var supporter = new QuoteInvoicingSupporter(quote);
			AssertNoExceptionThrown(() => supporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, "FRT", carrier.PK)));
		}

		#region Implementation
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			return QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
		}
		#endregion
	}
}
