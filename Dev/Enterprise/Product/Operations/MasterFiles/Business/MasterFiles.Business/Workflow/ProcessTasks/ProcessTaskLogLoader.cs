using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Workflow;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class ProcessTaskLogLoader : IProcessTaskLogLoader<ProcessTaskWrapper>
	{
		public Task<IEnumerable<IStmALogWrapper>> GetStatusChangeLogsAsync(ProcessTaskWrapper processTaskWrapper)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, processTaskWrapper.ProcessTask.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode);
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + " desc";
			var result = processTaskWrapper.ProcessTask.Factory.Load<StmALog>(query).Select(WrapStmLog);
			return Task.FromResult(result);
		}

		IStmALogWrapper WrapStmLog(StmALog log) => new StmALogWrapper(log);
	}
}
