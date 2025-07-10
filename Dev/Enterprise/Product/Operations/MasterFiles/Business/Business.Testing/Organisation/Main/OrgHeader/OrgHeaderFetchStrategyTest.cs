using CargoWise.EntityFramework;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgHeaderFetchStrategyTest : TestCase
	{
		public void TestAddFetchHintWithGlbCurrentCompany()
		{
			var factory = new BusinessObjectFactory();
			var fetchStrategy = new OrgHeaderFetchStrategy(factory.NewWithValidTestData<OrgHeader>());

			using (Env.SetTemporaryUserContext(null))
			{
				AssertNoExceptionThrown("No exception is expected here", () => fetchStrategy.FetchForLoad());
			}
		}
	}
}
