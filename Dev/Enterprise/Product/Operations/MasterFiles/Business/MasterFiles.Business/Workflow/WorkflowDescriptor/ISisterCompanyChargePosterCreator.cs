using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface ISisterCompanyChargePosterCreator
	{
		IProcessor CreateSisterCompanyChargePoster(IWorkflowProvider provider);
	}
}
