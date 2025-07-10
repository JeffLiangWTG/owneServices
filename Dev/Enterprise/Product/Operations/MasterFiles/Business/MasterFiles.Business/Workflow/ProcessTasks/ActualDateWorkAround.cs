using System;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class ActualDateWorkAround : IActualDateWorkAround
	{
#if DEBUG
		public void SetActualDateForTest(IProcessTask task, ZDateTime value)
		{
			((ProcessTask)task).P9_ActualDateInternal = new ZDateTimeOffset(value);
		}
#endif

		public void SetActualDateAndIKnowIShouldNotBeCallingThis(IProcessTask task, ZDateTimeOffset value)
		{
			((ProcessTask)task).P9_ActualDateInternal = value;
		}

		public void SetActualDateAndIKnowIShouldNotBeCallingThis(IProcessTask task, ZDateTime value)
		{
			SetActualDateAndIKnowIShouldNotBeCallingThis(task, new ZDateTimeOffset(value));
		}
	}

#if DEBUG
	public static class ActualDateWorkAround_Extensions
	{
		public static ZDateTimeOffset GetEstimateDefaultedFromDateForTest(this ProcessTask task) => task.EstimateDefaultedFromDate;
		public static void SetMilestoneActualDateForTest(this ProcessTask task, ZDateTime value) => task.P9_ActualDateInternal = new ZDateTimeOffset(value);
		public static void SetMilestoneActualDateForTest(this ProcessTask task, DateTime value) => task.P9_ActualDateInternal = value;
		public static void SetMilestoneActualDateForTest(this ProcessTask task, ZDateTimeOffset value) => task.P9_ActualDateInternal = value;
		public static void SetMilestoneExceptionAddedForTest(this ProcessTask task, ZDateTimeOffset value) => task.P9_ExceptionAddedForBinding = value;
		public static void SetMilestoneScheduledDateForTest(this ProcessTask task, ZDateTimeOffset value) => task.P9_ScheduledDateForBinding = value;
	}
#endif
}
