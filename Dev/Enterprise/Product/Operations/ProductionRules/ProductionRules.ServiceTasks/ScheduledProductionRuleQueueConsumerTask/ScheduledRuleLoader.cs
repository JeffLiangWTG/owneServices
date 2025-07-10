using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Core;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.ProductionRules.ServiceTasks
{
	class ScheduledRuleLoader : IScheduledRuleLoader
	{
		public ScheduledRuleLoader(IScheduledRuleProcessorFactory ruleProcessorFactory)
		{
			RuleProcessorFactory = Argument.NotNull(ruleProcessorFactory, nameof(ruleProcessorFactory));
		}

		IScheduledRuleProcessorFactory RuleProcessorFactory { get; }

		public ScheduledRuleLoaderResult GetNextRuleSetToProcess(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));

			ScheduledRuleLoaderResult result = null;

			while (result == null)
			{
				var ruleSet = GetRuleSetToProcess(factory);
				if (ruleSet == null)
				{
					break;
				}

				var ruleSetItem = ruleSet.Item;
				var queuedRules = GetQueuedRules(factory, ruleSetItem.PK);

				var rulesContext = ruleSetItem.PRS_Context.ToString().GetEnumValue<RulesContextType>().Value;
				var rulesProcessor = RuleProcessorFactory.GetRuleProcessor(rulesContext);

				if (rulesProcessor is null)
				{
					ruleSet.Dispose();
					throw new InvalidOperationException($"Could not load rule processor for Context: {rulesContext}, Code: {ruleSetItem.PRS_Context}.");
				}

				using (rulesProcessor.GetTemporaryUserContextWithBranchOffRuleSet(factory.GetCachedReadOnlyFactory(), ruleSetItem))
				{
					var (rulesToInclude, disabledRules) = BumpRetryCounts(factory, queuedRules);

					if (rulesToInclude.Any())
					{
						var allRulesToRun = GetAllRulesToRun(factory, ruleSet.Item.PK, rulesToInclude);
						result = new ScheduledRuleLoaderResult(ruleSet, allRulesToRun, rulesToInclude, rulesProcessor);
					}
					else
					{
						ruleSet.Dispose();
					}

					if (disabledRules.Any())
					{
						var groupToContact = rulesProcessor?.ErrorContactGroupRegistryItem;
						SendEmailForDisabledProductionRules(groupToContact, ruleSetItem.PRS_Name, rulesContext, disabledRules);
					}
				}
			}

			return result;
		}

		static AppLockedItem<ProductionRuleSet> GetRuleSetToProcess(BusinessObjectFactory factory)
		{
			const string AppLockKey = nameof(ScheduledRuleLoader) + "_" + nameof(AppLockKey);
			const int BatchSize = 1;

			var queueSubQuery = new ZDBOnlySubQuery(typeof(ProductionRuleScheduleQueue), ProductionRuleScheduleQueueSchema.PRQ_PRL_Rule);

			var ruleSubQuery = new ZDBOnlySubQuery(typeof(ProductionRule), ProductionRuleSchema.PRL_PRS_RuleSet);
			ruleSubQuery.AddSubQuery(queueSubQuery, JoinCondition.And);

			var ruleSetQuery = new ZDBOnlyQuery(typeof(ProductionRuleSet));
			ruleSetQuery.AddSubQuery(ruleSubQuery, JoinCondition.And);

			// We want to sort by RetryCount/CreateTime on the queue, but lock the RuleSet. This seems to be the only way to do it with the existing architecture.
			ruleSetQuery.OrderBy = $@"
(
	SELECT MIN(DATEADD(year, {ProductionRuleScheduleQueueSchema.Constants.PRQ_RetryCount}, {ProductionRuleScheduleQueueSchema.Constants.PRQ_SystemCreateTimeUtc}))
	FROM {ProductionRuleScheduleQueueSchema.Constants.SqlSchemaName}.{ProductionRuleScheduleQueueSchema.Constants.TableName}
	WHERE {ProductionRuleScheduleQueueSchema.Constants.PRQ_PRL_Rule} IN
	(
		SELECT {ProductionRuleSchema.Constants.PK} FROM {ProductionRuleSchema.Constants.SqlSchemaName}.{ProductionRuleSchema.Constants.TableName} WHERE {ProductionRuleSchema.Constants.PRL_PRS_RuleSet} = {ProductionRuleSetSchema.Constants.PK}
	)
)";

			var ruleSet = factory.LoadWithApplocks<ProductionRuleSet>(AppLockKey, ruleSetQuery, BatchSize);
			return ruleSet.ItemsWithLocks.SingleOrDefault();
		}

		static IEnumerable<ProductionRuleScheduleQueue> GetQueuedRules(BusinessObjectFactory factory, ZGuid ruleSetPK)
		{
			var ruleSubQuery = new ZDBOnlySubQuery(typeof(ProductionRule), ProductionRuleScheduleQueueSchema.PRQ_PRL_Rule);
			ruleSubQuery.AddToFilter(ProductionRuleSchema.PRL_PRS_RuleSet, ruleSetPK);

			var scheduledRulesQueueQuery = new ZDBOnlyQuery(typeof(ProductionRuleScheduleQueue));
			scheduledRulesQueueQuery.AddSubQuery(ruleSubQuery, JoinCondition.And);

			return factory.Load<ProductionRuleScheduleQueue>(scheduledRulesQueueQuery);
		}

		static IEnumerable<ProductionRule> GetAllRulesToRun(BusinessObjectFactory factory, ZGuid ruleSetPK, IEnumerable<ProductionRuleScheduleQueue> queuedRules)
		{
			var scheduledSubQuery = new ZDBOnlySubQuery(typeof(ProductionRuleScheduleTask), StmScheduleTaskSchema.S5_ParentID, notIn: true);

			var nonScheduledRulesQuery = new ZDBOnlyQuery(typeof(ProductionRule));
			nonScheduledRulesQuery.AddToFilter(ProductionRuleSchema.PRL_PRS_RuleSet, ruleSetPK);
			nonScheduledRulesQuery.AddSubQuery(scheduledSubQuery, JoinCondition.And);

			var rulesQuery = new ZDBOnlyQuery(typeof(ProductionRule));
			rulesQuery.AddToFilter(ProductionRuleSchema.PK, queuedRules.Select(q => q.PRQ_PRL_Rule));
			rulesQuery.AddToFilter(nonScheduledRulesQuery, JoinCondition.Or);

			return factory.Load<ProductionRule>(rulesQuery);
		}

		static (IEnumerable<ProductionRuleScheduleQueue> rulesToInclude, IEnumerable<ProductionRule> deletedRules) BumpRetryCounts(BusinessObjectFactory factory, IEnumerable<ProductionRuleScheduleQueue> queuedRules)
		{
			var rulesToInclude = new List<ProductionRuleScheduleQueue>();
			var disabledRules = new List<ProductionRule>();

			var attemptCap = SystemDataRegistry.Instance.MaxNumberOfAttemptsForRuleProcessing.Value;

			foreach (var queuedRule in queuedRules)
			{
				queuedRule.PRQ_RetryCount++;

				if (queuedRule.PRQ_RetryCount > attemptCap)
				{
					if (DisableProductionRuleScheduleTask(factory, queuedRule))
					{
						disabledRules.Add(queuedRule.Rule);
					}

					queuedRule.Delete();
				}
				else
				{
					rulesToInclude.Add(queuedRule);
				}
			}

			factory.Save();

			return (rulesToInclude, disabledRules);
		}

		static bool DisableProductionRuleScheduleTask(BusinessObjectFactory factory, ProductionRuleScheduleQueue rule)
		{
			var scheduleTask = factory.LoadTop1<ProductionRuleScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ParentID, rule.PRQ_PRL_Rule));
			var wasActive = scheduleTask.S5_IsActive;

			if (wasActive)
			{
				scheduleTask.S5_IsActive = false;
			}

			return wasActive;
		}

		static void SendEmailForDisabledProductionRules(GuidRegistryItem groupToContact, string ruleSetName, RulesContextType rulesContext, IEnumerable<ProductionRule> disabledRules)
		{
			var groupToContactPK = groupToContact?.Value ?? Guid.Empty;
			if (groupToContactPK != Guid.Empty)
			{
				var email = new EmailDef();
				email.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(groupToContactPK, groupToContact);
				if (email.Recipients.Count > 0)
				{
					email.Subject = Res.GetString("a741a169-6306-46ac-9876-496cd119264d", "{0} Rule Set Failure", ruleSetName);

					var stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(Res.GetString("301713eb-32e3-4b0e-bc55-487907967aba", "Production Rule Set {0} for Context: {1} disabled the following rule(s) due to repeated failure to process:", ruleSetName, rulesContext));
					foreach (var rule in disabledRules)
					{
						stringBuilder.AppendLine(rule.PRL_Name);
					}
					stringBuilder.AppendLine();
					stringBuilder.AppendLine(Res.GetString("ea49cf05-f6f9-4470-8048-73c6e0ad1708", "Error Logs can be viewed on the Scheduled Production Rule Consumer (SPC) service task."));
					stringBuilder.AppendLine();
					stringBuilder.AppendLine(Res.GetString("48a1ecb0-3091-4335-9ce8-982ab573944e", "To include disabled rules in future runs, re-enable the rules via the Production Rules Management Portal."));

					email.Body = stringBuilder.ToString();
					Env.OutgoingMailManager.CreateAndSave(email);
				}
			}
		}
	}
}
