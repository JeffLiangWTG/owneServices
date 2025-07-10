using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconInterestRateRetrieverTest : TestCase
	{
		public void TestGetRate()
		{
			ReconInterestRateCollection collection = USCustomsDataRegistry.Instance.ReconInterestRates.Value;
			ReconInterestRate firstRate = collection[0];
			AssertEquals(ZDecimal.Zero, ReconInterestRateRetriever.GetRate(ZDateTime.Empty));
			AssertEquals(firstRate.Rate, ReconInterestRateRetriever.GetRate(firstRate.StartDate.AddDays(1)));
			AssertEquals(ZDecimal.Zero, ReconInterestRateRetriever.GetRate(new ZDateTime(1980, 1, 1)));
			AssertEquals(ZDecimal.Zero, ReconInterestRateRetriever.GetRate(new ZDateTime(2020, 1, 1)));
		}
	}
}
