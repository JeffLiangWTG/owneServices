using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.CalendarArithmetic;
using CargoWise.Common.Collections;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public interface IWorkingTimeContext
	{
		TimeSpan GetUserDurationSelection(TimeSpan durationIncludingWorkingTime, TimeSpan durationExceptWorkingTime);
		TimeSpan TimeDifference(DateTime start, DateTime end);
		bool IsInWorkHours(DateTime time);
	}

	class NoWorkingDaysWorkingTimeContext : IWorkingTimeContext
	{
		public TimeSpan GetUserDurationSelection(TimeSpan durationIncludingWorkingTime, TimeSpan durationExceptWorkingTime) => durationIncludingWorkingTime;
		public TimeSpan TimeDifference(DateTime start, DateTime end) => end - start;
		public bool IsInWorkHours(DateTime time) => true;
	}

	class WorkingTimeContext : IWorkingTimeContext
	{
		readonly IWorkTimeArithmetic workDaysHelper;

		public WorkingTimeContext(IWorkTimeArithmetic workDaysHelper)
		{
			this.workDaysHelper = workDaysHelper;
		}

		public TimeSpan GetUserDurationSelection(TimeSpan durationWithoutWorkingDays, TimeSpan durationWithWorkingDays)
		{
			if (durationWithoutWorkingDays == durationWithWorkingDays)
			{
				return durationWithoutWorkingDays;
			}
			else
			{
				return durationWithWorkingDays;
			}
		}

		public bool IsInWorkHours(DateTime dateTime) => workDaysHelper.IsWorkDateTime(dateTime);
		public TimeSpan TimeDifference(DateTime start, DateTime end) => workDaysHelper.TimeDifference(start, end);
	}

	public class DurationTracker<TSpan>
		where TSpan : ISpan<DateTime>
	{
		public DurationTracker(IWorkTimeArithmetic workDaysHelper = null)
		{
			workingTime = workDaysHelper != null ? new WorkingTimeContext(workDaysHelper) : new NoWorkingDaysWorkingTimeContext();
		}

		readonly IWorkingTimeContext workingTime;

		public TimeSpan GetDuration(TimeSpan baseRecordedTime, IEnumerable<TSpan> spans)
		{
			return spans.MapOverlap<TimeSpan, TSpan, DateTime>((start, end, includedSpans) =>
			{
				var workingDuration = workingTime.TimeDifference(start, end);
				var totalDuration = end - start;
				if ((totalDuration - workingDuration).TotalHours <= WorkflowDataRegistry.Instance.TaskDurationTrackingOutOfHoursLimit.Value)
				{
					return totalDuration; // Use total duration unless the total time outside of work hours > 10 hours.
				}
				else
				{
					return workingDuration;
				}
			}).Aggregate(baseRecordedTime, (t1, t2) => t1.Add(t2)); // And make sure to add the initial duration to the new tracked spans.
		}
	}
}
