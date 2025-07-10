using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ProductionRules.Integration;
using Enterprise.ZArchitecture.Core;
using WTG.ProductionRules.Core;
using Argument = CargoWise.Common.Argument;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.ProductionRules.ServiceTasks
{
	class ScheduledProductionRuleQueueConsumer : IScheduledProductionRuleQueueConsumer
	{
		public ScheduledProductionRuleQueueConsumer(
			IScheduledRuleLoader scheduledRuleLoader,
			IProductionRulesEnginePullService rulesEngine)
		{
			ScheduledRuleLoader = Argument.NotNull(scheduledRuleLoader, nameof(scheduledRuleLoader));
			RulesEngine = Argument.NotNull(rulesEngine, nameof(rulesEngine));
		}

		IScheduledRuleLoader ScheduledRuleLoader { get; }
		IProductionRulesEnginePullService RulesEngine { get; }

		public void ProcessQueue(INotifications notifications, CancellationToken token)
		{
			Argument.NotNull(notifications, nameof(notifications));

			ScheduledRuleLoaderResult nextRuleSetToProcess = null;

			while (!token.IsCancellationRequested && (nextRuleSetToProcess = GetNextRuleSetToProcessInNewFactory()) != null)
			{
				using (nextRuleSetToProcess)
				{
					try
					{
						ProcessQueueForSingleRuleSet(nextRuleSetToProcess, notifications, token);
					}
					catch (OperationCanceledException operationCanceledException) when (operationCanceledException.CancellationToken == token)
					{
						throw;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce("CriticalErrorInScheduledProductionRuleQueueConsumer", ex);
						notifications.AddError(string.Format(CultureInfo.InvariantCulture, (NoResString)"Unexpected error occurred: {0}", ex.Message));
					}
				}
			}

			ScheduledRuleLoaderResult GetNextRuleSetToProcessInNewFactory()
			{
				var newFactory = new BusinessObjectFactory { NameForDebugging = $"{nameof(ScheduledProductionRuleQueueConsumer)}_{nameof(ScheduledRuleLoader)}", RefreshEnabled = false };
				return ScheduledRuleLoader.GetNextRuleSetToProcess(newFactory);
			}
		}

		void ProcessQueueForSingleRuleSet(
			ScheduledRuleLoaderResult nextRuleSetToProcess,
			INotifications notifications,
			CancellationToken token)
		{
			var ruleSet = nextRuleSetToProcess.RuleSetWithLock.Item;
			var rulesContext = ruleSet.PRS_Context.ToString().GetEnumValue<RulesContextType>().Value;

			var rulesNamesForLog = string.Join(", ", nextRuleSetToProcess.Rules.Select(r => r.Name));
			notifications.Add(NotificationType.Information, $"Processing RuleSet: {ruleSet.PRS_Name} for Context: {rulesContext}, Scheduled Rules: {rulesNamesForLog}.");

			var rulesProcessor = nextRuleSetToProcess.RuleProcessor;
			var factory = ruleSet.Factory;
			var readOnlyFactory = factory.GetCachedReadOnlyFactory();

			using (rulesProcessor.GetTemporaryUserContextWithBranchOffRuleSet(readOnlyFactory, ruleSet))
			{
				var (facts, hadErrorsLoadingFacts) = LoadFacts(rulesProcessor, readOnlyFactory, ruleSet, notifications, token);

				if (!hadErrorsLoadingFacts)
				{
					var shouldDequeueEntryAndSaveFactory = true;
					var factsArray = facts.ToArray();

					if (factsArray.Length > 0)
					{
						shouldDequeueEntryAndSaveFactory = RunRulesEngineAndProcessResults(nextRuleSetToProcess, rulesContext, rulesProcessor, factsArray, notifications, token);
					}
					else
					{
						notifications.Add(NotificationType.Information, rulesProcessor.InformationMessageForNothingProcessed);
					}

					if (shouldDequeueEntryAndSaveFactory)
					{
						foreach (var queuedEntry in nextRuleSetToProcess.QueueEntries)
						{
							queuedEntry.Delete();
						}

						SaveFactoryWithErrorHandlingAndLogging();
					}
				}
			}

			void SaveFactoryWithErrorHandlingAndLogging()
			{
				try
				{
					factory.Save();
					notifications.Add(NotificationType.Information, $"Succesfully processed and saved RuleSet: {ruleSet.PRS_Name} for Context: {rulesContext}, Scheduled Rules: {rulesNamesForLog}.");
				}
				catch (ZSaveConcurrencyException)
				{
					notifications.AddError((NoResString)"Concurrency error occurred while saving the results of the processed rule(s).");
				}
				// catch (ZSaveException ex), these should be caught at the top level handler and be sent via ErrorReporter!
				catch (ZCannotSaveException ex)
				{
					notifications.AddError(string.Format(CultureInfo.InvariantCulture, (NoResString)"Error occurred while saving the results of the processed rule(s): {0}", ex.Message));
				}
			}
		}

		static (IEnumerable<IInputFact> Facts, bool HadErrors) LoadFacts(
			IScheduledRuleProcessor rulesProcessor,
			ReadOnlyBusinessObjectFactory readOnlyFactory,
			IProductionRuleSet ruleSet,
			INotifications notifications,
			CancellationToken token)
		{
			IEnumerable<IInputFact> facts;
			var hadErrors = false;

			try
			{
				facts = rulesProcessor.LoadInputFacts(readOnlyFactory, ruleSet, token);
			}
			catch (FactLoadingException ex)
			{
				facts = Enumerable.Empty<IInputFact>();
				notifications.AddError(ex.Message);
				hadErrors = true;
			}

			return (facts, hadErrors);
		}

		bool RunRulesEngineAndProcessResults(
			ScheduledRuleLoaderResult nextRuleSetToProcess,
			RulesContextType rulesContext,
			IScheduledRuleProcessor rulesProcessor,
			IInputFact[] factsArray,
			INotifications notifications,
			CancellationToken token)
		{
			bool shouldDequeueEntryAndSaveFactory;

			var ruleSet = nextRuleSetToProcess.RuleSetWithLock.Item;
			var subContextType = ruleSet.PRS_ContextSubType.ToString().GetEnumValue<RulesContextSubType>() ?? RulesContextSubType.None;
			var result = RulesEngine.RunRulesEngine(rulesContext, subContextType, ProductionRuleSetFilter.Empty, nextRuleSetToProcess.Rules, factsArray, token);

			if (result.Status == ResultStatus.Success)
			{
				if (result.Facts.Any())
				{
					var notificationDecorator = new NotificationsDecorator(notifications);
					rulesProcessor.ProcessResults(ruleSet.Factory, result, notificationDecorator, token);
					shouldDequeueEntryAndSaveFactory = !notificationDecorator.ReportedFatalError;
				}
				else
				{
					notifications.Add(NotificationType.Information, rulesProcessor.InformationMessageForNothingProcessed);
					shouldDequeueEntryAndSaveFactory = true;
				}
			}
			else
			{
				shouldDequeueEntryAndSaveFactory = false;

				if (result.Status == ResultStatus.Error)
				{
					notifications.AddError(result.Notifications);
				}
			}

			return shouldDequeueEntryAndSaveFactory;
		}
	}
}
