using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefTimeZoneSetFactoryTest : TestCaseWithFactory
	{
		#region RegisterStandardTimeZone

		public void TestRegisterStandardTimeZone()
		{
			RefTimeZoneSet timeZoneSet = Factory.New<RefTimeZoneSet>();

			timeZoneSet.CreateStandardZoneIfNotExists();
			Assert(timeZoneSet.IsRegisteredEditableChildObject(timeZoneSet.StandardZone));
		}

		#endregion

		#region HumanReadableName

		public void TestHumanReadableName()
		{
			RefTimeZoneSet timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			AssertEquals("Human Readable name is incorrect or doesn't exist", "Time Zone", timeZoneSet.HumanReadableName);
		}

		#endregion

		#region GetRuleForSpecifiedYear

		public void TestGetRuleForSpecifiedYear()
		{
			RefTimeZoneSet timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet.HasDaylightSavings = true;
			RefTimeZoneStartRuleCollection startRules = timeZoneSet.DaylightSavingZone.StartDateRules;

			startRules.AddNew();
			startRules.AddNew();
			startRules.AddNew();

			startRules[0].R4_FromYear = 2000;
			startRules[0].R4_ToYear = 2005;
			startRules[1].R4_FromYear = 2006;
			startRules[1].R4_ToYear = 2008;
			startRules[2].R4_FromYear = 2009;
			startRules[2].R4_ToYear = 2011;

			RefTimeZoneRule returnedRule = timeZoneSet.GetRuleForSpecifiedYear(2007, startRules);

			AssertEquals(returnedRule, startRules[1]);

			returnedRule = null;
			returnedRule = timeZoneSet.GetRuleForSpecifiedYear(2008, startRules);
			AssertEquals(returnedRule, startRules[1]);

			returnedRule = timeZoneSet.GetRuleForSpecifiedYear(2015, startRules);
			AssertNull(returnedRule);

			returnedRule = timeZoneSet.GetRuleForSpecifiedYear(2008, null);
			AssertNull(returnedRule);
		}

		public void TestGetRuleForSpecifiedYear_EmptyEndYear()
		{
			RefTimeZoneSet timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet.HasDaylightSavings = true;
			RefTimeZoneStartRuleCollection startRules = timeZoneSet.DaylightSavingZone.StartDateRules;

			RefTimeZoneRule rule1 = startRules.AddNew();
			RefTimeZoneRule rule2 = startRules.AddNew();

			rule1.R4_FromYear = 2000;
			rule1.R4_ToYear = 2005;

			rule2.R4_FromYear = 2006;
			rule2.R4_ToYear = 0;

			RefTimeZoneRule returnedRule = timeZoneSet.GetRuleForSpecifiedYear(2003, startRules);
			AssertEquals(returnedRule, rule1);

			returnedRule = timeZoneSet.GetRuleForSpecifiedYear(2008, startRules);
			AssertEquals(returnedRule, rule2);
		}

		#endregion

		#region CreateStandardZoneIfNotExists

		public void TestCreateStandardZoneIfNotExists()
		{
			RefTimeZoneSet timeZoneSet2 = Factory.New<RefTimeZoneSet>();
			Assert("Creation of the StandardZone should be done by suspending HasChanges.", !timeZoneSet2.HasChanges);
			AssertNotNull(timeZoneSet2.R3_R2_StandardZone);

			StandardTimeZone standardZone2 = Factory.Load<StandardTimeZone>(timeZoneSet2.R3_R2_StandardZone);
			Assert(timeZoneSet2.IsRegisteredEditableChildObject(standardZone2));

			timeZoneSet2.CreateStandardZoneIfNotExists();
			AssertEquals("A new standard zone should not be created and not replace the existing StandardZone", standardZone2.PK, timeZoneSet2.R3_R2_StandardZone);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			RefTimeZoneSet timeZoneSet3 = newFactory.Load<RefTimeZoneSet>(timeZoneSet2.PK);

			Assert("Every time a standard time zone is loaded, it should be registered as an editable child", timeZoneSet3.IsRegisteredEditableChildObject(timeZoneSet3.StandardZone));
		}

		#endregion

		#region CreateDaylightSavingZoneIfNotExists

		public void TestCreateDaylightSavingZoneIfNotExists()
		{
			RefTimeZoneSet timeZoneSet = Factory.New<RefTimeZoneSet>();
			timeZoneSet.HasDaylightSavings = true;
			DaylightSavingTimeZone daylightSavingZone = timeZoneSet.DaylightSavingZone;

			timeZoneSet.HasDaylightSavings = false;
			Assert(timeZoneSet.R3_R2_DaylightSavingZone.IsEmpty);

			timeZoneSet.CreateDaylightSavingZoneIfNotExists();
			daylightSavingZone = timeZoneSet.DaylightSavingZone;

			AssertEquals("A daylight saving zone should have been created", 1, timeZoneSet.DaylightSavingZones.Count);
			AssertEquals("UTC Offset should be one more hour than that of Standard Zone UTC Offset", timeZoneSet.StandardZone.R2_OffsetMinutesFromUTC + 60, daylightSavingZone.R2_OffsetMinutesFromUTC);
			AssertEquals(daylightSavingZone.PK, timeZoneSet.R3_R2_DaylightSavingZone);

			timeZoneSet.CreateDaylightSavingZoneIfNotExists();
			AssertEquals("If a daylight saving zone exists, a new one shouldn't be created", 1, timeZoneSet.DaylightSavingZones.Count);
		}

		#endregion

		public void TestWhenIsSystemThenItsAllReadOnly()
		{
			var zoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			zoneSet.R3_IsSystem = true;
			zoneSet.HasDaylightSavings = true;
			var daylightZone = zoneSet.DaylightSavingZone;
			var stdZone = zoneSet.StandardZone;

			Assert(daylightZone.EndDateRules.ReadOnly);
			Assert(daylightZone.StartDateRules.ReadOnly);
			Assert(daylightZone.R2_CivilianTimeZoneCodeInfo.ReadOnly);
			Assert(daylightZone.R2_CivilianTimeZoneFullNameInfo.ReadOnly);
			Assert(daylightZone.R2_MilitaryTimeZoneCodeInfo.ReadOnly);
			Assert(daylightZone.R2_OffsetMinutesFromUTCInfo.ReadOnly);

			Assert(stdZone.R2_CivilianTimeZoneCodeInfo.ReadOnly);
			Assert(stdZone.R2_CivilianTimeZoneFullNameInfo.ReadOnly);
			Assert(stdZone.R2_MilitaryTimeZoneCodeInfo.ReadOnly);
			Assert(stdZone.R2_OffsetMinutesFromUTCInfo.ReadOnly);

			Assert(zoneSet.R3_TimeZoneSetNameInfo.ReadOnly);
			Assert(zoneSet.HasDaylightSavingsInfo.ReadOnly);
		}
	}
}
