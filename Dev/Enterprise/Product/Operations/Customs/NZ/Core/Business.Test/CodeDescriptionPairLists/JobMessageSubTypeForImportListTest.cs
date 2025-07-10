using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.Testing
{
	public class JobMessageSubTypeForImportListTest : TestCaseWithFactory
	{
		public void TestListHasRightNumberOfCodesAndDescendsFromBase()
		{
			JobMessageSubTypeForImportList list = new JobMessageSubTypeForImportList(false);
			AssertEquals("List.Count", 7, list.Count);

			list = new JobMessageSubTypeForImportList(true);
			AssertEquals("List should now include TSW import IPI code", 8, list.Count);
		}
	}
}
