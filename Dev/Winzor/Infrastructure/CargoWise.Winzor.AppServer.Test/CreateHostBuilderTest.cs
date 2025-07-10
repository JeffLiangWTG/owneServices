using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using WinzorFramework;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace CargoWise.Winzor.AppServer.Test
{
	public class CreateHostBuilderTest
	{
		const string TestGuid1 = "62FA647C-AD54-4BCC-A860-E5A2664B019D";
		const string TestGuid2 = "e8d0d3b8-2b73-4e70-a754-512d82d4994e";

		[Test]
		public async Task TestCreateHostBuilderWhenMissingCorrelationIdSuccessOnDevelopmentAndDAT()
		{
			var hostBuilder = CreateTestHostBuilder(isProd: false, TestGuid1, null, null);
			using var host = hostBuilder.Build();
			await host.StartAsync();
			await host.StopAsync();
		}

		[TestCase(true, TestGuid1, null, TestGuid2, "sessionBrokerProcessCorrelationId")]
		[TestCase(false, null, TestGuid1, TestGuid2, "appServerProcessCorrelationId")]
		[TestCase(true, null, TestGuid1, TestGuid2, "appServerProcessCorrelationId")]
		[TestCase(true, TestGuid1, TestGuid2, null, "versionBrokerProcessCorrelationId")]
		public void TestCreateHostBuilderThrowArgumentNullException(bool isProd, string appServerProcessCorrelationId, string sessionBrokerProcessCorrelationId, string versionBrokerProcessCorrelationId, string paramMessage)
		{
			var hostBuilder = CreateTestHostBuilder(isProd, appServerProcessCorrelationId, sessionBrokerProcessCorrelationId, versionBrokerProcessCorrelationId);
			var exception = Assert.Throws<ArgumentNullException>(() => hostBuilder.Build());
			Assert.That(exception!.Message, Does.Contain(paramMessage));
		}

		[Test]
		public async Task TestWinzorDispatcherShouldBeDisposedOnlyWhenHostingIsDisposing()
		{
			var hosting = CreateTestHostBuilder(false, TestGuid1, null, null).Build();
			hosting.Start();
			var dispatcherFromHosting = hosting.Services.GetRequiredService<WinzorDispatcher>();

			await hosting.StopAsync();
			Assert.That(dispatcherFromHosting.IsDisposed, Is.False);

			hosting.Dispose();
			Assert.That(dispatcherFromHosting.IsDisposed, Is.True);
		}

		[Test]
		public async Task IgnoreNoBrowserRendererError()
		{
			using var host = CreateTestHostBuilder(false, TestGuid1, null, null).Build();
			host.Start();
			var testLogger = new TestLogger<Renderer>();
			var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
			using (var testLoggerProvider = new TestLoggerProvider(testLogger))
			{
				loggerFactory.AddProvider(testLoggerProvider);
				var logger = loggerFactory.CreateLogger("Microsoft.AspNetCore.Components.RenderTree.Renderer");
				var innerException = new InvalidOperationException("Error: There is no browser renderer with ID 1.");
				var aggregateException = new AggregateException(innerException);
				logger.LogError(new EventId(111, "CircuitUnhandledException"), aggregateException, "Unhandled exception in circuit '{CircuitId}'.", 1);
			}
			var logEntry = testLogger.LogEntries.FirstOrDefault(entry => entry.Exception is AggregateException);
			Assert.That(logEntry, Is.Null);
			Assert.That(logEntry?.Exception.InnerException.Message, Is.Not.EqualTo("Error: There is no browser renderer with ID 1."));
			await host.StopAsync();
		}

		class TestLoggerProvider : ILoggerProvider
		{
			readonly ILogger _logger;

			public TestLoggerProvider(ILogger logger)
			{
				_logger = logger;
			}

			public ILogger CreateLogger(string categoryName) => _logger;

			public void Dispose() { }
		}

		class TestLogger<T> : ILogger<T>
		{
			readonly List<LogEntry> _logEntries = new();

			public IReadOnlyList<LogEntry> LogEntries => _logEntries;

			public IDisposable BeginScope<TState>(TState state) => null;

			public bool IsEnabled(LogLevel logLevel) => true;

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
			{
				_logEntries.Add(new LogEntry
				{
					LogLevel = logLevel,
					EventId = eventId,
					State = state,
					Exception = exception,
					Message = formatter(state, exception)
				});
			}

			public class LogEntry
			{
				public LogLevel LogLevel { get; set; }
				public EventId EventId { get; set; }
				public object State { get; set; }
				public Exception Exception { get; set; }
				public string Message { get; set; }
			}
		}

		IHostBuilder CreateTestHostBuilder(bool isProd, string appServerProcessCorrelationId, string sessionBrokerProcessCorrelationId, string versionBrokerProcessCorrelationId)
		{
			static Guid? AsGuid(string src) => src is not null ? Guid.Parse(src) : null;
			var testEnv = bool.TryParse(Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var datIsTesting) && datIsTesting ? "DAT" : Environments.Development;
			var hostBuilder = Program.CreateHostBuilder(Array.Empty<string>())
				.UseEnvironment(isProd ? Environments.Production : testEnv)
				.ConfigureServices(services => services.PostConfigure<CargoWiseOptions>(o =>
				{
					o.AppServerProcessCorrelationId = AsGuid(appServerProcessCorrelationId);
					o.SessionBrokerProcessCorrelationId = AsGuid(sessionBrokerProcessCorrelationId);
					o.VersionBrokerProcessCorrelationId = AsGuid(versionBrokerProcessCorrelationId);
				}));
			return hostBuilder;
		}
	}
}
