using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.LandTransport;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class TransportBookingLoaderTest : DtbBookingTestCaseWithFactory
	{
		public void TestGetRelatedTransportBookingEvents()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			var relatedObjectsWithEvents = TransportBookingLoader.GetRelatedTransportBookingEvents(parent);
			AssertEquals("Should not have any Related Object with Events yet.", 0, relatedObjectsWithEvents.Count());

			var consolidationBooking = Helper.CreateConsolidation(parent);
			var booking1 = Helper.CreateBooking(consolidationBooking);
			var booking2 = Helper.CreateBooking(consolidationBooking);
			var outOfScopeBooking = Helper.CreateBooking();

			var consignmentConsol = Helper.CreateConsignmentConsol();
			consignmentConsol.KB_ParentID = parent.PK;
			var consignment = ((DtbTransportConsolidation)consignmentConsol).Bookings.AddNew();

			relatedObjectsWithEvents = TransportBookingLoader.GetRelatedTransportBookingEvents(parent);
			AssertEquals("Should contain 2 Related TB Objects.", 2, relatedObjectsWithEvents.Count());
			AssertContainsExactElementsInAnyOrder("Should contain both Related TB Objects.", new[] { booking1, booking2 }, relatedObjectsWithEvents);

			var consolidationBooking2 = Helper.CreateConsolidation(parent);
			var booking3 = Helper.CreateBooking(consolidationBooking2);
			relatedObjectsWithEvents = TransportBookingLoader.GetRelatedTransportBookingEvents(parent);
			AssertEquals("Should contain 3 Related TB Objects.", 3, relatedObjectsWithEvents.Count());
			AssertContainsExactElementsInAnyOrder("Should contain all Related TB Objects.", new[] { booking1, booking2, booking3 }, relatedObjectsWithEvents);
		}

		public void TestGetRelatedTransportBookingEvents_WithOtherConsolidationTypes()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			var relatedObjectsWithEvents = TransportBookingLoader.GetRelatedTransportBookingEvents(parent);
			AssertEquals("Should not have any Related Object with Events yet.", 0, relatedObjectsWithEvents.Count());

			// We must filter out Consignments to avoid the type checking exception, but are not required to filter out other Booking Consol types
			// These should not be possible functionally, but will handle it to avoid possible regressions.
			var quotedBookingConsol = Helper.CreateConsolidation(parent);
			quotedBookingConsol.KB_JobType = TransportConsolidationJobTypes.Codes.QuotedBooking;
			var quotedBooking = Helper.CreateBooking(quotedBookingConsol);

			var multiJobConsol = Helper.CreateConsolidation(parent);
			multiJobConsol.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			var multiJobBooking = Helper.CreateBooking(multiJobConsol);

			var relatedObjects = TransportBookingLoader.GetRelatedTransportBookingEvents(parent);
			AssertEquals("Should contain 2 Related TB Objects.", 2, relatedObjects.Count());
			AssertContainsExactElementsInAnyOrder("Should contain both Related TB Objects.", new[] { quotedBooking, multiJobBooking }, relatedObjects);
		}

		public void TestGetRelatedTransportBookingEDocs()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			var relatedObjectsWithEDocs = TransportBookingLoader.GetRelatedTransportBookingEDocs(parent);
			AssertEquals("Should not have any Related Object with EDocs yet.", 0, relatedObjectsWithEDocs.Count());

			var consolidationBooking = Helper.CreateConsolidation(parent);
			var booking1 = Helper.CreateBooking(consolidationBooking);
			var booking2 = Helper.CreateBooking(consolidationBooking);
			var outOfScopeBooking = Helper.CreateBooking();

			var consignmentConsol = Helper.CreateConsignmentConsol();
			consignmentConsol.KB_ParentID = parent.PK;
			var consignment = ((DtbTransportConsolidation)consignmentConsol).Bookings.AddNew();

			relatedObjectsWithEDocs = TransportBookingLoader.GetRelatedTransportBookingEDocs(parent);
			AssertEquals("Should contain 2 Related TB Objects.", 3, relatedObjectsWithEDocs.Count());
			AssertContainsExactElementsInExactOrder("Should contain both Consolidation and Related TB Objects.", new BusinessObject[] { consolidationBooking, booking1, booking2 }, relatedObjectsWithEDocs);

			var consolidationBooking2 = Helper.CreateConsolidation(parent);
			var booking3 = Helper.CreateBooking(consolidationBooking2);
			relatedObjectsWithEDocs = TransportBookingLoader.GetRelatedTransportBookingEDocs(parent);
			AssertEquals("Should contain 3 Related TB Objects.", 5, relatedObjectsWithEDocs.Count());
			AssertContainsExactElementsInExactOrder("Should contain Consolidation and Related TB Objects.", new BusinessObject[] { consolidationBooking, consolidationBooking2, booking1, booking2, booking3 }, relatedObjectsWithEDocs);

			var landTransportConsignment = Factory.New<IDtbConsignment>();
			landTransportConsignment.LTC_KM_Booking = booking3.PK;

			relatedObjectsWithEDocs = TransportBookingLoader.GetRelatedTransportBookingEDocs(parent);
			AssertEquals("Should contain Land Transport Consignment as well as 3 related TB Objects and consolidationBookings", 6, relatedObjectsWithEDocs.Count());
			AssertContainsExactElementsInExactOrder("Should contain Consolidation and Related TB Objects and LandTransportConsignment child of TB3.", new[] { consolidationBooking, consolidationBooking2, booking1, booking2, booking3, (BusinessObject)landTransportConsignment }, relatedObjectsWithEDocs);

			var portTransport2 = Factory.New<ICommonCartage>();
			portTransport2.JJ_ParentID = booking3.PK;
			portTransport2.JJ_ParentTableCode = booking3.TablePrefix;
			portTransport2.JJ_ConsignmentID = "2";

			var portTransport1 = Factory.New<ICommonCartage>();
			portTransport1.JJ_ParentID = booking3.PK;
			portTransport1.JJ_ParentTableCode = booking3.TablePrefix;
			portTransport1.JJ_ConsignmentID = "1";

			relatedObjectsWithEDocs = TransportBookingLoader.GetRelatedTransportBookingEDocs(parent);
			AssertEquals("Should also contain port transport objects", 8, relatedObjectsWithEDocs.Count());
			AssertContainsExactElementsInExactOrder("Should have ordered port transports by ConsignmentID", new[] { consolidationBooking, consolidationBooking2, booking1, booking2, booking3, (BusinessObject)landTransportConsignment, (BusinessObject)portTransport1, (BusinessObject)portTransport2 }, relatedObjectsWithEDocs);
		}

		public void TestGetRelatedTransportBookingJobInvoicingPlugIn()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			var relatedObjectsWithEvents = TransportBookingLoader.GetRelatedTransportBookingJobInvoicingPlugIn(parent);
			AssertEquals("Should not have any Related Object with Events yet.", 0, relatedObjectsWithEvents.Count());

			var consolidationBooking = Helper.CreateConsolidation(parent);
			var booking1 = Helper.CreateBooking(consolidationBooking);
			var booking2 = Helper.CreateBooking(consolidationBooking);
			var outOfScopeBooking = Helper.CreateBooking();
			var packageJob = Helper.CreatePackageJob(consolidationBooking);
			var container = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);
			Helper.CreateAndAssignPackageDivots(booking1, container);
			Helper.CreateAndAssignPackageDivots(booking2, container);

			booking1.KM_KT_NKBookingTemplate = "EECR";
			booking2.KM_KT_NKBookingTemplate = "EECR";
			outOfScopeBooking.KM_KT_NKBookingTemplate = "EECR";

			var consignmentConsol = Helper.CreateConsignmentConsol();
			consignmentConsol.KB_ParentID = parent.PK;
			var consignment = ((DtbTransportConsolidation)consignmentConsol).Bookings.AddNew();

			relatedObjectsWithEvents = TransportBookingLoader.GetRelatedTransportBookingJobInvoicingPlugIn(parent);
			AssertEquals("Should contain 2 Related TB Objects.", 2, relatedObjectsWithEvents.Count());
			AssertContainsExactElementsInAnyOrder("Should contain both Related TB Objects.", new[] { booking1, booking2 }, relatedObjectsWithEvents);

			var consolidationBooking2 = Helper.CreateConsolidation(parent);
			var booking3 = Helper.CreateBooking(consolidationBooking2);
			packageJob = Helper.CreatePackageJob(consolidationBooking2);
			container = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);
			Helper.CreateAndAssignPackageDivots(booking3, container);

			booking3.KM_KT_NKBookingTemplate = "EECR";
			relatedObjectsWithEvents = TransportBookingLoader.GetRelatedTransportBookingJobInvoicingPlugIn(parent);
			AssertEquals("Should contain 3 Related TB Objects.", 3, relatedObjectsWithEvents.Count());
			AssertContainsExactElementsInAnyOrder("Should contain all Related TB Objects.", new[] { booking1, booking2, booking3 }, relatedObjectsWithEvents);

			booking3.KM_RatingFreightMode = "";
			relatedObjectsWithEvents = TransportBookingLoader.GetRelatedTransportBookingJobInvoicingPlugIn(parent);
			AssertEquals("Should contain 2 Related TB Objects. 3rd one is not rateable", 2, relatedObjectsWithEvents.Count());
			AssertContainsExactElementsInAnyOrder("Should contain all Related TB Objects that are rateable.", new[] { booking1, booking2 }, relatedObjectsWithEvents);
		}

		public void TestGetBookingConsolidations()
		{
			var parent1 = Factory.New<DummyWithDtbBooking>();
			var parent2 = Factory.New<DummyWithDtbBooking>();
			AssertEquals("Should not have any Booking Consolidations yet.", 0, TransportBookingLoader.GetBookingConsolidations(parent1).Length);

			var tbConsolOnParent1 = Helper.CreateConsolidation(parent1);
			var tbConsolOnParent2 = Helper.CreateConsolidation(parent2);

			var consignmentConsol = Helper.CreateConsignmentConsol();
			consignmentConsol.KB_ParentID = parent1.PK;
			var consignment = ((DtbTransportConsolidation)consignmentConsol).Bookings.AddNew();

			var quotedBookingConsol = Helper.CreateConsolidation(parent1);
			quotedBookingConsol.KB_JobType = TransportConsolidationJobTypes.Codes.QuotedBooking;

			var multiJobConsol = Helper.CreateConsolidation(parent1);
			multiJobConsol.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;

			var parentBooking1BookingConsolidations = TransportBookingLoader.GetBookingConsolidations(parent1);
			AssertContainsExactElementsInAnyOrder("Parent1 should contain 3 Booking Consolidations.", new[] { tbConsolOnParent1, quotedBookingConsol, multiJobConsol }, parentBooking1BookingConsolidations);
			AssertContainsExactElementsInAnyOrder("Parent2 should contain 1 Booking Consolidation.", new[] { tbConsolOnParent2 }, TransportBookingLoader.GetBookingConsolidations(parent2));
		}

		public void TestGetBookingPKs()
		{
			var parent1 = Factory.New<DummyWithDtbBooking>();
			var parent2 = Factory.New<DummyWithDtbBooking>();
			AssertEquals("Should not have any Booking Consolidations yet.", 0, TransportBookingLoader.GetBookingConsolidations(parent1).Length);

			var tbConsolOnParent1 = Helper.CreateConsolidation(parent1);
			var tbConsolOnParent2 = Helper.CreateConsolidation(parent2);

			var booking1 = tbConsolOnParent1.Bookings.AddNew();

			var booking2 = tbConsolOnParent2.Bookings.AddNew();
			var booking3 = tbConsolOnParent2.Bookings.AddNew();
			var booking4 = tbConsolOnParent2.Bookings.AddNew();

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Parent1 should have 1 related booking.", new[] { booking1.PK }, TransportBookingLoader.GetBookingPKs(parent1));
			AssertContainsExactElementsInAnyOrder("Parent2 should have 3 related bookings.", new[] { booking2.PK, booking3.PK, booking4.PK }, TransportBookingLoader.GetBookingPKs(parent2));
		}
	}
}
