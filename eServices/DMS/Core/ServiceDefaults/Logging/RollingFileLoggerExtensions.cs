using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace eServices.Dms.Core.ServiceDefaults.Logging;

public static class RollingFileLoggerExtensions
{
	public static ILoggingBuilder AddRollingFileLogger(this ILoggingBuilder builder)
	{
		builder.AddConfiguration();
		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, RollingFileLoggerProvider>());
		LoggerProviderOptions.RegisterProviderOptions<RollingFileLoggerConfiguration, RollingFileLoggerProvider>(builder.Services);
		return builder;
	}

	public static ILoggingBuilder AddRollingFileLogger(this ILoggingBuilder builder, Action<RollingFileLoggerConfiguration> configure)
	{
		builder.AddRollingFileLogger();
		builder.Services.Configure(configure);
		return builder;
	}
}
