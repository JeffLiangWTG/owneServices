using CargoWise.EntityFramework;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class RunSheetDashboardValidation : AutoRunSheetDashboardValidation
	{
		public RunSheetDashboardValidation(AutoRunSheetDashboard parent)
			: base(parent) { }

		public new RunSheetDashboard Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (RunSheetDashboard)base.Parent; }
		}

		protected override void CheckDateRangeFilter()
		{
			base.CheckDateRangeFilter();
			ListValidation.ErrorIfInvalidCode(Parent.DateRangeFilterInfo);
		}

		protected override void CheckDateRangeFrom()
		{
			base.CheckDateRangeFrom();

			if (Parent.IsDateRange)
			{
				MandatoryValidation.CheckEntered(Parent.DateRangeFromInfo);
				if (Parent.DateRangeFrom.IsValid && Parent.DateRangeTo.IsValid && Parent.DateRangeFrom > Parent.DateRangeTo)
				{
					Parent.DateRangeFromInfo.AddError(Res.GetString("5ce94252-c40c-49cd-8aa4-1e14b0738399", "Date range 'From Date' needs to be before 'To Date'"));
				}
			}
		}

		protected override void CheckDateRangeTo()
		{
			base.CheckDateRangeTo();

			if (Parent.IsDateRange)
			{
				MandatoryValidation.CheckEntered(Parent.DateRangeToInfo);
				if (Parent.DateRangeFrom.IsValid && Parent.DateRangeTo.IsValid && Parent.DateRangeTo < Parent.DateRangeFrom)
				{
					Parent.DateRangeToInfo.AddError(Res.GetString("d1591821-fae6-4cbb-bb26-630258e6ca99", "Date range 'To Date' needs to be after 'From Date'"));
				}
			}
		}
	}
}
