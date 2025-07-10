using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	public class DelegatingSink : ILogEventSink
	{
		readonly Action<LogEvent> _write;

		public DelegatingSink(Action<LogEvent> write)
		{
			_write = write ?? throw new ArgumentNullException(nameof(write));
		}

		public void Emit(LogEvent logEvent)
		{
			_write(logEvent);
		}

		public static LogEvent GetLogEvent(Action<ILogger> writeAction)
		{
			LogEvent result = null;
			var logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.Sink(new DelegatingSink(le => result = le))
				.CreateLogger().ForContext<DelegatingSink>();

			writeAction(logger);
			return result;
		}
	}
}
