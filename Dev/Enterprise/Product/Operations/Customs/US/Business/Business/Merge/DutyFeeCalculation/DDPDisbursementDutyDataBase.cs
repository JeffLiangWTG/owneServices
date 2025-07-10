using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.DDPDisbursementCalculation;

namespace Enterprise.Customs.US.Business
{
	public abstract class DDPDisbursementDutyDataBase : IEntryLineOrInvoiceLineDutyData, IFeeCalculationDataProvider
	{
		public DDPDisbursementDutyDataBase(JobComInvoiceLine invoiceLine, IDutyData dutyData, Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> chargesAndFees, Dictionary<JobComInvoiceLine, CustomsValues> customsValues, bool calculateMPF)
		{
			this.invoiceLine = invoiceLine;
			this.dutyData = dutyData;
			this.chargesAndFees = chargesAndFees;
			this.customsValues = customsValues;
			this.calculateMPF = calculateMPF;
		}

		protected readonly JobComInvoiceLine invoiceLine;
		protected readonly IDutyData dutyData;
		protected readonly Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> chargesAndFees;
		protected readonly Dictionary<JobComInvoiceLine, CustomsValues> customsValues;
		protected readonly bool calculateMPF;

		public void SetFeeResult(string feeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			SetDutyOrFeeResult(feeCode, amount, feeCalculationInternalData);
		}

		void SetDutyOrFeeResult(string feeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			decimal amountToSet = amount;

			if (feeCode == Core.Constants.USCustoms.FeeCodes.Cotton && amountToSet > 0m && amountToSet < CottonFeeCalculator.ThresholdCottonFeeAmount && invoiceLine.Declaration != null && invoiceLine.Declaration.IsCottonFeeDeMinimusApplicable())
			{
				decimal totalCottonFee = 0m;

				CottonFeeCalculator cottonFeeCalculator = new CottonFeeCalculator();
				foreach (JobComInvoiceLine line in invoiceLine.CusEntryLine.InvoiceLines)
				{
					totalCottonFee += cottonFeeCalculator.CalculateFee(line).Amount;
				}

				if (totalCottonFee < CottonFeeCalculator.ThresholdCottonFeeAmount)
				{
					amountToSet = 0m;
					feeCalculationInternalData.NoneCustomsValueAmount = 0m;
				}
			}

			if (feeCode != Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing || calculateMPF)
			{
				chargesAndFees.SetDutyFeeCalculationResult(invoiceLine, feeCode, new DDPCalculationResultData(amountToSet, feeCalculationInternalData));
			}
		}

		void IEntryLineOrInvoiceLineDutyData.SetDutyResult(IDutyResult dutyResult)
		{
			var amountToSet = dutyResult.TotalAmount.Amount;
			var feeCalculationInternalData = DDPCalculationHelper.GetFeeCalculationInternalDataFromIDutyResult(dutyResult);

			if (ParentTariffLine is IDutyData parentTariffLine && parentTariffLine.ImportTariff is USCTariff tariff && tariff.Applies(TariffRuleList.Codes.AdditionalTariffs, invoiceLine.EffectiveDateForDutyRate))
			{
				var invoiceLineToGetCalculationResultData = invoiceLine;
				if (parentTariffLine is DDPDisbursementDutyDataBase dutyDataLine)
				{
					invoiceLineToGetCalculationResultData = dutyDataLine.invoiceLine;
				}

				var existingFeeCalculationInternalData = new FeeCalculationInternalData();
				var existingDDPResultData = chargesAndFees.GetDDPCalculationResultData(invoiceLineToGetCalculationResultData, Core.Constants.Customs.CusEntryFeeTypes.DutyAmount);
				if (existingDDPResultData != null)
				{
					amountToSet += existingDDPResultData.Amount;
					existingFeeCalculationInternalData = existingDDPResultData.InternalData;
				}

				feeCalculationInternalData = DDPCalculationHelper.CombineFeeCalculationInternalDatas(feeCalculationInternalData, existingFeeCalculationInternalData);

				if (invoiceLineToGetCalculationResultData.PK != invoiceLine.PK)
				{
					chargesAndFees.SetDutyFeeCalculationResult(invoiceLineToGetCalculationResultData, Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, new DDPCalculationResultData());
				}
			}

			this.SetFeeResult(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, amountToSet, feeCalculationInternalData);
		}

		void IEntryLineOrInvoiceLineDutyData.SetDutyFeeChargeAmount(string chargeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			SetDutyOrFeeResult(chargeCode, amount, feeCalculationInternalData);
		}

