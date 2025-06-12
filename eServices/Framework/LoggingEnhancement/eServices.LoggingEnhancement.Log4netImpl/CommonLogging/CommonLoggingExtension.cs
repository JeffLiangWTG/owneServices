using System.Collections.Generic;
using System.Linq;
using Common.Logging;

namespace eServices.LoggingEnhancement.Log4netImpl.CommonLogging
{
	public static class CommonLoggingExtension
	{
		public const string messageId = nameof(messageId);
		public const string activityId = nameof(activityId);

		public static ILog WithContext(this ILog logger, params KeyValuePair<string, object>[] context)
		{
			LoggingProxy proxy;
			if (logger is LoggingProxy loggingProxy)
			{
				proxy = loggingProxy;
				foreach (var entry in context)
				{
					if (!string.IsNullOrEmpty(entry.Key))
					{
						proxy.ContextData[entry.Key] = entry.Value;
					}
				}
			}
			else
			{
				proxy = new LoggingProxy(logger) { ContextData = context.ToDictionary(x => x.Key, x => x.Value) };
			}

			return proxy;
		}

		public static ILog WithContext(this ILog logger, string key, object value)
		{
			if (string.IsNullOrEmpty(key)) return logger;

			LoggingProxy proxy;
			if (logger is LoggingProxy loggingProxy)
			{
				proxy = loggingProxy;
				proxy.ContextData[key] = value;
			}
			else
			{
				proxy = new LoggingProxy(logger) { ContextData = new Dictionary<string, object>{{key, value}} };
			}

			return proxy;
		}
	}
}
