using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;

namespace Enterprise.Customs.US.Business
{
	public static class ReconInterestRateRetriever
	{
		public static ZDecimal GetRate(ZDateTime date)
		{
			foreach (ReconInterestRate interestRate in USCustomsDataRegistry.Instance.ReconInterestRates.Value)
			{
				if (interestRate.IsWithinDateRate(date))
				{
					return interestRate.Rate;
				}
			}
			return ZDecimal.Zero;
		}
	}
}
