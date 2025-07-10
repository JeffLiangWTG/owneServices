using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// In the context of Recon
	/// </summary>
	public class ReconCurrentDutyData : IEntryLineOrInvoiceLineDutyData, IFeeCalculationDataProvider
	{
		public ReconCurrentDutyData(JobComInvoiceLine line, ZDateTime dateForMPFCalculation)
		{
			this.dutyData = Argument.NotNull(line, nameof(line));
			this.line = line;
			this.dateForMPFCalculation = dateForMPFCalculation;
		}

		readonly JobComInvoiceLine line;
		readonly IDutyData dutyData;
		readonly ZDateTime dateForMPFCalculation;

		#region IEntryLineOrInvoiceLineDutyData Members

		BusinessObjectFactory IDutyData.Factory
		{
			get { return line.Factory; }
		}

		ZGuid IEntryLineOrInvoiceLineDutyData.PK
		{
			get { return line.PK; }
		}

		bool IEntryLineOrInvoiceLineDutyData.IsRecon
		{
			get { return line.IsRecon; }
		}

		public USCTariff ImportTariff
		{
			get { return line.ImportTariff; }
		}

		public bool IsDutyFreeSPIClaimed
		{
			get { return line.IsDutyFreeSPIClaimed; }
		}

		public ZDecimal TotalCustomsValueIncludingSecondaryLines
		{
			get
			{
				ZDecimal result = GetCustomsValue(line).Round(0);

				foreach (JobComInvoiceLine childLine in line.SecondaryTariffLines)
				{
					result += GetCustomsValue(childLine).Round(0);
				}

				return result;
			}
		}

		ZString IEntryLineOrInvoiceLineDutyData.CalculateException
		{
			get => line.TariffCalculateExceptionMessage;
			set => line.TariffCalculateExceptionMessage = value;
		}

		public ZDecimal ValueForADD
		{
			get { return ZDecimal.Zero; }//No ADD or CVD involved with Recon
		}

		public ZDecimal ADDDepositRate
		{
			get { return ZDecimal.Zero; }//No ADD or CVD involved with Recon
		}

		public ZDecimal ValueForCVD
		{
			get { return ZDecimal.Zero; }//No ADD or CVD involved with Recon
		}

		public ZDecimal CVDDepositRate
		{
			get { return ZDecimal.Zero; }//No ADD or CVD involved with Recon
		}

		ZDecimal IDutyData.ADDQuantity
		{
			get { return ZDecimal.Zero; }// No ADD for Recon
		}

		ZString IDutyData.ADDCaseRateTypeQualifier
		{
			get { return ZString.Empty; }// No ADD for Recon
		}

		ZDecimal IDutyData.CVDQuantity
		{
			get { return ZDecimal.Zero; }// No CVD for Recon
		}

		ZString IDutyData.CVDCaseRateTypeQualifier
		{
			get { return ZString.Empty; }// No CVD for Recon
		}

		ZDecimal? IDutyData.ADDutyManual
		{
			get { return null; }// No ADD for Recon
		}

		ZDecimal? IDutyData.CVDutyManual
		{
			get { return null; }// No CVD for Recon
		}

		IDutyData IDutyData.CombineParentLine
		{
			get
			{
				if (line.ParentTariffLine is JobComInvoiceLine parentTariffLine)
				{
					return new ReconCurrentDutyData(parentTariffLine, dateForMPFCalculation);
				}

				return null;
			}
		}

		public IEnumerable<IEntryLineOrInvoiceLineDutyData> SecondaryLines
		{
			get
			{
				if (line.HasEmptySupTariff && !line.IsSecondaryTariffLine)
				{
					foreach (var childLine in line.SecondaryTariffLines)
					{
						if (!childLine.HasEmptySupTariff)
						{
							yield return new ReconSupDutyData(childLine, dateForMPFCalculation);
						}

						yield return new ReconCurrentDutyData(childLine, dateForMPFCalculation);
					}
				}
			}
		}

		IEnumerable<IDutyData> IEntryLineOrInvoiceLineDutyData.ChildLines
		{
			get
			{
				if (line.HasEmptySupTariff)
				{
					foreach (var childInvoiceLine in line.ChildLines)
					{
						if (!childInvoiceLine.HasEmptySupTariff)
						{
							yield return new ReconSupDutyData(childInvoiceLine, dateForMPFCalculation);
						}

						yield return new ReconCurrentDutyData(childInvoiceLine, dateForMPFCalculation);
					}
				}
			}
		}

		IEntryLineOrInvoiceLineDutyData IEntryLineOrInvoiceLineDutyData.ParentLine
		{
			get
			{
				IEntryLineOrInvoiceLineDutyData supTariffLine = !line.HasEmptySupTariff ? new ReconSupDutyData(line, dateForMPFCalculation) : null;
				return supTariffLine?.ParentLine ?? supTariffLine;
			}
		}

		public ZDecimal GetDutyFeeChargeAmount(string chargeCode)
		{
			ZDecimal result = 0m;

			if (chargeCode == Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)
			{
				result = line.US_Duty;
			}
			else
			{
				result = line.FeeCusCodes.GetFeeOrChargeAmount(chargeCode);
			}

			return result;
		}

		public void SetDutyFeeChargeAmount(string chargeCode, decimal chargeAmount, FeeCalculationInternalData feeCalculationInternalData)
		{
			if (chargeCode == Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)
			{
				line.US_Duty = chargeAmount;
			}
			else
			{
				ZDecimal existingAmount = line.FeeCusCodes.GetFeeOrChargeAmount(chargeCode);

				if (existingAmount + chargeAmount != existingAmount)
				{
					line.FeeCusCodes.UpdateOrAddCharge(chargeCode, new ZDecimal(existingAmount + chargeAmount).Round(2));
				}
			}
		}

		public void SetDutyResult(IDutyResult dutyResult)
		{
			//Do not store duty rate at invoice lines
			SetDutyFeeChargeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, dutyResult.TotalAmount.Amount, new FeeCalculationInternalData());
		}

		public void StoreAdjustedDerivedCustomsValue(ZDecimal derivedCustomsValue)
		{
			line.US_DerivedCV = derivedCustomsValue;
		}

		public void UpdateInvoiceLinesLinkToEntryLineForDerivedDutyCalculation(ZGuid newEntryLinePK)
		{
			//do nothing for invoice line
		}

		public IEnumerable<IFeeCalculationDataProvider> FeeDataProviders
		{
			get { yield return this; }
		}

		public void RollUpFees(IDutyDataLineHeader entry)
		{
			line.US_HasMPF = line.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing) > 0;
			if (entry.CalculateChangedLinesOnly)
			{
				var originalLineHMF = line.ReconOriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF);
				if (originalLineHMF == 0m)
				{
					var hmfRate = new FeeCalculationHelper(line.Factory, DateForDutyCalculation).HMFRatePercentage;
					originalLineHMF = line.US_R_OrigCV * hmfRate / 100;
				}
				var existingHMFTotal = entry.FeeAndCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF);
				if (entry.IsHMFApplicable || entry.FeeAndCharges.OfType<ReconEntryOriginalCharge>().Any(x => x.CY_Code == Core.Constants.USCustoms.FeeCodes.HMF))
				{
					entry.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.HMF, existingHMFTotal - originalLineHMF);
				}

				foreach (ReconEntryOriginalCharge originalCharge in line.ReconOriginalCharges)
				{
					var originalChargeCode = originalCharge.CY_Code;
					if (!entry.IsFeeUserEntered(originalChargeCode) && originalChargeCode != Core.Constants.USCustoms.FeeCodes.HMF)
					{
						var existingTotal = entry.FeeAndCharges.GetFeeOrChargeAmount(originalChargeCode);
						var originalLineCharge = originalCharge.CY_Amount;
						entry.FeeAndCharges.UpdateOrAddCharge(originalChargeCode, existingTotal - originalLineCharge);
					}
				}
			}

			foreach (FeeCusCodeData fee in line.FeeCusCodes)
			{
				var reconFeeCode = fee.CY_Code;
				if (!reconFeeCode.IsEmpty)
				{
					var roundedAmount = fee.CY_FeeAmount.Round(2);
					fee.CY_FeeAmount = roundedAmount;

					if (!entry.IsFeeUserEntered(reconFeeCode))
					{
						var existingTotal = entry.FeeAndCharges.GetFeeOrChargeAmount(reconFeeCode);
						var amountToAdd = roundedAmount;
						entry.FeeAndCharges.UpdateOrAddCharge(reconFeeCode, existingTotal + amountToAdd);
					}
				}
			}

			ZDecimal existingDuty = entry.FeeAndCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Duty);
			var dutyAmountToAdd = line.US_Duty;
			if (entry.CalculateChangedLinesOnly)
			{
				dutyAmountToAdd -= line.US_R_OrigDuty;
			}
			entry.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, existingDuty + dutyAmountToAdd);
		}

		public void UpdateLineAndHeaderFeeAmountLessThanThreshold(ZString feeType, ZDecimal thresholdAmount)
		{
			ReconOriginalEntryHeader reconEntry = line.InvoiceHeader != null ? line.InvoiceHeader.ReconOriginalEntry : null;

			line.FeeCusCodes.UpdateLineAndHeaderFeeAmountLessThanThreshold(feeType, thresholdAmount, reconEntry != null ? reconEntry.ReconCharges : null);
		}

		public bool IsDutyOverridden
		{
			get { return line.US_OverrideDuty; }
		}

		public bool IsDomesticMerchandise
		{
			get { return line.IsDomesticMerchandise; }
		}

		public bool HasMPF
		{
			get { return line.US_HasMPF; }
			set { line.US_HasMPF = value; }
		}

		public bool IsMPFOverridden
		{
			get { return line.IsMPFOverridden; }
		}

		#endregion

		#region IDutyData Members

		public ZString Tariff
		{
			get { return dutyData.Tariff; }
		}

		public ZDate DateForDutyCalculation
		{
			get { return dutyData.DateForDutyCalculation; }
		}

		public ZDecimal Quantity1
		{
			get { return dutyData.Quantity1; }
		}

		public ZString UQ1
		{
			get { return dutyData.UQ1; }
		}

		public ZDecimal Quantity2
		{
			get { return dutyData.Quantity2; }
		}

		public ZString UQ2
		{
			get { return dutyData.UQ2; }
		}

		public ZDecimal Quantity3
		{
			get { return dutyData.Quantity3; }
		}

		public ZString UQ3
		{
			get { return dutyData.UQ3; }
		}

		ZDecimal GetCustomsValue(JobComInvoiceLine invoiceLine)
		{
			return CustomsValueDeciderForInvoiceLine.GetCustomsValue(invoiceLine, false);
		}

		ZDecimal IDutyData.CustomsValue
		{
			get { return GetCustomsValue(line).Round(0); }
		}

		ZDecimal IDutyData.SupCustomsValue
		{
			get { return dutyData.SupCustomsValue; }
		}

		public ZString SpecialProgramsIndicatorPrimary
		{
			get { return dutyData.SpecialProgramsIndicatorPrimary; }
		}

		public ZString SpecialProgramsIndicatorCountry
		{
			get { return dutyData.SpecialProgramsIndicatorCountry; }
		}

		public ZString CountryOfOrigin
		{
			get { return dutyData.CountryOfOrigin; }
		}

		public ZString SpecialProgramsIndicatorSecondary
		{
			get { return dutyData.SpecialProgramsIndicatorSecondary; }
		}

		public ZString SelectedRateType
		{
			get { return dutyData.SelectedRateType; }
		}

		public ZString EntryType
		{
			get { return ""; }//This is for recon declarations and TIB or Informal entry type cannot be reconciled. EntryType is used by MPF Calculator to know whether it is exempt or not
		}

		public bool IsClearedInPR
		{
			get { return dutyData.IsClearedInPR; }
		}

		public bool IsAMSFeeExempt
		{
			get { return dutyData.IsAMSFeeExempt; }
		}

		public bool IsRaspberryFeeExempt
		{
			get { return dutyData.IsRaspberryFeeExempt; }
		}

		public ZBool IsSetXLine
		{
			get { return dutyData.IsSetXLine; }
		}

		public ZBool IsSetVLine
		{
			get { return dutyData.IsSetVLine; }
		}

		public bool IsCottonFeeExemptIndicated
		{
			get { return dutyData.IsCottonFeeExemptIndicated; }
		}

		public bool IsCombineSecondaryTariffLine
		{
			get { return dutyData.IsCombineSecondaryTariffLine; }
		}

		public IReadOnlyList<ZString> SupTariffs
		{
			get { return dutyData.SupTariffs; }
		}

		public IEnumerable<IDutyData> CombineChildLines
		{
			get
			{
				foreach (var childInvoiceLine in line.ChildLines)
				{
					yield return new ReconCurrentDutyData(childInvoiceLine, dateForMPFCalculation);
				}
			}
		}

		public IEnumerable<IDutyData> CombineAllLines
		{
			get
			{
				var result = new List<IDutyData>();
				var parentLine = new ReconCurrentDutyData(line.ParentTariffLine ?? line, dateForMPFCalculation);
				foreach (var combineChildLine in parentLine.CombineChildLines)
				{
					result.Add(combineChildLine);
					if (combineChildLine.ParentTariffLine is ReconSupDutyData combineChildLineSupDutyData)
					{
						result.Add(combineChildLineSupDutyData);
					}
				}
				if (result.Count > 0)
				{
					result.Insert(0, parentLine);
					if (parentLine.ParentTariffLine is ReconSupDutyData parentLineSupDutyData)
					{
						result.Insert(1, parentLineSupDutyData);
					}
				}

				return result;
			}
		}

		/// <summary>
		/// recon declarations do not have a separate field for cotton certificates. When IMP lines are imported, if a cotton certificate is indicated, it sets US_CotteonFeeExempt to Yes
		/// </summary>
		public bool HasCottonCertificate
		{
			get { return false; }
		}

		public IDutyData ParentTariffLine
		{
			get
			{
				return !line.HasEmptySupTariff ? new ReconSupDutyData(line, dateForMPFCalculation) :
					line.ParentTariffLine != null ? (IDutyData)new ReconCurrentDutyData(line.ParentTariffLine, dateForMPFCalculation) : null;
			}
		}

		public bool IsSecondaryTariffLine
		{
			get { return !line.HasEmptySupTariff || dutyData.IsSecondaryTariffLine; }
		}

		public ZDecimal? OverriddenTaxRate
		{
			get { return ((IFeeCalculationDataProvider)line).OverriddenTaxRate; }
		}

		public ZString OverriddenTaxRateUQ
		{
			get { return ((IFeeCalculationDataProvider)line).OverriddenTaxRateUQ; }
		}

		bool IDutyData.HasTextileCategoryNo
		{
			get { return line.US_R_Textile; }
		}

		#endregion

		#region IFeeCalculationDataProvider Members

		bool IFeeCalculationDataProvider.IsACS => line.IsACSForRecon;

		bool IFeeCalculationDataProvider.IsFeeOverriden(string feeCode)
		{
			FeeCusCodeData charge = line.FeeCusCodes.GetFirstElementHaving(feeCode);
			return charge != null && charge.CY_IsOverridden;
		}

		void IFeeCalculationDataProvider.SetFeeResult(string feeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			SetDutyFeeChargeAmount(feeCode, amount, feeCalculationInternalData);
		}

		ZString IFeeCalculationDataProvider.GetSelectedRateType(string feeCode)
		{
			FeeCusCodeData charge = line.FeeCusCodes.GetFirstElementHaving(feeCode);
			return charge != null ? charge.CY_SelectedRateType : ZString.Empty;
		}

		ZDateTime IFeeCalculationDataProvider.DateForMPFCalculation
		{
			get { return dateForMPFCalculation; }
		}

		ZDecimal? IFeeCalculationDataProvider.OverriddenTaxRate
		{
			get { return line.GetOverriddenTaxRate(line.US_TaxApply, line.US_TaxRate, line.US_TaxCode, line.ImportTariff); }
		}

		ZString IFeeCalculationDataProvider.TaxCode
		{
			get { return line.US_TaxCode; }
		}

		ZString IFeeCalculationDataProvider.TaxRateType
		{
			get { return line.US_TaxRateT; }
		}

		ZString IFeeCalculationDataProvider.TaxComputationCode => line.Factory.GetValue(ref taxComputationCodeCached, () => AppendixBTaxRateList.GetComputationCode(line.US_TaxRateS, line.JI_Tariff, line.JI_CustomsUnitQty, line.JI_CustomsSecondUnitQty));
		CachedProperty<ZString> taxComputationCodeCached;

		ZDecimal IFeeCalculationDataProvider.TaxRateQuantity
		{
			get { return line.US_TaxQty; }
		}

		ZDecimal IFeeCalculationDataProvider.DairyQty
		{
			get { return line.DairyQty; }
		}

		ZString IFeeCalculationDataProvider.VisaNumber
		{
			get { return line.US_VisaNo; }
		}

		IEnumerable<IDutyData> IFeeCalculationDataProvider.SecondaryLines
		{
			get { return SecondaryLines.Cast<IDutyData>(); }
		}

		#endregion
	}
}
