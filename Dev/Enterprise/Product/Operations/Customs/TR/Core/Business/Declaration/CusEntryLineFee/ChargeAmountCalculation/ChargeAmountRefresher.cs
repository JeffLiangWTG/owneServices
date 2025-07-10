using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ChargeAmountRefresher : EU.Business.Declaration.ChargeAmountRefresher
	{
		public ChargeAmountRefresher(CusEntryLineFee lineFee) : base(lineFee)
		{
		}

		protected override bool ShouldRefreshChargeAmount => true;

		protected override IChargeAmountCalculator GetNewChargeAmountCalculatorCore()
		{
			var defaultCalculator = new ChargeAmountCalculator(LineFee);
			return new PercentageChargeAmountCalculator(defaultCalculator);
		}
	}
}
