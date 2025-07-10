using System;
using CargoWise.Types;
using Enterprise.Scheduler.Business;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveySystemUpgradeScheduleTaskWarning
	{
		public string CaptionText => Res.GetString("B0867DCB-6B36-4488-8B27-77B1CEFFFEE2", "Warning:");

		public VoteExamSurveySystemUpgradeScheduleTaskWarning()
		{
			ScheduleTask = ScheduleTaskHelper.GetScheduleTaskByScheduleType("UPG");
		}

#if DEBUG
		public VoteExamSurveySystemUpgradeScheduleTaskWarning(StmScheduleTask task) : this()
		{
			ScheduleTask = task;
		}
#endif

		readonly StmScheduleTask ScheduleTask;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		const int WarningPeriodInMinutes = 60;

		public string Message
		{
			get
			{
				var result = string.Empty;
				var utcNow = ZDateTime.UtcNow;
				var nextScheduledSystemUpgradeDateTimeUtc = ScheduleTaskHelper.GetNextScheduleServiceTaskDateTime(ScheduleTask);

				if (!nextScheduledSystemUpgradeDateTimeUtc.IsEmpty)
				{
					var totalMinutes = (nextScheduledSystemUpgradeDateTimeUtc - utcNow).TotalMinutes;
					if (totalMinutes >= 0 && totalMinutes <= WarningPeriodInMinutes)
					{
						result = Res.GetString("084c36fe-3271-4360-a0aa-e06a9dc0dc3c", "A scheduled system upgrade will commence in {0} minute(s). Exams in progress, that are not submitted before the upgrade, will require you to sit them again.", Math.Ceiling(totalMinutes));
					}
				}

				return result.Trim();
			}
		}
	}
}
