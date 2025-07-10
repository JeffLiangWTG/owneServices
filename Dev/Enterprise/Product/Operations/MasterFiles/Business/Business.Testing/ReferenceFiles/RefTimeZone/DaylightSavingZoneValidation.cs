using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DaylightSavingZoneValidation : BusinessObjectValidationTestCase
	{
		#region StartAndEndRuleYearsCorrespond_Errors

		public void TestStartAndEndRuleYearsCorrespond_Errors()
		{
			Assert(!TimeZoneSet.HasDaylightSavingsInfo.HasMessageError("Validation errors exist within the specified start rules or end rules."));

			StartRules[0].R4_FromYear = 1;
			StartRules[0].R4_ToYear = 1;

			// Set mandatory fields so it can be saved
			StartRules[0].R4_DaylightSavingDate = ZDateTime.Now;
			EndRules[0].R4_DaylightSavingDate = ZDateTime.Now;

			Factory.Save();
			TimeZoneSet.RunPreSaveValidation();
			Assert("Should have error if only one rule collection has validation errors", TimeZoneSet.HasDaylightSavingsInfo.HasError("Validation errors exist within the specified start rules or end rules."));

			StartRules[0].R4_FromYear = 2005;
			StartRules[0].R4_ToYear = 2006;

			EndRules[0].R4_FromYear = 1;
			EndRules[0].R4_ToYear = 1;

			Factory.Save();
			Assert("Should have error if only both rule collections have validation errors", TimeZoneSet.HasDaylightSavingsInfo.HasError("Validation errors exist within the specified start rules or end rules."));
		}

		#endregion

		#region StartAndEndRuleYearsCorrespond_Overlaps

		public void TestStartAndEndRuleYearsCorrespond_Overlaps()
		{
			StartRules.AddNew();

			StartRules[1].R4_FromYear = 2006;
			StartRules[1].R4_ToYear = 2008;

			// Set mandatory fields so it can be saved
			StartRules[0].R4_DaylightSavingDate = ZDateTime.Now;
			EndRules[0].R4_DaylightSavingDate = ZDateTime.Now;
			StartRules[1].R4_DaylightSavingDate = ZDateTime.Now;

			Factory.Save();
			TimeZoneSet.RunPreSaveValidation();
			Assert(TimeZoneSet.HasDaylightSavingsInfo.HasError("All start rules must have a corresponding end rule."));

			StartRules[1].R4_FromYear = 2006;
			StartRules[1].R4_ToYear = 2008;

			EndRules.AddNew();
			EndRules[1].R4_FromYear = 2006;
			EndRules[1].R4_ToYear = 2008;

			// Set mandatory fields so it can be saved
			EndRules[1].R4_DaylightSavingDate = ZDateTime.Now;

			Factory.Save();
			TimeZoneSet.RunPreSaveValidation();
			Assert(!TimeZoneSet.HasDaylightSavingsInfo.HasErrors());
		}

		#endregion

		#region	StartAndEndRuleYearsCorrespond_UNLOCO

		public void TestStartAndEndRuleYearsCorrespond_UNLOCO()
		{
			RefUNLOCO uNLOCO = Factory.New<RefUNLOCO>();
			RefTimeZoneSet timeZoneSet = Factory.New<RefTimeZoneSet>();

			uNLOCO.RL_R3 = timeZoneSet.PK;
			uNLOCO.RL_GeoLocation = new ZGeography("POINT(77.15 28.6)");
			timeZoneSet.R3_R2_DaylightSavingZone = DaylightSavingZone.PK;

			StartRules.AddNew();
			StartRules.AddNew();
			StartRules[1].R4_FromYear = 2006;
			StartRules[1].R4_ToYear = 2008;
			StartRules[2].R4_FromYear = 2010;
			StartRules[2].R4_ToYear = 2011;

			EndRules.AddNew();
			EndRules.AddNew();
			EndRules[1].R4_FromYear = 2006;
			EndRules[1].R4_ToYear = 2008;
			EndRules[2].R4_FromYear = 2010;
			EndRules[2].R4_ToYear = 2011;

			timeZoneSet.RunPreSaveValidation();
			Assert(!timeZoneSet.HasDaylightSavingsInfo.HasErrors());

			StartRules[2].R4_ToYear = 2015;

			timeZoneSet.RunPreSaveValidation();
			Assert("Error should be displayed if difference in StartRules", timeZoneSet.HasDaylightSavingsInfo.HasError("All start rules must have a corresponding end rule."));

			StartRules[2].R4_ToYear = 2011;
			EndRules[2].R4_ToYear = 2015;

			timeZoneSet.RunPreSaveValidation();
			Assert("Error should be displayed if difference in EndRules", timeZoneSet.HasDaylightSavingsInfo.HasError("All start rules must have a corresponding end rule."));
		}

		#endregion

		#region StartAndEndRuleYearsCorrespond_HemisphereUnknown

		public void TestStartAndEndRuleYearsCorrespond_HemisphereUnknown()
		{
			StartRules.AddNew();
			StartRules.AddNew();
			StartRules[1].R4_FromYear = 2006;
			StartRules[1].R4_ToYear = 2008;
			StartRules[2].R4_FromYear = 2010;
			StartRules[2].R4_ToYear = 2011;

			EndRules.AddNew();
			EndRules.AddNew();
			EndRules[1].R4_FromYear = 2006;
			EndRules[1].R4_ToYear = 2008;
			EndRules[2].R4_FromYear = 2010;
			EndRules[2].R4_ToYear = 2011;

			TimeZoneSet.RunPreSaveValidation();
			Assert("If all start and end rules start and end on the same year, then there should be no error.", !TimeZoneSet.HasDaylightSavingsInfo.HasErrors());

			StartRules[0].R4_FromYear = 2000;
			StartRules[0].R4_ToYear = 2005;
			StartRules[1].R4_FromYear = 2006;
			StartRules[1].R4_ToYear = 2008;
			StartRules[2].R4_FromYear = 2009;
			StartRules[2].R4_ToYear = 2011;

			EndRules[0].R4_FromYear = 2001;
			EndRules[0].R4_ToYear = 2006;
			EndRules[1].R4_FromYear = 2007;
			EndRules[1].R4_ToYear = 2009;
			EndRules[2].R4_FromYear = 2010;
			EndRules[2].R4_ToYear = 2012;

			TimeZoneSet.RunPreSaveValidation();
			Assert("If there is only one year difference between the start and end year of rules, there should be no error.", !TimeZoneSet.HasDaylightSavingsInfo.HasErrors());

			StartRules[0].R4_FromYear = 2000;
			StartRules[0].R4_ToYear = 2005;
			StartRules[1].R4_FromYear = 2006;
			StartRules[1].R4_ToYear = 2008;
			StartRules[2].R4_FromYear = 2009;
			StartRules[2].R4_ToYear = 2017;

			EndRules[0].R4_FromYear = 2001;
			EndRules[0].R4_ToYear = 2006;
			EndRules[1].R4_FromYear = 2007;
			EndRules[1].R4_ToYear = 2009;
			EndRules[2].R4_FromYear = 2011;
			EndRules[2].R4_ToYear = 2012;

			TimeZoneSet.RunPreSaveValidation();
			Assert("Difference in number of years of StartRules and EndRules should be no greater than the minimum number of rules in either StartRules or EndRules", TimeZoneSet.HasDaylightSavingsInfo.HasError("All start rules must have a corresponding end rule."));

			StartRules[2].R4_FromYear = 2010;
			StartRules[2].R4_ToYear = 2011;
			EndRules[2].R4_ToYear = 2020;

			TimeZoneSet.RunPreSaveValidation();
			Assert("Difference in number of years of StartRules and EndRules should be no greater than the minimum number of rules in either StartRules or EndRules", TimeZoneSet.HasDaylightSavingsInfo.HasError("All start rules must have a corresponding end rule."));

			StartRules.DeleteAll();
			StartRules.AddNew();
			StartRules[0].R4_FromYear = 2000;
			StartRules[0].R4_ToYear = 2015;

			EndRules[0].R4_FromYear = 2001;
			EndRules[0].R4_ToYear = 2006;
			EndRules[1].R4_FromYear = 2007;
			EndRules[1].R4_ToYear = 2012;
			EndRules[2].R4_FromYear = 2013;
			EndRules[2].R4_ToYear = 2016;

			TimeZoneSet.RunPreSaveValidation();
			Assert("Difference in number of years of StarRules and EndRules should be no greater than the minimum number of rules in either StartRules or EndRules", !TimeZoneSet.HasDaylightSavingsInfo.HasErrors());

			EndRules.DeleteAll();

			StartRules.AddNew();
			StartRules.AddNew();
			StartRules[0].R4_FromYear = 2000;
			StartRules[0].R4_ToYear = 2005;
			StartRules[1].R4_FromYear = 2006;
			StartRules[1].R4_ToYear = 2009;
			StartRules[2].R4_FromYear = 2010;
			StartRules[2].R4_ToYear = 2020;

			EndRules.AddNew();
			EndRules[0].R4_FromYear = 2001;
			EndRules[0].R4_ToYear = 2021;

			TimeZoneSet.RunPreSaveValidation();
			Assert("Difference in number of years of StartRules and EndRules should be no greater than the minimum number of rules in either StartRules or EndRules", !TimeZoneSet.HasDaylightSavingsInfo.HasErrors());

			StartRules.DeleteAll();
			EndRules.DeleteAll();

			StartRules.AddNew();
			EndRules.AddNew();

			StartRules[0].R4_FromYear = 2000;
			StartRules[0].R4_ToYear = 0;
			EndRules[0].R4_FromYear = 2002;
			EndRules[0].R4_ToYear = 0;

			TimeZoneSet.RunPreSaveValidation();
			Assert("Difference in number of years of StartRules and EndRules should be no greater than the minimum number of rules in either StartRules or EndRules", TimeZoneSet.HasDaylightSavingsInfo.HasErrors());

			StartRules.DeleteAll();
			EndRules.DeleteAll();

			StartRules.AddNew();
			StartRules.AddNew();
			StartRules.AddNew();
			EndRules.AddNew();
			EndRules.AddNew();
			EndRules.AddNew();

			StartRules[0].R4_FromYear = 2000;
			StartRules[0].R4_ToYear = 2005;
			StartRules[1].R4_FromYear = 2008;
			StartRules[1].R4_ToYear = 2010;
			StartRules[2].R4_FromYear = 2012;
			StartRules[2].R4_ToYear = 0;

			EndRules[0].R4_FromYear = 2001;
			EndRules[0].R4_ToYear = 2006;
			EndRules[1].R4_FromYear = 2009;
			EndRules[1].R4_ToYear = 2011;
			EndRules[2].R4_FromYear = 2013;
			EndRules[2].R4_ToYear = 0;

			TimeZoneSet.RunPreSaveValidation();
			Assert("Difference in number of years of StartRules and EndRules should be no greater than the minimum number of rules in either StartRules or EndRules", !TimeZoneSet.HasDaylightSavingsInfo.HasErrors());

			EndRules[2].R4_FromYear = 2014;
			TimeZoneSet.RunPreSaveValidation();
			Assert("Difference in number of years of StartRules and EndRules should be no greater than the minimum number of rules in either StartRules or EndRules", TimeZoneSet.HasDaylightSavingsInfo.HasErrors());

			EndRules[2].R4_FromYear = 2013;
			EndRules[2].R4_ToYear = 2013;
			Assert("Difference in number of years of StartRules and EndRules should be no greater than the minimum number of rules in either StartRules or EndRules", TimeZoneSet.HasDaylightSavingsInfo.HasErrors());

			StartRules.DeleteAll();
			EndRules.DeleteAll();

			StartRules.AddNew();
			StartRules.AddNew();
			EndRules.AddNew();
			EndRules.AddNew();
			EndRules.AddNew();

			StartRules[0].R4_FromYear = 2000;
			StartRules[0].R4_ToYear = 2000;
			StartRules[1].R4_FromYear = 2001;
			StartRules[1].R4_ToYear = 0;

			EndRules[0].R4_FromYear = 2000;
			EndRules[0].R4_ToYear = 2005;
			EndRules[1].R4_FromYear = 2006;
			EndRules[1].R4_ToYear = 2006;
			EndRules[2].R4_FromYear = 2007;
			EndRules[2].R4_ToYear = 0;

			TimeZoneSet.RunPreSaveValidation();
			Assert("There should be no error for the entered rules.", !TimeZoneSet.HasDaylightSavingsInfo.HasErrors());
		}

		#endregion

		#region CheckToYearSetToZero

		public void TestToYearSetToZero()
		{
			StartRules.AddNew();
			StartRules[1].R4_FromYear = 2006;
			StartRules[1].R4_ToYear = 0;

			EndRules.AddNew();
			EndRules[1].R4_FromYear = 2006;
			EndRules[1].R4_ToYear = 0;

			// Set mandatory fields so it can be saved
			StartRules[0].R4_DaylightSavingDate = ZDateTime.Now;
			EndRules[0].R4_DaylightSavingDate = ZDateTime.Now;
			StartRules[1].R4_DaylightSavingDate = ZDateTime.Now;
			EndRules[1].R4_DaylightSavingDate = ZDateTime.Now;

			Factory.Save();
			Assert("Setting 0 as the ToYear should be a valid action.", !TimeZoneSet.DaylightSavingZone.StartDateRules[0].R4_ToYearInfo.HasErrors());
			Assert("Setting 0 as the ToYear should be a valid action.", !TimeZoneSet.DaylightSavingZone.EndDateRules[0].R4_ToYearInfo.HasErrors());
		}

		#endregion

		#region CheckGetAllYearsInBetween_SameYear

		public void TestCheckGetAllYearsInBetween_SameYear()
		{
			StartRules.AddNew();
			StartRules[1].R4_FromYear = 2006;
			StartRules[1].R4_ToYear = 2006;

			EndRules.AddNew();
			EndRules[1].R4_FromYear = 2006;
			EndRules[1].R4_ToYear = 2006;

			// Set mandatory fields so it can be saved
			StartRules[0].R4_DaylightSavingDate = ZDateTime.Now;
			EndRules[0].R4_DaylightSavingDate = ZDateTime.Now;
			StartRules[1].R4_DaylightSavingDate = ZDateTime.Now;
			EndRules[1].R4_DaylightSavingDate = ZDateTime.Now;

			Factory.Save();
			Assert("Setting the same year for the FromYear and the ToYear should be a valid action.", !TimeZoneSet.DaylightSavingZone.StartDateRules[0].R4_ToYearInfo.HasErrors());
			Assert("Setting the same year for the FromYear and the ToYear should be a valid action.", !TimeZoneSet.DaylightSavingZone.EndDateRules[0].R4_ToYearInfo.HasErrors());
		}

		#endregion

		#region Implementation

		RefTimeZoneSet TimeZoneSet;
		DaylightSavingTimeZone DaylightSavingZone;
		RefTimeZoneStartRuleCollection StartRules;
		RefTimeZoneEndRuleCollection EndRules;

		protected override void SetUp()
		{
			base.SetUp();

			TimeZoneSet = Factory.New<RefTimeZoneSet>();
			TimeZoneSet.HasDaylightSavings = true;
			DaylightSavingZone = TimeZoneSet.DaylightSavingZone;
			StartRules = DaylightSavingZone.StartDateRules;
			EndRules = DaylightSavingZone.EndDateRules;
			StartRules.AddNew();
			EndRules.AddNew();

			StartRules[0].R4_FromYear = 2000;
			StartRules[0].R4_ToYear = 2005;
			EndRules[0].R4_FromYear = 2000;
			EndRules[0].R4_ToYear = 2005;
		}

		#endregion
	}
}
