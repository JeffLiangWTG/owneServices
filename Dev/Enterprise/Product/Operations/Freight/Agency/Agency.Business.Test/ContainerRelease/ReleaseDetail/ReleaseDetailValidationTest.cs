using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ReleaseDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestReleaseCount()
		{
			Detail.ReleaseCount = 10;
			AssertNoNotifications(Detail.ReleaseCountInfo);
			Detail.ReleaseCount = 11;
			AssertHasError(Detail.ReleaseCountInfo, "You can't release more containers than are booked.");
			Detail.ReleaseCount = 0;
			AssertNoNotifications(Detail.ReleaseCountInfo);
			Detail.ReleaseCount = -1;
			AssertHasError(Detail.ReleaseCountInfo, "Please enter a 'Container Release Count' greater than or equal to 0.");
		}

		#region Implementation
		public ReleaseHeader Header
		{
			get
			{
				if (header == null)
				{
					header = new ReleaseHeader(Booking, false);
					header.Init();
				}

				return header;
			}
		}

		ReleaseHeader header;
		public ReleaseDetail Detail
		{
			get
			{
				return detail ?? (detail = Header.Details[0]);
			}
		}

		ReleaseDetail detail;
		public AgencyBooking Booking
		{
			get
			{
				if (booking == null)
				{
					SetupBookingAndShipment();
				}

				return booking;
			}
		}

		public AgencyBookingContainer Container
		{
			get
			{
				if (container == null)
				{
					SetupBookingAndShipment();
				}

				return container;
			}
		}

		void SetupBookingAndShipment()
		{
			booking = Factory.New<AgencyBooking>();
			container = booking.BookedContainers.AddNew();
			container.JC_ContainerCount = 10;
		}

		AgencyBooking booking;
		AgencyBookingContainer container;
		#endregion
	}
}
