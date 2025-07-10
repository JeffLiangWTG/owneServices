using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using static System.FormattableString;

namespace Enterprise.TransportConsignment.Business
{
	public static class RunSheetDayExtensions
	{
		#region GetDate

		public static ZDate GetDate(this RunSheetDay day)
		{
			var today = ZDate.Today;

			switch (day)
			{
				// yesterday, today and tomorrow
				case RunSheetDay.Yesterday:
					return today.AddDays(-1);
				case RunSheetDay.Today:
					return today;
				case RunSheetDay.Tomorrow:
					return today.AddDays(1);
				case RunSheetDay.CustomDate:
					throw new ArgumentException("GetDate() should not be called on CustomDate.");

				// days of the week
				default:
					return GetDateForDayStartingAfterTomorrow(day);
			}
		}

		static ZDate GetDateForDayStartingAfterTomorrow(RunSheetDay day)
		{
			var start = GetDayAfterTomorrow();

			for (int i = 0; i < NumberOfDaysInWeek; i++)
			{
				var date = start.AddDays(i); // start the day after tomorrow
				if (GetRunSheetDay(date.DayOfWeek) == day)
				{
					return date;
				}
			}

			throw new ArgumentException(Invariant($"RunSheetDay {day} not supported."), nameof(day));
		}

		static RunSheetDay GetRunSheetDay(DayOfWeek day)
		{
			switch (day)
			{
				case DayOfWeek.Sunday:
					return RunSheetDay.Sunday;
				case DayOfWeek.Monday:
					return RunSheetDay.Monday;
				case DayOfWeek.Tuesday:
					return RunSheetDay.Tuesday;
				case DayOfWeek.Wednesday:
					return RunSheetDay.Wednesday;
				case DayOfWeek.Thursday:
					return RunSheetDay.Thursday;
				case DayOfWeek.Friday:
					return RunSheetDay.Friday;
				case DayOfWeek.Saturday:
					return RunSheetDay.Saturday;

				default:
					throw new NotSupportedException();
			}
		}

		static ZDate GetDayAfterTomorrow()
		{
			return ZDate.Today.AddDays(2);
		}

		const int NumberOfDaysInWeek = 7;

		#endregion

		#region GetDescription

		public static ResourceStringData GetDescription(this RunSheetDay day)
		{
			switch (day)
			{
				case RunSheetDay.Yesterday:
					return Yesterday;
				case RunSheetDay.Today:
					return Today;
				case RunSheetDay.Tomorrow:
					return Tomorrow;

				case RunSheetDay.Sunday:
					return Sunday;
				case RunSheetDay.Monday:
					return Monday;
				case RunSheetDay.Tuesday:
					return Tuesday;
				case RunSheetDay.Wednesday:
					return Wednesday;
				case RunSheetDay.Thursday:
					return Thursday;
				case RunSheetDay.Friday:
					return Friday;
				case RunSheetDay.Saturday:
					return Saturday;

				case RunSheetDay.CustomDate:
					return CustomDate;

				default:
					return null;
			}
		}

		static ResourceStringData Yesterday { get { return Res.GetData("c9739749-cde4-4261-bf6c-bf6cc657825d", "Yesterday"); } }
		static ResourceStringData Today { get { return Res.GetData("d901b70e-3c88-4c6e-a745-f9f75eed9b35", "Today"); } }
		static ResourceStringData Tomorrow { get { return Res.GetData("a0cba25a-c0f5-4d98-8b2a-2e3c736ce389", "Tomorrow"); } }

		static ResourceStringData Sunday { get { return Res.GetData("dae6f6f1-8811-4c4f-a529-f325fe4948b8", "Sunday"); } }
		static ResourceStringData Monday { get { return Res.GetData("c7990d78-f93b-4e43-ba6e-5634624c7c7f", "Monday"); } }
		static ResourceStringData Tuesday { get { return Res.GetData("c8f0cfda-7ebd-4d6f-9924-53c5d90c9e96", "Tuesday"); } }
		static ResourceStringData Wednesday { get { return Res.GetData("a6eb47f6-772b-421c-ac9d-9269c8de9c6f", "Wednesday"); } }
		static ResourceStringData Thursday { get { return Res.GetData("e54eb510-b941-47e6-a87f-ce146b416404", "Thursday"); } }
		static ResourceStringData Friday { get { return Res.GetData("ec99969c-f24c-463e-a36e-288205523b24", "Friday"); } }
		static ResourceStringData Saturday { get { return Res.GetData("50e21610-5678-47b4-a1d1-76cb7db1a155", "Saturday"); } }

		static ResourceStringData CustomDate { get { return Res.GetData("4ccf6e31-3952-4e9e-ad86-a34e40998a60", "Custom Date"); } }

		#endregion

		#region GetNextSevenDaysAfterTomorrow

		public static RunSheetDay[] GetNextSevenDaysAfterTomorrow()
		{
			var result = new RunSheetDay[NumberOfDaysInWeek];
			var start = GetDayAfterTomorrow();

			for (int i = 0; i < NumberOfDaysInWeek; i++)
			{
				result[i] = GetRunSheetDay(start.AddDays(i).DayOfWeek);
			}

			return result;
		}

		#endregion

		#region GetYesterdayTodayTomorrow

		public static RunSheetDay[] GetYesterdayTodayTomorrow()
		{
			return new[]
			{
				RunSheetDay.Yesterday,
				RunSheetDay.Today,
				RunSheetDay.Tomorrow
			};
		}

		#endregion

		#region IsCustomDate

		public static bool IsCustomDate(this RunSheetDay day)
		{
			return day == RunSheetDay.CustomDate;
		}

		#endregion
	}
}
