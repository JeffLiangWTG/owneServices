using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class DutyDataProxy : IDutyData
	{
		public DutyDataProxy(IDutyData line)
		{
			this.line = line;

			Quantity1 = line.Quantity1;
			Quantity2 = line.Quantity2;
			Quantity3 = line.Quantity3;
			CustomsValue = line.CustomsValue;
			ADDDepositRate = line.ADDDepositRate;
			CVDDepositRate = line.CVDDepositRate;
			ADDCaseRateTypeQualifier = line.ADDCaseRateTypeQualifier;
			CVDCaseRateTypeQualifier = line.CVDCaseRateTypeQualifier;
		}

		readonly IDutyData line;

		#region IDutyData Members

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
			get;
			set;
		}

		public ZString UQ1
		{
			get { return line.UQ1; }
		}

		public ZDecimal Quantity2
		{
			get;
			set;
		}

		public ZString UQ2
		{
			get { return line.UQ2; }
		}

		public ZDecimal Quantity3
		{
			get;
			set;
		}

		public ZString UQ3
		{
			get { return line.UQ3; }
		}

		public ZDecimal CustomsValue
		{
			get;
			set;
		}

		public ZDecimal SupCustomsValue
		{
			get { return line.SupCustomsValue; }
		}

		public ZString SpecialProgramsIndicatorPrimary
		{
			get { return line.SpecialProgramsIndicatorPrimary; }
		}

		public ZString SpecialProgramsIndicatorCountry
		{
			get { return line.SpecialProgramsIndicatorCountry; }
		}

		public ZString CountryOfOrigin
		{
			get { return line.CountryOfOrigin; }
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

		public bool IsCombineSecondaryTariffLine
		{
			get { return line.IsCombineSecondaryTariffLine; }
		}

		IReadOnlyList<ZString> IDutyData.SupTariffs
		{
			get { return line.SupTariffs; }
		}

		IEnumerable<IDutyData> IDutyData.CombineChildLines
		{
			get { return line.CombineChildLines; }
		}

		IEnumerable<IDutyData> IDutyData.CombineAllLines
		{
			get { return line.CombineAllLines; }
		}

		IDutyData IDutyData.CombineParentLine
		{
			get { return line.CombineParentLine; }
		}

		public bool HasCottonCertificate
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

		bool IDutyData.IsDomesticMerchandise
		{
			get { return line.IsDomesticMerchandise; }
		}

		public ZDecimal ValueForADD
		{
			get { return line.ValueForADD; }
		}

		public ZDecimal ADDDepositRate
		{
			get;
			set;
		}

		public ZDecimal ValueForCVD
		{
			get { return line.ValueForCVD; }
		}

		public ZDecimal CVDDepositRate
		{
			get;
			set;
		}

		ZDecimal IDutyData.ADDQuantity
		{
			get { return line.ADDQuantity; }
		}

		public ZString ADDCaseRateTypeQualifier
		{
			get;
			set;
		}

		ZDecimal IDutyData.CVDQuantity
		{
			get { return line.CVDQuantity; }
		}

		public ZString CVDCaseRateTypeQualifier
		{
			get;
			set;
		}

		ZDecimal? IDutyData.ADDutyManual
		{
			get { return line.ADDutyManual; }
		}

		ZDecimal? IDutyData.CVDutyManual
		{
			get { return line.CVDutyManual; }
		}

		bool IDutyData.HasTextileCategoryNo
		{
			get { return line.HasTextileCategoryNo; }
		}

		#endregion

	}
}
