using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class CW1ChargeViewModel : ChargeViewModel
	{
		public CW1ChargeViewModel()
		{
		}

		public CW1ChargeViewModel(AutoRateInfo autoRateInfo, RateSelectorContext context)
			: base(context?.Factory, context?.CurrencyConverter)
		{
			AutoRateInfo = Argument.NotNull(autoRateInfo, nameof(autoRateInfo));
			Argument.NotNull(context, nameof(context));

			// Populate ViewModel
			Currency = autoRateInfo.Currency;
			Amount = autoRateInfo.Amount;
			ChargeCode = autoRateInfo.ChargeCode.AC_Code;
			AccChargeCode = autoRateInfo.ChargeCode;
			ChargeCodeDescription = autoRateInfo.ChargeCode.AC_DescMultilingual;
			CalculationDescription = autoRateInfo?.CalculationDescription;

			if (!autoRateInfo.HasResult)
			{
				ChargeCodeErrorLevel = ErrorLevel.Warning;
				ChargeCodeError = autoRateInfo.CalculationDescription;

				context.Logger.Log(LogType.Warning, ChargeCodeError);
			}

			PopulateLocalAmounts(ChargeCode, Amount, Currency);
		}
	}
}
