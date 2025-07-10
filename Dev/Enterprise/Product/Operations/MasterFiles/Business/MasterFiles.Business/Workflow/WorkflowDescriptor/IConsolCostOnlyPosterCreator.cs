using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IConsolCostOnlyPosterCreator
	{
		IProcessor CreateConsolCostOnlyPoster(IWorkflowProvider provider);
	}
}
