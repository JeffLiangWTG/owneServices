using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface ILocalSisterCompanyChargePosterCreator
	{
		IProcessor CreateLocalSisterCompanyChargePoster(IWorkflowProvider provider);
	}
}
