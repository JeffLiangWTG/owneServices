using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MasterFilesListProviderTest : TestCaseWithFactory
	{
		public void TestMasterFilesLists()
		{
			MasterFilesListProvider listProvider = new MasterFilesListProvider();
			AssertEquals("Wrong Type of WhsPackingSlipOrderByList", typeof(WhsPackingSlipOrderByList), listProvider.PackingSlipOrderByList().GetType());
			AssertEquals("Wrong Type of FailureReasons", typeof(FailureReasonList), listProvider.FailureReasons().GetType());
		}
	}
}
