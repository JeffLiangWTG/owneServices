using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class ReconOriginalSupDutyData : IEntryLineOrInvoiceLineDutyData, IFeeCalculationDataProvider
	{
		public ReconOriginalSupDutyData(JobComInvoiceLine invoiceLine, ZDateTime dateForMPFCalculation)
		{
			this.invoiceLine = invoiceLine;
			this.originalSecondaryDutyData = new ReconOriginalDutyData(invoiceLine, dateForMPFCalculation);
			this.dateForMPFCalculation = dateForMPFCalculation;
		}

		readonly JobComInvoiceLine invoiceLine;
		readonly ReconOriginalDutyData originalSecondaryDutyData;
		readonly ZDateTime dateForMPFCalculation;

		#region IEntryLineOrInvoiceLineDutyData Members

		ZGuid IEntryLineOrInvoiceLineDutyData.PK
		{
			get { return invoiceLine.PK; }
		}

		bool IEntryLineOrInvoiceLineDutyData.IsDutyFreeSPIClaimed
		{
			get
			{
				USCTariff importTariff = ImportTariff;

				return importTariff != null && importTariff.BecomesDutyFreeDueTo(invoiceLine.US_R_OrigSPI);
			}
		}

		bool IEntryLineOrInvoiceLineDutyData.IsRecon
		{
			get { return invoiceLine.IsRecon; }
		}

		ZDecimal IEntryLineOrInvoiceLineDutyData.TotalCustomsValueIncludingSecondaryLines
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
			return CustomsValueDeciderForInvoiceLine.GetReconOriginalCustomsValue(invoiceLine, true);
		}

		ZDecimal IDutyData.ValueForADD
		{
			get { return ZDecimal.Zero; }// No ADD for Recon
		}

		ZDecimal IDutyData.ADDDepositRate
		{
			get { return ZDecimal.Zero; }// No ADD for Recon
		}

		ZDecimal IDutyData.ValueForCVD
		{
			get { return ZDecimal.Zero; }// No CVD for Recon
		}

		ZDecimal IDutyData.CVDDepositRate
		{
			get { return ZDecimal.Zero; }// No CVD for Recon
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

		IEnumerable<IEntryLineOrInvoiceLineDutyData> IEntryLineOrInvoiceLineDutyData.SecondaryLines
		{
			get { return SecondaryLines; }
		}

		IEnumerable<IEntryLineOrInvoiceLineDutyData> SecondaryLines
		{
			get
			{
				if (!invoiceLine.IsSecondaryTariffLine && !invoiceLine.US_R_OrigSupTariff.IsEmpty)
				{
					yield return originalSecondaryDutyData;

					foreach (JobComInvoiceLine secondaryLine in invoiceLine.SecondaryTariffLines)
					{
						if (!secondaryLine.US_R_OrigSupTariff.IsEmpty)
						{
							yield return new ReconOriginalSupDutyData(secondaryLine, dateForMPFCalculation);
						}

						yield return new ReconOriginalDutyData(secondaryLine, dateForMPFCalculation);
					}
				}
			}
		}

		IEntryLineOrInvoiceLineDutyData IEntryLineOrInvoiceLineDutyData.ParentLine
		{
			get
			{
				IEntryLineOrInvoiceLineDutyData parentTariffLine = invoiceLine.ParentTariffLine is JobComInvoiceLine tariffLine ? new ReconOriginalSupDutyData(tariffLine, dateForMPFCalculation) : null;
				return parentTariffLine?.ParentLine ?? parentTariffLine;
			}
		}

		void IEntryLineOrInvoiceLineDutyData.SetDutyResult(IDutyResult dutyResult)
		{
			invoiceLine.US_R_OrigSupDuty = dutyResult.TotalAmount.Amount;
		}

		public void SetDutyFeeChargeAmount(string chargeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			if (chargeCode == Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)
			{
				invoiceLine.US_R_OrigSupDuty = amount;
			}
			else
			{
				ZDecimal existingAmount = invoiceLine.ReconOriginalCharges.GetFeeOrChargeAmount(chargeCode);

				if (existingAmount + amount != existingAmount)
				{
					invoiceLine.ReconOriginalCharges.SetAmount(chargeCode, existingAmount + amount);
				}
			}
		}

		ZDecimal IEntryLineOrInvoiceLineDutyData.GetDutyFeeChargeAmount(string chargeCode)
		{
			ZDecimal result = 0m;

			if (chargeCode == Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)
			{
				result = invoiceLine.US_R_OrigSupDuty;
			}
			else
			{
				result = invoiceLine.ReconOriginalCharges.GetFeeOrChargeAmount(chargeCode);
			}

			return result;
		}

		void IEntryLineOrInvoiceLineDutyData.StoreAdjustedDerivedCustomsValue(ZDecimal derivedCustomsValue)
		{
			//do not need to store for invoice line
		}

		void IEntryLineOrInvoiceLineDutyData.UpdateInvoiceLinesLinkToEntryLineForDerivedDutyCalculation(ZGuid newEntryLinePK)
		{
			//do not need to store for invoice line
		}

		IEnumerable<IFeeCalculationDataProvider> IEntryLineOrInvoiceLineDutyData.FeeDataProviders
		{
			get { yield return this; }
		}

		void IEntryLineOrInvoiceLineDutyData.RollUpFees(IDutyDataLineHeader entry)
		{
			//Fees will be rolled up in ReconOriginalDutyData
			if (!entry.CalculateChangedLinesOnly)
			{
				var existingDuty = entry.FeeAndCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Duty);
				entry.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, invoiceLine.US_R_OrigSupDuty + existingDuty);
			}
		}

		void IEntryLineOrInvoiceLineDutyData.UpdateLineAndHeaderFeeAmountLessThanThreshold(ZString feeType, ZDecimal thresholdAmount)
		{
			// Will do in ReconOriginalDutyData
		}

		bool IEntryLineOrInvoiceLineDutyData.IsDutyOverridden
		{
			get { return invoiceLine.US_R_OrigOverrideSupDuty; }
		}

		// invoiceLine.US_R_OrigHasMPF will do
		bool IEntryLineOrInvoiceLineDutyData.HasMPF
		{
			get;
			set;
		}

		bool IEntryLineOrInvoiceLineDutyData.IsMPFOverridden
		{
			get { return invoiceLine.IsMPFOverridden; }
		}

		#endregion

		#region IDutyData Members

		ZString IDutyData.Tariff
		{
			get { return invoiceLine.US_R_OrigSupTariff; }
		}

		public USCTariff ImportTariff
		{
			get { return invoiceLine.OriginalImportSupTariff; }
		}

		ZDate IDutyData.DateForDutyCalculation
		{
			get { return invoiceLine.EffectiveDateForDutyRate; }
		}

		ZDecimal IDutyData.Quantity1
		{
			get { return RoundedQuantity.GetRoundedQuantity1(this, invoiceLine.US_R_OrigSupQty1); }
		}

		ZString IDutyData.UQ1
		{
			get { return invoiceLine.US_R_OrigSupUQ1; }
		}

		ZDecimal IDutyData.Quantity2
		{
			get { return RoundedQuantity.GetRoundedQuantity2(this, invoiceLine.US_R_OrigSupQty2); }
		}

		ZString IDutyData.UQ2
		{
			get { return invoiceLine.US_R_OrigSupUQ2; }
		}

		ZDecimal IDutyData.Quantity3
		{
			get { return RoundedQuantity.GetRoundedQuantity3(this, invoiceLine.US_R_OrigSupQty3); }
		}

		ZString IDutyData.UQ3
		{
			get { return invoiceLine.US_R_OrigSupUQ3; }
		}

		ZDecimal IDutyData.CustomsValue
		{
			get { return GetCustomsValue(invoiceLine).Round(0); }
		}

		ZDecimal IDutyData.SupCustomsValue
		{
			get { return ZDecimal.Zero; }
		}

		ZString IDutyData.SpecialProgramsIndicatorPrimary
		{
			get { return ((IDutyData)originalSecondaryDutyData).SpecialProgramsIndicatorPrimary; }
		}

		ZString IDutyData.SpecialProgramsIndicatorCountry
		{
			get { return ((IDutyData)originalSecondaryDutyData).SpecialProgramsIndicatorCountry; }
		}

		ZString IDutyData.CountryOfOrigin
		{
			get { return ((IDutyData)originalSecondaryDutyData).CountryOfOrigin; }
		}

		ZString IDutyData.SpecialProgramsIndicatorSecondary
		{
			get { return ((IDutyData)originalSecondaryDutyData).SpecialProgramsIndicatorSecondary; }
		}

		ZString IDutyData.SelectedRateType
		{
			get { return ((IDutyData)originalSecondaryDutyData).SelectedRateType; }
		}

		CargoWise.EntityFramework.BusinessObjectFactory IDutyData.Factory
		{
			get { return invoiceLine.Factory; }
		}

		IDutyData IDutyData.ParentTariffLine
		{
			get { return invoiceLine.ParentTariffLine != null ? new ReconOriginalSupDutyData(invoiceLine.ParentTariffLine, dateForMPFCalculation) : null; }
		}

		bool IDutyData.IsCottonFeeExemptIndicated
		{
			get { return ((IDutyData)originalSecondaryDutyData).IsCottonFeeExemptIndicated; }
		}

		/// <summary>
		/// recon declarations do not have a separate field for cotton certificates. When IMP lines are imported, if a cotton certificate is indicated, it sets US_CotteonFeeExempt to Yes
		/// </summary>
		bool IDutyData.HasCottonCertificate
		{
			get { return false; }
		}

		ZBool IDutyData.IsSetXLine
		{
			get { return ((IDutyData)originalSecondaryDutyData).IsSetXLine; }
		}

		ZBool IDutyData.IsSetVLine
		{
			get { return ((IDutyData)originalSecondaryDutyData).IsSetVLine; }
		}

		bool IDutyData.IsAMSFeeExempt
		{
			get { return ((IDutyData)originalSecondaryDutyData).IsAMSFeeExempt; }
		}

		bool IDutyData.IsRaspberryFeeExempt
		{
			get { return ((IDutyData)originalSecondaryDutyData).IsRaspberryFeeExempt; }
		}

		ZString IDutyData.EntryType
		{
			get { return ((IDutyData)originalSecondaryDutyData).EntryType; }
		}

		bool IDutyData.IsClearedInPR
		{
			get { return ((IDutyData)originalSecondaryDutyData).IsClearedInPR; }
		}

		bool IDutyData.IsSecondaryTariffLine
		{
			get { return invoiceLine.IsSecondaryTariffLine; }
		}

		bool IDutyData.IsDomesticMerchandise
		{
			get { return ((IDutyData)originalSecondaryDutyData).IsDomesticMerchandise; }
		}

		bool IDutyData.IsCombineSecondaryTariffLine
		{
			get { return ((IDutyData)originalSecondaryDutyData).IsCombineSecondaryTariffLine; }
		}

		IReadOnlyList<ZString> IDutyData.SupTariffs
		{
			get { return ((IDutyData)originalSecondaryDutyData).SupTariffs; }
		}

		IEnumerable<IDutyData> IEntryLineOrInvoiceLineDutyData.ChildLines
		{
			get
			{
				yield return new ReconOriginalDutyData(invoiceLine, dateForMPFCalculation);

				foreach (var childInvoiceLine in invoiceLine.ChildLines)
				{
					if (!childInvoiceLine.US_R_OrigSupTariff.IsEmpty)
					{
						yield return new ReconOriginalSupDutyData(childInvoiceLine, dateForMPFCalculation);
					}

					yield return new ReconOriginalDutyData(childInvoiceLine, dateForMPFCalculation);
				}
			}
		}

		IEnumerable<IDutyData> IDutyData.CombineChildLines
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

		#region IFeeCalculationDataProvider Members

		bool IFeeCalculationDataProvider.IsACS
		{
			get => ((IFeeCalculationDataProvider)originalSecondaryDutyData).IsACS;
		}

		bool IFeeCalculationDataProvider.IsFeeOverriden(string feeCode)
		{
			return ((IFeeCalculationDataProvider)originalSecondaryDutyData).IsFeeOverriden(feeCode);
		}

		void IFeeCalculationDataProvider.SetFeeResult(string feeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			SetDutyFeeChargeAmount(feeCode, amount, feeCalculationInternalData);
		}

		ZString IFeeCalculationDataProvider.GetSelectedRateType(string feeCode)
		{
			return ((IFeeCalculationDataProvider)originalSecondaryDutyData).GetSelectedRateType(feeCode);
		}

		ZDateTime IFeeCalculationDataProvider.DateForMPFCalculation
		{
			get { return dateForMPFCalculation; }
		}

		// for supplementary tariff, tax is not relevant
		ZDecimal? IFeeCalculationDataProvider.OverriddenTaxRate
		{
			get { return null; }
		}

		ZString IFeeCalculationDataProvider.OverriddenTaxRateUQ
		{
			get { return ZString.Empty; }
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
			get { return SecondaryLines.Cast<IDutyData>(); }
		}

		#endregion
	}
}
