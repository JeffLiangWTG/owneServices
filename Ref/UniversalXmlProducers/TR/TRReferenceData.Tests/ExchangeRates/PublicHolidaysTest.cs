using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Business.ExchangeRates;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.ExchangeRates
{
	[TestFixture]
	class HolidaysTest
	{
		[Test]
		public void CheckNextYearPublicHoliday()
		{
			DateTime currentDate = DateTime.Now;
			currentDate = currentDate.AddYears(1);

			if (currentDate.Month == 12)
			{
				bool availableHoliday = false;
				DateTime startDate = new DateTime(currentDate.Year, 01, 01);
				DateTime stopDate = new DateTime(currentDate.Year, 12, 31);

				for (DateTime dateTime = startDate; dateTime < stopDate; dateTime += TimeSpan.FromDays(1))
				{
					if (ExchangeDateCalc.religiousHoliday.Contains(dateTime))
					{
						availableHoliday = true;
						break;
					}
				}

				Assert.That(true, Is.EqualTo(availableHoliday));
			}
			else
			{
				Assert.That(true);
			}
		}

		[TestCase(2019, 12, 31, ExpectedResult = "2020-01-01 to 2020-01-02")]
		[TestCase(2020, 01, 03, ExpectedResult = "2020-01-04 to 2020-01-06")]
		[TestCase(2020, 04, 22, ExpectedResult = "2020-04-23 to 2020-04-24")]
		[TestCase(2020, 05, 18, ExpectedResult = "2020-05-19 to 2020-05-20")]
		[TestCase(2020, 05, 22, ExpectedResult = "2020-05-23 to 2020-05-27")]
		[TestCase(2020, 07, 14, ExpectedResult = "2020-07-15 to 2020-07-16")]
		[TestCase(2020, 07, 29, ExpectedResult = "2020-07-30 to 2020-08-04")]
		[TestCase(2020, 10, 28, ExpectedResult = "2020-10-29 to 2020-10-30")]
		[TestCase(2023, 01, 06, ExpectedResult = "2023-01-07 to 2023-01-09")]
		[TestCase(2023, 10, 28, ExpectedResult = "2023-10-29 to 2023-10-30")]
		[TestCase(2022, 12, 31, ExpectedResult = "2023-01-01 to 2023-01-02")]
		[TestCase(2023, 12, 31, ExpectedResult = "2024-01-01 to 2024-01-02")]
		[TestCase(2024, 06, 16, ExpectedResult = "2024-06-17 to 2024-06-19")]
		[TestCase(2024, 04, 10, ExpectedResult = "2024-04-11 to 2024-04-15")]
		public string TestCalculateValidDate(int year, int month, int day)
		{
			DateTime testingDate = new DateTime(year, month, day);
			ExchangeDateCalc.AvailableDateRange(testingDate, out DateTime startDate, out DateTime endDate);
			return startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " to " + endDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		}
	}
}
