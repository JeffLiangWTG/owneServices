using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Rating.Business.RateSelector
{
	public class MemoryLogger : ILogger
	{
		public void Log(LogType type, string message)
		{
			var logEvent = new LogEvent
			{
				LogTime = ZDateTime.Now,
				Level = type,
				Message = message
			};

			lock (logs)
			{
				logs.Add(logEvent);
			}

			LogsChanged?.Invoke(this, new LogEventArgs(logEvent));
		}

		public void Log(LogType type, string message, Exception ex)
		{
			var logEvent = new LogEvent
			{
				LogTime = ZDateTime.Now,
				Level = type,
				Message = message,
				Exception = ex
			};

			lock (logs)
			{
				logs.Add(logEvent);
			}

			LogsChanged?.Invoke(this, new LogEventArgs(logEvent));
		}

		public void Clear()
		{
			lock (logs)
			{
				logs.Clear();
			}
			LogsChanged?.Invoke(this, new LogEventArgs(null));
		}

		public IEnumerable<LogEvent> Logs
		{
			get
			{
				lock (logs)
				{
					return logs.ToArray();
				}
			}
		}

		/// <summary>
		/// Fired when logs are added or cleared
		/// </summary>
		public event EventHandler<LogEventArgs> LogsChanged;

		readonly List<LogEvent> logs = new List<LogEvent>();

		public class LogEvent
		{
			public ZDateTime LogTime { get; set; }
			public LogType Level { get; set; }
			public string Message { get; set; }
			public Exception Exception { get; set; }

			public override string ToString()
			{
				return $"{Level} : {Message}";
			}
		}

		public class LogEventArgs : EventArgs
		{
			public LogEventArgs(LogEvent logAdded)
			{
				LogAdded = logAdded;
			}

			/// <summary>
			/// Log line added, or <c>null</c> if logs were cleared
			/// </summary>
			public LogEvent LogAdded { get; }
		}
	}
}
