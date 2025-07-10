
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.Testing
{
	public class JobMessageSubTypeForExportListTest : TestCaseWithFactory
	{
		public void TestListHasRightNumberOfCodesAndDescendsFromBase()
		{
			JobMessageSubTypeForExportList list = new JobMessageSubTypeForExportList();
			AssertEquals("List.Count", 4, list.Count);
		}
	}
}
