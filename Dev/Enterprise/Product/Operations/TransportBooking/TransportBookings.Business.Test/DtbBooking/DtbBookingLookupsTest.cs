using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Shared.Lists;

namespace Enterprise.TransportBookings.Business.Testing
{
	sealed class DtbBookingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBookingTemplates()
		{
			var booking = GetNewBooking();
			AssertContainsExactElementsInAnyOrder(GetExpectedBookingTmplCollection(), booking.Lookups.BookingTemplates);
		}

		DtbBookingTmplCollection GetExpectedBookingTmplCollection()
		{
			Data.CreateTransportBookingTemplates();
			return new BindToLists(Factory).BookingTemplates;
		}

		public void TestLocalTransportOrganisations()
		{
			var booking = Helper.CreateBooking();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(LocalTransportCollection), booking.Lookups.LocalTransportOrganisations.GetType());
		}

		public void TestLocalTransportOrganisationsHasOrganisationDefaultProviderAttribute()
		{
			var propertyInfo = typeof(DtbBookingLookups).GetProperty(nameof(DtbBookingLookups.LocalTransportOrganisations));

			OrganisationDefaultProviderAttribute ordDefaultAttr = null;

			foreach (var attr in propertyInfo.GetCustomAttributes(true))
			{
				if (attr as OrganisationDefaultProviderAttribute != null)
				{
					ordDefaultAttr = attr as OrganisationDefaultProviderAttribute;
				}
			}

			AssertNotNull(ordDefaultAttr);
			AssertEquals(OrganisationTypes.Carrier, ordDefaultAttr.OrganisationType);
		}

		public void TestRatingFreightModes()
		{
			var booking = Helper.CreateBooking();
			AssertContainsExactElementsInAnyOrder(new BindToLists(Factory).RatingFreightModes.List, booking.Lookups.RatingFreightModes);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;

		TransportBookingTestData Data
		{
			get { return data ?? (data = new TransportBookingTestData(Factory)); }
		}
		TransportBookingTestData data;

		public void TestBindToLists()
		{
			var lookups = Factory.New<DtbBooking>().Lookups;
			AssertNotNull(lookups.BindToLists);
			AssertEquals(lookups.BindToLists, lookups.BindToLists);
		}

		public void TestCarrierAccounts()
		{
			var carrier12 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();

			var can1 = Factory.New<OrgCarrierAccount>();
			can1.OAN_OH_Carrier = carrier12.PK;
			var can2 = Factory.New<OrgCarrierAccount>();
			can2.OAN_OH_Carrier = carrier12.PK;
			var can3 = Factory.New<OrgCarrierAccount>();
			can3.OAN_OH_Carrier = carrier3.PK;

			var booking = Helper.CreateBooking(carrier12);
			var lookups = booking.Lookups;
			AssertContainsExactElementsInAnyOrder(new[] { can1, can2 }, lookups.CarrierAccounts);
		}

		public void TestCarrierAccounts_EmptyTransportCompany()
		{
			var booking = Helper.CreateBooking();
			var lookups = booking.Lookups;
			AssertEquals(0, lookups.CarrierAccounts.Count);
		}

		public void TestBookingTransportModes()
		{
			var booking = Helper.CreateBooking();
			var lookups = booking.Lookups;
			AssertContainsExactElementsInAnyOrder(new BookingTransportModes().List, lookups.BookingTransportModes);
		}

		public void TestCarrierServiceLevelCollections()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var booking1 = Helper.CreateBooking(org1);
			var booking2 = Helper.CreateBooking(org2);
			org1.MiscServ.CarrierServiceLevels.AddNew();

			CombineAssertions("Bookings carrier service level populates distinct collection based on Orgs Settings", () =>
			{
				AssertEquals(org1.MiscServ.CarrierServiceLevels.Count, booking1.Lookups.CarrierServiceLevels.Count);
				AssertNotEquals("Booking1 should have a distinct collection and be uniquely cached", booking1.Lookups.CarrierServiceLevels.Count, booking2.Lookups.CarrierServiceLevels.Count);
			});
		}

		public void TestCarrierAccountLevelCollection()
		{
			var carrier12 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();

			var can1 = Factory.New<OrgCarrierAccount>();
			can1.OAN_OH_Carrier = carrier12.PK;
			var can2 = Factory.New<OrgCarrierAccount>();
			can2.OAN_OH_Carrier = carrier12.PK;
			var can3 = Factory.New<OrgCarrierAccount>();
			can3.OAN_OH_Carrier = carrier3.PK;

			var booking1 = Helper.CreateBooking(carrier12);
			var booking2 = Helper.CreateBooking(carrier3);

			CombineAssertions("Bookings carrier accounts populates distinct collection based on Orgs Settings", () =>
			{
				AssertEquals(carrier12.CarrierAccounts.Count, booking1.Lookups.CarrierAccounts.Count);
				AssertNotEquals("Booking1 should have a distinct collection and be uniquely cached", booking1.Lookups.CarrierAccounts.Count, booking2.Lookups.CarrierAccounts.Count);
			});
		}

		DtbBooking GetNewBooking()
		{
			return Helper.CreateBooking();
		}
	}
}
