using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.Business
{
	public class DutyCalculatorStrategy : IDutyCalculatorStrategy
	{
		public DutyCalculatorStrategy(BaseJobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		protected BaseJobDeclaration declaration;

		protected virtual bool CanBeNegative => false;

		protected virtual bool ShouldTruncate => false;

		protected virtual bool ShouldCalculateDuties => false;

		protected virtual int DutyDecimalPlace => declaration.LocalCurrency.ISODecimals;

		public virtual void CalculateDuties()
		{
			if (ShouldCalculateDuties)
			{
				CacheRatesForAllEntries();
				CalculateDutiesForAllEntries();
			}
			CalculateValueForVAT();
		}

		protected void CalculateValueForVAT()
		{
			foreach (CusEntryHeader entryHeader in GetEntryHeadersForCalculation())
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					entryLine.CL_ValueForVAT =
						ValueForVATRounder.Round(entryLine.InvoiceLines.Sum(x => ((BaseJobComInvoiceLine)x).JI_Calc_ValueForVat));
				}
			}
		}

		protected IFeeRounder ValueForVATRounder => valueForVATRounder ??= GetNewValueForVATRounder();
		IFeeRounder valueForVATRounder;

		protected virtual IFeeRounder GetNewValueForVATRounder() => new FeeNoRounder();

		protected void CacheRatesForAllEntries()
		{
			var rateCriteriaSets = GetTariffAndRateCriteriaSetsForAllEntries();
			RateLoader.CacheRatesForMultipleCriteriaSets(rateCriteriaSets);
		}

		IEnumerable<RateLoadTariffCriteriaSet> GetTariffAndRateCriteriaSetsForAllEntries()
		{
			var rateCriteriaSets = new List<RateLoadTariffCriteriaSet>();

			foreach (CusEntryHeader entryHeader in GetEntryHeadersForCalculation())
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					rateCriteriaSets.AddRange(GetTariffAndRateCriteriaSetsForInvoiceLine(entryLine.RandomLine));
				}
			}

			return rateCriteriaSets;
		}

		protected virtual IEnumerable<RateLoadTariffCriteriaSet> GetTariffAndRateCriteriaSetsForInvoiceLine(BaseJobComInvoiceLine randomLine)
		{
			var rateCriteriaSets = new List<RateLoadTariffCriteriaSet>();
			var universalTariff = randomLine.UniversalTariff;

			if (universalTariff != null)
			{
				foreach (var criteria in GetRateSelectionCriteria(randomLine).ToArray())
				{
					rateCriteriaSets.Add(new RateLoadTariffCriteriaSet(universalTariff, criteria));
				}
			}

			return rateCriteriaSets;
		}

		protected virtual IEnumerable<IZZRateSelectionCriteria> GetRateSelectionCriteria(BaseJobComInvoiceLine invLine)
		{
			yield return invLine.DutyRateSelectionCriteria;
		}

		protected virtual void CalculateDutiesForAllEntries()
		{
			foreach (CusEntryHeader entry in GetEntryHeadersForCalculation())
			{
				foreach (CusEntryLine entryLine in entry.MergedLines)
				{
					var universalData = GetEntryLineUniversalData(entryLine);
					CalculateDutiesForEntryLine(entryLine, universalData);
					UpdateEntryLineFees(entryLine, universalData);
				}
			}
		}

		protected virtual EntryLineUniversalRate GetEntryLineUniversalData(CusEntryLine entryLine) => new EntryLineUniversalRate(entryLine);

		void CalculateDutiesForEntryLine(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData)
		{
			var invoiceLine = entryLine.RandomLine;
			var universalTariff = invoiceLine.UniversalTariff;
			if (universalTariff != null)
			{
				var entryLineTariffAndCriteriaSets = GetTariffAndRateCriteriaSetsForInvoiceLine(entryLine.RandomLine);
				var applicableRates = RateLoader.LoadRatesForMultipleCriteriaSets(entryLineTariffAndCriteriaSets);
				CalculateDutiesForRates(entryLine, entryLineUniversalData, applicableRates);
			}
		}

		protected virtual void CalculateDutiesForRates(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, IEnumerable<RateView> applicableRates)
		{
			foreach (var rate in applicableRates)
			{
				CalculateDutiesForRate(entryLine, entryLineUniversalData, rate);
			}
		}

		protected void CalculateDutiesForRate(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, RateView rate)
		{
			entryLineUniversalData.CustomsValueFormula = rate?.CusRateType?.ZZR_CustomsValueFormula ?? ZString.Empty;
			var dutyAmount = Calculate(entryLine, entryLineUniversalData, rate.ZZ2_RateFormula);
			entryLineUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(GetEntryLineFeeType(rate), dutyAmount);
		}

		protected virtual void UpdateEntryLineFees(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData)
		{
			foreach (var countrySpecificValue in entryLineUniversalData.CountrySpecificValueList)
			{
				if (countrySpecificValue.Value != 0)
				{
					entryLine.Fees.AddOrUpdate(countrySpecificValue.Key, countrySpecificValue.Value);
				}
			}
		}

		protected decimal Calculate(CusEntryLine entryLine, IUniversalRateCalcData universalRateCalcData, ZString rateFormula)
						=> Calculate(universalRateCalcData, rateFormula, DutyDecimalPlace, ZString.Empty, 0m);

		protected virtual ZString GetEntryLineFeeType(RateView rate) => rate.RateCode;

		protected decimal Calculate(IUniversalRateCalcData universalRateCalcData, ZString rateFormula, int dutyDecimalPlaces, ZString formulaSpecificQuestion, ZDecimal formulaSpecificValue)
			=> DutyCalculatorHelper.Calculate(universalRateCalcData, rateFormula, dutyDecimalPlaces, formulaSpecificQuestion, formulaSpecificValue, ShouldTruncate, CanBeNegative);

		protected ApplicableRateLoader RateLoader => rateLoader ??= new ApplicableRateLoader(declaration.Factory);
		ApplicableRateLoader rateLoader;

		protected virtual IEnumerable<CusEntryHeader> GetEntryHeadersForCalculation() => declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
	}
}
