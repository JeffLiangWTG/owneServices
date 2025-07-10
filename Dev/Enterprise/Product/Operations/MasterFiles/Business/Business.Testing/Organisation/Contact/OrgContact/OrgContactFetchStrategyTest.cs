using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgContactFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForValidate()
		{
			var factory = new BusinessObjectFactory();
			var fetchStrategy = new OrgContactFetchStrategy(factory.NewWithValidTestData<OrgContact>());

			using (Env.SetTemporaryUserContext(null))
			{
				AssertNoExceptionThrown("No exception is expected here", () => fetchStrategy.FetchForValidate());
			}
		}
	}
}
