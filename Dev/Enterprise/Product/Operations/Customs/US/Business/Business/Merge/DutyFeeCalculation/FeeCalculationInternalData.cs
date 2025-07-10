namespace Enterprise.Customs.US.Business
{
	public class FeeCalculationInternalData
	{
		public FeeCalculationInternalData()
		{
		}
		public FeeCalculationInternalData(decimal noneCustomsValueAmount, decimal percentOfRate)
		{
			NoneCustomsValueAmount = noneCustomsValueAmount;
			PercentOfRate = percentOfRate;
		}

		public decimal NoneCustomsValueAmount { get; set; }
		public decimal PercentOfRate { get; set; }
	}
}
