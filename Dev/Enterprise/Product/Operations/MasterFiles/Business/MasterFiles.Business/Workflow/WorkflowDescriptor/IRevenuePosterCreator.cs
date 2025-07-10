using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IRevenuePosterCreator
	{
		IProcessor CreateRevenuePoster(IWorkflowProvider provider);
	}
}
