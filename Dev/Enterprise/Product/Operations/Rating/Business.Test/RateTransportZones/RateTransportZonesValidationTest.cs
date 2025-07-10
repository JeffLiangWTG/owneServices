using CargoWise.EntityFramework.Testing;

namespace Enterprise.Rating.Business.Testing
{
	public class RateTransportZonesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUniqueZoneName()
		{
			var provider = Factory.New<RateTransportProvider>();

			var zone1 = provider.Zones.AddNew();
			zone1.TZ_ZoneName = "splaty";
			AssertNoErrors("Zone name is unique, no error", zone1.TZ_ZoneNameInfo);

			var zone2 = provider.Zones.AddNew();
			zone2.TZ_ZoneName = "splaty";
			AssertHasErrors("Zone name is a duplicate, there should be an error", zone2.TZ_ZoneNameInfo);
		}

		public void TestMandatoryZoneName()
		{
			var provider = Factory.New<RateTransportProvider>();
			var zone = provider.Zones.AddNew();

			zone.TZ_ZoneName = "splaty";
			AssertNoErrors("Having a zone name is valid", zone.TZ_ZoneNameInfo);
			zone.TZ_ZoneName = "";
			AssertHasErrors("Empty zone name should have an error", zone.TZ_ZoneNameInfo);
		}
	}
}
