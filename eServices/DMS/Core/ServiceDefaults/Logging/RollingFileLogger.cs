using log4net;
using Microsoft.Extensions.Logging;

namespace eServices.Dms.Core.ServiceDefaults.Logging;

public sealed class RollingFileLogger(string name, ILog logger, IExternalScopeProvider? scopeProvider) : ILogger
{
	private readonly string name = name;
	private readonly ILog logger = logger;

	internal IExternalScopeProvider? ScopeProvider { get; set; } = scopeProvider;
	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => ScopeProvider?.Push(state) ?? default!;

	[ThreadStatic]
	private static StringWriter? stringWriter;

	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
			return;

		stringWriter ??= new StringWriter();
		stringWriter.Write($"{DateTimeOffset.Now:yyyy-MM-ddTHH:mm:ss.fffzzz} " +
			$"{logLevel switch
			{
				LogLevel.Trace => "TRC",
				LogLevel.Debug => "DBG",
				LogLevel.Information => "INF",
				LogLevel.Warning => "WRN",
				LogLevel.Error => "ERR",
				LogLevel.Critical => "FAL",
				_ => "INF"
			}}");

		ScopeProvider?.ForEachScope((scope, state) =>
		{
			if (scope is IEnumerable<KeyValuePair<string, object?>> properties)
			{
				foreach (var prop in properties)
				{
					state.Write($" {prop.Key}={prop.Value}");
				}
			}
			else
			{
				stringWriter.Write(" ");
				stringWriter.Write(scope);
			}
		}, stringWriter);
		stringWriter.Write($" Source={name}");

		WriteIndented(stringWriter, formatter(state, exception));
		if (exception != null)
		{
			WriteIndented(stringWriter, exception.ToString());
		}

		static void WriteIndented(StringWriter writer, string text)
		{
			using var reader = new StringReader(text);
			while (reader.ReadLine() is string line)
			{
				writer.WriteLine();
				writer.Write("   ");
				writer.Write(line);
			}
		}

		var sb = stringWriter.GetStringBuilder();
		if (sb.Length == 0)
		{
			return;
		}
		string computedString = sb.ToString();
		sb.Clear();
		if (sb.Capacity > 1024)
		{
			sb.Capacity = 1024;
		}

		logger.Info(computedString);
	}

	public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
}