using System;
using CargoWise.Types;
using static Enterprise.Customs.US.Business.DutyRateWrapper;

namespace Enterprise.Customs.US.Business
{
	public static class RoundedQuantity
	{
		public static ZDecimal GetRoundedQuantity1(IFeeCalculationDataProvider dutyData, ZDecimal qty1)
		{
			var importTariff = dutyData.ImportTariff;
			if (IsTaxApplicable(dutyData, () => ComputationCodeList.IsFirstQuantityRequired(dutyData.TaxComputationCode))
				|| (!dutyData.VisaNumber.IsEmpty && importTariff != null
				&& (importTariff.UE_DutyComputationCode == ComputationCodeList.Codes.AdValorem || importTariff.UE_DutyComputationCode == ComputationCodeList.Codes.Free)))
			{
				return qty1.Round(2);
			}

			var result = qty1.Round(0);
			if (importTariff != null && !dutyData.HasTextileCategoryNo)
			{
				var rate = CalculateRateWithComputationCode(importTariff, dutyData);

				if (rate >= 0)
				{
					bool doNotRoundZeroRate = rate == 0 &&
											(importTariff.UE_DutyComputationCode == ComputationCodeList.Codes.SpecificRateFirstQuantity ||
											importTariff.UE_DutyComputationCode == ComputationCodeList.Codes.SpecificRateSecondQuantity);
					if (rate > 1 || doNotRoundZeroRate)
					{
						result = qty1.Round(2);
					}
				}
			}

			return result;
		}

		static ZDecimal CalculateRateWithComputationCode(USCTariff importTariff, IFeeCalculationDataProvider dutyData)
		{
			var rateWrapper = DutyRateWrapper.GetWrapper(dutyData);
			var rate = ZDecimal.Zero;
			if (rateWrapper is SpecialWrapper specialWrapper && specialWrapper.tariffDutyRate.IsNull)
			{
				rate = -1m;
			}
			else
			{
				switch (importTariff.UE_DutyComputationCode)
				{
					case ComputationCodeList.Codes.SpecificRateFirstQuantity:
					case ComputationCodeList.Codes.SpecificRateSecondQuantity:
					case ComputationCodeList.Codes.MultipleSpecific:
					case ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity:
					case ComputationCodeList.Codes.SpecificPlusCompound:
						rate = rateWrapper.Specific;
						break;

					case ComputationCodeList.Codes.SpecificSpecific:
						rate = dutyData.SelectedRateType == RateTypeList.Codes.Primary ? rateWrapper.Specific : rateWrapper.Advalorem;
						break;

					case ComputationCodeList.Codes.SpecificPyrotechnics:
						rate = rateWrapper.Specific + rateWrapper.Other * dutyData.Quantity2;
						break;

					case ComputationCodeList.Codes.SpecificSugarK:
						rate = Math.Max(rateWrapper.Specific - rateWrapper.Advalorem * (100 - dutyData.Quantity2), rateWrapper.Other);
						break;
				}
			}
			return rate;
		}

		public static ZDecimal GetRoundedQuantity2(IFeeCalculationDataProvider dutyData, ZDecimal qty2)
		{
			var importTariff = dutyData.ImportTariff;
			if (IsTaxApplicable(dutyData, () => ComputationCodeList.IsSecondQuantityRequired(dutyData.TaxComputationCode)))
			{
				return qty2.Round(2);
			}

			var result = qty2.Round(0);

			if (importTariff != null && !dutyData.HasTextileCategoryNo)
			{
				var rateWrapper = DutyRateWrapper.GetWrapper(dutyData);
				var rate = ZDecimal.Zero;

				switch (importTariff.UE_DutyComputationCode)
				{
					case ComputationCodeList.Codes.SpecificRateSecondQuantity:
					case ComputationCodeList.Codes.CompoundSpecificAndAdValoremSecondQuantity:
					case ComputationCodeList.Codes.SpecificFunctionalAdValorem:
					case ComputationCodeList.Codes.SpecificCompound:
						rate = rateWrapper.Specific;
						break;
					case ComputationCodeList.Codes.MultipleSpecific:
					case ComputationCodeList.Codes.SpecificPlusCompound:
						rate = rateWrapper.Other;
						break;

					case ComputationCodeList.Codes.SpecificPyrotechnics:
						rate = rateWrapper.Other;
						break;

					case ComputationCodeList.Codes.SpecificSugarJ:
						rate = Math.Max(rateWrapper.Specific - rateWrapper.Advalorem * (100 - dutyData.Quantity3), rateWrapper.Other);
						break;

					case ComputationCodeList.Codes.SpecificSugarK:
						rate = ZDecimal.Zero;
						result = qty2.Round(2);
						break;
				}

				if (rate > 0)
				{
					result = rate <= 1 ? qty2.Round(0) : qty2.Round(2);
				}
			}

			return result;
		}

		public static ZDecimal GetRoundedQuantity3(IFeeCalculationDataProvider dutyData, ZDecimal qty3)
		{
			var importTariff = dutyData.ImportTariff;
			if (IsTaxApplicable(dutyData, () => ComputationCodeList.IsThirdQuantityRequired(dutyData.TaxComputationCode)))
			{
				return qty3.Round(2);
			}

			var result = qty3.Round(0);
			if (importTariff != null && !dutyData.HasTextileCategoryNo)
			{
				var rateWrapper = DutyRateWrapper.GetWrapper(dutyData);
				var rate = ZDecimal.Zero;
				switch (importTariff.UE_DutyComputationCode)
				{
					case ComputationCodeList.Codes.FunctionalAdValorem:
					case ComputationCodeList.Codes.SpecificFunctionalAdValorem:
					case ComputationCodeList.Codes.SpecificCompound:
						rate = rateWrapper.Other;
						break;

					case ComputationCodeList.Codes.CompoundSpecificAdValorem:
						rate = rateWrapper.Specific;
						break;

					case ComputationCodeList.Codes.SpecificSugarJ:
						rate = ZDecimal.Zero;
						result = qty3.Round(2);
						break;
				}

				if (rate > 0)
				{
					result = rate <= 1 ? qty3.Round(0) : qty3.Round(2);
				}
			}

			return result;
		}

		static ZBool IsTaxApplicable(IFeeCalculationDataProvider dutyData, Func<bool> isQuantityRequired)
		{
			return !dutyData.TaxCode.IsEmpty && isQuantityRequired()
				|| dutyData.Tariff.StartsWith("22", StringComparison.OrdinalIgnoreCase);//In Chapter 22 should report decimals and not be rounded
		}
	}
}
