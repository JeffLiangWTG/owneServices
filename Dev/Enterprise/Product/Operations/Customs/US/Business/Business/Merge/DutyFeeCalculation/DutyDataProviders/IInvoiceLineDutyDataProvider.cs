using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IInvoiceLineDutyDataProvider
	{
		object this[string propertyName] { get; set; }
		ZBool IsADDManual { get; }
		ZBool IsCVDManual { get; }
		bool IsInformal { get; }
		ZDecimal JI_CustomsValue { get; }
		ZDecimal TotalOriginalGoodsValueInUSD { get; }
		ZGuid PK { get; }
		ZShort JI_LineNo { get; }
		ZGuid JI_ParentID { get; }
		ZString JI_AddInfo { get; }
		ZBool US_OverrideDuty { get; }
		ZBool US_OverrideSupDuty { get; }
		ZBool US_OverrideSupAdditionalTariff1Duty { get; }
		ZBool US_OverrideSupAdditionalTariff2Duty { get; }
		ZBool US_OverrideSupAdditionalTariff3Duty { get; }
		ZBool US_OverrideSupAdditionalTariff4Duty { get; }
		ZBool US_OverrideSupAdditionalTariff5Duty { get; }
		ZDecimal US_ADDuty { get; set; }
		ZDecimal US_CVDuty { get; set; }
		ZDecimal US_Duty { get; set; }
		ZDecimal US_SupDuty { get; set; }
		ZDecimal US_SupAdditionalTariff1Duty { get; set; }
		ZDecimal US_SupAdditionalTariff2Duty { get; set; }
		ZDecimal US_SupAdditionalTariff3Duty { get; set; }
		ZDecimal US_SupAdditionalTariff4Duty { get; set; }
		ZDecimal US_SupAdditionalTariff5Duty { get; set; }
		ZDecimal US_PayableMPF { get; set; }
		ZShort? InvoiceDisplaySequence { get; }
		ZString SpecialProgramsIndicatorCountry { get; }
		IFees FeeCusCodes { get; }
		IEnumerable<IEntryLineDutyDataProvider> AllEntryLines { get; }
		ZBool IsQuotaProductExclusion { get; }
		ZBool HasSupTariffOnly { get; }
	}
}
