namespace Enterprise.Customs.US.Business
{
	interface ILineFeeCalculator
	{
		FeeResult CalculateFee(IFeeCalculationDataProvider invoiceLine);
	}
}
