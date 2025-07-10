using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public enum PostDateValidationResult { OK, PeriodNotFound, SubLedgerPeriodClosed, GLPeriodClosed }

	public class AccountingPeriodCalculator
	{
		public AccountingPeriodCalculator(BusinessObjectFactory factory)
			: this(factory, GlbCompany.CurrentCompany)
		{
		}

		public AccountingPeriodCalculator(BusinessObjectFactory factory, GlbCompany company)
		{
			this.fFactory = factory;
			this.Company = company;
		}

		public const int InvalidPeriod = 000000;

		public static string GetInvalidPeriodValidationError(ZDateTime date, GlbCompany company = null)
		{
			return GetInvalidPeriodValidationErrorWithCompanyInfo(date.ToShortDateString(), company);
		}

		public static string GetInvalidPeriodValidationError(int period, GlbCompany company = null)
		{
			return GetInvalidPeriodValidationErrorWithCompanyInfo(period.ToString(), company);
		}

		public static string GetInvalidPeriodValidationError(ZString periodString)
		{
			return Res.GetString("B3D048B4-3E9F-4982-AAFA-5AAD1F2FFD10", "There is no period set up for {0}.", periodString);
		}

		protected static string GetInvalidPeriodValidationError(string stringToDisplay)
		{
			return Res.GetString("d04eb69d-23b7-4e5c-902a-394845c5253d", "There is no period set up for {0}.\r\nPlease go to General Ledger >> Period Management to setup periods.", stringToDisplay);
		}

		public static string GetInvalidPeriodValidationErrorWithCompanyInfo(string stringToDisplay, GlbCompany company)
		{
			if (company == null || company.PK == GlbCompany.CurrentCompany.PK)
			{
				return GetInvalidPeriodValidationError(stringToDisplay);
			}

			return Res.GetString("2E691B56-1AB7-4CD3-AA3F-66E74B69D283", "There is no period set up for {0} in {1} - {2}.\r\nPlease go to General Ledger >> Period Management to setup periods.", stringToDisplay, company.GC_Code, company.GC_Name);
		}

		public AccPeriodManagement GetPeriodManagementFromDate(ZDateTime proposedDate)
		{
			return GetPeriodManagementFromDate(proposedDate, Company.PK);
		}

		public AccPeriodManagement GetPeriodManagementFromDate(ZDateTime proposedDate, ZGuid companyPK)
		{
			if (proposedDate.IsValid && proposedDate.IsValidSmallDateTime)
			{
				ZQuery filter;
				filter = new ZQuery(AccPeriodManagementSchema.AM_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, proposedDate);
				filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_StartDate, SQLComparisonOperator.LessThanOrEqualTo, proposedDate);
				filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, companyPK);
				filter.OrderBy = AccPeriodManagementSchema.Constants.AM_Period + " DESC";
				return fFactory.LoadTop1(typeof(AccPeriodManagement), filter) as AccPeriodManagement;
			}
			else
			{
				return null;
			}
		}

		public AccPeriodManagement GetFirstPeriodManagementFromDate(ZDateTime proposedDate, ZGuid companyPK)
		{
			if (proposedDate.IsValid && proposedDate.IsValidSmallDateTime)
			{
				var periodManagement = GetPeriodManagementFromDate(proposedDate, companyPK);
				if (periodManagement == null)
				{
					return null;
				}

				var sqlQuery = new ZDBOnlyQuery(typeof(AccPeriodManagement));
				sqlQuery.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, companyPK);
				sqlQuery.AddToFilter(AccPeriodManagementSchema.AM_Year, periodManagement.AM_Year);
				sqlQuery.OrderBy = AccPeriodManagementSchema.Constants.AM_StartDate;

				return fFactory.LoadTop1<AccPeriodManagement>(sqlQuery);
			}
			else
			{
				return null;
			}
		}

		public AccPeriodManagement GetFirstPeriodFromYear(ZInt year)
		{
			var query = new ZQuery(AccPeriodManagementSchema.AM_Year, (short)year);
			query.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, Company.PK);
			query.OrderBy = AccPeriodManagement.Schema.AM_Period;
			return fFactory.LoadTop1<AccPeriodManagement>(query);
		}

		public AccPeriodManagement GetPreviousPeriodManagementFromDate(ZDateTime proposedDate, ZGuid companyPK)
		{
			if (proposedDate.IsValid && proposedDate.IsValidSmallDateTime)
			{
				var sqlQuery = new ZDBOnlyQuery(typeof(AccPeriodManagement));
				sqlQuery.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, companyPK);
				sqlQuery.AddToFilter(AccPeriodManagementSchema.AM_EndDate, SQLComparisonOperator.LessThanOrEqualTo, proposedDate);
				sqlQuery.OrderBy = AccPeriodManagementSchema.Constants.AM_Period + " DESC";
				
				return fFactory.LoadTop1<AccPeriodManagement>(sqlQuery);
			}
			else
			{
				return null;
			}
		}

		public AccPeriodManagement GetNextSubLedgerOpenPeriodManagementFromDate(ZDateTime proposedDate, ZGuid companyPK)
		{
			if (proposedDate.IsValid && proposedDate.IsValidSmallDateTime)
			{
				ZQuery filter;
				filter = new ZQuery(AccPeriodManagementSchema.AM_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, proposedDate);
				filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, companyPK);
				filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_IsSubLedgerClosed, ZBool.False);
				filter.OrderBy = AccPeriodManagementSchema.Constants.AM_Period;
				return fFactory.LoadTop1(typeof(AccPeriodManagement), filter) as AccPeriodManagement;
			}
			else
			{
				return null;
			}
		}

		public AccPeriodManagement GetNextGeneralLedgerOpenPeriodManagementFromDate(ZDateTime proposedDate, ZGuid companyPK)
		{
			if (proposedDate.IsValid && proposedDate.IsValidSmallDateTime)
			{
				ZQuery filter;
				filter = new ZQuery(AccPeriodManagementSchema.AM_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, proposedDate);
				filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, companyPK);
				filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_IsGeneralLedgerClosed, ZBool.False);
				filter.OrderBy = AccPeriodManagementSchema.Constants.AM_Period;
				return fFactory.LoadTop1(typeof(AccPeriodManagement), filter) as AccPeriodManagement;
			}
			else
			{
				return null;
			}
		}

		public AccPeriodManagement GetFirstOpenPeriod(ZGuid companyPK)
		{
			ZQuery filter;
			filter = new ZQuery();
			filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, companyPK);
			filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_IsSubLedgerClosed, ZBool.False);
			filter.OrderBy = AccPeriodManagementSchema.Constants.AM_Period;
			return fFactory.LoadTop1<AccPeriodManagement>(filter);
		}

		public ZInt GetPeriodFromDate(ZDateTime date)
		{
			ZInt result = InvalidPeriod;
			var periodManagementFromDate = GetPeriodManagementFromDate(date);
			if (periodManagementFromDate != null)
			{
				result = periodManagementFromDate.AM_Period;
			}
			return result;
		}

		public ZInt GetPeriodFromDate(ZDateTime date, ZGuid companyPK)
		{
			ZInt result = InvalidPeriod;
			var periodManagementFromDate = GetPeriodManagementFromDate(date, companyPK);
			if (periodManagementFromDate != null)
			{
				result = periodManagementFromDate.AM_Period;
			}
			return result;
		}

		public ZDateTime GetFirstDayForPeriod(ZDateTime proposedDate)
		{
			var period = GetPeriodManagementFromDate(proposedDate);
			return (period != null) ? period.AM_StartDate : ZDateTime.Invalid;
		}

		public ZDateTime GetLastDayForPeriod(ZDateTime proposedDate)
		{
			var period = GetPeriodManagementFromDate(proposedDate);
			return (period != null) ? period.AM_EndDate : ZDateTime.Invalid;
		}

		public PostDateValidationResult IsPostDateValid(ZDateTime proposedDate)
		{
			var period = GetPeriodManagementFromDate(proposedDate);

			if (period == null)
			{
				return PostDateValidationResult.PeriodNotFound;
			}
			else if (period.AM_IsSubLedgerClosed)
			{
				return PostDateValidationResult.SubLedgerPeriodClosed;
			}
			else if (period.AM_IsGeneralLedgerClosed)
			{
				return PostDateValidationResult.GLPeriodClosed;
			}
			else
			{
				return PostDateValidationResult.OK;
			}
		}

		public ZDateTime GetLastDayForPeriod(int period)
		{
			var retrievedPeriod = GetPeriodManagementFromPeriod(period);
			var result = ZDateTime.Invalid;
			if (retrievedPeriod != null)
			{
				result = retrievedPeriod.AM_EndDate;
			}
			return result;
		}

		public ZDateTime GetFirstDayForPeriod(int period)
		{
			var retrievedPeriod = GetPeriodManagementFromPeriod(period);
			var result = ZDateTime.Invalid;
			if (retrievedPeriod != null)
			{
				result = retrievedPeriod.AM_StartDate;
			}
			return result;
		}

		public ZBool IsPeriodValid(int period)
		{
			var retrievedPeriod = GetPeriodManagementFromPeriod(period);
			return retrievedPeriod != null;
		}

		public ZBool IsFuturePeriod(ZInt period)
		{
			var startDate = GetFirstDayForPeriod(period);
			return ZDateTime.Now < startDate;
		}

		public ZBool IsCurrentPeriod(ZInt period)
		{
			var startDate = GetFirstDayForPeriod(period);
			var endDate = GetLastDayForPeriod(period);
			var currentDate = ZDateTime.Now;
			return currentDate >= startDate && currentDate <= endDate;
		}

		public ZBool IsCurrentPeriod(ZDateTime dateTime)
		{
			var period = GetPeriodFromDate(dateTime);
			return IsCurrentPeriod(period);
		}

		public ZBool IsPreviousPeriod(ZInt period)
		{
			var endDate = GetLastDayForPeriod(period);
			return ZDateTime.Now > endDate;
		}

		public ZBool IsPeriodGLClosed(ZInt period)
		{
			var result = false;
			if (period != AccountingPeriodCalculator.InvalidPeriod)
			{
				var periodManagement = GetPeriodManagementFromPeriod(period);
				if (periodManagement != null)
				{
					result = periodManagement.AM_IsGeneralLedgerClosed;
				}
			}
			return result;
		}

		public ZBool IsPeriodSubLedgerClosed(ZInt period)
		{
			var result = false;
			if (period != AccountingPeriodCalculator.InvalidPeriod)
			{
				var periodManagement = GetPeriodManagementFromPeriod(period);
				if (periodManagement != null)
				{
					result = periodManagement.AM_IsSubLedgerClosed;
				}
			}
			return result;
		}

		public ZBool IsPeriodSubledgerClosedForAdjustments(ZInt period)
		{
			var result = false;
			if (period != AccountingPeriodCalculator.InvalidPeriod)
			{
				var periodManagement = GetPeriodManagementFromPeriod(period);
				if (periodManagement != null)
				{
					result = periodManagement.AM_IsSubledgerClosedForAdjustments;
				}
			}
			return result;
		}

		public ZInt GetPeriodCount(ZInt startPeriod, ZInt endPeriod)
		{
			var aMPeriodGT = new ZQuery(AccPeriodManagementSchema.AM_Period, SQLComparisonOperator.GreaterThanOrEqualTo, startPeriod);
			var aMPeriodLT = new ZQuery(AccPeriodManagementSchema.AM_Period, SQLComparisonOperator.LessThanOrEqualTo, endPeriod);
			var filter = new ZQuery(aMPeriodGT, JoinCondition.And, aMPeriodLT);

			filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, Company.PK);
			var periods = (AccPeriodManagement[])fFactory.Load(typeof(AccPeriodManagement), filter);
			return periods.Length;
		}

		public AccPeriodManagement[] GetRangeOfPeriods(ZDate startPeriod, ZDate endPeriod)
		{
			var periods = Array.Empty<AccPeriodManagement>();
			var firstDay = GetFirstDayForPeriod(startPeriod);
			var lastDay = GetLastDayForPeriod(endPeriod);
			if (firstDay.IsValid && lastDay.IsValid)
			{
				var aMPeriodGT = new ZQuery(AccPeriodManagementSchema.AM_StartDate, SQLComparisonOperator.GreaterThanOrEqualTo, firstDay);
				var aMPeriodLT = new ZQuery(AccPeriodManagementSchema.AM_EndDate, SQLComparisonOperator.LessThanOrEqualTo, lastDay);
				var filter = new ZQuery(aMPeriodGT, JoinCondition.And, aMPeriodLT);
				filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, Company.PK);
				periods = (AccPeriodManagement[])fFactory.Load(typeof(AccPeriodManagement), filter);
			}
			return periods;
		}

		public ZInt GetPeriodCount(ZDateTime startDate, ZDateTime endDate)
		{
			return GetPeriodCount(GetPeriodFromDate(startDate), GetPeriodFromDate(endDate));
		}

		public ZInt GetPreviousPeriod(ZInt period)
		{
			var periodFromInt = GetPeriodManagementFromPeriod(period);
			ZInt result = InvalidPeriod;
			if (periodFromInt != null)
			{
				var previousPeriod = GetPeriodManagementFromDate(periodFromInt.AM_StartDate.AddDays(-1));
				if (previousPeriod != null)
				{
					result = previousPeriod.AM_Period;
				}
			}
			return result;
		}

		public ZInt GetNextPeriod(ZInt period)
		{
			var periodFromInt = GetPeriodManagementFromPeriod(period);
			ZInt result = InvalidPeriod;
			if (periodFromInt != null)
			{
				var nextPeriod = GetPeriodManagementFromDate(periodFromInt.AM_EndDate.AddDays(1));
				if (nextPeriod != null)
				{
					result = nextPeriod.AM_Period;
				}
			}
			return result;
		}

		public ZInt GetFirstPeriodForYear(ZInt year)
		{
			var query = new ZQuery(AccPeriodManagementSchema.AM_Year, (short)year);
			query.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, Company.PK);
			query.OrderBy = AccPeriodManagement.Schema.AM_Period;
			query.MaximumRows = 1;
			var results = (AccPeriodManagement[])fFactory.Load(typeof(AccPeriodManagement), query);
			if (results.Length > 0)
			{
				return results[0].AM_Period;
			}
			return InvalidPeriod;
		}

		public ZInt GetLastPeriodForYear(ZInt year)
		{
			var query = new ZQuery(AccPeriodManagementSchema.AM_Year, (ZShort)year);
			query.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, Company.PK);
			query.OrderBy = AccPeriodManagement.Schema.AM_Period + " DESC";
			query.MaximumRows = 1;
			var results = (AccPeriodManagement[])fFactory.Load(typeof(AccPeriodManagement), query);
			if (results.Length > 0)
			{
				return results[0].AM_Period;
			}
			return InvalidPeriod;
		}

		protected AccPeriodManagement GetPeriodManagementFromPeriod(int period)
		{
			var filter = new ZQuery(AccPeriodManagementSchema.AM_Period, period);
			filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, Company.PK);

			return fFactory.LoadTop1<AccPeriodManagement>(filter);
		}

		public AccPeriodManagement GetPeriodManagementByOffset(AccPeriodManagement period, sbyte offset)
		{
			var offsetPeriod = period;
			if (offsetPeriod != null)
			{
				for (int i = 1; i <= Math.Abs(offset); i++)
				{
					offsetPeriod = GetPeriodManagementFromDate(offset > 0 ? offsetPeriod.AM_EndDate.AddDays(1) :
																			offsetPeriod.AM_StartDate.AddDays(-1));
					if (offsetPeriod == null)
					{
						break;
					}
				}
			}
			return offsetPeriod;
		}

		#region Implementation

		protected BusinessObjectFactory fFactory;

		public GlbCompany Company { get; set; }

		#endregion

	}
}
