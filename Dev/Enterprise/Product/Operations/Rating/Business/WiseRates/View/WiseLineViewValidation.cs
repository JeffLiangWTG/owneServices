using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class WiseLineViewValidation : WiseRatesViewsValidation<WiseLineView>
	{
		public WiseLineViewValidation(WiseLineView parent) : base(parent)
		{
		}

		protected override void ValidateAllCore()
		{
			ValidateCalculatedProperty(parent.TL_ACInfo);
			ValidateCalculatedProperty(parent.TL_RX_NKCurrencyInfo);
			ValidateCalculatedProperty(parent.TL_RateCalculatorInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for UI Components")]
		void CheckTL_AC()
		{
			if (parent.UnderlyingWiseLine.Errors.TryGetValue(RateLinesSchema.TL_AC, out var error))
			{
				parent.TL_ACInfo.AddError(error);
				AddUnmappedForeignCode(parent.UnderlyingWiseLine?.WiseCharge?.ChargeCode, Constants.OrgPatternMatchOverrideRelationships.ChargeCodes);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for UI Components")]
		void CheckTL_RX_NKCurrency()
		{
			if (parent.UnderlyingWiseLine.Errors.TryGetValue(RateLinesSchema.TL_RX_NKCurrency, out var error))
			{
				parent.TL_RX_NKCurrencyInfo.AddError(error);
				AddUnmappedForeignCode(parent.UnderlyingWiseLine?.WiseCharge?.Currency, Constants.OrgPatternMatchOverrideRelationships.Currency);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for UI Components")]
		void CheckTL_RateCalculator()
		{
			if (parent.UnderlyingWiseLine.Errors.TryGetValue(RateLinesSchema.TL_RateCalculator, out var error))
			{
				parent.TL_RateCalculatorInfo.AddError(error);
			}
		}
	}
}
