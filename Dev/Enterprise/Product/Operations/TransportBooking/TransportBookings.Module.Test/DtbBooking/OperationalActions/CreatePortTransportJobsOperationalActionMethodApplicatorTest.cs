using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Module.OperationalActions;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(CreatePortTransportJobsOperationalActionMethodApplicator))]
	public class CreatePortTransportJobsOperationalActionMethodApplicatorTest : DtbBookingOperationalActionMethodApplicatorTest
	{
		public void TestCreatePortTransportJobsOperationalAction()
		{
			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);
			var validConfirmationDate = now.AddHours(1); // one hour in the future
			var validJobCreatedDate = now.AddDays(-10).AddMinutes(-30); // 10 days, 30 minutes old

			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper);
			BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, booking, validJobCreatedDate, validConfirmationDate);

			Factory.Save();

			var expectedMessage = @"INFO: Successfully created Port Transport T00001000 from Transport Booking TB00000001";

			var bookings = new DtbBooking[] { booking };
			AssertNoExceptionThrown("No Exception expected", () => ApplyApplicator(bookings, expectedMessage));

			var query = new ZQuery(JobCartageSchema.JJ_ParentID, booking.PK);
			query.AddToFilter(JobCartageSchema.JJ_ParentTableCode, booking.TablePrefix);
			var typeCommonCartage = CargoWise.Application.ObjectFactory.GetType("ICommonCartage");
			var portTransportRecords = Factory.Load(typeCommonCartage, query);
			AssertNotNull("Port transport job should exist in database.", portTransportRecords);
		}

		public void TestCreatePortTransportJobsOperationalAction_ErrorsOnEmptyTargets()
		{
			var expectedMessage = "ERROR: Selected Transport Bookings need to exist in database.";

			var bookings = Array.Empty<DtbBooking>();
			AssertNoExceptionThrown("No Developer Exception expected", () => ApplyApplicator(bookings, expectedMessage));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CreatePortTransportJobsOperationalActionMethodApplicator("Print Cartage Advice", Factory);
		}
	}
}
