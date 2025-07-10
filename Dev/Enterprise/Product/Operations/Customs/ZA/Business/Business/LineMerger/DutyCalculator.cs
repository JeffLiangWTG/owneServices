using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.ZA.Business
{
	class DutyCalculator
	{
		public DutyCalculator(bool canBeNegative, bool shouldTruncate, int dutyDecimalPlace)
		{
			this.canBeNegative = canBeNegative;
			this.shouldTruncate = shouldTruncate;
			this.dutyDecimalPlace = dutyDecimalPlace;
		}

		readonly bool canBeNegative;
		readonly bool shouldTruncate;
		readonly int dutyDecimalPlace;

		#region CalculateOtherDuties

		public void CalculateOtherDuties(UniversalRateCalcData universalRateData, IEnumerable<ITariffDetail> tariffDetails, ZString tariff)
		{
			var cusLineTariffDetails = tariffDetails
										.Where(x => !x.ShouldBeExcluded)
										.OrderBy(x => x.UniversalTariff, new DutyCalculationHelper.CusTariffTypeOrderComparer(DefaultRateCalculationOrder));
			CalculateAdValoremExcise(universalRateData, cusLineTariffDetails, tariff);
			CalculateOtherDutiesLeviesAndRebates(universalRateData, cusLineTariffDetails, tariff);
		}

		static readonly ImmutableList<ZString> DefaultRateCalculationOrder = new ZString[] { RateTypes.AdValoremExcise, RateTypes.Excise, RateTypes.Levy, RateTypes.AntiDumping, RateTypes.Rebate, RateTypes.Refund }.ToImmutableList();
		static readonly ImmutableList<ZString> RateTypesNeededForEX1Calculation = new ZString[] { RateTypes.AntiDumping, RateTypes.Rebate }.ToImmutableList();

		void CalculateAdValoremExcise(UniversalRateCalcData universalRateData, IOrderedEnumerable<ITariffDetail> cusLineTariffDetails, ZString mainTariff)
		{
			if (cusLineTariffDetails.Any(x => x.UniversalTariff?.Rates.Any(y => y.ZZ2_ZZR_RateTypeCode == RateTypes.AdValoremExcise) ?? false))
			{
				foreach (var tariffDetail in cusLineTariffDetails.WhereRateTypeIn(RateTypesNeededForEX1Calculation))
				{
					CalculateAndPopulateDuty(universalRateData, tariffDetail, mainTariff, applyRebate: false);
				}

				foreach (var tariffDetail in cusLineTariffDetails.WhereRateType(RateTypes.AdValoremExcise))
				{
					CalculateAndPopulateDuty(universalRateData, tariffDetail, mainTariff);
				}
				universalRateData.SetVPBAmount(universalRateData.ValueForDuty);

				var ex1TypeRateCodeCriteria = new RateCodeLoadCriteria { RateTypesToInclude = RateTypesNeededForEX1Calculation.ToArray() };
				var rateCodesToClearInValueList = CusRefRateCodeView.Loader.Load(universalRateData.Factory, Core.Constants.CountryCodes.SouthAfrica, ex1TypeRateCodeCriteria);
				foreach (var rateCode in rateCodesToClearInValueList)
				{
					universalRateData.ResetCountrySpecificValueListEntry(rateCode.ZY1_RateCode);
				}
			}
		}

		void CalculateOtherDutiesLeviesAndRebates(UniversalRateCalcData universalRateData, IOrderedEnumerable<ITariffDetail> cusLineTariffDetails, ZString mainTariff)
		{
			foreach (var tariffDetail in cusLineTariffDetails.WhereRateTypeNot(RateTypes.AdValoremExcise))
			{
				CalculateAndPopulateDuty(universalRateData, tariffDetail, mainTariff);
			}
		}

		#endregion

		void CalculateAndPopulateDuty(UniversalRateCalcData universalRateData, ITariffDetail tariffDetail, string relatedTariffCode = null, bool applyRebate = true)
		{
			var formulaSpecificValue = ZArchitecture.Core.Utilities.Round(ZArchitecture.Core.Utilities.ConvertToDecimal(tariffDetail.FormulaSpecificValue.ToString()), tariffDetail.FormulaSpecificValueScale);
			var formulaSpecificQuestion = tariffDetail.FormulaSpecificQuestion;
			CalculateAndPopulateDuty(universalRateData, tariffDetail.Type, tariffDetail.Tariff, formulaSpecificQuestion, formulaSpecificValue, relatedTariffCode, applyRebate);
		}

		public List<Action<ZShort>> CalculateAndPopulateDuty(UniversalRateCalcData universalRateData, ZString tariffType, ZString tariffCode, ZString formulaSpecificQuestion, ZDecimal formulaSpecificValue, string relatedTariffCode = null, bool applyRebate = true, CusEntryLine entryLine = null, DutyRebateCalculator dutyRebateCalculator = null)
		{
			var addInfoActions = new List<Action<ZShort>>();

			if (!tariffType.IsEmpty && !tariffCode.IsEmpty)
			{
				var dateOfValuation = universalRateData.DateOfValuation;

				var selectedTariff = universalRateData.Factory.GetCusTariff(tariffType, tariffCode, dateOfValuation);
				if (!(selectedTariff != null && selectedTariff.RelatedTariffs.Any(x => x.ZZH_ZZI_TariffTypeCode == "1P1" && (x.ZZH_TariffCode.IsEmpty || x.ZZH_TariffCode == "0"))))
				{
					selectedTariff = universalRateData.Factory.GetCusTariff(tariffType, tariffCode, dateOfValuation, relatedTariffCode);
				}

				if (selectedTariff != null)
				{
					var criteria = new SpecificRateSelectionCriteria(universalRateData.CountryOfOrigin, Core.Constants.CountryCodes.SouthAfrica, universalRateData.Preference, ZString.Empty, null, universalRateData.DateOfValuation, ZString.Empty, ZString.Empty);
					var selectedRate = selectedTariff.GetApplicableRate(criteria);
					if (selectedRate != null)
					{
						universalRateData.CustomsValueFormula = selectedRate.CusRateType?.ZZR_CustomsValueFormula ?? ZString.Empty;
						var result = Enterprise.Customs.Business.DutyCalculatorHelper.Calculate(universalRateData, selectedRate.ZZ2_RateFormula, dutyDecimalPlace, formulaSpecificQuestion, formulaSpecificValue, shouldTruncate, canBeNegative);

						if (entryLine != null && result > 0)
						{
							addInfoActions = dutyRebateCalculator?.CalculateRebatedValueForDuty(result, entryLine);
							result -= dutyRebateCalculator.AmountToRebate;
						}

						universalRateData.CountrySpecificValueListUpdateOrAddNew(tariffType, result);
						if (!universalRateData.CountrySpecificTypeList.Contains(tariffType))
						{
							universalRateData.CountrySpecificTypeList.Add(tariffType);
						}

						if (applyRebate && selectedTariff.Rates.Any(x => x.ZZ2_ZZR_RateTypeCode == RateTypes.Rebate))
						{
							ApplyRebate(universalRateData, selectedTariff, result);
						}
					}
				}
			}

			return addInfoActions;
		}

		void ApplyRebate(UniversalRateCalcData universalRateData, TariffView tariff, ZDecimal rebateAmount)
		{
			var rebateSequenceByRateType = tariff.CusTariffType.ZZI_TariffType.SubstringSafe(0, 1) == UniversalReferenceConstants.Schedule._3 ? S3DefaultRebateOrder : S4DefaultRebateOrder;
			var rebateSequenceOverride = tariff?.GetAttribute(UniversalReferenceConstants.TariffAttributes.RebateSequence);
			if (rebateSequenceOverride != null && !rebateSequenceOverride.ZZ3_Value.IsEmpty)
			{
				rebateSequenceByRateType = ((ZString)new Regex(@"\s").Replace(rebateSequenceOverride.ZZ3_Value, ZString.Empty)).Split(',').ToImmutableList();
			}

			var rebateSequenceByRateCode = new List<ZString>();

			var rebateSequenceRateCodeCriteria = new RateCodeLoadCriteria { RateTypesToInclude = rebateSequenceByRateType.ToArray() };
			var rateCodes = CusRefRateCodeView.Loader.Load(universalRateData.Factory, Core.Constants.CountryCodes.SouthAfrica, rebateSequenceRateCodeCriteria);
			if (rateCodes != null)
			{
				rebateSequenceByRateCode.AddRange(rateCodes.OrderBy(x => x, new DutyCalculationHelper.CusRateTypeOrderComparer(rebateSequenceByRateType)).Select(x => x.ZY1_RateCode));
			}

			var countrySpecificValueList = universalRateData.CountrySpecificValueList;
			foreach (var rateCodeReduceable in rebateSequenceByRateCode)
			{
				if (rebateAmount == 0)
				{
					break;
				}

				if (countrySpecificValueList.ContainsKey(rateCodeReduceable))
				{
					var reduceableAmount = countrySpecificValueList[rateCodeReduceable];
					if (reduceableAmount <= 0)
					{
						continue;
					}

					if (reduceableAmount <= rebateAmount)
					{
						rebateAmount -= reduceableAmount;

						countrySpecificValueList[rateCodeReduceable] = ZDecimal.Zero;
					}
					else
					{
						countrySpecificValueList[rateCodeReduceable] -= rebateAmount;
						rebateAmount = ZDecimal.Zero;
					}
				}
			}
		}

		static readonly ImmutableList<ZString> S3DefaultRebateOrder = new ZString[] { RateTypes.Duty, RateTypes.Excise, RateTypes.AdValoremExcise, RateTypes.Levy }.ToImmutableList();
		static readonly ImmutableList<ZString> S4DefaultRebateOrder = new ZString[] { RateTypes.Duty, RateTypes.AntiDumping, RateTypes.Excise, RateTypes.AdValoremExcise, RateTypes.Levy }.ToImmutableList();
	}
}
