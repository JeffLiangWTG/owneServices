using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ReconOriginalDutyData : IEntryLineOrInvoiceLineDutyData, IFeeCalculationDataProvider
	{
		public ReconOriginalDutyData(JobComInvoiceLine invoiceLine, ZDateTime dateForMPFCalculation)
		{
			this.invoiceLine = invoiceLine;
			this.dateForMPFCalculation = dateForMPFCalculation;
		}
		readonly ZDateTime dateForMPFCalculation;
		readonly JobComInvoiceLine invoiceLine;

		#region IEntryLineOrInvoiceLineDutyData Members

		public BusinessObjectFactory Factory
		{
			get { return invoiceLine.Factory; }
		}

		public ZGuid PK
		{
			get { return invoiceLine.PK; }
		}

		public USCTariff ImportTariff
		{
			get { return invoiceLine.OriginalImportTariff; }
		}

		public bool IsDutyFreeSPIClaimed
		{
			get
			{
				USCTariff importTariff = ImportTariff;

				ZString spiCode = !SpecialProgramsIndicatorCountry.IsEmpty ? SpecialProgramsIndicatorCountry : SpecialProgramsIndicatorPrimary;
				return importTariff != null && importTariff.BecomesDutyFreeDueTo(spiCode);
			}
		}

		public bool IsRecon
		{
			get { return invoiceLine.IsRecon; }
		}

		public bool HasMPF
		{
			get { return invoiceLine.US_R_OrigHasMPF; }
			set { invoiceLine.US_R_OrigHasMPF = value; }
		}

		public ZDecimal TotalCustomsValueIncludingSecondaryLines
		{
			get
			{
				ZDecimal result = GetCustomsValue(invoiceLine).Round(0);

				foreach (JobComInvoiceLine secondaryLine in invoiceLine.SecondaryTariffLines)
				{
					result += GetCustomsValue(secondaryLine).Round(0);
				}

				return result;
			}
		}

		ZString IEntryLineOrInvoiceLineDutyData.CalculateException
		{
			get => invoiceLine.TariffCalculateExceptionMessage;
			set => invoiceLine.TariffCalculateExceptionMessage = value;
		}

		ZDecimal GetCustomsValue(JobComInvoiceLine invoiceLine)
		{
			return CustomsValueDeciderForInvoiceLine.GetReconOriginalCustomsValue(invoiceLine, false);
		}

		public ZDecimal ValueForADD
		{
			get { return ZDecimal.Zero; }//No ADD or CVD duty involved with Recon
		}

		public ZDecimal ADDDepositRate
		{
			get { return ZDecimal.Zero; }//No ADD or CVD duty involved with Recon
		}

		public ZDecimal ValueForCVD
		{
			get { return ZDecimal.Zero; }//No ADD or CVD duty involved with Recon
		}

		public ZDecimal CVDDepositRate
		{
			get { return ZDecimal.Zero; }//No ADD or CVD duty involved with Recon
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
			get { return ZDecimal.Zero; }// No ADD for Recon
		}

		ZString IDutyData.CVDCaseRateTypeQualifier
		{
			get { return ZString.Empty; }// No ADD for Recon
		}

		public IEnumerable<IEntryLineOrInvoiceLineDutyData> SecondaryLines
		{
			get
			{
				if (invoiceLine.US_R_OrigSupTariff.IsEmpty && !invoiceLine.IsSecondaryTariffLine)
				{
					foreach (JobComInvoiceLine childLine in invoiceLine.SecondaryTariffLines)
					{
						if (!childLine.HasEmptySupTariff)
						{
							yield return new ReconOriginalSupDutyData(childLine, dateForMPFCalculation);
						}

						yield return new ReconOriginalDutyData(childLine, dateForMPFCalculation);
					}
				}
			}
		}

		IEntryLineOrInvoiceLineDutyData IEntryLineOrInvoiceLineDutyData.ParentLine
		{
			get
			{
				IEntryLineOrInvoiceLineDutyData supTariffLine = !invoiceLine.US_R_OrigSupTariff.IsEmpty ? new ReconOriginalSupDutyData(invoiceLine, dateForMPFCalculation) : null;
				return supTariffLine?.ParentLine ?? supTariffLine;
			}
		}

		public ZDecimal GetDutyFeeChargeAmount(string chargeCode)
		{
			ZDecimal result = 0m;

			if (chargeCode == Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)
			{
				result = invoiceLine.US_R_OrigDuty;
			}
			else
			{
				result = invoiceLine.ReconOriginalCharges.GetFeeOrChargeAmount(chargeCode);
			}

			return result;
		}

		public void SetDutyFeeChargeAmount(string chargeCode, decimal chargeAmount, FeeCalculationInternalData feeCalculationInternalData)
		{
			if (chargeCode == Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)
			{
				invoiceLine.US_R_OrigDuty = chargeAmount;
			}
			else
			{
				ZDecimal existingAmount = invoiceLine.ReconOriginalCharges.GetFeeOrChargeAmount(chargeCode);

				if (existingAmount + chargeAmount != existingAmount)
				{
					invoiceLine.ReconOriginalCharges.SetAmount(chargeCode, existingAmount + chargeAmount);
				}
			}
		}

		public void StoreDutyRate(decimal percentage, decimal amount, string perUQ)
		{
			//do not need to store for invoice line
		}

		public void StoreAdjustedDerivedCustomsValue(ZDecimal derivedCustomsValue)
		{
			//do not need to store for invoice lines
		}

		public void UpdateInvoiceLinesLinkToEntryLineForDerivedDutyCalculation(ZGuid newEntryLinePK)
		{
			//do not need to store for invoice line
		}

		public IEnumerable<IFeeCalculationDataProvider> FeeDataProviders
		{
			get { yield return this; }
		}

		public void RollUpFees(IDutyDataLineHeader entry)
		{
			invoiceLine.US_R_OrigHasMPF = invoiceLine.ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing) > 0;

			if (!entry.CalculateChangedLinesOnly)
			{
				foreach (ReconEntryOriginalCharge fee in invoiceLine.ReconOriginalCharges)
				{
					if (!fee.CY_Code.IsEmpty && fee.CY_Amount > 0m)
					{
						var roundedAmount = fee.CY_Amount.Round(2);
						fee.CY_Amount = roundedAmount;

						if (!entry.IsFeeUserEntered(fee.CY_Code))
						{
							var newAmount = entry.FeeAndCharges.GetFeeOrChargeAmount(fee.CY_Code) + roundedAmount;
							entry.FeeAndCharges.UpdateOrAddCharge(fee.CY_Code, newAmount);
						}
					}
				}

				ZDecimal existingDuty = entry.FeeAndCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Duty);
				entry.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, invoiceLine.US_R_OrigDuty + existingDuty);
			}
		}

		public void UpdateLineAndHeaderFeeAmountLessThanThreshold(ZString feeType, ZDecimal thresholdAmount)
		{
			ReconOriginalEntryHeader reconEntry = invoiceLine.InvoiceHeader != null ? invoiceLine.InvoiceHeader.ReconOriginalEntry : null;

			invoiceLine.ReconOriginalCharges.UpdateLineAndHeaderFeeAmountLessThanThreshold(feeType, thresholdAmount, reconEntry != null ? reconEntry.OriginalCharges : null);
		}

		public bool IsDutyOverridden
		{
			get { return invoiceLine.US_R_OrigOverrideDuty; }
		}

		public void SetDutyResult(IDutyResult dutyResult)
		{
			invoiceLine.US_R_OrigDuty = dutyResult.TotalAmount.Amount;
		}

		public bool IsMPFOverridden
		{
			get { return invoiceLine.IsMPFOverridden; }
		}

		#endregion

		#region IFeeCalculationDataProvider Members

		bool IFeeCalculationDataProvider.IsACS => invoiceLine.IsACSForRecon;

		bool IFeeCalculationDataProvider.IsFeeOverriden(string feeCode)
		{
			ReconEntryOriginalCharge charge = invoiceLine.ReconOriginalCharges.GetFirstElementHaving(feeCode);

			return charge != null && charge.CY_IsOverridden;
		}

		void IFeeCalculationDataProvider.SetFeeResult(string feeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			SetDutyFeeChargeAmount(feeCode, amount, feeCalculationInternalData);
		}

		ZDateTime IFeeCalculationDataProvider.DateForMPFCalculation
		{
			get { return dateForMPFCalculation; }
		}

		ZDecimal IFeeCalculationDataProvider.DairyQty
		{
			get { return invoiceLine.OrigDairyQty; }
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
			get { return SecondaryLines.Cast<IDutyData>(); }
		}

		#endregion

		#region IDutyData Members

		public ZString Tariff
		{
			get { return invoiceLine.US_R_OrigTariff; }
		}

		public ZDate DateForDutyCalculation
		{
			get { return invoiceLine.EffectiveDateForDutyRate; }
		}

		public ZDecimal Quantity1
		{
			get { return RoundedQuantity.GetRoundedQuantity1(this, invoiceLine.US_R_OrigFirstQty); }
		}

		public ZString UQ1
		{
			get { return invoiceLine.US_R_OrigFirstUQ; }
		}

		public ZDecimal Quantity2
		{
			get { return RoundedQuantity.GetRoundedQuantity2(this, invoiceLine.US_R_OrigSecondQty); }
		}

		public ZString UQ2
		{
			get { return invoiceLine.US_R_OrigSecondUQ; }
		}

		public ZDecimal Quantity3
		{
			get { return RoundedQuantity.GetRoundedQuantity3(this, invoiceLine.US_R_OrigThirdQty); }
		}

		public ZString UQ3
		{
			get { return invoiceLine.US_R_OrigThirdUQ; }
		}

		ZDecimal IDutyData.CustomsValue
		{
			get { return GetCustomsValue(invoiceLine).Round(0); }
		}

		ZDecimal IDutyData.SupCustomsValue
		{
			get { return ZDecimal.Zero; }
		}

		public ZString SpecialProgramsIndicatorPrimary
		{
			get
			{
				var result = ZString.Empty;
				var originSPI = invoiceLine.US_R_OrigSPI;
				if (Factory.GetCachedValue<PrimarySpecProgramIndicatorList>().ContainsCode(originSPI))
				{
					result = this.GetEffectiveSPIForDutyCalculation(originSPI);
				}

				return result;
			}
		}

		public ZString SpecialProgramsIndicatorCountry
		{
			get
			{
				var result = ZString.Empty;
				var originSPI = invoiceLine.US_R_OrigSPI;
				if (invoiceLine.AddInfoLookups.US_SpecialProgramList.ContainsCode(originSPI))
				{
					result = this.GetEffectiveSPIForDutyCalculation(originSPI);
				}

				return result;
			}
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
			get { return invoiceLine.US_R_OrigRateType; }
		}

		public ZString EntryType
		{
			get { return ZString.Empty; }
		}

		public bool IsClearedInPR
		{
			get { return ((IDutyData)invoiceLine).IsClearedInPR; }
		}

		public bool IsAMSFeeExempt
		{
			get { return invoiceLine.IsAMSFeeExempt; }
		}

		public bool IsRaspberryFeeExempt
		{
			get { return invoiceLine.IsRaspberryFeeExempt; }
		}

		public ZBool IsSetXLine
		{
			get { return invoiceLine.IsSetXLine; }
		}

		public ZBool IsSetVLine
		{
			get { return invoiceLine.IsSetVLine; }
		}

		public bool IsCottonFeeExemptIndicated
		{
			get { return invoiceLine.US_R_OrigCottonFeeExempt == YesNoDefaultList.Codes.Yes; }
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
				return !invoiceLine.US_R_OrigSupTariff.IsEmpty ? new ReconOriginalSupDutyData(invoiceLine, dateForMPFCalculation) :
					invoiceLine.ParentTariffLine != null ? (IDutyData)new ReconOriginalDutyData(invoiceLine.ParentTariffLine, dateForMPFCalculation) : null;
			}
		}

		public bool IsSecondaryTariffLine
		{
			get { return !invoiceLine.US_R_OrigSupTariff.IsEmpty || invoiceLine.IsSecondaryTariffLine; }
		}

		public ZString GetSelectedRateType(string feeCode)
		{
			ReconEntryOriginalCharge charge = invoiceLine.ReconOriginalCharges.GetFirstElementHaving(feeCode);
			return charge != null ? charge.CY_SelectedRateType : ZString.Empty;
		}

		public bool IsDomesticMerchandise
		{
			get { return invoiceLine.IsDomesticMerchandise; }
		}

		public bool IsCombineSecondaryTariffLine
		{
			get { return ((IDutyData)invoiceLine).IsCombineSecondaryTariffLine; }
		}

		IEnumerable<IDutyData> IEntryLineOrInvoiceLineDutyData.ChildLines
		{
			get
			{
				if (invoiceLine.US_R_OrigSupTariff.IsEmpty)
				{
					foreach (var childLine in invoiceLine.ChildLines)
					{
						if (!childLine.US_R_OrigSupTariff.IsEmpty)
						{
							yield return new ReconOriginalSupDutyData(childLine, dateForMPFCalculation);
						}

						yield return new ReconOriginalDutyData(childLine, dateForMPFCalculation);
					}
				}
			}
		}

		public IEnumerable<IDutyData> CombineChildLines
		{
			get
			{
				foreach (var childInvoiceLine in invoiceLine.ChildLines)
				{
					yield return new ReconOriginalDutyData(childInvoiceLine, dateForMPFCalculation);
				}
			}
		}

		IEnumerable<IDutyData> IDutyData.CombineAllLines
		{
			get
			{
				var result = new List<IDutyData>();
				var parentLine = new ReconOriginalDutyData(invoiceLine.ParentTariffLine ?? invoiceLine, dateForMPFCalculation);
				foreach (var combineChildLine in parentLine.CombineChildLines)
				{
					result.Add(combineChildLine);
					if (combineChildLine.ParentTariffLine is ReconOriginalSupDutyData combineChildLineSupDutyData)
					{
						result.Add(combineChildLineSupDutyData);
					}
				}
				if (result.Count > 0)
				{
					result.Insert(0, parentLine);
					if (parentLine.ParentTariffLine is ReconOriginalSupDutyData parentLineSupDutyData)
					{
						result.Insert(1, parentLineSupDutyData);
					}
				}

				return result;
			}
		}

		IDutyData IDutyData.CombineParentLine
		{
			get
			{
				if (invoiceLine.ParentTariffLine is JobComInvoiceLine parentTariffLine)
				{
					return new ReconOriginalDutyData(parentTariffLine, dateForMPFCalculation);
				}

				return null;
			}
		}

		public IReadOnlyList<ZString> SupTariffs
		{
			get { return ((IDutyData)invoiceLine).SupTariffs; }
		}

		public ZDecimal? OverriddenTaxRate
		{
			get { return invoiceLine.GetOverriddenTaxRate(invoiceLine.US_R_OrigTaxApply, invoiceLine.US_R_OrigTaxRate, invoiceLine.US_R_OrigTaxCode, invoiceLine.OriginalImportTariff); }
		}

		public ZString OverriddenTaxRateUQ
		{
			get { return invoiceLine.GetOverriddenTaxRateUQ(invoiceLine.US_R_OrigTaxRateS); }
		}

		public ZString TaxCode
		{
			get { return invoiceLine.US_R_OrigTaxCode; }
		}

		public ZString TaxComputationCode => Factory.GetValue(ref taxComputationCodeCached, () => AppendixBTaxRateList.GetComputationCode(invoiceLine.US_R_OrigTaxRateS, invoiceLine.US_R_OrigTariff, invoiceLine.US_R_OrigFirstUQ, invoiceLine.US_R_OrigSecondUQ));
		CachedProperty<ZString> taxComputationCodeCached;

		public ZString TaxRateType
		{
			get { return invoiceLine.US_R_OrigTaxRateT; }
		}

		public ZDecimal TaxRateQuantity
		{
			get { return invoiceLine.US_R_OrigTaxQty; }
		}

		bool IDutyData.HasTextileCategoryNo
		{
			get { return invoiceLine.US_R_Textile; }
		}

		ZDecimal? IDutyData.ADDutyManual
		{
			get { return null; }// No ADD for Recon
		}

		ZDecimal? IDutyData.CVDutyManual
		{
			get { return null; }// No CVD for Recon
		}

		#endregion
	}
}
