using System;

namespace Enterprise.MasterFiles.Integration
{
	public interface IAccUsageCollectorProvider
	{
		void ReportGeneralLedgerProcess(Guid companyPk);

		void Report(string featureCode, Guid companyPk);
	}
}
