using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Business.Testing
{
	class DtbBookingJobDataTest : TestCaseWithFactory
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(DtbBooking), BookingJobData.BusinessObjectType);
		}

		public void TestGetBusinessObjectCollection()
		{
			AssertEquals(typeof(DtbBookingCollection), BookingJobData.GetBusinessObjectCollection(new BusinessObjectFactory()).GetType());
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.DtbBooking, BookingJobData.ModuleID);
		}

		public void TestReferenceType()
		{
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, BookingJobData.ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Transport Booking", BookingJobData.HumanReadableName);
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals(true, BookingJobData.IsAllowedForUnallocatedeDocs);
		}

		DtbBookingJobData BookingJobData
		{
			get { return bookingJobData ?? (bookingJobData = new DtbBookingJobData()); }
		}
		DtbBookingJobData bookingJobData;
	}
}
