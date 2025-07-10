using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGCommonDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypes()
		{
			AssertEquals("SPP, STS", lookups.Types.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var commonData = Factory.New<UNDGCommonData>();
			lookups = new UNDGCommonDataLookups(commonData);
		}
		UNDGCommonDataLookups lookups;
	}
}
