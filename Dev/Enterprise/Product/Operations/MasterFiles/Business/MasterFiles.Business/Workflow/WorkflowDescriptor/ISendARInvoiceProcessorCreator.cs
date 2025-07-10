using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface ISendARInvoiceProcessorCreator
	{
		IProcessor CreateSendARInvoiceProcessor(IWorkflowProvider provider);
	}
}
