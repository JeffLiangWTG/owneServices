using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.ContractManagement.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.Business.Testing.RatingAdapterTestHelper;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBooking))]
	public class QuotedBookingRatingAdapterTest : NonPersistentBusinessObjectTestCase
	{
		#region Auto Rating

		public void TestAutoRating_CustomsInfo()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.CurrentOneOffQuote.TT_NumberOfEntries = 3;
			quote.CurrentOneOffQuote.TT_NumberOfEntryLines = 54;

			var qb = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			var rating = (IAutoRatingCustomsInfo)qb.GetFirstAdapter();
			AssertEquals(3, rating.Entries.Count);
			AssertEquals(54, rating.Invoices[0].InvoiceLines);

			var booking = QuotedBooking.CreateNewBooking(Factory);
			qb = CreateNewQuotedBooking(quote.PK, booking.PK);
			AssertEquals(3, rating.Entries.Count);
			AssertEquals(54, rating.Invoices[0].InvoiceLines);
		}

		public void TestAdapterTypeAndID_Booking()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_UniqueConsignRef = "666";

			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			var adapter = new QuotedBookingRatingAdapter(quotedBooking);

			AssertEquals(AdapterType.Booking, adapter.AdapterType);
			AssertEquals("666", adapter.OperationalJobCode);
			AssertEquals("666", adapter.JobID);
		}

		public void TestAdapterTypeAndID_Quote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.TH_QuoteNumber = "666";

			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			var adapter = new QuotedBookingRatingAdapter(quotedBooking);

			AssertEquals(AdapterType.OneOffQuote, adapter.AdapterType);
			AssertEquals("666", adapter.OperationalJobCode);
			AssertEquals("666", adapter.JobID);
		}

		public void TestAdapterTypeAndID_BookingWithQuote()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_UniqueConsignRef = "999";

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.TH_QuoteNumber = "666";

			var quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			var adapter = new QuotedBookingRatingAdapter(quotedBooking);

			AssertEquals(AdapterType.BookingWithQuote, adapter.AdapterType);
			AssertEquals("666", adapter.OperationalJobCode);
			AssertEquals("666", adapter.JobID);
		}

		public void TestAutoRatingTransportProviders()
		{
			var carrier = Factory.New<OrgHeader>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.CurrentOneOffQuote.TT_OH_Carrier = carrier.PK;
			var qb = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			var rating = qb.GetFirstAdapter();
			AssertCollectionContains(carrier, rating.Creditors.AllOrgs);
		}

		public void TestPossibleCarriers_OneOffQuote()
		{
			var carrier1 = Factory.New<OrgHeader>();
			var carrier2 = Factory.New<OrgHeader>();
			var carrier3 = Factory.New<OrgHeader>();

			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			var quote = quotedBooking.Quote;
			quotedBooking.OH_Carrier = carrier3.PK;

			AssertEquals(0, new QuotedBookingRatingAdapter(quotedBooking).PossibleCarriers?.Count() ?? 0);

			var oneOffQuote = quote.CurrentOneOffQuote;
			oneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;
			oneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier2.PK;
			AssertContainsExactElementsInAnyOrder("PossibleCarriers", new[] { carrier1, carrier2 },
				new QuotedBookingRatingAdapter(quotedBooking).PossibleCarriers);

			AssertContainsExactElementsInAnyOrder("PossibleServiceProviders", new[] { carrier1, carrier2, carrier3 },
				new QuotedBookingRatingAdapter(quotedBooking).PossibleServiceProviders);

			AssertContainsExactElementsInAnyOrder("Creditors", new[] { carrier1, carrier2, carrier3 },
				new QuotedBookingRatingAdapter(quotedBooking).Creditors.AllOrgs);

			quotedBooking.ConvertQuoteToQuotedBooking();
			AssertEquals("Convert deletes PossibleCarriers (since they are no longer visible)", 0, oneOffQuote.PossibleCarriers.Count);

			AssertContainsExactElementsInAnyOrder("Creditors", new[] { carrier3 },
				new QuotedBookingRatingAdapter(quotedBooking).Creditors.AllOrgs);
		}

		public void TestPossibleCarriers_BookingWithQuote()
		{
			var carrier1 = Factory.New<OrgHeader>();
			var carrier2 = Factory.New<OrgHeader>();

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var quote = quotedBooking.Quote;

			var oneOffQuote = quote.CurrentOneOffQuote;
			oneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;
			oneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier2.PK;

			AssertEquals("PossibleCarriers not set when has Booking", 0, new QuotedBookingRatingAdapter(quotedBooking).PossibleCarriers?.Count() ?? 0);

			var serviceProviders = new QuotedBookingRatingAdapter(quotedBooking).PossibleServiceProviders.ToList();
			AssertCollectionNotContains("PossibleCarriers should not be in PossibleServiceProviders", carrier1, serviceProviders);
			AssertCollectionNotContains("PossibleCarriers should not be in PossibleServiceProviders", carrier2, serviceProviders);

			var creditorOrgs = new QuotedBookingRatingAdapter(quotedBooking).Creditors.AllOrgs;
			AssertCollectionNotContains("PossibleCarriers should not be in Creditors", carrier1, creditorOrgs);
			AssertCollectionNotContains("PossibleCarriers should not be in Creditors", carrier2, creditorOrgs);
		}

		public void TestAutoRatingOrganizationsAddress()
		{
			var officeAddress = Factory.New<OrgAddress>();
			officeAddress.OA_Address1 = "111 TIM TAM STREET";
			officeAddress.OA_City = "SYDNEY";

			var pickupAddress = Factory.New<OrgAddress>();
			pickupAddress.OA_Address1 = "123 ARNOTT STREET";
			pickupAddress.OA_City = "MASCOT";

			var deliveryAddress = Factory.New<OrgAddress>();
			deliveryAddress.OA_Address1 = "344 SESME STREET";
			deliveryAddress.OA_City = "GREENSQUARE";

			var shipment = QuotedBooking.CreateNewBooking(Factory);
			shipment.ConsignorDocumentaryAddress.E2_Address1 = officeAddress.OA_Address1;
			shipment.ConsignorPickupAddress.E2_Address1 = pickupAddress.OA_Address1;

			var booking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			var rating = booking.GetFirstAdapter();

			AssertEquals("123 ARNOTT STREET", rating.PickupAddress.E2_Address1);

			var shipment1 = QuotedBooking.CreateNewBooking(Factory);
			shipment1.ConsigneeDocumentaryAddress.E2_Address1 = officeAddress.OA_Address1;
			shipment1.ConsigneeDeliveryAddress.E2_Address1 = deliveryAddress.OA_Address1;

			var booking1 = QuotedBooking.New(ZGuid.Empty, shipment1.PK, Factory);
			var rating1 = booking1.GetFirstAdapter();

			AssertEquals("344 SESME STREET", rating1.DeliveryAddress.E2_Address1);

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var spotQuote = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			var rating3 = spotQuote.GetFirstAdapter();
			spotQuote.Mode = "LSE";
			spotQuote.ConsigneeDocumentaryAddress.E2_Address1 = officeAddress.OA_Address1;

			AssertEquals("111 TIM TAM STREET", rating3.DeliveryAddress.E2_Address1);

			var quote1 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var spotQuote1 = CreateNewQuotedBooking(quote1.PK, ZGuid.Empty);
			var rating4 = spotQuote1.GetFirstAdapter();
			spotQuote1.Mode = "LSE";
			spotQuote1.ConsignorDocumentaryAddress.E2_Address1 = officeAddress.OA_Address1;

			AssertEquals("111 TIM TAM STREET", rating4.PickupAddress.E2_Address1);
		}

		public void TestAutoRatingChargeCodeGroups()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.CurrentOneOffQuote.TT_NumberOfEntries = 2;
			quote.CurrentOneOffQuote.TT_NumberOfEntryLines = 2;

			var qb = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			qb.Mode = "LSE";
			var rating = qb.GetFirstAdapter();

			AssertContainsChargeCodeGroups("Always", true, rating, Env.Registry.Rating.FreightRatedCodes);
			AssertContainsChargeCodeGroups("When QuoteNumberOfEntries > 0 || QuoteNumberOfEntryLines > 0, include all brokerage charges", true, rating, Env.Registry.Rating.BrokerageRatedCodes);
			AssertContainsChargeCodeGroups("When QuoteNumberOfEntries > 0 || QuoteNumberOfEntryLines > 0, include all brokerage charges", true, rating, Env.Registry.Rating.OriginBrokerageRatedCodes);

			quote.CurrentOneOffQuote.TT_NumberOfEntries = 0;
			quote.CurrentOneOffQuote.TT_NumberOfEntryLines = 0;
			AssertContainsChargeCodeGroups("Always", true, rating, Env.Registry.Rating.FreightRatedCodes);
			AssertContainsChargeCodeGroups("When QuoteNumberOfEntries = 0 || QuoteNumberOfEntryLines = 0, don't include", false, rating, Env.Registry.Rating.BrokerageRatedCodes);
			AssertContainsChargeCodeGroups("When QuoteNumberOfEntries = 0 || QuoteNumberOfEntryLines = 0, don't include", false, rating, Env.Registry.Rating.OriginBrokerageRatedCodes);

			quote.CurrentOneOffQuote.TT_NumberOfEntries = 0;
			quote.CurrentOneOffQuote.TT_NumberOfEntryLines = 100;
			AssertContainsChargeCodeGroups("Always", true, rating, Env.Registry.Rating.FreightRatedCodes);
			AssertContainsChargeCodeGroups("When QuoteNumberOfEntries = 0 || QuoteNumberOfEntryLines > 0, include all brokerage charges", true, rating, Env.Registry.Rating.BrokerageRatedCodes);
			AssertContainsChargeCodeGroups("When QuoteNumberOfEntries = 0 || QuoteNumberOfEntryLines > 0, include all brokerage charges", true, rating, Env.Registry.Rating.OriginBrokerageRatedCodes);

			quote.CurrentOneOffQuote.TT_NumberOfEntries = 100;
			quote.CurrentOneOffQuote.TT_NumberOfEntryLines = 0;
			AssertContainsChargeCodeGroups("Always", true, rating, Env.Registry.Rating.FreightRatedCodes);
			AssertContainsChargeCodeGroups("When QuoteNumberOfEntries > 0 || QuoteNumberOfEntryLines = 0, include all brokerage charges", true, rating, Env.Registry.Rating.BrokerageRatedCodes);
			AssertContainsChargeCodeGroups("When QuoteNumberOfEntries > 0 || QuoteNumberOfEntryLines = 0, include all brokerage charges", true, rating, Env.Registry.Rating.OriginBrokerageRatedCodes);

			quote.CurrentOneOffQuote.TT_NumberOfEntries = short.MaxValue;
			quote.CurrentOneOffQuote.TT_NumberOfEntryLines = short.MaxValue;
			AssertContainsChargeCodeGroups("Always", true, rating, Env.Registry.Rating.FreightRatedCodes);
			AssertContainsChargeCodeGroups("When QuoteNumberOfEntries = short.MaxValue || QuoteNumberOfEntryLines = short.MaxValue, include all brokerage charges", true, rating, Env.Registry.Rating.BrokerageRatedCodes);
			AssertContainsChargeCodeGroups("When QuoteNumberOfEntries = short.MaxValue || QuoteNumberOfEntryLines = short.MaxValue, include all brokerage charges", true, rating, Env.Registry.Rating.OriginBrokerageRatedCodes);

			AssertEquals(ChargeCodeFilter.AutorateAll, rating.ChargeCodeGroups.CostChargesFilter);
			AssertEquals(ChargeCodeFilter.AutorateAll, rating.ChargeCodeGroups.SellChargesFilter);
		}

		void AssertContainsChargeCodeGroups(string message, bool expected, IAutoRating rating, string[] chargeCodeGroups)
		{
			foreach (string group in chargeCodeGroups)
			{
				if (expected)
				{
					AssertCollectionContains(message, group, rating.ChargeCodeGroups);
				}
				else
				{
					AssertCollectionNotContains(message, group, rating.ChargeCodeGroups);
				}
			}
		}

		public void TestAutoRatingJobServices()
		{
			var booking1 = QuotedBooking.CreateNewBooking(Factory);
			var booking2 = QuotedBooking.CreateNewBooking(Factory);
			var quote1 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quote2 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);

			var quotedBookingRating = CreateNewQuotedBooking(quote1.PK, booking1.PK).GetFirstAdapter();
			var spotQuoteRating = CreateNewQuotedBooking(quote2.PK, ZGuid.Empty).GetFirstAdapter();
			var quickBookingRating = CreateNewQuotedBooking(ZGuid.Empty, booking2.PK).GetFirstAdapter();

			var code = Factory.New<AccChargeCode>();
			code.AC_Code = "CH1";
			code.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			code.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.Fumigation;

			var code2 = Factory.New<AccChargeCode>();
			code2.AC_Code = "CH2";
			code2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			code2.AC_ChargeSubGroup = ChargeCodeSubGroupList.UnpackingCharges;

			Assert(!quotedBookingRating.JobServices.IsEnabledOrServiceInactive(code));
			Assert(spotQuoteRating.JobServices.IsEnabledOrServiceInactive(code));
			Assert(!quickBookingRating.JobServices.IsEnabledOrServiceInactive(code));

			Assert(quotedBookingRating.JobServices.IsEnabledOrServiceInactive(code2));
			Assert(spotQuoteRating.JobServices.IsEnabledOrServiceInactive(code2));
			Assert(quickBookingRating.JobServices.IsEnabledOrServiceInactive(code2));
		}

		#region Autorating Via Port Registry

		public void TestAutoratingViaPortRegistry_InUse_ForQuotedBooking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "USLAX";
			quotedBooking.LoadPort = "AUMEL";
			quotedBooking.DischargePort = "USCHI";

			var configurations = new AutoratingViaPortConfigurationCollection();
			var configuration = configurations.AddNew();
			configuration.JobType = "QSH";
			configuration.TransportMode = "ALL";
			var setting = configuration.Settings.AddNew();
			setting.Direction = "ALL";
			setting.ViaSourceOption = "VL";

			var adapter = quotedBooking.GetFirstAdapter();

			using (RatingDataRegistry.Instance.AutoratingViaPort.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations))
			{
				AssertEquals("Load Port has to be returned", "AUMEL", adapter.GetVia(CostSell.Cost)?.Code);
				AssertEquals("Load Port has to be returned", "AUMEL", adapter.GetVia(CostSell.Revenue)?.Code);
			}

			setting.ViaSourceOption = "VD";

			using (RatingDataRegistry.Instance.AutoratingViaPort.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations))
			{
				AssertEquals("Discharge Port has to be returned", "USCHI", adapter.GetVia(CostSell.Cost)?.Code);
				AssertEquals("Discharge Port has to be returned", "USCHI", adapter.GetVia(CostSell.Revenue)?.Code);
			}
		}

		public void TestViaIsSpecified()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var spotQuote = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			spotQuote.Origin = "AUSYD";
			spotQuote.Destination = "SGSIN";

			var spotQuotedRating = spotQuote.GetFirstAdapter();
			AssertNull("No Via expected", spotQuotedRating.GetVia(CostSell.Cost));
			AssertNull("No Via expected", spotQuotedRating.GetVia(CostSell.Revenue));

			spotQuote.Via = "AUMEL";

			AssertEquals("Route Via specified port", "AUMEL", spotQuotedRating.GetVia(CostSell.Cost).Code);
			AssertEquals("Route Via specified port", "AUMEL", spotQuotedRating.GetVia(CostSell.Revenue).Code);
		}

		public void TestViaLoadPortIsSpecified()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "SGSIN";

			var quotedBookingRating = quotedBooking.GetFirstAdapter();
			AssertNull("No Via expected", quotedBookingRating.GetVia(CostSell.Cost));
			AssertNull("No Via expected", quotedBookingRating.GetVia(CostSell.Revenue));

			quotedBooking.LoadPort = "AUMEL";

			AssertEquals("Route Via Load Port", "AUMEL", quotedBookingRating.GetVia(CostSell.Cost).Code);
			AssertEquals("Route Via Load Port", "AUMEL", quotedBookingRating.GetVia(CostSell.Revenue).Code);
		}

		#endregion

		#region Matching Locations

		public void TestMatchingLocations()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "USLAX";
			quotedBooking.LoadPort = "AUMEL";
			quotedBooking.DischargePort = "USCHI";

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_JV = jobVoyage.PK;

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "AUCNS";
			destination.JB_JV = jobVoyage.PK;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			Factory.Save();

			((ISailingChooserParent)quotedBooking).SailingJX = sailing.PK;

			var adapterToTest = quotedBooking.GetFirstAdapter();

			AssertEquals("FirstLoad", "AUMEL", adapterToTest.GetFirstLoad(CostSell.Cost).Code);
			AssertEquals("FirstLoad", "AUMEL", adapterToTest.GetFirstLoad(CostSell.Revenue).Code);
			AssertEquals("LastDischarge", "USCHI", adapterToTest.GetLastDischarge(CostSell.Cost).Code);
			AssertEquals("LastDischarge", "USCHI", adapterToTest.GetLastDischarge(CostSell.Revenue).Code);
			AssertEquals("FirstRouteSetLoad", "AUBNE", adapterToTest.GetFirstRouteSetLoad(CostSell.Cost).Code);
			AssertEquals("FirstRouteSetLoad", "AUBNE", adapterToTest.GetFirstRouteSetLoad(CostSell.Revenue).Code);
			AssertEquals("LastRouteSetDischarge", "AUCNS", adapterToTest.GetLastRouteSetDischarge(CostSell.Cost).Code);
			AssertEquals("LastRouteSetDischarge", "AUCNS", adapterToTest.GetLastRouteSetDischarge(CostSell.Revenue).Code);
		}

		#endregion

		public void TestConsumerType()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);

			var quotedOnly = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			AssertEquals("Quote only", JobInvoicingConsumerTypes.OneOffQuotation, ((IJobInvoicingPlugIn)quotedOnly).InvoicingSupporter.ConsumerType);
			AssertEquals("Quote only", JobInvoicingConsumerTypes.OneOffQuotation, quotedOnly.GetFirstAdapter().ConsumerType);

			quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedAndBookingOnly = QuotedBooking.New(quote.PK, booking.PK, Factory);
			AssertEquals("Quote involved, not consolidated", JobInvoicingConsumerTypes.QuotedBooking, ((IJobInvoicingPlugIn)quotedAndBookingOnly).InvoicingSupporter.ConsumerType);
			AssertEquals("Quote involved, not consolidated", JobInvoicingConsumerTypes.QuotedBooking, quotedAndBookingOnly.GetFirstAdapter().ConsumerType);

			quotedAndBookingOnly.Booking.JS_IsForwardRegistered = true;
			AssertEquals("Quote involved, is consolidated", JobInvoicingConsumerTypes.Shipment, ((IJobInvoicingPlugIn)quotedAndBookingOnly).InvoicingSupporter.ConsumerType);
			AssertEquals("Quote involved, is consolidated", JobInvoicingConsumerTypes.Shipment, quotedAndBookingOnly.GetFirstAdapter().ConsumerType);

			quotedAndBookingOnly.Booking.JS_IsCFSRegistered = true;
			AssertEquals("Quote involved, is consolidated, should still be shipment", JobInvoicingConsumerTypes.Shipment, ((IJobInvoicingPlugIn)quotedAndBookingOnly).InvoicingSupporter.ConsumerType);
			AssertEquals("Quote involved, is consolidated, should still be shipment", JobInvoicingConsumerTypes.Shipment, quotedAndBookingOnly.GetFirstAdapter().ConsumerType);

			quotedAndBookingOnly.Quote.TH_Accepted = ZDateTime.Empty;
			AssertEquals("Quote involved, converted", JobInvoicingConsumerTypes.Shipment, ((IJobInvoicingPlugIn)quotedAndBookingOnly).InvoicingSupporter.ConsumerType);
			AssertEquals("Quote involved, converted", JobInvoicingConsumerTypes.Shipment, quotedAndBookingOnly.GetFirstAdapter().ConsumerType);

			booking = QuotedBooking.CreateNewBooking(Factory);
			var bookingOnly = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			bookingOnly.Booking.JS_IsCFSRegistered = true;
			AssertEquals("Booking Only, not consolidated", JobInvoicingConsumerTypes.QuotedBooking, ((IJobInvoicingPlugIn)bookingOnly).InvoicingSupporter.ConsumerType);
			AssertEquals("Booking Only, not consolidated", JobInvoicingConsumerTypes.QuotedBooking, bookingOnly.GetFirstAdapter().ConsumerType);

			bookingOnly.Booking.JS_IsCFSRegistered = false;
			AssertEquals("Booking Only, not consolidated", JobInvoicingConsumerTypes.QuotedBooking, ((IJobInvoicingPlugIn)bookingOnly).InvoicingSupporter.ConsumerType);
			AssertEquals("Booking Only, not consolidated", JobInvoicingConsumerTypes.QuotedBooking, bookingOnly.GetFirstAdapter().ConsumerType);

			bookingOnly.Booking.JS_IsForwardRegistered = true;
			AssertEquals("Booking Only, is consolidated (forwarding)", JobInvoicingConsumerTypes.Shipment, ((IJobInvoicingPlugIn)bookingOnly).InvoicingSupporter.ConsumerType);
			AssertEquals("Booking Only, is consolidated (forwarding)", JobInvoicingConsumerTypes.Shipment, bookingOnly.GetFirstAdapter().ConsumerType);

			bookingOnly.Booking.JS_IsCFSRegistered = true;
			AssertEquals("Booking Only, quoted bookings cannot be CFS only", JobInvoicingConsumerTypes.Shipment, ((IJobInvoicingPlugIn)bookingOnly).InvoicingSupporter.ConsumerType);
			AssertEquals("Booking Only, quoted bookings cannot be CFS only", JobInvoicingConsumerTypes.Shipment, bookingOnly.GetFirstAdapter().ConsumerType);
		}

		public void TestConsumerTypeVsGenericJob()
		{
			var bookingWithQuote = CreateQuotedBooking(QuoteBookingType.BookingWithQuote, false, false);
			AssertEquals("Booking with quote", JobInvoicingConsumerTypes.QuotedBooking, ((IJobInvoicingPlugIn)bookingWithQuote).InvoicingSupporter.ConsumerType);

			var cfsBookingWithQuote = CreateQuotedBooking(QuoteBookingType.BookingWithQuote, false, true);
			AssertEquals("CFS registered booking with quote", JobInvoicingConsumerTypes.QuotedBooking, ((IJobInvoicingPlugIn)cfsBookingWithQuote).InvoicingSupporter.ConsumerType);

			var convertedBookingWithQuote = CreateQuotedBooking(QuoteBookingType.BookingWithQuote, true, false);
			AssertEquals("Consolidated booking with quote", JobInvoicingConsumerTypes.Shipment, ((IJobInvoicingPlugIn)convertedBookingWithQuote).InvoicingSupporter.ConsumerType);

			var convertedCFSBookingWithQuote = CreateQuotedBooking(QuoteBookingType.BookingWithQuote, true, true);
			AssertEquals("Consolidated CFS registered booking with quote", JobInvoicingConsumerTypes.Shipment, ((IJobInvoicingPlugIn)convertedCFSBookingWithQuote).InvoicingSupporter.ConsumerType);

			var bookingOnly = CreateQuotedBooking(QuoteBookingType.QuickBooking, false, false);
			AssertEquals("Booking only", JobInvoicingConsumerTypes.QuotedBooking, ((IJobInvoicingPlugIn)bookingOnly).InvoicingSupporter.ConsumerType);

			var cfsBookingOnly = CreateQuotedBooking(QuoteBookingType.QuickBooking, false, true);
			AssertEquals("CFS registered booking only", JobInvoicingConsumerTypes.QuotedBooking, ((IJobInvoicingPlugIn)cfsBookingOnly).InvoicingSupporter.ConsumerType);

			var convertedBookingOnly = CreateQuotedBooking(QuoteBookingType.QuickBooking, true, false);
			AssertEquals("Consolidated booking only", JobInvoicingConsumerTypes.Shipment, ((IJobInvoicingPlugIn)convertedBookingOnly).InvoicingSupporter.ConsumerType);

			var convertedCFSBookingOnly = CreateQuotedBooking(QuoteBookingType.QuickBooking, true, true);
			AssertEquals("Consolidated CFS registered booking only", JobInvoicingConsumerTypes.Shipment, ((IJobInvoicingPlugIn)convertedCFSBookingOnly).InvoicingSupporter.ConsumerType);

			Factory.Save();

			var shipmentGenericJobs = Factory.Load<IGenericJob>(new ZQuery(ViewGenericJobSchema.VJ_JobType, JobInvoicingConsumerTypes.ShipmentCode));
			AssertEquals(6, shipmentGenericJobs.Length);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				bookingWithQuote.Booking.PK, cfsBookingWithQuote.Booking.PK, convertedBookingWithQuote.Booking.PK,
				convertedCFSBookingWithQuote.Booking.PK, convertedBookingOnly.Booking.PK, convertedCFSBookingOnly.Booking.PK
			}, shipmentGenericJobs.Select(job => job.Consumer.PK));

			var quotedBookingGenericJobs = Factory.Load<IGenericJob>(new ZQuery(ViewGenericJobSchema.VJ_JobType, JobInvoicingConsumerTypes.QuotedBookingCode));
			AssertEquals(2, quotedBookingGenericJobs.Length);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				bookingOnly.PK, cfsBookingOnly.PK
			}, quotedBookingGenericJobs.Select(job => job.Consumer.PK));
		}

		QuotedBooking CreateQuotedBooking(QuoteBookingType quotedBookingType, bool isConsolidated, bool isCFSRegistered)
		{
			var quotedBooking = QuotedBooking.New(quotedBookingType, Factory);
			if (quotedBooking.Booking != null)
			{
				if (isConsolidated)
				{
					quotedBooking.Booking.JS_IsForwardRegistered = true;
				}

				if (isCFSRegistered)
				{
					quotedBooking.Booking.JS_IsCFSRegistered = true;
				}
			}

			return quotedBooking;
		}

		public void TestMeasures_Booking_ContainerCountTEU()
		{
			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OneTimeQuote = true;
			var booking = Factory.NewWithValidTestData<ForwardingShipment>();
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var quotedBookingAdapter = new QuotedBookingRatingAdapter(quotedBooking);

			var measures = (RateableMeasureSet)quotedBookingAdapter.RateableMeasures;
			AssertEquals(AdapterType.Shipment, measures.AdapterType);
			AssertEquals(0, measures.GetAllContainers().Count());

			var quotedBookingContainer = quotedBooking.QuotedBookingContainers.AddNew();
			quotedBookingContainer.JC_ContainerCount = 1;
			quotedBookingContainer.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;

			measures = (RateableMeasureSet)quotedBookingAdapter.RateableMeasures;
			var containers = measures.GetAllContainers().ToList();
			AssertEquals(1, containers.Count);
			AssertEquals(2m, containers[0].TEU);
		}

		public void TestGetStatusInformationCoreHandlesInvalidWeightAndVolumeUnits()
		{
			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OneTimeQuote = true;
			var booking = Factory.NewWithValidTestData<ForwardingShipment>();
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var quotedBookingAdapter = new QuotedBookingRatingAdapter(quotedBooking);

			quotedBooking.VolumeUnit = "KG";
			AssertEquals("AutoRating cannot be executed with invalid volume unit", false, quotedBookingAdapter.StatusInformation.CanExecute);
			AssertEquals("Error message should be shown for invalid volume unit", "Invalid unit of volume: 'KG'.", quotedBookingAdapter.StatusInformation.Message.ToString());
			quotedBooking.VolumeUnit = "M3";
			AssertEquals("AutoRating can be executed with valid volume unit", true, quotedBookingAdapter.StatusInformation.CanExecute);

			quotedBooking.WeightUnit = "M3";
			AssertEquals("AutoRating cannot be executed with invalid weight unit", false, quotedBookingAdapter.StatusInformation.CanExecute);
			AssertEquals("Error message should be shown for invalid weight unit", "Invalid unit of weight: 'M3'.", quotedBookingAdapter.StatusInformation.Message.ToString());

			quotedBooking.WeightUnit = "KG";
			AssertEquals("AutoRating can be executed with valid weight unit", true, quotedBookingAdapter.StatusInformation.CanExecute);
		}

		public void TestDebtorOrgs_ShouldIncludeControllingCustomer()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();

			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.Booking.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;

			var adapter = new QuotedBookingRatingAdapter(booking);

			AssertEquals("Booking should provide controlling customer for the rating engine so that it applies rates for a specific controlling customer",
				controllingCustomer.PK,
				adapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS]?.PK);
		}

		public void TestGetContractNumberConfiguration()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			var ratingAdapter = new QuotedBookingRatingAdapter(quotedBooking);

			var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
			AssertEquals(true, configuration.ShouldAddContractNumberQueryFilter);
			AssertEquals(true, configuration.ShouldApplySpecificAdapterContractNumberFilter);
			AssertEquals(false, configuration.ShouldIgnoreJobCarrierContractNumbers);
			AssertEquals(false, configuration.ShouldIgnoreJobClientContractNumbers);
			AssertEquals(false, configuration.ShouldMatchJobBlankContractNumber);
			AssertEquals(false, configuration.ShouldUseCarrierContractDateFilter);

			configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
			AssertEquals(true, configuration.ShouldAddContractNumberQueryFilter);
			AssertEquals(false, configuration.ShouldApplySpecificAdapterContractNumberFilter);
			AssertEquals(false, configuration.ShouldIgnoreJobCarrierContractNumbers);
			AssertEquals(false, configuration.ShouldIgnoreJobClientContractNumbers);
			AssertEquals(false, configuration.ShouldMatchJobBlankContractNumber);
			AssertEquals(false, configuration.ShouldUseCarrierContractDateFilter);
		}

		public void TestShouldRemoveChargeWhenMissingServiceOrChargeableUnit()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			var ratingAdapter = new QuotedBookingRatingAdapter(quotedBooking);

			AssertEquals(true, ratingAdapter.ShouldRemoveChargeWhenMissingServiceOrChargeableUnit(null));

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeType = ChargeType.Margin;
			chargeCode.AC_MarginPercentage = 100m;
			chargeCode.AC_Code = "TEST";
			chargeCode.AC_Desc = "Test charge";
			chargeCode.AC_RateCalculator = "FLT";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;

			chargeCode.AC_ChargeSubGroup = ZString.Empty;
			AssertEquals(true, ratingAdapter.ShouldRemoveChargeWhenMissingServiceOrChargeableUnit(chargeCode));

			chargeCode.AC_ChargeSubGroup = "XIN";
			AssertEquals(false, ratingAdapter.ShouldRemoveChargeWhenMissingServiceOrChargeableUnit(chargeCode));
		}

		public void TestTariffLevelShouldWorkCorrectlyUnderDifferentCases()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedOnly = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			var bookingOnly = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			Factory.New<CompanyTariff>();

			var ratingAdapter = new QuotedBookingRatingAdapter(quotedOnly);
			var ratingAdapter2 = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter3 = new QuotedBookingRatingAdapter(bookingOnly);
			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				quotedOnly.CompanyTariffLevel = "1";
				quotedBooking.CompanyTariffLevel = "1";
				bookingOnly.CompanyTariffLevel = "1";

				AssertEquals("When AllowOverrideCompanyTariffLevel is true, TariffLevel of OOQ should be working", 1, ((IAutoRatingCompanyTariffLevelProvider)ratingAdapter).TariffLevel);
				AssertEquals("When AllowOverrideCompanyTariffLevel is true, TariffLevel of BWQ should be working", 1, ((IAutoRatingCompanyTariffLevelProvider)ratingAdapter2).TariffLevel);
				AssertEquals("When AllowOverrideCompanyTariffLevel is true, TariffLevel of QB should be working", 1, ((IAutoRatingCompanyTariffLevelProvider)ratingAdapter3).TariffLevel);
				quotedOnly.ClientDocAddress.E2_AddressOverride = true;

				AssertEquals("When AllowOverrideCompanyTariffLevel is true but client is overriden, TariffLevel of OOQ should be still working", 1, ((IAutoRatingCompanyTariffLevelProvider)ratingAdapter).TariffLevel);
			}

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				quotedOnly.CompanyTariffLevel = "1";
				quotedBooking.CompanyTariffLevel = "1";
				bookingOnly.CompanyTariffLevel = "1";
				quotedOnly.ClientDocAddress.E2_AddressOverride = false;

				AssertEquals("When AllowOverrideCompanyTariffLevel is false, TariffLevel of OOQ should be invalid", 0, ((IAutoRatingCompanyTariffLevelProvider)ratingAdapter).TariffLevel);
				AssertEquals("When AllowOverrideCompanyTariffLevel is false, TariffLevel of BWQ should be invalid", 0, ((IAutoRatingCompanyTariffLevelProvider)ratingAdapter2).TariffLevel);
				AssertEquals("When AllowOverrideCompanyTariffLevel is false, TariffLevel of QB should be invalid", 0, ((IAutoRatingCompanyTariffLevelProvider)ratingAdapter3).TariffLevel);
				quotedOnly.ClientDocAddress.E2_AddressOverride = true;

				AssertEquals("When AllowOverrideCompanyTariffLevel is false but client is overriden, TariffLevel of OOQ should be still working", 1, ((IAutoRatingCompanyTariffLevelProvider)ratingAdapter).TariffLevel);
			}
		}

		public void TestBookingWithQuote_GivenHBLDeliveryMode_ThenRatingAdapterHBLDeliveryModeShouldBeUpdated()
		{
			var bookingWithQuote1 = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			bookingWithQuote1.Booking.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CFS_CY;
			var ratingAdapter1 = new QuotedBookingRatingAdapter(bookingWithQuote1);
			AssertEquals("HBLDeliveryMode: CFS/CY", Constants.HBLDeliveryModes.Codes.CFS_CY, ratingAdapter1.HBLDeliveryMode);

			var bookingWithQuote2 = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var ratingAdapter2 = new QuotedBookingRatingAdapter(bookingWithQuote2);
			AssertEquals("HBLDeliveryMode: empty", string.Empty, ratingAdapter2.HBLDeliveryMode);
		}

		public void TestQuickBooking_GivenHBLDeliveryMode_ThenRatingAdapterHBLDeliveryModeShouldBeUpdated()
		{
			var quickBooking1 = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quickBooking1.Booking.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.DOOR_ARPT;
			var ratingAdapter1 = new QuotedBookingRatingAdapter(quickBooking1);
			AssertEquals("HBLDeliveryMode: DOOR/ARPT", Constants.HBLDeliveryModes.Codes.DOOR_ARPT, ratingAdapter1.HBLDeliveryMode);

			var quickBooking2 = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var ratingAdapter2 = new QuotedBookingRatingAdapter(quickBooking2);
			AssertEquals("HBLDeliveryMode: empty", string.Empty, ratingAdapter2.HBLDeliveryMode);
		}

		public void TestQuickBooking_SkipFreightCharge()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contractAllocationLine = Factory.New<RatingContractAllocationLine>();

			quotedBooking.Booking.JS_RCA_BookingAllocationLine = contractAllocationLine.PK;

			var ratingAdapter = new QuotedBookingRatingAdapter(quotedBooking);

			AssertEquals(false, ratingAdapter.SkipFreightCharge);

			contractAllocationLine.RCA_AllowFreightSpotRate = true;

			AssertEquals(true, ratingAdapter.SkipFreightCharge);
		}

		public void TestBookingWithQuote_SkipFreightCharge()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var contractAllocationLine = Factory.New<RatingContractAllocationLine>();

			quotedBooking.Booking.JS_RCA_BookingAllocationLine = contractAllocationLine.PK;

			var ratingAdapter = new QuotedBookingRatingAdapter(quotedBooking);

			AssertEquals(false, ratingAdapter.SkipFreightCharge);

			contractAllocationLine.RCA_AllowFreightSpotRate = true;

			AssertEquals(true, ratingAdapter.SkipFreightCharge);
		}

		#region IJobDataUpdater

		public void TestUpdateServiceLevel_BookingWithQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.CarrierServiceLevel = "DIR";

			AssertEquals("DIR", quotedBooking.Booking.JS_PL_NKCarrierServiceLevel);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			AssertEquals("DIR", adapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));

			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("DIR", ratingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));

			ratingAdapter.UpdateServiceLevel("EXP");
			AssertEquals("EXP", quotedBooking.Booking.JS_PL_NKCarrierServiceLevel);
		}

		public void TestUpdateServiceLevel_QuickBooking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.CarrierServiceLevel = "DIR";

			AssertEquals("DIR", quotedBooking.Booking.JS_PL_NKCarrierServiceLevel);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			AssertEquals("DIR", adapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));

			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("DIR", ratingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));

			ratingAdapter.UpdateServiceLevel("EXP");
			AssertEquals("EXP", quotedBooking.Booking.JS_PL_NKCarrierServiceLevel);
		}

		public void TestUpdateServiceLevel_OneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.CarrierServiceLevel = "DIR";

			AssertEquals("DIR", quotedBooking.Quote.CurrentOneOffQuote.TT_PL_NKCarrierServiceLevel);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			AssertEquals("DIR", adapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));

			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("DIR", ratingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));

			ratingAdapter.UpdateServiceLevel("EXP");
			AssertEquals("EXP", quotedBooking.Quote.CurrentOneOffQuote.TT_PL_NKCarrierServiceLevel);
		}

		public void TestUpdateCommodityAndFMCTariffID_OneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.Commodity = "AAAA";
			quotedBooking.FMCTariffID = "1234";

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);
			ratingAdapter.UpdateRateCommodityCodeAndFMCTariffID("BBBB", "5678");
			AssertEquals("BBBB", quotedBooking.Quote.CurrentOneOffQuote.TT_RH_NKCommodity);
			AssertEquals("5678", quotedBooking.Quote.CurrentOneOffQuote.TT_FMCTariffID);
		}

		public void TestUpdateCommodityAndFMCTariffID_BookingWithQuote()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.Commodity = "AAAA";
			quotedBooking.FMCTariffID = "1234";

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);
			ratingAdapter.UpdateRateCommodityCodeAndFMCTariffID("BBBB", "5678");
			AssertEquals("BBBB", quotedBooking.Booking.JS_RH_NKRateCommodity);
			AssertEquals("5678", quotedBooking.Booking.JS_FMCTariffID);
		}

		public void TestUpdateDetailedGoodsDescription_BookingWithQuote()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);
			ratingAdapter.UpdateDetailedGoodsDescription("BBBB", false);
			AssertEquals("BBBB", quotedBooking.Booking.DetailedGoodsDescriptionNoteText);
		}

		public void TestUpdateCarrier_BookingWithQuote()
		{
			var oldCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var newCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.OH_Carrier = oldCarrier.PK;

			AssertEquals(oldCarrier.MainAddress.PK, quotedBooking.Booking.JS_OA_BookedShippingLineAddress);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			AssertEquals(oldCarrier.OH_Code, adapter.Carrier.OH_Code);

			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals(oldCarrier.OH_Code, ratingAdapter.Carrier.OH_Code);

			ratingAdapter.UpdateCarrier(newCarrier);
			AssertEquals(newCarrier.OH_Code, ratingAdapter.Carrier.OH_Code);
			AssertEquals(newCarrier.MainAddress.PK, quotedBooking.Booking.JS_OA_BookedShippingLineAddress);
		}

		public void TestUpdateCarrier_OneOffQuote()
		{
			var oldCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var newCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.OH_Carrier = oldCarrier.PK;

			AssertEquals(oldCarrier.PK, quotedBooking.Quote.CurrentOneOffQuote.TT_OH_Carrier);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			AssertEquals(oldCarrier.OH_Code, adapter.Carrier.OH_Code);

			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals(oldCarrier.OH_Code, ratingAdapter.Carrier.OH_Code);

			ratingAdapter.UpdateCarrier(newCarrier);
			AssertEquals(newCarrier.OH_Code, ratingAdapter.Carrier.OH_Code);
			AssertEquals(newCarrier.PK, quotedBooking.Quote.CurrentOneOffQuote.TT_OH_Carrier);
		}

		public void TestUpdateCarrier_QuickBooking()
		{
			var oldCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var newCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quickBooking.OH_Carrier = oldCarrier.PK;

			AssertEquals(oldCarrier.MainAddress.PK, quickBooking.Booking.JS_OA_BookedShippingLineAddress);

			var adapter = new QuotedBookingRatingAdapter(quickBooking);
			AssertEquals(oldCarrier.OH_Code, adapter.Carrier.OH_Code);

			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals(oldCarrier.OH_Code, ratingAdapter.Carrier.OH_Code);

			ratingAdapter.UpdateCarrier(newCarrier);
			AssertEquals(newCarrier.OH_Code, ratingAdapter.Carrier.OH_Code);
			AssertEquals(newCarrier.MainAddress.PK, quickBooking.Booking.JS_OA_BookedShippingLineAddress);
		}

		public void TestUpdateLocation_BookingWithQuote_ShouldNotChangeLocation()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Origin = "UAIEV";
			quotedBooking.Destination = "AUSYD";

			AssertEquals("UAIEV", quotedBooking.Booking.JS_RL_NKOrigin);
			AssertEquals("AUSYD", quotedBooking.Booking.JS_RL_NKDestination);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			AssertEquals("UAIEV", adapter.Origin.Code);
			AssertEquals("AUSYD", adapter.Destination.Code);

			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("UAIEV", ratingAdapter.Origin.Code);
			AssertEquals("AUSYD", ratingAdapter.Destination.Code);

			ratingAdapter.UpdateOrigin("SGSIN");
			ratingAdapter.UpdateDestination("USLAX");

			AssertEquals("UAIEV", ratingAdapter.Origin.Code);
			AssertEquals("AUSYD", ratingAdapter.Destination.Code);

			AssertEquals("UAIEV", quotedBooking.Booking.JS_RL_NKOrigin);
			AssertEquals("AUSYD", quotedBooking.Booking.JS_RL_NKDestination);
		}

		public void TestUpdateLocation_OneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.Origin = "UAIEV";
			quotedBooking.Destination = "AUSYD";

			AssertEquals("UAIEV", quotedBooking.Quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation);
			AssertEquals("AUSYD", quotedBooking.Quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			AssertEquals("UAIEV", adapter.Origin.Code);
			AssertEquals("AUSYD", adapter.Destination.Code);

			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("UAIEV", ratingAdapter.Origin.Code);
			AssertEquals("AUSYD", ratingAdapter.Destination.Code);

			ratingAdapter.UpdateOrigin("SGSIN");
			ratingAdapter.UpdateDestination("USLAX");

			AssertEquals("SGSIN", ratingAdapter.Origin.Code);
			AssertEquals("USLAX", ratingAdapter.Destination.Code);

			AssertEquals("SGSIN", quotedBooking.Quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation);
			AssertEquals("USLAX", quotedBooking.Quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation);
		}

		public void TestUpdateLocation_QuickBooking_ShouldNotChangeLocation()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quickBooking.Origin = "UAIEV";
			quickBooking.Destination = "AUSYD";

			AssertEquals("UAIEV", quickBooking.Booking.JS_RL_NKOrigin);
			AssertEquals("AUSYD", quickBooking.Booking.JS_RL_NKDestination);

			var adapter = new QuotedBookingRatingAdapter(quickBooking);
			AssertEquals("UAIEV", adapter.Origin.Code);
			AssertEquals("AUSYD", adapter.Destination.Code);

			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("UAIEV", ratingAdapter.Origin.Code);
			AssertEquals("AUSYD", ratingAdapter.Destination.Code);

			ratingAdapter.UpdateOrigin("SGSIN");
			ratingAdapter.UpdateDestination("USLAX");

			AssertEquals("UAIEV", ratingAdapter.Origin.Code);
			AssertEquals("AUSYD", ratingAdapter.Destination.Code);

			AssertEquals("UAIEV", quickBooking.Booking.JS_RL_NKOrigin);
			AssertEquals("AUSYD", quickBooking.Booking.JS_RL_NKDestination);
		}

		public void TestRatesSelectorOriginDestination_BookingWithQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "USLAX";
			quotedBooking.LoadPort = "AUMEL";
			quotedBooking.DischargePort = "USWBC";

			AssertEquals("AUMEL", quotedBooking.Booking.JS_RL_NKLoadPort);
			AssertEquals("USWBC", quotedBooking.Booking.JS_RL_NKDischargePort);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("AUMEL", ratingAdapter.DefaultFilterValueForOrigin.Code);
			AssertEquals("USWBC", ratingAdapter.DefaultFilterValueForDestination.Code);
		}

		public void TestRatesSelectorOriginDestination_OneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "USLAX";
			quotedBooking.LoadPort = "AUMEL";
			quotedBooking.DischargePort = "USWBC";

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("AUSYD", ratingAdapter.DefaultFilterValueForOrigin.Code);
			AssertEquals("USLAX", ratingAdapter.DefaultFilterValueForDestination.Code);
		}

		public void TestRatesSelectorOriginDestination_QuickBooking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "USLAX";
			quotedBooking.LoadPort = "AUMEL";
			quotedBooking.DischargePort = "USWBC";

			AssertEquals("AUMEL", quotedBooking.Booking.JS_RL_NKLoadPort);
			AssertEquals("USWBC", quotedBooking.Booking.JS_RL_NKDischargePort);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("AUMEL", ratingAdapter.DefaultFilterValueForOrigin.Code);
			AssertEquals("USWBC", ratingAdapter.DefaultFilterValueForDestination.Code);
		}

		public void TestUpdateNamedAccount_BookingWithQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Booking.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "NAC_OLD");

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			AssertEquals("NAC_OLD", adapter.NamedAccount);

			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("NAC_OLD", ratingAdapter.NamedAccount);

			ratingAdapter.UpdateNamedAccount("NAC_NEW");

			AssertEquals("NAC_NEW", ratingAdapter.NamedAccount);
			AssertEquals("NAC_NEW", quotedBooking.Booking.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount).CE_EntryNum);
		}

		public void TestUpdateNamedAccount_OneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.Quote.CurrentOneOffQuote.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "NAC_OLD");

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			AssertEquals("NAC_OLD", adapter.NamedAccount);

			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("NAC_OLD", ratingAdapter.NamedAccount);

			ratingAdapter.UpdateNamedAccount("NAC_NEW");

			AssertEquals("NAC_NEW", ratingAdapter.NamedAccount);
			AssertEquals("NAC_NEW", quotedBooking.Quote.CurrentOneOffQuote.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount).CE_EntryNum);
		}

		public void TestUpdateNamedAccount_QuickBooking()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quickBooking.Booking.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "NAC_OLD");

			var adapter = new QuotedBookingRatingAdapter(quickBooking);
			AssertEquals("NAC_OLD", adapter.NamedAccount);

			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("NAC_OLD", ratingAdapter.NamedAccount);

			ratingAdapter.UpdateNamedAccount("NAC_NEW");

			AssertEquals("NAC_NEW", ratingAdapter.NamedAccount);
			AssertEquals("NAC_NEW", quickBooking.Booking.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount).CE_EntryNum);
		}

		public void TestUpdateCarrierQuoteNumber_BookingWithQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);

			ratingAdapter.UpdateCarrierQuoteNumber("PI0001");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0002");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0003");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0002");

			var values = adapter.Parent.Booking.Numbers.GetAllReferenceNumbersByType("CQN");
			AssertEquals(3, values.Length);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "PI0001", "PI0002", "PI0003" }, values);
		}

		public void TestUpdateCarrierQuoteNumber_OneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);

			ratingAdapter.UpdateCarrierQuoteNumber("PI0001");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0002");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0003");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0002");

			var values = adapter.Parent.Quote.CurrentOneOffQuote.Numbers.GetAllReferenceNumbersByType("CQN");
			AssertEquals(3, values.Length);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "PI0001", "PI0002", "PI0003" }, values);
		}

		public void TestUpdateCarrierQuoteNumber_QuickBooking()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var adapter = new QuotedBookingRatingAdapter(quickBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);

			ratingAdapter.UpdateCarrierQuoteNumber("PI0001");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0002");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0003");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0002");

			var values = adapter.Parent.Booking.Numbers.GetAllReferenceNumbersByType("CQN");
			AssertEquals(3, values.Length);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "PI0001", "PI0002", "PI0003" }, values);
		}

		#region CarrierContractNumber

		void AllowMultipleCONRefs()
		{
			// By default, CON is unique but the registry below can change that.
			var overridenCollection = new CustomsReferenceNumberTypeCollection();
			var customsReferenceNumberType1 = overridenCollection.Add("CON", (NoResString)"Carrier Contract Number");
			customsReferenceNumberType1.IsUnique = false;

			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, overridenCollection);
		}

		public void TestGetCarrierContractNumbers_ForOneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);

			AllowMultipleCONRefs();
			quote.CurrentOneOffQuote.Numbers.AddOrSkipContractNumber("", isEmptyAllowed: true);
			quote.CurrentOneOffQuote.Numbers.AddOrSkipContractNumber("123");
			quote.CurrentOneOffQuote.Numbers.AddOrSkipContractNumber("456");

			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var adapter = new QuotedBookingRatingAdapter(quotedBooking);

			AssertEquals("Should return all CON ref numbers from OOQ", 3, adapter.CarrierContractNumbers.Count());
			AssertEquals("Should return all CON ref numbers from OOQ", "", adapter.CarrierContractNumbers.ElementAt(0));
			AssertEquals("Should return all CON ref numbers from OOQ", "123", adapter.CarrierContractNumbers.ElementAt(1));
			AssertEquals("Should return all CON ref numbers from OOQ", "456", adapter.CarrierContractNumbers.ElementAt(2));

			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("Should return all CON ref numbers from OOQ", 3, ratingAdapter.CarrierContractNumbers.Count());
			AssertEquals("Should return all CON ref numbers from OOQ", "", ratingAdapter.CarrierContractNumbers.ElementAt(0));
			AssertEquals("Should return all CON ref numbers from OOQ", "123", ratingAdapter.CarrierContractNumbers.ElementAt(1));
			AssertEquals("Should return all CON ref numbers from OOQ", "456", ratingAdapter.CarrierContractNumbers.ElementAt(2));
		}

		/// <summary>
		///   As CarrierContractNumber is accessed via Booking. It doesn't matter booking type is BwQ or QB.
		/// </summary>
		void TestGetCarrierContractNumbers_ForBookingWithQuoteOrForQuickBooking(QuoteBookingType bookingType)
		{
			var quotedBooking = QuotedBooking.New(bookingType, Factory);
			quotedBooking.CarrierContractNumber = "CON123";

			// quotedBooking.Booking.CusEntryNumbers should be synced with quotedBooking.CarrierContractNumber,
			// and ensure only one CON ref number allowed but the synchronization should be tested separately, not here.

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			AssertEquals("Should return the single contract number from the quoted booking, plus a blank number", 2, adapter.CarrierContractNumbers.Count());
			AssertEquals("Should return the single contract number from the quoted booking, plus a blank number", "CON123", adapter.CarrierContractNumbers.ElementAt(0));
			AssertEquals("Should return the single contract number from the quoted booking, plus a blank number", ZString.Empty, adapter.CarrierContractNumbers.ElementAt(1));

			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("Should return the single contract number from the quoted booking, plus a blank number", 2, ratingAdapter.CarrierContractNumbers.Count());
			AssertEquals("Should return the single contract number from the quoted booking, plus a blank number", "CON123", ratingAdapter.CarrierContractNumbers.ElementAt(0));
			AssertEquals("Should return the single contract number from the quoted booking, plus a blank number", ZString.Empty, ratingAdapter.CarrierContractNumbers.ElementAt(1));
		}

		public void TestGetCarrierContractNumbers_ForBookingWithQuote() =>
			TestGetCarrierContractNumbers_ForBookingWithQuoteOrForQuickBooking(QuoteBookingType.BookingWithQuote);

		public void TestGetCarrierContractNumbers_ForQuickBooking() =>
			TestGetCarrierContractNumbers_ForBookingWithQuoteOrForQuickBooking(QuoteBookingType.QuickBooking);

		void AssertCanUpdateNumber(CanUpdateCarrierContractNumberResult result, Mock<IDialogService> mockDialog, Times times, string expectedNumber, string because)
		{
			mockDialog.Verify(x => x.SelectSingleCarrierContractNumber(It.IsAny<IEnumerable<string>>()), times);
			AssertEquals(because, expectedNumber, result.Token.SelectionResult.Number);
		}

		public void TestCanUpdateCarrierContractNumber_OneOffQuote_ShouldReturnDefault()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var adapter = new QuotedBookingRatingAdapter(quotedBooking);

			var dialogServiceMock = new Mock<IDialogService>();
			var result = adapter.CanUpdateCarrierContractNumber(new[] { "123" }, dialogServiceMock.Object, isManualCostSelected: true);
			AssertEquals(true, result.CanUpdate);
			AssertEquals(ContractNumberSelectionResult.Default, result.Token.SelectionResult.Result);
			AssertNull(result.Token.SelectionResult.Number);
		}

		public void TestCanUpdateCarrierContractNumber_WhenJobNumberIsBlank()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quickBooking.Booking.JS_CarrierContractNumber = "";

			var dialogServiceMock = new Mock<IDialogService>();
			dialogServiceMock
				.Setup(x => x.SelectSingleCarrierContractNumber(It.IsAny<IEnumerable<string>>()))
				.Returns(new SingleCarrierContractNumberSelectionResult { Number = "123" });

			var adapter = new QuotedBookingRatingAdapter(quickBooking);

			var result = adapter.CanUpdateCarrierContractNumber(new[] { "" }, dialogServiceMock.Object, isManualCostSelected: true);
			AssertCanUpdateNumber(result, dialogServiceMock, Times.Never(), "", "Same number should be returned without asking");

			result = adapter.CanUpdateCarrierContractNumber(new[] { "789" }, dialogServiceMock.Object, isManualCostSelected: true);
			AssertCanUpdateNumber(result, dialogServiceMock, Times.Never(), "789", "A new single number should be returned without asking");

			result = adapter.CanUpdateCarrierContractNumber(new[] { "123", "" }, dialogServiceMock.Object, isManualCostSelected: true);
			AssertCanUpdateNumber(result, dialogServiceMock, Times.Once(), "123", "A selected number should be returned");
		}

		public void TestCanUpdateCarrierContractNumber_WhenJobNumberIsNotBlank()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quickBooking.Booking.JS_CarrierContractNumber = "CCA";

			var dialogServiceMock = new Mock<IDialogService>();
			dialogServiceMock
				.Setup(x => x.SelectSingleCarrierContractNumber(It.IsAny<IEnumerable<string>>()))
				.Returns(new SingleCarrierContractNumberSelectionResult { Number = "123" });

			var adapter = new QuotedBookingRatingAdapter(quickBooking);

			var result = adapter.CanUpdateCarrierContractNumber(new[] { "CCA" }, dialogServiceMock.Object, isManualCostSelected: true);
			AssertCanUpdateNumber(result, dialogServiceMock, Times.Never(), "CCA", "Same number should be returned without asking");

			result = adapter.CanUpdateCarrierContractNumber(new[] { "", "CCA"  }, dialogServiceMock.Object, isManualCostSelected: true);
			AssertCanUpdateNumber(result, dialogServiceMock, Times.Never(), "CCA", "Matched number should be returned without asking");

			result = adapter.CanUpdateCarrierContractNumber(new[] { "123" }, dialogServiceMock.Object, isManualCostSelected: true);
			AssertCanUpdateNumber(result, dialogServiceMock, Times.Once(), "123", "A selected number should be returned");

			result = adapter.CanUpdateCarrierContractNumber(new[] { "123", "456", "CCA", "" }, dialogServiceMock.Object, isManualCostSelected: true);
			AssertCanUpdateNumber(result, dialogServiceMock, Times.Exactly(2), "123", "A selected number should be returned"); // one more call from Once
		}

		public void TestUpdateContractNumber_ForOneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);

			AllowMultipleCONRefs();
			quote.CurrentOneOffQuote.Numbers.AddOrSkipContractNumber("");
			quote.CurrentOneOffQuote.Numbers.AddOrSkipContractNumber("123");
			quote.CurrentOneOffQuote.Numbers.AddOrSkipContractNumber("456");

			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var jobDataUpdater = (IJobDataUpdater)new AutoRatingProxy(new QuotedBookingRatingAdapter(quotedBooking));

			jobDataUpdater.UpdateCarrierContractNumber(new UpdateCarrierContractNumberToken(new[] { "NEWCON" }));

			AssertEquals(
				"Should clear old numbers and replace with the new ones",
				"NEWCON",
				quotedBooking.Quote.CurrentOneOffQuote.Numbers.Select(n => n.CE_EntryNum).Single());
		}

		void TestUpdateCarrierContractNumber_ForBookingWithQuoteOrQuickBooking(QuoteBookingType bookingType)
		{
			var quotedBooking = QuotedBooking.New(bookingType, Factory);
			quotedBooking.CarrierContractNumber = "123";
			var jobDataUpdater = (IJobDataUpdater)new AutoRatingProxy(new QuotedBookingRatingAdapter(quotedBooking));

			jobDataUpdater.UpdateCarrierContractNumber(new UpdateCarrierContractNumberToken(new[] { "456" }));

			AssertEquals("456", quotedBooking.CarrierContractNumber);
		}

		public void TestUpdateCarrierContractNumber_ForBookingWithQuote() =>
			TestUpdateCarrierContractNumber_ForBookingWithQuoteOrQuickBooking(QuoteBookingType.BookingWithQuote);

		public void TestUpdateCarrierContractNumber_QuickBooking() =>
			TestUpdateCarrierContractNumber_ForBookingWithQuoteOrQuickBooking(QuoteBookingType.QuickBooking);

		public void TestUpdateCarrierContractNumber_WhenNumberIsLong_ShouldBeTruncatedToFitFields()
		{
			const string longNumber = "A_VERY_LONG_NUMBER_THAT_ITS_LENGTH_EXCEEDS_THE_MAX_LENGTH";

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var jobDataUpdater = (IJobDataUpdater)new AutoRatingProxy(new QuotedBookingRatingAdapter(quotedBooking));

			jobDataUpdater.UpdateCarrierContractNumber(new UpdateCarrierContractNumberToken(new[] { longNumber }));

			AssertEquals("The CCA number should be a truncated version of the full number", "A_VERY_LONG_NUMBER_THAT_ITS_LENGTH_EXCEEDS_THE_MAX", quotedBooking.CarrierContractNumber);
			AssertEquals("The CON REF should be a truncated version of the full number", "A_VERY_LONG_NUMBER_THAT_ITS_LENGTH_", quotedBooking.Booking.Numbers[0].CE_EntryNum);
		}

		#endregion

		public void TestUpdateCarrierConfirmationIsNeeded_OneOffQuote() => AssertUpdateCarrierConfirmationIsNeeded(true, null);

		public void TestUpdateCarrierConfirmationIsNeeded_BookingWithQuote() => AssertUpdateCarrierConfirmationIsNeeded(false, QuoteBookingType.BookingWithQuote);

		public void TestUpdateCarrierConfirmationIsNeeded_QuickBooking() => AssertUpdateCarrierConfirmationIsNeeded(false, QuoteBookingType.QuickBooking);

		void AssertUpdateCarrierConfirmationIsNeeded(bool isTestingOneOfQuote, QuoteBookingType? bookingType)
		{
			QuotedBooking testObject;
			if (isTestingOneOfQuote)
			{
				var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
				testObject = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			}
			else
			{
				testObject = QuotedBooking.New(bookingType.Value, Factory);
			}

			var adapter = new QuotedBookingRatingAdapter(testObject);
			AssertEquals("Precondition: Job's carrier should be empty", ZGuid.Empty, testObject.OH_Carrier);
			Assert("Updating carrier on Booking with Quote should be required", adapter.UpdateCarrierConfirmationIsNeeded("Carrier", out var confirmationMessage));
			AssertNullOrEmpty("Confirmation message should not have a value because job's carrier is empty", confirmationMessage);

			var carrier = Factory.New<OrgHeader>();
			testObject.OH_Carrier = carrier.PK;
			Assert("Updating carrier on Booking with Quote should be required", adapter.UpdateCarrierConfirmationIsNeeded("Carrier", out confirmationMessage));
			AssertNotNullOrEmpty("Confirmation message should have a value because job's carrier presents", confirmationMessage);
		}

		public void TestUpdateOriginConfirmationIsNeeded_OneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			Assert("Confirmation is needed to update origin on One-Off Quote", adapter.UpdateOriginConfirmationIsNeeded("Origin", out var confirmationMessage));
			AssertContains("Should show One Off Quote > Origin", "During this operation, would you like to update the Origin of the One Off Quote to be the Origin 'Origin' of the chosen rates?", confirmationMessage);
		}

		public void TestUpdateOriginConfirmationIsNeeded_BookingWithQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			Assert("Confirmation is needed to update origin on Booking with Quote", adapter.UpdateOriginConfirmationIsNeeded("Origin", out var confirmationMessage));
			AssertContains("Should show Booking with Quote > Load", "During this operation, would you like to update the Load of the Booking with Quote to be the Origin 'Origin' of the chosen rates?", confirmationMessage);
		}

		public void TestUpdateOriginConfirmationIsNeeded_QuickBooking()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			var adapter = new QuotedBookingRatingAdapter(quickBooking);
			Assert("Confirmation is needed to update origin on Quick Booking", adapter.UpdateOriginConfirmationIsNeeded("Origin", out var confirmationMessage));
			AssertContains("Should show Quick Booking > Load", "During this operation, would you like to update the Load of the Quick Booking to be the Origin 'Origin' of the chosen rates?", confirmationMessage);
		}

		public void TestUpdateDestinationConfirmationIsNeeded_OneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			Assert("Confirmation is needed to update destination on One-Off Quote", adapter.UpdateDestinationConfirmationIsNeeded("Destination", out var confirmationMessage));
			AssertContains("Should show One Off Quote > Destination", "During this operation, would you like to update the Destination of the One Off Quote to be the Destination 'Destination' of the chosen rates?", confirmationMessage);
		}

		public void TestUpdateDestinationConfirmationIsNeeded_BookingWithQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			Assert("Confirmation is needed to update destination on Booking with Quote", adapter.UpdateDestinationConfirmationIsNeeded("Destination", out var confirmationMessage));
			AssertContains("Should show Booking with Quote > Discharge", "During this operation, would you like to update the Discharge of the Booking with Quote to be the Destination 'Destination' of the chosen rates?", confirmationMessage);
		}

		public void TestUpdateDestinationConfirmationIsNeeded_QuickBooking()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			var adapter = new QuotedBookingRatingAdapter(quickBooking);
			Assert("Confirmation is needed to update destination on Quick Booking", adapter.UpdateDestinationConfirmationIsNeeded("Destination", out var confirmationMessage));
			AssertContains("Should show Quick Booking > Discharge", "During this operation, would you like to update the Discharge of the Quick Booking to be the Destination 'Destination' of the chosen rates?", confirmationMessage);
		}

		public void TestMissingOrigin_OneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("Should show origin is missing", "Origin is mandatory for running Autorating Costs", ratingAdapter.OriginMissingMessage);
		}

		public void TestMissingOrigin_BookingWithQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("Should show Load Port is missing", "Load Port is mandatory for Rates Selector to be displayed. Autorating process will continue without Rates Selector", ratingAdapter.OriginMissingMessage);
		}

		public void TestMissingOrigin_QuickBooking()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			var adapter = new QuotedBookingRatingAdapter(quickBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("Should show Load Port is missing", "Load Port is mandatory for Rates Selector to be displayed. Autorating process will continue without Rates Selector", ratingAdapter.OriginMissingMessage);
		}

		public void TestMissingDestination_OneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("Should show Destination is missing", "Destination is mandatory for running Autorating Costs", ratingAdapter.DestinationMissingMessage);
		}

		public void TestMissingDestination_BookingWithQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("Should show Discharge Port is missing", "Discharge Port is mandatory for Rates Selector to be displayed. Autorating process will continue without Rates Selector", ratingAdapter.DestinationMissingMessage);
		}

		public void TestMissingDestination_QuickBooking()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			var adapter = new QuotedBookingRatingAdapter(quickBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);
			AssertEquals("Should show Discharge Port is missing", "Discharge Port is mandatory for Rates Selector to be displayed. Autorating process will continue without Rates Selector", ratingAdapter.DestinationMissingMessage);
		}

		public void TestUpdateContainerPenalties()
		{
			var description = PredefinedNoteTypes.Instance.SpotBookingPenaltiesFees.Description;
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "USLAX";
			quotedBooking.LoadPort = "AUMEL";
			quotedBooking.DischargePort = "USWBC";

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);

			var containerPenalties = new[]
			{
						new DummyContainerPenalty("20GP")
						{
							CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
							CPY_FreeTime = new DateTime(ZDateTime.Now.Year, 1, 10),
							CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
							CPY_ProcessType = ContainerPenaltyProcessType.Import,
							CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
							CPY_PerUnitCost = 32,
							CPY_RX_NKCurrency = "USD"
						}
					};

			Assert(quotedBooking.Notes.FindByDescription(description)?.Any() == false);

			ratingAdapter.UpdateContainerPenalties(containerPenalties, true);
			var currentNote = quotedBooking.Notes.FindByDescription(description).FirstOrDefault();
			Assert(currentNote != null);

			var pk = currentNote.PK;
			var noteText = currentNote.ST_NoteText;
			containerPenalties = new[]
			{
						new DummyContainerPenalty("40GP")
						{
							CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
							CPY_FreeTime = new DateTime(ZDateTime.Now.Year, 1, 10),
							CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
							CPY_ProcessType = ContainerPenaltyProcessType.Import,
							CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
							CPY_PerUnitCost = 64,
							CPY_RX_NKCurrency = "USD"
						}
					};

			ratingAdapter.UpdateContainerPenalties(containerPenalties, true);
			currentNote = quotedBooking.Notes.FindByDescription(description).FirstOrDefault();
			Assert(currentNote != null);
			AssertNotEquals(currentNote.ST_NoteText, noteText);
		}

		public void TestUpdateSpotBookingTerms()
		{
			var description = PredefinedNoteTypes.Instance.SpotBookingTermsAndFees.Description;
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "USLAX";
			quotedBooking.LoadPort = "AUMEL";
			quotedBooking.DischargePort = "USWBC";

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);
			var spotBookingNote = "test spot booking term for 20GP";

			Assert(quotedBooking.Notes.FindByDescription(description)?.Any() == false);

			ratingAdapter.UpdateSpotBookingTerms(spotBookingNote);
			var currentNote = quotedBooking.Notes.FindByDescription(description).FirstOrDefault();
			Assert(currentNote != null);

			var pk = currentNote.PK;
			var noteText = currentNote.ST_NoteText;
			spotBookingNote = "test spot booking term for 40GP";

			ratingAdapter.UpdateSpotBookingTerms(spotBookingNote);
			currentNote = quotedBooking.Notes.FindByDescription(description).FirstOrDefault();
			Assert(currentNote != null);
			AssertNotEquals(currentNote.ST_NoteText, noteText);
			AssertEquals(currentNote.ST_NoteText, spotBookingNote);
		}

		public void TestUpdateTransports()
		{
			var description = PredefinedNoteTypes.Instance.SpotBookingRouting.Description;
			var carrier = CreateCarrierOrg("MAERSK LINES PTY LTD", "MAE");
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "USLAX";
			quotedBooking.LoadPort = "SEGVX";
			quotedBooking.DischargePort = "DEBRV";
			quotedBooking.OH_Carrier = carrier.PK;

			var adapter = new QuotedBookingRatingAdapter(quotedBooking);
			var ratingAdapter = new AutoRatingProxy(adapter);

			var transports = new[]
			{
				new DummyTransport()
				{
					JW_Status = TransportStatus.Planned,
					JW_IsLinked = true,
					JW_LegOrder = 1,
					JW_TransportMode = TransportModes.Sea,
					JW_VoyageFlight = "V9849384",
					JW_Vessel = "ADRIANA D",
					JW_RL_NKLoadPort = "SEGVX",
					JW_RL_NKDiscPort = "DEBRV",
					JW_ETA = new ZDateTime(2021, 01, 15, 14, 0, 0),
					JW_ETD = new ZDateTime(2021, 01, 10, 07, 0, 0),
					JW_LegNotes = "Some Notes X",
					JW_DocumentaryCutOff = new ZDateTime(2021, 01, 08, 10, 0, 0),
					JW_TerminalCutOff = new ZDateTime(2021, 01, 08, 19, 0, 0),
					JW_VGMCutOff = new ZDateTime(2021, 01, 08, 17, 0, 0),
				},
				new DummyTransport()
				{
					JW_LegOrder = 2,
					JW_RL_NKLoadPort = "DEBRV",
					JW_RL_NKDiscPort = "ESALG",
				}
			};

			Assert(quotedBooking.Notes.FindByDescription(description)?.Any() == false);

			ratingAdapter.UpdateTransports(transports);
			var currentNote = quotedBooking.Notes.FindByDescription(description).FirstOrDefault();
			Assert(currentNote != null);

			var pk = currentNote.PK;
			var noteText = currentNote.ST_NoteText;
			transports = new[]
			{
				new DummyTransport()
				{
					JW_Status = TransportStatus.Planned,
					JW_IsLinked = true,
					JW_LegOrder = 1,
					JW_TransportMode = TransportModes.Sea,
					JW_VoyageFlight = "X9849300",
					JW_Vessel = "Maersk WW",
					JW_RL_NKLoadPort = "NLTRM",
					JW_RL_NKDiscPort = "ZADUR",
					JW_ETA = new ZDateTime(2021, 01, 15, 14, 0, 0),
					JW_ETD = new ZDateTime(2021, 01, 10, 07, 0, 0),
					JW_LegNotes = "Some Notes Y",
					JW_DocumentaryCutOff = new ZDateTime(2021, 01, 08, 10, 0, 0),
					JW_TerminalCutOff = new ZDateTime(2021, 01, 08, 19, 0, 0),
					JW_VGMCutOff = new ZDateTime(2021, 01, 08, 17, 0, 0),
				}
			};

			ratingAdapter.UpdateTransports(transports);
			currentNote = quotedBooking.Notes.FindByDescription(description).FirstOrDefault();
			Assert(currentNote != null);
			AssertNotEquals(currentNote.ST_NoteText, noteText);
		}

		#endregion

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			return CreateNewQuotedBooking(quote.PK, booking.PK);
		}

		protected virtual QuotedBooking CreateNewQuotedBooking(ZGuid quotePK, ZGuid bookingPK)
		{
			return QuotedBooking.New(quotePK, bookingPK, Factory);
		}

		public OrgHeader CreateCarrierOrg(string fullName, string scac = "SCAC")
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = scac + "CARRIER";
			carrier.OH_FullName = fullName;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsCreditor = true;
			carrier.CompanyData.SetAPTaxApplicable(false);

			if (!string.IsNullOrWhiteSpace(scac))
			{
				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_StandardCarrierAlphaCode = scac;
				carrier.OH_RSL_ShippingLine = shippingLine.PK;
			}

			return carrier;
		}

		#region Helpers

		protected TestHelper Helper => helper ?? (helper = new TestHelper(Factory));
		TestHelper helper;

		#endregion

		#endregion
	}
}