		ZDecimal IEntryLineOrInvoiceLineDutyData.GetDutyFeeChargeAmount(string chargeCode)
		{
			return chargesAndFees.GetChargeOrFeeAmount(invoiceLine, chargeCode);
		}

		public abstract IEnumerable<IFeeCalculationDataProvider> FeeDataProviders { get; }

		ZString IEntryLineOrInvoiceLineDutyData.CalculateException
		{
			get => invoiceLine.TariffCalculateExceptionMessage;
			set => invoiceLine.TariffCalculateExceptionMessage = value;
		}

		IEnumerable<IEntryLineOrInvoiceLineDutyData> IEntryLineOrInvoiceLineDutyData.SecondaryLines
		{
			get { return SecondaryLinesCore; }
		}

		protected abstract IEnumerable<IEntryLineOrInvoiceLineDutyData> SecondaryLinesCore
		{
			get;
		}

		IEnumerable<IDutyData> IEntryLineOrInvoiceLineDutyData.ChildLines
		{
			get
			{
				var parentLine = invoiceLine.ParentTariffLine ?? invoiceLine;
				var childLines = parentLine.ChildLines;
				foreach (var line in childLines)
				{
					yield return new DDPDisbursementSupDutyData(line, chargesAndFees, customsValues, calculateMPF);
				}
			}
		}
		IEntryLineOrInvoiceLineDutyData IEntryLineOrInvoiceLineDutyData.ParentLine => ParentLineCore;

		protected abstract IEntryLineOrInvoiceLineDutyData ParentLineCore { get; }

		public virtual void CalculateNormalDutyIfRequired(ILineDutyFeeCalculator dutyFeeCalculator)
		{
		}

		#region IEntryLineOrInvoiceLineDutyData Members

		ZGuid IEntryLineOrInvoiceLineDutyData.PK
		{
			get { return invoiceLine.PK; }
		}

		bool IEntryLineOrInvoiceLineDutyData.IsDutyFreeSPIClaimed
		{
			get { return invoiceLine.IsDutyFreeSPIClaimed; }
		}

		bool IEntryLineOrInvoiceLineDutyData.IsRecon
		{
			get { return invoiceLine.IsRecon; }
		}

		ZDecimal IEntryLineOrInvoiceLineDutyData.TotalCustomsValueIncludingSecondaryLines
		{
			get
			{
				ZDecimal result = GetCustomsValueUnrounded();

				foreach (IEntryLineOrInvoiceLineDutyData secondary in SecondaryLinesCore)
				{
					result += secondary.CustomsValue;
				}

				return result.Round(0);
			}
		}

		public bool HasMPF
		{
			get { return invoiceLine.US_HasMPF; }
			set { invoiceLine.US_HasMPF = value; }
		}

		ZBool IsDeductADD_CVDDuty
		{
			get { return (invoiceLine.InvoiceHeader.US_DeductADDCVDDuty == YesNoDefaultList.Codes.Yes); }
		}

		ZDecimal IDutyData.ValueForADD
		{
			get
			{
				var retVal = ZDecimal.Zero;
				if (IsDeductADD_CVDDuty)
				{
					retVal = invoiceLine.ADDDepositValueInLocalCurrency;
					if (retVal == ZDecimal.Zero)
					{
						retVal = ((IDutyData)this).CustomsValue;
					}
				}
				return retVal;
			}
		}

		ZDecimal IDutyData.ADDDepositRate
		{
			get
			{
				var retVal = ZDecimal.Zero;
				if (IsDeductADD_CVDDuty)
				{
					retVal = dutyData.ADDDepositRate;
				}
				return retVal;
			}
		}

		ZDecimal IDutyData.ValueForCVD
		{
			get
			{
				var retVal = ZDecimal.Zero;
				if (IsDeductADD_CVDDuty)
				{
					retVal = invoiceLine.CVDDepositValueInLocalCurrency;
					if (retVal == ZDecimal.Zero)
					{
						retVal = ((IDutyData)this).CustomsValue;
					}
				}
				return retVal;
			}
		}

		ZDecimal IDutyData.CVDDepositRate
		{
			get
			{
				var retVal = ZDecimal.Zero;
				if (IsDeductADD_CVDDuty)
				{
					retVal = dutyData.CVDDepositRate;
				}
				return retVal;
			}
		}

