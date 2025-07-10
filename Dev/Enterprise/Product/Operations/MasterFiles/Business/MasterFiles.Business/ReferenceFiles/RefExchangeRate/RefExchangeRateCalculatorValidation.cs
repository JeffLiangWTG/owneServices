using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefExchangeRateCalculatorValidation : AutoRefExchangeRateCalculatorValidation
	{
		public RefExchangeRateCalculatorValidation(AutoRefExchangeRateCalculator parent) : base(parent)
		{
		}

		new RefExchangeRateCalculator Parent => (RefExchangeRateCalculator)base.Parent;

		protected override void CheckBaseCurrencyValue()
		{
			base.CheckBaseCurrencyValue();
			MandatoryValidation.CheckNotZero(Parent.BaseCurrencyValueInfo);
			MandatoryValidation.CheckNotNegative(Parent.BaseCurrencyValueInfo);
		}

		protected override void CheckQuoteCurrencyValue()
		{
			base.CheckQuoteCurrencyValue();
			MandatoryValidation.CheckNotZero(Parent.QuoteCurrencyValueInfo);
			MandatoryValidation.CheckNotNegative(Parent.QuoteCurrencyValueInfo);
		}
	}
}
