using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	public class CargoSphereModuleTest : TestCase
	{
		public void TestSecurityCheckpoint_ReturnCheckpointBasedOnGrantedOnes()
		{
			using (var module = new CargoSphereModule())
			{
				var checkpointRateSearch = Env.Security.WiseRatesCargoSphereRateSearch;
				var checkpointSUDS = Env.Security.WiseRatesCargoSphereContractManagement;

				checkpointRateSearch.IsAllowed = true;
				checkpointSUDS.IsAllowed = true;
				AssertEquals("Should return Rate Search checkpoint", Env.Security.WiseRatesCargoSphereRateSearch, module.SecurityCheckpoint);

				checkpointRateSearch.IsAllowed = false;
				checkpointSUDS.IsAllowed = true;
				AssertEquals("Should return SUDS checkpoint", Env.Security.WiseRatesCargoSphereContractManagement, module.SecurityCheckpoint);

				checkpointRateSearch.IsAllowed = true;
				checkpointSUDS.IsAllowed = false;
				AssertEquals("Should return Rate Search checkpoint", Env.Security.WiseRatesCargoSphereRateSearch, module.SecurityCheckpoint);

				checkpointRateSearch.IsAllowed = false;
				checkpointSUDS.IsAllowed = false;
				AssertEquals("Should return Rate Search checkpoint", Env.Security.WiseRatesCargoSphereRateSearch, module.SecurityCheckpoint);
			}
		}
	}
}
