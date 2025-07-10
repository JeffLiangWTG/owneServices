namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework.Testing;

	public class MawbApplicationCodeListTest : TestCaseWithFactory
	{
		public void TestListHasRightNumberOfCodesAndDescendsFromBase()
		{
			var list = new MawbApplicationCodeList();
			AssertEquals("List.Count should have Legacy ECI type & TSW write off type", 2, list.Count);
		}
	}
}
