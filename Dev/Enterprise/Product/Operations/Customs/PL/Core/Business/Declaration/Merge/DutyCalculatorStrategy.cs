using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.PL.Business.Declaration;

public class DutyCalculatorStrategy : EU.Business.Declaration.DutyCalculatorStrategy
{
	public DutyCalculatorStrategy(JobDeclaration declaration) : base(declaration)
	{ }

	protected override IUniversalDutyCalculator GetNewEntryLineDutyCalculator(EU.Business.Declaration.CusEntryLine entryLine)
		=> new EntryLineDutyCalculator((CusEntryLine)entryLine, RateCalculationVisitorMode);

	protected override void PopulateEntryLineFees(EU.Business.Declaration.CusEntryLine entryLine, IEnumerable<RateView> rates)
		=> base.PopulateEntryLineFees(entryLine, rates.OrderBy(x => x.RateCode == TaxTypeList.Codes.ExciseTax));

	protected override void SetBaseValue(EU.Business.Declaration.CusEntryLine entryLine, EU.Business.Declaration.CusEntryLineFee entryLineFee, IDutyCalculationIntermediateResult intermediateResult)
		=> entryLineFee.CF_BaseValue = RoundBaseValueIfNeeded(entryLineFee, GetBaseValue(entryLine, entryLineFee, intermediateResult));

	protected override void SetChargeAmount(EU.Business.Declaration.CusEntryLine entryLine, EU.Business.Declaration.CusEntryLineFee entryLineFee, IDutyCalculationIntermediateResult intermediateResult)
	{
		entryLineFee.CF_ChargeAmount = MustBeConvertedToLocalCurrency(entryLineFee)
			? GetAmountInLocalCurrency(entryLine, intermediateResult.ParticipatingAmount)
			: intermediateResult.ParticipatingAmount;
	}

	protected override void SetUserEditableValues(EU.Business.Declaration.CusEntryLineFee entryLineFee, IDutyCalculationIntermediateResult intermediateResult)
	{
		base.SetUserEditableValues(entryLineFee, intermediateResult);
		if (ShouldUpdateMethodOfPaymentForAddedFee(entryLineFee))
		{
			entryLineFee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.L;
		}
	}

	protected override bool ShouldCalculateDutiesForEntryLine(EU.Business.Declaration.CusEntryLine entryLine)
		=> entryLine.RandomLine is JobComInvoiceLine randomLine
			&& (DutyCalculationHelper.HasCUDExchangeRateOrSpecifiedDateIsInvalid(entryLine.Factory, randomLine.EffectiveAssessmentDate) || !randomLine.IsImport);

	protected override IFeeRounder GetNewValueForVATRounder() => new IntegerCusEntryLineFeeRounder();

	protected override IEnumerable<IExtraFeeCalculator> GetNonVatableExtraFeeCalculatorCollectionCore(EU.Business.Declaration.CusEntryLine entryLine)
	{
		var plEntryLine = (CusEntryLine)entryLine;

		var result = base.GetNonVatableExtraFeeCalculatorCollectionCore(entryLine);

		return entryLine.RandomLine?.AppliedTaxAndFee is RefCusTaxOrFee appliedTaxAndFee
			? result.Concat(plEntryLine.Fees.Where(x => x.CF_ChargeType == TaxTypeList.Codes.ProvisionalAntidumpingDuties)
				.Select(x => new GuaranteedChargesCalculator(x, appliedTaxAndFee.ZZF_Value)))
			: result;
	}

	protected override void AddNewEntryLineFee(EU.Business.Declaration.CusEntryLine entryLine, ZString rateCode, ZString overrideReasonCode, IDutyCalculationIntermediateResult calculatedFee, RateView rateForCalculation = null)
	{
		if (IsAgricultureDuty(calculatedFee))
		{
			rateCode = EU.Business.UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge;
		}

		base.AddNewEntryLineFee(entryLine, rateCode, overrideReasonCode, calculatedFee, rateForCalculation);
	}

	ZDecimal GetBaseValue(EU.Business.Declaration.CusEntryLine entryLine, EU.Business.Declaration.CusEntryLineFee entryLineFee, IDutyCalculationIntermediateResult intermediateResult)
	{
		if (entryLineFee.CF_ChargeType.EqualsIgnoringCase(TaxTypeList.Codes.ExciseTax))
		{
			var customsDutiesFeeBaseValueOnlyA00 = entryLine.Fees.Cast<CusEntryLineFee>()
				.FirstOrDefault(x => x.CF_ChargeType.EqualsIgnoringCase(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts))?.CF_BaseValue ?? 0.0m;

			var customsDutiesFeeTotalAmountAllA = entryLine.Fees.Cast<CusEntryLineFee>()
				.Where(x => x.CF_ChargeType.StartsWith("A", StringComparison.InvariantCultureIgnoreCase)
							&& x.CF_MethodOfPayment != PLMethodOfPaymentList.Codes.L && x.CF_MethodOfPayment != PLMethodOfPaymentList.Codes.Z)
				.Sum(x => x.CF_ChargeAmount);

			return customsDutiesFeeBaseValueOnlyA00 + customsDutiesFeeTotalAmountAllA;
		}

		return MustBeConvertedToLocalCurrency(entryLineFee)
			? GetAmountInLocalCurrency(entryLine, intermediateResult.BaseValue)
			: intermediateResult.BaseValue;
	}

	static bool ShouldUpdateMethodOfPaymentForAddedFee(EU.Business.Declaration.CusEntryLineFee entryLineFee)
	{
		var methodOfPayment = entryLineFee.CF_MethodOfPayment;
		return (methodOfPayment == PLMethodOfPaymentList.Codes.R || methodOfPayment == PLMethodOfPaymentList.Codes.E)
				&& entryLineFee.CF_ChargeAmount.IsEmpty;
	}

	static ZDecimal GetAmountInLocalCurrency(EU.Business.Declaration.CusEntryLine entryLine, ZDecimal amount)
	{
		var plEntryLine = (CusEntryLine)entryLine;
		return plEntryLine.CUDCurrencyConverter.ConvertExact(new Money(amount, plEntryLine.EURCurrency), plEntryLine.LocalCurrency).Amount;
	}

	static ZDecimal RoundBaseValueIfNeeded(EU.Business.Declaration.CusEntryLineFee entryLineFee, ZDecimal value)
	{
		var chargeType = entryLineFee.CF_ChargeType;
		return chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts
				|| chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge
				|| chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty
				|| chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty
				|| chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty
				|| chargeType == TaxTypeList.Codes.ExciseTax
			? value.Round(0)
			: value;
	}

	static bool MustBeConvertedToLocalCurrency(EU.Business.Declaration.CusEntryLineFee entryLineFee)
	{
		var chargeType = entryLineFee.CF_ChargeType;
		return chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts
				|| chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge
				|| chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty
				|| chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty
				|| chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty;
	}

	static bool IsAgricultureDuty(IDutyCalculationIntermediateResult fee)
	{
		var originalMeursingExpression = fee?.OriginalMeursingExpression ?? ZString.Empty;

		return !originalMeursingExpression.IsEmpty
				&& AgricultureRateCodes.Any(x => originalMeursingExpression.StartsWith(x));
	}

	[ThreadSafe]
	static readonly string[] AgricultureRateCodes = {
		EU.Business.UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent,
		EU.Business.UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnSugarContents,
		EU.Business.UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnFlourContents,
	};
}