		ZDecimal IDutyData.ADDQuantity
		{
			get
			{
				var retVal = ZDecimal.Zero;
				if (IsDeductADD_CVDDuty)
				{
					retVal = dutyData.ADDQuantity;
				}
				return retVal;
			}
		}

		ZString IDutyData.ADDCaseRateTypeQualifier
		{
			get
			{
				var retVal = ZString.Empty;
				if (IsDeductADD_CVDDuty)
				{
					retVal = dutyData.ADDCaseRateTypeQualifier;
				}
				return retVal;
			}
		}

		ZDecimal IDutyData.CVDQuantity
		{
			get
			{
				var retVal = ZDecimal.Zero;
				if (IsDeductADD_CVDDuty)
				{
					retVal = dutyData.CVDQuantity;
				}
				return retVal;
			}
		}

		ZString IDutyData.CVDCaseRateTypeQualifier
		{
			get
			{
				var retVal = ZString.Empty;
				if (IsDeductADD_CVDDuty)
				{
					retVal = dutyData.CVDCaseRateTypeQualifier;
				}
				return retVal;
			}
		}

		void IEntryLineOrInvoiceLineDutyData.StoreAdjustedDerivedCustomsValue(ZDecimal derivedCustomsValue)
		{
			invoiceLine.US_DerivedCV = derivedCustomsValue;
		}

		void IEntryLineOrInvoiceLineDutyData.UpdateInvoiceLinesLinkToEntryLineForDerivedDutyCalculation(ZGuid newEntryLinePK)
		{
			//nothing to do
		}

		void IEntryLineOrInvoiceLineDutyData.UpdateLineAndHeaderFeeAmountLessThanThreshold(ZString feeType, ZDecimal thresholdAmount)
		{
			//nothing to do
		}

		bool IEntryLineOrInvoiceLineDutyData.IsDutyOverridden
		{
			get { return IsSup ? invoiceLine.US_OverrideSupDuty : invoiceLine.US_OverrideDuty; }
		}

		void IEntryLineOrInvoiceLineDutyData.RollUpFees(IDutyDataLineHeader entry)
		{
			//nothing to do
		}

		bool IEntryLineOrInvoiceLineDutyData.IsMPFOverridden
		{
			get { return invoiceLine.IsMPFOverridden; }
		}

		#endregion

		#region IDutyData Members

		protected virtual ZString TariffCore => dutyData.Tariff;

		ZString IDutyData.Tariff
		{
			get { return TariffCore; }
		}

		protected virtual USCTariff ImportTariffCore => dutyData.ImportTariff;

		USCTariff IDutyData.ImportTariff
		{
			get { return ImportTariffCore; }
		}

		ZDate IDutyData.DateForDutyCalculation
		{
			get { return dutyData.DateForDutyCalculation; }
		}

		ZDecimal IDutyData.Quantity1
		{
			get { return dutyData.Quantity1; }
		}

		ZString IDutyData.UQ1
		{
			get { return dutyData.UQ1; }
		}

		ZDecimal IDutyData.Quantity2
		{
			get { return dutyData.Quantity2; }
		}

		ZString IDutyData.UQ2
		{
			get { return dutyData.UQ2; }
		}

		ZDecimal IDutyData.Quantity3
		{
			get { return dutyData.Quantity3; }
		}

		ZString IDutyData.UQ3
		{
			get { return dutyData.UQ3; }
		}

		ZDecimal IDutyData.CustomsValue
		{
			get { return GetCustomsValueUnrounded(); }
		}

		ZDecimal IDutyData.SupCustomsValue
		{
			get { return GetSupCustomsValue(); }
		}

		protected virtual ZDecimal GetSupCustomsValue()
		{
			return dutyData.SupCustomsValue;
		}

