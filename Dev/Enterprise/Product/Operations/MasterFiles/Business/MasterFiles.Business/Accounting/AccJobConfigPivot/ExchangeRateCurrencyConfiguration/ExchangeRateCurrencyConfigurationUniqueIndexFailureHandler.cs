using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ExchangeRateCurrencyConfigurationUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		public ExchangeRateCurrencyConfigurationUniqueIndexFailureHandler()
		{
		}

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get
			{
				yield return ExchangeRateCurrencyConfigurationIndexedViewName;
			}
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			if (indexName == ExchangeRateCurrencyConfigurationIndexedViewName)
			{
				notifier.ReportError(AccountingMasterFilesConstants.ValidationErrorMessages.DuplicateCurrencyCodeForJobBillingExRateConfig, Res.GetString("5BB0DE45-534B-4233-9403-5B679074908D", "Save Error"));
			}
		}

		const string ExchangeRateCurrencyConfigurationIndexedViewName = "NR_UC__vw_ExchangeRateCurrencyConfiguration";
	}
}
