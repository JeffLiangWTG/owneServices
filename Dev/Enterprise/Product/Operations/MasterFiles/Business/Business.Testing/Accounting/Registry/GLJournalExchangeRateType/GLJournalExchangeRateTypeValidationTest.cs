using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GLJournalExchangeRateTypeValidationTest : TestCaseWithFactory
	{
		public void TestValidateBalanceSheetAccountTypeExchangeRateType()
		{
			var gLJournalExchangeRate = new GLJournalExchangeRateType(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals(ZString.Empty, gLJournalExchangeRate.BalanceSheetAccountTypeExchangeRateType);
			gLJournalExchangeRate.RunPreSaveValidation();
			AssertHasError(gLJournalExchangeRate.BalanceSheetAccountTypeExchangeRateTypeInfo, "Please enter a value.");

			gLJournalExchangeRate.BalanceSheetAccountTypeExchangeRateType = "XXX";
			AssertHasError(gLJournalExchangeRate.BalanceSheetAccountTypeExchangeRateTypeInfo, "Enter a valid selection.");

			gLJournalExchangeRate.BalanceSheetAccountTypeExchangeRateType = "PER";
			AssertNoErrors(gLJournalExchangeRate.BalanceSheetAccountTypeExchangeRateTypeInfo);
		}

		public void TestValidateProfitAndLossAccountTypeExchangeRateType()
		{
			var gLJournalExchangeRate = new GLJournalExchangeRateType(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals(ZString.Empty, gLJournalExchangeRate.ProfitAndLossAccountTypeExchangeRateType);
			gLJournalExchangeRate.RunPreSaveValidation();
			AssertHasError(gLJournalExchangeRate.ProfitAndLossAccountTypeExchangeRateTypeInfo, "Please enter a value.");

			gLJournalExchangeRate.ProfitAndLossAccountTypeExchangeRateType = "XXX";
			AssertHasError(gLJournalExchangeRate.ProfitAndLossAccountTypeExchangeRateTypeInfo, "Enter a valid selection.");

			gLJournalExchangeRate.ProfitAndLossAccountTypeExchangeRateType = "PER";
			AssertNoErrors(gLJournalExchangeRate.ProfitAndLossAccountTypeExchangeRateTypeInfo);
		}
	}
}
