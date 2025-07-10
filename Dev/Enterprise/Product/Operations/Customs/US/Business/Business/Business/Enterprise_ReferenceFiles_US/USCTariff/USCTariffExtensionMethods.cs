using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class USCTariffExtensionMethods
	{
		public static ZString GetTaxFeeRateDescription(this USCTariff importTariff, ZString taxCode, ZString rateType)
		{
			USCTariffDutyRate feeTaxDutyRate = null;
			foreach (USCTariffDutyRate dutyRate in importTariff.DutyRates)
			{
				if (dutyRate.UD_TaxFeeClassCode == taxCode)
				{
					feeTaxDutyRate = dutyRate;
					break;
				}
			}

			ZString result = ZString.Empty;
			if (feeTaxDutyRate != null)
			{
				//Computation code x due to CKG being missed out from reporting units sometimes
				if (taxCode == Core.Constants.USCustoms.FeeCodes.DairyFee)
				{
					result = GetPerUnitDescription(feeTaxDutyRate.UD_TaxFeeSpecificRate, ABIUnitOfMeasureList.Codes.ContentKilogram);
				}
				else
				{
					ZString computationCode = feeTaxDutyRate.UD_TaxFeeComputationCode;

					// This is a US customs mistake we cover for. In reference file, it is recorded as 'X'
					if (importTariff.UE_Tariff == "2403102050" || importTariff.UE_Tariff == "2403102080")
					{
						computationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
					}

					result = GetRateDescription(computationCode, rateType, feeTaxDutyRate.UD_TaxFeeSpecificRate, feeTaxDutyRate.UD_TaxFeeAdvalorem, ZDecimal.Zero, importTariff);
				}
			}
			return result;
		}

		static ZString GetRateDescription(string computationCode, ZString rateType, ZDecimal p1, ZDecimal p2, ZDecimal p3, USCTariff importTariff)
		{
			ZString unit1 = importTariff.UE_Unit1;
			ZString unit2 = importTariff.UE_Unit2;
			ZString unit3 = importTariff.UE_Unit3;

			switch (computationCode)
			{
				case ComputationCodeList.Codes.SpecificRateFirstQuantity:
					return GetPerUnitDescription(p1, unit1);

				case ComputationCodeList.Codes.SpecificRateSecondQuantity:
					return GetPerUnitDescription(p1, unit2);

				case ComputationCodeList.Codes.MultipleSpecific:
					return GetPerUnitDescription(p1, unit1) + " + " + GetPerUnitDescription(p3, unit2);

				case ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity:
					return GetPerUnitDescription(p1, unit1) + " + " + GetPercentageDescription(p2);

				case ComputationCodeList.Codes.CompoundSpecificAndAdValoremSecondQuantity:
					return GetPerUnitDescription(p1, unit2) + " + " + GetPercentageDescription(p2);

				case ComputationCodeList.Codes.SpecificPlusCompound:
					return GetPerUnitDescription(p1, unit2) + " + " + GetPerUnitDescription(p3, unit2) + " + " + GetPercentageDescription(p2);

				case ComputationCodeList.Codes.AdValorem:
					return GetPercentageDescription(p2);

				case ComputationCodeList.Codes.Derived:
					return GetPercentageDescription(p2) + " on Derived Duty";

				case ComputationCodeList.Codes.FunctionalAdValorem:
					return GetDescWrappedWithParanthesis(GetPerUnitDescription(p3, unit3), p2.ToStringTrimZeros()) + " * EV";

				case ComputationCodeList.Codes.SpecificFunctionalAdValorem:
					string specificToSecondQty = GetPerUnitDescription(p1, unit2);
					string functionalAdvalorem = GetRateDescription(ComputationCodeList.Codes.FunctionalAdValorem, rateType, p1, p2, p3, importTariff);
					return GetDescWrappedWithParanthesis(specificToSecondQty, functionalAdvalorem);

				case ComputationCodeList.Codes.SpecificSpecific:
					if (rateType == RateTypeList.Codes.Primary)
					{
						return GetPerUnitDescription(p1, unit1);
					}
					else if (rateType == RateTypeList.Codes.Secondary)
					{
						return GetPerUnitDescription(p2, unit1);
					}
					else
					{
						return "Select Tax Rate Type";
					}

				case ComputationCodeList.Codes.CompoundSpecificAdValorem:
					return GetPerUnitDescription(p1, unit3) + " + " + GetPercentageDescription(p2);

				case ComputationCodeList.Codes.SpecificCompound:
					string specificToSecondQty_1 = GetRateDescription(ComputationCodeList.Codes.SpecificRateSecondQuantity, rateType, p1, p2, p3, importTariff);
					string specificToThirdQty = GetPerUnitDescription(p3, unit3);
					return specificToSecondQty_1 + " + " + specificToThirdQty + " + " + GetPercentageDescription(p2);

				case ComputationCodeList.Codes.SpecificPyrotechnics:
					string secondPart = GetDescWrappedWithParanthesis(p1.ToStringTrimZeros(), GetPerUnitDescription(p3, unit2));
					return p1.ToStringTrimZeros() + " * " + secondPart;

				default:
					return "";
			}
		}

		public const string SelectTaxRateType = "Select Tax Rate Type";

		static string GetPerUnitDescription(ZDecimal rate, ZString unit)
		{
			return IFeeCalculationDataProviderExtensionMethods.AmountPerUnit(rate, unit);
		}

		static string GetPercentageDescription(ZDecimal rate)
		{
			ZDecimal percentageRate = rate * 100m;
			return percentageRate.ToString(percentageRate.DecimalPlaces) + "%";
		}

		static string GetDescWrappedWithParanthesis(ZString desc1, ZString desc2)
		{
			return "(" + desc1 + (!desc2.IsEmpty ? " + " + desc2 : "") + ")";
		}

		public static IEnumerable<ZString> GetReconTariffRequiredFeeCodes(this USCTariff tariff, ZBool isCottonFeeExempt, ZBool isCottonFeeMandatory)
		{
			if (tariff != null)
			{
				foreach (var feeCode in tariff.GetRequiredFeeCodes())
				{
					if (!CusFeeCodeConstants.IsExciseTax(feeCode) && (feeCode != Core.Constants.USCustoms.FeeCodes.Cotton || !isCottonFeeExempt || isCottonFeeMandatory))
					{
						yield return feeCode;
					}
				}
			}
		}
	}
}
