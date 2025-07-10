using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Registry.Business.Customs.US;

namespace Enterprise.Customs.US.Business
{
	public class EntryLineIEntryLineOrInvoiceLineDutyData : IEntryLineOrInvoiceLineDutyData
	{
		public EntryLineIEntryLineOrInvoiceLineDutyData(CusEntryLine entryLine)
		{
			this.dutyData = entryLine;
			this.entryLine = entryLine;
			this.randomInvoiceLine = entryLine.RandomLine;
		}

		readonly CusEntryLine entryLine;
		readonly IDutyData dutyData;

		readonly JobComInvoiceLine randomInvoiceLine;

		#region IEntryLineDutyData Members

		BusinessObjectFactory IDutyData.Factory
		{
			get { return entryLine.Factory; }
		}

		ZGuid IEntryLineOrInvoiceLineDutyData.PK
		{
			get { return entryLine.PK; }
		}

		bool IEntryLineOrInvoiceLineDutyData.HasMPF
		{
			get { return entryLine.US_HasMPF; }
			set { entryLine.US_HasMPF = value; }
		}

		bool IEntryLineOrInvoiceLineDutyData.IsRecon
		{
			get { return randomInvoiceLine.IsRecon; }
		}

		ZString IEntryLineOrInvoiceLineDutyData.CalculateException
		{
			get => randomInvoiceLine.TariffCalculateExceptionMessage;
			set => randomInvoiceLine.TariffCalculateExceptionMessage = value;
		}

		public USCTariff ImportTariff
		{
			get { return entryLine.ImportTariff; }
		}

		public bool IsDutyFreeSPIClaimed
		{
			get { return entryLine.IsDutyFreeSPIClaimed; }
		}

		public ZDecimal TotalCustomsValueIncludingSecondaryLines
		{
			get { return entryLine.TotalRoundedCustomsValueIncludingSecondaryLines.Round(0); }
		}

		public ZDecimal ValueForADD
		{
			get { return dutyData.ValueForADD; }
		}

		public ZDecimal ADDDepositRate
		{
			get { return dutyData.ADDDepositRate; }
		}

		public ZString ADDCaseRateTypeQualifier
		{
			get { return dutyData.ADDCaseRateTypeQualifier; }
		}

		public ZDecimal ValueForCVD
		{
			get { return dutyData.ValueForCVD; }
		}

		public ZDecimal ADDQuantity
		{
			get { return dutyData.ADDQuantity; }
		}

		public ZDecimal CVDDepositRate
		{
			get { return dutyData.CVDDepositRate; }
		}

		public ZString CVDCaseRateTypeQualifier
		{
			get { return dutyData.CVDCaseRateTypeQualifier; }
		}

		public ZDecimal CVDQuantity
		{
			get { return dutyData.CVDQuantity; }
		}

		public IEnumerable<IDutyData> ChildLines
		{
			get { return entryLine.ChildLines; }
		}

		public IEntryLineOrInvoiceLineDutyData ParentLine
		{
			get { return entryLine.ParentLine != null ? new EntryLineIEntryLineOrInvoiceLineDutyData(entryLine.ParentLine) : null; }
		}

		public IEnumerable<IEntryLineOrInvoiceLineDutyData> SecondaryLines
		{
			get
			{
				foreach (CusEntryLine childLine in entryLine.ChildSecondaryEntryLines)
				{
					yield return new EntryLineIEntryLineOrInvoiceLineDutyData(childLine);
				}
			}
		}

		public ZDecimal GetDutyFeeChargeAmount(string chargeCode)
		{
			return entryLine.Fees.GetFeeOrChargeAmount(chargeCode);
		}

		public void SetDutyFeeChargeAmount(string chargeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			entryLine.Fees.SetAmount(chargeCode, amount);
		}

		public IFees FeeAndCharges
		{
			get { return entryLine.Fees; }
		}

		public void SetDutyResult(IDutyResult dutyResult)
		{
			entryLine.CL_DutyPercent = dutyResult.PercentOfValue;
			entryLine.CL_FlatAmount = dutyResult.PerUnitAmount.Amount;
			entryLine.CL_FlatAmountUQ = dutyResult.PerUnitUQ;

			entryLine.Fees.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, dutyResult.TotalAmount.Amount);
			entryLine.US_DutyRateDesc = ((DutyResult)dutyResult).RateString;
		}

		public void StoreAdjustedDerivedCustomsValue(ZDecimal derivedCustomsValue)
		{
			entryLine.CL_CustomsValue = derivedCustomsValue;
		}

		public void UpdateInvoiceLinesLinkToEntryLineForDerivedDutyCalculation(ZGuid newEntryLinePK)
		{
			var invoiceLines = new List<JobComInvoiceLine>(new TypedEnumerable<JobComInvoiceLine>(entryLine.InvoiceLines));

			var newEntryLine = entryLine.Factory.Load<CusEntryLine>(newEntryLinePK);
			foreach (var invoiceLine in invoiceLines)
			{
				if (newEntryLine != null && invoiceLine.AdditionalEntryLineLinks.ContainsPivotFor(newEntryLine) && invoiceLine.CusEntryLine is CusEntryLine currentEntryLine && !invoiceLine.AdditionalEntryLineLinks.ContainsPivotFor(currentEntryLine))
				{
					invoiceLine.AdditionalEntryLineLinks.DeleteLinkIfExistsFor(newEntryLine);
					invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(currentEntryLine);
				}

				invoiceLine.JI_CL = newEntryLinePK;
			}

			entryLine.RefreshInvoiceLines();

			if (newEntryLine != null)
			{
				newEntryLine.RefreshInvoiceLines();
			}
		}

