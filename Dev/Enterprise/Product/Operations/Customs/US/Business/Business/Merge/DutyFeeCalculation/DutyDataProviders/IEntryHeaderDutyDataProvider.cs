using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IEntryHeaderDutyDataProvider : IDutyDataLineHeader
	{
		ZString EntryType { get; }
		ZDecimal InformalFee { get; }
		ZDecimal MPFAmountForEntry { get; }
		IEnumerable<IInvoiceLineDutyDataProvider> InvoiceLines { get; }
		IEnumerable<IEntryLineDutyDataProvider> EntryLines { get; }
	}
}
