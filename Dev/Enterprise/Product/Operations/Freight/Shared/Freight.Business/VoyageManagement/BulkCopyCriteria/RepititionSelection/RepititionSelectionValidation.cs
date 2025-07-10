using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	public class RepititionSelectionValidation : AutoRepititionSelectionValidation
	{
		public RepititionSelectionValidation(AutoRepititionSelection parent)
			: base(parent) { }

		protected override void CheckToDate()
		{
			base.CheckToDate();
			MandatoryValidation.CheckEntered(Parent.ToDateInfo);

			if (!Parent.ToDate.IsEmpty && Parent.FromDate.IsValid)
			{
				if (Parent.ToDate < Parent.FromDate)
				{
					Parent.ToDateInfo.AddError(Res.GetString("33ef4cb0-abe7-4ff1-bcdc-c84b9f4ebceb", "'To Date' must be after 'From Date'."));
				}
				else if (Parent.ToDate > Parent.FromDate.AddMonths(6))
				{
					Parent.ToDateInfo.AddError(Res.GetString("fb79ef31-4877-4286-9637-edb4d7660850", "Schedule can be created a maximum of 6 months in advance only."));
				}
			}
		}

		protected override void CheckFromDate()
		{
			base.CheckFromDate();
			MandatoryValidation.CheckEntered(Parent.FromDateInfo);

			if (!Parent.FromDate.IsEmpty && Parent.FromDate.IsValid)
			{
				if (Parent.FromDate.Date < ZDateTime.Now.Date)
				{
					Parent.FromDateInfo.AddError(Res.GetString("203ef436-0309-4270-bcab-ef9b0fb62e6e", "'From Date' cannot be prior to today's date."));
				}

				if (Parent.ToDate.IsValid && Parent.ToDate < Parent.FromDate)
				{
					Parent.FromDateInfo.AddError(Res.GetString("32526396-92e7-43d5-9e1c-f38956a3b5c0", "'From Date' must be before 'To Date'."));
				}
			}
		}

		protected override void CheckDayOfTheMonth()
		{
			base.CheckDayOfTheMonth();

			if (Parent.UseMonthlyPattern)
			{
				if (Parent.DayOfTheMonth < 1)
				{
					Parent.DayOfTheMonthInfo.AddError(Res.GetString("9283f469-8e71-4f44-9af4-6cd64fc1015e", "Select a day of the month."));
				}
				else if (Parent.DayOfTheMonth > 31)
				{
					Parent.DayOfTheMonthInfo.AddError(Res.GetString("e5b451e6-0f66-4e28-8198-9bbd04556689", "No month has {0} days.", Parent.DayOfTheMonth));
				}
				else if (Parent.DayOfTheMonth > 28)
				{
					Parent.DayOfTheMonthInfo.AddWarning(Res.GetString("989a4adb-9e1d-4ed1-8876-672ed848162d", "Some months have fewer than {0} days. For these months, the schedules will not be created.", Parent.DayOfTheMonth));
				}
			}
		}

		public void ValidateRow()
		{
			Parent.ClearRowNotifications();
			CheckRow();
		}
		protected virtual void CheckRow()
		{
			ZBool atLeastOneSelected = false;

			if (Parent.UseWeeklyPattern)
			{
				foreach (ZBoolDescriptionPair pair in Parent.DayOfTheWeek)
				{
					if (pair.Value)
					{
						atLeastOneSelected = true;
						break;
					}
				}

				if (!atLeastOneSelected)
				{
					Parent.AddRowError(Res.GetString("915027c2-633c-4c82-870f-efa48cecdd11", "Please select the day of the week to create the sailing schedules."));
				}
			}
		}

		#region Implementation

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateRow();
		}

		public new RepititionSelection Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (RepititionSelection)base.Parent; }
		}

		#endregion
	}
}
