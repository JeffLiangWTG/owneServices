using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ProductionRules.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WTG.ProductionRules.Core;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.ProductionRules.GUI
{
	public class UserHaltableProductionRulesEngineService : IUserHaltableProductionRulesEngineService
	{
		public void RunRulesEngine(
			BusinessObjectFactory factory,
			INotifications notifications,
			RulesContextType contextType,
			Func<IEnumerable<IInputFact>> getFacts,
			Func<ProductionRulesEngineResult,
			INotification> processResults)
		{
			RunRulesEngine(factory, notifications, contextType, ProductionRuleSetFilter.Empty, getFacts, processResults);
		}

		public void RunRulesEngine(
			BusinessObjectFactory factory,
			INotifications notifications,
			RulesContextType contextType,
			ProductionRuleSetFilter filters,
			Func<IEnumerable<IInputFact>> getFacts,
			Func<ProductionRulesEngineResult, INotification> processResults)
		{
			Argument.NotNull(getFacts, nameof(getFacts));
			RunRulesEngine(factory, notifications, contextType, filters, () => [getFacts()], processResults);
		}

		public void RunRulesEngine(
			BusinessObjectFactory factory,
			INotifications notifications,
			RulesContextType contextType,
			ProductionRuleSetFilter filters,
			Func<IEnumerable<IEnumerable<IInputFact>>> getFactBatches,
			Func<ProductionRulesEngineResult, INotification> processResults)
		{
			Argument.NotNull(filters, nameof(filters));
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(getFactBatches, nameof(getFactBatches));
			Argument.NotNull(processResults, nameof(processResults));

			if (Globals.IsUserInteractive)
			{
				RunUserInteractive(factory, notifications, contextType, filters, getFactBatches, processResults);
			}
			else
			{
				RunNonUserInteractive(factory, notifications, contextType, filters, getFactBatches, processResults);
			}
		}

		static void RunUserInteractive(
			BusinessObjectFactory factory,
			INotifications notifications,
			RulesContextType contextType,
			ProductionRuleSetFilter filters,
			Func<IEnumerable<IEnumerable<IInputFact>>> getFactBatches,
			Func<ProductionRulesEngineResult, INotification> processResults)
		{
			ProductionRulesEngineResult result;

			IInputFact[][] factBatchesArray = null;
			using (var progressForm = new ProgressFormManager { IsCancelButtonVisible = false, IsProgressBarVisible = true })
			{
				progressForm.UpdateStatus(Res.GetString("86af5e90-6099-493d-ad16-4be2bcf08ce3", "Loading facts..."), 0);
				progressForm.Start();

				factBatchesArray = GetFactBatchesArrayWithErrorHandling(notifications, getFactBatches);
			}

			if (factBatchesArray.Length > 0)
			{
				using (var progressForm = new ProgressFormManager { IsCancelButtonVisible = true, IsProgressBarVisible = true })
				using (var cts = new CancellationTokenSource())
				{
					progressForm.Cancelled += (s, e) => cts.Cancel();
					progressForm.UpdateStatus(Res.GetString("6e1103e6-0217-4c8f-a5fc-f17af71fab1f", "Running rules..."), 30);
					progressForm.Start();

					var rulesEngineService = ObjectFactory.Get<IProductionRulesEnginePushService>(nameof(IProductionRulesEnginePushService), new[] { factory });
					result = rulesEngineService.RunRulesEngine(contextType, RulesContextSubType.None, filters, factBatchesArray, null, cts.Token);
				}

				if (result.Status == ResultStatus.Success)
				{
					INotification notification = null;

					using (var progressForm = new ProgressFormManager { IsCancelButtonVisible = false, IsProgressBarVisible = true })
					{
						progressForm.UpdateStatus(Res.GetString("7c0256b2-47ec-4f5a-a6f1-2c6eb324dd83", "Processing results..."), 75);
						progressForm.Start();
						notification = processResults(result);
					}

					if (notification != null)
					{
						notifications.Add(notification);
					}
				}
				else if (result.Status == ResultStatus.Error)
				{
					notifications.AddError(result.Notifications);
				}
			}
		}

		static void RunNonUserInteractive(
			BusinessObjectFactory factory,
			INotifications notifications,
			RulesContextType contextType,
			ProductionRuleSetFilter filters,
			Func<IEnumerable<IEnumerable<IInputFact>>> getFactBatches,
			Func<ProductionRulesEngineResult, INotification> processResults)
		{
			var factBatchesArray = GetFactBatchesArrayWithErrorHandling(notifications, getFactBatches);
			if (factBatchesArray.Length > 0)
			{
				var rulesEngineService = ObjectFactory.Get<IProductionRulesEnginePushService>(nameof(IProductionRulesEnginePushService), new[] { factory });
				var result = rulesEngineService.RunRulesEngine(contextType, RulesContextSubType.None, filters, factBatchesArray, null, CancellationToken.None);

				if (result.Status == ResultStatus.Success)
				{
					var notification = processResults(result);

					if (notification != null)
					{
						notifications.Add(notification);
					}
				}
				else if (result.Status == ResultStatus.Error)
				{
					notifications.AddError(result.Notifications);
				}
			}
		}

		static IInputFact[][] GetFactBatchesArrayWithErrorHandling(
			INotifications notifications,
			Func<IEnumerable<IEnumerable<IInputFact>>> getFactBatches)
		{
			IInputFact[][] factBatchesArray;

			try
			{
				var factBatches = getFactBatches();
				Argument.NotNull(factBatches, $"{nameof(getFactBatches)}() returned null.");

				factBatchesArray = factBatches.Select(f => Argument.NotNull(f, $"{nameof(getFactBatches)}() returned a null batch.").ToArray()).Where(f => f.Length > 0).ToArray();
			}
			catch (FactLoadingException ex)
			{
				notifications.AddError(Res.GetString("e91c5037-5f9c-46a9-b2e8-c302e4e21150", "Error occurred while loading data: {0}", ex.Message));
				factBatchesArray = [];
			}

			return factBatchesArray;
		}
	}
}
