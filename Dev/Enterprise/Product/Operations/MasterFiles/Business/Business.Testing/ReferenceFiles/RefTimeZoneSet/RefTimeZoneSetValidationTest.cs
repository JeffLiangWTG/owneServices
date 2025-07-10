using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefTimeZoneSetValidationTest : BusinessObjectValidationTestCase
	{
		#region TimeZoneSetName

		public void TestTimeZoneSetName()
		{
			RefTimeZoneSet timeZoneSet = Factory.New<RefTimeZoneSet>();

			timeZoneSet.R3_TimeZoneSetName = "";
			AssertHasErrors("TimeZoneSetName should not be empty", timeZoneSet.R3_TimeZoneSetNameInfo);

			timeZoneSet.R3_TimeZoneSetName = "Eastern Standard Time";
			AssertNoErrors("Setting a value to TimeZoneSetName should not give errors", timeZoneSet.R3_TimeZoneSetNameInfo);
		}

		#endregion

		#region CheckHasDaylightSavings

		public void TestCheckHasDaylightSavings()
		{
			RefTimeZoneSet timeZoneSet = Factory.New<RefTimeZoneSet>();
			timeZoneSet.HasDaylightSavings = true;
			RefTimeZoneStartRuleCollection startRules = timeZoneSet.DaylightSavingZone.StartDateRules;
			RefTimeZoneEndRuleCollection endRules = timeZoneSet.DaylightSavingZone.EndDateRules;

			startRules.DeleteAll();
			timeZoneSet.RunPreSaveValidation();
			AssertHasErrors("At least one start rule should exist", timeZoneSet.HasDaylightSavingsInfo);

			startRules.AddNew();
			endRules.DeleteAll();
			timeZoneSet.RunPreSaveValidation();
			AssertHasErrors("At least one end rule should exist", timeZoneSet.HasDaylightSavingsInfo);

			startRules.DeleteAll();
			timeZoneSet.RunPreSaveValidation();
			AssertHasErrors("At least one start rule and one end rule should exist", timeZoneSet.HasDaylightSavingsInfo);

			startRules.AddNew();
			endRules.AddNew();

			startRules[0].R4_FromYear = 2005;
			startRules[0].R4_ToYear = 2005;
			endRules[0].R4_FromYear = 2005;
			endRules[0].R4_ToYear = 2007;

			AssertHasErrors("If Start and End rules don't correspond, HasDaylightSavings should have an error.", timeZoneSet.HasDaylightSavingsInfo);

			startRules[0].R4_FromYear = 1;
			startRules[0].R4_ToYear = 1;
			endRules[0].R4_FromYear = 1;
			endRules[0].R4_ToYear = 1;

			AssertHasErrors("If Validation errors exist on R4_FromYear or R4_ToYear, HasDaylightSavings should have an error.", timeZoneSet.HasDaylightSavingsInfo);

			startRules.AddNew();
			startRules[0].R4_FromYear = 2005;
			startRules[0].R4_ToYear = 2009;
			startRules[1].R4_FromYear = 2008;
			startRules[1].R4_ToYear = 2010;

			endRules.AddNew();
			endRules[0].R4_FromYear = 2005;
			endRules[0].R4_ToYear = 2006;
			endRules[1].R4_FromYear = 2007;
			endRules[1].R4_ToYear = 2010;

			AssertHasErrors("If Start rules contains overlaps, HasDaylightSavings should have an error.", timeZoneSet.HasDaylightSavingsInfo);

			startRules[0].R4_FromYear = 2005;
			startRules[0].R4_ToYear = 2007;
			startRules[1].R4_FromYear = 2008;
			startRules[1].R4_ToYear = 2010;

			endRules[0].R4_FromYear = 2005;
			endRules[0].R4_ToYear = 2009;
			endRules[1].R4_FromYear = 2007;
			endRules[1].R4_ToYear = 2010;

			AssertHasErrors("If End rules contains overlaps, HasDaylightSavings should have an error.", timeZoneSet.HasDaylightSavingsInfo);
		}

		#endregion

	}
}
