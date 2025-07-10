using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IEntryLineDutyDataProvider : IDutyData
	{
		ZDecimal AntidumpingDuty { get; }
		ZDecimal CountervailingDuty { get; }
		ZDecimal DutyAmount { get; }
		ZBool US_SupLine { get; }
		ZBool US_SupAdditionalLine { get; }
		ZBool US_SupAdditionalLine2 { get; }
		ZBool US_SupAdditionalLine3 { get; }
		ZBool US_SupAdditionalLine4 { get; }
		ZBool US_SupAdditionalLine5 { get; }
		ZDecimal MPFAmount { get; }
		ZBool US_HasMPF { get; }
		IEntryLineDutyDataProvider ParentLine { get; }
		IFees Fees { get; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		IInvoiceLineDutyDataProvider[] InvoiceLines { get; }
	}
}
