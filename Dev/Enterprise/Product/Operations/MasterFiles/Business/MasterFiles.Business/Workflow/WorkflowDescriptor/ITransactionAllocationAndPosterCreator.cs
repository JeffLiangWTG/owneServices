using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface ITransactionAllocationAndPosterCreator
	{
		IProcessor CreateTransactionAllocationAndPoster(IWorkflowProvider provider);
	}
}
