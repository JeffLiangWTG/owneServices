using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Billing.API;
using CargoWise.Billing.Kafka.API;
using CargoWise.Billing.Client;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using WTG.ErrorReporting;

namespace CargoWise.Billing.CollectorService.Plugin
{
	public sealed class PluginController : IDisposable
	{
		public event EventHandler LastSuccessfulRunChanged = delegate { };

		public PluginController(Assembly pluginAssembly,
			PluginControllerSettings settings,
			PluginControllerState state,
			Dictionary<string, string> globalSettings,
			IBillingServiceClient billingServiceClient,
			IBillingKafkaClient billingKafkaClient,
			IClock clock,
			object stateLock,
			ILoggerFactory loggerFactory,
			IErrorReportingClient? errorReportingClient,
			Dictionary<string, string[]?>? knownExceptions = null,
			Type[]? additionalSchedulerTypes = null)
		{
			if (pluginAssembly == null) throw new ArgumentNullException("pluginAssembly");
			this.clock = clock ?? throw new ArgumentNullException("clock");
			this.clock.Notify += OnClockNotify;

			this.billingServiceClient = billingServiceClient ?? throw new ArgumentNullException("billingServiceClient");
			this.billingKafkaClient = billingKafkaClient ?? throw new ArgumentNullException("billingKafkaClient");
			this.settings = settings ?? throw new ArgumentNullException("settings");
			this.state = state ?? throw new ArgumentNullException("state");
			this.globalSettings = globalSettings ?? throw new ArgumentNullException("globalSettings");
			this.stateLock = stateLock;
			this.errorReportingClient = errorReportingClient;
			this.knownExceptions = knownExceptions;
			pluginScheduler = GetPluginScheduler(this.settings.SchedulerType, additionalSchedulerTypes);
			var pluginType = pluginAssembly.GetType(this.settings.TypeName);
			transactionsPlugin = (IBillingTransactionsPlugin)Activator.CreateInstance(pluginType, BindingFlags.Public | BindingFlags.Instance, default, null, default);
			transactionsPlugin.SetErrorReportingClient(errorReportingClient);
			transactionsPlugin.SetLoggerFactory(loggerFactory);
			transactionsPlugin.UpdateSettings(this.settings.PluginSettings);
		}

		public void Start()
		{
			ScheduleNextRun(pluginScheduler.GetNextRunDueTime(clock, settings, state));
		}

		public PluginControllerSettings CurrentSettings
		{
			get { return new PluginControllerSettings(settings); }
		}
		public PluginControllerState CurrentState
		{
			get { return new PluginControllerState(state); }
		}

		public void UpdateSettings(PluginControllerSettings newSettings)
		{
			if (!settings.Equals(newSettings))
			{
				if (settings.Key != newSettings.Key)
				{
					throw new ArgumentException("Plugin key cannot be changed.", "newSettings");
				}
				if (settings.TypeName != newSettings.TypeName)
				{
					throw new ArgumentException("Plugin type cannot be changed.", "newSettings");
				}
				settings = new PluginControllerSettings(newSettings);
				transactionsPlugin.UpdateSettings(settings.PluginSettings);
				ScheduleNextRun(pluginScheduler.GetNextRunDueTime(clock, settings, state));
			}
		}

