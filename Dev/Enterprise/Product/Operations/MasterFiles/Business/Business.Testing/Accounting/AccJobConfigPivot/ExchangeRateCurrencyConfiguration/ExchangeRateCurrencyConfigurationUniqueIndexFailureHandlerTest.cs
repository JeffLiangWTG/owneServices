using System.Linq;
using CargoWise.EntityFramework.Testing;
using static Enterprise.MasterFiles.Business.Testing.OrgCompanyDataTest;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.AccJobConfigPivot.ExchangeRateCurrencyConfiguration
{
	sealed class ExchangeRateCurrencyConfigurationUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestHandledUniqueIndexNames()
		{
			var handler = new ExchangeRateCurrencyConfigurationUniqueIndexFailureHandler();
			AssertEquals(1, handler.HandledUniqueIndexNames.Count());
			AssertEquals("NR_UC__vw_ExchangeRateCurrencyConfiguration", handler.HandledUniqueIndexNames.First());
		}

		public void TestNotifyUserAndAttemptToResolve()
		{
			var handler = new ExchangeRateCurrencyConfigurationUniqueIndexFailureHandler();
			var notifier = new NotificationHandlerForTest();

			handler.NotifyUserAndAttemptToResolve(notifier, "XXX");

			AssertEquals(0, notifier.ReportErrorCount);

			handler.NotifyUserAndAttemptToResolve(notifier, "NR_UC__vw_ExchangeRateCurrencyConfiguration");

			AssertEquals(1, notifier.ReportErrorCount);
			AssertContains(@"The same Currency Code already exists in the Job Billing Exchange Rate configuration.
Please check values for each parameter, make sure the currency lists don't overlap when you save multiple Job Exchange Rate Configurations with the same attributes (job type, transport mode, ledger, direction, and currency type).", notifier.Message);
		}
	}
}
