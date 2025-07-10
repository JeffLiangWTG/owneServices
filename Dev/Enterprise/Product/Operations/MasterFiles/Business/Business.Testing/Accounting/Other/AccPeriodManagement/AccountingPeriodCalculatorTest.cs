using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccountingPeriodCalculatorTest : TestCaseWithFactory
	{
		public void TestGetPeriodManagementFromDate()
		{
			AssertEquals(null, Calculator.GetPeriodManagementFromDate(ZDateTime.MaxSmallDateTime));

			AssertEquals(true, Calculator.GetPeriodManagementFromDate(new ZDateTime(PreviousGLClosedPeriod.AM_StartDate.AddDays(5))).AM_IsGeneralLedgerClosed);
		}

		public void TestGetPeriodManagementFromDateHandlesMinMaxDates()
		{
			try
			{
				AssertEquals(null, Calculator.GetPeriodManagementFromDate(ZDateTime.MaxSmallDateTime.AddDays(5)));
				AssertEquals(null, Calculator.GetPeriodManagementFromDate(ZDateTime.MinSmallDateTimeValue.AddDays(-5)));
			}
			catch (Exception ex)
			{
				Fail("GetPeriodFromDate should not throw exception: " + ex.Message);
			}
		}

		public void TestGetNextSubLedgerOpenPeriodManagementFromDateHandlesMinMaxDates()
		{
			try
			{
				AssertEquals(null, Calculator.GetNextSubLedgerOpenPeriodManagementFromDate(ZDateTime.MaxSmallDateTime.AddDays(5), ZGuid.Empty));
				AssertEquals(null, Calculator.GetNextSubLedgerOpenPeriodManagementFromDate(ZDateTime.MinSmallDateTimeValue.AddDays(-5), ZGuid.Empty));
			}
			catch (Exception ex)
			{
				Fail("GetPeriodFromDate should not throw exception: " + ex.Message);
			}
		}

		public void TestIsPostDateDateValid()
		{
			AssertEquals(PostDateValidationResult.PeriodNotFound, Calculator.IsPostDateValid(ZDateTime.MaxSmallDateTime));

			AssertEquals(PostDateValidationResult.GLPeriodClosed, Calculator.IsPostDateValid(PreviousGLClosedPeriod.AM_StartDate.AddDays(2)));

			AssertEquals(PostDateValidationResult.OK, Calculator.IsPostDateValid(CurrentPeriod.AM_StartDate.AddDays(2)));
		}

		public void TestGetPeriodFromDate()
		{
			AssertEquals("Current Period From Date", CurrentPeriod.AM_Period, Calculator.GetPeriodFromDate(ZDateTime.Now.Date));

			AssertEquals("Previous Period From Date", PreviousSubLedgerClosedPeriod.AM_Period, Calculator.GetPeriodFromDate(PreviousSubLedgerClosedPeriod.AM_StartDate.AddDays(5)));

			AssertEquals("Junk Date should return invalid period", 0, Calculator.GetPeriodFromDate(new ZDateTime(1900, 1, 1)));
		}

		public void TestGetLastDayFromPeriod()
		{
			AssertEquals("End Date", CurrentPeriod.AM_EndDate.Date, Calculator.GetLastDayForPeriod(CurrentPeriod.AM_Period).Date);

			AssertEquals("Invalid Date for invalid Period", ZDateTime.Invalid, Calculator.GetLastDayForPeriod(AccountingPeriodCalculator.InvalidPeriod));
		}

		public void TestGetFirstDayFromPeriod()
		{
			AssertEquals("Start Date", CurrentPeriod.AM_StartDate.Date, Calculator.GetFirstDayForPeriod(CurrentPeriod.AM_Period).Date);

			AssertEquals("Invalid Date for Invalid Period", ZDateTime.Invalid, Calculator.GetFirstDayForPeriod(AccountingPeriodCalculator.InvalidPeriod));
		}

		public void TestIsPeriodValid()
		{
			Assert("Valid Period", Calculator.IsPeriodValid(PeriodManagementTestHelper.CurrentPeriodInt));
			Assert("Invalid Period", !Calculator.IsPeriodValid(PeriodManagementTestHelper.InvalidPeriodInt));
		}

		public void TestGetNextSubLedgerOpenPeriodManagementFromDate()
		{
			AssertEquals("Should get previous open period",
					PeriodManagementTestHelper.PreviousOpenPeriodInt,
					Calculator.GetNextSubLedgerOpenPeriodManagementFromDate(PeriodManagementTestHelper.PreviousSubLedgerClosedPeriod.AM_StartDate, GlbCompany.CurrentCompany.PK).AM_Period);
		}

		public void TestGetNextGeneralLedgerOpenPeriodManagementFromDate()
		{
			AssertEquals("Should get previous open period",
					PeriodManagementTestHelper.PreviousGeneralLedgerOpenPeriodInt,
					Calculator.GetNextGeneralLedgerOpenPeriodManagementFromDate(PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_StartDate, GlbCompany.CurrentCompany.PK).AM_Period);
		}

		public void TestIsFuturePeriod()
		{
			Assert("Future Period", Calculator.IsFuturePeriod(FuturePeriod.AM_Period));

			Assert("Current Period is not future period", !Calculator.IsFuturePeriod(CurrentPeriod.AM_Period));

			Assert("Previous Period is not future period", !Calculator.IsFuturePeriod(PreviousGLClosedPeriod.AM_Period));
		}

		public void TestIsCurrentPeriod()
		{
			Assert("Current Period", Calculator.IsCurrentPeriod(CurrentPeriod.AM_Period));

			Assert("Previous Period is not current period", !Calculator.IsCurrentPeriod(PreviousGLClosedPeriod.AM_Period));

			Assert("Future Period is not current period", !Calculator.IsCurrentPeriod(FuturePeriod.AM_Period));
		}

		public void TestIsCurrentPeriodFromDate()
		{
			Assert("Current Period", Calculator.IsCurrentPeriod(CurrentPeriod.AM_StartDate.AddDays(5)));

			Assert("Previous Period is not current period", !Calculator.IsCurrentPeriod(PreviousGLClosedPeriod.AM_StartDate.AddDays(5)));

			Assert("Future Period is not current period", !Calculator.IsCurrentPeriod(FuturePeriod.AM_StartDate.AddDays(5)));
		}

		public void TestIsPreviousPeriod()
		{
			Assert("Previous Period", Calculator.IsPreviousPeriod(PreviousGLClosedPeriod.AM_Period));

			Assert("Current Period is not previous period", !Calculator.IsPreviousPeriod(CurrentPeriod.AM_Period));

			Assert("Future Period is not previous period", !Calculator.IsPreviousPeriod(FuturePeriod.AM_Period));
		}

		public void TestIsPeriodGLClosed()
		{
			Assert("Previous GL Closed Period for invalid Period", !Calculator.IsPeriodGLClosed(0));

			Assert("Previous GL Closed Period", Calculator.IsPeriodGLClosed(PreviousGLClosedPeriod.AM_Period));

			Assert("Previous Sub Ledger Closed Period is not closed for GL", !Calculator.IsPeriodGLClosed(PreviousSubLedgerClosedPeriod.AM_Period));

			Assert("Current Period is not closed for GL", !Calculator.IsPeriodGLClosed(CurrentPeriod.AM_Period));
		}

		public void TestIsPeriodSubLedgerClosed()
		{
			Assert("Previous Sub Ledger Closed Period Invalid Period", !Calculator.IsPeriodSubLedgerClosed(0));

			Assert("Previous Sub Ledger Closed Period", Calculator.IsPeriodSubLedgerClosed(PreviousSubLedgerClosedPeriod.AM_Period));

			Assert("Previous GL Closed Period is not closed for Sub Ledger", !Calculator.IsPeriodSubLedgerClosed(PreviousGLClosedPeriod.AM_Period));

			Assert("Current Period is not closed for Sub Ledger", !Calculator.IsPeriodSubLedgerClosed(CurrentPeriod.AM_Period));
		}

		public void TestGetNoOfPeriod()
		{
			AssertEquals("No of Periods", 4, Calculator.GetPeriodCount(999995, 999998));
			AssertEquals("No of Periods", 4, Calculator.GetPeriodCount(Calculator.GetFirstDayForPeriod(999995).AddDays(1), Calculator.GetFirstDayForPeriod(999998).AddDays(1)));
		}

		public void TestGetNumberOfPeriodsWithMultipleYearsAndCompanies()
		{
			PeriodManagementTestHelper.PostPeriodsForEntireYear(2002);
			PeriodManagementTestHelper.PostPeriodsForEntireYear(2003);
			PeriodManagementTestHelper.PostPeriodsForEntireYear(2002, NonCurrentCompany.PK);
			PeriodManagementTestHelper.PostPeriodsForEntireYear(2003, NonCurrentCompany.PK);

			ZInt numberOfPeriods = Calculator.GetPeriodCount(200206, 200312);
			AssertEquals("Should be 19 periods", 19, numberOfPeriods);
		}

		public void TestGetPreviousPeriod()
		{
			ZInt actual = Calculator.GetPreviousPeriod(PeriodManagementTestHelper.CurrentPeriod.AM_Period);
			AssertEquals("Previous Period", PeriodManagementTestHelper.PreviousOpenPeriod.AM_Period, actual);

			actual = Calculator.GetPreviousPeriod(PeriodManagementTestHelper.InvalidPeriodInt);
			AssertEquals("Invalid Period", PeriodManagementTestHelper.InvalidPeriodInt, actual);

			actual = Calculator.GetPreviousPeriod(PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_Period);
			AssertEquals("Period for which there is no valid previous period", PeriodManagementTestHelper.InvalidPeriodInt, actual);
		}

		public void TestGetNextPeriod()
		{
			ZInt actual = Calculator.GetNextPeriod(PeriodManagementTestHelper.PreviousOpenPeriod.AM_Period);
			AssertEquals("Previous Period", PeriodManagementTestHelper.CurrentPeriod.AM_Period, actual);

			actual = Calculator.GetNextPeriod(PeriodManagementTestHelper.InvalidPeriodInt);
			AssertEquals("Invalid Period", PeriodManagementTestHelper.InvalidPeriodInt, actual);

			actual = Calculator.GetNextPeriod(PeriodManagementTestHelper.FuturePeriod.AM_Period);
			AssertEquals("Period for which there is no valid previous period", PeriodManagementTestHelper.InvalidPeriodInt, actual);
		}

		public void TestGetFirstPeriodForYear()
		{
			AssertEquals("First Period for Year", PreviousGLClosedPeriod.AM_Period, Calculator.GetFirstPeriodForYear(CurrentPeriod.AM_Year));

			AssertEquals("Invalid Year passed", AccountingPeriodCalculator.InvalidPeriod, Calculator.GetFirstPeriodForYear(1234));
		}

		public void TestGetLastPeriodForYear()
		{
			AssertEquals("Last Period for Year", FuturePeriod.AM_Period, Calculator.GetLastPeriodForYear(CurrentPeriod.AM_Year));

			AssertEquals("Invalid Year passed", AccountingPeriodCalculator.InvalidPeriod, Calculator.GetLastPeriodForYear(1234));
		}

		public void TestGetLastPeriodForYearAfterGettingFirst()
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);

			PeriodManagementTestHelper.PostPeriodsForEntireYear(2003);
			PeriodManagementTestHelper.PostPeriodsForEntireYear(2002);

			AssertEquals("Should get first period for 2003", 200301, Calculator.GetFirstPeriodForYear(2003));

			Calculator = new AccountingPeriodCalculator(Factory);  // This is the interface required at the moment because of some 
																   //issues with the factory and related ZQuery/ZQuery refactor that is required/planned
																   // i.e. you must make a new calculator between getting the last period after the first. 
																   //i.e. these work when run independently, but not when run in succession.

			AssertEquals("Should now get last period for 2003", 200312, Calculator.GetLastPeriodForYear(2003));
		}

		public void TestGetInvalidPeriodValidationError()
		{
			AssertEquals("There is no period set up for 200304.\r\nPlease go to General Ledger >> Period Management to setup periods.", AccountingPeriodCalculator.GetInvalidPeriodValidationError(200304));
			AssertEquals("There is no period set up for 12-May-04.\r\nPlease go to General Ledger >> Period Management to setup periods.", AccountingPeriodCalculator.GetInvalidPeriodValidationError(new ZDateTime(2004, 05, 12)));
			AssertEquals("There is no period set up for <Invalid>.\r\nPlease go to General Ledger >> Period Management to setup periods.", AccountingPeriodCalculator.GetInvalidPeriodValidationError(ZDateTime.Invalid));
			AssertEquals("There is no period set up for Test.", AccountingPeriodCalculator.GetInvalidPeriodValidationError("Test"));
		}

		public void TestGetInvalidPeriodValidationErrorWithCompanyInfo()
		{
			var company = GlbCompany.GetDemoCompany(Factory);
			company.GC_Name = "AU test";
			company.GC_Code = "DAU";

			AssertEquals("There is no period set up for 200304 in DAU - AU test.\r\nPlease go to General Ledger >> Period Management to setup periods.", AccountingPeriodCalculator.GetInvalidPeriodValidationErrorWithCompanyInfo("200304", company));
			AssertEquals("There is no period set up for 200304 in DAU - AU test.\r\nPlease go to General Ledger >> Period Management to setup periods.", AccountingPeriodCalculator.GetInvalidPeriodValidationError(200304, company));
			AssertEquals("There is no period set up for 12-May-04 in DAU - AU test.\r\nPlease go to General Ledger >> Period Management to setup periods.", AccountingPeriodCalculator.GetInvalidPeriodValidationError(new ZDateTime(2004, 05, 12), company));
			AssertEquals("There is no period set up for 200304.\r\nPlease go to General Ledger >> Period Management to setup periods.", AccountingPeriodCalculator.GetInvalidPeriodValidationErrorWithCompanyInfo("200304", GlbCompany.CurrentCompany));
		}

		public void TestGetPeriodManagementByOffset()
		{
			ZDateTime expectedDate1 = new ZDateTime(2005, 04, 10);
			ZDateTime expectedDate2 = new ZDateTime(2005, 08, 10);

			PeriodManagementTestHelper.SetupSinglePeriod(10, new ZDateTime(2005, 03, 10), expectedDate1);
			PeriodManagementTestHelper.SetupSinglePeriod(20, new ZDateTime(2005, 04, 10), new ZDateTime(2005, 05, 10));
			PeriodManagementTestHelper.SetupSinglePeriod(30, new ZDateTime(2005, 05, 10), new ZDateTime(2005, 06, 10));
			PeriodManagementTestHelper.SetupSinglePeriod(40, new ZDateTime(2005, 06, 10), new ZDateTime(2005, 07, 10));
			PeriodManagementTestHelper.SetupSinglePeriod(50, new ZDateTime(2005, 07, 10), new ZDateTime(2005, 08, 10));
			PeriodManagementTestHelper.SetupSinglePeriod(60, expectedDate2, new ZDateTime(2005, 09, 10));

			AccPeriodManagement period = Calculator.GetPeriodManagementFromDate(new ZDateTime(2005, 06, 20));
			AssertNull(Calculator.GetPeriodManagementByOffset(null, 10));
			AssertEquals(period.PK, Calculator.GetPeriodManagementByOffset(period, 0).PK);
			AssertEquals(expectedDate1, Calculator.GetPeriodManagementByOffset(period, -3).AM_EndDate);
			AssertEquals(expectedDate2, Calculator.GetPeriodManagementByOffset(period, 2).AM_StartDate);
		}

		public void TestGetRangeOfPeriods()
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			ZDate expectedDate1 = new ZDate(2020, 02, 10);
			ZDate expectedDate2 = new ZDate(2020, 05, 29);

			PeriodManagementTestHelper.SetupSinglePeriod(10, new ZDateTime(2020, 02, 01), new ZDateTime(2020, 02, 29));
			PeriodManagementTestHelper.SetupSinglePeriod(20, new ZDateTime(2020, 03, 01), new ZDateTime(2020, 03, 31));
			PeriodManagementTestHelper.SetupSinglePeriod(30, new ZDateTime(2020, 04, 01), new ZDateTime(2020, 04, 30));
			PeriodManagementTestHelper.SetupSinglePeriod(40, new ZDateTime(2020, 05, 01), new ZDateTime(2020, 05, 31));

			var rangeOfPeriods = Calculator.GetRangeOfPeriods(expectedDate1, expectedDate2);

			CombineAssertions(() =>
			{
				AssertEquals(new ZDateTime(2020, 02, 01), rangeOfPeriods[0].AM_StartDate);
				AssertEquals(new ZDateTime(2020, 02, 29), rangeOfPeriods[0].AM_EndDate);
				AssertEquals(new ZDateTime(2020, 03, 01), rangeOfPeriods[1].AM_StartDate);
				AssertEquals(new ZDateTime(2020, 03, 31), rangeOfPeriods[1].AM_EndDate);
				AssertEquals(new ZDateTime(2020, 04, 01), rangeOfPeriods[2].AM_StartDate);
				AssertEquals(new ZDateTime(2020, 04, 30), rangeOfPeriods[2].AM_EndDate);
				AssertEquals(new ZDateTime(2020, 05, 01), rangeOfPeriods[3].AM_StartDate);
				AssertEquals(new ZDateTime(2020, 05, 31), rangeOfPeriods[3].AM_EndDate);
			});

			var outsideRangeStartDate = new ZDate(2017, 02, 10);
			var outsideRangeEndDate = new ZDate(2026, 05, 29);

			rangeOfPeriods = Calculator.GetRangeOfPeriods(outsideRangeStartDate, expectedDate2);
			AssertEquals("Should return nothing for outside range date", 0, rangeOfPeriods.Length);

			rangeOfPeriods = Calculator.GetRangeOfPeriods(expectedDate1, outsideRangeEndDate);
			AssertEquals("Should return nothing for outside range date", 0, rangeOfPeriods.Length);

			rangeOfPeriods = Calculator.GetRangeOfPeriods(outsideRangeStartDate, outsideRangeEndDate);
			AssertEquals("Should return nothing for outside range date", 0, rangeOfPeriods.Length);
		}

		[TestDate(2012, 03, 01)]
		public void TestGetFirstOpenPeriod()
		{
			AccPeriodManagement period = Calculator.GetFirstOpenPeriod(GlbCompany.CurrentCompany.PK);
			AssertEquals("Period Start Date", new ZDateTime(2011, 12, 22, 0, 0, 0), period.AM_StartDate);
			AssertEquals("Period End Date", new ZDateTime(2012, 01, 11, 23, 59, 0), period.AM_EndDate);
			AssertEquals("Period", 999995, period.AM_Period);

			period = Calculator.GetFirstOpenPeriod(ZGuid.NewZGuid());
			AssertNull(period);
		}

		public void TestGetFirstPeriodManagementFromDate()
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();

			PeriodManagementTestHelper.SetupSinglePeriod(202210, new ZDateTime(2020, 02, 01), new ZDateTime(2020, 02, 29));
			PeriodManagementTestHelper.SetupSinglePeriod(202211, new ZDateTime(2020, 03, 01), new ZDateTime(2020, 03, 31));
			PeriodManagementTestHelper.SetupSinglePeriod(202311, new ZDateTime(2020, 03, 01), new ZDateTime(2020, 05, 31));

			var period1 = Calculator.GetFirstPeriodManagementFromDate(new ZDateTime(2020, 02, 01), companyPK);
			var period2 = Calculator.GetFirstPeriodManagementFromDate(new ZDateTime(2020, 03, 31), companyPK);
			var period3 = Calculator.GetFirstPeriodManagementFromDate(new ZDateTime(2020, 05, 31), companyPK);

			AssertEquals("AM_StartDate", new ZDateTime(2020, 02, 01), period1.AM_StartDate);
			AssertEquals("AM_StartDate", new ZDateTime(2020, 03, 01), period2.AM_StartDate);
			AssertEquals("AM_StartDate", new ZDateTime(2020, 03, 01), period3.AM_StartDate);
			AssertNull("Can't get periodManagement when date does not belong to any period", Calculator.GetFirstPeriodManagementFromDate(new ZDateTime(2020, 01, 03), companyPK));
			AssertNull("Can't get periodManagement when date does not belong to any period", Calculator.GetFirstPeriodManagementFromDate(new ZDateTime(2020, 06, 03), companyPK));
		}

		public void TestGetPreviousPeriodManagementFromDate()
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();

			PeriodManagementTestHelper.SetupSinglePeriod(202210, new ZDateTime(2020, 02, 01), new ZDateTime(2020, 02, 29));
			PeriodManagementTestHelper.SetupSinglePeriod(202310, new ZDateTime(2020, 03, 01), new ZDateTime(2020, 03, 31));

			var period1 = Calculator.GetPreviousPeriodManagementFromDate(new ZDateTime(2020, 02, 01), companyPK);
			var period2 = Calculator.GetPreviousPeriodManagementFromDate(new ZDateTime(2020, 03, 01), companyPK);
			var period3 = Calculator.GetPreviousPeriodManagementFromDate(new ZDateTime(2020, 03, 31), companyPK);
			var period4 = Calculator.GetPreviousPeriodManagementFromDate(ZDateTime.Invalid, companyPK);
			var period5 = Calculator.GetPreviousPeriodManagementFromDate(new ZDateTime(1899, 03, 31), companyPK);

			AssertNull("Can't get periodManagement when does not set up previous period management", period1);
			AssertEquals("AM_EndDate", new ZDateTime(2020, 02, 29), period2.AM_EndDate);
			AssertEquals("AM_EndDate", new ZDateTime(2020, 03, 31), period3.AM_EndDate);
			AssertNull("Can't get periodManagement when proposed value is invalid", period4);
			AssertNull("Can't get periodManagement when proposed value is less than MinSmallDateTimeValue", period5);
		}

		public void TestGetFirstPeriodFromYear()
		{
			PeriodManagementTestHelper.PostPeriodsForEntireYear(2023);
			var periodManagement = Calculator.GetFirstPeriodFromYear(2023);
			AssertEquals(new ZDateTime(2022, 7, 1, 0, 0, 0), periodManagement.AM_StartDate);
			AssertEquals(new ZDateTime(2022, 7, 31, 23, 59, 0), periodManagement.AM_EndDate);
			AssertEquals(202301, periodManagement.AM_Period);
			AssertEquals((ZShort)2023, periodManagement.AM_Year);
		}

		#region Implementation

		protected AccountingPeriodCalculator Calculator;
		protected AccountingPeriodTestHelper PeriodManagementTestHelper;
		protected AccPeriodManagement PreviousGLClosedPeriod;
		protected AccPeriodManagement PreviousSubLedgerClosedPeriod;
		protected AccPeriodManagement CurrentPeriod;
		protected AccPeriodManagement FuturePeriod;

		protected GlbCompany NonCurrentCompany;

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			Calculator = new AccountingPeriodCalculator(Factory);
			SetupPeriods();
			SetupNonCurrentCompany();
		}

		protected void SetupPeriods()
		{
			PeriodManagementTestHelper = new AccountingPeriodTestHelper();
			PeriodManagementTestHelper.SetupPeriods();
			PreviousGLClosedPeriod = PeriodManagementTestHelper.PreviousGLClosedPeriod;
			PreviousSubLedgerClosedPeriod = PeriodManagementTestHelper.PreviousSubLedgerClosedPeriod;
			CurrentPeriod = PeriodManagementTestHelper.CurrentPeriod;
			FuturePeriod = PeriodManagementTestHelper.FuturePeriod;
		}

		protected void SetupNonCurrentCompany()
		{
			ZQuery query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			NonCurrentCompany = (GlbCompany)Factory.Load(typeof(GlbCompany), query)[0];
		}
		#endregion

	}
}
