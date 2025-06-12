using System;
using Dat.Integration;
using Microsoft.Extensions.Logging;

namespace eServices.BuildTools.DatDeploy
{
	internal class DeployTaskLogger : ILogger
	{
		private ITaskLogger taskLogger;

		public DeployTaskLogger(ITaskLogger taskLogger)
		{
			this.taskLogger = taskLogger;
		}

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			taskLogger.RecordInfo(formatter(state, exception));
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return taskLogger.RecordTask(state.ToString());
		}
	}
}