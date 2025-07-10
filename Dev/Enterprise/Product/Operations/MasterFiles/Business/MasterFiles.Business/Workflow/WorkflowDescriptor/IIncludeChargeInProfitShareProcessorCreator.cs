using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IIncludeChargeInProfitShareProcessorCreator
	{
		IProcessor CreateIncludeChargeInProfitShareProcessor(IWorkflowProvider provider);
	}
}
