using System;
using System.Collections.Generic;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public sealed class RepititionSelection : AutoRepititionSelection
	{
		#region Properties

		public override ZBool FromFirstETD
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.FromFirstETD; }
			set
			{
				base.FromFirstETD = value;
				base.FromFirstETA = !value;
			}
		}

		public override ZBool FromFirstETA
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.FromFirstETA; }
			set
			{
				base.FromFirstETA = value;
				base.FromFirstETD = !value;
			}
		}

		public override ZBool UseDailyPattern
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.UseDailyPattern; }
			set
			{
				if (value != UseDailyPattern)
				{
					base.UseDailyPattern = false;
					base.UseWeeklyPattern = false;
					base.UseMonthlyPattern = false;

					if (value)
					{
						base.UseDailyPattern = true;
					}
					else
					{
						base.UseWeeklyPattern = true;
					}

					ResetPatternDependentFields();
				}
			}
		}

		public override ZBool UseWeeklyPattern
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.UseWeeklyPattern; }
			set
			{
				if (value != UseWeeklyPattern)
				{
					base.UseDailyPattern = false;
					base.UseWeeklyPattern = false;
					base.UseMonthlyPattern = false;

					if (value)
					{
						base.UseWeeklyPattern = true;
					}
					else
					{
						base.UseDailyPattern = true;
					}

					ResetPatternDependentFields();
				}
			}
		}

		public override ZBool UseMonthlyPattern
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.UseMonthlyPattern; }
			set
			{
				if (value != UseMonthlyPattern)
				{
					base.UseDailyPattern = false;
					base.UseWeeklyPattern = false;
					base.UseMonthlyPattern = false;

					if (value)
					{
						base.UseMonthlyPattern = true;
					}
					else
					{
						base.UseWeeklyPattern = true;
					}

					ResetPatternDependentFields();
				}
			}
		}

		public ZBoolDescriptionPairList DayOfTheWeek
		{
			get
			{
				if (dayOfTheWeek == null)
				{
					dayOfTheWeek = new ZBoolDescriptionPairList();
					CodeDescriptionPairList weekDays = new CodeDescriptionPairList(new DayOfWeekCodeList());
					foreach (ICodeDescription weekDay in weekDays)
					{
						dayOfTheWeek.AddNew(weekDay.Description, false);
					}
				}

				return dayOfTheWeek;
			}
		}
		ZBoolDescriptionPairList dayOfTheWeek;

		#endregion

		public IEnumerable<ZDateTime> GetDateTimes(ZDateTime referenceDate)
		{
			if (UseDailyPattern)
			{
				return GetDateTimes_Daily(referenceDate);
			}
			else if (UseWeeklyPattern)
			{
				return GetDateTimes_Weekly(referenceDate);
			}
			else if (UseMonthlyPattern)
			{
				return GetDateTimes_Monthly(referenceDate);
			}
			else
			{
				return Array.Empty<ZDateTime>();
			}
		}

		public RepititionSelectionLookups Lookups
		{
			get { return lookups ?? (lookups = new RepititionSelectionLookups(this)); }
		}
		RepititionSelectionLookups lookups;

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			FromFirstETD = true;
			UseDailyPattern = true;
			FromDate = ZDateTime.Now;
			DayOfTheMonth = 1;
		}

		IEnumerable<ZDateTime> GetDateTimes_Daily(ZDateTime referenceDate)
		{
			int interval = Math.Max(1, RecurrenceInterval);
			ZDateTime current = StartOfMinute(referenceDate);
			ZDateTime commence = StartOfDay(FromDate);
			ZDateTime cutOff = ToDate.EndOfDay();

			while (current < commence)
			{
				current = current.AddDays(interval);
			}

			while (current <= cutOff)
			{
				yield return current;

				current = current.AddDays(interval);
			}
		}

		IEnumerable<ZDateTime> GetDateTimes_Weekly(ZDateTime referenceDate)
		{
			int weekInterval = Math.Max(1, RecurrenceInterval);

			bool[] include = new bool[7];
			include[(int)DayOfWeek.Sunday] = DayOfTheWeek[AutoDayOfWeekCodeList.Descriptions.Sunday].Value;
			include[(int)DayOfWeek.Monday] = DayOfTheWeek[AutoDayOfWeekCodeList.Descriptions.Monday].Value;
			include[(int)DayOfWeek.Tuesday] = DayOfTheWeek[AutoDayOfWeekCodeList.Descriptions.Tuesday].Value;
			include[(int)DayOfWeek.Wednesday] = DayOfTheWeek[AutoDayOfWeekCodeList.Descriptions.Wednesday].Value;
			include[(int)DayOfWeek.Thursday] = DayOfTheWeek[AutoDayOfWeekCodeList.Descriptions.Thursday].Value;
			include[(int)DayOfWeek.Friday] = DayOfTheWeek[AutoDayOfWeekCodeList.Descriptions.Friday].Value;
			include[(int)DayOfWeek.Saturday] = DayOfTheWeek[AutoDayOfWeekCodeList.Descriptions.Saturday].Value;

			int currentDayInterval;

			for (currentDayInterval = 0; currentDayInterval < include.Length; currentDayInterval++)
			{
				if (include[currentDayInterval])
				{
					break;
				}
			}

			if (currentDayInterval < include.Length)
			{
				int[] dayIntervals = new int[7];

				currentDayInterval += 7 * (weekInterval - 1);

				for (int i = include.Length - 1; i >= 0; i--)
				{
					currentDayInterval++;
					dayIntervals[i] = currentDayInterval;

					if (include[i])
					{
						currentDayInterval = 0;
					}
				}

				ZDateTime current = new ZDateTime(FromDate.Year, FromDate.Month, FromDate.Day, referenceDate.Hour, referenceDate.Minute, 0);
				ZDateTime cutOff = ToDate.EndOfDay();

				while (current <= cutOff)
				{
					if (include[(int)current.DayOfWeek])
					{
						yield return current;
					}

					current = current.AddDays(dayIntervals[(int)current.DayOfWeek]);
				}
			}
		}

		IEnumerable<ZDateTime> GetDateTimes_Monthly(ZDateTime referenceDate)
		{
			int interval = Math.Max(1, RecurrenceInterval);
			int dayOfMonth = Math.Min(31, DayOfTheMonth);
			ZDateTime startOfMonth = new ZDateTime(referenceDate.Year, referenceDate.Month, 1, referenceDate.Hour, referenceDate.Minute, 0);
			ZDateTime commence = StartOfDay(FromDate);
			ZDateTime cutOff = ToDate.EndOfDay();

			do
			{
				ZDateTime current = startOfMonth.AddDays(dayOfMonth - 1);

				if (current > cutOff)
				{
					break;
				}
				else if (current.Month == startOfMonth.Month && current >= commence)
				{
					yield return current;
				}

				startOfMonth = startOfMonth.AddMonths(interval);
			} while (true);
		}

		void ResetPatternDependentFields()
		{
			RecurrenceInterval = 1;
			DayOfTheMonth = 1;
		}

		static ZDateTime StartOfMinute(ZDateTime inDateTime)
		{
			return new ZDateTime(inDateTime.Year, inDateTime.Month, inDateTime.Day, inDateTime.Hour, inDateTime.Minute, 0);
		}

		static ZDateTime StartOfDay(ZDateTime inDateTime)
		{
			return new ZDateTime(inDateTime.Year, inDateTime.Month, inDateTime.Day, 0, 0, 0);
		}

		#endregion
	}
}
