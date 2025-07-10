using System;
using System.Collections.Generic;
using System.Linq;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Web
{
	static class ExtraValidationLogger
	{
		const string ExtraValidationLogsList = nameof(ExtraValidationLogsList);

		[ThreadStatic]
		static Dictionary<string, string> logs;

		[ThreadSafe]
		static Dictionary<string, string> Logs
		{
			get
			{
				if (logs == null)
				{
					logs = new Dictionary<string, string>();
				}

				return logs;
			}
		}

		[ThreadStatic]
		static object logsLock;

		[ThreadSafe]
		static object LogsLock
		{
			get
			{
				if (logsLock == null)
				{
					logsLock = new object();
				}

				return logsLock;
			}
		}

		public static void LogExtraValidationMessageOnce(string key, string message)
		{
			lock (LogsLock)
			{
				if (!Logs.ContainsKey(key))
				{
					Logs[key] = message;
				}
			}
		}

		public static IEnumerable<string> GetExtraValidationMessages()
		{
			lock (LogsLock)
			{
				return Logs.Values.ToArray();
			}
		}

		public static void RemoveAllLogs()
		{
			lock (LogsLock)
			{
				Logs.Clear();
			}
		}
	}
}
