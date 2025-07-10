using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.LandTransport;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Module.OperationalActions;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(CreateLandTransportConsignmentsOperationalActionMethodApplicator))]

	public class CreateLandTransportConsignmentsOperationalActionMethodApplicatorTest : DtbBookingOperationalActionMethodApplicatorTest
	{
		public void TestCreateLandTransportConsignmentsOperationalAction()
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

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedMessage = @"INFO: Successfully created Land Transport Consignment CN00000001 from Transport Booking TB00000001";

				var targetBookings = new DtbBooking[] { booking };
				AssertNoExceptionThrown("No Exception expected", () => ApplyApplicator(targetBookings, expectedMessage));

				var query = new ZQuery(DtbConsignmentSchema.LTC_KM_Booking, booking.PK);
				var consignment = new BusinessObjectFactory().LoadTop1<IDtbConsignment>(query);
				AssertNotNull("Land transport job should exist in database.", consignment);
			}
		}

		public void TestCreateLandTransportConsignmentsOperationalAction_ErrorsOnEmptyTargets()
		{
			var expectedMessage = "ERROR: Selected Transport Bookings need to exist in database.";

			var bookings = Array.Empty<DtbBooking>();
			AssertNoExceptionThrown("No Developer Exception expected", () => ApplyApplicator(bookings, expectedMessage));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CreateLandTransportConsignmentsOperationalActionMethodApplicator("Print Cartage Advice", Factory);
		}
	}
}
