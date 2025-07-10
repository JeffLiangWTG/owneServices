using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.HRM.Common.ServiceTasks
{
	[Serializable]
	public class CancelChangeRequestLogSubscriber : LogSubscriber
	{
		public override string Name => nameof(CancelChangeRequestLogSubscriber);

		public override string[] EventTypes => [AutoEvents.CancelledCode];

		public override string[] TableNames => [GlbStaffChangeRequestSchema.Constants.TableName];

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var cancelledJobs = queuedLogs
				.Select(l => l.SJ_ParentID)
				.Distinct();

			queuedLogs[0].Factory
				.Load<GlbStaffChangeRequest>(new ZQuery(GlbStaffChangeRequestSchema.PK, cancelledJobs))
				.ForEach(r => r.CancelNonStartedTasksAndClosePartiallyCompletedTasks());
		}
	}
}
