using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccountingPeriodTestHelper
	{
		public AccountingPeriodTestHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
			PreviousGLClosedPeriodInt = 999995;
			PreviousGeneralLedgerOpenPeriodInt = 999996;
			PreviousSubLedgerClosedPeriodInt = 999996;
			PreviousOpenPeriodInt = 999997;
			CurrentPeriodInt = 999998;
			FuturePeriodInt = 999999;
			InvalidPeriodInt = 000000;
		}

		public AccountingPeriodTestHelper() : this(new BusinessObjectFactory())
		{
		}

		public AccPeriodManagement PreviousGLClosedPeriod;
		public AccPeriodManagement PreviousSubLedgerClosedPeriod;
		public AccPeriodManagement PreviousOpenPeriod;
		public AccPeriodManagement CurrentPeriod;
		public AccPeriodManagement FuturePeriod;
		public readonly int PreviousSubLedgerClosedPeriodInt;
		public readonly int PreviousGLClosedPeriodInt;
		public readonly int PreviousOpenPeriodInt;
		public readonly int PreviousGeneralLedgerOpenPeriodInt;
		public readonly int CurrentPeriodInt;
		public readonly int FuturePeriodInt;
		public readonly int InvalidPeriodInt;
		protected BusinessObjectFactory Factory;
		public enum CalendarType { CalendarYear, FinancialYear }

		public void SetupPeriods()
		{
			DateTime currentDate = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, ZDateTime.Now.Day, 0, 0, 0).ToDateTime();
			ZDateTime previousGLClosedPeriodStartDate = currentDate.Date.Subtract(new TimeSpan(70, 0, 0, 0));
			ZDateTime previousGLClosedPeriodEndDate = previousGLClosedPeriodStartDate.Add(new TimeSpan(20, 23, 59, 0));

			ZDateTime previousSubLedgerClosedPeriodStartDate = previousGLClosedPeriodEndDate.Date.AddDays(1);
			ZDateTime previousSubLedgerClosedPeriodEndDate = previousSubLedgerClosedPeriodStartDate.Add(new TimeSpan(20, 23, 59, 0));

			ZDateTime previousOpenPeriodStartDate = previousSubLedgerClosedPeriodEndDate.Date.AddDays(1);
			ZDateTime previousOpenPeriodEndDate = previousOpenPeriodStartDate.Add(new TimeSpan(20, 23, 59, 0));

			ZDateTime currentPeriodStartDate = previousOpenPeriodEndDate.Date.AddDays(1);
			ZDateTime currentPeriodEndDate = currentDate.Add(new TimeSpan(21, 23, 59, 0));

			ZDateTime futurePeriodStartDate = currentPeriodEndDate.Date.AddDays(1);
			ZDateTime futurePeriodEndDate = futurePeriodStartDate.Add(new TimeSpan(15, 23, 59, 0));

			PreviousGLClosedPeriod = SetupPeriod(Factory, true, false, PreviousGLClosedPeriodInt, previousGLClosedPeriodStartDate, previousGLClosedPeriodEndDate);
			PreviousSubLedgerClosedPeriod = SetupPeriod(Factory, false, true, PreviousSubLedgerClosedPeriodInt, previousSubLedgerClosedPeriodStartDate, previousSubLedgerClosedPeriodEndDate);
			PreviousOpenPeriod = SetupPeriod(Factory, false, false, PreviousOpenPeriodInt, previousOpenPeriodStartDate, previousOpenPeriodEndDate);
			CurrentPeriod = SetupPeriod(Factory, false, false, CurrentPeriodInt, currentPeriodStartDate, currentPeriodEndDate);
			FuturePeriod = SetupPeriod(Factory, false, false, FuturePeriodInt, futurePeriodStartDate, futurePeriodEndDate);
			Factory.Save();
		}

		public AccPeriodManagement SetupSinglePeriod(int period, ZDateTime startDate, ZDateTime endDate)
		{
			AccPeriodManagement result = SetupPeriod(Factory, false, false, period, startDate, endDate);
			Factory.Save();
			return result;
		}

		public AccPeriodManagement SetupSinglePeriod(int period, ZDateTime startDate, ZDateTime endDate, ZGuid companyPK)
		{
			AccPeriodManagement result = SetupPeriod(Factory, false, false, period, startDate, endDate);
			result.AM_GC_Company = companyPK;
			Factory.Save();
			return result;
		}

		protected AccPeriodManagement SetupPeriod(BusinessObjectFactory factory, ZBool isGLPeriodClosed, ZBool isSubLedgerPeriodClosed, ZInt period, ZDateTime startDate, ZDateTime endDate)
		{
			AccPeriodManagement result = factory.New<AccPeriodManagement>();

			result.AM_IsGeneralLedgerClosed = isGLPeriodClosed;
			result.AM_IsSubLedgerClosed = isSubLedgerPeriodClosed;
			result.AM_StartDate = startDate;
			result.AM_EndDate = endDate;
			result.AM_Period = period;
			result.AM_Year = Convert.ToInt16(period / 100);
			result.AM_GC_Company = Env.CurrentCompany.PK;
			return result;
		}

		/// <summary>
		/// Inserts 12 Periods (one month per period) for a particular year
		/// </summary>
		/// <param name="year"></param>
		public void PostPeriodsForEntireYear(ZInt year)
		{
			PostPeriodsForEntireYear(year, GlbCompany.CurrentCompany.PK);
		}

		public void PostPeriodsForEntireYear(ZInt year, ZGuid companyPK)
		{
			PostPeriodsForEntireYear(year, companyPK, CalendarType.FinancialYear);
		}

		public void PostPeriodsForEntireYear(ZInt year, ZGuid companyPK, CalendarType calendarType)
		{
			AccPeriodManagementCollection periods = new AccPeriodManagementCollection(Factory);
			ZDateTime startDate;
			ZDateTime endDate;
			if (calendarType == CalendarType.FinancialYear)
			{
				startDate = new ZDateTime(year - 1, 7, 1, 0, 0, 0);
				endDate = new ZDateTime(year - 1, 7, 31, 23, 59, 0);
			}
			else if (calendarType == CalendarType.CalendarYear)
			{
				startDate = new ZDateTime(year, 1, 1, 0, 0, 0);
				endDate = new ZDateTime(year, 1, 31, 23, 59, 0);
			}
			else
			{
				throw new NotSupportedException("Unsupported CalendarType");
			}
			for (int periodNumber = 1; periodNumber <= 12; periodNumber++)
			{
				ZInt period = (year * 100) + periodNumber;
				ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, companyPK);
				filter.AddToFilter(AccPeriodManagementSchema.AM_Period, period);
				filter.AddToFilter(AccPeriodManagementSchema.AM_StartDate, startDate);
				filter.AddToFilter(AccPeriodManagementSchema.AM_EndDate, endDate);
				AccPeriodManagement currentPeriod = (AccPeriodManagement)Factory.LoadTop1(typeof(AccPeriodManagement), filter);
				if (currentPeriod == null)
				{
					currentPeriod = periods.AddNew();
					currentPeriod.AM_StartDate = startDate;
					currentPeriod.AM_EndDate = endDate;
					currentPeriod.AM_Year = (short)year;
					currentPeriod.AM_Period = period;
					currentPeriod.AM_GC_Company = companyPK;
				}
				startDate = startDate.AddMonths(1);
				endDate = endDate.AddMonths(1);
				endDate = new ZDateTime(endDate.Year, endDate.Month, GetDaysInMonth(endDate.Month, endDate.Year), endDate.Hour, endDate.Minute, endDate.Second);
			}
			Factory.Save();
		}

		protected int GetDaysInMonth(int month, int year)
		{
			switch (month)
			{
				case 1:
					return 31;
				case 2:
					return DateTime.IsLeapYear(year) ? 29 : 28;
				case 3:
					return 31;
				case 4:
					return 30;
				case 5:
					return 31;
				case 6:
					return 30;
				case 7:
					return 31;
				case 8:
					return 31;
				case 9:
					return 30;
				case 10:
					return 31;
				case 11:
					return 30;
				case 12:
					return 31;
				default:
					throw new NotSupportedException("Can only get no of days in month for 1-12 months. Imaginary months not supported");
			}
		}
	}
}
