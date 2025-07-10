using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class RepititionSelectionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateSelectedDayOfWeek()
		{
			Settings.UseDailyPattern = true;
			AssertEquals("No day is selected", false, IsDayOfTheWeekSelected());

			Settings.Validation.ValidateAll();
			AssertNoRowErrors("No Error Expected as Schedule Recurs Daily", Settings);

			Settings.UseWeeklyPattern = true;
			Settings.Validation.ValidateAll();
			AssertHasRowError("Error Expected", Settings, "Please select the day of the week to create the sailing schedules.");

			Settings.DayOfTheWeek[0].Value = true;
			Settings.Validation.ValidateRow();
			AssertNoRowErrors("No Error is expected", Settings);
		}

		public void TestValidateFromDate()
		{
			AssertNoNotifications("BulkCopyDateFrom doesnt' have any errors", Settings.FromDateInfo);

			Settings.FromDate = ZDateTime.Now.AddDays(-1);
			Settings.Validation.ValidateFromDate();

			AssertHasError("Error Expected", Settings.FromDateInfo, "'From Date' cannot be prior to today's date.");
		}

		public void TestValidateToDate()
		{
			Settings.ToDate = ZDateTime.Empty;
			AssertHasError("Error Expected", Settings.ToDateInfo, "Please enter a To Date.");

			Settings.ToDate = ZDateTime.Today.AddDays(2);
			AssertNoNotifications("BulkCopyDateTo doesnt' have any errors", Settings.ToDateInfo);

			Settings.ToDate = ZDateTime.Now.AddMonths(7);
			Settings.Validation.ValidateToDate();
			AssertHasError("Error Expected", Settings.ToDateInfo, "Schedule can be created a maximum of 6 months in advance only.");
		}

		public void TestValidateDayOfMonth()
		{
			Settings.UseMonthlyPattern = true;
			Settings.DayOfTheMonth = 28;
			AssertNoNotifications("28 is always valid", Settings.DayOfTheMonthInfo);

			Settings.DayOfTheMonth = 29;
			AssertHasWarning("29 is not always valid", Settings.DayOfTheMonthInfo, "Some months have fewer than 29 days. For these months, the schedules will not be created.");

			Settings.DayOfTheMonth = 0;
			AssertHasError("0 is never valid.", Settings.DayOfTheMonthInfo, "Select a day of the month.");

			Settings.DayOfTheMonth = 32;
			AssertHasError("32 is never valid.", Settings.DayOfTheMonthInfo, "No month has 32 days.");

			Settings.UseDailyPattern = true;
			Settings.DayOfTheMonth = 0;
			AssertNoNotifications("Dont validate if not monthly", Settings.DayOfTheMonthInfo);
		}

		#region Implementation

		bool IsDayOfTheWeekSelected()
		{
			foreach (ZBoolDescriptionPair pair in Settings.DayOfTheWeek)
			{
				if (pair.Value)
				{
					return true;
				}
			}

			return false;
		}

		RepititionSelection Settings
		{
			get { return criteria ?? (criteria = new RepititionSelection()); }
		}
		RepititionSelection criteria;

		#endregion
	}
}
