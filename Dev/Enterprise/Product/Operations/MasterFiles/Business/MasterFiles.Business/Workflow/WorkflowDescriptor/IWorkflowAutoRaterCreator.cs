using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IWorkflowAutoRaterCreator
	{
		IProcessor CreateWorkflowAutoRater(IWorkflowProvider plugIn, bool autoRateRevenue, bool autoRateCosts, bool excludeConsolLevelCharges);
	}
}
