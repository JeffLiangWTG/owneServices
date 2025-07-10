using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface ICostPosterCreator
	{
		IProcessor CreateCostPoster(IWorkflowProvider provider);
	}
}
