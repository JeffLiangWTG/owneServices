using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ReleaseDetailTest : BaseAgencyTest
	{
		public void TestProxyFromContainer()
		{
			Container.JC_RC = RC_20GP_PK;
			Container.JC_ContainerCount = 10;
			Container.JC_ContainerQuality = "QAL";
			Container.JC_ContainerStatus = "STA";
			Container.JC_OA_DepartureContainerYardAddress = Address.PK;
			AssertEquals("20GP", Detail.ContainerType);
			AssertEquals(10, (int)Detail.ContainerCount);
			AssertEquals("QAL", Detail.ContainerQuality);
			AssertEquals("STA", Detail.ContainerStatus);
			AssertEquals(Address.PK, Detail.ContainerYardAddress);
			AssertEquals(Address.OA_OH, Detail.ContainerYardOrg);
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
		public OrgAddress Address
		{
			get
			{
				if (address == null)
				{
					address = Factory.New<OrgHeader>().MainAddress;
					address.Header.OH_FullName = "CY";
				}

				return address;
			}
		}

		OrgAddress address;
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
		}

		AgencyBooking booking;
		AgencyBookingContainer container;
		#endregion
	}
}
