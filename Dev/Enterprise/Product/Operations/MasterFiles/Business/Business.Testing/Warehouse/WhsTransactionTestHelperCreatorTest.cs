using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WhsTransactionTestHelperCreatorTest : TestCaseWithFactory
	{
		public void TestGetNewHelper()
		{
			AssertNotNull(WhsTransactionTestHelperCreator.GetNewHelper(Factory));
		}
	}
}
