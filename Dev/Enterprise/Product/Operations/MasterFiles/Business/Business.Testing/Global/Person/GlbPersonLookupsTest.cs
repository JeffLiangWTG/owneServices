using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbPersonLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var person = Factory.New<GlbPerson>();
			AssertContains(Core.SharedConstants.Languages.English, person.Lookups.Languages.CodesAsString);
			person.PER_RN_NKNationalityCodeISO = Core.Constants.CountryCodes.SouthAfrica;
			AssertContains(Core.SharedConstants.Languages.English, person.Lookups.Languages.CodesAsString);
			AssertContains("ZUL", person.Lookups.Languages.CodesAsString);
			AssertContains("M", person.Lookups.Genders.CodesAsString);
		}
	}
}
