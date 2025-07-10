using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class WeekendsAndHolidaysValidatorTest : TestCaseWithFactory
	{
		public void TestCheckWeekendsAndHolidays()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_PaymentType = "2";
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2006, 1, 14); // Saturday
			AssertHasMessageErrorContaining(declaration.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend);
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2006, 1, 10); // Tuesday
			AssertNoMessageErrorContaining(declaration.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend);
			//holidays (2010, 02, 15)
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2010, 02, 15);
			AssertHasMessageErrorContaining(declaration.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday);
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2010, 02, 16);
			AssertNoMessageErrorContaining(declaration.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday);
		}

		public void TestCheckFederalCustomsHolidays()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_PaymentType = "2";
			//holidays (2009, 07, 03)
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 07, 03); // Friday - Independace Day Holiday 2009
			AssertHasMessageErrorContaining(declaration.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday);
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 07, 04); // Saturday
			AssertNoMessageErrorContaining(declaration.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday);
		}
	}
}
