namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class DocketLineUNDGValidationResultTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			var overLimitMessage = "xxx over limit";
			var overThresholdMessage = "xxx over threshold";

			var docketLineValidationResult = new WarehouseUNDGValidationResult(overLimitMessage, overThresholdMessage);
			CombineAssertions(() =>
			{
				AssertEquals(overLimitMessage, docketLineValidationResult.OverLimitMessage);
				AssertEquals(overThresholdMessage, docketLineValidationResult.OverThresholdMessage);
			});
		}
	}
}
