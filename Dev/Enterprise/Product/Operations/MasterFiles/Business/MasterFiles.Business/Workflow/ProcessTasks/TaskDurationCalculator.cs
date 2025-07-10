using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class TaskDurationCalculator
	{
		public static double GetHoursFromDuration(ZDateTime duration)
		{
			double result = 0;

			if (!duration.IsEmpty && duration.IsValid)
			{
				var baseDuration = GetBaseDuration(duration.Year);
				var difference = duration - baseDuration;
				result = difference.TotalHours;
			}

			return result;
		}

		static readonly TimeSpan maxDisplayValue = TimeSpan.FromDays(365) - TimeSpan.FromHours(1);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		internal static ZDateTime GetDurationFromHours(decimal hours) => hours <= (decimal)maxDisplayValue.TotalHours
				? TimeSpan.FromHours((double)hours)
				: maxDisplayValue;

		internal static ZDateTime GetDurationWithFactor(ZDateTime duration, decimal factor = 1m)
		{
			if (!duration.IsEmpty && duration.IsValid)
			{
				var durationHours = (decimal)GetHoursFromDuration(duration) * factor;

				return GetDurationFromHours(durationHours);
			}

			return ZDateTime.Empty;
		}

		public static ZDateTime GetDurationFromTimeSpan(TimeSpan timespan)
		{
			var result = ZDateTime.DefaultDurationEpoch;
			if (timespan.CompareTo(TimeSpan.Zero) > 0)
			{
				return new[] { result.Add(timespan), result.Add(TimeSpan.FromDays(180)) }.Min();
			}
			return result;
		}

		public static ZDateTime AddToDurationSafe(ZDateTime duration, TimeSpan timespan)
		{
			var baseDuration = ZDateTime.DefaultDurationEpoch;
			var newDuration = duration.Add(timespan);
			if (newDuration > baseDuration)
			{
				return new[] { newDuration, baseDuration.Add(TimeSpan.FromDays(180)) }.Min(); // Duration can't be higher than 180
			}
			else
			{
				return baseDuration; // Duration can't be lower than base duration. (because you can't work negative hours)
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static double GetRelevantEstimateHours(ZDateTime estimatedTimeToComplete, ZDateTime lowEstimateDuration, decimal estimateVariationFactor)
		{
			var returnVal = GetEstimatedTimeToCompleteHours(estimatedTimeToComplete);

			if (returnVal > 0)
			{
				return returnVal;
			}
			else
			{
				return GetStandardEstimateHours(lowEstimateDuration, estimateVariationFactor);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static double GetEstimatedTimeToCompleteHours(ZDateTime estimatedTimeToComplete)
		{
			return GetHoursFromDuration(estimatedTimeToComplete);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static double GetLowEstimatedDurationHours(ZDateTime lowEstimateDuration)
		{
			return GetHoursFromDuration(lowEstimateDuration);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static double GetHighEstimatedDurationHours(ZDateTime lowEstimateDuration, decimal estimateVariationFactor)
		{
			return GetHoursFromDuration(GetDurationWithFactor(lowEstimateDuration, estimateVariationFactor));
		}

		public static ZDateTime GetHighEstimatedDuration(ZDateTime lowEstimateDuration, decimal estimateVariationFactor)
		{
			return GetDurationWithFactor(lowEstimateDuration, estimateVariationFactor);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static double GetStandardEstimateHours(ZDateTime lowEstimateDuration, decimal estimateVariationFactor)
		{
			return (GetLowEstimatedDurationHours(lowEstimateDuration) + GetHighEstimatedDurationHours(lowEstimateDuration, estimateVariationFactor)) / 2.0;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static (int estimatedTimeToCompleteMins, int standardEstimateMins, int lowEstimatedMins, int highEstimatedMins) ConvertDatesToMinutes(ZDateTime estimatedTimeToComplete, ZDateTime estimatedDuration, decimal estimateVariationFactor)
		{
			var estimatedTimeToCompleteMins = GetEstimatedTimeToCompleteHours(estimatedTimeToComplete) * 60;
			var standardEstimateMins = GetRelevantEstimateHours(estimatedTimeToComplete, estimatedDuration, estimateVariationFactor) * 60;
			var lowEstimatedMins = GetLowEstimatedDurationHours(estimatedDuration) * 60;
			var highEstimatedMins = GetHighEstimatedDurationHours(estimatedDuration, estimateVariationFactor) * 60;
			return ((int)estimatedTimeToCompleteMins, (int)standardEstimateMins, (int)lowEstimatedMins, (int)highEstimatedMins);
		}

		internal static ZDateTime GetBaseDuration(int year)
		{
			return new ZDateTime(year, 1, 1);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static int GetEstimatedMinutesToComplete(ZDateTime estimatedTimeToComplete, ZDateTime duration, decimal variationFactor)
		{
			var estimatedMinutesToComplete = decimal.Zero;

			if (estimatedTimeToComplete != ZDateTime.Empty)
			{
				estimatedMinutesToComplete = GetDurationMinutes(estimatedTimeToComplete);
			}

			if (estimatedMinutesToComplete == decimal.Zero && duration != ZDateTime.Empty)
			{
				estimatedMinutesToComplete = (GetDurationMinutes(duration) + GetDurationMinutes(duration, variationFactor)) / 2;
			}

			return Convert.ToInt32(Utilities.Round(estimatedMinutesToComplete, 0));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		static decimal GetDurationMinutes(ZDateTime duration, decimal factor = 1m)
		{
			if (!(duration == default))
			{
				return DurationToTimeSpanInMinutes(duration) * factor;
			}

			return 0m;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		static decimal DurationToTimeSpanInMinutes(ZDateTime duration)
		{
			return (decimal)(duration - new ZDateTime(duration.Year, 1, 1)).TotalMinutes;
		}
	}
}
