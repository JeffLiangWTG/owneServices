using System.Collections.Concurrent;
using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace eServices.Dms.Core.ServiceDefaults.Logging;

[ProviderAlias("RollingFile")]
public sealed class RollingFileLoggerProvider : ILoggerProvider, ISupportExternalScope
{
	private readonly ILog rollingFileLogger;
	private readonly RollingFileLoggerConfiguration config;
	private readonly ConcurrentDictionary<string, RollingFileLogger> loggers = new(StringComparer.OrdinalIgnoreCase);
	private IExternalScopeProvider? scopeProvider = null;

	public RollingFileLoggerProvider(IOptions<RollingFileLoggerConfiguration> config)
	{
		this.config = config.Value;
		rollingFileLogger = GetLogger(this.config);
	}

	public ILogger CreateLogger(string categoryName)
	{
		return loggers.GetOrAdd(categoryName, name => new RollingFileLogger(categoryName, rollingFileLogger, scopeProvider));
	}

	private ILog GetLogger(RollingFileLoggerConfiguration config)
	{
		var hierarchy = (Hierarchy)LogManager.GetRepository();
		hierarchy.Configured = true;

		var appender = new RollingFileAppender
		{
			AppendToFile = true,
			File = config.File ?? $"logs/{Assembly.GetEntryAssembly()!.GetName().Name}.log",
			Layout = new PatternLayout(),
			MaxSizeRollBackups = config.MaxFileCount ?? 10,
			MaximumFileSize = config.MaxFileSize ?? "10MB",
			RollingStyle = RollingFileAppender.RollingMode.Size,
			StaticLogFileName = false,
			Threshold = Level.All
		};
		appender.ActivateOptions();

		var logger = LogManager.GetLogger(nameof(RollingFileLogger));
		((Logger)logger.Logger).AddAppender(appender);
		return logger;
	}

	public void Dispose() { }

	public void SetScopeProvider(IExternalScopeProvider scopeProvider)
	{
		this.scopeProvider = scopeProvider;

		foreach (var logger in loggers)
		{
			logger.Value.ScopeProvider = this.scopeProvider;
		}
	}
}
