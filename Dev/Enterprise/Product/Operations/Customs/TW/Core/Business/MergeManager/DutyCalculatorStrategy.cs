using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.Business
{
	public class DutyCalculatorStrategy : Customs.Business.DutyCalculatorStrategy
	{
		Dictionary<CusEntryLine, HasChangesHunter> hasChangesHunters;

		public DutyCalculatorStrategy(JobDeclaration declaration) : base(declaration)
		{
			hasChangesHunters = new Dictionary<CusEntryLine, HasChangesHunter>();
		}

		protected new JobDeclaration declaration => (JobDeclaration)base.declaration;

		protected override int DutyDecimalPlace => 3;

		protected override bool CanBeNegative => true;

		static bool EntryLineHasFeeForThisCode(CusEntryLine entryLine, string rateCode)
		{
			return entryLine.Fees.HasOverriddenFeeOfGivenCode(rateCode);
		}

		static void ClearEntryLineSystemCalculatedFees(CusEntryLine entryLine)
		{
			foreach (var systemLineFee in entryLine.Fees.Cast<CusEntryLineFee>().Where(f => f.CF_RateOverrideReasonCode.IsEmpty).ToList())
			{
				entryLine.Fees.RemoveAndDelete(systemLineFee);
			}
		}

		internal bool AnyEntryLineHasChangesSinceLastMark => declaration.EntryHeader?.AllEntryLines.Cast<CusEntryLine>().Any(EntryLineHasChangesSinceLastMark) ?? false;

		bool EntryLineHasChangesSinceLastMark(CusEntryLine entryLine)
		{
			if (!hasChangesHunters.TryGetValue(entryLine, out var hunter))
			{
				hunter = new HasChangesHunter(entryLine);
				hasChangesHunters.Add(entryLine, hunter);
			}

			return hunter.HasChangesSinceLastMark;
		}

		public override void CalculateDuties()
		{
			var dutyCalculationEntryLineTempResultsCache = new Dictionary<CusEntryLine, List<DutyCalculationIntermediateResult>>();
			var tpfThreshold = GetRefCusTaxOrFee(RefCusTaxOrFeeCodes.TPF, declaration.DeclarationDate)?.ZZF_Threshold ?? ZDecimal.Zero;
			foreach (CusEntryHeader entryHeader in declaration.ActiveEntryHeaders)
			{
				entryHeader.Charges.RemoveAndDeleteAll();
				var isImport = entryHeader.IsImport;

				foreach (var entryLine in entryHeader.MergedLines)
				{
					entryLine.CL_ValueForVAT = ZDecimal.Zero;
					var invoiceLine = entryLine.RandomLine;
					var isROR = invoiceLine.IsROR;
					var dutyCalculationEntryLineTempResults = new List<DutyCalculationIntermediateResult>();
					var entryLineUniversalData = new EntryLineUniversalRate(entryLine);
					(EntryLineUniversalRate cashPayment, EntryLineUniversalRate nonCashPayment, EntryLineUniversalRate allPayment)? entryLineUniversalDataForROR = null;
					if (isROR)
					{
						var entryLineUniversalDataForRorCashPayment = new EntryLineUniversalRate(entryLine);
						var entryLineUniversalDataForRorNonCashPayment = new EntryLineUniversalRate(entryLine, entryLine.CL_CustomsValue - entryLineUniversalDataForRorCashPayment.ValueForDuty);
						var entryLineUniversalDataForRorAllPayment = new EntryLineUniversalRate(entryLine, entryLine.CL_CustomsValue);
						entryLineUniversalDataForROR = (entryLineUniversalDataForRorCashPayment, entryLineUniversalDataForRorNonCashPayment, entryLineUniversalDataForRorAllPayment);
					}

					ClearEntryLineSystemCalculatedFees(entryLine);
					SetDutyCalculationIntermediateResultFromEntryLineFee(entryLine, entryLineUniversalData, dutyCalculationEntryLineTempResults);

					if (isImport)
					{
						CalculateImportDuties(entryLine, entryLineUniversalData, entryLineUniversalDataForROR, invoiceLine, dutyCalculationEntryLineTempResults);
						CalculateSpecialDuties(invoiceLine, dutyCalculationEntryLineTempResults, entryLineUniversalData);

						if (invoiceLine.Taxes.Cast<JobComInvoiceLineTax>().Any(x => x.JLT_Type != Constants.UniversalReferenceConstants.RefCusRateTypes.SS))
						{
							CalculateInvoiceLineAdditionalTax(entryLine, entryLineUniversalData, entryLineUniversalDataForROR, invoiceLine, RefCusRateTypes.CommodityTaxes, dutyCalculationEntryLineTempResults, new ZString[] { RefCusRateCodes.CTA, RefCusRateCodes.CTS });
						}

						var vatPymntMthd = invoiceLine.JI_VatPymntMthd;
						if (!vatPymntMthd.IsEmpty || isROR)
						{
							var entryLineUniversalDataForVAT = (isROR && vatPymntMthd == DutyTaxPaymentMethodList.Codes.RorPayment) ? entryLineUniversalDataForROR.Value.cashPayment : (isROR ? entryLineUniversalDataForROR.Value.allPayment : entryLineUniversalData);
							CalculateVAT(entryLine, entryLineUniversalDataForVAT, entryLineUniversalDataForROR?.nonCashPayment, isROR, invoiceLine, RefCusTaxOrFeeCodes.VAT, entryLineUniversalData.DateOfValuation, vatPymntMthd, dutyCalculationEntryLineTempResults);
						}

						if (invoiceLine.Taxes.Cast<JobComInvoiceLineTax>().Any(x => x.JLT_Type == Constants.UniversalReferenceConstants.RefCusRateTypes.SS))
						{
							CalculateInvoiceLineAdditionalTax(entryLine, entryLineUniversalData, entryLineUniversalDataForROR, invoiceLine, RefCusRateTypes.SpecialServiceAndGoods, dutyCalculationEntryLineTempResults);
						}
					}
					CalculateTPF(entryHeader, entryLine, entryLineUniversalData, entryLineUniversalDataForROR, tpfThreshold, invoiceLine.JI_TpfPymntMthd, entryLineUniversalData.DateOfValuation, dutyCalculationEntryLineTempResults);
					dutyCalculationEntryLineTempResultsCache[entryLine] = dutyCalculationEntryLineTempResults;
				}
				ExemptDutyCalculation(entryHeader, dutyCalculationEntryLineTempResultsCache);
				var aggregateCharges = new List<DutyCalculationIntermediateResult>();
				CalculateFeeDelayedDeclaration(entryHeader, aggregateCharges);
				ClearTpfUnderMinimumThreshold(tpfThreshold, dutyCalculationEntryLineTempResultsCache);
				AddDutyChargesToEntryHeader(entryHeader, aggregateCharges.GetGroupedFees().ToList());
				AddFeesToEntryLines(dutyCalculationEntryLineTempResultsCache);
				AddAllEntryLinesToHasChangesHunters(entryHeader);
			}
		}

		internal List<DutyCalculationIntermediateResult> GetDutyCalculationResults(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, IEnumerable<RateView> rates)
		{
			var list = new List<DutyCalculationIntermediateResult>();
			var highestDutyForEntryLine = new DutyCalculationIntermediateResult();
			foreach (var rate in rates)
			{
				entryLineUniversalData.CustomsValueFormula = rate?.CusRateType?.ZZR_CustomsValueFormula ?? ZString.Empty;
				var currentCalculatedDutyAmount = CalculateDutyAmount(entryLine, entryLineUniversalData, rate.ZZ2_RateFormula, rate.RateCode, rate.ZZ2_RateFormulaDerivedFrom);
				list.Add(currentCalculatedDutyAmount);
				if (currentCalculatedDutyAmount.Amount > highestDutyForEntryLine.Amount || highestDutyForEntryLine.RateCode.IsEmpty)
				{
					highestDutyForEntryLine.Amount = ZDecimal.Zero;
					highestDutyForEntryLine = currentCalculatedDutyAmount;
				}
				else
				{
					currentCalculatedDutyAmount.Amount = ZDecimal.Zero;
				}
			}
			return list;
		}

		public RateView GetRateViewOfHighestDutyCalculationResult(IUniversalRateCalcData universalData, IEnumerable<RateView> rates)
		{
			RateView result = null;
			if (rates.Any())
			{
				result = rates.First();
				var highestDutyAmount = CalculateDutyAmount(null, universalData, result.ZZ2_RateFormula, result.RateCode, result.ZZ2_RateFormulaDerivedFrom).Amount;

				foreach (var rate in rates.Skip(1))
				{
					var currentCalculatedDutyAmount = CalculateDutyAmount(null, universalData, rate.ZZ2_RateFormula, rate.RateCode, rate.ZZ2_RateFormulaDerivedFrom);
					if (currentCalculatedDutyAmount.Amount > highestDutyAmount || (currentCalculatedDutyAmount.Amount == highestDutyAmount && currentCalculatedDutyAmount.RateCode == RefCusRateCodes.DTA))
					{
						highestDutyAmount = currentCalculatedDutyAmount.Amount;
						result = rate;
					}
				}
			}
			return result;
		}

		(decimal baseValue, decimal rate, string methodOfCalculation) CalculateSimpleRateFormula(string rateFormula, string rateFormulaDerivedFrom, IUniversalRateCalcData entryLineUniversalData)
		{
			var unitAndRate = rateFormulaDerivedFrom.Split(new char[] { '/' });
			var rate = unitAndRate.Length > 0 ? ZDecimal.ParseSafe(unitAndRate[0], ZDecimal.Zero) : ZDecimal.Zero;
			var methodOfCalculation = unitAndRate.Length == 2 ? unitAndRate[1] : MethodOfCalculation.Percentage;
			decimal baseValue;
			if (methodOfCalculation == MethodOfCalculation.Percentage)
			{
				baseValue = entryLineUniversalData.ValueForDuty;
			}
			else
			{
				entryLineUniversalData.UnitOfMeasureValueList.TryGetValue(methodOfCalculation, out baseValue);
			}

			if (rateFormula.EndsWith(TWSpecificRateParameterList.Codes.AlcoholPercentage)
				&& entryLineUniversalData.CountrySpecificValueList.TryGetValue(TWSpecificRateParameterList.Codes.AlcoholPercentage, out var result))
			{
				rate *= result;
			}

			return (baseValue, rate, methodOfCalculation);
		}

		(decimal baseValue, decimal rate, string methodOfCalculation) CalculateIFRateFormula(decimal amount, string rateFormula, IUniversalRateCalcData entryLineUniversalData)
		{
			rateFormula = rateFormula.Trim();
			var result = (default(decimal), default(decimal), default(string));
			if (amount == decimal.Zero)
			{
				result = (entryLineUniversalData.ValueForDuty, decimal.Zero, MethodOfCalculation.Percentage);
			}
			else if (rateFormula.Length > 4)
			{
				var rateFormulaParts = rateFormula.Substring(3, rateFormula.Length - 4).Split(new char[] { ',' });
				if (rateFormulaParts.Length == 3)
				{
					foreach (var formula in rateFormulaParts.Skip(1))
					{
						var trimmedFormula = formula.Trim();
						var formulaDerivedFrom = trimmedFormula.Split(new char[] { '*' })[0].Trim();
						var tempResult = CalculateSimpleRateFormula(trimmedFormula, formulaDerivedFrom, entryLineUniversalData);
						if (tempResult.rate * tempResult.baseValue == amount)
						{
							result = tempResult;
							break;
						}
					}
				}
			}
			return result;
		}

		internal DutyCalculationIntermediateResult CalculateDutyAmount(CusEntryLine entryLine, IUniversalRateCalcData entryLineUniversalData, ZString rateFormula, ZString rateCode, ZString rateFormulaDerivedFrom)
		{
			var amount = Calculate(entryLine, entryLineUniversalData, rateFormula);

			decimal baseValue, rate;
			string methodOfCalculation;
			if (rateFormula.TrimStart().ToLower().StartsWith((NoResString)"if("))
			{
				(baseValue, rate, methodOfCalculation) = CalculateIFRateFormula(amount, rateFormula, entryLineUniversalData);
			}
			else
			{
				(baseValue, rate, methodOfCalculation) = CalculateSimpleRateFormula(rateFormula, rateFormulaDerivedFrom, entryLineUniversalData);
			}

			return new DutyCalculationIntermediateResult()
			{
				Amount = amount,
				RateCode = rateCode,
				Rate = rate,
				BaseValue = baseValue,
				UnitOfCalculation = methodOfCalculation
			};
		}

		void CalculateSpecialDuties(JobComInvoiceLine invoiceLine, List<DutyCalculationIntermediateResult> dutyCalculationEntryLineTempResults, EntryLineUniversalRate entryLineUniversalData)
		{
			CalculateSpecialDuty(entryLineUniversalData, SpecialDutyRateCodeList.Codes.AntiDumpingDuty, invoiceLine.JI_AntiDumpingDutyRate, dutyCalculationEntryLineTempResults);
			CalculateSpecialDuty(entryLineUniversalData, SpecialDutyRateCodeList.Codes.CountervailingDuty, invoiceLine.JI_CountervailingDutyRate, dutyCalculationEntryLineTempResults);
			CalculateSpecialDuty(entryLineUniversalData, SpecialDutyRateCodeList.Codes.AdditionalDuty, invoiceLine.JI_AdditionalDutyRate, dutyCalculationEntryLineTempResults);
			CalculateSpecialDuty(entryLineUniversalData, SpecialDutyRateCodeList.Codes.RetaliatoryDuty, invoiceLine.JI_RetaliatoryDutyRate, dutyCalculationEntryLineTempResults);
		}

		void CalculateSpecialDuty(EntryLineUniversalRate entryLineUniversalData, ZString rateCode, ZDecimal rate, List<DutyCalculationIntermediateResult> dutyCalculationIntermediateResults)
		{
			if (!dutyCalculationIntermediateResults.Any(x => x.RateCode == rateCode) && !rate.IsEmpty)
			{
				var refCusRateType = RefCusRateType.Loader.GetRateTypeByRateCode(declaration.Factory, Core.Constants.CountryCodes.Taiwan, rateCode);
				entryLineUniversalData.CustomsValueFormula = refCusRateType?.ZZR_CustomsValueFormula ?? ZString.Empty;
				var intermediateResult = new DutyCalculationIntermediateResult()
				{
					Amount = entryLineUniversalData.CustomsValue * rate,
					RateCode = rateCode,
					Rate = rate,
					BaseValue = entryLineUniversalData.ValueForDuty,
					PaymentMethod = EntryChargePaymentMethod.Codes.CAS,
					UnitOfCalculation = MethodOfCalculation.Percentage
				};
				dutyCalculationIntermediateResults.Add(intermediateResult);
				entryLineUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(intermediateResult.RateCode, intermediateResult.Amount);
			}
		}

		public void AddFeesToEntryLines(Dictionary<CusEntryLine, List<DutyCalculationIntermediateResult>> dutyCalculationEntryLineTempResultsCache)
		{
			foreach (var entryLineDutyResults in dutyCalculationEntryLineTempResultsCache)
			{
				var entryLine = entryLineDutyResults.Key;
				var fees = entryLineDutyResults.Value.GetGroupedFees().Where(x => !EntryLineHasFeeForThisCode(entryLine, x.RateCode) && (!x.Amount.IsEmpty || x.RateCode.Equals(RefCusRateCodes.DTA)));
				foreach (var dutyCalculation in fees)
				{
					var lineFee = entryLine.Fees.AddNew();
					lineFee.CF_RateOverrideReasonCode = ZString.Empty;
					lineFee.CF_BaseValue = dutyCalculation.BaseValue;
					lineFee.CF_ChargeType = dutyCalculation.RateCode;
					lineFee.CF_MethodOfCalculation = dutyCalculation.UnitOfCalculation.Left(Customs.Business.AutoCusEntryLineFee.Schema.CF_MethodOfCalculationMaxLength);
					lineFee.CF_MethodOfPayment = dutyCalculation.PaymentMethod;
					lineFee.CF_ChargeAmount = dutyCalculation.Amount;
					lineFee.CF_Rate = dutyCalculation.Rate;
				}
			}
		}

		static void SetDutyCalculationIntermediateResultFromEntryLineFee(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, List<DutyCalculationIntermediateResult> dutyCalculationEntryLineTempResults)
		{
			var overrideFees = entryLine.Fees.Cast<CusEntryLineFee>().Where(f => f.CF_RateOverrideReasonCode == TWRateOverrideReasonList.Codes.Override);
			foreach (var lineFee in overrideFees)
			{
				var dutyCalculationIntermediateResult = new DutyCalculationIntermediateResult();
				dutyCalculationIntermediateResult.Amount = lineFee.CF_ChargeAmount;
				dutyCalculationIntermediateResult.RateCode = lineFee.CF_ChargeType;
				dutyCalculationIntermediateResult.UnitOfCalculation = lineFee.CF_MethodOfCalculation;
				dutyCalculationIntermediateResult.PaymentMethod = lineFee.CF_MethodOfPayment;
				dutyCalculationIntermediateResult.Rate = lineFee.CF_Rate;
				dutyCalculationIntermediateResult.BaseValue = lineFee.CF_BaseValue;
				dutyCalculationEntryLineTempResults.Add(dutyCalculationIntermediateResult);

				entryLineUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(lineFee.CF_ChargeType, lineFee.CF_ChargeAmount);
			}
		}

		void CalculateFeeDelayedDeclaration(CusEntryHeader entryHeader, List<DutyCalculationIntermediateResult> dutyCalculationIntermediateResults)
		{
			decimal daysOfDelayedDeclaration = entryHeader.EntryInstruction?.CEI_DaysOfDelayedDeclaration ?? ZInt.Zero;
			if (daysOfDelayedDeclaration > 0)
			{
				var valuationDate = !entryHeader.EffectiveValuationDate.IsEmpty ? entryHeader.EffectiveValuationDate : ZDateTime.Now;
				CalculateCusTaxOrFee(daysOfDelayedDeclaration, RefCusTaxOrFeeCodes.DDF, valuationDate.ToDateTime(), DutyTaxPaymentMethodList.Codes.CashPayment, dutyCalculationIntermediateResults);
			}
		}

		#region Import Duty Calculation

		internal void CalculateImportDuties(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, (EntryLineUniversalRate cashPayment, EntryLineUniversalRate nonCashPayment, EntryLineUniversalRate allPayment)? entryLineUniversalDataForROR, JobComInvoiceLine invoiceLine, List<DutyCalculationIntermediateResult> dutyCalculationIntermediateResults)
		{
			var dutyPaymentMethod = invoiceLine.JI_DtyPymntMthd;
			if (invoiceLine.UniversalTariff is TariffView universalTariff)
			{
				var applicableRates = universalTariff.GetApplicableRates(invoiceLine.DutyRateSelectionCriteria).Where(x => !EntryLineHasFeeForThisCode(entryLine, x.RateCode));
				if (applicableRates.Any())
				{
					if (entryLineUniversalDataForROR.HasValue)
					{
						var entryLineUniversalDataForRORValue = entryLineUniversalDataForROR.Value;
						if (invoiceLine.JI_DtyPymntMthd == DutyTaxPaymentMethodList.Codes.RorPayment)
						{
							CalculateImportDuties(entryLine, applicableRates, entryLineUniversalDataForRORValue.cashPayment, invoiceLine, dutyCalculationIntermediateResults, DutyTaxPaymentMethodList.Codes.CashPayment);
							CalculateImportDuties(entryLine, applicableRates, entryLineUniversalDataForRORValue.nonCashPayment, invoiceLine, dutyCalculationIntermediateResults, DutyTaxPaymentMethodList.Codes.NonCashPayment);
						}
						else
						{
							CalculateImportDuties(entryLine, applicableRates, entryLineUniversalDataForRORValue.allPayment, invoiceLine, dutyCalculationIntermediateResults, dutyPaymentMethod);
						}
					}
					else
					{
						CalculateImportDuties(entryLine, applicableRates, entryLineUniversalData, invoiceLine, dutyCalculationIntermediateResults, dutyPaymentMethod);
					}
				}
			}
		}

		void CalculateImportDuties(CusEntryLine entryLine, IEnumerable<RateView> applicableRates, EntryLineUniversalRate entryLineUniversalData, JobComInvoiceLine invoiceLine, List<DutyCalculationIntermediateResult> dutyCalculationIntermediateResults, ZString dutyPaymentMethod)
		{
			var results = GetDutyCalculationResults(entryLine, entryLineUniversalData, applicableRates);
			var dutyRatesCodes = GetDutyRatesCodesIfRequired(invoiceLine);
			var hasDutyRatesCodes = dutyRatesCodes.Any();
			var isTariffAdditionalCodeEmpty = invoiceLine.JI_TariffAdditionalCode.IsEmpty;
			foreach (var dutyForEntryLineResult in results.Where(x => x.Amount != ZDecimal.Zero))
			{
				dutyForEntryLineResult.PaymentMethod = dutyPaymentMethod;
				var rateCode = dutyForEntryLineResult.RateCode;
				if (!dutyPaymentMethod.IsEmpty)
				{
					if (hasDutyRatesCodes && dutyRatesCodes.Contains(rateCode))
					{
						dutyForEntryLineResult.Amount = Utilities.Round(dutyForEntryLineResult.Amount * invoiceLine.JI_CusValueConvRatio, 3);
					}
					dutyCalculationIntermediateResults.Add(dutyForEntryLineResult);
					entryLineUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(rateCode, dutyForEntryLineResult.Amount);
				}
				else if (!isTariffAdditionalCodeEmpty && rateCode.Equals(RefCusRateCodes.DTA))
				{
					dutyForEntryLineResult.Rate = 0m;
					dutyForEntryLineResult.Amount = 0m;
					dutyCalculationIntermediateResults.Add(dutyForEntryLineResult);
					entryLineUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(rateCode, dutyForEntryLineResult.Amount);
				}
			}
		}

		static IEnumerable<ZString> GetDutyRatesCodesIfRequired(JobComInvoiceLine invoiceLine)
		{
			var convRatio = invoiceLine.JI_CusValueConvRatio;
			var dutyRatesCodes = Enumerable.Empty<ZString>();
			if (convRatio > 0 && convRatio < 1M)
			{
				dutyRatesCodes = CusRefRateCodeView.Loader.LoadByRateType(invoiceLine.Factory, Core.Constants.CountryCodes.Taiwan, RefCusRateTypes.Duty).Select(x => x.ZY1_RateCode);
			}
			return dutyRatesCodes;
		}

		#endregion

		ZDecimal CalculateRatesCodesToBeIncludedInValueForVATBaseAmount(List<DutyCalculationIntermediateResult> dutyCalculationEntryLineTempResults, ZString paymentMethod)
		{
			var ratesCodesToBeIncludedInValueForVATBase = new HashSet<ZString>
			{
				RefCusRateCodes.DTA,
				RefCusRateCodes.DTS,
				RefCusRateCodes.CTA,
				RefCusRateCodes.CTS,
				RefCusRateCodes.TAT,
				RefCusRateCodes.HWS,
				RefCusRateCodes.SSG,
				RefCusRateCodes.ADT,
				RefCusRateCodes.RTD,
				RefCusRateCodes.CVD,
				RefCusRateCodes.ADD
			};
			return dutyCalculationEntryLineTempResults.Where(x => ratesCodesToBeIncludedInValueForVATBase.Contains(x.RateCode) && x.PaymentMethod == paymentMethod).Sum(x => x.Amount.Truncate(2));
		}

		#region Commodity Duty Calculation

		internal void CalculateInvoiceLineAdditionalTax(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, (EntryLineUniversalRate cashPayment, EntryLineUniversalRate nonCashPayment, EntryLineUniversalRate allPayment)? entryLineUniversalDataForROR, JobComInvoiceLine invoiceLine, ZString rateType, List<DutyCalculationIntermediateResult> dutyCalculationIntermediateResults, ZString[] mutualExcludingRateCodes = null)
		{
			var isROR = entryLineUniversalDataForROR.HasValue;
			var entryLineUniversalDataForRORValue = entryLineUniversalDataForROR.GetValueOrDefault();
			foreach (JobComInvoiceLineTax lineTax in invoiceLine.Taxes)
			{
				var paymentMethod = lineTax.JLT_MethodOfPayment;
				if (lineTax.UniversalTariff is TariffView universalTariff)
				{
					if (isROR)
					{
						if (paymentMethod == DutyTaxPaymentMethodList.Codes.RorPayment)
						{
							CalculateTax(entryLine, entryLineUniversalDataForRORValue.cashPayment, universalTariff, rateType, dutyCalculationIntermediateResults, DutyTaxPaymentMethodList.Codes.CashPayment, mutualExcludingRateCodes);
							CalculateTax(entryLine, entryLineUniversalDataForRORValue.nonCashPayment, universalTariff, rateType, dutyCalculationIntermediateResults, DutyTaxPaymentMethodList.Codes.NonCashPayment, mutualExcludingRateCodes);
						}
						else if (!paymentMethod.IsEmpty)
						{
							CalculateTax(entryLine, entryLineUniversalDataForRORValue.allPayment, universalTariff, rateType, dutyCalculationIntermediateResults, paymentMethod, mutualExcludingRateCodes);
						}
					}
					else if (!paymentMethod.IsEmpty)
					{
						CalculateTax(entryLine, entryLineUniversalData, universalTariff, rateType, dutyCalculationIntermediateResults, paymentMethod, mutualExcludingRateCodes);
					}
				}
			}
		}

		void CalculateTax(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, TariffView universalTariff, ZString rateType, List<DutyCalculationIntermediateResult> dutyCalculationIntermediateResults, string paymentMethod, ZString[] mutualExcludingRateCodes = null)
		{
			var dutyInternalResults = new List<DutyCalculationIntermediateResult>();
			var rates = universalTariff.Rates.Where(rate => rate.ZZ2_ZZR_RateTypeCode == rateType && !EntryLineHasFeeForThisCode(entryLine, rate.RateCode));
			if (rates.Any())
			{
				foreach (var rate in rates)
				{
					entryLineUniversalData.CustomsValueFormula = rate?.CusRateType?.ZZR_CustomsValueFormula ?? ZString.Empty;
					var dutyForEntryLine = CalculateDutyAmount(entryLine, entryLineUniversalData, rate.ZZ2_RateFormula, rate.RateCode, rate.ZZ2_RateFormulaDerivedFrom);
					dutyForEntryLine.PaymentMethod = paymentMethod;

					if (dutyForEntryLine.Amount > 0)
					{
						dutyInternalResults.Add(dutyForEntryLine);
					}
				}

				dutyInternalResults = KeepAmongRatesIfNeeded(mutualExcludingRateCodes, dutyInternalResults);

				foreach (var dutyIntermediateResult in dutyInternalResults)
				{
					dutyCalculationIntermediateResults.Add(dutyIntermediateResult);
					entryLineUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(dutyIntermediateResult.RateCode, dutyIntermediateResult.Amount);
				}
			}
		}

		static List<DutyCalculationIntermediateResult> KeepAmongRatesIfNeeded(ZString[] mutualExcludingRateCodes, List<DutyCalculationIntermediateResult> dutyInternalResults)
		{
			if (mutualExcludingRateCodes != null && mutualExcludingRateCodes.Length > 1 && dutyInternalResults.Any())
			{
				var comparedIntermediateResult = dutyInternalResults.Where(result => mutualExcludingRateCodes.Contains(result.RateCode));
				dutyInternalResults = dutyInternalResults.Except(comparedIntermediateResult).ToList();
				if (comparedIntermediateResult.Any())
				{
					dutyInternalResults.Add(comparedIntermediateResult.OrderByDescending(item => item.Amount).FirstOrDefault());
				}
			}

			return dutyInternalResults;
		}

		#endregion

		public void ExemptDutyCalculation(CusEntryHeader entryHeader, Dictionary<CusEntryLine, List<DutyCalculationIntermediateResult>> dutyCalculationEntryLineTempResultsCache)
		{
			var waiverOfExemption = entryHeader.EntryInstruction?.CEI_WaiverOfExemption ?? ZBool.False;
			var allTariffExemptible = !entryHeader.MergedLines.Any(x => IsNonExemptibleTariff(x.CL_AdValoremTariff));
			if (entryHeader.CustomsValue < 2000 && !waiverOfExemption && allTariffExemptible)
			{
				dutyCalculationEntryLineTempResultsCache.Keys.ForEach(x => x.CL_ValueForVAT = ZDecimal.Zero);
				dutyCalculationEntryLineTempResultsCache.Clear();
			}
		}

		bool IsNonExemptibleTariff(ZString tariff) => tariff.StartsWith("240", StringComparison.Ordinal) ||
										tariff.StartsWith("210390901", StringComparison.Ordinal) ||
										tariff.StartsWith("210390902", StringComparison.Ordinal) ||
										tariff.StartsWith("210390903", StringComparison.Ordinal) ||
										tariff.StartsWith("2203", StringComparison.Ordinal) ||
										tariff.StartsWith("2204", StringComparison.Ordinal) ||
										tariff.StartsWith("2205", StringComparison.Ordinal) ||
										tariff.StartsWith("2206", StringComparison.Ordinal) ||
										tariff.StartsWith("2207", StringComparison.Ordinal) ||
										tariff.StartsWith("2208", StringComparison.Ordinal);

		internal void CalculateTPF(CusEntryHeader entryHeader, CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, (EntryLineUniversalRate cashPayment, EntryLineUniversalRate nonCashPayment, EntryLineUniversalRate allPayment)? entryLineUniversalDataForROR, ZDecimal tpfThreshold, ZString tpfPaymentMethod, DateTime dateOfValuation, List<DutyCalculationIntermediateResult> dutyCalculationIntermediateResults)
		{
			if (entryLineUniversalDataForROR.HasValue)
			{
				var entryLineUniversalDataForRORValue = entryLineUniversalDataForROR.Value;
				if (entryHeader.RorTPFCashAmountCalculation >= tpfThreshold && tpfPaymentMethod == DutyTaxPaymentMethodList.Codes.RorPayment)
				{
					CalculateTPF(entryLine, entryLineUniversalDataForRORValue.cashPayment, RefCusTaxOrFeeCodes.TPF, dateOfValuation, DutyTaxPaymentMethodList.Codes.CashPayment, dutyCalculationIntermediateResults);
					CalculateTPF(entryLine, entryLineUniversalDataForRORValue.nonCashPayment, RefCusTaxOrFeeCodes.TPF, dateOfValuation, DutyTaxPaymentMethodList.Codes.NonCashPayment, dutyCalculationIntermediateResults);
				}
				else if (tpfPaymentMethod == DutyTaxPaymentMethodList.Codes.CashPayment)
				{
					CalculateTPF(entryLine, entryLineUniversalDataForRORValue.allPayment, RefCusTaxOrFeeCodes.TPF, dateOfValuation, DutyTaxPaymentMethodList.Codes.CashPayment, dutyCalculationIntermediateResults);
				}
				else
				{
					CalculateTPF(entryLine, entryLineUniversalDataForRORValue.allPayment, RefCusTaxOrFeeCodes.TPF, dateOfValuation, DutyTaxPaymentMethodList.Codes.NonCashPayment, dutyCalculationIntermediateResults);
				}
			}
			else if (!tpfPaymentMethod.IsEmpty)
			{
				CalculateTPF(entryLine, entryLineUniversalData, RefCusTaxOrFeeCodes.TPF, dateOfValuation, tpfPaymentMethod, dutyCalculationIntermediateResults);
			}
		}

		void CalculateTPF(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, ZString taxOrFeeType, DateTime dateOfValuation, ZString paymentMethod, List<DutyCalculationIntermediateResult> dutyCalculationIntermediateResults)
		{
			var baseValue = entryLineUniversalData.CustomsValue;
			if (entryLine != null && !EntryLineHasFeeForThisCode(entryLine, taxOrFeeType))
			{
				var taxOrFee = GetRefCusTaxOrFee(taxOrFeeType, dateOfValuation);
				var taxOrFeeValue = taxOrFee?.ZZF_Value ?? ZDecimal.Zero;
				var tempResult = baseValue * taxOrFeeValue;
				var calculationResult = new DutyCalculationIntermediateResult()
				{
					Amount = Utilities.Round(tempResult, 3),
					RateCode = taxOrFeeType,
					PaymentMethod = paymentMethod,
					Rate = taxOrFeeValue,
					BaseValue = baseValue,
					UnitOfCalculation = MethodOfCalculation.Percentage
				};
				dutyCalculationIntermediateResults.Add(calculationResult);
				entryLineUniversalData?.CountrySpecificValueList.AddNewKeyOrAccumulateValue(calculationResult.RateCode, calculationResult.Amount);
			}
		}

		public void CalculateVAT(CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, EntryLineUniversalRate entryLineUniversalDataAdditional, bool isROR, JobComInvoiceLine invoiceLine, ZString taxOrFeeType, DateTime dateOfValuation, ZString paymentMethod, List<DutyCalculationIntermediateResult> dutyCalculationIntermediateResults)
		{
			var lineProcedure = invoiceLine.JI_Procedure;
			var isLineProcedure67or69 = lineProcedure == Constants.ProcedureCodes._67 || lineProcedure == Constants.ProcedureCodes._69;
			var needAddCAS = paymentMethod == DutyTaxPaymentMethodList.Codes.CashPayment || (isROR && (paymentMethod == DutyTaxPaymentMethodList.Codes.NonCashPayment || paymentMethod == DutyTaxPaymentMethodList.Codes.RorPayment));
			var needAddDEF = paymentMethod == DutyTaxPaymentMethodList.Codes.NonCashPayment || (paymentMethod == DutyTaxPaymentMethodList.Codes.CashPayment && !isROR && !isLineProcedure67or69);

			var allCAS = CalculateRatesCodesToBeIncludedInValueForVATBaseAmount(dutyCalculationIntermediateResults, DutyTaxPaymentMethodList.Codes.CashPayment);
			var allDEF = CalculateRatesCodesToBeIncludedInValueForVATBaseAmount(dutyCalculationIntermediateResults, DutyTaxPaymentMethodList.Codes.NonCashPayment);
			entryLine.CL_ValueForVAT = entryLineUniversalData.CustomsValue + (needAddCAS ? allCAS : 0m) + (needAddDEF ? allDEF : 0m);

			if (entryLine != null && !EntryLineHasFeeForThisCode(entryLine, taxOrFeeType))
			{
				var taxOrFee = GetRefCusTaxOrFee(taxOrFeeType, dateOfValuation);
				var taxOrFeeValue = taxOrFee?.ZZF_Value ?? ZDecimal.Zero;
				var taxesDEF = CalculateRatesCodesToBeIncludedInValueForVATBaseAmount(dutyCalculationIntermediateResults, DutyTaxPaymentMethodList.Codes.NonCashPayment);

				if (paymentMethod == DutyTaxPaymentMethodList.Codes.RorPayment && isROR)
				{
					CalculateVATForCashPayment(entryLine.CL_ValueForVAT, taxesDEF, isLineProcedure67or69, entryLineUniversalData, taxOrFeeValue, dutyCalculationIntermediateResults);
					CalculateVATForNonCashPayment(entryLineUniversalDataAdditional.CustomsValue + taxesDEF, entryLineUniversalDataAdditional, taxOrFeeValue, dutyCalculationIntermediateResults);
				}
				else if (paymentMethod == DutyTaxPaymentMethodList.Codes.CashPayment)
				{
					CalculateVATForCashPayment(entryLine.CL_ValueForVAT, taxesDEF, isLineProcedure67or69, entryLineUniversalData, taxOrFeeValue, dutyCalculationIntermediateResults);
				}
				else
				{
					CalculateVATForNonCashPayment(entryLine.CL_ValueForVAT, entryLineUniversalData, taxOrFeeValue, dutyCalculationIntermediateResults);
				}
			}
		}

		void CalculateVATForCashPayment(ZDecimal baseValue, ZDecimal taxesDEF, bool isLineProcedure67or69, EntryLineUniversalRate entryLineUniversalData, ZDecimal taxOrFeeValue, List<DutyCalculationIntermediateResult> dutyCalculationIntermediateResults)
		{
			AddCalculationVATResult(baseValue, taxOrFeeValue, DutyTaxPaymentMethodList.Codes.CashPayment, dutyCalculationIntermediateResults, entryLineUniversalData);
			if (isLineProcedure67or69 && !taxesDEF.IsEmpty)
			{
				AddCalculationVATResult(taxesDEF, taxOrFeeValue, DutyTaxPaymentMethodList.Codes.NonCashPayment, dutyCalculationIntermediateResults, entryLineUniversalData);
			}
		}

		void CalculateVATForNonCashPayment(ZDecimal baseValue, EntryLineUniversalRate entryLineUniversalData, ZDecimal taxOrFeeValue, List<DutyCalculationIntermediateResult> dutyCalculationIntermediateResults)
		{
			AddCalculationVATResult(baseValue, taxOrFeeValue, DutyTaxPaymentMethodList.Codes.NonCashPayment, dutyCalculationIntermediateResults, entryLineUniversalData);
		}

		void AddCalculationVATResult(ZDecimal baseValue, ZDecimal rate, ZString paymentMethod, List<DutyCalculationIntermediateResult> dutyCalculationIntermediateResults, EntryLineUniversalRate entryLineUniversalData)
		{
			var tempResult = baseValue * rate;
			var calculationResult = new DutyCalculationIntermediateResult()
			{
				Amount = Utilities.Round(tempResult, 3),
				RateCode = RefCusTaxOrFeeCodes.VAT,
				PaymentMethod = paymentMethod,
				Rate = rate,
				BaseValue = baseValue,
				UnitOfCalculation = MethodOfCalculation.Percentage
			};
			dutyCalculationIntermediateResults.Add(calculationResult);
			entryLineUniversalData?.CountrySpecificValueList.AddNewKeyOrAccumulateValue(calculationResult.RateCode, calculationResult.Amount);
		}

		void CalculateCusTaxOrFee(ZDecimal baseValue, ZString taxOrFeeType, DateTime dateOfValuation, ZString paymentMethod, List<DutyCalculationIntermediateResult> dutyCalculationIntermediateResults)
		{
			if (!paymentMethod.IsEmpty)
			{
				var taxOrFee = GetRefCusTaxOrFee(taxOrFeeType, dateOfValuation);
				var taxOrFeeValue = taxOrFee?.ZZF_Value ?? ZDecimal.Zero;
				var tempResult = baseValue * taxOrFeeValue;
				var calculationResult = new DutyCalculationIntermediateResult()
				{
					Amount = Utilities.Round(tempResult, 3),
					RateCode = taxOrFeeType,
					PaymentMethod = paymentMethod,
					Rate = taxOrFeeValue,
					BaseValue = baseValue,
					UnitOfCalculation = MethodOfCalculation.Percentage
				};
				dutyCalculationIntermediateResults.Add(calculationResult);
			}
		}

		RefCusTaxOrFee GetRefCusTaxOrFee(ZString taxOrFeeType, ZDateTime dateOfValuation)
		{
			return new RefCusTaxOrFee.Loader(declaration.Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Taiwan, taxOrFeeType, dateOfValuation);
		}

		void ClearTpfUnderMinimumThreshold(ZDecimal tpfThreshold, Dictionary<CusEntryLine, List<DutyCalculationIntermediateResult>> dutyCalculationEntryLineTempResultsCache)
		{
			var tpfCharges = dutyCalculationEntryLineTempResultsCache.Values.SelectMany(x => x).Where(charge => charge.RateCode == RefCusTaxOrFeeCodes.TPF);
			if (tpfThreshold > ZDecimal.Zero && tpfCharges.Any())
			{
				var totalTpfAmounts = tpfCharges.Sum(charge => charge.Amount);
				if (totalTpfAmounts < tpfThreshold)
				{
					foreach (var dutyCalculationEntryLineTempResults in dutyCalculationEntryLineTempResultsCache.Values)
					{
						dutyCalculationEntryLineTempResults.RemoveAll(charge => charge.RateCode == RefCusTaxOrFeeCodes.TPF);
					}
				}
			}
		}

		static void AddDutyChargesToEntryHeader(CusEntryHeader entryHeader, IEnumerable<DutyCalculationIntermediateResult> duties)
		{
			foreach (var duty in duties)
			{
				var charge = entryHeader.Charges.AddNew();
				charge.C1_ChargeType = duty.RateCode;
				charge.C1_ChargeAmount = duty.Amount;
				charge.C1_MethodOfPayment = duty.PaymentMethod;
			}
		}

		void AddAllEntryLinesToHasChangesHunters(CusEntryHeader entryHeader)
		{
			hasChangesHunters = new Dictionary<CusEntryLine, HasChangesHunter>();
			entryHeader.AllEntryLines.Cast<CusEntryLine>().ForEach(entryLine =>
			{
				if (!hasChangesHunters.TryGetValue(entryLine, out var hunter))
				{
					hunter = new HasChangesHunter(entryLine);
					hasChangesHunters.Add(entryLine, hunter);
				}
				hunter.Mark();
			});
		}
	}
}
