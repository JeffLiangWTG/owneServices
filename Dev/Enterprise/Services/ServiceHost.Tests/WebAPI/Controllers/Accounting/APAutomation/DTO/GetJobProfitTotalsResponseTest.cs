using CargoWise.EntityFramework.Testing;

namespace Enterprise.Services.ServiceHost.Tests
{
	class GetJobProfitTotalsResponseTest : TestCaseWithFactory
	{
		public void TestGetJobProfitTotalsResponseProperties()
		{
			var response = new GetJobProfitTotalsResponse
			{
				Cost = 100m,
				Revenue = 200m,
				Profit = 100m,
				LocalDecimals = 2
			};

			AssertEquals(100m, response.Cost);
			AssertEquals(200m, response.Revenue);
			AssertEquals(100m, response.Profit);
			AssertEquals(2, response.LocalDecimals);
		}

		public void TestGetJobProfitTotalsResponseDefaultValues()
		{
			var response = new GetJobProfitTotalsResponse();
			AssertEquals(0m, response.Cost);
			AssertEquals(0m, response.Revenue);
			AssertEquals(0m, response.Profit);
			AssertEquals(0, response.LocalDecimals);
		}
	}
}
