using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	public sealed class PenaltyExclusionsCalculatorTest : TestCaseWithFactory
	{
		public void TestFreeDaysToSkip_Weekdays()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 4;

			var exclusion = SetupExclusion(thursday: true, friday: true);
			containerPenalty.CPY_CEX_FreeDayExclusion = exclusion.PK;

			var freeDaysToSkip = PenaltyExclusionsCalculator.CalculateTotalFreeDaysToSkip(containerPenalty);
			AssertEquals(freeDaysToSkip, (ZByte)2);
		}

		public void TestFreeDaysToSkip_Weekdays_Loop()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 7;

			var exclusion = SetupExclusion(thursday: true, friday: true);
			containerPenalty.CPY_CEX_FreeDayExclusion = exclusion.PK;

			var freeDaysToSkip = PenaltyExclusionsCalculator.CalculateTotalFreeDaysToSkip(containerPenalty);

			// Free Days: Wed, Sat, Sun, Mon, Tues, Wed, Sat - (Thu / Fri skipped twice)
			AssertEquals(freeDaysToSkip, (ZByte)4);
		}

		public void TestFreeDaysToSkip_Holidays()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 7;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(holiday: true);
			containerPenalty.CPY_CEX_FreeDayExclusion = exclusion.PK;

			var location = Factory.New<RefUNLOCO>();
			location.RL_Code = "AAAAA";

			var state = Factory.New<RefCountryStates>();
			location.RL_RW = state.PK;

			var holiday = state.Holidays.AddNew();
			holiday.GH_HolidayName = "New Years";
			holiday.GH_ParentID = state.PK;
			holiday.GH_ParentTableCode = "RW";
			holiday.GH_IsWorkingDay = false;
			holiday.GH_Date = new ZDateTime(2025, 1, 1);
			holiday.GH_Recurring = true;
			holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;

			var freeDaysToSkip = PenaltyExclusionsCalculator.CalculateTotalFreeDaysToSkip(containerPenalty);

			// Excludes New Years
			AssertEquals(freeDaysToSkip, (ZByte)1);
		}

		public void TestFreeDaysToSkip_LocationWeekends()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 7;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(weekend: true);
			containerPenalty.CPY_CEX_FreeDayExclusion = exclusion.PK;

			var location = Factory.New<RefUNLOCO>();
			location.RL_Code = "AAAAA";

			var state = Factory.New<RefCountryStates>();
			location.RL_RW = state.PK;

			SetupWeekendsForState(state, thursday: true, friday: true);

			var freeDaysToSkip = PenaltyExclusionsCalculator.CalculateTotalFreeDaysToSkip(containerPenalty);

			// Free Days: Wed, Sat, Sun, Mon, Tues, Wed, Sat - (Thu / Fri skipped twice)
			AssertEquals(freeDaysToSkip, (ZByte)4);
		}

		public void TestFreeDaysToSkip_CountryWeekends()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 7;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(weekend: true);
			containerPenalty.CPY_CEX_FreeDayExclusion = exclusion.PK;

			var location = Factory.New<RefUNLOCO>();
			location.RL_Code = "AAAAA";
			location.RL_RN_NKCountryCode = "AA";

			var state = Factory.New<RefCountryStates>();
			location.RL_RW = state.PK;

			var country = Factory.New<RefCountry>();
			country.RN_Code = "AA";

			state.RW_RN_NKCountryCode = "AA";
			SetupWeekendsForCountry(country, thursday: true, friday: true);

			var freeDaysToSkip = PenaltyExclusionsCalculator.CalculateTotalFreeDaysToSkip(containerPenalty);

			// Free Days: Wed, Sat, Sun, Mon, Tues, Wed, Sat - (Thu / Fri skipped twice)
			AssertEquals(freeDaysToSkip, (ZByte)4);
		}

		public void TestFreeDaysToSkip_StateWeekendOverrideBlank()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 7;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(weekend: true);
			containerPenalty.CPY_CEX_FreeDayExclusion = exclusion.PK;

			var location = Factory.New<RefUNLOCO>();
			location.RL_Code = "AAAAA";
			location.RL_RN_NKCountryCode = "AA";

			var state = Factory.New<RefCountryStates>();
			location.RL_RW = state.PK;

			var country = Factory.New<RefCountry>();
			country.RN_Code = "AA";

			state.RW_RN_NKCountryCode = "AA";
			SetupWeekendsForCountry(country, thursday: true, friday: true);
			SetupWeekendsForState(state);

			var freeDaysToSkip = PenaltyExclusionsCalculator.CalculateTotalFreeDaysToSkip(containerPenalty);

			AssertEquals(freeDaysToSkip, (ZByte)0);
		}

		public void TestFreeDaysToSkip_All()
		{
			var containerPenalty = Factory.NewWithValidTestData<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 7;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(monday: true, weekend: true, holiday: true);
			containerPenalty.CPY_CEX_FreeDayExclusion = exclusion.PK;

			var location = Factory.NewWithValidTestData<RefUNLOCO>();
			location.RL_Code = "AAAAA";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			location.RL_RW = state.PK;

			SetupWeekendsForState(state, thursday: true, friday: true);

			var holiday = state.Holidays.AddNew();
			holiday.GH_HolidayName = "Some Other Day";
			holiday.GH_ParentID = state.PK;
			holiday.GH_ParentTableCode = "RW";
			holiday.GH_IsWorkingDay = false;
			holiday.GH_Date = new ZDateTime(2025, 1, 11);
			holiday.GH_Recurring = true;
			holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;

			Factory.Save();

			var freeDaysToSkip = PenaltyExclusionsCalculator.CalculateTotalFreeDaysToSkip(containerPenalty);

			// 1/1: Wed
			// 1/2: Thu - Skip
			// 1/3: Fri - Skip
			// 1/4: Sat
			// 1/5: Sun
			// 1/6: Mon - Skip
			// 1/7: Tue
			// 1/8: Wed
			// 1/9: Thu - Skip
			// 1/10:Fri - Skip
			// 1/11:Sat - Skip (holiday)
			// 1/12:Sun
			// 1/13:Mon - Skip
			// 1/14:Tue
			AssertEquals(freeDaysToSkip, (ZByte)7);
		}

		public void TestFreeDaysToSkip_WeekendNotSpecified()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 7;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(weekend: false);
			containerPenalty.CPY_CEX_FreeDayExclusion = exclusion.PK;

			var location = Factory.New<RefUNLOCO>();
			location.RL_Code = "AAAAA";

			var state = Factory.New<RefCountryStates>();
			location.RL_RW = state.PK;

			SetupWeekendsForState(state, thursday: true, friday: true);

			var freeDaysToSkip = PenaltyExclusionsCalculator.CalculateTotalFreeDaysToSkip(containerPenalty);

			AssertEquals(freeDaysToSkip, (ZByte)0);
		}

		public void TestFreeDaysToSkip_HolidaysNotSpecified()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 7;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(holiday: false);
			containerPenalty.CPY_CEX_FreeDayExclusion = exclusion.PK;

			var location = Factory.New<RefUNLOCO>();
			location.RL_Code = "AAAAA";

			var state = Factory.New<RefCountryStates>();
			location.RL_RW = state.PK;

			var holiday = state.Holidays.AddNew();
			holiday.GH_HolidayName = "New Years";
			holiday.GH_ParentID = state.PK;
			holiday.GH_ParentTableCode = "RW";
			holiday.GH_IsWorkingDay = false;
			holiday.GH_Date = new ZDateTime(2025, 1, 1);
			holiday.GH_Recurring = true;
			holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;

			var freeDaysToSkip = PenaltyExclusionsCalculator.CalculateTotalFreeDaysToSkip(containerPenalty);

			// Excludes New Years
			AssertEquals(freeDaysToSkip, (ZByte)0);
		}

		[TestDate(2025, 01, 06)]
		public void TestFreeDaysSkippedSoFar_Weekdays()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 10;

			var exclusion = SetupExclusion(thursday: true, friday: true);
			containerPenalty.CPY_CEX_FreeDayExclusion = exclusion.PK;

			var freeDaysSkippedSoFar = PenaltyExclusionsCalculator.CalculateNumberOfExcludedFreeDaysSoFar(containerPenalty);
			AssertEquals(freeDaysSkippedSoFar, (ZByte)2);
		}

		[TestDate(2025, 01, 06)]
		public void TestFreeDaysSkippedSoFar_LocationWeekends()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 7;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(weekend: true);
			containerPenalty.CPY_CEX_FreeDayExclusion = exclusion.PK;

			var location = Factory.New<RefUNLOCO>();
			location.RL_Code = "AAAAA";

			var state = Factory.New<RefCountryStates>();
			location.RL_RW = state.PK;

			SetupWeekendsForState(state, thursday: true, friday: true);

			var freeDaysSkippedSoFar = PenaltyExclusionsCalculator.CalculateNumberOfExcludedFreeDaysSoFar(containerPenalty);
			AssertEquals(freeDaysSkippedSoFar, (ZByte)2);
		}

		[TestDate(2025, 01, 06)]
		public void TestFreeDaysSkippedSoFar_Holiday()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 7;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(holiday: true);
			containerPenalty.CPY_CEX_FreeDayExclusion = exclusion.PK;

			var location = Factory.New<RefUNLOCO>();
			location.RL_Code = "AAAAA";

			var state = Factory.New<RefCountryStates>();
			location.RL_RW = state.PK;

			var holiday = state.Holidays.AddNew();
			holiday.GH_HolidayName = "Some Other Day";
			holiday.GH_ParentID = state.PK;
			holiday.GH_ParentTableCode = "RW";
			holiday.GH_IsWorkingDay = false;
			holiday.GH_Date = new ZDateTime(2025, 1, 05);
			holiday.GH_Recurring = true;
			holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;

			var freeDaysSkippedSoFar = PenaltyExclusionsCalculator.CalculateNumberOfExcludedFreeDaysSoFar(containerPenalty);
			AssertEquals(freeDaysSkippedSoFar, (ZByte)1);
		}

		[TestDate(2025, 01, 06)]
		public void TestFreeDaysSkippedSoFar_All()
		{
			var containerPenalty = Factory.NewWithValidTestData<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 7;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(monday: true, weekend: true, holiday: true);
			containerPenalty.CPY_CEX_FreeDayExclusion = exclusion.PK;

			var location = Factory.NewWithValidTestData<RefUNLOCO>();
			location.RL_Code = "AAAAA";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			location.RL_RW = state.PK;

			SetupWeekendsForState(state, thursday: true, friday: true);

			var holiday = state.Holidays.AddNew();
			holiday.GH_HolidayName = "Some Other Day";
			holiday.GH_ParentID = state.PK;
			holiday.GH_ParentTableCode = "RW";
			holiday.GH_IsWorkingDay = false;
			holiday.GH_Date = new ZDateTime(2025, 1, 4);
			holiday.GH_Recurring = true;
			holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;

			Factory.Save();

			var freeDaysSkippedSoFar = PenaltyExclusionsCalculator.CalculateNumberOfExcludedFreeDaysSoFar(containerPenalty);

			// 1/1: Wed
			// 1/2: Thu - Skip
			// 1/3: Fri - Skip
			// 1/4: Sat - Skip
			// 1/5: Sun
			// 1/6: Mon - Skip
			AssertEquals(freeDaysSkippedSoFar, (ZByte)4);
		}

		[TestDate(2025, 01, 10)]
		public void TestDurationDaysSkippedSoFar_Weekdays()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 4;

			var exclusion = SetupExclusion(thursday: true, friday: true);
			containerPenalty.CPY_CEX_DurationExclusion = exclusion.PK;

			AssertEquals(containerPenalty.LastFreeDay, new ZDateTime(2025, 1, 4));

			var durationDaysSkippedSoFar = PenaltyExclusionsCalculator.CalculateNumberOfExcludedDurationSoFar(containerPenalty);

			// 1/5: Sun
			// 1/6: Mon 
			// 1/7: Tue 
			// 1/8: Wed 
			// 1/9: Thu - Skip
			// 1/10:Fri - Skip
			AssertEquals(durationDaysSkippedSoFar, (ZByte)2);
		}

		[TestDate(2025, 01, 18)]
		public void TestDurationDaysSkippedSoFar_Weekdays_Loop()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 4;

			var exclusion = SetupExclusion(thursday: true, friday: true);
			containerPenalty.CPY_CEX_DurationExclusion = exclusion.PK;

			AssertEquals(containerPenalty.LastFreeDay, new ZDateTime(2025, 1, 4));

			var durationDaysSkippedSoFar = PenaltyExclusionsCalculator.CalculateNumberOfExcludedDurationSoFar(containerPenalty);

			// 1/5: Sun
			// 1/6: Mon 
			// 1/7: Tue 
			// 1/8: Wed 
			// 1/9: Thu - Skip
			// 1/10:Fri - Skip
			// 1/11:Sat
			// 1/12:Sun
			// 1/13:Mon
			// 1/14:Tue
			// 1/15:Wed
			// 1/16:Thu - Skip
			// 1/17:Fri - Skip
			// 1/18:Sat
			AssertEquals(durationDaysSkippedSoFar, (ZByte)4);
		}

		[TestDate(2025, 01, 06)]
		public void TestDurationDaysSkippedSoFar_Holiday()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 3;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(holiday: true);
			containerPenalty.CPY_CEX_DurationExclusion = exclusion.PK;

			var location = Factory.New<RefUNLOCO>();
			location.RL_Code = "AAAAA";

			var state = Factory.New<RefCountryStates>();
			location.RL_RW = state.PK;

			var holiday = state.Holidays.AddNew();
			holiday.GH_HolidayName = "Some Other Day";
			holiday.GH_ParentID = state.PK;
			holiday.GH_ParentTableCode = "RW";
			holiday.GH_IsWorkingDay = false;
			holiday.GH_Date = new ZDateTime(2025, 1, 05);
			holiday.GH_Recurring = true;
			holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;

			AssertEquals(containerPenalty.LastFreeDay, new ZDateTime(2025, 1, 3));

			var durationDaysSkippedSoFar = PenaltyExclusionsCalculator.CalculateNumberOfExcludedDurationSoFar(containerPenalty);
			AssertEquals(durationDaysSkippedSoFar, (ZByte)1);
		}

		[TestDate(2025, 01, 10)]
		public void TestDurationDaysSkippedSoFar_LocationWeekends()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 5;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(weekend: true);
			containerPenalty.CPY_CEX_DurationExclusion = exclusion.PK;

			var location = Factory.New<RefUNLOCO>();
			location.RL_Code = "AAAAA";

			var state = Factory.New<RefCountryStates>();
			location.RL_RW = state.PK;

			SetupWeekendsForState(state, thursday: true, friday: true);

			AssertEquals(containerPenalty.LastFreeDay, new ZDateTime(2025, 1, 5));

			var durationDaysSkippedSoFar = PenaltyExclusionsCalculator.CalculateNumberOfExcludedDurationSoFar(containerPenalty);
			AssertEquals(durationDaysSkippedSoFar, (ZByte)2);
		}

		[TestDate(2025, 01, 15)]
		public void TestDurationDaysSkippedSoFar_All()
		{
			var containerPenalty = Factory.NewWithValidTestData<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 7;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(monday: true, weekend: true, holiday: true);
			containerPenalty.CPY_CEX_DurationExclusion = exclusion.PK;

			var location = Factory.NewWithValidTestData<RefUNLOCO>();
			location.RL_Code = "AAAAA";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			location.RL_RW = state.PK;

			SetupWeekendsForState(state, thursday: true, friday: true);

			var holiday = state.Holidays.AddNew();
			holiday.GH_HolidayName = "Some Other Day";
			holiday.GH_ParentID = state.PK;
			holiday.GH_ParentTableCode = "RW";
			holiday.GH_IsWorkingDay = false;
			holiday.GH_Date = new ZDateTime(2025, 1, 11);
			holiday.GH_Recurring = true;
			holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;

			Factory.Save();

			AssertEquals(containerPenalty.LastFreeDay, new ZDateTime(2025, 1, 7));

			var durationDaysSkippedSoFar = PenaltyExclusionsCalculator.CalculateNumberOfExcludedDurationSoFar(containerPenalty);

			// 1/8: Wed
			// 1/9: Thu - Skip
			// 1/10: Fri - Skip
			// 1/11: Sat - Skip (Holiday)
			// 1/12: Sun
			// 1/13: Mon - Skip
			// 1/14: Tue
			// 1/15: Wed
			AssertEquals(durationDaysSkippedSoFar, (ZByte)4);
		}

		[TestDate(2025, 01, 06)]
		public void TestDurationDaysSkippedSoFar_HolidayNotSpecified()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 3;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(holiday: false);
			containerPenalty.CPY_CEX_DurationExclusion = exclusion.PK;

			var location = Factory.New<RefUNLOCO>();
			location.RL_Code = "AAAAA";

			var state = Factory.New<RefCountryStates>();
			location.RL_RW = state.PK;

			var holiday = state.Holidays.AddNew();
			holiday.GH_HolidayName = "Some Other Day";
			holiday.GH_ParentID = state.PK;
			holiday.GH_ParentTableCode = "RW";
			holiday.GH_IsWorkingDay = false;
			holiday.GH_Date = new ZDateTime(2025, 1, 05);
			holiday.GH_Recurring = true;
			holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;

			AssertEquals(containerPenalty.LastFreeDay, new ZDateTime(2025, 1, 3));

			var durationDaysSkippedSoFar = PenaltyExclusionsCalculator.CalculateNumberOfExcludedDurationSoFar(containerPenalty);
			AssertEquals(durationDaysSkippedSoFar, (ZByte)0);
		}

		[TestDate(2025, 01, 10)]
		public void TestDurationDaysSkippedSoFar_LocationWeekendsNotSpecified()
		{
			var containerPenalty = Factory.New<ContainerPenalty>();

			// First of January 2025 is a Wednesday
			containerPenalty.CPY_FirstFreeDay = new ZDateTimeOffset(2025, 1, 1);
			containerPenalty.FreeTimeAsDays = 5;
			containerPenalty.CPY_RL_NKLocation = "AAAAA";

			var exclusion = SetupExclusion(weekend: false);
			containerPenalty.CPY_CEX_DurationExclusion = exclusion.PK;

			var location = Factory.New<RefUNLOCO>();
			location.RL_Code = "AAAAA";

			var state = Factory.New<RefCountryStates>();
			location.RL_RW = state.PK;

			SetupWeekendsForState(state, thursday: true, friday: true);

			AssertEquals(containerPenalty.LastFreeDay, new ZDateTime(2025, 1, 5));

			var durationDaysSkippedSoFar = PenaltyExclusionsCalculator.CalculateNumberOfExcludedDurationSoFar(containerPenalty);
			AssertEquals(durationDaysSkippedSoFar, (ZByte)0);
		}

		ContainerPenaltyDayExclusion SetupExclusion(
			bool monday = false,
			bool tuesday = false,
			bool wednesday = false,
			bool thursday = false,
			bool friday = false,
			bool saturday = false,
			bool sunday = false,
			bool weekend = false,
			bool holiday = false)
		{
			var exclusion = Factory.New<ContainerPenaltyDayExclusion>();
			exclusion.CEX_Monday = monday;
			exclusion.CEX_Tuesday = tuesday;
			exclusion.CEX_Wednesday = wednesday;
			exclusion.CEX_Thursday = thursday;
			exclusion.CEX_Friday = friday;
			exclusion.CEX_Saturday = saturday;
			exclusion.CEX_Sunday = sunday;
			exclusion.CEX_Weekend = weekend;
			exclusion.CEX_Holiday = holiday;

			return exclusion;
		}

		void SetupWeekendsForState(RefCountryStates state,
			bool monday = false,
			bool tuesday = false,
			bool wednesday = false,
			bool thursday = false,
			bool friday = false,
			bool saturday = false,
			bool sunday = false)
		{
			var mon = state.Weekends.AddNew();
			mon.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			mon.GH_RecurrDay = CalendarCodes.Days.Monday;
			mon.GH_ParentID = state.PK;
			mon.GH_ParentTableCode = "RW";
			mon.GH_IsWorkingDay = !monday;

			var tue = state.Weekends.AddNew();
			tue.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			tue.GH_RecurrDay = CalendarCodes.Days.Tuesday;
			tue.GH_ParentID = state.PK;
			tue.GH_ParentTableCode = "RW";
			tue.GH_IsWorkingDay = !tuesday;

			var wed = state.Weekends.AddNew();
			wed.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			wed.GH_RecurrDay = CalendarCodes.Days.Wednesday;
			wed.GH_ParentID = state.PK;
			wed.GH_ParentTableCode = "RW";
			wed.GH_IsWorkingDay = !wednesday;

			var thu = state.Weekends.AddNew();
			thu.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			thu.GH_RecurrDay = CalendarCodes.Days.Thursday;
			thu.GH_ParentID = state.PK;
			thu.GH_ParentTableCode = "RW";
			thu.GH_IsWorkingDay = !thursday;

			var fri = state.Weekends.AddNew();
			fri.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			fri.GH_RecurrDay = CalendarCodes.Days.Friday;
			fri.GH_ParentID = state.PK;
			fri.GH_ParentTableCode = "RW";
			fri.GH_IsWorkingDay = !friday;

			var sat = state.Weekends.AddNew();
			sat.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			sat.GH_RecurrDay = CalendarCodes.Days.Saturday;
			sat.GH_ParentID = state.PK;
			sat.GH_ParentTableCode = "RW";
			sat.GH_IsWorkingDay = !saturday;

			var sun = state.Weekends.AddNew();
			sun.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			sun.GH_RecurrDay = CalendarCodes.Days.Sunday;
			sun.GH_ParentID = state.PK;
			sun.GH_ParentTableCode = "RW";
			sun.GH_IsWorkingDay = !sunday;
		}

		void SetupWeekendsForCountry(RefCountry country,
			bool monday = false,
			bool tuesday = false,
			bool wednesday = false,
			bool thursday = false,
			bool friday = false,
			bool saturday = false,
			bool sunday = false)
		{
			var mon = country.Weekends.AddNew();
			mon.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			mon.GH_RecurrDay = CalendarCodes.Days.Monday;
			mon.GH_ParentID = country.PK;
			mon.GH_ParentTableCode = "RN";
			mon.GH_IsWorkingDay = !monday;

			var tue = country.Weekends.AddNew();
			tue.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			tue.GH_RecurrDay = CalendarCodes.Days.Tuesday;
			tue.GH_ParentID = country.PK;
			tue.GH_ParentTableCode = "RN";
			tue.GH_IsWorkingDay = !tuesday;

			var wed = country.Weekends.AddNew();
			wed.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			wed.GH_RecurrDay = CalendarCodes.Days.Wednesday;
			wed.GH_ParentID = country.PK;
			wed.GH_ParentTableCode = "RN";
			wed.GH_IsWorkingDay = !wednesday;

			var thu = country.Weekends.AddNew();
			thu.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			thu.GH_RecurrDay = CalendarCodes.Days.Thursday;
			thu.GH_ParentID = country.PK;
			thu.GH_ParentTableCode = "RN";
			thu.GH_IsWorkingDay = !thursday;

			var fri = country.Weekends.AddNew();
			fri.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			fri.GH_RecurrDay = CalendarCodes.Days.Friday;
			fri.GH_ParentID = country.PK;
			fri.GH_ParentTableCode = "RN";
			fri.GH_IsWorkingDay = !friday;

			var sat = country.Weekends.AddNew();
			sat.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			sat.GH_RecurrDay = CalendarCodes.Days.Saturday;
			sat.GH_ParentID = country.PK;
			sat.GH_ParentTableCode = "RN";
			sat.GH_IsWorkingDay = !saturday;

			var sun = country.Weekends.AddNew();
			sun.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			sun.GH_RecurrDay = CalendarCodes.Days.Sunday;
			sun.GH_ParentID = country.PK;
			sun.GH_ParentTableCode = "RN";
			sun.GH_IsWorkingDay = !sunday;
		}
	}
}
