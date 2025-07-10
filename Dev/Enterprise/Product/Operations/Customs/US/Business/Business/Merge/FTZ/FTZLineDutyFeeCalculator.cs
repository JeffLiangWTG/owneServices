namespace Enterprise.Customs.US.Business
{
	public class FTZLineDutyFeeCalculator : ILineDutyFeeCalculator
	{
		public FTZLineDutyFeeCalculator(IDutyDataLineHeaderProvider declaration)
		{
			this.declaration = declaration;
		}

		readonly IDutyDataLineHeaderProvider declaration;

		public void Calculate(IEntryLineOrInvoiceLineDutyData line, FeeCalculator feeCalculator)
		{
			foreach (IFeeCalculationDataProvider dataProvider in line.FeeDataProviders)
			{
				feeCalculator.Calculate(dataProvider, feeCode => declaration.IsCustomsChargeRelevantForDecType(feeCode));
			}
		}

		public void CalculateNormalDuty(IEntryLineOrInvoiceLineDutyData line) { }
	}
}
