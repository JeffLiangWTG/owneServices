using System;
#if NET48
using eServices.eHubDataModel.eHubTransactions;
#else
using eServices.eHubDataModel.eHubTransactionsCore;
#endif

namespace eServices.eHubRoutingRuleEngine
{
	public interface IRoutingRuleFactory
	{
		IRule GetForReading(eHubClient client);
		IRule GetForReading(eHubClient client, TimeSpan ruleCheckInterval, TimeSpan noRuleCheckInterval);
		IRule GetForReading(string ruleId);
		IRule GetForReading(string ruleId, TimeSpan ruleCheckInterval, TimeSpan noRuleCheckInterval);
		IRule GetForEditing(string ruleId);
		IRule GetForEditing(eHubClient client);
	}
}