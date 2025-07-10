
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business
{
	public interface IInvoicingSecurityCheckpointProvider
	{
		SecurityCheckpoint InvoicingCheckpoint { get; }
	}
}
