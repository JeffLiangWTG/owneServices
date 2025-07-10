using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class CustomsWorkingDaysTest : WorkingDaysTest
	{
		public void TestHardcodedHolidays()
		{
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2008, 12, 25, 12, 12, 12)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2009, 1, 1)));
			AssertEquals(false, workingDays.IsDateTimeAHoliday(new DateTime(2009, 1, 2)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2009, 1, 19)));
			AssertEquals(false, workingDays.IsDateTimeAHoliday(new DateTime(2009, 2, 18)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2009, 2, 16)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2009, 7, 3)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2009, 9, 7)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2009, 10, 12)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2009, 11, 11)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2009, 11, 26)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2009, 12, 25)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2010, 01, 01)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2010, 01, 18)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2010, 02, 15)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2010, 05, 31)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2010, 07, 05)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2010, 09, 06)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2010, 10, 11)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2010, 11, 11)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2010, 11, 25)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2010, 12, 24)));
			//2021
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2021, 01, 01)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2021, 01, 18)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2021, 02, 15)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2021, 05, 31)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2021, 07, 05)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2021, 09, 06)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2021, 10, 11)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2021, 11, 11)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2021, 11, 25)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2021, 12, 24)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2021, 12, 31)));
			//2022
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2022, 01, 17)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2022, 02, 21)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2022, 05, 30)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2022, 06, 20)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2022, 07, 04)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2022, 09, 05)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2022, 10, 10)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2022, 11, 11)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2022, 11, 24)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2022, 12, 26)));
			//2023
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2023, 01, 02)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2023, 01, 16)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2023, 02, 20)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2023, 05, 29)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2023, 06, 19)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2023, 07, 04)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2023, 09, 04)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2023, 10, 09)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2023, 11, 10)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2023, 11, 23)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2023, 12, 25)));
			//2024
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2024, 01, 01)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2024, 01, 15)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2024, 02, 19)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2024, 05, 27)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2024, 06, 19)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2024, 07, 04)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2024, 09, 02)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2024, 10, 14)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2024, 11, 11)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2024, 11, 28)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2024, 12, 25)));
			//2025
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2025, 01, 01)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2025, 01, 20)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2025, 02, 17)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2025, 05, 26)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2025, 06, 19)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2025, 07, 04)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2025, 09, 01)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2025, 10, 13)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2025, 11, 11)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2025, 11, 27)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2025, 12, 25)));
			//2026
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2026, 01, 01)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2026, 01, 19)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2026, 02, 16)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2026, 05, 25)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2026, 06, 19)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2026, 07, 03)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2026, 09, 07)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2026, 10, 12)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2026, 11, 11)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2026, 11, 26)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2026, 12, 25)));
			//2027
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2027, 01, 01)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2027, 01, 18)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2027, 02, 15)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2027, 05, 31)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2027, 06, 18)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2027, 07, 05)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2027, 09, 06)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2027, 10, 11)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2027, 11, 11)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2027, 11, 25)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2027, 12, 24)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2027, 12, 31)));
			//2028
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2028, 01, 17)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2028, 02, 21)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2028, 05, 29)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2028, 06, 19)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2028, 07, 04)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2028, 09, 04)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2028, 10, 09)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2028, 11, 10)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2028, 11, 23)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2028, 12, 25)));
			//2029
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2029, 01, 01)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2029, 01, 15)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2029, 02, 19)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2029, 05, 28)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2029, 06, 19)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2029, 07, 04)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2029, 09, 03)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2029, 10, 08)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2029, 11, 12)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2029, 11, 22)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2029, 12, 25)));
			//2030
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2030, 01, 01)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2030, 01, 21)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2030, 02, 18)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2030, 05, 27)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2030, 06, 19)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2030, 07, 04)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2030, 09, 02)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2030, 10, 14)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2030, 11, 11)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2030, 11, 28)));
			AssertEquals(true, workingDays.IsDateTimeAHoliday(new DateTime(2030, 12, 25)));
		}

		public void TestFutureHolidaysAreUpdated()
		{
			// url for holidays: eg. http://www.opm.gov/Operating_Status_Schedules/fedhol/2014.asp
			Assert("US Customs hard coded Federal Holidays need to be updated.", ZDateTime.Now < new ZDateTime(2030, 06, 01));
		}

		CustomsWorkingDays workingDays;
		protected override void SetUp()
		{
			base.SetUp();
			workingDays = CustomsWorkingDays.GetInstance(Factory);
		}
	}
}
