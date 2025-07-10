using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AutoBillingResultTest : TestCaseWithFactory
	{
		public void TestWasSuccessful()
		{
			AutoBillingResult result = new AutoBillingResult();
			AssertEquals(false, result.WasSuccessful);

			result.WasSuccessful = true;
			AssertEquals(true, result.WasSuccessful);
		}

		public void TestSetResult()
		{
			AutoBillingResult result = new AutoBillingResult();

			AutoBillingResult passed = new AutoBillingResult();
			passed.WasSuccessful = true;
			passed.Message = "Blah";

			AssertEquals(false, result.WasSuccessful);

			result.SetResult(passed);
			AssertEquals(true, result.WasSuccessful);
			AssertEquals("Blah", result.Message);

			passed.WasSuccessful = false;
			passed.Message = "More Blah";

			result.SetResult(passed);
			AssertEquals(false, result.WasSuccessful);
			AssertEquals("Blah\r\nMore Blah", result.Message);
		}
	}
}
