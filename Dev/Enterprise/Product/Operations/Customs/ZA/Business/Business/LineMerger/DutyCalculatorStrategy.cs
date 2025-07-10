using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ZA.Business
{
	class DutyCalculatorStrategy : Customs.Business.DutyCalculatorStrategy
	{
		public DutyCalculatorStrategy(JobDeclaration declaration, ValaValueRebateCalculator valaValueRebateCalculator, DutyRebateCalculator dutyRebateCalculator)
			: base(declaration)
		{
			this.valaValueRebateCalculator = valaValueRebateCalculator;
			this.dutyRebateCalculator = dutyRebateCalculator;
		}

		readonly ValaValueRebateCalculator valaValueRebateCalculator;
		readonly DutyRebateCalculator dutyRebateCalculator;

		JobDeclaration Declaration => (JobDeclaration)base.declaration;

		public override void CalculateDuties()
		{
			var fees = new List<CusEntryLineFee>();
			var isImportedFromBLNS = Declaration.Origin?.Country?.IsBLNS ?? false;
			foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					var randomLine = entryLine.RandomLine;
					if (randomLine != null)
					{
						var procedure = randomLine.CusProcedure;
						if (procedure != null)
						{
							var calculateDuty = procedure.ZZ6_CalculateDuty;
							var landedCost = procedure.ZZ6_LandedCost;
							if (calculateDuty || landedCost)
							{
								CalculateDutiesForEntryLine(entryLine, randomLine, procedure);
								CalculateTaxOrFeeForEntryLine(isImportedFromBLNS, entryLine, randomLine);
							}
						}
					}
					fees.AddRange(entryLine.Fees.Cast<CusEntryLineFee>());
				}
			}
			fees.Where(x => x.shouldDeleteIfChargeAmountIsZero && x.CF_ChargeAmount.IsEmpty).DeleteAll();
		}

		void CalculateDutiesForEntryLine(CusEntryLine entryLine, JobComInvoiceLine randomLine, RefCusProcedure procedure)
		{
			var tariff = randomLine.JI_Tariff;

			if (!tariff.IsEmpty)
			{
				var lineVFD = entryLine.CustomsValue.Amount;
				if (randomLine.CusLineTariffDetails.Any(x => x.UniversalTariff?.HasAttribute(UniversalReferenceConstants.TariffAttributes.CostOfRepair) ?? false))
				{
					lineVFD = ((MessageBuilders.ILineLevelInformation)entryLine).ActualPrice;
				}
				var lineVPB = lineVFD;
				var vala100Percent = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.ZAVALA100PERCENT, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);
				var addInfos = valaValueRebateCalculator.CalculateRebatedValueForDuty(entryLine.CL_CustomsValue, entryLine, (entryLine.IsSpecifiedMotorVehicle && !vala100Percent) ? 0.8m : 1.0m);
				lineVFD -= valaValueRebateCalculator.AmountToRebate;

				var universalRateData = new EntryLineUniversalRate(entryLine, lineVFD);
				var dutyCalculator = new DutyCalculator(CanBeNegative, ShouldTruncate, DutyDecimalPlace);
				if (randomLine.UniversalTariff?.IsApplicableForDutyCalculation(procedure) ?? false)
				{
					entryLine.AdditionalInformationCodesActions.Clear();
					entryLine.AdditionalInformationCodesActions.AddRange(addInfos);
					var extraAddInfoActions = dutyCalculator.CalculateAndPopulateDuty(universalRateData, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, tariff, ZString.Empty, 0m, entryLine: entryLine, dutyRebateCalculator: dutyRebateCalculator);
					entryLine.AdditionalInformationCodesActions.AddRange(extraAddInfoActions);
				}

				universalRateData.UpdateCustomsValue(lineVPB);
				dutyCalculator.CalculateOtherDuties(universalRateData, randomLine.CusLineTariffDetails, tariff);

				foreach (KeyValuePair<string, decimal> pair in universalRateData.CountrySpecificValueList.Where(x => universalRateData.CountrySpecificTypeList.Contains(x.Key)))
				{
					entryLine.Fees.AddOrUpdate(pair.Key, pair.Value).MarkDoNotDeleteIfChargeAmountIsZero();
					if (pair.Key == UniversalReferenceConstants.CusTariffCode.Schedule1Part1)
					{
						var dutyPercent = (entryLine.CL_CustomsValue != ZDecimal.Zero) ? (pair.Value / entryLine.CL_CustomsValue) * 100 : 0.0m;
						entryLine.CL_DutyPercent = dutyPercent > 999.99999m ? 999.99999m : dutyPercent; // CL_DutyPercent Column is decimal (8,5)
					}
				}
			}
		}

		static void CalculateTaxOrFeeForEntryLine(bool isImportedFromBLNS, CusEntryLine entryLine, JobComInvoiceLine randomLine)
		{
			var appliedTaxOrFee = randomLine.AppliedTaxAndFee;
			if (appliedTaxOrFee != null)
			{
				var lineVFD = entryLine.CustomsValue.Amount;
				if (randomLine.CusLineTariffDetails.Any(x => x.UniversalTariff?.HasAttribute(UniversalReferenceConstants.TariffAttributes.CostOfRepair) ?? false))
				{
					lineVFD = ((MessageBuilders.ILineLevelInformation)entryLine).ActualPrice;
				}
				var lineAssessmentDate = entryLine.EntryInstruction.CEI_DateForDuty;

				var taxValue = CalculateTaxValue(isImportedFromBLNS, randomLine.CountryOfOrigin, appliedTaxOrFee.ZZF_Value, lineVFD, entryLine.GetDutyAmount(), lineAssessmentDate);
				if (taxValue != 0)
				{
					entryLine.Fees.AddOrUpdate(appliedTaxOrFee.ZZF_Code, ZArchitecture.Core.Utilities.Round(taxValue, 2)).MarkDoNotDeleteIfChargeAmountIsZero();
				}
			}
		}

		internal static decimal CalculateTaxValue(bool isImportedFromBLNS, RefCountry countryOfOrigin, decimal taxRate, decimal lineVFD, decimal customsDuty, ZDateTime lineAssessmentDate)
		{
			ZDecimal taxBase = (lineVFD * GetVFDFraction(isImportedFromBLNS, countryOfOrigin, lineAssessmentDate)) + customsDuty;
			return taxBase.RoundUsingCustomsValueRule() * taxRate;
		}

		//http://www.sars.gov.za/FAQs/Pages/477.aspx
		static decimal GetVFDFraction(bool isImportedFromBLNS, RefCountry countryOfOrigin, ZDateTime lineAssessmentDate)
		{
			bool isOriginBLNS = !IsBLNSVatUplift(lineAssessmentDate) || (countryOfOrigin?.IsBLNS ?? false);

			return isImportedFromBLNS && isOriginBLNS ? 1m : 1.1m;
		}

		static bool IsBLNSVatUplift(ZDateTime lineAssessmentDate)
		{
			var effectiveDate = lineAssessmentDate.IsEmpty ? ZDateTime.Today : lineAssessmentDate;
			return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.ZABLNSVatUplift
				, Core.Constants.CountryCodes.SouthAfrica
				, effectiveDate);
		}

		#region BND Calculation

		internal void CalculateBND()
		{
			if (Declaration.JE_RemovalTransportCode == Core.Constants.TransportModes.Road)
			{
				var isExport = Declaration.IsExport;
				var isImportedFromBLNS = Declaration.Origin?.Country?.IsBLNS ?? false;

				foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
				{
					foreach (var entryLine in entryHeader.MergedLines)
					{
						var randomLine = entryLine.RandomLine;
						if (randomLine != null)
						{
							var procedure = randomLine.CusProcedure;
							if (procedure != null)
							{
								var applicableRateTypes = isExport ? bNDRateTypesForExport : bNDRateTypesForImport;
								if (!procedure?.ZZ6_CalculateDuty ?? true) // Duty not calculated or already calculated (i.e. landed costing) but not compatible
								{
									CalculateBNDForEntryLine(isImportedFromBLNS, applicableRateTypes, entryLine, randomLine, procedure, isExport);
								}
								else
								{
									var bND = entryLine.CustomsDuty + entryLine.VAT;
									entryLine.AdditionalInformationCodes.SetStringValueHavingCodeOrDeleteIfValueEmpty(UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount, ((decimal)(new ZDecimal(bND).RoundUsingCustomsValueRule())).ToString(CultureInfo.CurrentCulture));
								}
							}
						}
					}
				}
			}
		}

		readonly IImmutableList<ZString> bNDRateTypesForImport = new ZString[] {
			Universal.Constants.RateTypes.AdValoremExcise,
			Universal.Constants.RateTypes.Excise,
			Universal.Constants.RateTypes.Levy
		}.ToImmutableList();

		readonly IImmutableList<ZString> bNDRateTypesForExport = new ZString[] {
			Universal.Constants.RateTypes.AdValoremExcise
		}.ToImmutableList();

		void CalculateBNDForEntryLine(bool isImportedFromBLNS, IImmutableList<ZString> applicableRateTypes, CusEntryLine entryLine, JobComInvoiceLine randomLine, RefCusProcedure procedure, ZBool isExport)
		{
			var tariff = randomLine.JI_Tariff;
			if (!tariff.IsEmpty && procedure != null)
			{
				var preference = isExport ? new ZString(UniversalReferenceConstants.PrimaryPreference.Standard) : randomLine.JI_PrimaryPreference;

				var universalRateData = new EntryLineUniversalRate(entryLine, entryLine.CL_CustomsValue, preference);
				var dutyCalculator = new DutyCalculator(CanBeNegative, ShouldTruncate, DutyDecimalPlace);
				const string tariffType = UniversalReferenceConstants.CusTariffCode.Schedule1Part1;
				dutyCalculator.CalculateAndPopulateDuty(universalRateData, tariffType, tariff, ZString.Empty, ZDecimal.Zero, entryLine: entryLine, dutyRebateCalculator: dutyRebateCalculator);

				var tariffDetails = new CusLineTariffDetailCollection<CusLineTariffDetail>(randomLine);
				GetTariffsApplicableForBND(applicableRateTypes, procedure.Factory, tariffType, tariff, universalRateData.DateOfValuation, tariffDetails);
				dutyCalculator.CalculateOtherDuties(universalRateData, tariffDetails, tariff);

				var customsDutyForBND = universalRateData.CountrySpecificValueList.Sum(x => x.Value);
				var bND = customsDutyForBND + CalculateBNDTaxOrFeeForEntryLine(isImportedFromBLNS, entryLine, randomLine, customsDutyForBND, isExport, universalRateData.DateOfValuation);

				entryLine.AdditionalInformationCodes.SetStringValueHavingCodeOrDeleteIfValueEmpty(UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount, ((decimal)(new ZDecimal(bND).RoundUsingCustomsValueRule())).ToString(CultureInfo.CurrentCulture));
				tariffDetails.RemoveAndDeleteAll();
			}
		}

		static void GetTariffsApplicableForBND(IImmutableList<ZString> applicableRateTypes, BusinessObjectFactory factory, string tariffType, string tariff, ZDateTime dateOfValuation, CusLineTariffDetailCollection<CusLineTariffDetail> tariffDetails)
		{
			foreach (var relatedTariff in new TariffView.Loader(factory).GetEffectiveChildTariffs(Core.Constants.CountryCodes.SouthAfrica, tariffType, tariff, dateOfValuation))
			{
				if (relatedTariff != null)
				{
					var relatedTariffRateType = relatedTariff.Rates.FirstOrDefault()?.ZZ2_ZZR_RateTypeCode ?? ZString.Empty;
					if (!relatedTariffRateType.IsEmpty && applicableRateTypes.Contains(relatedTariffRateType))
					{
						var tariffDetail = tariffDetails.AddNew();
						using (tariffDetail.InvoiceLine.SuspendCusLineTariffDetailDefaulting())
						{
							tariffDetail.BZ_Type = relatedTariff.ZZ1_ZZI_TariffTypeCode;
							tariffDetail.BZ_Tariff = relatedTariff.ZZ1_TariffCode.Left(CusLineTariffDetail.Schema.BZ_TariffMaxLength);
						}
					}
				}
			}
		}

		static ZDecimal CalculateBNDTaxOrFeeForEntryLine(bool isImportedFromBLNS, CusEntryLine entryLine, JobComInvoiceLine randomLine, decimal bNDCustomsDuty, ZBool isExport, ZDateTime dateOfValuation)
		{
			var result = ZDecimal.Zero;

			var appliedTaxOrFee = GetAppliedTaxAndFee(randomLine, isExport, dateOfValuation);
			if (appliedTaxOrFee != null)
			{
				var lineVFD = entryLine.CustomsValue.Amount;

				var taxValue = DutyCalculatorStrategy.CalculateTaxValue(isImportedFromBLNS, randomLine.CountryOfOrigin, appliedTaxOrFee.ZZF_Value, lineVFD, bNDCustomsDuty, dateOfValuation);
				if (taxValue != 0)
				{
					result = ZArchitecture.Core.Utilities.Round(taxValue, 2);
				}
			}

			return result;
		}

		static RefCusTaxOrFee GetAppliedTaxAndFee(JobComInvoiceLine randomLine, ZBool isExport, ZDateTime dateOfValuation)
		{
			var taxOrFee = randomLine.AppliedTaxAndFee;
			var taxCode = taxOrFee?.ZZF_Code ?? ZString.Empty;

			if (isExport)
			{
				var relatedTariff = new TariffView.Loader(randomLine.Factory).LoadLatestCachedTariff(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, randomLine.JI_Tariff);

				if (relatedTariff != null && relatedTariff.ZZ1_ZZF_NKTaxOrFeeCode != taxCode)
				{
					var loader = new RefCusTaxOrFee.Loader(randomLine.Factory);
					taxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.SouthAfrica, relatedTariff.ZZ1_ZZF_NKTaxOrFeeCode, dateOfValuation);
				}
			}

			return taxOrFee;
		}

		#endregion
	}
}
