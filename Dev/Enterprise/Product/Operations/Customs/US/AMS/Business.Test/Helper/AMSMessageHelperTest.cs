using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class AMSMessageHelperTest : TestCaseWithFactory
	{
		public void TestBatchSizeAndDelay()
		{
			var helper = new AMSMessageHelper();
			AssertEquals(1000, helper.BatchSize);
			AssertEquals(10, helper.Delay);
		}
	}
}
