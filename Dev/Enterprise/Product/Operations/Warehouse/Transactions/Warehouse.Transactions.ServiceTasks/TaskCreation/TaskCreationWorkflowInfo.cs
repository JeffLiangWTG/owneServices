using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class TaskCreationWorkflowInfo
	{
		public TaskCreationWorkflowInfo(
			string nameForLog,
			ZGuid branchPK,
			ZGuid warehousePK,
			ZGuid releaseGroupPK,
			IWorkflowProvider workflowProvider)
		{
			NameForLog = Argument.NotNull(nameForLog, nameof(nameForLog));
			BranchPK = branchPK;
			WarehousePK = warehousePK;
			ReleaseGroupPK = releaseGroupPK;
			WorkflowProvider = Argument.NotNull(workflowProvider, nameof(workflowProvider));
		}

		public string NameForLog { get; }
		public ZGuid BranchPK { get; }
		public ZGuid WarehousePK { get; }
		public ZGuid ReleaseGroupPK { get; }
		public IWorkflowProvider WorkflowProvider { get; }
	}
}
