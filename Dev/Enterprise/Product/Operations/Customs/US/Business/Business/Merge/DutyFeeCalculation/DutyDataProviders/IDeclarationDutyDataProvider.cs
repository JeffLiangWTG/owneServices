using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IDeclarationDutyDataProvider : IDutyDataLineHeaderProvider
	{
		ZBool IsFormalImport { get; }
		ZBool IsFTZAdmission { get; }
		bool IsFixedTransportInstallations { get; }
		ZBool US_MonthlyFiling { get; }
		IEntryHeaderDutyDataProvider EntrySummaryEntry { get; }
		IEntryHeaderDutyDataProvider FTZEntry { get; }
		IEnumerable<IInvoiceLineDutyDataProvider> InvoiceLines { get; }
	}
}
