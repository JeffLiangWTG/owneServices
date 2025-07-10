using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IFeeCalculationDataProvider : IDutyData
	{
		bool IsACS { get; }

		bool IsFeeOverriden(string feeCode);

		void SetFeeResult(string feeCode, decimal amount, FeeCalculationInternalData ddpData);

		ZString GetSelectedRateType(string feeCode);

		ZDateTime DateForMPFCalculation { get; }

		/// <summary>
		/// If it is not overriden, return null
		/// </summary>
		ZDecimal? OverriddenTaxRate { get; }
		ZString OverriddenTaxRateUQ { get; }
		ZString TaxCode { get; }
		ZString TaxRateType { get; }
		ZString TaxComputationCode { get; }
		ZDecimal TaxRateQuantity { get; }

		ZDecimal DairyQty { get; }

		ZString VisaNumber { get; }
		IEnumerable<IDutyData> SecondaryLines { get; }
	}

	public static class IFeeCalculationDataProviderExtensionMethods
	{
		public static ZString GetTaxOrFeeRate(this IFeeCalculationDataProvider line, ZString feeCode, bool taxDeferred)
		{
			ZString result = ZString.Empty;

			ZDecimal? overriddenTaxRate = line.OverriddenTaxRate;

			if (CusFeeCodeConstants.IsExciseTax(feeCode) && overriddenTaxRate.HasValue)
			{
				result = GetTaxRateWhenOverridden(line, feeCode, overriddenTaxRate.Value, line.OverriddenTaxRateUQ);
			}
			else
			{
				result = line.ImportTariff != null ? line.ImportTariff.GetTaxFeeRateDescription(feeCode, line.GetSelectedRateType(feeCode)) : ZString.Empty;
			}

			if (CusFeeCodeConstants.IsExciseTax(feeCode) && taxDeferred && !result.IsEmpty)
			{
				result = TaxDeferredString + result;
			}

			return result;
		}

		const string TaxDeferredString = "DEF ";

		static ZString GetTaxRateWhenOverridden(IFeeCalculationDataProvider line, ZString feeCode, ZDecimal overriddenAmount, ZString overriddenUQ)
		{
			ZString taxComputationCode = line.TaxComputationCode;
			ZDecimal? overriddenTaxRate = line.OverriddenTaxRate;

			ZString result = ZString.Empty;

			if (taxComputationCode == ComputationCodeList.Codes.SpecificRateFirstQuantity || taxComputationCode == ComputationCodeList.Codes.SpecificRateSecondQuantity)
			{
				ZDecimal amountPerUQ = overriddenAmount;
				ZString uQ = taxComputationCode == ComputationCodeList.Codes.SpecificRateFirstQuantity ? line.UQ1 : line.UQ2;

				result = !uQ.IsEmpty ? AmountPerUnit(amountPerUQ, uQ) : ZString.Empty;
			}
			else if (taxComputationCode == ComputationCodeList.Codes.AdValorem)
			{
				ZDecimal rate = overriddenAmount * 100m;
				result = rate.ToStringTrimZeros() + "%";
			}
			else if (taxComputationCode == ComputationCodeList.CustomComputationCodeForCalculatingIRTax)
			{
				result = AmountPerUnit(overriddenAmount, overriddenUQ);
			}

			return result;
		}

		public static ZString AmountPerUnit(ZDecimal amountPerUQ, ZString uQ)
		{
			ZString result = ZString.Empty;
			if (amountPerUQ < 1)
			{
				ZDecimal amountAsCents = amountPerUQ * 100;

				if (amountAsCents.DecimalPlaces > 0)
				{
					result = amountAsCents.ToString(amountPerUQ.DecimalPlaces).TrimEnd('0') + "c/" + uQ;
				}
				else
				{
					result = amountAsCents.ToString(0) + "c/" + uQ;
				}
			}
			else
			{
				int decimalPlaces = amountPerUQ.DecimalPlaces > 2 ? amountPerUQ.DecimalPlaces : 2;
				result = "$" + amountPerUQ.ToString(decimalPlaces) + "/" + uQ;
			}

			return result;
		}

		public static bool ShouldCottonFeeExemptBeIndicated(this IFeeCalculationDataProvider dataProvider)
		{
			return !dataProvider.IsSetXLine
				&& dataProvider.ImportTariff != null
				&& dataProvider.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton)
				&& !dataProvider.IsCottonFeeExempt(false, false);
		}

		public static bool IsCottonFeeExempt(this IFeeCalculationDataProvider dataProvider, bool ignoreTIBExemptionCondition, bool ignoreCottonExemptionCertificate)
		{
			IDutyData dutyData = dataProvider.ParentTariffLine ?? dataProvider;

			var parentFallbackToChildTariff = dutyData.ImportTariff;

			return DoesTariffRuleMakeExempt(dutyData, parentFallbackToChildTariff)
				|| DoesSPIMakeExempt(dataProvider)
				|| DoesInformalOrTIBMakeExempt(dutyData, ignoreTIBExemptionCondition)
				|| CottonFeeOnParentMakesChildExempt(dataProvider, parentFallbackToChildTariff)
				|| IsCottonExemptionCertificateIndicated(dataProvider, ignoreCottonExemptionCertificate);
		}

		public static bool IsRaspberryFeeExempt(this IFeeCalculationDataProvider dataProvider)
		{
			IDutyData dutyData = dataProvider.ParentTariffLine ?? dataProvider;

			return dutyData.IsRaspberryFeeExempt;
		}

		static bool DoesTariffRuleMakeExempt(IDutyData dutyData, USCTariff tariff)
		{
			return tariff != null && tariff.Applies(TariffRuleList.Codes.CottonFeeExemption, dutyData.DateForDutyCalculation);
		}

		static bool DoesSPIMakeExempt(IDutyData dutyData)
		{
			IDutyData dutyDataToCheck = dutyData.ParentTariffLine ?? dutyData;

			if (dutyDataToCheck.Tariff.StartsWith("98") || dutyDataToCheck.Tariff.StartsWith("99"))//if sup tariff, ZString.Empty is returned
			{
				dutyDataToCheck = dutyData;
			}

			return dutyDataToCheck.SpecialProgramsIndicatorSecondary == SecondarySpecProgIndicatorList.Codes.H;
		}

		static bool DoesInformalOrTIBMakeExempt(IDutyData dutyData, bool ignoreTIBExemptionCondition)
		{
			return EntryTypeList.IsInformal(dutyData.EntryType)
				|| !ignoreTIBExemptionCondition && dutyData.EntryType == EntryTypeList.Codes.TemporaryImportationBond;
		}

		internal static bool CottonFeeOnParentMakesChildExempt(this IFeeCalculationDataProvider dataProvider, USCTariff parentFallbackToChildTariff)
		{
			var childTariff = dataProvider.ImportTariff;

			return dataProvider.IsACS
				&& dataProvider.ParentTariffLine != null
				&& !dataProvider.ParentTariffLine.IsSetXLine
				&& childTariff != null
				&& parentFallbackToChildTariff != null
				&& parentFallbackToChildTariff.UE_Tariff != childTariff.UE_Tariff
				&& parentFallbackToChildTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton);
		}

		static bool IsCottonExemptionCertificateIndicated(IFeeCalculationDataProvider dataProvider, bool ignoreCottonExemptionCertificate)
		{
			return !ignoreCottonExemptionCertificate && (dataProvider.IsAMSFeeExempt || dataProvider.IsCottonFeeExemptIndicated || dataProvider.HasCottonCertificate);
		}
	}

	public interface IFee
	{
		ZString Code { get; set; }

		ZDecimal Amount { get; set; }

		ZString SelectedRateType { get; set; }

		ZBool IsOverridden { get; }

		void Delete();
	}

	public interface IFees : IEnumerable
	{
		IFee GetFeeFor(ZString code);

		IFee AddNew();
	}
}
