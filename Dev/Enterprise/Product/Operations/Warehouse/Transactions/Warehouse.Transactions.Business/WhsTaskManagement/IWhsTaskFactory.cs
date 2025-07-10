using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsTaskFactory
	{
		ProcessTask CreateTask(
			IWorkflowProvider workflowProvider,
			ZString formflowType,
			ZString workflowName,
			ZString taskName,
			ZString staffCode,
			ZShort rawNudge,
			ZString capabilityCode,
			ZGuid releaseGroupPk,
			string taskType = "UDF");
	}
}
