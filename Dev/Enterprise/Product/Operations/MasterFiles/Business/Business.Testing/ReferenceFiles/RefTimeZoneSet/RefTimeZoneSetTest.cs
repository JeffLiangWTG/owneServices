using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefTimeZoneSet))]
	sealed class RefTimeZoneSetTest : EnterpriseBusinessObjectTestCase
	{
		#region When Daylight Saving Zone has no START and/or END rule

		/// <summary>
		/// If the DaylightSavingZone has no START or END rule for any given year,
		/// it should be interpreted as:
		///   - !START and !END => No DST in the whole year
		///   -  START and !END => DST from START rule till the end of the year
		///   - !START and  END => DST from beginning of the year till END rule
		///   -  START and  END => DST as per normal
		/// </summary>
		public void TestWhenDaylightSavingZoneHasNoStartAndOrEndRuleForGivenYear()
		{
			RefTimeZoneSet testZoneSet = GetRefTimeZoneSetForNoStartAndOrEndRuleTest();
			ITimeZone timeZone = testZoneSet.GetCalculationTimeZone();

			// 2005: !START and !END => No DST in the whole year
			AssertIsDst(timeZone, new DateTime(2005, 01, 01, 0, 0, 0), false, "2005: No DST Rules (start of the year)");
			AssertIsDst(timeZone, new DateTime(2005, 06, 25, 1, 0, 0), false, "2005: No DST Rules (any date-time)");
			AssertIsDst(timeZone, new DateTime(2005, 12, 31, 23, 59, 59), false, "2005: No DST Rules (end of the year)");

			// 2006: START and !END => DST from START rule till the end of the year
			AssertIsDst(timeZone, new DateTime(2006, 06, 25, 1, 0, 0), false, "2006: No DST END Rule (< DST Start)");
			AssertIsDst(timeZone, new DateTime(2006, 11, 25, 1, 0, 0), true, "2006: No DST END Rule (> DST Start)");
			AssertIsDst(timeZone, new DateTime(2006, 12, 31, 23, 59, 59), true, "2006: No DST END Rule (end of the year)");

			// 2007: START and END => DST as per normal
			AssertIsDst(timeZone, new DateTime(2007, 02, 25, 1, 0, 0), true, "2007: Both START/END Rules (< DST End)");
			AssertIsDst(timeZone, new DateTime(2007, 06, 25, 1, 0, 0), false, "2007: Both START/END Rules (> DST End, < Start)");
			AssertIsDst(timeZone, new DateTime(2007, 11, 25, 1, 0, 0), true, "2007: Both START/END Rules (> DST Start)");

			// 2008: !START and END => DST from beginning of the year till END rule
			AssertIsDst(timeZone, new DateTime(2008, 01, 01, 0, 0, 0), true, "2008: No DST START Rule (start of the year)");
			AssertIsDst(timeZone, new DateTime(2008, 02, 25, 1, 0, 0), true, "2008: No DST START Rule (< DST End)");
			AssertIsDst(timeZone, new DateTime(2008, 06, 25, 1, 0, 0), false, "2008: No DST START Rule (> DST End)");

			// 2009: !START and !END => No DST in the whole year
			AssertIsDst(timeZone, new DateTime(2009, 01, 01, 0, 0, 0), false, "2009: No DST Rules (start of the year)");
			AssertIsDst(timeZone, new DateTime(2009, 06, 25, 1, 0, 0), false, "2009: No DST Rules (any date-time)");
			AssertIsDst(timeZone, new DateTime(2009, 12, 31, 23, 59, 59), false, "2009: No DST Rules (end of the year)");
		}

		/// <summary>
		/// Create Test Zone Set with DST Zone and rules for no START and/or END rule test
		/// </summary>
		RefTimeZoneSet GetRefTimeZoneSetForNoStartAndOrEndRuleTest()
		{
			RefTimeZoneSet testZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			StandardTimeZone testStdZone = testZoneSet.StandardZone;
			testZoneSet.HasDaylightSavings = true;
			DaylightSavingTimeZone testDstZone = testZoneSet.DaylightSavingZone;

			RefTimeZoneRule ruleStart20062007 = testDstZone.StartDateRules.AddNew();
			ruleStart20062007.R4_FromYear = 2006;
			ruleStart20062007.R4_ToYear = 2007;
			ruleStart20062007.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;
			ruleStart20062007.R4_TypeOfTime = TimeZoneConstants.DstTimeBaseUtc;
			ruleStart20062007.R4_DaylightSavingDate = new ZDateTime(1900, 10, 15, 2, 0, 0);

			RefTimeZoneRule ruleEnd20072008 = testDstZone.EndDateRules.AddNew();
			ruleEnd20072008.R4_FromYear = 2007;
			ruleEnd20072008.R4_ToYear = 2008;
			ruleEnd20072008.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;
			ruleEnd20072008.R4_TypeOfTime = TimeZoneConstants.DstTimeBaseUtc;
			ruleEnd20072008.R4_DaylightSavingDate = new ZDateTime(1900, 3, 15, 2, 0, 0);

			return testZoneSet;
		}

		void AssertIsDst(ITimeZone calculationTimeZone, DateTime testUtc, bool expectedDst, string messagePrefix)
		{
			string assertMessage = String.Format(
				"{0}. Is [UTC {1}] in Daylight Saving?", messagePrefix, Env.Time.FormatDateTime(testUtc));
			AssertEquals(assertMessage, expectedDst, calculationTimeZone.IsDaylightSavingBasedOnUtc(testUtc));
		}

		#endregion

		#region Related Business Objects

		#region DaylightSavingZones

		public void TestDaylightSavingZones()
		{
			Factory.Save();

			ZGuid firstTimeZoneSetPK = TimeZoneSet.PK;
			ZGuid firstDaylightZonePK = TimeZoneSet.DaylightSavingZone.PK;

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			RefTimeZoneSet timeZoneSet1 = newFactory.Load<RefTimeZoneSet>(firstTimeZoneSetPK);
			DaylightSavingTimeZone daylightZoneLoadedFromDb = newFactory.Load<DaylightSavingTimeZone>(firstDaylightZonePK);

			Assert(timeZoneSet1.DaylightSavingZones is DaylightSavingTimeZoneCollection);
			AssertCollectionContains("Daylight saving zone should be loaded if it exists", daylightZoneLoadedFromDb, timeZoneSet1.DaylightSavingZones);
			Assert(timeZoneSet1.IsRegisteredEditableChildObject(timeZoneSet1.DaylightSavingZones));

			RefTimeZoneSet timeZoneSet2 = newFactory.New<RefTimeZoneSet>();
			AssertEquals("There should be zero daylight savings zones if HasDaylightSAvings has not been set to true.", 0, timeZoneSet2.DaylightSavingZones.Count);
		}

		#endregion

		#region DaylightSavingZone

		public void TestDaylightSavingZone()
		{
			AssertEquals(DaylightSavingZone, TimeZoneSet.DaylightSavingZones[0]);

			TimeZoneSet.HasDaylightSavings = false;
			AssertEquals(0, TimeZoneSet.DaylightSavingZones.Count);

			AssertNull(TimeZoneSet.DaylightSavingZone);
		}

		#endregion

		#endregion

		#region HasDaylightSavings

		public void TestHasDaylightSavingsFirstCase()
		{
			TimeZoneSet.R3_R2_DaylightSavingZone = ZGuid.Empty;

			//test with no daylight savings
			TimeZoneSet.R3_TimeZoneSetName = "Australian Eastern Standard Time";
			StandardZone.R2_CivilianTimeZoneFullName = "Australian NSW Time";
			StandardZone.R2_CivilianTimeZoneCode = "AEST";
			StandardZone.R2_MilitaryTimeZoneCode = "SD";
			StandardZone.R2_OffsetMinutesFromUTC = new ZShort(600);
			TimeZoneSet.HasDaylightSavings = false;

			Assert("DaylightSavingZone should not have been created", TimeZoneSet.R3_R2_DaylightSavingZone.IsEmpty);
			AssertEquals("A RefTimeZoneCollection should have been created", 0, TimeZoneSet.DaylightSavingZones.Count);

			//test daylight savings - normal case
			TimeZoneSet.HasDaylightSavings = true;

			Assert("A DaylightSavingZone should have been created", !TimeZoneSet.R3_R2_DaylightSavingZone.IsEmpty);
			AssertEquals("There should be one item in RefTimeZoneCollection.", 1, TimeZoneSet.DaylightSavingZones.Count);
			AssertEquals("The DaylightSavingZone in RefTimeZoneCollection should be the same as the one in TimeZoneSet", TimeZoneSet.DaylightSavingZones[0].PK, TimeZoneSet.R3_R2_DaylightSavingZone);

			DaylightSavingZone = TimeZoneSet.DaylightSavingZones[0];

			DaylightSavingZone.R2_CivilianTimeZoneFullName = "Australian NSW Daylight Savings Time";
			DaylightSavingZone.R2_CivilianTimeZoneCode = "ADST";
			DaylightSavingZone.R2_MilitaryTimeZoneCode = "DS";
			DaylightSavingZone.R2_OffsetMinutesFromUTC = new ZShort(660);

			Rule = DaylightSavingZone.StartDateRules.AddNew();

			int startYear = DateTime.Now.Year;
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleWeekday;
			Rule.R4_FromYear = startYear;
			Rule.R4_ToYear = startYear + 1;
			Rule.R4_DaylightSavingDayName = "MON";
			Rule.R4_DaylightSavingMonth = "JAN";
			Rule.DayNumber = "2";

			//the following just fills out the rest of the fields. done to clear notifications on HasDaylightSavingsInfo
			Rule.DaylightSavingChangeTime = new ZDateTime(startYear, 1, 1, 12, 0, 0);
			Rule.R4_TypeOfTime = TimeZoneConstants.DstTimeBaseUtc;
			DaylightSavingZone.EndDateRules.AddNew();
			DaylightSavingZone.EndDateRules[0].R4_FromYear = startYear;
			DaylightSavingZone.EndDateRules[0].R4_ToYear = startYear + 1;
			DaylightSavingZone.EndDateRules[0].R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;
			DaylightSavingZone.EndDateRules[0].DateDayNumber = "1";
			DaylightSavingZone.EndDateRules[0].DateMonth = "AUG";
			DaylightSavingZone.EndDateRules[0].R4_TypeOfTime = TimeZoneConstants.DstTimeBaseUtc;
			DaylightSavingZone.EndDateRules[0].DaylightSavingChangeTime = new ZDateTime(startYear, 1, 1, 12, 0, 0);
			TimeZoneSet.RunPreSaveValidation();

			AssertNoErrors("There should be no errors at this point in the execution", TimeZoneSet);

			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;
			Rule.R4_DaylightSavingDate = new ZDateTime(startYear, 07, 07);

			AssertNoErrors("There should be no errors at this point in the execution", TimeZoneSet);
		}

		public void TestHasDaylightSavingsTurnedOff()
		{
			//create normal standard zone

			TimeZoneSet.R3_TimeZoneSetName = "Australian Eastern Standard Time";
			StandardZone.R2_CivilianTimeZoneFullName = "Australian NSW Time";
			StandardZone.R2_CivilianTimeZoneCode = "AEST";
			StandardZone.R2_MilitaryTimeZoneCode = "SD";
			StandardZone.R2_OffsetMinutesFromUTC = new ZShort(600);

			TimeZoneSet.HasDaylightSavings = true;

			//create incomplete daylight saving object
			DaylightSavingZone = TimeZoneSet.DaylightSavingZones[0];

			DaylightSavingZone.R2_CivilianTimeZoneFullName = "Australian NSW Daylight Savings Time";
			DaylightSavingZone.R2_CivilianTimeZoneCode = "ADST";
			DaylightSavingZone.R2_MilitaryTimeZoneCode = "DS";
			DaylightSavingZone.R2_OffsetMinutesFromUTC = new ZShort(660);

			Rule = DaylightSavingZone.StartDateRules[0];

			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleWeekday;
			Rule.DayNumber = "";
			Rule.R4_DaylightSavingDayName = "MON";
			Rule.R4_DaylightSavingMonth = "";

			AssertHasErrors("Should have errors if empty", Rule.R4_DaylightSavingMonthInfo);
			AssertHasErrors("Should have errors if empty", Rule.DayNumberInfo);

			//delete incomplete daylight saving object
			TimeZoneSet.HasDaylightSavings = false;
			Assert("The existing DaylightSavingZone should have been deleted", TimeZoneSet.R3_R2_DaylightSavingZone.IsEmpty);
			Assert("The existing DaylightSavingZone should have been deleted", DaylightSavingZone.IsDeleted);
			AssertEquals("There should be NO items in the RefTimeZoneCollection", 0, TimeZoneSet.DaylightSavingZones.Count);

			TimeZoneSet.HasDaylightSavings = true;

			//create a complete daylight saving object
			Assert("A DaylightSavingZone should have been created", !(TimeZoneSet.R3_R2_DaylightSavingZone.IsEmpty));
			AssertEquals("There should be one item in RefTimeZoneCollection.", 1, TimeZoneSet.DaylightSavingZones.Count);
			AssertEquals("The DaylightSavingZone in RefTimeZoneCollection should be the same as the one in TimeZoneSet", TimeZoneSet.DaylightSavingZones[0].PK, TimeZoneSet.R3_R2_DaylightSavingZone);

			DaylightSavingZone = TimeZoneSet.DaylightSavingZones[0];

			DaylightSavingZone.R2_CivilianTimeZoneFullName = "Australian NSW Daylight Savings Time";
			DaylightSavingZone.R2_CivilianTimeZoneCode = "ADST";
			DaylightSavingZone.R2_MilitaryTimeZoneCode = "DS";
			DaylightSavingZone.R2_OffsetMinutesFromUTC = new ZShort(-540);

			Rule = DaylightSavingZone.StartDateRules.AddNew();

			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleWeekday;
			Rule.DayNumber = "2";
			Rule.R4_DaylightSavingDayName = "MON";
			Rule.R4_DaylightSavingMonth = "JAN";
			Rule.R4_FromYear = 2006;
			Rule.R4_ToYear = 2007;

			//TimeZoneSet.HasDaylightSavingsInfo.ClearAllNotifications();

			Rule.DaylightSavingChangeTime = new ZDateTime(2006, 1, 1, 12, 0, 0);
			Rule.R4_TypeOfTime = TimeZoneConstants.DstTimeBaseUtc;
			DaylightSavingZone.EndDateRules.AddNew();
			DaylightSavingZone.EndDateRules[0].R4_FromYear = 2006;
			DaylightSavingZone.EndDateRules[0].R4_ToYear = 2007;
			DaylightSavingZone.EndDateRules[0].R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;
			DaylightSavingZone.EndDateRules[0].DateDayNumber = "1";
			DaylightSavingZone.EndDateRules[0].DateMonth = "AUG";
			DaylightSavingZone.EndDateRules[0].R4_TypeOfTime = TimeZoneConstants.DstTimeBaseUtc;
			DaylightSavingZone.EndDateRules[0].DaylightSavingChangeTime = new ZDateTime(2006, 1, 1, 12, 0, 0);
			TimeZoneSet.RunPreSaveValidation();

			AssertNoErrors("There should be no errors at this point in the execution", TimeZoneSet);

			TimeZoneSet.HasDaylightSavings = false;
			Assert("There should be NO items in the RefTimeZoneCollection", TimeZoneSet.DaylightSavingZones.Count == 0);
		}

		#endregion

		#region DaylightSavingOffsetIncremented

		public void TestDaylightSavingOffsetIncremented()
		{
			TimeZoneSet.HasDaylightSavings = false;
			StandardZone.R2_OffsetMinutesFromUTC = 120;
			TimeZoneSet.HasDaylightSavings = true;

			AssertEquals("Daylight Saving offset should default to one more than the Standard Zone offset", (ZShort)180, TimeZoneSet.DaylightSavingZone.R2_OffsetMinutesFromUTC);

			TimeZoneSet.DaylightSavingZone.R2_OffsetMinutesFromUTC = 540;
			StandardZone.R2_OffsetMinutesFromUTC = 180;

			AssertEquals("Once a daylight saving zone is created, changes to the standard zone offset should not affect the daylight saving offset", (ZShort)540, TimeZoneSet.DaylightSavingZone.R2_OffsetMinutesFromUTC);
		}

		#endregion

		#region Logging

		public void TestLogging()
		{
			Factory.Save();
			AssertEquals("There should be three events in the log, add for set, std zone and daylight zone", 3, TimeZoneSetLogs.Collection.Count);

			TimeZoneSet.HasDaylightSavings = false;
			Factory.Save();
			AssertEquals("There should be three events in the log - 1 add for Set, 1 add for standard zone, 1 edit for set", 3, TimeZoneSetLogs.Collection.Count);

			TimeZoneSet.HasDaylightSavings = true;
			((ILogsInternals)TimeZoneSet.Logs).ReloadFromDB();
			AssertEquals("There should be three events in the log - exactly as per previous as nothing has been saved", 3, TimeZoneSetLogs.Collection.Count);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			AssertCollectionContains(StandardZone, TimeZoneSet.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains(DaylightSavingZone, TimeZoneSet.BusinessObjectsWithRelatedEvents);

			TimeZoneSet.HasDaylightSavings = false;
			AssertCollectionNotContains(DaylightSavingZone, TimeZoneSet.BusinessObjectsWithRelatedEvents);
		}

		#endregion

		#region DaylightSavingRecordSavedAndLoaded

		public void TestDaylightSavingRecordSavedAndLoaded()
		{
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			RefTimeZoneSet newTimeZoneSet = newFactory.Load<RefTimeZoneSet>(TimeZoneSet.PK);
			DaylightSavingTimeZone newDaylightSavingZone = newTimeZoneSet.DaylightSavingZone;
			StandardTimeZone newStandardZone = newTimeZoneSet.StandardZone;

			AssertNotNull(newTimeZoneSet);
			AssertNotNull(newDaylightSavingZone);
			AssertNotNull(newStandardZone);
			AssertEquals("Daylight Saving Zone is not the same", newTimeZoneSet.R3_R2_DaylightSavingZone, DaylightSavingZone.PK);
			AssertEquals("Daylight Saving Zone is not the same", newDaylightSavingZone.PK, DaylightSavingZone.PK);
			AssertEquals("Standard Zone is not the same", newTimeZoneSet.R3_R2_StandardZone, StandardZone.PK);
			AssertEquals("Standard Zone is not the same", newStandardZone.PK, StandardZone.PK);
		}

		#endregion

		#region DaylightSavingDatesAreInLocalTime

		public void TestDaylightSavingDatesAreInLocalTime()
		{
			RefTimeZoneStartRuleCollection startRules = DaylightSavingZone.StartDateRules;
			RefTimeZoneEndRuleCollection endRules = DaylightSavingZone.EndDateRules;

			DaylightSavingZone.R2_OffsetMinutesFromUTC = 240;
			StandardZone.R2_OffsetMinutesFromUTC = 180;

			endRules[0].R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;
			endRules[0].R4_TypeOfTime = TimeZoneConstants.DstTimeBaseUtc;
			endRules[0].R4_DaylightSavingDate = new ZDateTime(2006, 04, 05, 9, 10, 10);
			endRules[0].R4_FromYear = 2006;
			endRules[0].R4_ToYear = 2006;

			ZDateTime expected = new ZDateTime(2006, 04, 05, 13, 10, 10);
			AssertEquals("Time should be converted to local time", expected, TimeZoneSet.GetDaylightSavingStartOrEndDateInYear(2006, DaylightSavingZone.EndDateRules));

			endRules[0].R4_TypeOfTime = TimeZoneConstants.DstTimeBaseStandard;

			expected = new ZDateTime(2006, 04, 05, 10, 10, 10);
			AssertEquals("Time should be converted to local time", expected, TimeZoneSet.GetDaylightSavingStartOrEndDateInYear(2006, DaylightSavingZone.EndDateRules));

			startRules[0].R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;
			startRules[0].R4_TypeOfTime = TimeZoneConstants.DstTimeBaseUtc;
			startRules[0].R4_DaylightSavingDate = new ZDateTime(2006, 04, 05, 9, 10, 10);
			startRules[0].R4_FromYear = 2006;
			startRules[0].R4_ToYear = 2006;

			expected = new ZDateTime(2006, 04, 05, 12, 10, 10);
			AssertEquals("Time should be converted to local time", expected, TimeZoneSet.GetDaylightSavingStartOrEndDateInYear(2006, DaylightSavingZone.StartDateRules));

			startRules[0].R4_TypeOfTime = TimeZoneConstants.DstTimeBaseLocal;
			expected = new ZDateTime(2006, 04, 05, 9, 10, 10);
			AssertEquals("Should return local time", expected, TimeZoneSet.GetDaylightSavingStartOrEndDateInYear(2006, DaylightSavingZone.StartDateRules));

			expected = ZDateTime.Empty;
			AssertEquals("Should return empty date if no rule for a particular year exists", expected, TimeZoneSet.GetDaylightSavingStartOrEndDateInYear(2009, DaylightSavingZone.StartDateRules));
		}

		#endregion

		#region CalculationTimeZone

		public void TestToLocalTime()
		{
			StandardZone.R2_OffsetMinutesFromUTC = 600;
			DaylightSavingZone.R2_OffsetMinutesFromUTC = 660;

			ITimeZone calculationTimezone = TimeZoneSet.GetCalculationTimeZone();

			RefTimeZoneRule startRule = TimeZoneSet.DaylightSavingZone.StartDateRules[0];
			startRule.R4_FromYear = 2006;
			startRule.R4_ToYear = 2006;
			startRule.R4_DaylightSavingDate = new ZDateTime(2006, 03, 04, 2, 0, 0);
			startRule.R4_TypeOfTime = TimeZoneConstants.DstTimeBaseLocal;
			startRule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;

			RefTimeZoneRule endRule = TimeZoneSet.DaylightSavingZone.EndDateRules[0];
			endRule.R4_FromYear = 2006;
			endRule.R4_ToYear = 2006;
			endRule.R4_DaylightSavingDate = new ZDateTime(2006, 10, 05, 3, 0, 0);
			endRule.R4_TypeOfTime = TimeZoneConstants.DstTimeBaseLocal;
			endRule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;

			//
			// Middle conditions
			//

			DateTime uTCTime = new DateTime(2006, 01, 04, 2, 0, 0);
			DateTime expectedLocalTime = new DateTime(2006, 01, 04, 12, 0, 0);
			DateTime actualLocalTime = calculationTimezone.ToLocalTime(uTCTime);
			AssertEquals("Correct local time in standard zone should be returned", expectedLocalTime, actualLocalTime);

			uTCTime = new DateTime(2006, 06, 04, 2, 0, 0);
			expectedLocalTime = new DateTime(2006, 06, 04, 13, 0, 0);
			actualLocalTime = calculationTimezone.ToLocalTime(uTCTime);
			AssertEquals("Correct local time in daylight saving zone should be returned", expectedLocalTime, actualLocalTime);

			//
			// Border conditions
			//

			//Test Start of Daylight Saving

			//UTC time just before start of daylight saving (as per UTC time)
			uTCTime = new DateTime(2006, 03, 03, 15, 59, 0);
			expectedLocalTime = new DateTime(2006, 03, 04, 1, 59, 0);
			actualLocalTime = calculationTimezone.ToLocalTime(uTCTime);
			AssertEquals("Local time should have been calculated using Standard UTC offset", expectedLocalTime, actualLocalTime);

			//UTC time on start of daylight saving (at which point daylight saving should kick in) - (as per UTC time)
			uTCTime = new DateTime(2006, 03, 03, 16, 0, 0);
			expectedLocalTime = new DateTime(2006, 03, 04, 3, 0, 0);
			actualLocalTime = calculationTimezone.ToLocalTime(uTCTime);
			AssertEquals("Local time should have been calculated using Daylight Saving UTC offset", expectedLocalTime, actualLocalTime);

			//UTC time just after start of daylight saving (at which point daylight saving should kick in) - (as per UTC time)
			uTCTime = new DateTime(2006, 03, 03, 16, 1, 0);
			expectedLocalTime = new DateTime(2006, 03, 04, 3, 1, 0);
			actualLocalTime = calculationTimezone.ToLocalTime(uTCTime);
			AssertEquals("Local time should have been calculated using Daylight Saving UTC offset", expectedLocalTime, actualLocalTime);

			//UTC time just before start of daylight saving (as per local time)
			uTCTime = new DateTime(2006, 03, 04, 1, 59, 0);
			expectedLocalTime = new DateTime(2006, 03, 04, 12, 59, 0);
			actualLocalTime = calculationTimezone.ToLocalTime(uTCTime);
			AssertEquals("Local time should have been calculated using Daylight Saving UTC offset", expectedLocalTime, actualLocalTime);

			//UTC time on start of daylight saving time (as per local time)
			uTCTime = new DateTime(2006, 03, 04, 2, 0, 0);
			expectedLocalTime = new DateTime(2006, 03, 04, 13, 0, 0);
			actualLocalTime = calculationTimezone.ToLocalTime(uTCTime);
			AssertEquals("Local time should have been calculated using Daylight Saving UTC offset", expectedLocalTime, actualLocalTime);

			//UTC time just after start of daylight saving time (as per local time)
			uTCTime = new DateTime(2006, 03, 04, 2, 1, 0);
			expectedLocalTime = new DateTime(2006, 03, 04, 13, 1, 0);
			actualLocalTime = calculationTimezone.ToLocalTime(uTCTime);
			AssertEquals("Local time should have been calculated using Daylight Saving UTC offset", expectedLocalTime, actualLocalTime);

			//Test end of Daylight Saving

			//UTC time just before end of daylight saving (as per UTC time)
			uTCTime = new DateTime(2006, 10, 04, 15, 59, 0);
			expectedLocalTime = new DateTime(2006, 10, 05, 2, 59, 0);
			actualLocalTime = calculationTimezone.ToLocalTime(uTCTime);
			AssertEquals("Local time should have been calculated using Daylight Saving UTC offset", expectedLocalTime, actualLocalTime);

			//UTC time on end of daylight saving (as per UTC time)
			uTCTime = new DateTime(2006, 10, 04, 16, 0, 0);
			expectedLocalTime = new DateTime(2006, 10, 05, 2, 0, 0);
			actualLocalTime = calculationTimezone.ToLocalTime(uTCTime);
			AssertEquals("Local time should have been calculated using Standard UTC offset", expectedLocalTime, actualLocalTime);

			//UTC time just after end of daylight saving (at which point standard time should kick in) - (as per UTC time)
			uTCTime = new DateTime(2006, 10, 04, 16, 1, 0);
			expectedLocalTime = new DateTime(2006, 10, 05, 02, 1, 0);
			actualLocalTime = calculationTimezone.ToLocalTime(uTCTime);
			AssertEquals("Local time should have been calculated using Standard UTC offset", expectedLocalTime, actualLocalTime);

			//UTC time just before end of daylight saving (as per local time)
			uTCTime = new DateTime(2006, 10, 05, 2, 59, 0);
			expectedLocalTime = new DateTime(2006, 10, 05, 12, 59, 0);
			actualLocalTime = calculationTimezone.ToLocalTime(uTCTime);
			AssertEquals("Local time should have been calculated using Standard UTC offset", expectedLocalTime, actualLocalTime);

			//UTC time on end of daylight saving time (as per local time)
			uTCTime = new DateTime(2006, 10, 05, 3, 0, 0);
			expectedLocalTime = new DateTime(2006, 10, 05, 13, 0, 0);
			actualLocalTime = calculationTimezone.ToLocalTime(uTCTime);
			AssertEquals("Local time should have been calculated using Standard UTC offset", expectedLocalTime, actualLocalTime);

			//UTC time just after end of daylight saving time (as per local time)
			uTCTime = new DateTime(2006, 10, 05, 3, 1, 0);
			expectedLocalTime = new DateTime(2006, 10, 05, 13, 1, 0);
			actualLocalTime = calculationTimezone.ToLocalTime(uTCTime);
			AssertEquals("Local time should have been calculated using Standard UTC offset", expectedLocalTime, actualLocalTime);
		}

		public void TestToLocalTimeWithDateTimeFlaggedAsUtc()
		{
			RefUNLOCO sydLoco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(ZArchitecture.Schema.RefUNLOCOSchema.RL_Code, "AUSYD"));
			RefTimeZoneSet timeZoneSet = sydLoco.TimeZoneSet;
			ITimeZone calculationTimeZone = timeZoneSet.GetCalculationTimeZone();

			DateTime testUtc = new DateTime(2006, 8, 10, 4, 0, 0, DateTimeKind.Utc);
			DateTime testLocalTime = calculationTimeZone.ToLocalTime(testUtc);
			AssertEquals("Local Time (on 10/Aug/2006)", testUtc.AddHours(10), testLocalTime);

			testUtc = new DateTime(2006, 12, 30, 4, 0, 0, DateTimeKind.Utc);
			testLocalTime = calculationTimeZone.ToLocalTime(testUtc);
			AssertEquals("Local Time (on 30/Dec/2006)", testUtc.AddHours(11), testLocalTime);

			testUtc = new DateTime(2007, 2, 1, 4, 0, 0, DateTimeKind.Utc);
			testLocalTime = calculationTimeZone.ToLocalTime(testUtc);
			AssertEquals("Local Time (on 02/Jan/2007)", testUtc.AddHours(11), testLocalTime);
		}

		#endregion

		#region Implementation

		RefTimeZoneSet TimeZoneSet;
		StandardTimeZone StandardZone;
		DaylightSavingTimeZone DaylightSavingZone;
		RefTimeZoneRule Rule;

		LogsView TimeZoneSetLogs
		{
			get
			{
				if (timeZoneSetLogs == null)
				{
					timeZoneSetLogs = new LogsView(TimeZoneSet, LogsToShow.All);
				}
				return timeZoneSetLogs;
			}
		}
		LogsView timeZoneSetLogs;

		protected override void SetUp()
		{
			base.SetUp();

			TimeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			StandardZone = TimeZoneSet.StandardZone;
			TimeZoneSet.HasDaylightSavings = true;
			DaylightSavingZone = TimeZoneSet.DaylightSavingZone;
			Rule = DaylightSavingZone.StartDateRules.AddNew();
			Rule.R4_DaylightSavingDate = ZDateTime.Now;
			RefTimeZoneRule endRule = DaylightSavingZone.EndDateRules.AddNew();
			endRule.R4_DaylightSavingDate = Rule.R4_DaylightSavingDate.AddDays(7);
		}

		#endregion
	}
}
