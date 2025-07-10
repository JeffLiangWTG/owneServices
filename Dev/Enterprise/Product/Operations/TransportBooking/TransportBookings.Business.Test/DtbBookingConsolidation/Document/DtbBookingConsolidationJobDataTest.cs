using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Business.Test
{
	class DtbBookingConsolidationJobDataTest : TestCaseWithFactory
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(DtbBookingConsolidation), BookingConsolidationJobData.BusinessObjectType);
		}

		public void TestGetBusinessObjectCollection()
		{
			AssertEquals(typeof(DtbBookingMultiJobConsolidationCollection), BookingConsolidationJobData.GetBusinessObjectCollection(new BusinessObjectFactory()).GetType());
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.DtbBookingConsolidation, BookingConsolidationJobData.ModuleID);
		}

		public void TestReferenceType()
		{
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, BookingConsolidationJobData.ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Transport Booking Consolidation", BookingConsolidationJobData.HumanReadableName);
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals(true, BookingConsolidationJobData.IsAllowedForUnallocatedeDocs);
		}

		DtbBookingConsolidationJobData BookingConsolidationJobData
		{
			get { return bookingConsolidationJobData ?? (bookingConsolidationJobData = new DtbBookingConsolidationJobData()); }
		}
		DtbBookingConsolidationJobData bookingConsolidationJobData;
	}
}