		public void ScheduleNextRun()
		{
			ScheduleNextRun(pluginScheduler.GetNextRunDueTime(clock, settings, state));
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				clock.Dispose();
				transactionsPlugin.Dispose();
				billingServiceClient.Dispose();
				billingKafkaClient.Dispose();
			}
		}
		void OnClockNotify(object sender, EventArgs eventArgs)
		{
			try
			{
				ProcessTransactions();
				currentRetryAttempt = 0;
				ScheduleNextRun(pluginScheduler.GetNextRunDueTime(clock, settings, state));
			}
			catch (Exception e)
			{
				var message = "Error happened while collecting and sending transactions.";
				if (IsKnownException(e))
				{
					transactionsPlugin.Logger.LogWarning(e, message);
					ScheduleNextRun(GetMinIntervalDueTime(settings.RetryInterval, pluginScheduler.GetNextIntervalDueTime(clock, settings, state)));
					return;
				}

				currentRetryAttempt++;
				if (currentRetryAttempt <= settings.MaxRetryAttempts && settings.RetryInterval < pluginScheduler.GetNextIntervalDueTime(clock, settings, state))
				{
					transactionsPlugin.Logger.LogWarning(e, message);
					transactionsPlugin.Logger.LogWarning($"Retrying,  current attempts: {currentRetryAttempt}");
					ScheduleNextRun(settings.RetryInterval);
				}
				else
				{
					errorReportingClient?.ReportToIssueManager(message, e, transactionsPlugin.Logger);
					ScheduleNextRun(pluginScheduler.GetNextIntervalDueTime(clock, settings, state));
				}
			}
		}

		static TimeSpan GetMinIntervalDueTime(TimeSpan dt1, TimeSpan dt2) => (dt1 <= dt2) ? dt1 : dt2;

		void ScheduleNextRun(TimeSpan dueTime)
		{
			if (!disposed)
			{
				transactionsPlugin.Logger.LogInformation($"Next run scheduled in {dueTime:dd\\:hh\\:mm\\:ss}");
				clock.SetNotificationDueTime(dueTime);
			}
		}

		public const int BatchSize = 100;

		void ProcessTransactions()
		{
			var end = pluginScheduler.GetEndUtc(settings, state);
			var batch = new List<BillingTransaction>(BatchSize);
			while (!disposed && end <= clock.UtcNow)
			{
				successTransactionsCount = 0;
				errorTransactionsCount = 0;
				successUsageCount = 0;

				DateTime start;
				DateTime lastTransactionTimestamp;
				lock (stateLock)
				{
					start = pluginScheduler.GetStartUtc(settings, state);
					lastTransactionTimestamp = state.LastTransactionTimestamp;
				}
				transactionsPlugin.Logger.LogInformation("Transactions being retrieved for period '" + start + "' (exclusive) to '" + end + "'(inclusive)");

				var transactions = transactionsPlugin.GetTransactions(start, end).TakeWhile(transaction => !disposed);

				var count = 0;
				foreach (var transaction in transactions)
				{
					lastTransactionTimestamp = transaction.TimeStamp > lastTransactionTimestamp
						? transaction.TimeStamp
						: lastTransactionTimestamp;

					if (CurrentSettings.SendBillingTransaction)
					{
						batch.Add(transaction.BillingTransaction);
					}

					if (CurrentSettings.SendUsageTransaction)
					{
						SendUsageTransaction(transaction.UsageTransaction);
					}

					if (batch.Count == BatchSize)
					{
						ProcessBatch(batch);
					}

					count++;
				}

				if (batch.Count > 0)
				{
					ProcessBatch(batch);
				}

				if (CurrentSettings.SendBillingTransaction)
				{
					transactionsPlugin.Logger.LogInformation($"{count} billing transactions found. {successTransactionsCount} submitted successfully. {errorTransactionsCount} in error. ");
				}

				if (CurrentSettings.SendUsageTransaction)
				{
					billingKafkaClient.FlushProducers();
					transactionsPlugin.Logger.LogInformation($"{count} usage transactions found. {successUsageCount} submitted successfully. {failedUsageTransactions.Count} in error. ");
					if (failedUsageTransactions.Count != 0)
					{
						throw new InvalidOperationException($"{string.Join(Environment.NewLine, failedUsageTransactions.Select(x => $"{x.Key}{Environment.NewLine}{string.Join(Environment.NewLine, x.Value.Select(t => t.ToString()))}"))}");
					}
				}
				if (disposed) break;

				lock (stateLock)
				{
					state.LastTransactionTimestamp = lastTransactionTimestamp;
					state.LastSuccessfulRun = end;
					LastSuccessfulRunChanged(this, EventArgs.Empty);
				}
				end = pluginScheduler.GetEndUtc(settings, state);
			}
		}

