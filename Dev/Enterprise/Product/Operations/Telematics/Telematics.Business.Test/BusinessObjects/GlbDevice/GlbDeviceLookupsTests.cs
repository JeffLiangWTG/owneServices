using CargoWise.EntityFramework.Testing;

namespace Enterprise.Telematics.Business.Test
{
	class GlbDeviceLookupsTests : BusinessObjectLookupsTestCase
	{
		public void TestGlbDeviceStatusList()
		{
			var lookup = new GlbDeviceLookups(Factory.New<GlbDevice>());
			var list = lookup.GlbDeviceStatusList;
			AssertEquals(6, list.Count);

			Assert(list.ContainsCode("FUR"));
			Assert(list.ContainsCode("PUR"));
			Assert(list.ContainsCode("PRG"));
			Assert(list.ContainsCode("REG"));
			Assert(list.ContainsCode("FRG"));
			Assert(list.ContainsCode("UNR"));

			AssertEquals("De-registration Failure", list.GetDescriptionFromCode("FUR"));
			AssertEquals("Pending De-registration", list.GetDescriptionFromCode("PUR"));
			AssertEquals("Pending Registration", list.GetDescriptionFromCode("PRG"));
			AssertEquals("Registered", list.GetDescriptionFromCode("REG"));
			AssertEquals("Registration Failure", list.GetDescriptionFromCode("FRG"));
			AssertEquals("Unregistered", list.GetDescriptionFromCode("UNR"));
		}
	}
}
