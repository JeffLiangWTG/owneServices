using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefTimeZoneValidationTest : BusinessObjectValidationTestCase
	{
		// Note that StandardTimeZone is used rather than RefTimeZone because RefTimeZone is abstract
		// Additionally DaylightSavingTimeZone and StandardZone have identical validation 

		#region CivilianTimeZoneCodeValidation

		public void TestCivilianTimeZoneCodeValidation()
		{
			StandardTimeZone timeZone = Factory.New<StandardTimeZone>();

			timeZone.R2_CivilianTimeZoneCode = "Hello";
			AssertNoErrors(timeZone.R2_CivilianTimeZoneCodeInfo);

			timeZone.R2_CivilianTimeZoneCode = "";
			AssertHasErrors(timeZone.R2_CivilianTimeZoneCodeInfo);
		}

		#endregion

		#region MilitaryTimeZoneCodeValidation

		public void TestMilitaryTimeZoneCodeValidation()
		{
			StandardTimeZone timeZone = Factory.New<StandardTimeZone>();

			timeZone.R2_MilitaryTimeZoneCode = "HE";
			AssertNoErrors(timeZone.R2_MilitaryTimeZoneCodeInfo);

			timeZone.R2_MilitaryTimeZoneCode = "";
			AssertNoErrors(timeZone.R2_MilitaryTimeZoneCodeInfo);
		}

		#endregion

		#region CivilianTimeZoneFullNameValidation

		public void TestCivilianTimeZoneFullNameValidation()
		{
			StandardTimeZone timeZone = Factory.New<StandardTimeZone>();

			timeZone.R2_CivilianTimeZoneFullName = "Hello";
			AssertNoErrors(timeZone.R2_CivilianTimeZoneFullNameInfo);

			timeZone.R2_CivilianTimeZoneFullName = "";
			AssertNoErrors(timeZone.R2_CivilianTimeZoneFullNameInfo);
		}

		#endregion

		#region UTCOffsetValidation

		public void TestUTCOffsetValidation()
		{
			StandardTimeZone timeZone = Factory.New<StandardTimeZone>();

			timeZone.R2_OffsetMinutesFromUTC = 900;
			AssertHasErrors(timeZone.R2_OffsetMinutesFromUTCInfo);

			timeZone.R2_OffsetMinutesFromUTC = -780;
			AssertHasErrors(timeZone.R2_OffsetMinutesFromUTCInfo);

			timeZone.R2_OffsetMinutesFromUTC = 840;
			AssertNoErrors(timeZone.R2_OffsetMinutesFromUTCInfo);

			timeZone.R2_OffsetMinutesFromUTC = -720;
			AssertNoErrors(timeZone.R2_OffsetMinutesFromUTCInfo);

			timeZone.R2_OffsetMinutesFromUTC = 0;
			AssertNoErrors(timeZone.R2_OffsetMinutesFromUTCInfo);
		}

		#endregion
	}
}
