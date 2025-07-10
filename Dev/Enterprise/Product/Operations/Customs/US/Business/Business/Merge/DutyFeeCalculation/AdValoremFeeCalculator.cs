using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class AdValoremFeeCalculator : ILineFeeCalculator
	{
		public AdValoremFeeCalculator(ZString feeCode, ZBool ignoreTIBExemptionCondition)
		{
			this.feeCode = feeCode;
			this.ignoreTIBExemptionCondition = ignoreTIBExemptionCondition;
		}
		readonly ZString feeCode;
		readonly ZBool ignoreTIBExemptionCondition;

		FeeResult ILineFeeCalculator.CalculateFee(IFeeCalculationDataProvider invoiceLine)
		{
			var customsValue = invoiceLine.CustomsValue;
			if (invoiceLine.ParentTariffLine != null)
			{
				customsValue += invoiceLine.ParentTariffLine.CustomsValue;
			}

			var dataProvider = new FeeCalculationDataProvider(invoiceLine, customsValue);
			return new TaxFeeCalculator(feeCode, ignoreTIBExemptionCondition).CalculateFee(dataProvider);
		}
	}
}
