using CargoWise.Definitions;
namespace Enterprise.MasterFiles.Business
{
	public interface ISpecificDocumentBusinessContextProvider
	{
		BusinessContext[] GetDocumentBusinessContext(IWorkflowProvider workflowProvider);
	}
}