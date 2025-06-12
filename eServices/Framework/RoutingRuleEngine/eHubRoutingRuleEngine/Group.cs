using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
#if NET48
using eServices.eHubDataModel.eHubTransactions;
#else
using eServices.eHubDataModel.eHubTransactionsCore;
#endif
using Common.Logging;

namespace eServices.eHubRoutingRuleEngine
{
	public class Group : Criterion
	{
		public Group()
			: this(new eHubRoutingRule() { RR_Group_MatchMultiple = false })
		{
		}

		public Group(IEnumerable<Criterion> subRules)
			: this(new eHubRoutingRule() { RR_Group_MatchMultiple = false })
		{
			this.SubRules.Load(subRules);
		}

		internal Group(eHubRoutingRule rule)
			: base(rule)
		{
			this.SubRules = new SubRuleCollection(rule);
		}

		internal override void Delete()
		{
			this.SubRules.Clear();
			base.Delete();
		}

		internal override void SetRuleContexts(Rule rule, eHubTransactionsContext persistingContext)
		{
			this.SubRules.SetRuleContexts(rule, persistingContext);
			base.SetRuleContexts(rule, persistingContext);
		}

		internal override Group FindGroupRule(string groupName)
		{
			if (this.GroupName == groupName) return this;
			foreach (var rule in this.SubRules)
			{
				var result = rule.FindGroupRule(groupName);
				if (result != null) return result;
			}
			if (this.FailedSubRule != null)
				return this.FailedSubRule.FindGroupRule(groupName);
			return null;
		}

		internal override Collection<Result> Evaluate(eHubTransactionsContext context, Dictionary<string, string> facts, ILog logger)
		{
			try
			{
				logger.DebugFormat("Evaluating group rule.");
				var results = new Collection<Result>();

				if (this.MatchMultiple)
				{
					var concurrentResults = new ConcurrentBag<Result>();
					var evalActions = this.SubRules.Select((r, i) => new Action(() =>
					{
						foreach (var result in r.Evaluate(context, facts, logger))
							concurrentResults.Add(result);
					}));
					Parallel.Invoke(evalActions.ToArray());
					foreach (var result in concurrentResults)
						results.Add(result);
				}
				else
				{
					foreach (var rule in this.SubRules)
					{
						var result = rule.Evaluate(context, facts, logger).FirstOrDefault();
						if (result != null)
						{
							results.Add(result);
							break;
						}
					}
				}

				if (results.Count == 0 && this.FailedSubRule != null)
					results = this.FailedSubRule.Evaluate(context, facts, logger);

				string resultValue = null;
				if (results.Count == 0 && this.Value != null)
				{
					if (this.Value.StartsWith("@") && facts.ContainsKey(this.Value))
						resultValue = facts[this.Value];
					else
						resultValue = this.Value;
				}

				if (results.Count == 0 && base.eHubRoutingRule.eHubServiceProvider != null)
				{
					logger.DebugFormat("Evaluating provider '{0}'", base.eHubRoutingRule.eHubServiceProvider.eHubClient_Provider.CC_ID);
					var providerResult = base.EvaluateProvider(context, facts, logger);
					if (logger.IsDebugEnabled)
						logger.DebugFormat("Provider evaluation results = [ {0} ]", String.Join(", ", providerResult));
					if (providerResult.Count > 0)
						foreach (var result in providerResult)
						{
							result.Value = result.Value ?? resultValue;
							results.Add(result);
						}
				}

				if (results.Count == 0 && (base.Recipient != null || !String.IsNullOrWhiteSpace(base.ErrorCode) || !String.IsNullOrWhiteSpace(base.ErrorDescription) || resultValue != null))
					results.Add(new Result(base.Recipient, base.ErrorCode, base.ErrorDescription, resultValue));

				var nilResults = results.Where(x => x.Nil).ToArray();
				foreach (var result in nilResults)
				{
					results.Remove(result);
				}

				if (logger.IsDebugEnabled)
					logger.DebugFormat("Group rule evaluated results = [ {0} ]", String.Join(", ", results));

				return results;
			}
			catch (Exception ex)
			{
				throw new RoutingRuleException(string.Format("Current row having init error: {0}\r\n", CurrentRowData), ex);
			}
		}

		public string GroupName
		{
			get { return this.eHubRoutingRule.RR_Group_Name; }
			set { this.eHubRoutingRule.RR_Group_Name = value; }
		}

		public bool MatchMultiple
		{
			get { return base.eHubRoutingRule.RR_Group_MatchMultiple.Value; }
			set { base.eHubRoutingRule.RR_Group_MatchMultiple = value; }
		}

		public ServiceProvider DefaultServiceProvider
		{
			get { return base.ServiceProvider; }
			set { base.ServiceProvider = value; }
		}

		public eHubClient DefaultRecipient
		{
			get { return base.Recipient; }
			set { base.Recipient = value; }
		}

		public SubRuleCollection SubRules { get; private set; }
	}
}
