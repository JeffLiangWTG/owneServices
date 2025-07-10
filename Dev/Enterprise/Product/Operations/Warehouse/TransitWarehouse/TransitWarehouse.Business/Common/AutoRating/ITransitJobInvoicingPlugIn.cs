using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Warehouse.Transit.Business
{
	public interface ITransitJobInvoicingPlugIn
	{
		JobInvoicingConsumerType ConsumerType { get; }

		SecurityCheckpoint AuditSecurity { get; }

		SecurityCheckpoint JobInvoicingSecurity { get; }

		IJobInvoicingSupporter InvoicingSupporter { get; }
	}
}
