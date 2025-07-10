using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class Schedule1P8CodeListTest : TestCaseWithFactory
	{
		public void TestItemsCount()
		{
			AssertEquals(2, new Schedule1P8CodeList().Count);
		}
	}
}
