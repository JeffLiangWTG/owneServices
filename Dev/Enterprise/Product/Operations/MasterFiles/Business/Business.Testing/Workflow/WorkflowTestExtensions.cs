using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class WorkflowTestExtensions
	{
		public static StmALog[] GetWTELogs(this ProcessTask task)
		{
			ZQuery wteLogs = new ZQuery(StmALogSchema.SL_Parent, task.PK);
			wteLogs.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			wteLogs.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			wteLogs.ReLoadExistingRows = true;
			return task.Factory.Load<StmALog>(wteLogs);
		}
	}
}
