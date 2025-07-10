using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class FeeDutyData : IFeeCalculationDataProvider
	{
		public FeeDutyData(IFeeCalculationDataProvider dutyData, ZString selectedRateType)
		{
			this.dutyData = dutyData;
			this.selectedRateType = selectedRateType;

			Quantity1 = dutyData.Quantity1;
			Quantity2 = dutyData.Quantity2;
			Quantity3 = dutyData.Quantity3;
		}

		readonly IFeeCalculationDataProvider dutyData;
		readonly ZString selectedRateType;

		#region IDutyData Members

		public ZString Tariff
		{
			get { return dutyData.Tariff; }
		}

		public USCTariff ImportTariff
		{
			get { return dutyData.ImportTariff; }
		}

		public ZDate DateForDutyCalculation
		{
			get { return dutyData.DateForDutyCalculation; }
		}

		public ZDecimal Quantity1
		{
			get;
			set;
		}

		public ZString UQ1
		{
			get { return dutyData.UQ1; }
		}

		public ZDecimal Quantity2
		{
			get;
			set;
		}

		public ZString UQ2
		{
			get { return dutyData.UQ2; }
		}

		public ZDecimal Quantity3
		{
			get;
			set;
		}

		public ZString UQ3
		{
			get { return dutyData.UQ3; }
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
			get { return selectedRateType; }
		}

		public BusinessObjectFactory Factory
		{
			get { return dutyData.Factory; }
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

		public bool IsDomesticMerchandise
		{
			get { return dutyData.IsDomesticMerchandise; }
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

		public ZDecimal? OverriddenTaxRate
		{
			get { return dutyData.OverriddenTaxRate; }
		}

		public ZString OverriddenTaxRateUQ
		{
			get { return dutyData.OverriddenTaxRateUQ; }
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
			get { return ZDecimal.Zero; }
		}

		ZString IDutyData.ADDCaseRateTypeQualifier
		{
			get { return dutyData.ADDCaseRateTypeQualifier; }
		}

		ZDecimal IDutyData.CVDQuantity
		{
			get { return ZDecimal.Zero; }
		}

		ZString IDutyData.CVDCaseRateTypeQualifier
		{
			get { return dutyData.CVDCaseRateTypeQualifier; }
		}

		bool IDutyData.HasTextileCategoryNo
		{
			get { return dutyData.HasTextileCategoryNo; }
		}

		ZDecimal? IDutyData.ADDutyManual
		{
			get { return null; }
		}

		ZDecimal? IDutyData.CVDutyManual
		{
			get { return null; }
		}

		#endregion

		#region IFeeCalculationDataProvider Members

		bool IFeeCalculationDataProvider.IsACS
		{
			get => dutyData.IsACS;
		}

		bool IFeeCalculationDataProvider.IsFeeOverriden(string feeCode)
		{
			return dutyData.IsFeeOverriden(feeCode);
		}

		void IFeeCalculationDataProvider.SetFeeResult(string feeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			dutyData.SetFeeResult(feeCode, amount, feeCalculationInternalData);
		}

		ZString IFeeCalculationDataProvider.GetSelectedRateType(string feeCode)
		{
			return dutyData.GetSelectedRateType(feeCode);
		}

		ZDateTime IFeeCalculationDataProvider.DateForMPFCalculation
		{
			get { return dutyData.DateForMPFCalculation; }
		}

		ZDecimal? IFeeCalculationDataProvider.OverriddenTaxRate
		{
			get { return dutyData.OverriddenTaxRate; }
		}

		ZString IFeeCalculationDataProvider.OverriddenTaxRateUQ
		{
			get { return dutyData.OverriddenTaxRateUQ; }
		}

		ZString IFeeCalculationDataProvider.TaxCode
		{
			get { return dutyData.TaxCode; }
		}

		ZString IFeeCalculationDataProvider.TaxRateType
		{
			get { return dutyData.TaxRateType; }
		}

		ZString IFeeCalculationDataProvider.TaxComputationCode
		{
			get { return dutyData.TaxComputationCode; }
		}

		ZDecimal IFeeCalculationDataProvider.TaxRateQuantity
		{
			get { return dutyData.TaxRateQuantity; }
		}

		ZDecimal IFeeCalculationDataProvider.DairyQty
		{
			get { return dutyData.DairyQty; }
		}

		ZString IFeeCalculationDataProvider.VisaNumber
		{
			get { return dutyData.VisaNumber; }
		}

		IEnumerable<IDutyData> IFeeCalculationDataProvider.SecondaryLines
		{
			get { return dutyData.SecondaryLines; }
		}

		#endregion
	}
}
