using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
#if NET48
using eServices.eHubDataModel.eHubTransactions;
#else
using eServices.eHubDataModel.eHubTransactionsCore;
#endif
using Common.Logging;

namespace eServices.eHubRoutingRuleEngine
{
	public sealed class Rule : Group, IRule
	{
		static Rule()
		{
			Database.SetInitializer<eHubTransactionsContext>(null);
		}

		public Rule(eHubTransactionsContext context, eHubClient client)
			: this(context, client, new eHubRoutingRule() { RR_Group_MatchMultiple = false })
		{
			if (client.eHubRoutingRule != null)
				throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Unable to create rule. Rule already exists for client ID '{0}'.", client.CC_ID));

			client.eHubRoutingRule = base.eHubRoutingRule;
		}

		public static Rule GetForReading(eHubClient client)
		{
			if (client == null) throw new ArgumentNullException("client");
			if (client.eHubRoutingRule == null)
				throw new InvalidOperationException(String.Format("Rule not found for client ID '{0}'.", client.CC_ID));

			var rule = new Rule(client, client.eHubRoutingRule);
			rule.SetRuleContexts();
			return rule;
		}

		public static Rule GetForEditing(eHubTransactionsContext context, eHubClient client)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (client == null) throw new ArgumentNullException("client");
			if (client.eHubRoutingRule == null)
				return null;

			return new Rule(context, client, client.eHubRoutingRule);
		}

		internal Rule(eHubTransactionsContext context, eHubClient client, eHubRoutingRule rule)
			: this(client, rule)
		{
			if (context == null) throw new ArgumentNullException("context");
			SetRuleContexts(context);
		}

		internal Rule(eHubClient client, eHubRoutingRule rule)
			: base(rule)
		{
			if (client == null) throw new ArgumentNullException("client");
			this.client = client;
			this.Facts = new FactCollection(rule);
			this.ServiceProviders = new ServiceProviderCollection(client);
			this.Timestamp = base.eHubRoutingRule.RR_LastUpdateUTC.GetValueOrDefault();
			base.rule = this;
		}

		internal void SetRuleContexts(eHubTransactionsContext persistingContext = null)
		{
			base.SetRuleContexts(this, persistingContext);
			this.Facts.SetRuleContexts(this, persistingContext);
			this.ServiceProviders.SetRuleContexts(this, persistingContext);
		}

		public static void Delete(eHubTransactionsContext context, eHubClient client)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (client == null) throw new ArgumentNullException("client");
			var rule = Rule.GetForEditing(context, client);
			if (rule == null)
				throw new InvalidOperationException(String.Format("Rule not found for client ID '{0}'.", client.CC_ID));

			rule.Delete();
		}

		internal override void Delete()
		{
			this.Facts.Clear();
			base.Delete();
		}

		public new Group FindGroupRule(string groupName)
		{
			foreach (var rule in this.SubRules)
			{
				var result = rule.FindGroupRule(groupName);
				if (result != null) return result;
			}
			if (this.FailedSubRule != null)
				return this.FailedSubRule.FindGroupRule(groupName);
			return null;
		}

		public Collection<Result> Evaluate(eHubTransactionsContext context, IFactResolver[] factResolvers, ILog logger)
		{
			try
			{
				logger.InfoFormat("Evaluating rule for client '{0}'", this.client.CC_ID);
				var rawFacts = this.Facts.Where(f => f.Type != "COMPUTED").Select(f => new Fact { Name = f.Name, Type = f.Type, Query = f.Query, Value = f.Value }).ToArray();
				var computeFacts = this.Facts.Where(f => f.Type == "COMPUTED").Select(f => new Fact { Name = f.Name, Type = f.Type, Query = f.Query, Value = f.Value, ComputeRule = f.ComputeRule }).ToArray();
				foreach (var resolver in factResolvers)
					resolver.Resolve(rawFacts);
				var resolvedFacts = rawFacts.ToDictionary<Fact, string, string>(f => "@" + f.Name, f => f.Value);
				if (computeFacts.Any())
				{
					if (logger.IsDebugEnabled)
						logger.DebugFormat("Raw facts: {0}", String.Join(", ", rawFacts.Select(f => String.Format("[{0}={1}]", "@" + f.Name, f.Value))));
					foreach (var fact in computeFacts)
					{
						fact.Value = fact.ComputeRule.Evaluate(context, resolvedFacts, logger).First().Value;
						resolvedFacts["@" + fact.Name] = fact.Value;
					}
					if (logger.IsDebugEnabled)
						logger.DebugFormat("Computed facts: {0}", String.Join(", ", computeFacts.Select(f => String.Format("[{0}={1}]", "@" + f.Name, f.Value))));
				}

				logger.InfoFormat("Resolved facts for evaluation: {0}", String.Join(", ", resolvedFacts.Select(f => String.Format("[{0}={1}]", f.Key, f.Value))));
				var routes = base.Evaluate(context, resolvedFacts, logger);
				logger.InfoFormat("Rule evaluation results = [ {0} ]", String.Join(", ", routes));
				logger.DebugFormat("Finished evaluating rule for client '{0}'", this.client.CC_ID);
				return routes;
			}
			catch (Exception ex)
			{
				logger.ErrorFormat("Exception evaluating rule for client ID '{0}'.", ex, this.client.CC_ID);
				throw new RoutingRuleException(String.Format("Routing Rule Engine: Exception evaluating rule for client '{0}' - {1}", this.client.CC_ID, ex.GetBaseException().Message), ex);
			}
		}

		public FactCollection Facts { get; private set; }
		public ServiceProviderCollection ServiceProviders { get; private set; }

		eHubClient client;
		public string RuleID { get { return client.CC_ID; } }
		public string ServiceName { get { return client.CC_FriendlyName; } }
		public DateTime Timestamp { get; }
	}
}
