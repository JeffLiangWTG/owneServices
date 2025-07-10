using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MaximumAllowedTransactionAmountValidationTest : TestCaseWithFactory
	{
		public void TestValidateMaximumAllowedHeaderAmount()
		{
			var maximumAllowedTransactionAmount = new MaximumAllowedTransactionAmount(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals(ZDecimal.Zero, maximumAllowedTransactionAmount.MaximumAllowedHeaderAmount);
			maximumAllowedTransactionAmount.RunPreSaveValidation();
			AssertHasError(maximumAllowedTransactionAmount.MaximumAllowedHeaderAmountInfo, "Please enter a value.");

			maximumAllowedTransactionAmount.ShouldCheckMaximumSettingExceedSystemDefined = true;
			maximumAllowedTransactionAmount.MaximumAllowedHeaderAmount = 1000000000001M;
			AssertHasError(maximumAllowedTransactionAmount.MaximumAllowedHeaderAmountInfo, "The maximum allowed header amount must not be more than 1000000000000.");

			maximumAllowedTransactionAmount.MaximumAllowedHeaderAmount = 1000000000000M;
			AssertNoErrors(maximumAllowedTransactionAmount.MaximumAllowedHeaderAmountInfo);
		}

		public void TestValidateMaximumAllowedLineAmount()
		{
			var maximumAllowedTransactionAmount = new MaximumAllowedTransactionAmount(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals(ZDecimal.Zero, maximumAllowedTransactionAmount.MaximumAllowedLineAmount);
			maximumAllowedTransactionAmount.RunPreSaveValidation();
			AssertHasError(maximumAllowedTransactionAmount.MaximumAllowedLineAmountInfo, "Please enter a value.");

			maximumAllowedTransactionAmount.ShouldCheckMaximumSettingExceedSystemDefined = true;
			maximumAllowedTransactionAmount.MaximumAllowedLineAmount = 100000000001M;
			AssertHasError(maximumAllowedTransactionAmount.MaximumAllowedLineAmountInfo, "The maximum allowed line amount must not be more than 100000000000.");

			maximumAllowedTransactionAmount.MaximumAllowedLineAmount = 100000000000M;
			AssertNoErrors(maximumAllowedTransactionAmount.MaximumAllowedLineAmountInfo);
		}
	}
}
