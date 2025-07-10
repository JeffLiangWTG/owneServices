using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IGenerateARInvoiceToEdocsProcessorCreator
	{
		IProcessor GenerateARInvoiceToEdocsProcessor(IWorkflowProvider provider);
	}
}
