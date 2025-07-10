using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Declaration;

sealed class GuaranteedChargesCalculator : IExtraFeeCalculator
{
	readonly CusEntryLineFee entryLineFee;
	readonly ZDecimal rate;

	public GuaranteedChargesCalculator(CusEntryLineFee entryLineFee, ZDecimal rate)
	{
		this.entryLineFee = Argument.NotNull(entryLineFee, nameof(entryLineFee));
		this.rate = rate;
	}

	public ZString RateCode => TaxTypeList.Codes.GuaranteedCharges;

	public IEnumerable<IDutyCalculationIntermediateResult> CalculateExtraFees()
	{
		var baseValue = entryLineFee.CF_ChargeAmount;
		var amount = baseValue * rate;
		var calculationResult = new DutyCalculationIntermediateResult(amount, rate, baseValue, Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage)
		{
			MethodOfPayment = entryLineFee.CF_MethodOfPayment,
		};

		yield return calculationResult;
	}
}
