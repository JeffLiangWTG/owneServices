using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.eTail.Business.Testing
{
	class HVLVBookingHeaderDataTest : AssemblyDataTest
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(HVLVBookingHeader), BookingHeaderData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			AssertType<HVLVBookingHeaderAdhocEdocsSupportCollection>(BookingHeaderData.GetBusinessObjectCollection(Factory));
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, BookingHeaderData.ReferenceType);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			AssertEquals("Booking Header", BookingHeaderData.HumanReadableName.ToString());
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert(BookingHeaderData.IsAllowedForUnallocatedeDocs);
		}

		#endregion

		#region Implementation

		HVLVBookingHeaderData BookingHeaderData
		{
			get
			{
				return bookingHeaderJobData ?? (bookingHeaderJobData = new HVLVBookingHeaderData());
			}
		}

		HVLVBookingHeaderData bookingHeaderJobData;

		#endregion
	}
}
