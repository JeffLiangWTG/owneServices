using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class SupplementaryParentTariffIDutyData : IDutyData, IFeeCalculationDataProvider
	{
		public SupplementaryParentTariffIDutyData(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		#region IDutyData Members

		public ZString Tariff
		{
			get { return invoiceLine.US_SupTariff; }
		}

		public USCTariff ImportTariff
		{
			get { return invoiceLine.ImportSupTariff; }
		}

		public ZDate DateForDutyCalculation
		{
			get { return invoiceLine.EffectiveDateForDutyRate; }
		}

		public ZDecimal Quantity1
		{
			get { return RoundedQuantity.GetRoundedQuantity1(this, invoiceLine.US_SupQty1); }
		}

		public ZString UQ1
		{
			get { return invoiceLine.US_SupUQ1; }
		}

		public ZDecimal Quantity2
		{
			get { return RoundedQuantity.GetRoundedQuantity2(this, invoiceLine.US_SupQty2); }
		}

		public ZString UQ2
		{
			get { return invoiceLine.US_SupUQ2; }
		}

		public ZDecimal Quantity3
		{
			get { return RoundedQuantity.GetRoundedQuantity3(this, invoiceLine.US_SupQty3); }
		}

		public ZString UQ3
		{
			get { return invoiceLine.US_SupUQ3; }
		}

		ZDecimal IDutyData.CustomsValue
		{
			get { return GetCustomsValue(invoiceLine).Round(0); }
		}

		ZDecimal IDutyData.SupCustomsValue
		{
			get { return ZDecimal.Zero; }
		}

		ZDecimal GetCustomsValue(JobComInvoiceLine invoiceLine)
		{
			return CustomsValueDeciderForInvoiceLine.GetCustomsValue(invoiceLine, true);
		}

		public ZString SpecialProgramsIndicatorPrimary
		{
			get { return invoiceLine.US_SPI; }
		}

		public ZString SpecialProgramsIndicatorCountry
		{
			get { return invoiceLine.US_SPI; }
		}

		public ZString CountryOfOrigin
		{
			get { return invoiceLine.US_UC_NKCountryOfOrigin; }
		}

		public ZString SpecialProgramsIndicatorSecondary
		{
			get { return invoiceLine.US_SecondarySPI; }
		}

		public ZString SelectedRateType
		{
			get { return invoiceLine.US_SelectedRateType; }
		}

		public CargoWise.EntityFramework.BusinessObjectFactory Factory
		{
			get { return invoiceLine.Factory; }
		}

		public IDutyData ParentTariffLine
		{
			get { return null; }
		}

		public bool IsCottonFeeExemptIndicated
		{
			get { return invoiceLine.IsCottonFeeExemptIndicated; }
		}

		public bool HasCottonCertificate
		{
			get { return invoiceLine.HasCottonCertificate; }
		}

		public ZBool IsSetXLine
		{
			get { return invoiceLine.IsSetXLine; }
		}

		public ZBool IsSetVLine
		{
			get { return invoiceLine.IsSetVLine; }
		}

		public bool IsAMSFeeExempt
		{
			get { return invoiceLine.IsAMSFeeExempt; }
		}

		public bool IsRaspberryFeeExempt
		{
			get { return invoiceLine.IsRaspberryFeeExempt; }
		}

		public ZString EntryType
		{
			get { return invoiceLine.Declaration.US_EntryType; }
		}

		public bool IsClearedInPR
		{
			get { return invoiceLine.Declaration.IsClearedInPR; }
		}

		public bool IsSecondaryTariffLine
		{
			get { return invoiceLine.IsSecondaryTariffLine; }
		}

		public bool IsDomesticMerchandise
		{
			get { return invoiceLine.IsDomesticMerchandise; }
		}

		public ZDecimal? OverriddenTaxRate
		{
			get { return null; }
		}

		public ZString OverriddenTaxRateUQ
		{
			get { return ZString.Empty; }
		}

		ZDecimal IDutyData.ValueForADD
		{
			get { return invoiceLine.ValueForADD.Round(0); }
		}

		ZDecimal IDutyData.ADDDepositRate
		{
			get { return invoiceLine.US_ADDDepositRate; }
		}

		ZDecimal IDutyData.ValueForCVD
		{
			get { return invoiceLine.ValueForCVD.Round(0); }
		}

		ZDecimal IDutyData.CVDDepositRate
		{
			get { return invoiceLine.US_CVDDepositRate; }
		}

		ZDecimal IDutyData.ADDQuantity
		{
			get { return invoiceLine.US_ADDQty; }
		}

		ZString IDutyData.ADDCaseRateTypeQualifier
		{
			get { return invoiceLine.US_ADDDepositRateIndicator; }
		}

		ZDecimal IDutyData.CVDQuantity
		{
			get { return invoiceLine.US_CVDQty; }
		}

		ZString IDutyData.CVDCaseRateTypeQualifier
		{
			get { return invoiceLine.US_CVDDepositRateIndicator; }
		}

		ZDecimal? IDutyData.ADDutyManual
		{
			get
			{
				ZDecimal? result = null;

				if (invoiceLine.IsADDManual)
				{
					result = invoiceLine.US_ADDuty;
				}

				return result;
			}
		}

		ZDecimal? IDutyData.CVDutyManual
		{
			get
			{
				ZDecimal? result = null;

				if (invoiceLine.IsCVDManual)
				{
					result = invoiceLine.US_CVDuty;
				}

				return result;
			}
		}

		bool IDutyData.HasTextileCategoryNo
		{
			get { return ((IDutyData)invoiceLine).HasTextileCategoryNo; }
		}

		bool IDutyData.IsCombineSecondaryTariffLine
		{
			get { return ((IDutyData)invoiceLine).IsCombineSecondaryTariffLine; }
		}

		IReadOnlyList<ZString> IDutyData.SupTariffs
		{
			get { return ((IDutyData)invoiceLine).SupTariffs; }
		}

		IEnumerable<IDutyData> IDutyData.CombineChildLines
		{
			get { return ((IDutyData)invoiceLine).CombineChildLines; }
		}

		IEnumerable<IDutyData> IDutyData.CombineAllLines
		{
			get { return ((IDutyData)invoiceLine).CombineAllLines; }
		}

		IDutyData IDutyData.CombineParentLine
		{
			get { return invoiceLine.ParentTariffLine; }
		}

		#endregion

		#region IFeeCalculationDataProvider Members

		bool IFeeCalculationDataProvider.IsACS
		{
			get => ((IFeeCalculationDataProvider)invoiceLine).IsACS;
		}

		bool IFeeCalculationDataProvider.IsFeeOverriden(string feeCode)
		{
			return invoiceLine.IsFeeOverriden(feeCode);
		}

		void IFeeCalculationDataProvider.SetFeeResult(string feeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			ZDecimal existingAmount = invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(feeCode);

			if (existingAmount + amount != existingAmount)
			{
				invoiceLine.FeeCusCodes.UpdateOrAddCharge(feeCode, existingAmount + amount);
			}
		}

		ZString IFeeCalculationDataProvider.GetSelectedRateType(string feeCode)
		{
			FeeCusCodeData fee = invoiceLine.FeeCusCodes.GetFirstElementHaving(feeCode);

			return fee != null ? fee.CY_SelectedRateType : ZString.Empty;
		}

		ZDateTime IFeeCalculationDataProvider.DateForMPFCalculation
		{
			get { return invoiceLine.DateForMPFCalculation; }
		}

		ZString IFeeCalculationDataProvider.TaxCode
		{
			get { return ZString.Empty; }
		}

		ZString IFeeCalculationDataProvider.TaxComputationCode
		{
			get { return ZString.Empty; }
		}

		ZString IFeeCalculationDataProvider.TaxRateType
		{
			get { return ZString.Empty; }
		}

		ZDecimal IFeeCalculationDataProvider.TaxRateQuantity
		{
			get { return ZDecimal.Zero; }
		}

		ZDecimal IFeeCalculationDataProvider.DairyQty
		{
			get { return ZDecimal.Zero; }
		}

		ZString IFeeCalculationDataProvider.VisaNumber
		{
			get
			{
				if (invoiceLine != null)
				{
					return invoiceLine.US_VisaNo;
				}
				return ZString.Empty;
			}
		}

		IEnumerable<IDutyData> IFeeCalculationDataProvider.SecondaryLines
		{
			get
			{
				if (!invoiceLine.HasEmptySupTariff && !invoiceLine.IsSecondaryTariffLine)
				{
					yield return invoiceLine;

					foreach (JobComInvoiceLine secondaryLine in invoiceLine.SecondaryTariffLines)
					{
						if (!secondaryLine.HasEmptySupTariff)
						{
							yield return new SupplementaryParentTariffIDutyData(secondaryLine);
						}

						yield return secondaryLine;
					}
				}
			}
		}

		#endregion
	}

	static class CustomsValueDeciderForInvoiceLine
	{
		public static ZDecimal GetCustomsValue(IInvoiceLine invoiceLine, bool forSupplementaryTariffLine, bool isComesIDutyData = false, bool takeAddInfoCustomsValue = false, bool forSupplementaryAdditionalTariffLine = false, bool isParentLineSupAdditionalTariffLine = false)
		{
			var totalOriginalGoodsValueInUSD = invoiceLine.TotalOriginalGoodsValueInUSD;
			var customsValue = takeAddInfoCustomsValue ? invoiceLine.US_CustomsValue : (ZDecimal)(invoiceLine.JI_CustomsValue - totalOriginalGoodsValueInUSD);

			var result = customsValue;

			if (customsValue < 0)
			{
				customsValue = 0m;
			}

			var idutyData = invoiceLine as IDutyData;
			if (idutyData.IsCombinedLine() && !invoiceLine.IsVChildLine && !invoiceLine.IsVParentLine)
			{
				if (isComesIDutyData || forSupplementaryTariffLine)
				{
					if (invoiceLine.IsSetVLine && (forSupplementaryAdditionalTariffLine || idutyData.SupTariffs.Count == 1))
					{
						var supplementaryTariff = forSupplementaryAdditionalTariffLine ? invoiceLine.ImportSupAdditionalTariff1 : invoiceLine.ImportSupTariff;
						if (supplementaryTariff != null)
						{
							if (IsTariffValidFor98GoodsValue(supplementaryTariff.UE_Tariff))
							{
								result = totalOriginalGoodsValueInUSD;
							}
							else if (supplementaryTariff.IsValueToBeDeclaredInAlternateTariff(invoiceLine.EffectiveDateForDutyRate))
							{
								result = ZDecimal.Zero;
							}
						}
					}
					else
					{
						if (forSupplementaryAdditionalTariffLine)
						{
							if (idutyData.SupTariffs.Any(supTariff => Chapter98Helper.Is98Tariff(supTariff)))
							{
								result = totalOriginalGoodsValueInUSD;
							}
							else if (idutyData.SupTariffs.Any(supTariff => Chapter98Helper.Is99Tariff(supTariff)))
							{
								result = ZDecimal.Zero;
							}
						}
						else
						{
							if (idutyData.SupTariffs.Any(supTariff => Chapter98Helper.Is99Tariff(supTariff)))
							{
								result = ZDecimal.Zero;
							}
							else if (idutyData.SupTariffs.Any(supTariff => Chapter98Helper.Is98Tariff(supTariff)))
							{
								result = totalOriginalGoodsValueInUSD;
							}
						}
					}
				}
				else
				{
					if (invoiceLine.IsSetXLine || invoiceLine.IsSetVLine)
					{
						if (ShouldBeDeclaredAtParent(invoiceLine.ImportSupTariff, invoiceLine.EffectiveDateForDutyRate) || ShouldBeDeclaredAtParent(invoiceLine.ImportSupAdditionalTariff1, invoiceLine.EffectiveDateForDutyRate))
						{
							result = ZDecimal.Zero;
						}
					}
					else
					{
						result = customsValue;
					}
				}
			}
			else
			{
				var supplementaryTariffToCheck = invoiceLine.ImportSupTariff;
				if (invoiceLine.ImportSupAdditionalTariff1 is USCTariff supAdditionalTariff)
				{
					if (!forSupplementaryTariffLine || forSupplementaryAdditionalTariffLine)
					{
						supplementaryTariffToCheck = supAdditionalTariff;
					}
				}

				result = GetCustomsValue(invoiceLine.JI_Tariff, supplementaryTariffToCheck, totalOriginalGoodsValueInUSD, customsValue, forSupplementaryTariffLine, invoiceLine.EffectiveDateForDutyRate, isParentLineSupAdditionalTariffLine);
			}

			return result;
		}

		public static ZDecimal GetCustomsValueForDDP(JobComInvoiceLine invoiceLine, bool forSupplementaryTariffLine)
		{
			ZDecimal customsValue = invoiceLine.JI_CustomsValue - invoiceLine.TotalOriginalGoodsValueInUSD;

			if (customsValue < 0)
			{
				customsValue = 0m;
			}

			return GetCustomsValueForDDP(invoiceLine, forSupplementaryTariffLine, customsValue);
		}

		public static ZDecimal GetCustomsValueForDDP(JobComInvoiceLine invoiceLine, bool forSupplementaryTariffLine, ZDecimal newCustomsValue)
		{
			var supplementaryTariffToCheck = invoiceLine.ImportSupTariff;
			if (invoiceLine.ImportSupAdditionalTariff1 is USCTariff supAdditionalTariff)
			{
				supplementaryTariffToCheck = supAdditionalTariff;
			}

			return GetCustomsValue(invoiceLine.JI_Tariff, supplementaryTariffToCheck, invoiceLine.TotalOriginalGoodsValueInUSD, newCustomsValue, forSupplementaryTariffLine, invoiceLine.EffectiveDateForDutyRate);
		}

		public static ZDecimal GetReconOriginalCustomsValue(JobComInvoiceLine invoiceLine, bool forSupplementaryTariffLine)
		{
			var result = ZDecimal.Zero;

			if (invoiceLine.IsCombinedLine())
			{
				if (forSupplementaryTariffLine)
				{
					if (Chapter98Helper.Is99Tariff(invoiceLine.US_R_OrigSupTariff))
					{
						result = ZDecimal.Zero;
					}
					else if (Chapter98Helper.Is98Tariff(invoiceLine.US_R_OrigSupTariff))
					{
						result = invoiceLine.US_R_Orig98Value;
					}
				}
				else
				{
					result = invoiceLine.US_R_OrigCV;
				}
			}
			else
			{
				result = GetCustomsValue(invoiceLine.US_R_OrigTariff, invoiceLine.OriginalImportSupTariff, invoiceLine.US_R_Orig98Value, invoiceLine.US_R_OrigCV, forSupplementaryTariffLine, invoiceLine.EffectiveDateForDutyRate);
			}

			return result;
		}

		public static ZDecimal GetCustomsValue(ZString mainTariff, USCTariff supplementaryTariff, ZDecimal original98Value, ZDecimal customsValue, bool forSupplementaryTariffLine, ZDate dutyDate, bool isParentLineSupAdditionalTariffLine = false)
		{
			ZDecimal result = customsValue;

			if (supplementaryTariff != null)
			{
				if (forSupplementaryTariffLine)
				{
					if (IsTariffValidFor98GoodsValue(supplementaryTariff.UE_Tariff))
					{
						result = original98Value;
					}
					else if (isParentLineSupAdditionalTariffLine || supplementaryTariff.IsValueToBeDeclaredInAlternateTariff(dutyDate))
					{
						result = ZDecimal.Zero;
					}
				}
				else // non-98/99 line
				{
					// value should be declared at its parent
					if (ShouldBeDeclaredAtParent(supplementaryTariff, dutyDate))
					{
						result = ZDecimal.Zero;
					}
				}
			}
			else if (forSupplementaryTariffLine)
			{
				result = ZDecimal.Zero;
			}

			if (mainTariff.StartsWith("9801", System.StringComparison.OrdinalIgnoreCase) && !forSupplementaryTariffLine && supplementaryTariff == null)
			{
				result = customsValue + original98Value;
			}

			return result;
		}

		public static bool ShouldBeDeclaredAtParent(USCTariff tariff, ZDateTime effectiveDate)
		{
			return tariff != null && !IsTariffValidFor98GoodsValue(tariff.UE_Tariff) && !tariff.IsValueToBeDeclaredInAlternateTariff(effectiveDate);
		}

		public static bool IsTariffValidFor98GoodsValue(ZString tariffNumber)
		{
			return tariffNumber.StartsWith("9801")
				|| tariffNumber.StartsWith("9802")
				|| tariffNumber.StartsWith("982205");
		}

		public static ZDecimal GetCustomsValue(IEntryLine line)
		{
			var customsValue = ZDecimal.Zero;
			if (line.InvoiceLines.Count() > 1) //Multiple lines merged into one entry line
			{
				customsValue = line.InvoiceLines.Sum(x => x.US_CustomsValue);
			}
			else
			{
				customsValue = line.RandomLine.US_CustomsValue;
			}

			return customsValue;
		}
	}
}
