using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed public class DutyDataTest : IFeeCalculationDataProvider
	{
		public ZString Tariff { get; set; }

		public USCTariff ImportTariff { get; set; }

		public ZDate DateForDutyCalculation { get; set; }

		public ZDecimal Quantity1 { get; set; }

		public ZString UQ1 { get; set; }

		public ZDecimal Quantity2 { get; set; }

		public ZString UQ2 { get; set; }

		public ZDecimal Quantity3 { get; set; }

		public ZString UQ3 { get; set; }

		public ZDecimal CustomsValue { get; set; }

		public ZDecimal SupCustomsValue { get; set; }

		public ZString SpecialProgramsIndicatorPrimary { get; set; }

		public ZString SpecialProgramsIndicatorCountry { get; set; }

		public ZString CountryOfOrigin { get; set; }

		public ZString SpecialProgramsIndicatorSecondary { get; set; }

		public ZString SelectedRateType { get; set; }

		public BusinessObjectFactory Factory { get; set; }

		public ZString EntryType { get; set; }

		public bool IsClearedInPR { get; set; }

		public bool IsAMSFeeExempt { get; set; }

		public bool IsRaspberryFeeExempt { get; set; }

		public ZBool IsSetXLine { get; set; }

		public ZBool IsSetVLine { get; set; }

		public bool IsCottonFeeExemptIndicated { get; set; }

		public bool HasCottonCertificate { get; set; }

		public bool IsCombineSecondaryTariffLine { get; set; }

		public IReadOnlyList<ZString> SupTariffs { get; set; }

		public IEnumerable<IDutyData> CombineChildLines { get; set; }

		public IEnumerable<IDutyData> CombineAllLines { get; set; }

		public IDutyData CombineParentLine { get; set; }

		public IDutyData ParentTariffLine { get; set; }

		public bool IsSecondaryTariffLine { get; set; }

		public bool IsDomesticMerchandise { get; set; }

		public bool HasTextileCategoryNo { get; set; }

		public ZDecimal? OverriddenTaxRate { get; set; }

		public ZString OverriddenTaxRateUQ { get; set; }

		public ZDecimal ValueForADD { get; set; }

		public ZDecimal ADDDepositRate { get; set; }

		public ZDecimal ValueForCVD { get; set; }

		public ZDecimal CVDDepositRate { get; set; }

		public ZDecimal ADDQuantity { get; set; }

		public ZString ADDCaseRateTypeQualifier { get; set; }

		public ZDecimal CVDQuantity { get; set; }

		public ZString CVDCaseRateTypeQualifier { get; set; }

		public ZDecimal? ADDutyManual
		{
			get;
			set;
		}

		public ZDecimal? CVDutyManual
		{
			get;
			set;
		}

		#region IFeeCalculationDataProvider Members

		bool IFeeCalculationDataProvider.IsACS => false;

		public bool IsFeeOverriden(string feeCode)
		{
			return false;
		}

		public bool IsFeePayable(ZString feeCode)
		{
			return true;
		}

		public void SetFeeResult(string feeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
		}

		public ZString GetSelectedRateType(string feeCode)
		{
			return ZString.Empty;
		}

		public ZDecimal CustomsValueForMPFCalculation
		{
			get { return ZDecimal.Zero; }
		}

		public ZDateTime DateForMPFCalculation
		{
			get { return ZDateTime.Today; }
		}

		public ZString TaxCode
		{
			get { return ZString.Empty; }
		}

		public ZString TaxRateType
		{
			get { return ZString.Empty; }
		}

		public ZString TaxComputationCode
		{
			get { return ZString.Empty; }
		}

		public ZDecimal TaxRateQuantity
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal DairyQty
		{
			get { return ZDecimal.Zero; }
		}

		ZString IFeeCalculationDataProvider.VisaNumber
		{
			get { return ZString.Empty; }
		}

		IEnumerable<IDutyData> IFeeCalculationDataProvider.SecondaryLines
		{
			get { return System.Array.Empty<IDutyData>(); }
		}

		#endregion
	}
}
