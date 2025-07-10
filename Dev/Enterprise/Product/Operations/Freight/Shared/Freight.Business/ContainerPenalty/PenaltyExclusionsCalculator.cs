using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public static class PenaltyExclusionsCalculator
	{
		public static ZByte CalculateTotalFreeDaysToSkip(ContainerPenalty containerPenalty)
		{
			if (containerPenalty.FreeDayExclusion is not IContainerPenaltyDayExclusion freeDayExclusion
				|| freeDayExclusion.IsEmpty())
			{
				return 0;
			}

			return CalculateTotalFreeDaysToSkip(containerPenalty.FirstFreeDay,
				containerPenalty.FreeTimeAsDays,
				freeDayExclusion,
				containerPenalty.Location);
		}

		public static ZByte CalculateTotalFreeDaysToSkip(
			ZDateTime firstFreeDay,
			ZByte freeDays,
			IContainerPenaltyDayExclusion freeDayExclusion,
			RefUNLOCO penaltyLocation)
		{
			if (freeDayExclusion == null
				|| !firstFreeDay.IsValid)
			{
				return 0;
			}

			ZByte daysAdded = 0;
			ZByte daysSkipped = 0;
			var currentFreeDay = firstFreeDay;

			while (daysAdded < freeDays)
			{
				var dayOfWeek = currentFreeDay.DayOfWeek;
				if (GetExcludedDaysOfWeek(freeDayExclusion, penaltyLocation).Contains(dayOfWeek))
				{
					currentFreeDay = currentFreeDay.AddDays(1);
					daysSkipped++;
					continue;
				}

				foreach (var holiday in GetExcludedHolidaysForLocation(freeDayExclusion, penaltyLocation))
				{
					if (holiday.MatchesDate(currentFreeDay.Date))
					{
						currentFreeDay = currentFreeDay.AddDays(1);
						daysSkipped++;
						continue;
					}
				}

				currentFreeDay = currentFreeDay.AddDays(1);
				daysAdded++;
			}

			return daysSkipped;
		}

		public static ZByte CalculateNumberOfExcludedFreeDaysSoFar(ContainerPenalty containerPenalty)
		{
			ZByte daysSkipped = 0;

			if (containerPenalty.FreeDayExclusion is not IContainerPenaltyDayExclusion exclusion
				|| exclusion.IsEmpty()
				|| !containerPenalty.FirstFreeDay.IsValid)
			{
				return daysSkipped;
			}

			if (containerPenalty.LastFreeDay <= ZDate.Today)
			{
				return CalculateTotalFreeDaysToSkip(containerPenalty);
			}

			var currentFreeDay = containerPenalty.FirstFreeDay.Date;
			var totalDateDifference = (currentFreeDay - ZDate.Today).TotalDays;
			if (totalDateDifference > 255)
			{
				return 0;
			}

			while (currentFreeDay <= ZDate.Today)
			{
				var dayOfWeek = currentFreeDay.DayOfWeek;
				if (GetExcludedDaysOfWeek(exclusion, containerPenalty.Location).Contains(dayOfWeek))
				{
					daysSkipped++;
				}
				else
				{
					foreach (var holiday in GetExcludedHolidaysForLocation(exclusion, containerPenalty.Location))
					{
						if (holiday.MatchesDate(currentFreeDay))
						{
							daysSkipped++;
							break;
						}
					}
				}

				currentFreeDay = currentFreeDay.AddDays(1);
			}

			return daysSkipped;
		}

		public static ZByte CalculateNumberOfExcludedDurationSoFar(ContainerPenalty containerPenalty)
		{
			if (!containerPenalty.LastFreeDay.IsValid)
			{
				return 0;
			}

			ZDate endDate;
			if (containerPenalty.CPY_Duration.IsEmpty)
			{
				endDate = ZDate.Today;
			}
			else
			{
				endDate = containerPenalty.CPY_Duration.Date;
			}

			if ((endDate - containerPenalty.LastFreeDay).TotalDays > 255)
			{
				return 0;
			}

			return CalculateNumberOfExcludedDurationDays(containerPenalty, endDate);
		}

		public static ZByte CalculateNumberOfExcludedDurationDays(ContainerPenalty containerPenalty, ZDate endDate)
		{
			ZByte daysSkipped = 0;

			if (containerPenalty?.DurationExclusion is not IContainerPenaltyDayExclusion exclusion
				|| exclusion.IsEmpty()
				|| !containerPenalty.LastFreeDay.Date.IsValid)
			{
				return daysSkipped;
			}

			var durationDay = containerPenalty.LastFreeDay.Date.AddDays(1);
			while (durationDay <= endDate)
			{
				var dayOfWeek = durationDay.DayOfWeek;
				if (GetExcludedDaysOfWeek(exclusion, containerPenalty.Location).Contains(dayOfWeek))
				{
					daysSkipped++;
				}
				else
				{
					foreach (var holiday in GetExcludedHolidaysForLocation(exclusion, containerPenalty.Location))
					{
						if (holiday.MatchesDate(durationDay))
						{
							daysSkipped++;
							continue;
						}
					}
				}

				durationDay = durationDay.AddDays(1);
			}

			return daysSkipped;
		}

		static HashSet<DayOfWeek> GetExcludedDaysOfWeek(IContainerPenaltyDayExclusion exclusion, RefUNLOCO location)
		{
			if (exclusion.IsEmpty())
			{
				return new HashSet<DayOfWeek>();
			}

			var excludedDaysOfWeek = new List<DayOfWeek>();
			if (exclusion.CEX_Weekend)
			{
				var stateWeekends = location?.CountryStates?.Weekends ?? Enumerable.Empty<GlbHoliday>();
				if (stateWeekends.Any())
				{
					var nonWorkingDays = stateWeekends
						.Where(weekend => !weekend.GH_IsWorkingDay)
						.Select(weekend => HolidayCodes.GetDayFromCode(weekend.GH_RecurrDay).Value);
					excludedDaysOfWeek.AddRange(nonWorkingDays);
				}
				else
				{
					var countryWeekends = location?.Country?.Weekends.Where(weekend => !weekend.GH_IsWorkingDay) ?? Enumerable.Empty<GlbHoliday>();
					excludedDaysOfWeek.AddRange(countryWeekends.Select(weekend => HolidayCodes.GetDayFromCode(weekend.GH_RecurrDay).Value));
				}
			}

			excludedDaysOfWeek.AddRange(exclusion.GetExcludedDaysOfWeek());
			return excludedDaysOfWeek.ToHashSet();
		}

		static IReadOnlyCollection<GlbHoliday> GetExcludedHolidaysForLocation(IContainerPenaltyDayExclusion exclusion, RefUNLOCO location)
		{
			if (!exclusion.CEX_Holiday)
			{
				return new List<GlbHoliday>();
			}

			return location?.CountryStates?.Holidays?.ToList() ?? new List<GlbHoliday>();
		}
	}
}
