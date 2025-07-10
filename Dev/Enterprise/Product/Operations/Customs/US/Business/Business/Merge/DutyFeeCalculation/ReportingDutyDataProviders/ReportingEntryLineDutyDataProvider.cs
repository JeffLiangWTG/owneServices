using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Customs.US;

namespace Enterprise.Customs.US.Business
{
	class ReportingEntryLineDutyDataProvider : IEntryLineDutyDataProvider, IEntryLineOrInvoiceLineDutyData, IFeeCalculationDataProvider
	{
		public ReportingEntryLineDutyDataProvider(CusEntryLine entryLine, ReportingDeclarationDutyDataProvider reportingDeclaration)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
			this.reportingDeclaration = Argument.NotNull(reportingDeclaration, nameof(reportingDeclaration));
			dutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(entryLine);
			feeCalculationDataProvider = entryLine;
		}
		readonly CusEntryLine entryLine;
		readonly ReportingDeclarationDutyDataProvider reportingDeclaration;
		readonly IEntryLineOrInvoiceLineDutyData dutyData;
		readonly IFeeCalculationDataProvider feeCalculationDataProvider;

		public ZDecimal AntidumpingDuty
		{
			get
			{
				if (antidumpingDuty == null)
				{
					antidumpingDuty = new CachedProperty<ZDecimal>(entryLine.Factory, () =>
					{
						return GetFeeAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty) + ChildSecondaryEntryLines.Sum(x => x.AntidumpingDuty);
					});
				}
				return antidumpingDuty.Value;
			}
		}
		CachedProperty<ZDecimal> antidumpingDuty;

		public ZDecimal CountervailingDuty
		{
			get
			{
				if (countervailingDuty == null)
				{
					countervailingDuty = new CachedProperty<ZDecimal>(entryLine.Factory, () =>
					{
						return GetFeeAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty) + ChildSecondaryEntryLines.Sum(x => x.CountervailingDuty);
					});
				}
				return countervailingDuty.Value;
			}
		}
		CachedProperty<ZDecimal> countervailingDuty;

		ZDecimal IEntryLineDutyDataProvider.DutyAmount => GetFeeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount);
		public ZBool US_SupLine => entryLine.US_SupLine;
		public ZBool US_SupAdditionalLine => entryLine.US_SupAdditionalLine;
		public ZBool US_SupAdditionalLine2 => entryLine.US_SupAdditionalLine2;
		public ZBool US_SupAdditionalLine3 => entryLine.US_SupAdditionalLine3;
		public ZBool US_SupAdditionalLine4 => entryLine.US_SupAdditionalLine4;
		public ZBool US_SupAdditionalLine5 => entryLine.US_SupAdditionalLine5;
		public ZDecimal MPFAmount => GetFeeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
		ZBool IEntryLineDutyDataProvider.US_HasMPF => HasMPF;
		IEntryLineDutyDataProvider IEntryLineDutyDataProvider.ParentLine => ParentLine;
		IInvoiceLineDutyDataProvider[] IEntryLineDutyDataProvider.InvoiceLines => InvoiceLines;
		internal ReportingInvoiceLineDutyDataProvider[] InvoiceLines => invoiceLines ?? (invoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => reportingDeclaration.GetOrCreate(x)).ToArray());
		ReportingInvoiceLineDutyDataProvider[] invoiceLines;

		ReportingInvoiceLineDutyDataProvider RandomLine => InvoiceLines.FirstOrDefault();

		ReportingEntryLineDutyDataProvider[] ChildSecondaryEntryLines => childSecondaryEntryLines ?? (childSecondaryEntryLines = entryLine.ChildSecondaryEntryLines.Select(x => reportingDeclaration.GetOrCreate(x)).ToArray());
		ReportingEntryLineDutyDataProvider[] childSecondaryEntryLines;

		ZGuid IEntryLineOrInvoiceLineDutyData.PK => dutyData.PK;
		bool IEntryLineOrInvoiceLineDutyData.IsRecon => dutyData.IsRecon;

		bool IEntryLineOrInvoiceLineDutyData.IsDutyFreeSPIClaimed => RandomLine?.IsDutyFreeSPIClaimed ?? false;
		ZDecimal IEntryLineOrInvoiceLineDutyData.TotalCustomsValueIncludingSecondaryLines => dutyData.TotalCustomsValueIncludingSecondaryLines;

		ZString IEntryLineOrInvoiceLineDutyData.CalculateException { get => dutyData.CalculateException; set => dutyData.CalculateException = value; }

		IEnumerable<IEntryLineOrInvoiceLineDutyData> IEntryLineOrInvoiceLineDutyData.SecondaryLines => ChildSecondaryEntryLines;

		IEnumerable<IFeeCalculationDataProvider> IEntryLineOrInvoiceLineDutyData.FeeDataProviders
		{
			get { yield return this; }
		}

		bool IEntryLineOrInvoiceLineDutyData.IsDutyOverridden => dutyData.IsDutyOverridden;
		public bool HasMPF { get; set; }
		bool IEntryLineOrInvoiceLineDutyData.IsMPFOverridden => dutyData.IsMPFOverridden;
		IEntryLineOrInvoiceLineDutyData IEntryLineOrInvoiceLineDutyData.ParentLine => ParentLine;
		ReportingEntryLineDutyDataProvider ParentLine
		{
			get
			{
				if (parentLine == null)
				{
					var entry = entryLine.ParentLine;
					parentLine = entry == null ? null : reportingDeclaration.GetOrCreate(entry);
				}
				return parentLine;
			}
		}
		ReportingEntryLineDutyDataProvider parentLine;

		IEnumerable<IDutyData> IEntryLineOrInvoiceLineDutyData.ChildLines => ChildLines;
		ReportingEntryLineDutyDataProvider[] ChildLines => childLines ?? (childLines = entryLine.ChildLines.Select(x => reportingDeclaration.GetOrCreate(x)).ToArray());
		ReportingEntryLineDutyDataProvider[] childLines;

		ZString IDutyData.Tariff => dutyData.Tariff;

		USCTariff IDutyData.ImportTariff => dutyData.ImportTariff;

		ZDate IDutyData.DateForDutyCalculation => dutyData.DateForDutyCalculation;

		ZDecimal IDutyData.Quantity1 => dutyData.Quantity1;

		ZString IDutyData.UQ1 => dutyData.UQ1;

		ZDecimal IDutyData.Quantity2 => dutyData.Quantity2;

		ZString IDutyData.UQ2 => dutyData.UQ2;

		ZDecimal IDutyData.Quantity3 => dutyData.Quantity3;

		ZString IDutyData.UQ3 => dutyData.UQ3;

		ZDecimal IDutyData.CustomsValue => dutyData.CustomsValue;

		ZDecimal IDutyData.SupCustomsValue => dutyData.SupCustomsValue;

		ZString IDutyData.SpecialProgramsIndicatorPrimary => ZString.Empty;

		ZString IDutyData.SpecialProgramsIndicatorCountry => RandomLine?.SpecialProgramsIndicatorCountry ?? ZString.Empty;

		ZString IDutyData.CountryOfOrigin => dutyData.CountryOfOrigin;

		ZString IDutyData.SpecialProgramsIndicatorSecondary => dutyData.SpecialProgramsIndicatorSecondary;

		ZString IDutyData.SelectedRateType => dutyData.SelectedRateType;

		BusinessObjectFactory IDutyData.Factory => dutyData.Factory;

		ZDecimal IDutyData.ValueForADD => dutyData.ValueForADD;

		ZDecimal IDutyData.ADDDepositRate => dutyData.ADDDepositRate;

		ZString IDutyData.ADDCaseRateTypeQualifier => dutyData.ADDCaseRateTypeQualifier;

		ZDecimal IDutyData.ADDQuantity => dutyData.ADDQuantity;

		ZDecimal? IDutyData.ADDutyManual => dutyData.ADDutyManual;

		ZDecimal IDutyData.ValueForCVD => dutyData.ValueForCVD;

		ZDecimal IDutyData.CVDDepositRate => dutyData.CVDDepositRate;

		ZString IDutyData.CVDCaseRateTypeQualifier => dutyData.CVDCaseRateTypeQualifier;

		ZDecimal IDutyData.CVDQuantity => dutyData.CVDQuantity;

		ZDecimal? IDutyData.CVDutyManual => dutyData.CVDutyManual;

		IDutyData IDutyData.ParentTariffLine => ParentLine;

		bool IDutyData.IsCottonFeeExemptIndicated => dutyData.IsCottonFeeExemptIndicated;

		bool IDutyData.HasCottonCertificate => dutyData.HasCottonCertificate;

		ZBool IDutyData.IsSetXLine => dutyData.IsSetXLine;

		ZBool IDutyData.IsSetVLine => dutyData.IsSetVLine;

		bool IDutyData.IsAMSFeeExempt => dutyData.IsAMSFeeExempt;

		bool IDutyData.IsRaspberryFeeExempt => dutyData.IsRaspberryFeeExempt;

		ZString IDutyData.EntryType => dutyData.EntryType;

		bool IDutyData.IsClearedInPR => dutyData.IsClearedInPR;

		bool IDutyData.IsSecondaryTariffLine => dutyData.IsSecondaryTariffLine;

		bool IDutyData.IsDomesticMerchandise => dutyData.IsDomesticMerchandise;

		bool IDutyData.HasTextileCategoryNo => dutyData.HasTextileCategoryNo;

		bool IDutyData.IsCombineSecondaryTariffLine => dutyData.IsCombineSecondaryTariffLine;

		IEnumerable<IDutyData> IDutyData.CombineChildLines => combineChildLines ?? (combineChildLines = dutyData.CombineChildLines.Cast<JobComInvoiceLine>().Select(x => reportingDeclaration.GetOrCreate(x)).ToArray());
		ReportingInvoiceLineDutyDataProvider[] combineChildLines;

		IReadOnlyList<ZString> IDutyData.SupTariffs => dutyData.SupTariffs;

		IDutyData IDutyData.CombineParentLine
		{
			get
			{
				if (combineParentLine == null)
				{
					var line = (JobComInvoiceLine)dutyData.CombineParentLine;
					combineParentLine = line == null ? null : reportingDeclaration.GetOrCreate(line);
				}
				return combineParentLine;
			}
		}
		ReportingInvoiceLineDutyDataProvider combineParentLine;

		IEnumerable<IDutyData> IDutyData.CombineAllLines => combineAllLines ?? (combineAllLines = dutyData.CombineAllLines.Cast<CusEntryLine>().Select(x => reportingDeclaration.GetOrCreate(x)).ToArray());
		ReportingEntryLineDutyDataProvider[] combineAllLines;

		bool IFeeCalculationDataProvider.IsACS => feeCalculationDataProvider.IsACS;

		ZDateTime IFeeCalculationDataProvider.DateForMPFCalculation => feeCalculationDataProvider.DateForMPFCalculation;

		ZDecimal? IFeeCalculationDataProvider.OverriddenTaxRate => feeCalculationDataProvider.OverriddenTaxRate;

		ZString IFeeCalculationDataProvider.OverriddenTaxRateUQ => feeCalculationDataProvider.OverriddenTaxRateUQ;

		ZString IFeeCalculationDataProvider.TaxCode => feeCalculationDataProvider.TaxCode;

		ZString IFeeCalculationDataProvider.TaxRateType => feeCalculationDataProvider.TaxRateType;

		ZString IFeeCalculationDataProvider.TaxComputationCode => feeCalculationDataProvider.TaxComputationCode;

		ZDecimal IFeeCalculationDataProvider.TaxRateQuantity => feeCalculationDataProvider.TaxRateQuantity;

		ZDecimal IFeeCalculationDataProvider.DairyQty => feeCalculationDataProvider.DairyQty;

		ZString IFeeCalculationDataProvider.VisaNumber => entryLine.VisaNumber;

		IEnumerable<IDutyData> IFeeCalculationDataProvider.SecondaryLines => ChildSecondaryEntryLines;

		IFees IEntryLineDutyDataProvider.Fees => Fees;
		internal ReportingFeesDutyDataProvider Fees => fees ?? (fees = new ReportingFeesDutyDataProvider());
		ReportingFeesDutyDataProvider fees;

		ZDecimal GetFeeAmount(ZString feeType) => Fees.GetFeeFor(feeType)?.Amount ?? ZDecimal.Zero;

		void IEntryLineOrInvoiceLineDutyData.SetDutyResult(IDutyResult dutyResult)
		{
			Fees.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, dutyResult.TotalAmount.Amount);
		}

		void IEntryLineOrInvoiceLineDutyData.SetDutyFeeChargeAmount(string chargeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			Fees.UpdateOrAddCharge(chargeCode, amount);
		}

		ZDecimal IEntryLineOrInvoiceLineDutyData.GetDutyFeeChargeAmount(string chargeCode)
		{
			return Fees.GetFeeOrChargeAmount(chargeCode);
		}

		void IEntryLineOrInvoiceLineDutyData.StoreAdjustedDerivedCustomsValue(ZDecimal derivedCustomsValue)
		{
		}

		void IEntryLineOrInvoiceLineDutyData.UpdateInvoiceLinesLinkToEntryLineForDerivedDutyCalculation(ZGuid newEntryLinePK)
		{
		}

		void IEntryLineOrInvoiceLineDutyData.RollUpFees(IDutyDataLineHeader entry)
		{
			foreach (IInvoiceLineDutyDataProvider invoiceLine in InvoiceLines)
			{
				if (!US_SupLine)
				{
					foreach (IFee fee in invoiceLine.FeeCusCodes)
					{
						if (fee.IsOverridden)
						{
							Fees[fee.Code].Amount += fee.Amount;
						}
					}
				}

				if (!US_SupLine && invoiceLine.US_OverrideDuty)
				{
					Fees[Core.Constants.Customs.CusEntryFeeTypes.DutyAmount].Amount += invoiceLine.US_Duty;
				}
				else if (US_SupLine && invoiceLine.US_OverrideSupDuty)
				{
					Fees[Core.Constants.Customs.CusEntryFeeTypes.DutyAmount].Amount += invoiceLine.US_SupDuty;
				}
			}

			HasMPF = MPFAmount > 0;

			foreach (var fee in Fees)
			{
				if (EntryChargeTypeList.IsFeeType(fee.Code) || CusFeeCodeConstants.IsExciseTax(fee.Code))
				{
					fee.Amount = fee.Amount.Round(2);

					ZDecimal newAmount = entry.FeeAndCharges.GetFeeOrChargeAmount(fee.Code) + fee.Amount;
					entry.FeeAndCharges.UpdateOrAddCharge(fee.Code, newAmount);
				}
			}
		}

		void IEntryLineOrInvoiceLineDutyData.UpdateLineAndHeaderFeeAmountLessThanThreshold(ZString feeType, ZDecimal thresholdAmount)
		{
			Fees.UpdateLineAndHeaderFeeAmountLessThanThreshold(feeType, thresholdAmount, EntryHeaderDutyDataProvider.Charges);
		}

		ReportingEntryHeaderDutyDataProvider EntryHeaderDutyDataProvider => entryHeaderDutyDataProvider ?? (entryHeaderDutyDataProvider = reportingDeclaration.GetOrCreate(entryLine.Header));
		ReportingEntryHeaderDutyDataProvider entryHeaderDutyDataProvider;

		bool IFeeCalculationDataProvider.IsFeeOverriden(string feeCode)
		{
			return RandomLine?.FeeCusCodes?.GetFeeFor(feeCode)?.IsOverridden ?? false;
		}

		void IFeeCalculationDataProvider.SetFeeResult(string feeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			Fees.UpdateOrAddCharge(feeCode, amount);
		}

		ZString IFeeCalculationDataProvider.GetSelectedRateType(string feeCode)
		{
			return RandomLine?.FeeCusCodes.GetFeeFor(feeCode)?.SelectedRateType ?? ZString.Empty;
		}
	}
}
