using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Business;

sealed class DutyCalculatorStrategy(JobDeclaration declaration) : Customs.Business.DutyCalculatorStrategy(declaration)
{
	public override void CalculateDuties()
	{
		if (ShouldCalculateDuties)
		{
			CacheRatesForAllEntries();
			CalculateDutiesForAllEntries();
		}
		CalculateValueForVAT();
		OrderDuties();
	}

	public const decimal ArtsVATBase = 0.2m;
	public const decimal ArtsVATRate = 0.25m;

	protected override bool ShouldCalculateDuties => true;

	protected override void CalculateDutiesForAllEntries()
	{
		Declaration.LoadChildEditableObjects();
		foreach (var mergedLine in MergedLines)
		{
			ClearEntryLineDuties(mergedLine);
			CalculateEntryLineFees(mergedLine);
			CleanupEmptyDuties(mergedLine);
		}
	}

	protected override IFeeRounder GetNewValueForVATRounder() => new IntegerFeeRounder();

	protected override IEnumerable<IZZRateSelectionCriteria> GetRateSelectionCriteria(BaseJobComInvoiceLine invLine)
	{
		if (invLine is not JobComInvoiceLine invoiceLine)
		{
			yield break;
		}

		if (!invoiceLine.IsExport && !invoiceLine.CustomsRateIsOverridden)
		{
			yield return invoiceLine.DutyRateSelectionCriteria;
		}

		yield return invoiceLine.ExciseRateSelectionCriteria;
	}

	void CalculateEntryLineFees(CusEntryLine entryLine)
	{
		if (entryLine.RandomLine is not { } invoiceLine || !invoiceLine.CanCalculateDuty())
		{
			return;
		}

		if (invoiceLine.CustomsRateIsOverridden)
		{
			CalculateDutyForCustomRateFromInvoiceLine(entryLine, invoiceLine);
		}

		var entryLineTariffAndCriteriaSets = GetTariffAndRateCriteriaSetsForInvoiceLine(entryLine.RandomLine);
		var rates = RateLoader.LoadRatesForMultipleCriteriaSets(entryLineTariffAndCriteriaSets);
		rates.WhereNotNull().ForEach(rate => CalculateAndAddFeesForEntryByRate(entryLine, rate));
	}

	static void CalculateDutyForCustomRateFromInvoiceLine(CusEntryLine entryLine, JobComInvoiceLine invoiceLine)
	{
		var totalBaseValue = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(i => i.GetBaseValue());
		var calculatedResult = new DutyCalculationIntermediateResult(invoiceLine.GetOverridenCustomsAmount(), invoiceLine.CustomsRate, totalBaseValue, ZString.Empty)
		{
			MethodOfPayment = invoiceLine.CustomsRateType
		};
		AddNewEntryLineFee(entryLine, Constants.RateTypes.Duty, calculatedResult);
	}

	static void CalculateAndAddFeesForEntryByRate(CusEntryLine entryLine, RateView rateView)
	{
		var dutyCalculator = CreateNewDutyCalculator(entryLine);
		var calculationResults = dutyCalculator.CleanFormulaAndCalculate(rateView);
		var rateCode = rateView.RateCode;
		if (calculationResults is { IntermediateResults: { } intermediateResults })
		{
			intermediateResults.WhereNotNull()
				.ForEach(calculatedFee => AddNewEntryLineFee(entryLine, rateCode, calculatedFee));
		}
	}

	static void AddNewEntryLineFee(CusEntryLine entryLine, ZString rateCode, IDutyCalculationIntermediateResult calculatedFee)
	{
		var entryLineFee = entryLine.Fees.AddNew();
		entryLineFee.CF_ChargeType = rateCode == Constants.RateTypes.Duty ? NOCustomDutyCodeList.Codes.TL1 : rateCode;
		entryLineFee.CF_MethodOfCalculation = calculatedFee.MethodOfCalculation;
		entryLineFee.CF_Rate = calculatedFee.AdjustedRate;
		entryLineFee.CF_BaseValue = calculatedFee.BaseValue;
		var amount = calculatedFee.ParticipatingAmount;
		entryLineFee.CF_ChargeAmount = entryLine.FeeRounder?.Round(amount) ?? amount;
		entryLineFee.CF_MethodOfPayment = calculatedFee.MethodOfPayment;
	}

