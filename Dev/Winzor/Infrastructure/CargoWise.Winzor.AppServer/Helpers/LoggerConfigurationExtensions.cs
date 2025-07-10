using System;
using System.Reflection;
using Enterprise.ZArchitecture.Core;
using Serilog;
using WTG.Logging;

namespace CargoWise.Winzor.AppServer.Helpers
{
	public static class LoggerConfigurationExtensions
	{
		public static LoggerConfiguration AddEnrichers(this LoggerConfiguration configuration, Guid? versionBrokerProcessCorrelationId, Guid? sessionBrokerProcessCorrelationId, Guid? appServerProcessCorrelationId, string version, string hostname, bool isDevelopmentOrIntegrationTestingOrDAT)
		{
			ArgumentNullException.ThrowIfNull(appServerProcessCorrelationId);
			if (!isDevelopmentOrIntegrationTestingOrDAT)
			{
				ArgumentNullException.ThrowIfNull(versionBrokerProcessCorrelationId);
				ArgumentNullException.ThrowIfNull(sessionBrokerProcessCorrelationId);
			}

			return configuration
				.Enrich.WithProperty(SystemLogProperties.MachineName, Environment.MachineName)
				.Enrich.WithProperty(SystemLogProperties.AppName, Assembly.GetExecutingAssembly().GetName().Name)
				.Enrich.WithProperty("VersionBrokerProcessCorrelationId", versionBrokerProcessCorrelationId)
				.Enrich.WithProperty("SessionBrokerProcessCorrelationId", sessionBrokerProcessCorrelationId)
				.Enrich.WithProperty("AppServerProcessCorrelationId", appServerProcessCorrelationId)
				.Enrich.WithProperty((NoResString)"Version", version)
				.Enrich.WithProperty((NoResString)"Hostname", hostname)
				.Enrich.WithThreadInfo()
				.Enrich.FromLogContext();
		}
	}
}
