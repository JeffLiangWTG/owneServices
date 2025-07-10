using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgContactLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var lookups = Factory.New<OrgContact>().Lookups;
			AssertNotNull("Active job Categories not null", lookups.JobCategory_List);
			AssertEquals("NationalityTypes", typeof(RefCountryCollection), lookups.NationalityTypes.GetType());
			Assert("Gender", lookups.Gender.ContainsCode(Core.Constants.Genders.Woman));
		}

		public void TestGenders()
		{
			var lookups = Factory.New<OrgContact>().Lookups;
			Assert("Male", lookups.Gender.ContainsCode(Core.Constants.Genders.Man));
			Assert("Female", lookups.Gender.ContainsCode(Core.Constants.Genders.Woman));
			Assert("Non-Binary", lookups.Gender.ContainsCode(Core.Constants.Genders.NonBinary));
			Assert("NotSpecified", lookups.Gender.ContainsCode(Core.Constants.Genders.NotSpecified));
			Assert("Custom", lookups.Gender.ContainsCode(Core.Constants.Genders.Custom));
			Assert("Agender", lookups.Gender.ContainsCode(Core.Constants.Genders.Agender));
		}
	}
}
