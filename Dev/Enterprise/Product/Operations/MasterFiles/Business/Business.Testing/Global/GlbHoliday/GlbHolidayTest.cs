using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbHoliday))]
	sealed class GlbHolidayTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			var testHolidays = Factory.Load<GlbHoliday>(new ZQuery(GlbHolidaySchema.GH_IsValid, false));
			if (testHolidays.Any())
			{
				testHolidays.DeleteAll();
				Factory.Save();
			}

			base.TestBizObjectField(info);
		}

		public void TestHumanReadableName()
		{
			var holiday = GlbBranch.CurrentBranch.GlbHolidays.AddNew();
			holiday.GH_HolidayName = "My Bday";
			holiday.GH_Date = new ZDateTime(1994, 12, 23);
			holiday.GH_Recurring = true;
			holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;

			AssertNotNull(holiday.HumanReadableName);
		}

		#region GH_HolidayName

		public void TestGH_HolidayNameMultilingual()
		{
			var holiday = Factory.NewWithValidTestData<GlbHoliday>();

			using (var mockData = Res.UseMockData())
			{
				var key = holiday.GH_HolidayNameInfo.CustomizableDataResourceStrings.Source.GetKey(null, "Christmas Day");
				mockData.Put(key, new ResourceStringData(key, "圣诞节"));

				holiday.GH_HolidayName = "Christmas Day";
				AssertEquals("圣诞节", holiday.GH_HolidayNameMultilingual);
			}
		}

		#endregion

		#region Code Lists

		public void TestRecurrTypes()
		{
			Assert("RecurrTypes.Count > 0", Holiday.RecurrTypes.Count > 0);
		}

		public void TestMonths()
		{
			Assert("Months.Count > 0", Holiday.Months.Count > 0);
		}

		public void TestWeekDays()
		{
			Assert("WeekDays.Count > 0", Holiday.WeekDays.Count > 0);
		}

		public void TestCodesMatchCargoWiseDefintions_Months()
		{
			var pairListMonths = Holiday.Months.ToArray();
			var calendarCodesMonthsList = typeof(CalendarCodes.Months).GetAllPublicConstantValues();

			foreach (var month in pairListMonths)
			{
				AssertCollectionContains("All WeekDay codes in Dev should exist in CargoWise.Definitions", month.Code, calendarCodesMonthsList);
			}
		}

		public void TestCodesMatchCargoWiseDefintions_Days()
		{
			var pairListDays = Holiday.WeekDays.ToArray();
			var calendarCodesDaysList = typeof(CalendarCodes.Days).GetAllPublicConstantValues();

			foreach (var day in pairListDays)
			{
				AssertCollectionContains("All WeekDay codes in Dev should exist in CargoWise.Definitions", day.Code, calendarCodesDaysList);
			}
		}

		public void TestCodesMatchCargoWiseDefintions_RecurType()
		{
			var pairListRecurType = Holiday.RecurrTypes.ToArray();
			var holidayRecurTypesCodes = typeof(HolidayRecurTypeCodes).GetAllPublicConstantValues();

			foreach (var type in pairListRecurType)
			{
				AssertCollectionContains("All RecurType codes in Dev should exist in CargoWise.Definitions", type.Code, holidayRecurTypesCodes);
			}
		}

		public void TestCodesInCargoWiseDefintionsMatchDev_Months()
		{
			var pairListRecurTypes = Holiday.Months.ToArray();
			var calendarCodesMonthsList = typeof(CalendarCodes.Months).GetAllPublicConstantValues();

			foreach (var month in calendarCodesMonthsList)
			{
				Assert("All Month codes in CargoWise.Definitions should exist in Dev", pairListRecurTypes.Any(t => t.Code == month));
			}
		}

		public void TestCodesInCargoWiseDefintionsMatchDev_Days()
		{
			var pairListRecurTypes = Holiday.WeekDays.ToArray();
			var calendarCodesDaysList = typeof(CalendarCodes.Days).GetAllPublicConstantValues();

			foreach (var day in calendarCodesDaysList)
			{
				Assert("All Day codes in CargoWise.Definitions should exist in Dev", pairListRecurTypes.Any(t => t.Code == day));
			}
		}

		public void TestCodesInCargoWiseDefintionsMatchDev_RecurType()
		{
			var pairListRecurTypes = Holiday.RecurrTypes.ToArray();
			var holidayRecurTypesCodes = typeof(HolidayRecurTypeCodes).GetAllPublicConstantValues();

			foreach (var type in holidayRecurTypesCodes)
			{
				Assert("All Recur Type codes in CargoWise.Definitions should exist in Dev", pairListRecurTypes.Any(t => t.Code == type));
			}
		}

		public void TestWeeklyValueNotInGlbBranchRecurType()
		{
			var holiday = Factory.NewWithValidTestData<GlbHoliday>();
			Assert("Weekly should exist in RecurrType", holiday.RecurrTypes.GetAllCodes().Contains(HolidayRecurTypeCodes.Weekly));
			holiday.GH_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			Assert("Weekly should not exist in RecurrType in GlbBranch table", !holiday.RecurrTypes.GetAllCodes().Contains(HolidayRecurTypeCodes.Weekly));
		}

		#endregion

		#region Properties

		#region GH_RecurrType

		public void TestGH_RecurrType()
		{
			Assert("Holiday Date should not be readonly", !Holiday.GH_DateInfo.ReadOnly);
			Assert("Recurr Month should be readonly", Holiday.GH_RecurrMonthInfo.ReadOnly);
			Assert("Recurr Day should be readonly", Holiday.GH_RecurrDayInfo.ReadOnly);

			Holiday.GH_Date = ZDateTime.Now;
			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.First;
			Assert("Holiday Date should be readonly", Holiday.GH_DateInfo.ReadOnly);
			AssertEquals("Holiday Date should be empty", ZDateTime.Empty, Holiday.GH_Date);
			Assert("Recurr Month should not be readonly", !Holiday.GH_RecurrMonthInfo.ReadOnly);
			Assert("Recurr Day should not be readonly", !Holiday.GH_RecurrDayInfo.ReadOnly);

			Holiday.GH_RecurrMonth = Holiday.Months[0].Code;
			Holiday.GH_RecurrDay = Holiday.WeekDays[0].Code;
			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			Assert("Holiday Date should not be readonly", !Holiday.GH_DateInfo.ReadOnly);
			Assert("Recurr Month should be readonly", Holiday.GH_RecurrMonthInfo.ReadOnly);
			AssertEquals("Recurr Month should be empty", "", Holiday.GH_RecurrMonth);
			Assert("Recurr Day should be readonly", Holiday.GH_RecurrDayInfo.ReadOnly);
			AssertEquals("Recurr Day should be empty", "", Holiday.GH_RecurrDay);
		}

		public void TestGH_RecurrType_Weekly()
		{
			Assert("Holiday Date should not be readonly", !Holiday.GH_DateInfo.ReadOnly);
			Assert("Recurr Month should be readonly", Holiday.GH_RecurrMonthInfo.ReadOnly);
			Assert("Recurr Day should be readonly", Holiday.GH_RecurrDayInfo.ReadOnly);

			Holiday.GH_RecurrMonth = Holiday.Months[0].Code;
			Holiday.GH_RecurrDay = Holiday.WeekDays[0].Code;
			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.First;
			Assert("Holiday Date should be readonly", Holiday.GH_DateInfo.ReadOnly);
			Assert("Recurr Month should not be readonly", !Holiday.GH_RecurrMonthInfo.ReadOnly);
			AssertEquals("Recurr Month should be JAN", "JAN", Holiday.GH_RecurrMonth);
			Assert("Recurr Day should not be readonly", !Holiday.GH_RecurrDayInfo.ReadOnly);
			AssertEquals("Recurr Day should be SUN", "SUN", Holiday.GH_RecurrDay);

			Holiday.GH_Date = ZDateTime.Now;
			Holiday.GH_RecurrMonth = Holiday.Months[0].Code;
			Holiday.GH_RecurrDay = Holiday.WeekDays[0].Code;
			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			Assert("Holiday Date should not be readonly", Holiday.GH_DateInfo.ReadOnly);
			AssertEquals("Holiday Date should be empty", ZDateTime.Empty, Holiday.GH_Date);
			Assert("Recurr Month should be readonly", Holiday.GH_RecurrMonthInfo.ReadOnly);
			AssertEquals("Recurr Month should be empty", "", Holiday.GH_RecurrMonth);
			Assert("Recurr Day should not be readonly", !Holiday.GH_RecurrDayInfo.ReadOnly);
			AssertEquals("Recurr Day should be SUN", "SUN", Holiday.GH_RecurrDay);
		}

		public void TestGH_RecurrType_EasterChangesHolidayName()
		{
			AssertEquals(false, Holiday.GH_HolidayNameInfo.ReadOnly);

			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.GoodFriday;
			AssertEquals(GlbHolidayRecurTypeCodeList.Descriptions.GoodFriday, Holiday.GH_HolidayName);
			AssertEquals(true, Holiday.GH_HolidayNameInfo.ReadOnly);

			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.EasterMonday;
			AssertEquals(GlbHolidayRecurTypeCodeList.Descriptions.EasterMonday, Holiday.GH_HolidayName);
			AssertEquals(true, Holiday.GH_HolidayNameInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region Read-Only Fields

		public void TestRecurrTypeDate()
		{
			bool expectRecurringReadOnly = true;
			bool expectDateReadOnly = true;
			bool expectRecurrMonthReadOnly = true;
			bool expectRecurrDayReadOnly = true;

			TestPropertiesReadOnly(GlbHolidayRecurTypeCodeList.Codes.Date, !expectRecurringReadOnly, !expectDateReadOnly, expectRecurrMonthReadOnly, expectRecurrDayReadOnly);
			TestPropertiesReadOnly(GlbHolidayRecurTypeCodeList.Codes.First, expectRecurringReadOnly, expectDateReadOnly, !expectRecurrMonthReadOnly, !expectRecurrDayReadOnly);
			TestPropertiesReadOnly(GlbHolidayRecurTypeCodeList.Codes.Second, expectRecurringReadOnly, expectDateReadOnly, !expectRecurrMonthReadOnly, !expectRecurrDayReadOnly);
			TestPropertiesReadOnly(GlbHolidayRecurTypeCodeList.Codes.Third, expectRecurringReadOnly, expectDateReadOnly, !expectRecurrMonthReadOnly, !expectRecurrDayReadOnly);
			TestPropertiesReadOnly(GlbHolidayRecurTypeCodeList.Codes.Fourth, expectRecurringReadOnly, expectDateReadOnly, !expectRecurrMonthReadOnly, !expectRecurrDayReadOnly);
			TestPropertiesReadOnly(GlbHolidayRecurTypeCodeList.Codes.Last, expectRecurringReadOnly, expectDateReadOnly, !expectRecurrMonthReadOnly, !expectRecurrDayReadOnly);
			TestPropertiesReadOnly(GlbHolidayRecurTypeCodeList.Codes.GoodFriday, expectRecurringReadOnly, expectDateReadOnly, expectRecurrMonthReadOnly, expectRecurrDayReadOnly);
			TestPropertiesReadOnly(GlbHolidayRecurTypeCodeList.Codes.EasterMonday, expectRecurringReadOnly, expectDateReadOnly, expectRecurrMonthReadOnly, expectRecurrDayReadOnly);
		}

		void TestPropertiesReadOnly(ZString recurrType, ZBool expectRecurringReadOnly, ZBool expectDateReadOnly, ZBool expectRecurrMonthReadOnly, ZBool expectRecurrDayReadOnly)
		{
			Holiday.GH_RecurrType = recurrType;
			AssertEquals("GH_RecurringInfo.ReadOnly", expectRecurringReadOnly, Holiday.GH_RecurringInfo.ReadOnly);
			AssertEquals("GH_DateInfo.ReadOnly", expectDateReadOnly, Holiday.GH_DateInfo.ReadOnly);
			AssertEquals("GH_RecurrMonthInfoInfo.ReadOnly", expectRecurrMonthReadOnly, Holiday.GH_RecurrMonthInfo.ReadOnly);
			AssertEquals("GH_RecurrDayInfo.ReadOnly", expectRecurrDayReadOnly, Holiday.GH_RecurrDayInfo.ReadOnly);

			if (expectRecurrMonthReadOnly)
			{
				AssertEquals("GH_RecurrMonth empty when read-only", "", Holiday.GH_RecurrMonth);
			}
			if (expectRecurrDayReadOnly)
			{
				AssertEquals("GH_RecurrDay empty when read-only", "", Holiday.GH_RecurrDay);
			}
		}

		#endregion

		#region MatchesDate

		public void TestMatchesDate_NonRecurringDate()
		{
			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			Holiday.GH_Recurring = false;
			Holiday.GH_Date = new ZDateTime(2005, 1, 2, 5, 5, 5);

			AssertEquals(false, Holiday.MatchesDate(new ZDate(2005, 1, 1)));
			AssertEquals(false, Holiday.MatchesDate(new ZDate(2005, 1, 3)));

			AssertEquals(false, Holiday.MatchesDate(new ZDate(2004, 1, 2)));
			AssertEquals(false, Holiday.MatchesDate(new ZDate(2006, 1, 2)));

			AssertEquals(true, Holiday.MatchesDate(new ZDate(2005, 1, 2)));
		}

		public void TestMatchesDate_RecurringDate()
		{
			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			Holiday.GH_Recurring = true;
			Holiday.GH_Date = new ZDateTime(2005, 1, 2, 7, 7, 7);

			AssertEquals(false, Holiday.MatchesDate(new ZDate(2005, 1, 1)));
			AssertEquals(false, Holiday.MatchesDate(new ZDate(2005, 1, 3)));

			AssertEquals(true, Holiday.MatchesDate(new ZDate(2004, 1, 2)));
			AssertEquals(true, Holiday.MatchesDate(new ZDate(2006, 1, 2)));

			AssertEquals(true, Holiday.MatchesDate(new ZDate(2005, 1, 2)));
		}

		public void TestMatchesDate_GoodFriday()
		{
			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.GoodFriday;

			AssertEquals(false, Holiday.MatchesDate(new ZDate(2006, 4, 14 - 1)));
			AssertEquals(false, Holiday.MatchesDate(new ZDate(2007, 4, 6 - 1)));
			AssertEquals(false, Holiday.MatchesDate(new ZDate(2008, 3, 21 - 1)));

			AssertEquals(true, Holiday.MatchesDate(new ZDate(2006, 4, 14)));
			AssertEquals(true, Holiday.MatchesDate(new ZDate(2007, 4, 6)));
			AssertEquals(true, Holiday.MatchesDate(new ZDate(2008, 3, 21)));
		}

		public void TestMatchesDate_EasterMonday()
		{
			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.EasterMonday;

			AssertEquals(false, Holiday.MatchesDate(new ZDate(2006, 4, 17 - 1)));
			AssertEquals(false, Holiday.MatchesDate(new ZDate(2007, 4, 9 - 1)));
			AssertEquals(false, Holiday.MatchesDate(new ZDate(2008, 3, 24 - 1)));

			AssertEquals(true, Holiday.MatchesDate(new ZDate(2006, 4, 17)));
			AssertEquals(true, Holiday.MatchesDate(new ZDate(2007, 4, 9)));
			AssertEquals(true, Holiday.MatchesDate(new ZDate(2008, 3, 24)));
		}

		public void TestMatchesDate_RecurringLastWeekDay()
		{
			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Last;
			Holiday.GH_RecurrMonth = CalendarCodes.Months.March;
			Holiday.GH_RecurrDay = DayOfWeekCodeList.Codes.Thursday;

			AssertEquals(false, Holiday.MatchesDate(new ZDate(2006, 3, 23)));
			AssertEquals(true, Holiday.MatchesDate(new ZDate(2006, 3, 30)));
			AssertEquals(false, Holiday.MatchesDate(new ZDate(2006, 3, 31)));
		}

		public void TestMatchesDate_RecurringWeekDayDifferentMonth()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			GlbHoliday hol1 = branch.GlbHolidays.AddNew();
			hol1.GH_Recurring = true;
			hol1.GH_RecurrDay = CalendarCodes.Days.Monday;
			hol1.GH_RecurrMonth = CalendarCodes.Months.March;
			hol1.GH_RecurrType = HolidayRecurTypeCodes.Second;

			Assert(!hol1.MatchesDate(new ZDate(2006, 4, 10)));

			hol1.GH_RecurrMonth = CalendarCodes.Months.April;
			Assert(hol1.MatchesDate(new ZDate(2006, 4, 10)));
		}

		public void TestMatchesDate_LastWeekDayDifferentMonth()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			GlbHoliday hol1 = branch.GlbHolidays.AddNew();
			hol1.GH_Recurring = true;
			hol1.GH_RecurrDay = CalendarCodes.Days.Friday;
			hol1.GH_RecurrMonth = CalendarCodes.Months.March;
			hol1.GH_RecurrType = HolidayRecurTypeCodes.Last;

			Assert(!hol1.MatchesDate(new ZDate(2006, 4, 28)));

			hol1.GH_RecurrMonth = CalendarCodes.Months.April;
			Assert(hol1.MatchesDate(new ZDate(2006, 4, 28)));
		}

		public void TestMatchesDate_RecurringWeekDay()
		{
			TestMatchesDate_RecurringWeekDay(GlbHolidayRecurTypeCodeList.Codes.First, 6, 7, 1, 2, 3, 4, 5);
			TestMatchesDate_RecurringWeekDay(GlbHolidayRecurTypeCodeList.Codes.Second, 13, 14, 8, 9, 10, 11, 12);
			TestMatchesDate_RecurringWeekDay(GlbHolidayRecurTypeCodeList.Codes.Third, 20, 21, 15, 16, 17, 18, 19);
			TestMatchesDate_RecurringWeekDay(GlbHolidayRecurTypeCodeList.Codes.Fourth, 27, 28, 22, 23, 24, 25, 26);
			TestMatchesDate_RecurringWeekDay(GlbHolidayRecurTypeCodeList.Codes.Last, 27, 28, 22, 23, 24, 25, 26);
		}

		void TestMatchesDate_RecurringWeekDay(ZString recurrType, params int[] weekDaysOfFeburary)
		{
			TestMatchesDate_RecurringWeekDay(recurrType, DayOfWeekCodeList.Codes.Monday, new ZDate(2006, 2, weekDaysOfFeburary[0]));
			TestMatchesDate_RecurringWeekDay(recurrType, DayOfWeekCodeList.Codes.Tuesday, new ZDate(2006, 2, weekDaysOfFeburary[1]));
			TestMatchesDate_RecurringWeekDay(recurrType, DayOfWeekCodeList.Codes.Wednesday, new ZDate(2006, 2, weekDaysOfFeburary[2]));
			TestMatchesDate_RecurringWeekDay(recurrType, DayOfWeekCodeList.Codes.Thursday, new ZDate(2006, 2, weekDaysOfFeburary[3]));
			TestMatchesDate_RecurringWeekDay(recurrType, DayOfWeekCodeList.Codes.Friday, new ZDate(2006, 2, weekDaysOfFeburary[4]));
			TestMatchesDate_RecurringWeekDay(recurrType, DayOfWeekCodeList.Codes.Saturday, new ZDate(2006, 2, weekDaysOfFeburary[5]));
			TestMatchesDate_RecurringWeekDay(recurrType, DayOfWeekCodeList.Codes.Sunday, new ZDate(2006, 2, weekDaysOfFeburary[6]));
		}

		void TestMatchesDate_RecurringWeekDay(ZString recurrType, ZString weekDay, ZDate dayOfFebruary)
		{
			Holiday.GH_RecurrType = recurrType;
			Holiday.GH_RecurrMonth = CalendarCodes.Months.February;
			Holiday.GH_RecurrDay = weekDay;

			AssertEquals(false, Holiday.MatchesDate(dayOfFebruary.AddDays(-1)));
			AssertEquals(false, Holiday.MatchesDate(dayOfFebruary.AddDays(1)));
			AssertEquals(false, Holiday.MatchesDate(dayOfFebruary.AddMonths(-1)));
			AssertEquals(false, Holiday.MatchesDate(dayOfFebruary.AddMonths(1).AddDays(1)));

			AssertEquals(true, Holiday.MatchesDate(dayOfFebruary));
		}

		#endregion

		#region Implementation

		GlbHoliday Holiday
		{
			get
			{
				if (fHoliday == null)
				{
					fHoliday = Factory.New<GlbHoliday>();
				}
				return fHoliday;
			}
		}
		
		GlbHoliday fHoliday;

		protected override BusinessObject GetNewBusinessObject() => GetHolidayWithBranch();
		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory) => GetHolidayWithBranch();
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetHolidayWithBranch();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetHolidayWithBranch();

		GlbHoliday GetHolidayWithBranch()
		{
			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery());
			var holiday = Factory.New<GlbHoliday>();
			((ILightValidationInternals)holiday).IsValid = true;
			holiday.GH_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			holiday.GH_ParentID = branch.PK;

			return holiday;
		}

		#endregion

		public void TestPreventDeleteIsFalse()
		{
			Assert(!PreventDeleteAttribute.IsTrue(typeof(GlbHoliday)));
		}

		#region CountryStates

		public void TestOverridesHumanReadableNameForCountryAndStates()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "ZZ";
			var state1 = Factory.NewWithValidTestData<RefCountryStates>();
			state1.RW_RN_NKCountryCode = country.RN_Code;
			state1.RW_Code = "ZZ";

			var holiday = Factory.NewWithValidTestData<GlbHoliday>();
			holiday.GH_HolidayName = "Christmas";
			holiday.GH_ParentID = state1.PK;
			holiday.GH_ParentTableCode = RefCountryStatesSchema.Constants.Prefix;

			Assert(holiday.HumanReadableName == "Christmas - ZZ");

			var holiday2 = Factory.NewWithValidTestData<GlbHoliday>();
			holiday2.GH_HolidayName = "New Years";
			holiday2.GH_ParentID = country.PK;
			holiday2.GH_ParentTableCode = RefCountrySchema.Constants.Prefix;

			Assert(holiday2.HumanReadableName == "New Years - ZZ");
		}

		public void TestLoadCountryStateApplicability()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var state1 = Factory.NewWithValidTestData<RefCountryStates>();
			state1.RW_RN_NKCountryCode = country.RN_Code;
			var state2 = Factory.NewWithValidTestData<RefCountryStates>();
			state2.RW_RN_NKCountryCode = country.RN_Code;
			var state3 = Factory.NewWithValidTestData<RefCountryStates>();
			state3.RW_RN_NKCountryCode = country.RN_Code;

			var holiday = Factory.NewWithValidTestData<GlbHoliday>();
			holiday.GH_HolidayName = "Christmas";
			holiday.GH_ParentID = state1.PK;
			holiday.GH_ParentTableCode = RefCountryStatesSchema.Constants.Prefix;

			AssertEquals($"{country.RN_Code}: {state1.RW_Code}", holiday.CountryStatesApplicability);

			var holiday2 = Factory.NewWithValidTestData<GlbHoliday>();
			holiday2.GH_HolidayName = "Christmas";
			holiday2.GH_ParentID = state2.PK;
			holiday2.GH_ParentTableCode = RefCountryStatesSchema.Constants.Prefix;

			AssertEquals($"{country.RN_Code}: {state1.RW_Code}, {state2.RW_Code}", holiday.CountryStatesApplicability);
			AssertEquals($"{country.RN_Code}: {state1.RW_Code}, {state2.RW_Code}", holiday2.CountryStatesApplicability);

			var holiday3 = Factory.NewWithValidTestData<GlbHoliday>();
			holiday3.GH_HolidayName = "Christmas";
			holiday3.GH_ParentID = state3.PK;
			holiday3.GH_ParentTableCode = RefCountryStatesSchema.Constants.Prefix;

			AssertEquals($"{country.RN_Code}: whole country", holiday.CountryStatesApplicability);
			AssertEquals($"{country.RN_Code}: whole country", holiday2.CountryStatesApplicability);
			AssertEquals($"{country.RN_Code}: whole country", holiday3.CountryStatesApplicability);
		}
		#endregion

		#region Country

		public void TestLoadCountryApplicability()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var holiday = Factory.NewWithValidTestData<GlbHoliday>();
			holiday.GH_ParentID = country.PK;
			holiday.GH_ParentTableCode = RefCountrySchema.Constants.Prefix;

			AssertEquals($"{country.RN_Code}: whole country", holiday.CountryStatesApplicability);
		}
		#endregion
	}
}
