using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IJobInvoiceHeaderProcessorCreator
	{
		IProcessor CreateJobInvoiceHeaderProcessor(IWorkflowProvider provider);
	}
}
