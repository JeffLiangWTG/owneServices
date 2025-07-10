namespace Enterprise.Freight.Forwarding.Business
{
	// Empty validations class.
	public class MultiDaysSelectionValidationEmpty : MultiDaysSelectionValidation
	{
		public MultiDaysSelectionValidationEmpty(AutoMultiDaysSelection parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
		}

		public new void ValidateRangeFromDate()
		{
		}

		public new void ValidateRangeToDate()
		{
		}

		protected override void CheckDailyFromDateIsValidZDateTime()
		{
		}

		protected override void CheckDailyFromDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckDailyFromDate()
		{
		}

		protected override void CheckDailyRecurEvery()
		{
		}

		protected override void CheckWeeklyRecurEvery()
		{
		}

		protected override void CheckMonthlyFromDateIsValidZDateTime()
		{
		}

		protected override void CheckMonthlyFromDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckMonthlyFromDate()
		{
		}

		protected override void CheckMonthlyRecurEvery()
		{
		}

		protected override void CheckRangeFromDate()
		{
		}

		protected override void CheckRangeToDate()
		{
		}

		public new MultiDaysSelection Parent => base.Parent;
	}
}

