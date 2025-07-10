using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ViewLocationHelperTest : TestCaseWithFactory
	{
		public void TestGetLocationFromString()
		{
			var city = Factory.NewWithValidTestData<RefCityTown>();
			city.R9_InternationalName = "Dummy City";
			Factory.Save();

			var location = ViewLocationHelper.GetLocationFromString(Factory, "Dummy City", RefCityTownSchema.Constants.Prefix);
			AssertEquals(city.PK, location.PK);
		}

		public void TestGetLocationFromPk()
		{
			var city = Factory.NewWithValidTestData<RefCityTown>();
			city.R9_InternationalName = "Dummy City";
			Factory.Save();

			var location = ViewLocationHelper.GetLocationFromPk(Factory, city.PK);
			AssertEquals(city.PK, location.PK);
			AssertEquals("Dummy City", location.VLO_Code);
		}
	}
}
