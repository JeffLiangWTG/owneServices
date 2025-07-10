namespace Enterprise.Customs.US.Business
{
	public interface ILineDutyFeeCalculator
	{
		void Calculate(IEntryLineOrInvoiceLineDutyData line, FeeCalculator feeCalculator);

		void CalculateNormalDuty(IEntryLineOrInvoiceLineDutyData line);
	}

	public interface IDutyFeeCalculator
	{
		void Calculate();
	}
}
