using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace BArchitecture;
class TestLogger<T> : ILogger<T>
{
	public List<string> logs = new();
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
	{
		logs.Add(formatter(state, exception));
	}

	public IDisposable BeginScope<TState>(TState state) where TState : notnull
	{
		throw new NotImplementedException();
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		throw new NotImplementedException();
	}
}
