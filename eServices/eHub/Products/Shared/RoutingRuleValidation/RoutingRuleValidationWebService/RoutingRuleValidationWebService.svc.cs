using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Common.Logging;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;
using eServices.Routing.OceanCarrierMessaging;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService
{
	public class RoutingRuleValidationWebService : IRoutingRuleValidationWebService
	{
		private readonly eHubTransactionsContext Context;
		private readonly ILog Logger;
		const string Client_OCM_Copying = "OCM_MutipleRecipientsCopying";
		private readonly IRoutingRuleFactory RuleFactory;

		public RoutingRuleValidationWebService() : this(new eHubTransactionsContext(), LogManager.GetLogger(typeof(RoutingRuleValidationWebService)))
		{
		}

		public RoutingRuleValidationWebService(eHubTransactionsContext context, ILog logger) : this(context, logger, new RoutingRuleFactory(context, logger))
		{
		}

		public RoutingRuleValidationWebService(eHubTransactionsContext context, ILog logger, IRoutingRuleFactory routingRuleFactory)
		{
			Context = context;
			Logger = logger;
			RuleFactory = routingRuleFactory;
		}

		public CargoWise.eHub.Shared.RoutingRuleEngine.Result[] Evaluate(RoutingEvaluationGenericInput routingEvaluationInput)
		{
			Logger.InfoFormat("Starting evaluation in WS for RuleId [{0}], propertyFacts [{1}]", routingEvaluationInput.RuleId, string.Join(", ", routingEvaluationInput.PropertyFacts.Select(f =>
				$"[{"@" + f.Key}={f.Value}]")));
			var eHubClient = GetEHubClient(routingEvaluationInput.RuleId);

			var rule = RuleFactory.GetForReading(eHubClient);

			var results = rule.Evaluate(Context, new IFactResolver[] { new PropertyFactResolver(routingEvaluationInput.PropertyFacts), new MessageFactResolver(routingEvaluationInput.Message), new SqlFactResolver(Context) }, LogManager.GetLogger(typeof(Rule)));

			return results.Select(r => (CargoWise.eHub.Shared.RoutingRuleEngine.Result)r).ToArray();
		}

		public CargoWise.eHub.Shared.RoutingRuleEngine.Result[] EvaluateOCM(RoutingEvaluationOCMInput routingEvaluationInput)
		{
			var filteredResults = new List<Result>();

			try
			{
				var messageFactResolver = new MessageFactResolver(routingEvaluationInput.Message);
				var propertyFactResolver = new PropertyFactResolver(routingEvaluationInput.PropertyFacts);
				var sqlFactResolver = new SqlFactResolver(Context);
				var resolvers = new IFactResolver[] { messageFactResolver, propertyFactResolver, sqlFactResolver };

				var results = RoutingRuleEvaluation.EvaluateOCM(Context, RuleFactory, Client_OCM_Copying, "SHIPPING_INSTRUCTION", resolvers, Logger);

				if (results[1] != null)
				{
					filteredResults.Add(results[1]);
				}

				if (results.Count == 3)
				{
					filteredResults.Add(results[2]);
				}
			}

			catch (Exception ex)
			{
				Logger.Warn("Message processing failure: ", ex);
				throw;
			}
			finally
			{
				Logger.Info("Finished processing message");
			}

			return filteredResults.Select(r => (CargoWise.eHub.Shared.RoutingRuleEngine.Result)r).ToArray();
		}

		public eHubClient GetEHubClient(string ruleId)
		{
			return Context.eHubClients.Include(c => c.eHubRoutingRule).FirstOrDefault(c => c.CC_ID == ruleId);
		}
	}
}
