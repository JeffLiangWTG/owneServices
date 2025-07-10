using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestGlbHolidayValidation : BusinessObjectValidationTestCase
	{
		#region ValidateGH_HolidayName

		public void TestValidateGH_HolidayName()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			GlbHoliday holiday1 = branch.GlbHolidays.AddNew();

			holiday1.GH_HolidayName = "";
			Assert("Error expected - Holiday Name is required", holiday1.GH_HolidayNameInfo.HasErrors());
			holiday1.GH_HolidayName = "ABC";
			Assert("Error expected - The length of Holiday Name should be at least 4", holiday1.GH_HolidayNameInfo.HasErrors());
			holiday1.GH_HolidayName = "ABCD";
			Assert("No error expected", !holiday1.GH_HolidayNameInfo.HasErrors());

			GlbHoliday holiday2 = branch.GlbHolidays.AddNew();
			holiday2.GH_HolidayName = "ABCD";
			holiday1.Validation.ValidateGH_HolidayName();
			Assert("No error expected", !holiday1.GH_HolidayNameInfo.HasErrors());
			Assert("No error expected", !holiday2.GH_HolidayNameInfo.HasErrors());

			holiday2.GH_HolidayName = "ABCDE";
			holiday1.Validation.ValidateGH_HolidayName();
			Assert("No error expected", !holiday1.GH_HolidayNameInfo.HasErrors());
			Assert("No error expected", !holiday2.GH_HolidayNameInfo.HasErrors());
		}

		#endregion

		#region ValidateGH_RecurrType

		public void TestValidateGH_RecurrType()
		{
			Holiday.GH_RecurrType = "";
			Assert("Error expected - Recurr Type is required", Holiday.GH_RecurrTypeInfo.HasErrors());
			Holiday.GH_RecurrType = "AAA";
			Assert("Error expected - Recurr Type is not valid", Holiday.GH_RecurrTypeInfo.HasErrors());
			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			Assert("No error expected", !Holiday.GH_RecurrTypeInfo.HasErrors());
		}

		public void TestRecurringLeapDayHoliday()
		{
			Holiday.GH_Date = new ZDate(2024, 2, 29);
			Holiday.GH_Recurring = true;
			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;

			AssertHasError(Holiday.GH_RecurringInfo, "Leap days cannot be recurring holidays");
		}

		#endregion

		#region ValidateGH_Date

		public void TestValidateGH_Date()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			GlbHoliday holiday1 = branch.GlbHolidays.AddNew();

			Assert("Holiday Date should not be readonly", !holiday1.GH_DateInfo.ReadOnly);
			holiday1.GH_Date = ZDateTime.Empty;
			Assert("Error expected - Holiday Date is required", holiday1.GH_DateInfo.HasErrors());
			holiday1.GH_Date = ZDateTime.Invalid;
			Assert("Error expected - Holiday Date is not valid", holiday1.GH_DateInfo.HasErrors());
			holiday1.GH_Date = ZDateTime.Now;
			Assert("No error expected", !holiday1.GH_DateInfo.HasErrors());

			holiday1.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.First;
			Assert("No error expected", !holiday1.GH_DateInfo.HasErrors());
			Assert("Holiday Date should be readonly", holiday1.GH_DateInfo.ReadOnly);
			AssertEquals("Holiday Date should be empty", ZDateTime.Empty, holiday1.GH_Date);

			holiday1.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			GlbHoliday holiday2 = branch.GlbHolidays.AddNew();
			holiday1.GH_Date = ZDateTime.Now;
			holiday2.GH_Date = holiday1.GH_Date;
			holiday1.Validation.ValidateGH_Date();
			Assert("Error expected - Holiday Date should be unique for a branch", holiday1.GH_DateInfo.HasErrors());
			Assert("Error expected - Holiday Date should be unique for a branch", holiday2.GH_DateInfo.HasErrors());

			holiday2.GH_Date = ZDateTime.Now.AddDays(1);
			holiday1.Validation.ValidateGH_Date();
			Assert("No error expected", !holiday1.GH_DateInfo.HasErrors());
			Assert("No error expected", !holiday2.GH_DateInfo.HasErrors());
		}

		#endregion

		#region ValidateGH_RecurrMonth

		public void TestValidateGH_RecurrMonth()
		{
			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.First;
			Assert("Recurr Month should not be readonly", !Holiday.GH_RecurrMonthInfo.ReadOnly);
			Holiday.GH_RecurrMonth = "";
			Assert("Error expected - Recurr Month is required", Holiday.GH_RecurrMonthInfo.HasErrors());
			Holiday.GH_RecurrMonth = "AAA";
			Assert("Error expected - Recurr Month is not valid", Holiday.GH_RecurrMonthInfo.HasErrors());
			Holiday.GH_RecurrMonth = Holiday.Months[0].Code;
			Assert("No error expected", !Holiday.GH_RecurrMonthInfo.HasErrors());

			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			Assert("Recurr Month should be readonly", Holiday.GH_RecurrMonthInfo.ReadOnly);
			AssertEquals("Recurr Month should be empty", "", Holiday.GH_RecurrMonth);

			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			Holiday.GH_RecurrMonth = "";
			Assert("No error expected", !Holiday.GH_RecurrMonthInfo.HasErrors());
		}

		#endregion

		#region ValidateGH_RecurrDay

		public void TestValidateGH_RecurrDay()
		{
			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.First;
			Assert("Recurr Day should not be readonly", !Holiday.GH_RecurrDayInfo.ReadOnly);
			Holiday.GH_RecurrDay = "";
			Assert("Error expected - Recurr Day is required", Holiday.GH_RecurrDayInfo.HasErrors());
			Holiday.GH_RecurrDay = "AAA";
			Assert("Error expected - Recurr Day is not valid", Holiday.GH_RecurrDayInfo.HasErrors());
			Holiday.GH_RecurrDay = Holiday.WeekDays[0].Code;
			Assert("No error expected", !Holiday.GH_RecurrDayInfo.HasErrors());

			Holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			Assert("Recurr Day should be readonly", Holiday.GH_RecurrDayInfo.ReadOnly);
			AssertEquals("Recurr Day should be empty", "", Holiday.GH_RecurrDay);
		}

		#endregion

		#region TestImplementation

		GlbHoliday Holiday => fHoliday ??= Factory.New<GlbHoliday>();
		GlbHoliday fHoliday;

		#endregion

	}
}
