using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.Testing
{
	public class JobMessageSubTypeForExciseListTest : TestCaseWithFactory
	{
		public void TestListHasRightNumberOfCodesAndDescendsFromBase()
		{
			JobMessageSubTypeForExciseList list = new JobMessageSubTypeForExciseList();
			AssertEquals("List.Count", 1, list.Count);
		}
	}
}
