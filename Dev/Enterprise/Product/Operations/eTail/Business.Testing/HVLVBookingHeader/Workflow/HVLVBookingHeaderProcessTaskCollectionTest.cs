namespace Enterprise.eTail.Business.Testing
{
	using Enterprise.eTail.Business;
	using Enterprise.MasterFiles.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(HVLVBookingHeaderProcessTaskCollection))]
	public class HVLVBookingHeaderProcessTaskCollectionTest : ProcessTaskCollectionTest<HVLVBookingHeaderProcessTaskCollection>
	{
		#region Implementation

		protected override HVLVBookingHeaderProcessTaskCollection GetCollectionToTestCore()
		{
			return new HVLVBookingHeaderProcessTaskCollection(BookingHeader);
		}

		HVLVBookingHeader BookingHeader
		{
			get
			{
				if (bookingHeader == null)
				{
					bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				}

				return bookingHeader;
			}
		}

		HVLVBookingHeader bookingHeader;

		#endregion
	}
}
