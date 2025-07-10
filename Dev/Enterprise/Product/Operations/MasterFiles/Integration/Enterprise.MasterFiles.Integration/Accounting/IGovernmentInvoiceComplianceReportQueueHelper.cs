namespace Enterprise.MasterFiles.Integration
{
	public interface IGovernmentInvoiceComplianceReportQueueHelper
	{
		void QueueForComplianceReports(IAccTransactionHeader invoice);
	}
}
