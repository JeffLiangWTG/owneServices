using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.CalendarArithmetic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class CustomsWorkingDays : WorkTimeArithmetic
	{
		public CustomsWorkingDays(ICalendarDataSource dataSource)
			: base(dataSource)
		{
		}

		public static CustomsWorkingDays GetInstance(BusinessObjectFactory factory, ZGuid departmentPK = default(ZGuid), ZGuid branchPK = default(ZGuid), ZGuid staffPK = default(ZGuid))
		{
			ICalendarDataSource dataSource = new CalendarArithmeticDataSource(factory, departmentPK, branchPK, staffPK);
			dataSource = new CachedCalendarDataSource(dataSource);
			var creator = new Func<WorkTimeArithmetic>(() => new CustomsWorkingDays(dataSource));
			return (CustomsWorkingDays)WorkingDays.GetInstance(factory, departmentPK, branchPK, staffPK, creator);
		}

		protected override bool IsDayABranchHolidayCore(DateTime dateToCheck)
		{
			return IsDateACustomsFederalHoliday(dateToCheck);
		}

		public bool IsDateACustomsFederalHoliday(ZDateTime dateToCheck)
		{
			return CheckHardCodedFederalCustomsHolidays(dateToCheck);
		}

		#region Implementation

		bool CheckHardCodedFederalCustomsHolidays(ZDateTime dateToCheck)
		{
			dateToCheck = dateToCheck.Date;
			return GetFederalHolidaysList.Any(holiday => dateToCheck == holiday.Date);
		}

		IEnumerable<DateTime> GetFederalHolidaysList
		{
			get
			{
				if (holidays == null)
				{
					holidays = new List<DateTime>
					{
						//2008
						new DateTime(2008, 12, 25),

						//2009
						new DateTime(2009, 01, 01),
						new DateTime(2009, 01, 19),
						new DateTime(2009, 02, 16),
						new DateTime(2009, 05, 25),
						new DateTime(2009, 07, 03),
						new DateTime(2009, 09, 07),
						new DateTime(2009, 10, 12),
						new DateTime(2009, 11, 11),
						new DateTime(2009, 11, 26),
						new DateTime(2009, 12, 25),

						//2010
						new DateTime(2010, 01, 01),
						new DateTime(2010, 01, 18),
						new DateTime(2010, 02, 15),
						new DateTime(2010, 05, 31),
						new DateTime(2010, 07, 05),
						new DateTime(2010, 09, 06),
						new DateTime(2010, 10, 11),
						new DateTime(2010, 11, 11),
						new DateTime(2010, 11, 25),
						new DateTime(2010, 12, 24),

						//2011
						new DateTime(2010, 12, 31),
						new DateTime(2011, 01, 17),
						new DateTime(2011, 02, 21),
						new DateTime(2011, 05, 30),
						new DateTime(2011, 07, 04),
						new DateTime(2011, 09, 05),
						new DateTime(2011, 10, 10),
						new DateTime(2011, 11, 11),
						new DateTime(2011, 11, 24),
						new DateTime(2011, 12, 26),

						//2012
						new DateTime(2012, 01, 02),
						new DateTime(2012, 01, 16),
						new DateTime(2012, 02, 20),
						new DateTime(2012, 05, 28),
						new DateTime(2012, 07, 04),
						new DateTime(2012, 09, 03),
						new DateTime(2012, 10, 08),
						new DateTime(2012, 11, 12),
						new DateTime(2012, 11, 22),
						new DateTime(2012, 12, 25),

						//2013
						new DateTime(2013, 01, 01),
						new DateTime(2013, 01, 21),
						new DateTime(2013, 02, 18),
						new DateTime(2013, 05, 27),
						new DateTime(2013, 07, 04),
						new DateTime(2013, 09, 02),
						new DateTime(2013, 10, 14),
						new DateTime(2013, 11, 11),
						new DateTime(2013, 11, 28),
						new DateTime(2013, 12, 25),

						//2014
						new DateTime(2014, 01, 01),
						new DateTime(2014, 01, 20),
						new DateTime(2014, 02, 17),
						new DateTime(2014, 05, 26),
						new DateTime(2014, 07, 04),
						new DateTime(2014, 09, 01),
						new DateTime(2014, 10, 13),
						new DateTime(2014, 11, 11),
						new DateTime(2014, 11, 27),
						new DateTime(2014, 12, 25),

						//2015
						new DateTime(2015, 01, 01),
						new DateTime(2015, 01, 19),
						new DateTime(2015, 02, 16),
						new DateTime(2015, 05, 25),
						new DateTime(2015, 07, 03),
						new DateTime(2015, 09, 07),
						new DateTime(2015, 10, 12),
						new DateTime(2015, 11, 11),
						new DateTime(2015, 11, 26),
						new DateTime(2015, 12, 25),

						//2016
						new DateTime(2016, 01, 01),
						new DateTime(2016, 01, 18),
						new DateTime(2016, 02, 15),
						new DateTime(2016, 05, 30),
						new DateTime(2016, 07, 04),
						new DateTime(2016, 09, 05),
						new DateTime(2016, 10, 10),
						new DateTime(2016, 11, 11),
						new DateTime(2016, 11, 24),
						new DateTime(2016, 12, 26),

						//2017
						new DateTime(2017, 01, 02),
						new DateTime(2017, 01, 16),
						new DateTime(2017, 02, 20),
						new DateTime(2017, 05, 29),
						new DateTime(2017, 07, 04),
						new DateTime(2017, 09, 04),
						new DateTime(2017, 10, 09),
						new DateTime(2017, 11, 10),
						new DateTime(2017, 11, 23),
						new DateTime(2017, 12, 25),

						//2018
						new DateTime(2018, 01, 01),
						new DateTime(2018, 01, 15),
						new DateTime(2018, 02, 19),
						new DateTime(2018, 05, 28),
						new DateTime(2018, 07, 04),
						new DateTime(2018, 09, 03),
						new DateTime(2018, 10, 08),
						new DateTime(2018, 11, 12),
						new DateTime(2018, 11, 22),
						new DateTime(2018, 12, 25),

						//2019
						new DateTime(2019, 01, 01),
						new DateTime(2019, 01, 21),
						new DateTime(2019, 02, 18),
						new DateTime(2019, 05, 27),
						new DateTime(2019, 07, 04),
						new DateTime(2019, 09, 02),
						new DateTime(2019, 10, 14),
						new DateTime(2019, 11, 11),
						new DateTime(2019, 11, 28),
						new DateTime(2019, 12, 25),

						//2020
						new DateTime(2020, 01, 01),
						new DateTime(2020, 01, 20),
						new DateTime(2020, 02, 17),
						new DateTime(2020, 05, 25),
						new DateTime(2020, 07, 03),
						new DateTime(2020, 09, 07),
						new DateTime(2020, 10, 12),
						new DateTime(2020, 11, 11),
						new DateTime(2020, 11, 26),
						new DateTime(2020, 12, 25),

						//2021
						new DateTime(2021, 01, 01),
						new DateTime(2021, 01, 18),
						new DateTime(2021, 02, 15),
						new DateTime(2021, 05, 31),
						new DateTime(2021, 07, 05),
						new DateTime(2021, 09, 06),
						new DateTime(2021, 10, 11),
						new DateTime(2021, 11, 11),
						new DateTime(2021, 11, 25),
						new DateTime(2021, 12, 24),
						new DateTime(2021, 12, 31),

						//2022
						new DateTime(2022, 01, 17),
						new DateTime(2022, 02, 21),
						new DateTime(2022, 05, 30),
						new DateTime(2022, 06, 20),
						new DateTime(2022, 07, 04),
						new DateTime(2022, 09, 05),
						new DateTime(2022, 10, 10),
						new DateTime(2022, 11, 11),
						new DateTime(2022, 11, 24),
						new DateTime(2022, 12, 26),

						//2023
						new DateTime(2023, 01, 02),
						new DateTime(2023, 01, 16),
						new DateTime(2023, 02, 20),
						new DateTime(2023, 05, 29),
						new DateTime(2023, 06, 19),
						new DateTime(2023, 07, 04),
						new DateTime(2023, 09, 04),
						new DateTime(2023, 10, 09),
						new DateTime(2023, 11, 10),
						new DateTime(2023, 11, 23),
						new DateTime(2023, 12, 25),

						//2024
						new DateTime(2024, 01, 01),
						new DateTime(2024, 01, 15),
						new DateTime(2024, 02, 19),
						new DateTime(2024, 05, 27),
						new DateTime(2024, 06, 19),
						new DateTime(2024, 07, 04),
						new DateTime(2024, 09, 02),
						new DateTime(2024, 10, 14),
						new DateTime(2024, 11, 11),
						new DateTime(2024, 11, 28),
						new DateTime(2024, 12, 25),

						//2025
						new DateTime(2025, 01, 01),
						new DateTime(2025, 01, 20),
						new DateTime(2025, 02, 17),
						new DateTime(2025, 05, 26),
						new DateTime(2025, 06, 19),
						new DateTime(2025, 07, 04),
						new DateTime(2025, 09, 01),
						new DateTime(2025, 10, 13),
						new DateTime(2025, 11, 11),
						new DateTime(2025, 11, 27),
						new DateTime(2025, 12, 25),

						//2026
						new DateTime(2026, 01, 01),
						new DateTime(2026, 01, 19),
						new DateTime(2026, 02, 16),
						new DateTime(2026, 05, 25),
						new DateTime(2026, 06, 19),
						new DateTime(2026, 07, 03),
						new DateTime(2026, 09, 07),
						new DateTime(2026, 10, 12),
						new DateTime(2026, 11, 11),
						new DateTime(2026, 11, 26),
						new DateTime(2026, 12, 25),

						//2027
						new DateTime(2027, 01, 01),
						new DateTime(2027, 01, 18),
						new DateTime(2027, 02, 15),
						new DateTime(2027, 05, 31),
						new DateTime(2027, 06, 18),
						new DateTime(2027, 07, 05),
						new DateTime(2027, 09, 06),
						new DateTime(2027, 10, 11),
						new DateTime(2027, 11, 11),
						new DateTime(2027, 11, 25),
						new DateTime(2027, 12, 24),
						new DateTime(2027, 12, 31),

						//2028
						new DateTime(2028, 01, 17),
						new DateTime(2028, 02, 21),
						new DateTime(2028, 05, 29),
						new DateTime(2028, 06, 19),
						new DateTime(2028, 07, 04),
						new DateTime(2028, 09, 04),
						new DateTime(2028, 10, 09),
						new DateTime(2028, 11, 10),
						new DateTime(2028, 11, 23),
						new DateTime(2028, 12, 25),

						//2029
						new DateTime(2029, 01, 01),
						new DateTime(2029, 01, 15),
						new DateTime(2029, 02, 19),
						new DateTime(2029, 05, 28),
						new DateTime(2029, 06, 19),
						new DateTime(2029, 07, 04),
						new DateTime(2029, 09, 03),
						new DateTime(2029, 10, 08),
						new DateTime(2029, 11, 12),
						new DateTime(2029, 11, 22),
						new DateTime(2029, 12, 25),

						//2030
						new DateTime(2030, 01, 01),
						new DateTime(2030, 01, 21),
						new DateTime(2030, 02, 18),
						new DateTime(2030, 05, 27),
						new DateTime(2030, 06, 19),
						new DateTime(2030, 07, 04),
						new DateTime(2030, 09, 02),
						new DateTime(2030, 10, 14),
						new DateTime(2030, 11, 11),
						new DateTime(2030, 11, 28),
						new DateTime(2030, 12, 25),
					};
				}

				return holidays.ToArray();
			}
		}

		List<DateTime> holidays;

		#endregion
	}
}
