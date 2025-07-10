using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.DDPDisbursementCalculation;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public interface IDutyData
	{
		ZString Tariff { get; }
		USCTariff ImportTariff { get; }
		ZDate DateForDutyCalculation { get; }
		ZDecimal Quantity1 { get; }
		ZString UQ1 { get; }
		ZDecimal Quantity2 { get; }
		ZString UQ2 { get; }
		ZDecimal Quantity3 { get; }
		ZString UQ3 { get; }
		ZDecimal CustomsValue { get; }
		ZDecimal SupCustomsValue { get; }
		ZString SpecialProgramsIndicatorPrimary { get; }
		ZString SpecialProgramsIndicatorCountry { get; }
		ZString CountryOfOrigin { get; }
		ZString SpecialProgramsIndicatorSecondary { get; }
		ZString SelectedRateType { get; }
		BusinessObjectFactory Factory { get; }

		ZDecimal ValueForADD { get; }
		ZDecimal ADDDepositRate { get; }
		ZString ADDCaseRateTypeQualifier { get; }
		ZDecimal ADDQuantity { get; }
		ZDecimal? ADDutyManual { get; }

		ZDecimal ValueForCVD { get; }
		ZDecimal CVDDepositRate { get; }
		ZString CVDCaseRateTypeQualifier { get; }
		ZDecimal CVDQuantity { get; }
		ZDecimal? CVDutyManual { get; }

		IDutyData ParentTariffLine { get; }
		bool IsCottonFeeExemptIndicated { get; }
		bool HasCottonCertificate { get; }
		ZBool IsSetXLine { get; }
		ZBool IsSetVLine { get; }
		bool IsAMSFeeExempt { get; }
		bool IsRaspberryFeeExempt { get; }
		ZString EntryType { get; }
		bool IsClearedInPR { get; }
		bool IsSecondaryTariffLine { get; }

		bool IsDomesticMerchandise { get; }
		bool HasTextileCategoryNo { get; }

		bool IsCombineSecondaryTariffLine { get; }
		IEnumerable<IDutyData> CombineChildLines { get; }
		IReadOnlyList<ZString> SupTariffs { get; }
		IDutyData CombineParentLine { get; }
		IEnumerable<IDutyData> CombineAllLines { get; }
	}

	public static class IDutyDataExtensionMethod
	{
		public enum TariffTypeForDDP { None, RegularTariff, SupTariff, AdditionalTariff1, AdditionalTariff2, AdditionalTariff3, AdditionalTariff4, AdditionalTariff5 }

		public static List<JobComInvoiceLine> GetCombinedLines(this JobComInvoiceLine invoiceLine)
		{
			var result = new List<JobComInvoiceLine>();
			var parentTariffLine = invoiceLine.ParentTariffLine ?? invoiceLine;
			if (parentTariffLine != null)
			{
				result = parentTariffLine.ChildLines.ToList();
				result.Insert(0, parentTariffLine);
			}
			return result;
		}

		public static List<IEntryLineOrInvoiceLineDutyData> GetCombinedDutyDataListForDDPCalculation(this JobComInvoiceLine invoiceLine, Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> dutyFeeCalculationResult, Dictionary<JobComInvoiceLine, CustomsValues> customsValues, bool shouldCalculateMPF)
		{
			var result = new List<IEntryLineOrInvoiceLineDutyData>();
			var combineLines = invoiceLine.GetCombinedLines();
			foreach (var line in combineLines)
			{
				result.AddRange(line.GetDutyDataListFromInvoiceLine(dutyFeeCalculationResult, customsValues, shouldCalculateMPF, TariffTypeForDDP.None));
			}

			return result;
		}

		public static List<IEntryLineOrInvoiceLineDutyData> GetDutyDataListFromInvoiceLine(this JobComInvoiceLine invoiceLine, Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> dutyFeeCalculationResult, Dictionary<JobComInvoiceLine, CustomsValues> customsValues, bool shouldCalculateMPF, TariffTypeForDDP tariffTypeToExclude)
		{
			var result = new List<IEntryLineOrInvoiceLineDutyData>();

			void AddDutyDataIntoListIfTariffIsValid(JobComInvoiceLine line, ZString tariffNumber, USCTariff importTariff, TariffTypeForDDP tariffType)
			{
				if (!tariffNumber.IsEmpty && importTariff != null && tariffType != tariffTypeToExclude)
				{
					if (tariffType == TariffTypeForDDP.AdditionalTariff1 || tariffType == TariffTypeForDDP.AdditionalTariff2 || tariffType == TariffTypeForDDP.AdditionalTariff3 || tariffType == TariffTypeForDDP.AdditionalTariff4 || tariffType == TariffTypeForDDP.AdditionalTariff5)
					{
						result.Add(new DDPDisbursementSupAdditionalDutyData(line, tariffNumber, importTariff, dutyFeeCalculationResult, customsValues, tariffType, shouldCalculateMPF));
					}
					else if (tariffType == TariffTypeForDDP.SupTariff)
					{
						result.Add(new DDPDisbursementSupDutyData(line, dutyFeeCalculationResult, customsValues, shouldCalculateMPF));
					}
					else if (tariffType == TariffTypeForDDP.RegularTariff)
					{
						result.Add(new DDPDisbursementLineDutyData(line, dutyFeeCalculationResult, customsValues, shouldCalculateMPF));
					}
				}
			}

			AddDutyDataIntoListIfTariffIsValid(invoiceLine, invoiceLine.US_SupAdditionalTariff1, invoiceLine.ImportSupAdditionalTariff1, TariffTypeForDDP.AdditionalTariff1);
			AddDutyDataIntoListIfTariffIsValid(invoiceLine, invoiceLine.US_SupAdditionalTariff2, invoiceLine.ImportSupAdditionalTariff2, TariffTypeForDDP.AdditionalTariff2);
			AddDutyDataIntoListIfTariffIsValid(invoiceLine, invoiceLine.US_SupAdditionalTariff3, invoiceLine.ImportSupAdditionalTariff3, TariffTypeForDDP.AdditionalTariff3);
			AddDutyDataIntoListIfTariffIsValid(invoiceLine, invoiceLine.US_SupAdditionalTariff4, invoiceLine.ImportSupAdditionalTariff4, TariffTypeForDDP.AdditionalTariff4);
			AddDutyDataIntoListIfTariffIsValid(invoiceLine, invoiceLine.US_SupAdditionalTariff5, invoiceLine.ImportSupAdditionalTariff5, TariffTypeForDDP.AdditionalTariff5);
			AddDutyDataIntoListIfTariffIsValid(invoiceLine, invoiceLine.US_SupTariff, invoiceLine.ImportSupTariff, TariffTypeForDDP.SupTariff);
			AddDutyDataIntoListIfTariffIsValid(invoiceLine, invoiceLine.JI_Tariff, invoiceLine.ImportTariff, TariffTypeForDDP.RegularTariff);

			return result;
		}

		public static DDPDisbursementDutyDataBase GetDDPDisbursementDutyDataForCalculation(this JobComInvoiceLine invoiceLine, Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> dutyFeeCalculationResult, Dictionary<JobComInvoiceLine, CustomsValues> customsValues, bool shouldCalculateMPF)
		{
			DDPDisbursementDutyDataBase dutyData = null;
			if (invoiceLine.ImportSupAdditionalTariff1 != null)
			{
				dutyData = new DDPDisbursementSupAdditionalDutyData(invoiceLine, invoiceLine.US_SupAdditionalTariff1, invoiceLine.ImportSupAdditionalTariff1, dutyFeeCalculationResult, customsValues, TariffTypeForDDP.AdditionalTariff1, shouldCalculateMPF);
			}
			else if (invoiceLine.ImportSupTariff != null)
			{
				dutyData = new DDPDisbursementSupDutyData(invoiceLine, dutyFeeCalculationResult, customsValues, shouldCalculateMPF);
			}
			else
			{
				dutyData = new DDPDisbursementLineDutyData(invoiceLine, dutyFeeCalculationResult, customsValues, shouldCalculateMPF);
			}

			return dutyData;
		}

		public static bool IsCombinedLine(this IDutyData line)
		{
			return line != null && Chapter98Helper.GetCombineParentLine(line) != null;
		}

		public static bool IsNormalTariffLine(this IDutyData line)
		{
			return !line.Tariff.IsEmpty && !line.SupTariffs.Contains(line.Tariff);
		}

		public static bool IsSupTariffLine(this IDutyData line)
		{
			return !line.Tariff.IsEmpty && line.SupTariffs.Contains(line.Tariff);
		}

		public static ZString GetEffectiveSPIForDutyCalculation(this IDutyData dutyData, ZString spiCode)
		{
			var countryOfOrigin = dutyData.Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, dutyData.CountryOfOrigin);
			return UniversalReferenceDataHelper.GetEffectiveSPIForDutyCalculation(dutyData.Factory, spiCode, dutyData.DateForDutyCalculation, countryOfOrigin);
		}

		public static bool ShouldHaveMPFOrInformalFee(this IDutyData dataProvider, Func<IDutyData, bool> shouldHaveFeeCore)
		{
			bool result;

			IDutyData parentLine = dataProvider.ParentTariffLine;

			if (!dataProvider.IsSetVLine && parentLine != null && !Chapter98Helper.ShouldNotExemptMPFFor98(parentLine.Tariff) && !dataProvider.IsCombinedLine())
			{
				result = shouldHaveFeeCore(dataProvider.ParentTariffLine);
			}
			else
			{
				result = shouldHaveFeeCore(dataProvider);
			}

			return result;
		}

		public static ZDecimal GetEffectiveCustomsValue(this IDutyData dutyData)
		{
			return dutyData.SupCustomsValue > 0 ? dutyData.SupCustomsValue : dutyData.CustomsValue;
		}
	}

	public interface IEntryLineOrInvoiceLineDutyData : IDutyData
	{
		ZGuid PK { get; }
		bool IsDutyFreeSPIClaimed { get; }
		bool IsRecon { get; }
		ZDecimal TotalCustomsValueIncludingSecondaryLines { get; }

		ZString CalculateException { get; set; }

		IEnumerable<IEntryLineOrInvoiceLineDutyData> SecondaryLines { get; }

		//Sets Duty rate as well as duty amount
		void SetDutyResult(IDutyResult dutyResult);

		void SetDutyFeeChargeAmount(string chargeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData);

		ZDecimal GetDutyFeeChargeAmount(string chargeCode);

		/// <summary>
		/// For derived duty calculation, at the end of calculation, parent line will have the sum of CV from child lines and all children will have zero CV.
		/// While calculation happens, this value does not get used. Just for record only.
		/// </summary>
		void StoreAdjustedDerivedCustomsValue(ZDecimal derivedCustomsValue);

		/// <summary>
		/// For derived duty calculation, all child invoice lines should point to an entry line with max duty amount. The remaining entry lines are to be deleted.
		/// </summary>
		void UpdateInvoiceLinesLinkToEntryLineForDerivedDutyCalculation(ZGuid newEntryLinePK);

		IEnumerable<IFeeCalculationDataProvider> FeeDataProviders { get; }

		void RollUpFees(IDutyDataLineHeader entry);
		void UpdateLineAndHeaderFeeAmountLessThanThreshold(ZString feeType, ZDecimal thresholdAmount);

		bool IsDutyOverridden { get; }
		bool HasMPF { get; set; }
		bool IsMPFOverridden { get; }

		IEntryLineOrInvoiceLineDutyData ParentLine { get; }
		IEnumerable<IDutyData> ChildLines { get; }
	}

	public static class IEntryLineOrInvoiceLineDutyDataExtensionMethod
	{
		public static IEnumerable<IEntryLineOrInvoiceLineDutyData> GetCombineLines(this IEntryLineOrInvoiceLineDutyData line)
		{
			var result = new List<IEntryLineOrInvoiceLineDutyData>();
			if (line.IsCombinedLine())
			{
				var parentLine = line.ParentLine ?? line;
				result = parentLine?.SecondaryLines.Where(x => !x.Tariff.IsEmpty).ToList() ?? new List<IEntryLineOrInvoiceLineDutyData>();
				if (result.Count > 0)
				{
					result.Add(parentLine);
				}
			}

			return result;
		}

		public static ZDecimal GetADDAdjustValueForCombineLines(this IEntryLineOrInvoiceLineDutyData line)
		{
			var adjustADD = ZDecimal.Zero;
			var chapter98TariffLine = line.GetCombineLines().FirstOrDefault(x => Chapter98Helper.Is98Tariff(x.Tariff));
			if (chapter98TariffLine != null)
			{
				adjustADD = chapter98TariffLine.ValueForADD;
			}
			return adjustADD;
		}

		public static ZDecimal GetCVDAdjustValueForCombineLines(this IEntryLineOrInvoiceLineDutyData line)
		{
			var adjustCVD = ZDecimal.Zero;
			var chapter98TariffLine = line.GetCombineLines().FirstOrDefault(x => Chapter98Helper.Is98Tariff(x.Tariff));
			if (chapter98TariffLine != null)
			{
				adjustCVD = chapter98TariffLine.ValueForCVD;
			}
			return adjustCVD;
		}

		public static bool HasBothSupTariffAndNormalTariff(this IEntryLineOrInvoiceLineDutyData line)
		{
			return !line.Tariff.IsEmpty && line.SupTariffs.Any(supTariff => !supTariff.IsEmpty && supTariff != line.Tariff);
		}
	}

	/// <summary>
	/// CusEntryHeader or ReconOriginalEntryHeader
	/// </summary>
	public interface IDutyDataLineHeader
	{
		bool CalculateChangedLinesOnly { get; }
		ZDecimal OriginalTotalCV { get; }
		bool AreDutyFeeKnownAndImported { get; }
		bool IsHMFApplicable { get; }
		bool IsInformalFeeApplicable { get; }
		bool IsDutiableMailFeeApplicable { get; }
		bool IsHMFDeMinimisApplicable { get; }
		ZDateTime DateForMPFCalculation { get; }
		ZDateTime DateForFeeCalculation { get; }

		bool IsCottonFeeDeMinimusApplicable { get; }
		ZDecimal? OverridenTotalMPFPayable { get; }

		IEnumerable<IEntryLineOrInvoiceLineDutyData> DutyDataLines { get; }

		void OnCalculating();
		void DeleteDetachedEntryLines();
		void UpdateAfterHMFDeMinimusRuleApplied();
		IFees FeeAndCharges { get; }

		bool DoesMPFSurchargeApply { get; }
		BusinessObjectFactory Factory { get; }
	}

	/// <summary>
	/// JobDeclaration Or ReconDeclaration
	/// </summary>
	public interface IDutyDataLineHeaderProvider
	{
		IEnumerable<IDutyDataLineHeader> EntriesToCalculateDutyFeeTax { get; }

		BusinessObjectFactory Factory { get; }

		bool IsCustomsChargeRelevantForDecType(string chargeCode);

		ZDecimal? OverridenTotalMPFPayable { get; }
	}
}