	static EntryLineDutyCalculator CreateNewDutyCalculator(CusEntryLine entryLine) => new(entryLine);

	void OrderDuties()
	{
		foreach (var mergedLine in MergedLines)
		{
			mergedLine.Fees.Sort<CusEntryLineFee>(DutyComparer.Comparison);
		}
	}

	new void CalculateValueForVAT()
	{
		foreach (var mergedLine in MergedLines)
		{
			if (mergedLine.RandomLine.CanCalculateVAT())
			{
				CalculateVatableAmount(mergedLine);
				CreateVatDuty(mergedLine);
			}
		}
	}

	void CalculateVatableAmount(CusEntryLine mergedLine)
	{
		decimal vatableAmount = 0;
		var isVatable = mergedLine.RandomLine.AppliedTaxAndFee?.ZZF_Value > 0;

		if (Declaration.JE_MessageType == JobMessageTypeList.Codes.Import && isVatable)
		{
			var fees = mergedLine.Fees;
			var duties = fees.Where(fee => !fee.IsExciseDuty).Sum(fee => fee.CF_ChargeAmount);
			var exciseDuties = fees.Where(fee => fee.IsExciseDuty).Sum(fee => fee.CF_ChargeAmount);
			if (mergedLine.IsArt)
			{
				exciseDuties *= ArtsVATBase;
			}
			var feesAmount = duties + exciseDuties;
			vatableAmount = mergedLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_Calc_ValueForVat) + feesAmount;
		}

		mergedLine.CL_ValueForVAT = ValueForVATRounder.Round(vatableAmount);
	}

	void CreateVatDuty(CusEntryLine mergedLine)
	{
		decimal vatableAmount = mergedLine.CL_ValueForVAT;
		string vatCode = mergedLine.VatCode;
		decimal vatRate;
		decimal vatAmount;

		var vatGroup = mergedLine.RandomLine.AppliedTaxAndFee;
		if (vatGroup != null)
		{
			vatRate = vatGroup.ZZF_Value;

			// This is a workaround for MVK rate in the database currently being 5%, instead of 25% out of 20% base.
			if (mergedLine.IsArt)
			{
				vatRate = ArtsVATRate;
			}

			vatAmount = ValueForVATRounder.Round(mergedLine.CL_ValueForVAT * vatRate);

			if (vatAmount > 0)
			{
				var vatFee = mergedLine.Fees.AddNew();
				vatFee.CF_BaseValue = vatableAmount;
				vatFee.CF_ChargeType = vatCode;
				vatFee.CF_MethodOfPayment = RateTypeCodeList.Codes.PercentSign;
				// The rate is saved as a fraction, but we show it as percentage.
				vatFee.CF_Rate = vatRate * 100;
				vatFee.CF_ChargeAmount = vatAmount;
			}
		}
	}

	static void ClearEntryLineDuties(CusEntryLine entryLine)
	{
		entryLine.Fees.RemoveAndDeleteAll();
	}

	void CleanupEmptyDuties(CusEntryLine entryLine)
	{
		entryLine.Fees.RemoveRange(entryLine.Fees.OfType<CusEntryLineFee>().Where(fee => fee.CF_ChargeAmount == 0).ToArray());
	}

	JobDeclaration Declaration => (JobDeclaration)base.declaration;

	CusEntryLine[] MergedLines => mergedEntryLines ??= Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().SelectMany(header => header.MergedLines).ToArray();
	CusEntryLine[] mergedEntryLines;
}
