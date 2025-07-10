using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public static class IWorkflowTaskExtensions
	{
		public static double GetRelevantEstimateHours(this IWorkflowTask task)
		{
			return TaskDurationCalculator.GetRelevantEstimateHours(task.P9_EstimatedTimeToComplete, task.P9_EstDuration, task.P9_EstimateVariationFactor);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static int GetEstimatedMinutesToComplete(this IWorkflowTask task)
		{
			return TaskDurationCalculator.GetEstimatedMinutesToComplete(task.P9_EstimatedTimeToComplete, task.P9_EstDuration, task.P9_EstimateVariationFactor);
		}

		public static double GetEstimatedTimeToCompleteHours(this IWorkflowTask task)
		{
			return TaskDurationCalculator.GetEstimatedTimeToCompleteHours(task.P9_EstimatedTimeToComplete);
		}

		public static double GetLowEstimatedDurationHours(this IWorkflowTask task)
		{
			return TaskDurationCalculator.GetLowEstimatedDurationHours(task.P9_EstDuration);
		}

		public static double GetHighEstimatedDurationHours(this IWorkflowTask task)
		{
			return TaskDurationCalculator.GetHighEstimatedDurationHours(task.P9_EstDuration, task.P9_EstimateVariationFactor);
		}

		public static ZDateTime GetHighEstimatedDuration(this IWorkflowTask task)
		{
			return TaskDurationCalculator.GetHighEstimatedDuration(task.P9_EstDuration, task.P9_EstimateVariationFactor);
		}

		public static double GetStandardEstimateHours(this IWorkflowTask task)
		{
			return TaskDurationCalculator.GetStandardEstimateHours(task.P9_EstDuration, task.P9_EstimateVariationFactor);
		}
	}
}
