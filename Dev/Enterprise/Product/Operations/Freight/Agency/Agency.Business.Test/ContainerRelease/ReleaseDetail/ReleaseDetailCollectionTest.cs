using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ReleaseDetailCollectionTest : BaseAgencyTest
	{
		public void TestRePopulate()
		{
			AgencyBookingContainer container1 = Booking.BookedContainers.AddNew();
			container1.JC_RC = RC_20GP_PK;
			container1.JC_ContainerCount = 10;
			container1.JC_OA_DepartureContainerYardAddress = Address1.PK;
			container1.JC_ReleaseNum = "Ref-1";
			AgencyBookingContainer container2 = Booking.BookedContainers.AddNew();
			container2.JC_RC = RC_40GP_PK;
			container2.JC_ContainerCount = 10;
			container2.JC_OA_DepartureContainerYardAddress = Address2.PK;
			container2.JC_ReleaseNum = "Ref-2";
			AgencyBookingContainer container3 = Booking.BookedContainers.AddNew();
			container3.JC_RC = RC_20RE_PK;
			container3.JC_ContainerCount = 10;
			container3.JC_OA_DepartureContainerYardAddress = Address1.PK;
			AgencyBookingContainer container4 = Booking.BookedContainers.AddNew();
			container4.JC_RC = RC_40RE_PK;
			container4.JC_ContainerCount = 10;
			container4.JC_OA_DepartureContainerYardAddress = Address2.PK;
			ReleaseDetailCollection collection = new ReleaseDetailCollection(Booking);
			collection.RePopulate("Ref-1");
			AssertDetail("Ref-1", collection, container1, container3);
			collection.RePopulate("");
			AssertDetail("Ref-1", collection, container3, container4);
			collection.RePopulate("Ref-2");
			AssertDetail("Ref-1", collection, container2, container4);
		}

		#region Implementation
		void AssertDetail(string message, ReleaseDetailCollection collection, params AgencyShipmentContainer[] containers)
		{
			Converter<AgencyShipmentContainer, string> toStr = delegate(AgencyShipmentContainer container)
			{
				if (container.DepartureContainerYardAddress == null)
				{
					return container.JC_ContainerCode + ":" + container.JC_ReleaseNum;
				}
				else
				{
					return container.JC_ContainerCode + ":" + container.JC_ReleaseNum + ":" + container.DepartureContainerYardAddress.Header.OH_FullName;
				}
			};
			AssertContainsExactElementsInAnyOrder(message, toStr, containers, Array.ConvertAll(collection.ToArray<ReleaseDetail>(), (d) => d.Container));
		}

		public OrgAddress Address1
		{
			get
			{
				if (address1 == null)
				{
					address1 = Factory.New<OrgHeader>().MainAddress;
					address1.Header.OH_FullName = "CY1";
				}

				return address1;
			}
		}

		OrgAddress address1;
		public OrgAddress Address2
		{
			get
			{
				if (address2 == null)
				{
					address2 = Factory.New<OrgHeader>().MainAddress;
					address2.Header.OH_FullName = "CY2";
				}

				return address2;
			}
		}

		OrgAddress address2;
		public AgencyBooking Booking
		{
			get
			{
				return booking ?? (booking = Factory.New<AgencyBooking>());
			}
		}

		AgencyBooking booking;
		#endregion
	}
}
