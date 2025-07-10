using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsAreaAndPickMethodHelperTest : TestCase
	{
		public void TestAnyCode()
		{
			AssertEquals("ANY", WhsAreaAndPickMethodHelper.AnyCode);
		}

		public void TestIsAnyCode()
		{
			AssertEquals(false, WhsAreaAndPickMethodHelper.IsAnyCode(""));
			AssertEquals(false, WhsAreaAndPickMethodHelper.IsAnyCode(null));
			AssertEquals(false, WhsAreaAndPickMethodHelper.IsAnyCode("many"));
			AssertEquals(true, WhsAreaAndPickMethodHelper.IsAnyCode("any"));
			AssertEquals(true, WhsAreaAndPickMethodHelper.IsAnyCode("ANY"));
		}
	}
}
