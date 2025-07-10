using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// this is to return a passed CustomsValue instead of line.CustomsValue.
	/// </summary>
	class LineDutyData : IEntryLineOrInvoiceLineDutyData
	{
		public LineDutyData(IEntryLineOrInvoiceLineDutyData line, ZDecimal customsValue)
		{
			this.line = line;
			this.customsValue = customsValue;
		}

		public ZString SpecialProgramsIndicatorCountry
		{
			get { return line.SpecialProgramsIndicatorCountry; }
		}

		public ZString SpecialProgramsIndicatorPrimary
		{
			get { return line.SpecialProgramsIndicatorPrimary; }
		}

		public bool IsRecon
		{
			get { return line.IsRecon; }
		}

		public ZString CountryOfOrigin
		{
			get { return line.CountryOfOrigin; }
		}

		public ZString Tariff
		{
			get { return line.Tariff; }
		}

		public USCTariff ImportTariff
		{
			get { return line.ImportTariff; }
		}

		public ZDate DateForDutyCalculation
		{
			get { return line.DateForDutyCalculation; }
		}

		public ZDecimal Quantity1
		{
			get { return line.Quantity1; }
		}

		public ZString UQ1
		{
			get { return line.UQ1; }
		}

		public ZDecimal Quantity2
		{
			get { return line.Quantity2; }
		}

		public ZString UQ2
		{
			get { return line.UQ2; }
		}

		public ZDecimal Quantity3
		{
			get { return line.Quantity3; }
		}

		public ZString UQ3
		{
			get { return line.UQ3; }
		}

		public ZDecimal CustomsValue
		{
			get { return customsValue; }
		}

		public ZDecimal SupCustomsValue
		{
			get { return line.SupCustomsValue; }
		}

		public ZString SpecialProgramsIndicatorSecondary
		{
			get { return line.SpecialProgramsIndicatorSecondary; }
		}

		public ZString SelectedRateType
		{
			get { return line.SelectedRateType; }
		}

		public BusinessObjectFactory Factory
		{
			get { return line.Factory; }
		}

		public ZString EntryType
		{
			get { return line.EntryType; }
		}

		public bool IsClearedInPR
		{
			get { return line.IsClearedInPR; }
		}

		public bool IsAMSFeeExempt
		{
			get { return line.IsAMSFeeExempt; }
		}

		public bool IsCombineSecondaryTariffLine
		{
			get { return line.IsCombineSecondaryTariffLine; }
		}

		public IReadOnlyList<ZString> SupTariffs
		{
			get { return line.SupTariffs; }
		}

		IEnumerable<IDutyData> IDutyData.CombineChildLines
		{
			get { return line.ChildLines; }
		}

		IEnumerable<IDutyData> IDutyData.CombineAllLines
		{
			get { return line.CombineAllLines; }
		}

		IDutyData IDutyData.CombineParentLine
		{
			get { return line.CombineParentLine; }
		}

		public bool IsRaspberryFeeExempt
		{
			get { return line.IsRaspberryFeeExempt; }
		}

		public ZBool IsSetXLine
		{
			get { return line.IsSetXLine; }
		}

		public ZBool IsSetVLine
		{
			get { return line.IsSetVLine; }
		}

		public bool IsCottonFeeExemptIndicated
		{
			get { return line.IsCottonFeeExemptIndicated; }
		}

		bool IDutyData.HasCottonCertificate
		{
			get { return line.HasCottonCertificate; }
		}

		public IDutyData ParentTariffLine
		{
			get { return line.ParentTariffLine; }
		}

		public bool IsSecondaryTariffLine
		{
			get { return line.IsSecondaryTariffLine; }
		}

		public bool IsDomesticMerchandise
		{
			get { return line.IsDomesticMerchandise; }
		}

		public bool HasTextileCategoryNo
		{
			get { return line.HasTextileCategoryNo; }
		}

		ZDecimal? IDutyData.ADDutyManual
		{
			get { return line.ADDutyManual; }
		}

		ZDecimal? IDutyData.CVDutyManual
		{
			get { return line.CVDutyManual; }
		}

		readonly IEntryLineOrInvoiceLineDutyData line;
		readonly ZDecimal customsValue;

		#region IEntryLineOrInvoiceLineDutyData Members

		ZGuid IEntryLineOrInvoiceLineDutyData.PK
		{
			get { return line.PK; }
		}

		bool IEntryLineOrInvoiceLineDutyData.HasMPF
		{
			get { return line.HasMPF; }
			set { line.HasMPF = value; }
		}

		bool IEntryLineOrInvoiceLineDutyData.IsDutyFreeSPIClaimed
		{
			get { return line.IsDutyFreeSPIClaimed; }
		}

		IEnumerable<IDutyData> IEntryLineOrInvoiceLineDutyData.ChildLines
		{
			get { return line.ChildLines; }
		}

		IEntryLineOrInvoiceLineDutyData IEntryLineOrInvoiceLineDutyData.ParentLine
		{
			get { return line.ParentLine; }
		}

		ZDecimal IEntryLineOrInvoiceLineDutyData.TotalCustomsValueIncludingSecondaryLines
		{
			get { return customsValue; }
		}

		ZString IEntryLineOrInvoiceLineDutyData.CalculateException
		{
			get => line.CalculateException;
			set => line.CalculateException = value;
		}

		ZDecimal IDutyData.ValueForADD
		{
			get { return line.ValueForADD; }
		}

		ZDecimal IDutyData.ADDDepositRate
		{
			get { return line.ADDDepositRate; }
		}

		ZDecimal IDutyData.ValueForCVD
		{
			get { return line.ValueForCVD; }
		}

		ZDecimal IDutyData.CVDDepositRate
		{
			get { return line.CVDDepositRate; }
		}

		ZDecimal IDutyData.ADDQuantity
		{
			get { return line.ADDQuantity; }
		}

		ZString IDutyData.ADDCaseRateTypeQualifier
		{
			get { return line.ADDCaseRateTypeQualifier; }
		}

		ZDecimal IDutyData.CVDQuantity
		{
			get { return line.CVDQuantity; }
		}

		ZString IDutyData.CVDCaseRateTypeQualifier
		{
			get { return line.CVDCaseRateTypeQualifier; }
		}

		IEnumerable<IEntryLineOrInvoiceLineDutyData> IEntryLineOrInvoiceLineDutyData.SecondaryLines
		{
			get { return line.SecondaryLines; }
		}

		ZDecimal IEntryLineOrInvoiceLineDutyData.GetDutyFeeChargeAmount(string chargeCode)
		{
			return line.GetDutyFeeChargeAmount(chargeCode);
		}

		void IEntryLineOrInvoiceLineDutyData.SetDutyFeeChargeAmount(string chargeCode, decimal chargeAmount, FeeCalculationInternalData feeCalculationInternalData)
		{
			line.SetDutyFeeChargeAmount(chargeCode, chargeAmount, feeCalculationInternalData);
		}

		void IEntryLineOrInvoiceLineDutyData.SetDutyResult(IDutyResult dutyResult)
		{
			line.SetDutyResult(dutyResult);
		}

		void IEntryLineOrInvoiceLineDutyData.StoreAdjustedDerivedCustomsValue(ZDecimal derivedCustomsValue)
		{
			line.StoreAdjustedDerivedCustomsValue(derivedCustomsValue);
		}

		void IEntryLineOrInvoiceLineDutyData.UpdateInvoiceLinesLinkToEntryLineForDerivedDutyCalculation(ZGuid newEntryLinePK)
		{
			line.UpdateInvoiceLinesLinkToEntryLineForDerivedDutyCalculation(newEntryLinePK);
		}

		IEnumerable<IFeeCalculationDataProvider> IEntryLineOrInvoiceLineDutyData.FeeDataProviders
		{
			get { return line.FeeDataProviders; }
		}

		void IEntryLineOrInvoiceLineDutyData.RollUpFees(IDutyDataLineHeader entry)
		{
			line.RollUpFees(entry);
		}

		void IEntryLineOrInvoiceLineDutyData.UpdateLineAndHeaderFeeAmountLessThanThreshold(ZString feeType, ZDecimal thresholdAmount)
		{
			line.UpdateLineAndHeaderFeeAmountLessThanThreshold(feeType, thresholdAmount);
		}

		bool IEntryLineOrInvoiceLineDutyData.IsDutyOverridden
		{
			get { return line.IsDutyOverridden; }
		}

		bool IEntryLineOrInvoiceLineDutyData.IsMPFOverridden
		{
			get { return line.IsMPFOverridden; }
		}

		#endregion
	}
}