		public IEnumerable<IFeeCalculationDataProvider> FeeDataProviders
		{
			get { yield return entryLine; }
		}

		/// <summary>
		/// Happens for 7501 and FTZ entry
		/// </summary>
		/// <param name="entry"></param>
		public void RollUpFees(IDutyDataLineHeader entry)
		{
			//Roll up overriden fees only. Otherwise, it is calculated at entry line level
			foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
			{
				if (!entryLine.US_SupLine)
				{
					foreach (FeeCusCodeData fee in invoiceLine.FeeCusCodes)
					{
						if (fee.CY_IsOverridden)
						{
							entryLine.Fees.GetOrAddFeeByFeeType(fee.CY_Code).CF_ChargeAmount += fee.CY_FeeAmount;
						}
					}
				}

				if (!entryLine.US_SupLine && invoiceLine.US_OverrideDuty)
				{
					entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount += invoiceLine.US_Duty;
				}
				else if (entryLine.US_SupAdditionalLine && invoiceLine.US_OverrideSupAdditionalTariff1Duty)
				{
					entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount += invoiceLine.US_SupAdditionalTariff1Duty;
				}
				else if (entryLine.US_SupAdditionalLine2 && invoiceLine.US_OverrideSupAdditionalTariff2Duty)
				{
					entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount += invoiceLine.US_SupAdditionalTariff2Duty;
				}
				else if (entryLine.US_SupAdditionalLine3 && invoiceLine.US_OverrideSupAdditionalTariff3Duty)
				{
					entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount += invoiceLine.US_SupAdditionalTariff3Duty;
				}
				else if (entryLine.US_SupAdditionalLine4 && invoiceLine.US_OverrideSupAdditionalTariff4Duty)
				{
					entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount += invoiceLine.US_SupAdditionalTariff4Duty;
				}
				else if (entryLine.US_SupAdditionalLine5 && invoiceLine.US_OverrideSupAdditionalTariff5Duty)
				{
					entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount += invoiceLine.US_SupAdditionalTariff5Duty;
				}
				else if (entryLine.US_SupLine && !entryLine.US_SupAdditionalLine && !entryLine.US_SupAdditionalLine2 && !entryLine.US_SupAdditionalLine3 && !entryLine.US_SupAdditionalLine4 && !entryLine.US_SupAdditionalLine5 && invoiceLine.US_OverrideSupDuty)
				{
					entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount += invoiceLine.US_SupDuty;
				}
			}

			entryLine.US_HasMPF = entryLine.MPFAmount > 0;

			foreach (CusEntryLineFee fee in entryLine.Fees)
			{
				if (EntryChargeTypeList.IsFeeType(fee.CF_ChargeType) || CusFeeCodeConstants.IsExciseTax(fee.CF_ChargeType))
				{
					fee.CF_ChargeAmount = fee.CF_ChargeAmount.Round(2);

					ZDecimal newAmount = entry.FeeAndCharges.GetFeeOrChargeAmount(fee.CF_ChargeType) + fee.CF_ChargeAmount;
					entry.FeeAndCharges.UpdateOrAddCharge(fee.CF_ChargeType, newAmount);
				}
			}
		}

		public void UpdateLineAndHeaderFeeAmountLessThanThreshold(ZString feeType, ZDecimal thresholdAmount)
		{
			CusEntryHeader entry = entryLine.Header;

			entryLine.Fees.UpdateLineAndHeaderFeeAmountLessThanThreshold(feeType, thresholdAmount, entry != null ? entry.Charges : null);
		}

		public bool IsDutyOverridden
		{
			get { return entryLine.IsDutyOverriden; }
		}

		public bool IsDomesticMerchandise
		{
			get { return entryLine.RandomLine != null && entryLine.RandomLine.US_ZoneStatus == ZoneStatusList.Codes.Domestic; }
		}

		public bool IsMPFOverridden
		{
			get
			{
				var result = entryLine.RandomLine.IsMPFOverridden;
				if (result)
				{
					foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
					{
						if (!invoiceLine.IsMPFOverridden)
						{
							result = false;
							break;
						}
					}
				}

				return result;
			}
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

		public bool HasTextileCategoryNo
		{
			get { return dutyData.HasTextileCategoryNo; }
		}

		public ZDecimal CustomsValue
		{
			get { return dutyData.CustomsValue; }
		}

		public ZDecimal SupCustomsValue
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
			get { return dutyData.EntryType; }
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

		IReadOnlyList<ZString> IDutyData.SupTariffs
		{
			get { return dutyData.SupTariffs; }
		}

		IEnumerable<IDutyData> IDutyData.CombineChildLines
		{
			get { return dutyData.CombineChildLines; }
		}

		IEnumerable<IDutyData> IDutyData.CombineAllLines
		{
			get { return dutyData.CombineAllLines; }
		}

		IDutyData IDutyData.CombineParentLine
		{
			get { return dutyData.CombineParentLine; }
		}

		public bool HasCottonCertificate
		{
			get { return dutyData.HasCottonCertificate; }
		}

		public IDutyData ParentTariffLine
		{
			get { return dutyData.ParentTariffLine; }
		}

		public bool IsSecondaryTariffLine
		{
			get { return dutyData.IsSecondaryTariffLine; }
		}

		ZDecimal? IDutyData.ADDutyManual
		{
			get { return dutyData.ADDutyManual; }
		}

		ZDecimal? IDutyData.CVDutyManual
		{
			get { return dutyData.CVDutyManual; }
		}

		#endregion
	}
}