		/// <summary>
		/// Process a batch
		/// </summary>
		/// <param name="batch">List to process. Will be cleared.</param>
		void ProcessBatch(List<BillingTransaction> batch)
		{
			if (batch.Count > 1)
			{
				try
				{
					billingServiceClient.AddTransactionRange(batch);
					successTransactionsCount += batch.Count;
					batch.Clear();
					return;
				}
				catch (ValidationException)
				{
					// Fall through to process one by one
				}
			}

			validationErrors.Clear();
			foreach (var transaction in batch)
			{
				try
				{
					billingServiceClient.AddTransaction(transaction);
					successTransactionsCount++;
				}
				catch (ValidationException ex)
				{
					errorTransactionsCount++;
					var errorMessage = string.Format("Validation failed for transaction [{0}]:\r\n{1}\r\n{2}",
						transaction,
						string.Join(Environment.NewLine, ex.Errors.Select(error => "  " + error)),
						ex);
					validationErrors.Add(errorMessage);
				}
			}

			batch.Clear();

			if (validationErrors.Count > 0)
			{
				throw new ValidationException("Validation failed for transactions", validationErrors);
			}
		}

		void SendUsageTransaction(UsageTransaction? transaction) =>
			billingKafkaClient.SendUsageInfoToELK(globalSettings["UsageELKKafkaTopic"], Guid.NewGuid().ToString(), transaction, HandleDeliveryReport);

		void HandleDeliveryReport(DeliveryReport<string, ELKTransaction> report)
		{
			if (report.Error != ErrorCode.NoError)
			{
				try
				{
					if (report.Value is UsageTransaction transaction)
					{
						billingServiceClient.AddUsageTransaction(transaction);
						Interlocked.Increment(ref successUsageCount);
					}
				}
				catch (Exception e)
				{
					var error = $"Kafka Error: {report.Error.Reason}. Billing service exception message: {e.Message}";
					if (failedUsageTransactions.TryGetValue(error, out var transactions))
					{
						transactions.Add(report.Value);
					}
					else
					{
						failedUsageTransactions.TryAdd(error, new List<ELKTransaction>(new[] { report.Value }));
					}
				}
			}
			else
			{
				Interlocked.Increment(ref successUsageCount);
			}
		}

		bool IsKnownException(Exception e)
		{
			var exName = e.GetType().FullName;

			if (knownExceptions != null && knownExceptions.TryGetValue(exName, out string[]? errors))
			{
				if (errors is null || errors.Length == 0) return true;

				return errors.Any(error => e.Message.Contains(error));
			}

			return false;
		}

		IPluginScheduler GetPluginScheduler(string typeName, Type[]? additionalTypes)
		{
			if (string.IsNullOrEmpty(typeName) || typeof(PluginIntervalScheduler).FullName.Equals(typeName))
			{
				return new PluginIntervalScheduler();
			}

			Type? schedulerType = additionalTypes?.FirstOrDefault(t => t.FullName == typeName);

			if (schedulerType == null)
			{
				throw new InvalidOperationException($"Not found scheduler '{typeName}'");
			}

			return (IPluginScheduler)Activator.CreateInstance(schedulerType, BindingFlags.Public | BindingFlags.Instance, default, null, default);
		}

		int successTransactionsCount;
		int errorTransactionsCount;
		int successUsageCount;
		List<string> validationErrors = new ();
		bool disposed;
		int currentRetryAttempt;
		readonly IClock clock;
		PluginControllerSettings settings;
		PluginControllerState state;
		readonly IBillingServiceClient billingServiceClient;
		readonly IBillingKafkaClient billingKafkaClient;
		readonly IBillingTransactionsPlugin transactionsPlugin;
		readonly ConcurrentDictionary<string, List<ELKTransaction>> failedUsageTransactions = new ();
		readonly Dictionary<string, string> globalSettings;
		readonly IErrorReportingClient? errorReportingClient;
		readonly IPluginScheduler pluginScheduler;

		readonly object stateLock;

		readonly Dictionary<string, string[]?>? knownExceptions = new();
	}
}
