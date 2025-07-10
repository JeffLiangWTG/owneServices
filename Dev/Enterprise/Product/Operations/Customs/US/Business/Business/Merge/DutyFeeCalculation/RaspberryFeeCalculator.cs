using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class RaspberryFeeCalculator : ILineFeeCalculator
	{
		internal RaspberryFeeCalculator(bool ignoreTIBExemptionCondition)
		{
			this.ignoreTIBExemptionCondition = ignoreTIBExemptionCondition;
		}

		readonly ZBool ignoreTIBExemptionCondition;

		#region ILineFeeCalculator Members

		public FeeResult CalculateFee(IFeeCalculationDataProvider dataProvider)
		{
			var result = ZDecimal.Zero;
			var noneCustomsValueAmount = ZDecimal.Zero;
			var percentOfRate = ZDecimal.Zero;
			if (!dataProvider.IsRaspberryFeeExempt())
			{
				var feeResult = new TaxFeeCalculator(Core.Constants.USCustoms.FeeCodes.Raspberry, false, ignoreTIBExemptionCondition).CalculateFee(dataProvider);
				result = feeResult.Amount;
				noneCustomsValueAmount = feeResult.NoneCustomsValueAmount;
				percentOfRate = feeResult.PercentOfRate;
			}

			return new FeeResult(result, percentOfRate, noneCustomsValueAmount);
		}

		#endregion
	}
}
