using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class MiningInformationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountries()
		{
			var miningInformation = Factory.New<MiningInformation>();
			AssertEquals("Countries type", typeof(RefCountryCollection), miningInformation.Lookups.Countries.GetType());
		}
	}
}
