using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Blazor.SessionBroker.Helpers;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using WTG.Logging;
using WTG.Logging.Sinks;

namespace CargoWise.Blazor.SessionBroker.Test
{
	public class SerilogLoggingTest
	{
		string outputTemplateEnrichersAndMessageOnly;
		DirectoryInfo tempContent;
		Guid versionBrokerProcessCorrelationId;
		Guid sessionBrokerProcessCorrelationId;
		string version;
		string hostname;
		const int PerformanceIterations = 10;
		Mock<ILogEventSink> MockSink;

		[SetUp]
		public void SerilogLoggingSetup()
		{
			outputTemplateEnrichersAndMessageOnly = "{MachineName} {Version} {AppName} {Hostname} {VersionBrokerProcessCorrelationId} {SessionBrokerProcessCorrelationId} {Message:lj}";

			tempContent = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Logs"));
			versionBrokerProcessCorrelationId = Guid.NewGuid();
			sessionBrokerProcessCorrelationId = Guid.NewGuid();
			version = "1.0.0.0";
			hostname = "testhostname";

			Log.Logger = new LoggerConfiguration()
				.AddEnrichers(versionBrokerProcessCorrelationId, sessionBrokerProcessCorrelationId, version, hostname, true)
				.WriteTo.File(Path.Combine(tempContent.FullName, "log.txt"), outputTemplate: outputTemplateEnrichersAndMessageOnly)
				.CreateLogger();

			MockSink = new Mock<ILogEventSink>();
			MockSink.Setup(m => m.Emit(It.IsAny<LogEvent>())).Callback(() => { System.Threading.Thread.Sleep(150); });
		}

		[TearDown]
		public void TearDown()
		{
			Log.CloseAndFlush();
			tempContent.Delete(true);
		}

		[Test]
		public void TestSerilogCreatesLogFile()
		{
			Log.Information("I'm logging something here!");

			string[] readContents = null;

			using (var fs = new FileStream(Path.Combine(tempContent.FullName, "log.txt"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var sr = new StreamReader(fs, Encoding.Default))
			{
				readContents = sr.ReadToEnd().Split(" ", outputTemplateEnrichersAndMessageOnly.Split(" ").Length);
			}

			Assert.That(tempContent.Exists, Is.True);
			Assert.That(readContents[0], Is.EqualTo($@"{Environment.MachineName}"));
			Assert.That(readContents[1], Is.EqualTo(version));
			Assert.That(readContents[2], Is.EqualTo("CargoWise.Blazor.SessionBroker"));
			Assert.That(readContents[3], Is.EqualTo(hostname));
			Assert.That(Guid.Parse(readContents[4]), Is.EqualTo(versionBrokerProcessCorrelationId));
			Assert.That(Guid.Parse(readContents[5]), Is.EqualTo(sessionBrokerProcessCorrelationId));
			Assert.That(readContents[6], Is.EqualTo("I'm logging something here!"));
		}

		[Test]
		public void SerilogWriteToPathIsCorrect([Values(".Development", ".Production", "")] string environment)
		{
			var appSettingsPath = Path.Combine(AppContext.BaseDirectory, $"appsettings{environment}.json");

			Assert.That(File.Exists(appSettingsPath), Is.True);

			var jsonContent = File.ReadAllText(appSettingsPath);
			var configJson = JsonConvert.DeserializeObject<JObject>(jsonContent);

			var data = configJson.SelectToken("$.Serilog.WriteTo[?(@.Name=='Async')].Args.configure[?(@.Name=='File')].Args.path");
			Assert.That(data, Is.Not.Null);
			Assert.That(data.ToString(), Is.EqualTo("%LOCALAPPDATA%/WiseTech Global/SessionBroker/sb-.log"));
		}

		[Test]
		public void SeriLog_SyncSinkLog_VerifySlowPerformance()
		{
			var sw = new Stopwatch();

			using (var logger = new LoggerConfiguration()
						.AddEnrichers(versionBrokerProcessCorrelationId, sessionBrokerProcessCorrelationId, version, hostname, true)
						.WriteTo.Sink(MockSink.Object)
						.CreateLogger())
			{
				sw.Start();
				for (var i = 0; i < PerformanceIterations; i++)
				{
					logger.Write(LogEventLevel.Information, "Test Log");
				}
				sw.Stop();
			}

			Assert.That(sw.Elapsed.TotalMilliseconds, Is.GreaterThan(1000));
			Assert.That(sw.Elapsed.TotalMilliseconds, Is.LessThan(2000));
		}

		[Test]
		public void SeriLog_AsyncSinkLog_VerifyFastPerformance()
		{
			var sw = new Stopwatch();

			using (var logger = new LoggerConfiguration()
						.AddEnrichers(versionBrokerProcessCorrelationId, sessionBrokerProcessCorrelationId, version, hostname, true)
						.WriteTo.Async(a => { a.Sink(MockSink.Object); })
						.CreateLogger())
			{
				sw.Start();
				for (var i = 0; i < PerformanceIterations; i++)
				{
					logger.Write(LogEventLevel.Information, "Test Log");
				}
				sw.Stop();
			}

			Assert.That(sw.Elapsed.TotalMilliseconds, Is.LessThan(100));
		}
	}
}
