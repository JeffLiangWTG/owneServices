using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using WTG.ErrorReporting;

namespace CargoWise.Billing.CollectorService.Plugin
{
	public abstract class AbstractPlugin : IBillingTransactionsPlugin
	{
		#region Implement IBillingTransactionsPlugin
		public abstract IEnumerable<TimeStampedTransaction> GetTransactions(DateTime start, DateTime end);

		public abstract void UpdateSettings(PluginSettings settings);

		public ILogger Logger
		{
			get { return logger ??= CreateLogger(); }
		}

		public void SetLoggerFactory(ILoggerFactory loggerFactory)
		{
			this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
		}

		public void SetErrorReportingClient(IErrorReportingClient? errorReportingClient)
		{
			this.errorReportingClient = errorReportingClient;
		}

		ILogger? logger;
		protected ILoggerFactory? loggerFactory;

		protected IErrorReportingClient? ErrorReportingClient => errorReportingClient;
		IErrorReportingClient? errorReportingClient;

		/// <summary>
		/// Disposes of the <see cref="AbstractPlugin"/>.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Defines a method to release allocated resources.
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
		}

		#endregion

		protected internal virtual ILogger CreateLogger()
		{
			return loggerFactory!.CreateLogger(GetType());
		}

		protected static string ValueOrEmptyString(PluginParameter? param)
		{
			return param == null ? string.Empty : param.Value;
		}
	}
}
