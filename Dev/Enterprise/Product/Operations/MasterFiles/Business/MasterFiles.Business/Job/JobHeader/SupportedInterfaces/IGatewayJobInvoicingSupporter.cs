using System.Collections.ObjectModel;

namespace Enterprise.MasterFiles.Business
{
	public interface IGatewayJobInvoicingSupporter : IJobInvoicingSupporter
	{
		ReadOnlyCollection<IJobInvoicingPlugIn> OrderedInvoiceTargets { get; }
	}
}
