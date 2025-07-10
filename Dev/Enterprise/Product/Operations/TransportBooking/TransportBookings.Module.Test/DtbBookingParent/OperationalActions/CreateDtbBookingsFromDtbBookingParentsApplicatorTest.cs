using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(CreateDtbBookingsFromDtbBookingParentsApplicator))]
	sealed class CreateDtbBookingsFromDtbBookingParentsApplicatorTest : BaseCreateDtbBookingsFromDtbBookingParentsApplicatorTest
	{
		public void TestCreateDtbBookingsFromDtbBookingParents()
		{
			var parents = CreateForwardingShipmentsToCreateDtbBookingsFrom();

			Applicator.Direction = "PIC";
			Applicator.BookingTemplate = "EFPR";

			var log = SimulateRun(parents, true);

			var jobIDs = new List<string>();

			CombineAssertions("Check that CreateTransportBookings() with default template and settings from operational action will result in correct bookings and log", () =>
			{
				foreach (IDtbBookingParent shipment in parents)
				{
					var jobID = AssertBookingWasCreatedForShipmentAndHasCorrectDetailsAndReturnBookingJobID(shipment);
					jobIDs.Add(jobID);
				}
				AssertEquals("Should have been four Transport Bookings created, one for each Shipment", 4, jobIDs.Count);

				Assert("Should have been no Global messages", !UnitTestUserNotification.Instance.PreviousMessages.Any(m => !m.Text.IsNullOrEmpty()));

				var jobIDsArray = jobIDs.ToArray();
				var expectedLog = FormattableString.Invariant(
@$"INFO: Checking Shipment S1
INFO: Checking Shipment S2
INFO: Checking Shipment S3
INFO: Checking Shipment S4
INFO: Transport Booking {jobIDsArray[0]} created for Shipment S1
INFO: Transport Booking {jobIDsArray[1]} created for Shipment S2
INFO: Transport Booking {jobIDsArray[2]} created for Shipment S3
INFO: Transport Booking {jobIDsArray[3]} created for Shipment S4
").SplitByLine();
				var actualLog = log.MessagesString().SplitByLine();
				AssertContainsExactElementsInExactOrder(
					"Operational Action Section Log is correct",
					expectedLog,
					actualLog);
			});
		}

		string AssertBookingWasCreatedForShipmentAndHasCorrectDetailsAndReturnBookingJobID(IDtbBookingParent shipment)
		{
			var query = new ZQuery();

			var bookingQuery = new ZDBOnlyQuery(typeof(DtbBooking));
			var bookingConsolidationQuery = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
			bookingConsolidationQuery.AddToFilter(DtbBookingConsolidationSchema.KB_ParentID, SQLComparisonOperator.Equal, shipment.PK);
			bookingConsolidationQuery.AddToFilter(DtbBookingConsolidationSchema.KB_ParentTableCode, SQLComparisonOperator.Equal, shipment.TablePrefix);
			bookingQuery.AddSubQuery(bookingConsolidationQuery, JoinCondition.And);
			query.AddToFilter(bookingQuery);
			var booking = Factory.LoadTop1<DtbBooking>(query);
			AssertNotNull("Should have created a booking for shipment " + shipment.JobNumber, booking);
			if (booking != null)
			{
				AssertEquals("Booking for shipment " + shipment.JobNumber + " should have specified template", "EFPR", booking.KM_KT_NKBookingTemplate);
				AssertEquals("Booking consolidation for shipment " + shipment.JobNumber + " should have correct direction", "PIC", booking.ConsolidationSingleJob.KB_JobDirection);
				AssertEquals("Booking for shipment " + shipment.JobNumber + " should have correct direction", "ORG", booking.KM_Direction);
				AssertEquals("Booking for shipment " + shipment.JobNumber + " should not be overridden", false, booking.ConsolidationSingleJob.KB_IsOverridden);
				AssertEquals("Booking for shipment " + shipment.JobNumber + " should have all packages from shipment", 2, booking.TotalPackages);
			}
			return booking?.KM_JobID ?? string.Empty;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CreateDtbBookingsFromDtbBookingParentsApplicator(new BusinessObjectFactory());
		}

		new CreateDtbBookingsFromDtbBookingParentsApplicator Applicator => (CreateDtbBookingsFromDtbBookingParentsApplicator)base.Applicator;
	}
}
