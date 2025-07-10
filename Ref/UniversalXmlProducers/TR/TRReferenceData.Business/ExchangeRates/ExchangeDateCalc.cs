using System;
using System.Linq;

namespace CargoWise.RefDbRepo.TRReferenceData.Business.ExchangeRates
{
	public static class ExchangeDateCalc
	{
		public static void AvailableDateRange(DateTime publicationDate, out DateTime startDate, out DateTime endDate)
		{
			startDate = publicationDate.AddDays(1);
			endDate = startDate;

			while (endDate.DayOfWeek == DayOfWeek.Saturday || endDate.DayOfWeek == DayOfWeek.Sunday || nationalHoliday.Contains(new DateTime(0001, endDate.Month, endDate.Day)) || religiousHoliday.Contains(endDate))
			{
				endDate = endDate.AddDays(1);
			}
		}

		static readonly DateTime[] nationalHoliday =
		{
			new DateTime(0001,  1,  1), // New Year's Day
			new DateTime(0001,  4, 23), // National Sovereignty and Children's Day
			new DateTime(0001,  5,  1), // Labor and Solidarity Day
			new DateTime(0001,  5, 19), // Commemoration of Atatürk, Youth and Sports Day
			new DateTime(0001,  7, 15), // Democracy and National Unity Day
			new DateTime(0001,  8, 30), // Victory Day
			new DateTime(0001, 10, 29), // Republic Day
		};

		readonly public static DateTime[] religiousHoliday =
		{
			new DateTime(2019, 6, 4), // Feast of Ramadan (Arefe)
			new DateTime(2019, 6, 5), // Feast of Ramadan (1st day)
			new DateTime(2019, 6, 6), // Feast of Ramadan (2nd day)
			new DateTime(2019, 6, 7), // Feast of Ramadan (3rd day)
			new DateTime(2019, 8, 10), // Eid al-Adha (Arefe)
			new DateTime(2019, 8, 11), // Eid al-Adha (1st day)
			new DateTime(2019, 8, 12), // Eid al-Adha (2nd day)
			new DateTime(2019, 8, 13), // Eid al-Adha (3rd day)
			new DateTime(2019, 8, 14), // Eid al-Adha (4th day)

			new DateTime(2020, 5, 23), // Feast of Ramadan (Arefe)
			new DateTime(2020, 5, 24), // Feast of Ramadan (1st day)
			new DateTime(2020, 5, 25), // Feast of Ramadan (2nd day)
			new DateTime(2020, 5, 26), // Feast of Ramadan (3rd day)
			new DateTime(2020, 7, 30), // Eid al-Adha (Arefe)
			new DateTime(2020, 7, 31), // Eid al-Adha (1st day)
			new DateTime(2020, 8, 01), // Eid al-Adha (2nd day)
			new DateTime(2020, 8, 02), // Eid al-Adha (3rd day)
			new DateTime(2020, 8, 03), // Eid al-Adha (4th day)

			new DateTime(2021, 5, 12), // Feast of Ramadan (Arefe)
			new DateTime(2021, 5, 13), // Feast of Ramadan (1st day)
			new DateTime(2021, 5, 14), // Feast of Ramadan (2nd day)
			new DateTime(2021, 5, 15), // Feast of Ramadan (3rd day)
			new DateTime(2021, 7, 19), // Eid al-Adha (Arefe)
			new DateTime(2021, 7, 20), // Eid al-Adha (1st day)
			new DateTime(2021, 7, 21), // Eid al-Adha (2nd day)
			new DateTime(2021, 7, 22), // Eid al-Adha (3rd day)
			new DateTime(2021, 7, 23), // Eid al-Adha (4th day)

			new DateTime(2022, 5,  1), // Feast of Ramadan (Arefe)
			new DateTime(2022, 5,  2), // Feast of Ramadan (1st day)
			new DateTime(2022, 5,  3), // Feast of Ramadan (2nd day)
			new DateTime(2022, 5,  4), // Feast of Ramadan (3rd day)
			new DateTime(2022, 7,  8), // Eid al-Adha (Arefe)
			new DateTime(2022, 7,  9), // Eid al-Adha (1st day)
			new DateTime(2022, 7, 10), // Eid al-Adha (2nd day)
			new DateTime(2022, 7, 11), // Eid al-Adha (3rd day)
			new DateTime(2022, 7, 12), // Eid al-Adha (4th day)

			new DateTime(2023, 4, 20), // Feast of Ramadan (Arefe)
			new DateTime(2023, 4, 21), // Feast of Ramadan (1st day)
			new DateTime(2023, 4, 22), // Feast of Ramadan (2nd day)
			new DateTime(2023, 4, 23), // Feast of Ramadan (3rd day)
			new DateTime(2023, 6, 27), // Eid al-Adha (Arefe)
			new DateTime(2023, 6, 28), // Eid al-Adha (1st day)
			new DateTime(2023, 6, 29), // Eid al-Adha (2nd day)
			new DateTime(2023, 6, 30), // Eid al-Adha (3rd day)
			new DateTime(2023, 7, 1), // Eid al-Adha (4th day)

			new DateTime(2024, 4, 9), // Feast of Ramadan (Arefe)
			new DateTime(2024, 4, 10), // Feast of Ramadan (1st day)
			new DateTime(2024, 4, 11), // Feast of Ramadan (2nd day)
			new DateTime(2024, 4, 12), // Feast of Ramadan (3rd day)
			new DateTime(2024, 6, 15), // Eid al-Adha (Arefe)
			new DateTime(2024, 6, 16), // Eid al-Adha (1st day)
			new DateTime(2024, 6, 17), // Eid al-Adha (2nd day)
			new DateTime(2024, 6, 18), // Eid al-Adha (3rd day)
			new DateTime(2024, 7, 19), // Eid al-Adha (4th day)

			new DateTime(2025, 3, 29), // Feast of Ramadan (Arefe)
			new DateTime(2025, 3, 30), // Feast of Ramadan (1st day)
			new DateTime(2025, 3, 31), // Feast of Ramadan (2nd day)
			new DateTime(2025, 4, 1), // Feast of Ramadan (3rd day)
			new DateTime(2025, 6, 5), // Eid al-Adha (Arefe)
			new DateTime(2025, 6, 6), // Eid al-Adha (1st day)
			new DateTime(2025, 6, 7), // Eid al-Adha (2nd day)
			new DateTime(2025, 6, 8), // Eid al-Adha (3rd day)
			new DateTime(2025, 6, 9), // Eid al-Adha (4th day)
		};
	}
}
