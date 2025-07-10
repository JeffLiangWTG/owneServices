using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Warehouse.Yard.Business
{
	public interface ICYDJobInvoicingSupporter
	{
		JobInvoicingConsumerType ConsumerType { get; }

		SecurityCheckpoint AuditSecurity { get; }

		SecurityCheckpoint JobInvoicingSecurity { get; }

		OrgHeader DefaultClient { get; }
	}
}
