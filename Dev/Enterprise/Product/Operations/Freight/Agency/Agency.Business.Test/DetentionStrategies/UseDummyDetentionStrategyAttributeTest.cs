using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	class UseDummyDetentionStrategyAttributeTest : TestCase
	{
		public void TestSetUpAndTearDown()
		{
			var attribute = new UseDummyDetentionStrategyAttribute();
			AssertEquals("Precondition.", false, DetentionStrategy.useDummyDetentionStrategy_ForTesting.Value);

			attribute.SetUp(null);
			AssertEquals(true, DetentionStrategy.useDummyDetentionStrategy_ForTesting.Value);
			AssertType(typeof(DummyDetentionStrategy), DetentionStrategy.New("XXX"));

			attribute.TearDown(null);
			AssertEquals(false, DetentionStrategy.useDummyDetentionStrategy_ForTesting.Value);
			AssertType(typeof(DetentionNullStrategy), DetentionStrategy.New("XXX"));
		}

		[UseDummyDetentionStrategy]
		public void TestUseDummy()
		{
			AssertEquals(true, DetentionStrategy.useDummyDetentionStrategy_ForTesting.Value);
			AssertType(typeof(DummyDetentionStrategy), DetentionStrategy.New("XXX"));
		}

		public void TestDontUseDummy()
		{
			AssertEquals(false, DetentionStrategy.useDummyDetentionStrategy_ForTesting.Value);
			AssertType(typeof(DetentionNullStrategy), DetentionStrategy.New("XXX"));
		}
	}
}
