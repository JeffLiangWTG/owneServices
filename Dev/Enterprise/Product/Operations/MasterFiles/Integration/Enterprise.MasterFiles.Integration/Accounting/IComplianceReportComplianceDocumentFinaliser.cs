using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration.Accounting
{
	public interface IComplianceReportComplianceDocumentFinaliser
	{
		ZBool IsInFinalisedRange(ZString subtype, ZDateTime startDate, ZDateTime endDate, ZString ledger);
	}
}
