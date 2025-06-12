using System;
using System.Collections.ObjectModel;
#if NET48
using eServices.eHubDataModel.eHubTransactions;
#else
using eServices.eHubDataModel.eHubTransactionsCore;
#endif
using Common.Logging;

namespace eServices.eHubRoutingRuleEngine
{
	public interface IRule
	{
		FactCollection Facts { get; }
		string RuleID { get; }
		string ServiceName { get; }
		DateTime Timestamp { get; }
		ServiceProviderCollection ServiceProviders { get; }

		Collection<Result> Evaluate(eHubTransactionsContext context, IFactResolver[] factResolvers, ILog logger);
		Group FindGroupRule(string groupName);
	}
}