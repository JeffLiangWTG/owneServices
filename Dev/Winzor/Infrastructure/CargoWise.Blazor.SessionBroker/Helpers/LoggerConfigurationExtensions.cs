using System;
using System.Reflection;
using Serilog;
using WTG.Logging;

namespace CargoWise.Blazor.SessionBroker.Helpers
{
	public static class LoggerConfigurationExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static LoggerConfiguration AddEnrichers(this LoggerConfiguration configuration, Guid? versionBrokerProcessCorrelationId, Guid? sessionBrokerProcessCorrelationId, string version, string hostname, bool isDevelopmentOrIntegrationTestingOrDAT)
		{
			ArgumentNullException.ThrowIfNull(sessionBrokerProcessCorrelationId);
			if (!isDevelopmentOrIntegrationTestingOrDAT)
			{
				ArgumentNullException.ThrowIfNull(versionBrokerProcessCorrelationId);
			}

			return configuration
				.Enrich.WithProperty(SystemLogProperties.MachineName, Environment.MachineName)
				.Enrich.WithProperty(SystemLogProperties.AppName, Assembly.GetExecutingAssembly().GetName().Name)
				.Enrich.WithProperty("VersionBrokerProcessCorrelationId", versionBrokerProcessCorrelationId)
				.Enrich.WithProperty("SessionBrokerProcessCorrelationId", sessionBrokerProcessCorrelationId)
				.Enrich.WithProperty("Version", version)
				.Enrich.WithProperty("Hostname", hostname)
				.Enrich.WithThreadInfo()
				.Enrich.FromLogContext();
		}
	}
}
