using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IPostOverseasAgentChargesProcessorCreator
	{
		IProcessor CreateOverseasAgentChargesPoster(IWorkflowProvider provider);
	}
}
