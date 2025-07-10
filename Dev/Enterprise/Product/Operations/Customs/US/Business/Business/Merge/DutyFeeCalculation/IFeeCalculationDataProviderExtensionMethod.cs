using CargoWise.Types;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.US.Business
{
	public static class IFeeCalculationDataProviderExtensionMethod
	{
		public static bool TariffHasBabyFomulaAttribute(this IFeeCalculationDataProvider line, ZDate dateForDutyCalculation)
		{
			var factory = line.Factory;
			return USRefTariffDataLoader.TariffViewHasRuleWithAttribute(factory, line.Tariff, dateForDutyCalculation, TariffAttributeTypes.Codes.TYPE, TariffAttributeTypes.Values.BabyFomula) != null
				|| (line.ParentTariffLine != null && USRefTariffDataLoader.TariffViewHasRuleWithAttribute(factory, line.ParentTariffLine.Tariff, dateForDutyCalculation, TariffAttributeTypes.Codes.TYPE, TariffAttributeTypes.Values.BabyFomula) != null);
		}

		public static bool IsFeeApplicable(this IFeeCalculationDataProvider line, string feeCode, USCTariff importTariff, ZDate dateForDutyCalculation)
		{
			var result = false;
			if (TariffHasBabyFomulaAttribute(line, dateForDutyCalculation))
			{
				return result;
			}
			else if (feeCode == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing && Chapter98Helper.IsExemptMPFForCombineLines(line))
			{
				return result;
			}
			else if (feeCode == Core.Constants.USCustoms.FeeCodes.Coffee)
			{
				result = CoffeeFeeCalculator.IsFeeApplicable(line, importTariff);
			}
			else
			{
				bool isFeeRelatedToTariffNumber = CusFeeCodeConstants.IsRelatedToTariffNumber(feeCode);

				if (!isFeeRelatedToTariffNumber)
				{
					result = IsNonTariffRelatedFeeApplicable(line);
				}
				else
				{
					result = IsTariffRelatedFeeApplicable(line, feeCode, importTariff);
				}
			}

			return result;
		}

		static bool IsNonTariffRelatedFeeApplicable(IFeeCalculationDataProvider line)
		{
			return !line.IsSetVLine;
		}

		static bool IsTariffRelatedFeeApplicable(IFeeCalculationDataProvider line, string feeCode, USCTariff importTariff)
		{
			bool result = false;

			if (!line.IsSetXLine && importTariff != null)
			{
				result = !CusFeeCodeConstants.IsExciseTax(feeCode) && importTariff.IsFeeApplicable(feeCode)
							 || CusFeeCodeConstants.IsExciseTax(feeCode) && line.TaxCode == feeCode;
			}
			return result;
		}
	}
}
