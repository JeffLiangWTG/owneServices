using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class CottonFeeCalculator : ILineFeeCalculator
	{
		internal CottonFeeCalculator()
			: this(false, false)
		{
		}

		internal CottonFeeCalculator(bool ignoreTIBExemptionCondition, bool ignoreAMSFeeExempt)
		{
			this.ignoreTIBExemptionCondition = ignoreTIBExemptionCondition;
			this.ignoreAMSFeeExempt = ignoreAMSFeeExempt;
		}

		readonly ZBool ignoreTIBExemptionCondition;
		readonly ZBool ignoreAMSFeeExempt;

		#region ILineFeeCalculator Members

		internal const string ExemptCottonFeeCertificate = "999999999";
		internal const decimal ThresholdCottonFeeAmount = 2.01m;

		public FeeResult CalculateFee(IFeeCalculationDataProvider dataProvider)
		{
			ZDecimal result = ZDecimal.Zero;

			if (!dataProvider.IsCottonFeeExempt(ignoreTIBExemptionCondition, ignoreAMSFeeExempt))
			{
				return new TaxFeeCalculator(Core.Constants.USCustoms.FeeCodes.Cotton, ignoreAMSFeeExempt, ignoreTIBExemptionCondition).CalculateFee(dataProvider);
			}

			return new FeeResult(result);//invoice lines will be merged and the aggregated amount can be > minimum
		}

		#endregion
	}
}
