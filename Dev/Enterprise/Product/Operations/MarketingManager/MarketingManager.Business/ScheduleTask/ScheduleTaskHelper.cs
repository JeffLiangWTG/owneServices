using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public static class ScheduleTaskHelper
	{
		public static StmScheduleTask GetScheduleTaskByScheduleType(string scheduleType)
		{
			return new BusinessObjectFactory().LoadTop1<StmScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, scheduleType));
		}

		public static ZDateTime GetNextScheduleServiceTaskDateTime(StmScheduleTask task)
		{
			var result = ZDateTime.Empty;

			var isActive = task?.S5_IsActive ?? false;
			var nextScheduledPrintRunTimeUtc = task?.S5_NextScheduledPrintRunTimeUtc ?? ZDateTime.Empty; // S5_NextScheduledPrintRunTimeUtc is already in UTC

			if (isActive && !nextScheduledPrintRunTimeUtc.IsEmpty)
			{
				result = nextScheduledPrintRunTimeUtc;
			}

			return result;
		}
	}
}
