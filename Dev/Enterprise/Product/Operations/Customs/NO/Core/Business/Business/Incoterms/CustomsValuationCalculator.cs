using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business;

sealed class CustomsValuationCalculator(IChargeApportionee chargeApportionee) : Customs.Business.CustomsValuationCalculator(chargeApportionee)
{
	public override ZDecimal GetAmountToAddToITOTForStatistical(RefCurrency currency)
	{
		var charges = ChargeApportionee.Charges.Concat(ChargeApportionee.ApportionedCharges);
		var currencyConverter = ChargeApportionee.CurrencyConverter;
		return charges
			.Where(IsVGEOrForStatisticalValue)
			.Sum(charge => currencyConverter.ConvertExact(charge.Money, currency).Amount);
	}

	static bool IsVGEOrForStatisticalValue(JobComInvCharge charge) => (charge.J7_ChargeType == NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported) || charge.J7_IsStatisticalValueApplicable;
}
