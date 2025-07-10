using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(DtbBookingConsolidationFilterBusinessObject))]
	public class DtbBookingConsolidationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestConsolidationIDFilter()
		{
			Factory.Save(); // to create Consolidation IDs
			var filter = (ModuleFountainFilter)FilterStrip[FilterNameConstants.ConsolidationID];

			filter.Property = "";
			Asserter.AssertMatches("No filter -- expect all Consolidations", filter, Consolidation1, Consolidation2);

			var consolID1 = Consolidation1.KB_JobID;
			AssertEquals("Precondition", false, consolID1.IsEmpty);

			filter.Property = consolID1;
			Asserter.AssertMatches(string.Format("Filtering on {0} -- expect Consolidation1 only.", consolID1), filter, Consolidation1);

			filter.Property = "CB";
			Asserter.AssertMatches("Filtering on CB* -- expect all Consolidations.", filter, Consolidation1, Consolidation2);
		}

		public void TestConsolidationStatusFilter()
		{
			var filter = (ModuleTextFilter)FilterStrip[FilterNameConstants.ConsolidationStatus];
			AssertEquals(filter.Category, FilterCategories.StatusAndFlags);
			AssertEquals(filter.List, new BookingConsolidationStatuses());

			filter.Property = "";
			Asserter.AssertMatches("No filter -- expect all Consolidations", filter, Consolidation1, Consolidation2);

			filter.Property = TransportStatuses.Codes.Delivered;
			Asserter.AssertMatches(string.Format("Filtering on {0} -- expect Consolidation1 only.", TransportStatuses.Codes.Delivered), filter, Consolidation1);

			var bookingConsolidationStatusCodes = filter.List.Cast<CodeDescriptionPair>().Select(pair => pair.Code);
			CombineAssertions("Should only contain the codes related to booking consolidation status", () =>
			{
				AssertCollectionNotContains("Should not contain status codes unrelated to booking consolidation status", TransportStatuses.Codes.Deactivated, bookingConsolidationStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking consolidation status", TransportStatuses.Codes.Quote, bookingConsolidationStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status, let alone booking consolidation status", TransportStatuses.Codes.Allocated, bookingConsolidationStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status, let alone booking consolidation status", TransportStatuses.Codes.Booked, bookingConsolidationStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status, let alone booking consolidation status", TransportStatuses.Codes.DeliveryAllocated, bookingConsolidationStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status, let alone booking consolidation status", TransportStatuses.Codes.Incomplete, bookingConsolidationStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status, let alone booking consolidation status", TransportStatuses.Codes.PickUpAllocated, bookingConsolidationStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status, let alone booking consolidation status", TransportStatuses.Codes.PickUpCommenced, bookingConsolidationStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status, let alone booking consolidation status", TransportStatuses.Codes.PickUpConfirmed, bookingConsolidationStatusCodes);
			});
			var expectedBookingConsolidationStatusCodes = new string[] { TransportStatuses.Codes.Available, TransportStatuses.Codes.Delivered, TransportStatuses.Codes.DeliveredEmptyNotReturned, TransportStatuses.Codes.Held, TransportStatuses.Codes.PickedUp, TransportStatuses.Codes.ServiceCommenced, TransportStatuses.Codes.ActionRequired };
			AssertContainsExactElementsInAnyOrder("Should contain correct booking consolidation statuses", expectedBookingConsolidationStatusCodes, bookingConsolidationStatusCodes);
		}

		public void TestBookingIDFilter()
		{
			Factory.Save();
			var filter = (ModuleFountainFilter)FilterStrip[FilterNameConstants.BookingID];

			filter.Property = "";
			Asserter.AssertMatches("No filter -- expect all Consolidations", filter, Consolidation1, Consolidation2);

			var bookingID1 = Booking1.KM_JobID;
			AssertEquals("Precondition", false, bookingID1.IsEmpty);

			filter.Property = bookingID1;
			Asserter.AssertMatches(string.Format("Filtering on {0} -- expect Consolidation1 only.", bookingID1), filter, Consolidation1);

			filter.Property = "TB";
			Asserter.AssertMatches("Filtering on TB* -- expect all Consolidations.", filter, Consolidation1, Consolidation2);
		}

		public void TestTransportCompanyFilter()
		{
			Factory.Save();
			var filter = (ModuleGuidFilter)FilterStrip[FilterNameConstants.TransportCompany];
			AssertEquals(filter.List.GetType(), typeof(LocalTransportCollection));

			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("No filter -- expect all Consolidations.", filter, Consolidation1, Consolidation2);

			filter.Property = TransportCo1.PK;
			Asserter.AssertMatches("Filtering on TransportCo1 -- expect Consolidation1 only.", filter, Consolidation1);
		}

		public void TestTransportCompanyFilter_WithOtherDocAddresses()
		{
			var transportCo = Helper.CreateOrganisation("TCO");
			var bookedByParty = Helper.CreateOrganisation("BBP");
			var consol = Helper.CreateConsolidationMultiJob(transportCo);
			consol.BookedByAddress.OrganisationPK = bookedByParty.PK;
			Factory.Save();

			Asserter.AddToScope(consol);

			var filter = (ModuleGuidFilter)FilterStrip[FilterNameConstants.TransportCompany];
			filter.Property = bookedByParty.PK;
			Asserter.AssertMatches("Should *not* have matched on DocAddress types besides TransportCompany.", filter);

			filter.Property = transportCo.PK;
			Asserter.AssertMatches("Should have matched.", filter, consol);
		}

		public void TestTransportReferenceFilter()
		{
			Factory.Save();
			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingTransportReference];
			AssertEquals(filter.Category, FilterCategories.StatusAndFlags);

			filter.Property = "";
			Asserter.AssertMatches("No filter -- expect all Consolidations.", filter, Consolidation1, Consolidation2);

			filter.Property = "abc";
			Asserter.AssertMatches("Filtering on Booking Transport Reference 'abc' -- expect Consolidation1 only.", filter, Consolidation1);
		}

		public void TestCarrierAccountFilter()
		{
			var carrierAccount = Factory.New<OrgCarrierAccount>();
			carrierAccount.OAN_AccountNumber = "123";
			carrierAccount.OAN_OH_Carrier = TransportCo1.PK;
			carrierAccount.OAN_OH_BillToParty = TransportCo2.PK;

			Booking1.KM_OAN_CarrierAccount = carrierAccount.PK;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.CarrierAccount];
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.Property = "";
			Asserter.AssertMatches("No filter -- expect all Consolidations.", filter, Consolidation1, Consolidation2);

			filter.Property = "123";
			Asserter.AssertMatches("Filtering on Carrier Account -- expect Consolidation1 only.", filter, Consolidation1);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TransportCo1 = Helper.CreateOrganisation("1");
			TransportCo2 = Helper.CreateOrganisation("2");

			Booking1 = Helper.CreateBooking(TransportCo1);
			Booking2 = Helper.CreateBooking(TransportCo2);
			Booking1.Address.E2_OA_Address = TransportCo1.MainAddress.PK;
			Booking2.Address.E2_OA_Address = TransportCo1.MainAddress.PK;
			Booking1.KM_TransportReference = "abc";

			Consolidation1 = Helper.CreateConsolidationMultiJob(TransportCo1);
			Consolidation2 = Helper.CreateConsolidationMultiJob(TransportCo2);
			Consolidation1.Bookings.Add(Booking1);
			Consolidation2.Bookings.Add(Booking2);
			Consolidation1.KB_Status = TransportStatuses.Codes.Delivered;
			Consolidation2.KB_Status = TransportStatuses.Codes.Available;

			Asserter.AddToScope(Consolidation1);
			Asserter.AddToScope(Consolidation2);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DtbBookingConsolidationFilterBusinessObject();
		}

		DtbBookingConsolidationFilterBusinessObject FilterStrip
		{
			get { return filterStrip ?? (filterStrip = new DtbBookingConsolidationFilterBusinessObject()); }
		}

		FilterStripAsserter<DtbBookingConsolidation> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<DtbBookingConsolidation>(Factory, b => b.KB_JobID)); }
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		DtbBooking Booking1;
		DtbBooking Booking2;
		DtbBookingConsolidation Consolidation1;
		DtbBookingConsolidation Consolidation2;
		OrgHeader TransportCo1;
		OrgHeader TransportCo2;

		DtbBookingConsolidationFilterBusinessObject filterStrip;
		FilterStripAsserter<DtbBookingConsolidation> asserter;
		TransportBookingTestHelper helper;
	}
}
