using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using WTG.ErrorReporting;

namespace CargoWise.Billing.CollectorService.Plugin
{
	/// <summary>
	/// Represents a configurable plugin that can provide billing information.
	/// </summary>
	public interface IBillingTransactionsPlugin : IDisposable
	{
		/// <summary>
		/// Returns a sequence of billing transactions with service time between <paramref name="start"/> and <paramref name="end"/>
		/// </summary>
		/// <param name="start">Minimal service time of transactions to return, exclusive.</param>
		/// <param name="end">Maximum service time of transactions to return, inclusive.</param>
		IEnumerable<TimeStampedTransaction> GetTransactions(DateTime start, DateTime end);

		/// <summary>
		/// Updates internal plugin settings that are used to access billing information.
		/// </summary>
		/// <param name="settings">Generic plugin settings containing set of named string parameters.</param>
		void UpdateSettings(PluginSettings settings);

		/// <summary>
		/// Gets a logger used by this plugin.
		/// </summary>
		ILogger Logger { get; }
		/// <summary>
		/// Set logger loggerFactory
		/// </summary>
		/// <param name="loggerFactory"></param>
		void SetLoggerFactory(ILoggerFactory loggerFactory);

		/// <summary>
		/// Set error reporting client
		/// </summary>
		/// <param name="errorReportingClient"></param>
		void SetErrorReportingClient(IErrorReportingClient? errorReportingClient);
	}
}
