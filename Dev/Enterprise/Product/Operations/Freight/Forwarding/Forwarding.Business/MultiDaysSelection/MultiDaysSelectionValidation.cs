namespace Enterprise.Freight.Forwarding.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Core;

	public class MultiDaysSelectionValidation : AutoMultiDaysSelectionValidation
	{
		public MultiDaysSelectionValidation(AutoMultiDaysSelection parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			isValidatingAll = true;

			try
			{
				ValidateAllCore();
			}
			finally
			{
				isValidatingAll = false;
			}
		}

		public new void ValidateRangeFromDate()
		{
			base.ValidateRangeFromDate();

			if (!isValidatingAll)
			{
				ValidateDailyFromDate();
				ValidateMonthlyFromDate();
			}
		}

		public new void ValidateRangeToDate()
		{
			base.ValidateRangeToDate();

			if (!isValidatingAll)
			{
				ValidateDailyFromDate();
				ValidateMonthlyFromDate();
			}
		}

		protected override void CheckDailyFromDateIsValidZDateTime()
		{
			if (Parent.UseDailyPattern)
			{
				base.CheckDailyFromDateIsValidZDateTime();
			}
		}

		protected override void CheckDailyFromDateIsValidZDateTimeRange()
		{
			if (Parent.UseDailyPattern)
			{
				base.CheckDailyFromDateIsValidZDateTimeRange();
			}
		}

		protected override void CheckDailyFromDate()
		{
			if (Parent.UseDailyPattern)
			{
				MandatoryValidation.CheckEntered(Parent.DailyFromDateInfo);

				if (Parent.DailyFromDate < Parent.RangeFromDate)
				{
					Parent.DailyFromDateInfo.AddError(FromDateEarlierErrorMessage(Parent.RangeFromDate));
				}
				else if (Parent.DailyFromDate > Parent.RangeToDate)
				{
					Parent.DailyFromDateInfo.AddError(FromDateLaterErrorMessage(Parent.RangeToDate));
				}
			}
		}

		protected override void CheckDailyRecurEvery()
		{
			if (Parent.UseDailyPattern && (Parent.DailyRecurEvery < 1 || Parent.DailyRecurEvery > 366))
			{
				Parent.DailyRecurEveryInfo.AddError(DailyRecurrenceNumberErrorMessage);
			}
		}

		protected override void CheckWeeklyRecurEvery()
		{
			if (Parent.UseWeeklyPattern && (Parent.WeeklyRecurEvery < 1 || Parent.WeeklyRecurEvery > 52))
			{
				Parent.WeeklyRecurEveryInfo.AddError(WeeklyRecurrenceNumberErrorMessage);
			}
		}

		protected override void CheckMonthlyFromDateIsValidZDateTime()
		{
			if (Parent.UseMonthlyPattern)
			{
				base.CheckMonthlyFromDateIsValidZDateTime();
			}
		}

		protected override void CheckMonthlyFromDateIsValidZDateTimeRange()
		{
			if (Parent.UseMonthlyPattern)
			{
				base.CheckMonthlyFromDateIsValidZDateTimeRange();
			}
		}

		protected override void CheckMonthlyFromDate()
		{
			if (Parent.UseMonthlyPattern)
			{
				MandatoryValidation.CheckEntered(Parent.MonthlyFromDateInfo);

				if (Parent.MonthlyFromDate < Parent.RangeFromDate)
				{
					Parent.MonthlyFromDateInfo.AddError(FromDateEarlierErrorMessage(Parent.RangeFromDate));
				}
				else if (Parent.MonthlyFromDate > Parent.RangeToDate)
				{
					Parent.MonthlyFromDateInfo.AddError(FromDateLaterErrorMessage(Parent.RangeToDate));
				}
			}
		}

		protected override void CheckMonthlyRecurEvery()
		{
			if (Parent.UseMonthlyPattern && (Parent.MonthlyRecurEvery < 1 || Parent.MonthlyRecurEvery > 12))
			{
				Parent.MonthlyRecurEveryInfo.AddError(MonthlyRecurrenceNumberErrorMessage);
			}
		}

		protected override void CheckRangeFromDate()
		{
			MandatoryValidation.CheckEntered(Parent.RangeFromDateInfo);

			if (Parent.RangeFromDate < Parent.FromDateLimit)
			{
				Parent.RangeFromDateInfo.AddError(FromDateEarlierErrorMessage(Parent.FromDateLimit));
			}
			else if (Parent.RangeFromDate > Parent.RangeToDate)
			{
				Parent.RangeFromDateInfo.AddError(FromAndToDateErrorMessage);
			}
		}

		protected override void CheckRangeToDate()
		{
			MandatoryValidation.CheckEntered(Parent.RangeToDateInfo);

			if (Parent.RangeToDate > Parent.ToDateLimit)
			{
				Parent.RangeToDateInfo.AddError(ToDateLaterErrorMessage(Parent.ToDateLimit));
			}
			else if (Parent.RangeFromDate > Parent.RangeToDate)
			{
				Parent.RangeToDateInfo.AddError(FromAndToDateErrorMessage);
			}
		}

		public new MultiDaysSelection Parent => (MultiDaysSelection)base.Parent;

		#region Implementation

		ResourceString FromDateEarlierErrorMessage(ZDateTime date) => ResString.GetMultilingualString("ED5DEDEB-58CF-4E66-AD52-5E361C42E687",
			"'From Date' can not be before '{0}'.", date.Date.ToString());

		ResourceString FromDateLaterErrorMessage(ZDateTime date) => ResString.GetMultilingualString("8086687F-CDDA-4458-929F-FA9D6CC322E4",
			"'From Date' can not be after '{0}'.", date.Date.ToString());

		ResourceString FromAndToDateErrorMessage => ResString.GetMultilingualString("FCCD5758-9CE8-4FB2-8318-775DBB210C3E",
			"'From Date' must be before 'To Date'.");

		ResourceString ToDateLaterErrorMessage(ZDateTime date) => ResString.GetMultilingualString("21E585B9-449F-4504-94D8-1B084C0E51EF",
				"'To Date' can not be after '{0}'.", date.Date.ToString());

		ResourceString DailyRecurrenceNumberErrorMessage => ResString.GetMultilingualString("BA0006BE-1646-4713-93A9-C48382757640",
			"The recurrence number must be between 1 and 366.");

		ResourceString WeeklyRecurrenceNumberErrorMessage => ResString.GetMultilingualString("BB5CB4E7-189B-496B-ABC3-14B52A9CEB53",
			"The recurrence number must be between 1 and 52.");

		ResourceString MonthlyRecurrenceNumberErrorMessage => ResString.GetMultilingualString("D9415551-9517-4835-A842-859FF38D285C",
			"The recurrence number must be between 1 and 12.");

		bool isValidatingAll;

		#endregion
	}
}
