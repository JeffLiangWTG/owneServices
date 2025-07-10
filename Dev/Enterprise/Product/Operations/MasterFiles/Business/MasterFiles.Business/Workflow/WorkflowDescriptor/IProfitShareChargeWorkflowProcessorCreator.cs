using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IProfitShareChargeWorkflowProcessorCreator
	{
		IProcessor CreateProfitShareChargeWorkflowProcessor(IWorkflowProvider provider);
	}
}