		bool IDutyData.IsCombineSecondaryTariffLine
		{
			get { return dutyData.IsCombineSecondaryTariffLine; }
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

		IReadOnlyList<ZString> IDutyData.SupTariffs
		{
			get { return dutyData.SupTariffs; }
		}

		ZDecimal GetCustomsValueUnrounded()
		{
			return customsValues.GetCustomsValue(invoiceLine, IsSup);
		}

		protected abstract bool IsSup
		{
			get;
		}

		ZString IDutyData.SpecialProgramsIndicatorPrimary
		{
			get { return dutyData.SpecialProgramsIndicatorPrimary; }
		}

		ZString IDutyData.SpecialProgramsIndicatorCountry
		{
			get { return dutyData.SpecialProgramsIndicatorCountry; }
		}

		ZString IDutyData.CountryOfOrigin
		{
			get { return dutyData.CountryOfOrigin; }
		}

		ZString IDutyData.SpecialProgramsIndicatorSecondary
		{
			get { return dutyData.SpecialProgramsIndicatorSecondary; }
		}

		ZString IDutyData.SelectedRateType
		{
			get { return dutyData.SelectedRateType; }
		}

		BusinessObjectFactory IDutyData.Factory
		{
			get { return invoiceLine.Factory; }
		}

		public bool IsSecondaryTariffLine
		{
			get { return dutyData.IsSecondaryTariffLine; }
		}

		public ZBool IsSetXLine
		{
			get { return invoiceLine.IsSetXLine; }
		}

		public ZBool IsSetVLine
		{
			get { return invoiceLine.IsSetVLine; }
		}

		public IDutyData ParentTariffLine
		{
			get { return ParentTariffLineCore; }
		}

		protected abstract IDutyData ParentTariffLineCore { get; }

		public bool IsCottonFeeExemptIndicated
		{
			get { return invoiceLine.IsCottonFeeExemptIndicated; }
		}

		bool IDutyData.HasCottonCertificate
		{
			get { return invoiceLine.HasCottonCertificate; }
		}

		public ZString EntryType
		{
			get { return invoiceLine.Declaration != null ? invoiceLine.Declaration.US_EntryType : ZString.Empty; }
		}

		public bool IsClearedInPR
		{
			get { return invoiceLine.Declaration != null && invoiceLine.Declaration.IsClearedInPR; }
		}

		public bool IsAMSFeeExempt
		{
			get { return invoiceLine.IsAMSFeeExempt; }
		}

		public bool IsRaspberryFeeExempt
		{
			get { return invoiceLine.IsRaspberryFeeExempt; }
		}

		bool IDutyData.IsDomesticMerchandise
		{
			get { return invoiceLine.IsDomesticMerchandise; }
		}

		bool IDutyData.HasTextileCategoryNo
		{
			get { return dutyData.HasTextileCategoryNo; }
		}

		ZDecimal? IDutyData.ADDutyManual
		{
			get
			{
				return IsDeductADD_CVDDuty || !IsSup ? dutyData.ADDutyManual : null;
			}
		}

		ZDecimal? IDutyData.CVDutyManual
		{
			get
			{
				return IsDeductADD_CVDDuty || !IsSup ? dutyData.CVDutyManual : null;
			}
		}

		#endregion

		#region IFeeCalculationDataProvider Members

		public bool IsFeeOverriden(string feeCode)
		{
			return ((IFeeCalculationDataProvider)invoiceLine).IsFeeOverriden(feeCode);
		}

		public ZString GetSelectedRateType(string feeCode)
		{
			return ((IFeeCalculationDataProvider)invoiceLine).GetSelectedRateType(feeCode);
		}

		public ZDecimal CustomsValueForMPFCalculation
		{
			get { return GetCustomsValueUnrounded(); }
		}

		ZDateTime IFeeCalculationDataProvider.DateForMPFCalculation
		{
			get { return invoiceLine.DateForMPFCalculation; }
		}

		public abstract ZDecimal? OverriddenTaxRate { get; }
		public abstract ZString OverriddenTaxRateUQ { get; }
		public abstract ZString TaxCode { get; }
		public abstract ZString TaxRateType { get; }
		public abstract ZString TaxComputationCode { get; }
		public abstract ZDecimal TaxRateQuantity { get; }

		bool IFeeCalculationDataProvider.IsACS
		{
			get => ((IFeeCalculationDataProvider)invoiceLine).IsACS;
		}

		ZDecimal IFeeCalculationDataProvider.DairyQty
		{
			get { return IsSup ? ZDecimal.Zero : invoiceLine.DairyQty; }
		}

		ZString IFeeCalculationDataProvider.VisaNumber
		{
			get { return invoiceLine.US_VisaNo; }
		}

		IEnumerable<IDutyData> IFeeCalculationDataProvider.SecondaryLines
		{
			get { return SecondaryLinesCore.Cast<IDutyData>(); }
		}

		#endregion

		#region Implementation

		public override bool Equals(object obj)
		{
			DDPDisbursementDutyDataBase another = obj as DDPDisbursementDutyDataBase;

			return another != null && another.invoiceLine == invoiceLine && another.IsSup == IsSup;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

	}
}
