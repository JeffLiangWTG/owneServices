using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestAccTransactionMatchLinkValidation : BusinessObjectValidationTestCase
	{
		public void TestlValidationNotificationsRemovedBeforeValidateAll()
		{
			AccTransactionMatchLink testLink = Factory.New<AccTransactionMatchLink>();
			testLink.AP_Amount = 10m;
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			testLink.AP_AH = header.PK;
			testLink.AP_MatchGroupNum = "1234567890";

			testLink.Validation.ValidateAll();
			AssertNoRowErrors(testLink);

			string expectedError = "ExpectedError";
			testLink.AddRowError(expectedError);
			AssertHasRowError(testLink, expectedError);

			testLink.Validation.ValidateAll();
			AssertNoRowErrors(testLink);
		}

		public void TestMatchDateIsNotEmptyValidationSuspender()
		{
			var testLink = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			testLink.AP_Amount = 10m;
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			testLink.AP_MatchDate = ZDateTime.Empty;
			testLink.AP_AH = header.PK;
			testLink.AP_MatchGroupNum = "1234567890";

			testLink.RunPreSaveValidation();
			AssertHasError(testLink.AP_MatchDateInfo, "Please enter a Match Date.");

			using (testLink.MatchDateIsNotEmptyValidationSuspender.GetSuspender())
			{
				testLink.RunPreSaveValidation();
				AssertNoError(testLink.AP_MatchDateInfo, "Please enter a Match Date.");
			}
		}

		public void TestShouldValidateFKToCancelledRecord()
		{
			AssertShouldValidateFKToCancelledRecord(TransactionTypes.Overpayment, LedgerTypes.AccountsPayable);
			AssertShouldValidateFKToCancelledRecord(TransactionTypes.Discount, LedgerTypes.AccountsPayable);
			AssertShouldValidateFKToCancelledRecord(TransactionTypes.ExchangeDifference, LedgerTypes.AccountsPayable);
			AssertShouldValidateFKToCancelledRecord(TransactionTypes.Journal, LedgerTypes.AccountsPayable);

			AssertShouldValidateFKToCancelledRecord(TransactionTypes.Overpayment, LedgerTypes.AccountsReceivable);
			AssertShouldValidateFKToCancelledRecord(TransactionTypes.Discount, LedgerTypes.AccountsReceivable);
			AssertShouldValidateFKToCancelledRecord(TransactionTypes.ExchangeDifference, LedgerTypes.AccountsReceivable);
			AssertShouldValidateFKToCancelledRecord(TransactionTypes.Journal, LedgerTypes.AccountsReceivable);
		}

		void AssertShouldValidateFKToCancelledRecord(string transactionType, string ledgerType)
		{
			var matchLink = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLink.AP_Amount = 10m;
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_TransactionType = transactionType;
			header.AH_Ledger = ledgerType;
			header.AH_IsCancelled = true;

			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "12345";
			matchLink.AP_AH = header.PK;
			Assert("Match link should not contain this error.", !matchLink.Notifications.Contains("Error - AP_AH: This Transaction Header is inactive - it may not be used."));
		}
	}
}
