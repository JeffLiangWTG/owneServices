using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class DutyTaxFeeQuantityWrapper : IDutyTaxFeeQuantity
	{
		public DutyTaxFeeQuantityWrapper(ZString dutyUnitCode, ZDecimal taxRateNumeric)
		{
			this.dutyUnitCode = dutyUnitCode;
			this.taxRateNumeric = taxRateNumeric;
		}

		readonly ZString dutyUnitCode;
		readonly ZDecimal taxRateNumeric;

		ZString IDutyTaxFeeQuantity.DutyUnitCode => dutyUnitCode;

		ZDecimal IDutyTaxFeeQuantity.TaxRateNumeric => taxRateNumeric;

		ZDecimal IDutyTaxFeeQuantity.PercentageNumeric => ZDecimal.Zero;
	}
}
