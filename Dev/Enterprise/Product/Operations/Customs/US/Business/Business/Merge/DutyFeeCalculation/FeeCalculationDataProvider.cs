using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class FeeCalculationDataProvider : IFeeCalculationDataProvider
	{
		public FeeCalculationDataProvider(IFeeCalculationDataProvider dataProvider, ZDecimal customsValue)
		{
			this.dataProvider = dataProvider;
			this.customsValue = customsValue;
		}
		readonly IFeeCalculationDataProvider dataProvider;
		readonly ZDecimal customsValue;

		#region IFeeCalculationDataProvider Members

		bool IFeeCalculationDataProvider.IsACS => dataProvider.IsACS;

		bool IFeeCalculationDataProvider.IsFeeOverriden(string feeCode)
		{
			return dataProvider.IsFeeOverriden(feeCode);
		}

		void IFeeCalculationDataProvider.SetFeeResult(string feeCode, decimal amount, FeeCalculationInternalData ddpData)
		{
			dataProvider.SetFeeResult(feeCode, amount, ddpData);
		}

		ZString IFeeCalculationDataProvider.GetSelectedRateType(string feeCode)
		{
			return dataProvider.GetSelectedRateType(feeCode);
		}

		ZDateTime IFeeCalculationDataProvider.DateForMPFCalculation => dataProvider.DateForMPFCalculation;

		ZDecimal? IFeeCalculationDataProvider.OverriddenTaxRate => dataProvider.OverriddenTaxRate;

		ZString IFeeCalculationDataProvider.OverriddenTaxRateUQ => dataProvider.OverriddenTaxRateUQ;

		ZString IFeeCalculationDataProvider.TaxCode => dataProvider.TaxCode;

		ZString IFeeCalculationDataProvider.TaxRateType => dataProvider.TaxRateType;

		ZString IFeeCalculationDataProvider.TaxComputationCode => dataProvider.TaxComputationCode;

		ZDecimal IFeeCalculationDataProvider.TaxRateQuantity => dataProvider.TaxRateQuantity;

		ZDecimal IFeeCalculationDataProvider.DairyQty => dataProvider.DairyQty;

		ZString IFeeCalculationDataProvider.VisaNumber => dataProvider.VisaNumber;

		IEnumerable<IDutyData> IFeeCalculationDataProvider.SecondaryLines => dataProvider.SecondaryLines;

		#endregion

		#region IDutyData Members

		ZString IDutyData.Tariff => dataProvider.Tariff;

		USCTariff IDutyData.ImportTariff => dataProvider.ImportTariff;

		ZDate IDutyData.DateForDutyCalculation => dataProvider.DateForDutyCalculation;

		ZDecimal IDutyData.Quantity1 => dataProvider.Quantity1;

		ZString IDutyData.UQ1 => dataProvider.UQ1;

		ZDecimal IDutyData.Quantity2 => dataProvider.Quantity2;

		ZString IDutyData.UQ2 => dataProvider.UQ2;

		ZDecimal IDutyData.Quantity3 => dataProvider.Quantity3;

		ZString IDutyData.UQ3 => dataProvider.UQ3;

		ZDecimal IDutyData.CustomsValue => customsValue;

		ZDecimal IDutyData.SupCustomsValue => dataProvider.SupCustomsValue;

		ZString IDutyData.SpecialProgramsIndicatorPrimary => dataProvider.SpecialProgramsIndicatorPrimary;

		ZString IDutyData.SpecialProgramsIndicatorCountry => dataProvider.SpecialProgramsIndicatorCountry;

		ZString IDutyData.CountryOfOrigin => dataProvider.CountryOfOrigin;

		ZString IDutyData.SpecialProgramsIndicatorSecondary => dataProvider.SpecialProgramsIndicatorSecondary;

		ZString IDutyData.SelectedRateType => dataProvider.SelectedRateType;

		BusinessObjectFactory IDutyData.Factory => dataProvider.Factory;

		ZDecimal IDutyData.ValueForADD => dataProvider.ValueForADD;

		ZDecimal IDutyData.ADDDepositRate => dataProvider.ADDDepositRate;

		ZString IDutyData.ADDCaseRateTypeQualifier => dataProvider.ADDCaseRateTypeQualifier;

		ZDecimal IDutyData.ADDQuantity => dataProvider.ADDQuantity;

		ZDecimal? IDutyData.ADDutyManual => dataProvider.ADDutyManual;

		ZDecimal IDutyData.ValueForCVD => dataProvider.ValueForCVD;

		ZDecimal IDutyData.CVDDepositRate => dataProvider.CVDDepositRate;

		ZString IDutyData.CVDCaseRateTypeQualifier => dataProvider.CVDCaseRateTypeQualifier;

		ZDecimal IDutyData.CVDQuantity => dataProvider.CVDQuantity;

		ZDecimal? IDutyData.CVDutyManual => dataProvider.CVDutyManual;

		IDutyData IDutyData.ParentTariffLine => dataProvider.ParentTariffLine;

		bool IDutyData.IsCottonFeeExemptIndicated => dataProvider.IsCottonFeeExemptIndicated;

		bool IDutyData.HasCottonCertificate => dataProvider.HasCottonCertificate;

		ZBool IDutyData.IsSetXLine => dataProvider.IsSetXLine;

		ZBool IDutyData.IsSetVLine => dataProvider.IsSetVLine;

		bool IDutyData.IsAMSFeeExempt => dataProvider.IsAMSFeeExempt;

		bool IDutyData.IsRaspberryFeeExempt => dataProvider.IsRaspberryFeeExempt;

		ZString IDutyData.EntryType => dataProvider.EntryType;

		bool IDutyData.IsClearedInPR => dataProvider.IsClearedInPR;

		bool IDutyData.IsSecondaryTariffLine => dataProvider.IsSecondaryTariffLine;

		bool IDutyData.IsDomesticMerchandise => dataProvider.IsDomesticMerchandise;

		bool IDutyData.HasTextileCategoryNo => dataProvider.HasTextileCategoryNo;

		bool IDutyData.IsCombineSecondaryTariffLine => dataProvider.IsCombineSecondaryTariffLine;

		IEnumerable<IDutyData> IDutyData.CombineChildLines => dataProvider.CombineChildLines;

		IReadOnlyList<ZString> IDutyData.SupTariffs => dataProvider.SupTariffs;

		IDutyData IDutyData.CombineParentLine => dataProvider.CombineParentLine;

		IEnumerable<IDutyData> IDutyData.CombineAllLines => dataProvider.CombineAllLines;

		#endregion
	}
}
