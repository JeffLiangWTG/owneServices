using System;
using System.IO;
using System.Text;
using CargoWise.Winzor.AppServer.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Serilog;

namespace CargoWise.Winzor.AppServer.Test
{
	public class SerilogLoggingTest
	{
		DirectoryInfo tempContent;
		string outputTemplateEnrichersAndMessageOnly;
		Guid versionBrokerProcessCorrelationId;
		Guid sessionBrokerProcessCorrelationId;
		Guid appServerProcessCorrelationId;
		string version;
		string hostname;

		[SetUp]
		public void SerilogLoggingSetup()
		{
			outputTemplateEnrichersAndMessageOnly = "{MachineName} {Version} {AppName} {Hostname} {VersionBrokerProcessCorrelationId} {SessionBrokerProcessCorrelationId} {AppServerProcessCorrelationId} {Message:lj}";

			tempContent = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Logs"));
			versionBrokerProcessCorrelationId = Guid.NewGuid();
			sessionBrokerProcessCorrelationId = Guid.NewGuid();
			appServerProcessCorrelationId = Guid.NewGuid();
			version = "1.0.0.0";
			hostname = "testhostname";
			Log.Logger = new LoggerConfiguration()
				.AddEnrichers(versionBrokerProcessCorrelationId, sessionBrokerProcessCorrelationId, appServerProcessCorrelationId, version, hostname, true)
				.WriteTo.File(Path.Combine(tempContent.FullName, "log.txt"), outputTemplate: outputTemplateEnrichersAndMessageOnly)
				.CreateLogger();
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
			Assert.That(readContents[2], Is.EqualTo("CargoWise.Winzor.AppServer"));
			Assert.That(readContents[3], Is.EqualTo(hostname));
			Assert.That(Guid.Parse(readContents[4]), Is.EqualTo(versionBrokerProcessCorrelationId));
			Assert.That(Guid.Parse(readContents[5]), Is.EqualTo(sessionBrokerProcessCorrelationId));
			Assert.That(Guid.Parse(readContents[6]), Is.EqualTo(appServerProcessCorrelationId));
			Assert.That(readContents[7], Is.EqualTo("I'm logging something here!"));
		}

		[Test]
		public void SerilogWriteToPathIsCorrect([Values(".Development", ".Production", "")] string environment)
		{
			var appSettingsPath = Path.Combine(AppContext.BaseDirectory, $"appsettings{environment}.json");

			Assert.That(File.Exists(appSettingsPath), Is.True);

			var jsonContent = File.ReadAllText(appSettingsPath);
			var configJson = JsonConvert.DeserializeObject<JObject>(jsonContent);

			var data = configJson.SelectToken("$.Serilog.WriteTo[?(@.Name=='Async')].Args.configure[?(@.Name=='File')].Args.path");
			Assert.That(data.ToString(), Is.EqualTo("%LOCALAPPDATA%/WiseTech Global/AppServer/as-.log"));
		}
	}
}
