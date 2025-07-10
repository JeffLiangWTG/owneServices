using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class ReconSupDutyData : IEntryLineOrInvoiceLineDutyData, IFeeCalculationDataProvider
	{
		public ReconSupDutyData(JobComInvoiceLine invoiceLine, ZDateTime dateForMPFCalculation)
		{
			this.invoiceLine = invoiceLine;
			this.dateForMPFCalculation = dateForMPFCalculation;
		}
		readonly ZDateTime dateForMPFCalculation;
		readonly JobComInvoiceLine invoiceLine;

		public USCTariff ImportTariff
		{
			get { return invoiceLine.ImportSupTariff; }
		}

		#region IEntryLineOrInvoiceLineDutyData Members

		ZGuid IEntryLineOrInvoiceLineDutyData.PK
		{
			get { return invoiceLine.PK; }
		}

		bool IEntryLineOrInvoiceLineDutyData.IsDutyFreeSPIClaimed
		{
			get { return ImportTariff != null && ImportTariff.BecomesDutyFreeDueTo(invoiceLine.US_SPI); }
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
			return CustomsValueDeciderForInvoiceLine.GetCustomsValue(invoiceLine, true);
		}

		ZDecimal IDutyData.ValueForADD
		{
			get { return ZDecimal.Zero; }
		}

		ZDecimal IDutyData.ADDDepositRate
		{
			get { return ZDecimal.Zero; }
		}

		ZDecimal IDutyData.ValueForCVD
		{
			get { return ZDecimal.Zero; }
		}

		ZDecimal IDutyData.CVDDepositRate
		{
			get { return ZDecimal.Zero; }
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
				if (!invoiceLine.IsSecondaryTariffLine && !invoiceLine.HasEmptySupTariff)
				{
					yield return new ReconCurrentDutyData(invoiceLine, dateForMPFCalculation);

					foreach (JobComInvoiceLine secondaryLine in invoiceLine.SecondaryTariffLines)
					{
						if (!secondaryLine.HasEmptySupTariff)
						{
							yield return new ReconSupDutyData(secondaryLine, dateForMPFCalculation);
						}

						yield return new ReconCurrentDutyData(secondaryLine, dateForMPFCalculation);
					}
				}
			}
		}

		IEntryLineOrInvoiceLineDutyData IEntryLineOrInvoiceLineDutyData.ParentLine
		{
			get
			{
				IEntryLineOrInvoiceLineDutyData supTariffLine = invoiceLine.ParentTariffLine is JobComInvoiceLine tariffLine ? new ReconSupDutyData(tariffLine, dateForMPFCalculation) : null;
				return supTariffLine?.ParentLine ?? supTariffLine;
			}
		}

		void IEntryLineOrInvoiceLineDutyData.SetDutyResult(IDutyResult dutyResult)
		{
			invoiceLine.US_SupDuty = dutyResult.TotalAmount.Amount;
		}

		public void SetDutyFeeChargeAmount(string chargeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			if (chargeCode == Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)
			{
				invoiceLine.US_SupDuty = amount;
			}
			else
			{
				ZDecimal existingAmount = invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(chargeCode);

				if (existingAmount + amount != existingAmount)
				{
					invoiceLine.FeeCusCodes.UpdateOrAddCharge(chargeCode, existingAmount + amount);
				}
			}
		}

		ZDecimal IEntryLineOrInvoiceLineDutyData.GetDutyFeeChargeAmount(string chargeCode)
		{
			ZDecimal result = 0m;

			if (chargeCode == Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)
			{
				result = invoiceLine.US_SupDuty;
			}
			else
			{
				result = invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(chargeCode);
			}

			return result;
		}

		void IEntryLineOrInvoiceLineDutyData.StoreAdjustedDerivedCustomsValue(ZDecimal derivedCustomsValue)
		{
		}

		void IEntryLineOrInvoiceLineDutyData.UpdateInvoiceLinesLinkToEntryLineForDerivedDutyCalculation(ZGuid newEntryLinePK)
		{
		}

		IEnumerable<IFeeCalculationDataProvider> IEntryLineOrInvoiceLineDutyData.FeeDataProviders
		{
			get { yield return this; }
		}

		void IEntryLineOrInvoiceLineDutyData.RollUpFees(IDutyDataLineHeader entry)
		{
			var existingDuty = entry.FeeAndCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Duty);
			var amountToAdd = invoiceLine.US_SupDuty;
			if (entry.CalculateChangedLinesOnly)
			{
				amountToAdd -= invoiceLine.US_R_OrigSupDuty;
			}
			entry.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, amountToAdd + existingDuty);
		}

		void IEntryLineOrInvoiceLineDutyData.UpdateLineAndHeaderFeeAmountLessThanThreshold(ZString feeType, ZDecimal thresholdAmount)
		{
			// Will do in ReconCurrentDutyData.cs
		}

		bool IEntryLineOrInvoiceLineDutyData.IsDutyOverridden
		{
			get { return invoiceLine.US_OverrideSupDuty; }
		}

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
			get { return invoiceLine.US_SupTariff; }
		}

		USCTariff IDutyData.ImportTariff
		{
			get { return ImportTariff; }
		}

		ZDate IDutyData.DateForDutyCalculation
		{
			get { return invoiceLine.EffectiveDateForDutyRate; }
		}

		ZDecimal IDutyData.Quantity1
		{
			get { return RoundedQuantity.GetRoundedQuantity1(this, invoiceLine.US_SupQty1); }
		}

		ZString IDutyData.UQ1
		{
			get { return invoiceLine.US_SupUQ1; }
		}

		ZDecimal IDutyData.Quantity2
		{
			get { return RoundedQuantity.GetRoundedQuantity2(this, invoiceLine.US_SupQty2); }
		}

		ZString IDutyData.UQ2
		{
			get { return invoiceLine.US_SupUQ2; }
		}

		ZDecimal IDutyData.Quantity3
		{
			get { return RoundedQuantity.GetRoundedQuantity3(this, invoiceLine.US_SupQty3); }
		}

		ZString IDutyData.UQ3
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

		ZString IDutyData.SpecialProgramsIndicatorPrimary
		{
			get { return ((IDutyData)invoiceLine).SpecialProgramsIndicatorPrimary; }
		}

		ZString IDutyData.SpecialProgramsIndicatorCountry
		{
			get { return ((IDutyData)invoiceLine).SpecialProgramsIndicatorCountry; }
		}

		ZString IDutyData.CountryOfOrigin
		{
			get { return ((IDutyData)invoiceLine).CountryOfOrigin; }
		}

		ZString IDutyData.SpecialProgramsIndicatorSecondary
		{
			get { return ((IDutyData)invoiceLine).SpecialProgramsIndicatorSecondary; }
		}

		ZString IDutyData.SelectedRateType
		{
			get { return ((IDutyData)invoiceLine).SelectedRateType; }
		}

		CargoWise.EntityFramework.BusinessObjectFactory IDutyData.Factory
		{
			get { return invoiceLine.Factory; }
		}

		IDutyData IDutyData.ParentTariffLine
		{
			get { return invoiceLine.ParentTariffLine != null ? new ReconSupDutyData(invoiceLine.ParentTariffLine, dateForMPFCalculation) : null; }
		}

		bool IDutyData.IsCottonFeeExemptIndicated
		{
			get { return ((IDutyData)invoiceLine).IsCottonFeeExemptIndicated; }
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
			get
			{
				foreach (var childInvoiceLine in invoiceLine.ChildLines)
				{
					yield return new ReconCurrentDutyData(childInvoiceLine, dateForMPFCalculation);
				}
			}
		}

		IEnumerable<IDutyData> IDutyData.CombineAllLines
		{
			get
			{
				var result = new List<IDutyData>();
				var parentLine = new ReconCurrentDutyData(invoiceLine.ParentTariffLine ?? invoiceLine, dateForMPFCalculation);
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

		IEnumerable<IDutyData> IEntryLineOrInvoiceLineDutyData.ChildLines
		{
			get
			{
				yield return new ReconCurrentDutyData(invoiceLine, dateForMPFCalculation);

				foreach (var childInvoiceLine in invoiceLine.ChildLines)
				{
					if (!childInvoiceLine.HasEmptySupTariff)
					{
						yield return new ReconSupDutyData(childInvoiceLine, dateForMPFCalculation);
					}

					yield return new ReconCurrentDutyData(childInvoiceLine, dateForMPFCalculation);
				}
			}
		}

		IDutyData IDutyData.CombineParentLine
		{
			get
			{
				if (invoiceLine.ParentTariffLine is JobComInvoiceLine parentTariffLine)
				{
					return new ReconCurrentDutyData(parentTariffLine, dateForMPFCalculation);
				}

				return null;
			}
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
			get { return ((IDutyData)invoiceLine).IsSetXLine; }
		}

		ZBool IDutyData.IsSetVLine
		{
			get { return ((IDutyData)invoiceLine).IsSetVLine; }
		}

		bool IDutyData.IsAMSFeeExempt
		{
			get { return ((IDutyData)invoiceLine).IsAMSFeeExempt; }
		}

		bool IDutyData.IsRaspberryFeeExempt
		{
			get { return ((IDutyData)invoiceLine).IsRaspberryFeeExempt; }
		}

		ZString IDutyData.EntryType
		{
			get { return ((IDutyData)invoiceLine).EntryType; }
		}

		bool IDutyData.IsClearedInPR
		{
			get { return ((IDutyData)invoiceLine).IsClearedInPR; }
		}

		bool IDutyData.IsSecondaryTariffLine
		{
			get { return invoiceLine.IsSecondaryTariffLine; }
		}

		bool IDutyData.IsDomesticMerchandise
		{
			get { return ((IDutyData)invoiceLine).IsDomesticMerchandise; }
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

		bool IFeeCalculationDataProvider.IsACS => invoiceLine.IsACSForRecon;

		bool IFeeCalculationDataProvider.IsFeeOverriden(string feeCode)
		{
			return ((IFeeCalculationDataProvider)invoiceLine).IsFeeOverriden(feeCode);
		}

		void IFeeCalculationDataProvider.SetFeeResult(string feeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			SetDutyFeeChargeAmount(feeCode, amount, feeCalculationInternalData);
		}

		ZString IFeeCalculationDataProvider.GetSelectedRateType(string feeCode)
		{
			return ((IFeeCalculationDataProvider)invoiceLine).GetSelectedRateType(feeCode);
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
