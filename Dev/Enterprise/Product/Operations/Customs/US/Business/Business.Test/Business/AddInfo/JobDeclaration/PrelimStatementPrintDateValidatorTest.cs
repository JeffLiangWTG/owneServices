using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PrelimStatementPrintDateValidatorTest : TestCaseWithFactory
	{
		public void TestDateAgainstPaymentType()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			var validator = new PrelimStatementPrintDateValidator();
			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday;
			validator.ValidatePreliminaryStatementPrintDate(reconDeclaration.US_PreliminaryStatementPrintDateInfo, reconDeclaration);
			AssertHasMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintNotRequired);
			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			validator.ValidatePreliminaryStatementPrintDate(reconDeclaration.US_PreliminaryStatementPrintDateInfo, reconDeclaration);
			AssertNoMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintNotRequired);
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			validator.ValidatePreliminaryStatementPrintDate(reconDeclaration.US_PreliminaryStatementPrintDateInfo, reconDeclaration);
			AssertHasMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateRequired);
			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday;
			validator.ValidatePreliminaryStatementPrintDate(reconDeclaration.US_PreliminaryStatementPrintDateInfo, reconDeclaration);
			AssertNoMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateRequired);
			if (CargoWise.Common.ErrorReporter.LastKeyReported == "Validation:US_PreliminaryStatementPrintDate")
			{
				CargoWise.Common.ErrorReporter.Clear();
			}
		}

		public void TestDateRange()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			var validator = new PrelimStatementPrintDateValidator();
			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.Today;
			AssertHasMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateShouldBeInTheFuture);
			reconDeclaration.MessageStatus = ReconMessageStatusList.Codes.ClearReconOriginal;
			AssertEquals("PreCondition:HasBeenLodgedAtCustoms", false, ((IPrelimStatementDetailsDefault)reconDeclaration).ShouldValidatePastDate);
			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.Today;
			AssertNoMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateShouldBeInTheFuture);
			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(91);
			AssertHasMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateMoreThan90DaysInTheFuture);
			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(90);
			AssertNoMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateMoreThan90DaysInTheFuture);
		}

		public void TestWeekendAndHoliday()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			var validator = new PrelimStatementPrintDateValidator();
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 1, 10); //Saturday
			validator.ValidatePreliminaryStatementPrintDate(reconDeclaration.US_PreliminaryStatementPrintDateInfo, reconDeclaration);
			AssertHasMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend);
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 1, 12); //Saturday
			validator.ValidatePreliminaryStatementPrintDate(reconDeclaration.US_PreliminaryStatementPrintDateInfo, reconDeclaration);
			AssertNoMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend);
			if (CargoWise.Common.ErrorReporter.LastKeyReported == "Validation:US_PreliminaryStatementPrintDate")
			{
				CargoWise.Common.ErrorReporter.Clear();
			}
		}
	}
}
