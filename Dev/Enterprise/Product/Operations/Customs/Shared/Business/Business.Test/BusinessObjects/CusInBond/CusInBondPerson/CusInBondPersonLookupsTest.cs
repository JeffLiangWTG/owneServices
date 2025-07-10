using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusInBondPersonLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var lookups = Factory.New<CusInBondPersonForTesting>().Lookups;
			Assert("Gender", lookups.Gender.ContainsCode(Core.Constants.Genders.Woman));
		}
	}
}
