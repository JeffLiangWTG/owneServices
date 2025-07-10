using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IEntryLine : Customs.Business.IBaseEntryLine
	{
		ZDecimal ExciseTax { get; }
		ZDecimal TotalFeeAmount { get; }
		ZDecimal MPFAmount { get; }
		ZDecimal HMFAmount { get; }
		ZDecimal PayableMPFAmount { get; }
		ZString BaseDutyRateDesc { get; }
		bool NoDutyRateExists { get; }
		ZBool US_SupLine { get; }
		ZBool US_SupAdditionalLine { get; }
		ZBool US_SupAdditionalLine2 { get; }
		ZBool US_SupAdditionalLine3 { get; }
		ZBool US_SupAdditionalLine4 { get; }
		ZBool US_SupAdditionalLine5 { get; }

		ZGuid US_CL_ParentLine { get; }
		bool IsACE { get; }
		bool IsCustomsChargeToBeCalculated { get; }
		bool IsConsumptionFTZ { get; }
		bool IsFTZAdmission { get; }
		ZBool IsSetXLine { get; }
		ZBool IsSetVLine { get; }
		ZDecimal RoundedCustomsValue { get; }
		IEntryLine ParentLine { get; }
		IInvoiceLine FirstInvoiceLineAfterSortedOnInvoiceLineNo { get; }
		IInvoiceLine RandomLine { get; }
		IEnumerable<IInvoiceLine> InvoiceLines { get; }
		IEnumerable<IEntryLine> AllRelatedEntryLines { get; }
		IEnumerable<IEntryLine> ChildSecondaryEntryLines { get; }
	}
}
