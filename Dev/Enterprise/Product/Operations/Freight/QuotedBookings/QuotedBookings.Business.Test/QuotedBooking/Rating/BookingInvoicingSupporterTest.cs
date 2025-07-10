using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(BookingInvoicingSupporter))]
	public class BookingInvoicingSupporterTest : ForwardingShipmentInvoicingSupporterTest
	{
		public override void TestCustomsEntryNumberType()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.CustomsEntryNumberType = "TF";
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var supporter = quotedBooking.InvoicingSupporter;
			AssertEquals("CustomsEntryNumberType", "TF", supporter.CustomsEntryNumberType);
		}

		public override void TestCommunityTransitStatus()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_CommunityTransitStatus = "TF";
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var supporter = quotedBooking.InvoicingSupporter;
			AssertEquals("CommunityTransitStatus", "TF", supporter.CommunityTransitStatus);
		}

		public void TestArrivalAtLoadPort()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var supporter = quotedBooking.InvoicingSupporter;
			AssertEquals("ATL", ZDateTime.Empty, supporter.ArrivalAtLoadPort);
			var today = ZDateTime.Today;
			var sailing_E_ATL = today.AddDays(20);
			var sailing_A_ATL = today.AddDays(21);
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "12";
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("APL IVORY", Factory).First().RV_FK;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
			var sailing = voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");
			AssertNotNull("Sailing should exist", sailing);
			AssertEquals("TestQuotedBooking has NO Sailing", null, quotedBooking.ScheduleChooser.Sailing);
			AssertEquals("ATL: TestQuotedBooking has NO Sailing - Empty", ZDateTime.Empty, supporter.ArrivalAtLoadPort);
			((ISailingChooserParent)quotedBooking).SailingJX = sailing.PK;
			AssertEquals("TestQuotedBooking has Sailing", sailing, quotedBooking.ScheduleChooser.Sailing);
			AssertEquals("Sailing A_ATL: Empty", ZDateTime.Empty, quotedBooking.ScheduleChooser.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: Empty", ZDateTime.Empty, quotedBooking.ScheduleChooser.Sailing.Origin.JA_E_ARV);
			AssertEquals("ATL: Sailing A_ATL and E_ATL are Empty - Empty", ZDateTime.Empty, supporter.ArrivalAtLoadPort);
			origin.JA_E_ARV = sailing_E_ATL;
			AssertEquals("Sailing A_ATL: Empty", ZDateTime.Empty, quotedBooking.ScheduleChooser.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: sailing_E_ATL", sailing_E_ATL, quotedBooking.ScheduleChooser.Sailing.Origin.JA_E_ARV);
			AssertEquals("ATL: Sailing A_ATL is Empty - Sailing E_ATL", sailing_E_ATL, supporter.ArrivalAtLoadPort);
			origin.JA_A_ARV = sailing_A_ATL;
			AssertEquals("Sailing A_ATL: sailing_A_ATL", sailing_A_ATL, quotedBooking.ScheduleChooser.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: sailing_E_ATL", sailing_E_ATL, quotedBooking.ScheduleChooser.Sailing.Origin.JA_E_ARV);
			AssertEquals("ATL: Sailing A_ATL", sailing_A_ATL, supporter.ArrivalAtLoadPort);
		}

		public void TestDefaultCreditor()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var serviceOperator = Factory.NewWithValidTestData<OrgHeader>();
			serviceOperator.OH_IsShippingProvider = true;

			var bookingWithQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			bookingWithQuote.OH_Carrier = carrier.PK;
			bookingWithQuote.Creditor = creditor.PK;

			var pChargeCode = Factory.New<AccChargeCode>();
			pChargeCode.AC_Code = "_PC";

			var supporter = new BookingInvoicingSupporter(bookingWithQuote);

			// Test 1: Default Creditor from QuotedBooking
			AssertCreditor(supporter, pChargeCode, "FRT", carrier.PK, creditor.OH_Code, "Quote Carrier and Creditor");

			// Test 2: Empty creditor
			bookingWithQuote.Creditor = ZGuid.Empty;
			supporter = new BookingInvoicingSupporter(bookingWithQuote);
			AssertCreditor(supporter, pChargeCode, "FRT", carrier.PK, carrier.OH_Code, "Quote Carrier Creditor Empty");

			// Test 3: Standard Cost
			bookingWithQuote.OH_Carrier = ZGuid.Empty;
			supporter = new BookingInvoicingSupporter(bookingWithQuote);
			AssertCreditorNull(supporter, pChargeCode, "FRT", carrier.PK, "Standard Cost");

			var fields = new (Action setField, Action resetField, string description)[]
			{
				(() => bookingWithQuote.Booking.DocsAndCartage.JP_OA_PickupCartageCoAddr_ZAddress.OrgPK = serviceOperator.PK,
				 () => bookingWithQuote.Booking.DocsAndCartage.JP_OA_PickupCartageCoAddr_ZAddress.OrgPK = ZGuid.Empty,
				 "Port Transport"),

				(() => bookingWithQuote.ExportReceivingDepot_ZAddress.OrgPK = serviceOperator.PK,
				 () => bookingWithQuote.ExportReceivingDepot_ZAddress.OrgPK = ZGuid.Empty,
				 "Pickup CFS"),

				(() => bookingWithQuote.ImportReleaseDepot_ZAddress.OrgPK = serviceOperator.PK,
				 () => bookingWithQuote.ImportReleaseDepot_ZAddress.OrgPK = ZGuid.Empty,
				 "Delivery CFS"),

				(() => bookingWithQuote.Booking.PickupAgentDocumentaryAddress.OrganisationPK = serviceOperator.PK,
				 () => bookingWithQuote.Booking.PickupAgentDocumentaryAddress.OrganisationPK = ZGuid.Empty,
				 "Pickup Agent"),

				(() => bookingWithQuote.Booking.JS_OH_DeliveryAgent = serviceOperator.PK,
				 () => bookingWithQuote.Booking.JS_OH_DeliveryAgent = ZGuid.Empty,
				 "Delivery Agent"),

				(() => bookingWithQuote.Booking.JS_OH_ExportBroker = serviceOperator.PK,
				 () => bookingWithQuote.Booking.JS_OH_ExportBroker = ZGuid.Empty,
				 "Export Broker"),

				(() => bookingWithQuote.Booking.JS_OH_ImportBroker = serviceOperator.PK,
				 () => bookingWithQuote.Booking.JS_OH_ImportBroker = ZGuid.Empty,
				 "Import Broker"),

				(() => bookingWithQuote.Booking.ControllingCustomerAddress.OrganisationPK = serviceOperator.PK,
				 () => bookingWithQuote.Booking.ControllingCustomerAddress.OrganisationPK = ZGuid.Empty,
				 "Controlling Customer"),

				(() => bookingWithQuote.Booking.ControllingAgentDocumentaryAddress.OrganisationPK = serviceOperator.PK,
				 () => bookingWithQuote.Booking.ControllingAgentDocumentaryAddress.OrganisationPK = ZGuid.Empty,
				"Controlling Agent")
			};

			foreach (var (setField, resetField, description) in fields)
			{
				setField();
				AssertCreditor(supporter, pChargeCode, "FRT", serviceOperator.PK, serviceOperator.OH_Code, $"{description} charges should have own creditor");

				resetField();
				AssertCreditorNull(supporter, pChargeCode, "FRT", serviceOperator.PK, $"{description} field cleared");
			}
		}

		void AssertCreditor(BookingInvoicingSupporter supporter, AccChargeCode pChargeCode, string chargeCode, ZGuid carrierPK, string expectedCreditorCode, string message)
		{
			var defaultCreditor = supporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, chargeCode, carrierPK));
			AssertEquals(message, expectedCreditorCode, defaultCreditor.OH_Code);
		}

		void AssertCreditorNull(BookingInvoicingSupporter supporter, AccChargeCode pChargeCode, string chargeCode, ZGuid carrierPK, string message)
		{
			var defaultCreditor = supporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, chargeCode, carrierPK));
			AssertNull(message, defaultCreditor);
		}

		public override void TestJob()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = new QuotedBookingTest.TestQuotedBookingExposer(ZGuid.Empty, booking.PK, Factory);
			new JobHeader.Loader(quickBooking).TryLoadOrCreate();
			var quickBookingSupporter = quickBooking.InvoicingSupporter;
			AssertNotNull(quickBookingSupporter.Job);
			AssertEquals(booking.PK, quickBookingSupporter.Job.Parent.PK);
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			booking = QuotedBooking.CreateNewBooking(Factory);
			var bookingWithQuote = new QuotedBookingTest.TestQuotedBookingExposer(quote.PK, booking.PK, Factory);
			new JobHeader.Loader(bookingWithQuote).TryLoadOrCreate();
			var bookingWithQuoteSupporter = bookingWithQuote.InvoicingSupporter;
			AssertNotNull(bookingWithQuoteSupporter.Job);
			AssertEquals(quote.PK, bookingWithQuoteSupporter.Job.Parent.PK);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			return QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
		}
	}
}
