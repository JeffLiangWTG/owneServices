using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class DutyTaxFeeAmountWrapper : IDutyTaxFeeAmount
	{
		public DutyTaxFeeAmountWrapper(ZDecimal rate)
		{
			this.rate = rate;
		}

		readonly ZDecimal rate;

		ZDecimal IDutyTaxFeeAmount.TaxRateNumeric => rate;

		ZDecimal IDutyTaxFeeAmount.PercentageNumeric => ZDecimal.Zero;
	}
}
