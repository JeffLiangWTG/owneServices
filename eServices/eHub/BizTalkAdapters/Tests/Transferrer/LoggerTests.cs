using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using log4net;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Newtonsoft.Json;

namespace CargoWise.eHub.BizTalkAdapters.Tests.Transferrer
{
	[NonParallelizable]
	public class LoggerTests
	{
		private readonly string LogPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Flat");
		private readonly string StructPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Structured");

		[SetUp]
		public void SetUp()
		{
			if (Directory.Exists(LogPath))
			{
				foreach (var fileInfo in new DirectoryInfo(LogPath).GetFiles())
				{
					if (File.Exists(fileInfo.FullName))
						fileInfo.Delete();
				}
			}

			if (Directory.Exists(StructPath))
			{
				foreach (var fileInfo in new DirectoryInfo(StructPath).GetFiles())
				{
					if (File.Exists(fileInfo.FullName))
						fileInfo.Delete();
				}
			}
		}

		[Test]
		public void LoggerFactory_CreateLogger()
		{
			var flatLogFile = $"{TestContext.CurrentContext.WorkDirectory}\\Flat\\{nameof(LoggerFactory_CreateLogger)}.Receive.log";
			var structuredLogFile = $"{TestContext.CurrentContext.WorkDirectory}\\Structured\\{nameof(LoggerFactory_CreateLogger)}.Receive.{DateTime.Today.ToString("yyyyMMdd")}.log";

			var config = new XmlDocument();
			config.LoadXml(
				$"<Config>" +
					$"<LogFormat>{TransferrerLogFormat.Both}</LogFormat>" +
					$"<LogDir>{TestContext.CurrentContext.WorkDirectory}\\Flat</LogDir>" +
					$"<StructuredLogDir>{TestContext.CurrentContext.WorkDirectory}\\Structured</StructuredLogDir>" +
					$"<LogMaxSize>1</LogMaxSize><LogMaxCount>1</LogMaxCount>" +
					$"<LogLevel>{TransferrerLogLevel.Trace}</LogLevel>" +
				$"</Config>");
			var logger = LoggerFactory.Instance.CreateLogger(nameof(LoggerFactory_CreateLogger), "Receive", config);

			logger.InfoFormat("Test {0}", "message");
			LogManager.Shutdown();

			Assert.Multiple(() =>
			{
				Assert.That(File.ReadAllText(flatLogFile), Does.Match(@"^[a-zA-Z]{3}, \d{2} [a-zA-Z]{3} \d{4} \d{2}:\d{2}:\d{2} [+-]\d{2}:\d{2} \[INFO  \] Test message" + Environment.NewLine));
				Assert.That(File.ReadAllText(structuredLogFile), Does.Match(@"^{""@timestamp"":""\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{3}Z"",""CategoryName"":""LoggerFactory_CreateLogger.Receive"",""Severity"":""Info"",""Body"":""Test message""}" + Environment.NewLine));
			});
		}

		[Test]
		public void LoggerFactory_CreateLogger_ChangeFormat()
		{
			var flatCount1 = 20;
			var flatCount2 = 30;
			var structuredCount1 = 40;
			var structuredCount2 = 50;
			var bothCount = 3;

			var logName = nameof(LoggerFactory_CreateLogger_ChangeFormat);
			var logger = LoggerFactory.Instance.CreateLogger(logName, "Receive", CreateConfig(TransferrerLogFormat.Flat));
			foreach (var _ in Enumerable.Range(0, flatCount1))
			{
				logger.Info("Test message");
			}

			logger = LoggerFactory.Instance.CreateLogger(logName, "Receive", CreateConfig(TransferrerLogFormat.Structured));
			foreach (var _ in Enumerable.Range(0, structuredCount1))
			{
				logger.Info("Test message");
			}

			logger = LoggerFactory.Instance.CreateLogger(logName, "Receive", CreateConfig(TransferrerLogFormat.Both));
			foreach (var _ in Enumerable.Range(0, bothCount))
			{
				logger.Info("Test message");
			}

			logger = LoggerFactory.Instance.CreateLogger(logName, "Receive", CreateConfig(TransferrerLogFormat.Flat));
			foreach (var _ in Enumerable.Range(0, flatCount2))
			{
				logger.Info("Test message");
			}

			logger = LoggerFactory.Instance.CreateLogger(logName, "Receive", CreateConfig(TransferrerLogFormat.Structured));
			foreach (var _ in Enumerable.Range(0, structuredCount2))
			{
				logger.Info("Test message");
			}

			LogManager.Shutdown();
			Assert.That(File.ReadAllLines(new DirectoryInfo(LogPath).GetFiles().First().FullName).Length,
				Is.EqualTo(flatCount1 + flatCount2 + bothCount));
			Assert.That(File.ReadAllLines(new DirectoryInfo(StructPath).GetFiles().First().FullName).Length,
				Is.EqualTo(structuredCount1 + structuredCount2 + bothCount));
		}

		public XmlDocument CreateConfig(TransferrerLogFormat format)
		{
			var config = new XmlDocument();
			config.LoadXml(
				$"<Config>" +
				$"<LogFormat>{format}</LogFormat>" +
				$"<LogDir>{LogPath}</LogDir>" +
				$"<StructuredLogDir>{StructPath}</StructuredLogDir>" +
				$"<LogMaxSize>11</LogMaxSize><LogMaxCount>11</LogMaxCount>" +
				$"<LogLevel>{TransferrerLogLevel.Trace}</LogLevel>" +
				$"</Config>");

			return config;
		}

		[Test]
		public void LoggerFactory_LogException()
		{
			var flatLogFile = $"{TestContext.CurrentContext.WorkDirectory}\\Flat\\{nameof(LoggerFactory_LogException)}.log";
			var structuredLogFile = $"{TestContext.CurrentContext.WorkDirectory}\\Structured\\{nameof(LoggerFactory_LogException)}.{DateTime.Today.ToString("yyyyMMdd")}.log";

			var config = new XmlDocument();
			config.LoadXml(
				$"<Config>" +
					$"<LogFormat>{TransferrerLogFormat.Both}</LogFormat>" +
					$"<LogDir>{TestContext.CurrentContext.WorkDirectory}\\Flat</LogDir>" +
					$"<StructuredLogDir>{TestContext.CurrentContext.WorkDirectory}\\Structured</StructuredLogDir>" +
					$"<LogMaxSize>1</LogMaxSize><LogMaxCount>1</LogMaxCount>" +
					$"<LogLevel>{TransferrerLogLevel.Trace}</LogLevel>" +
				$"</Config>");
			var logger = LoggerFactory.Instance.CreateLogger(nameof(LoggerFactory_LogException), config);

			try
			{
				Math.DivRem(1, 0, out _);
			}
			catch (Exception ex)
			{
				logger.Error("Test error", new TransferrerException("Test exception", ex));
			}
			LogManager.Shutdown();

			var flatLog = File.ReadAllText(flatLogFile);
			var structuredLogLines = File.ReadAllLines(structuredLogFile);
			var structuredLogMessage = JObject.Parse(structuredLogLines[0]);

			Assert.Multiple(() =>
			{
				Assert.That(flatLog, Does.Match(@"(?m:^[a-zA-Z]{3}, \d{2} [a-zA-Z]{3} \d{4} \d{2}:\d{2}:\d{2} [+-]\d{2}:\d{2} \[ERROR \] Test error\r\n" +
					@"CargoWise\.eHub\.BizTalkAdapters\.Transferrer\.Adapter\.TransferrerException: Test exception ---> System\.DivideByZeroException: Attempted to divide by zero\.\r\n" +
					@"   at System\.Math\.DivRem\(Int32 a, Int32 b, Int32& result\)\r\n)"));
				Assert.That(structuredLogLines, Has.Length.EqualTo(1));
				Assert.That(structuredLogMessage["@timestamp"]?.ToString(), Is.Not.Null);
				Assert.That(structuredLogMessage["Severity"]?.ToString(), Is.EqualTo("Error"));
				Assert.That(structuredLogMessage["CategoryName"]?.ToString(), Is.EqualTo("LoggerFactory_LogException"));
				Assert.That(structuredLogMessage["Body"]?.ToString(), Does.StartWith(
					"Test error\r\nCargoWise.eHub.BizTalkAdapters.Transferrer.Adapter.TransferrerException: Test exception ---> System.DivideByZeroException: Attempted to divide by zero.\r\n" +
					"   at System.Math.DivRem(Int32 a, Int32 b, Int32& result)\r\n"));
			});
		}

		[Test]
		public void CreateLogger_MultiThread_StructContext()
		{
			const int count = 10;
			var logger = LoggerFactory.Instance.CreateLogger(nameof(CreateLogger_MultiThread_StructContext), "Send", CreateConfig(TransferrerLogFormat.Structured));
			var tasks = Enumerable.Range(0, count).Select(x => Task.Run(() => logger.Info(x))).ToArray();
			Task.WaitAll(tasks);
			LogManager.Shutdown();

			var resources = File.ReadAllLines(new DirectoryInfo(StructPath).GetFiles().First().FullName)
				.Where(x => !string.IsNullOrEmpty(x))
				.Select(JObject.Parse)
				.Select(x => x.GetValue("CategoryName")?.Value<string>() ?? string.Empty)
				.ToArray();
			Assert.That(resources.Count(), Is.EqualTo(count));
			Assert.That(resources.All(x => x == $"{nameof(CreateLogger_MultiThread_StructContext)}.Send"), Is.True);
		}

		[Test]
		public void LoggerFactory_CreateLogger_Multi_Thread()
		{
			List<Task> loggingTasks = new List<Task>();
			const int loop = 100;
			for (int i = 0; i < loop; i++)
			{
				var index = i;
				loggingTasks.Add(Task.Run(() => LoggingTask(index, LogPath, StructPath)));
			}
			Task.WaitAll(loggingTasks.ToArray());
			LogManager.Shutdown();

			var logFiles = new DirectoryInfo(LogPath).GetFiles();
			var structFiles = new DirectoryInfo(StructPath).GetFiles();
			Assert.That(logFiles.Length, Is.EqualTo(1));
			Assert.That(structFiles.Length, Is.EqualTo(1));
			Assert.That(File.ReadAllLines(logFiles.First().FullName).Length, Is.EqualTo(loop * 10));
			Assert.That(File.ReadAllLines(structFiles.First().FullName).Length, Is.EqualTo(loop * 10));
		}

		[Test]
		public void TestStructured_MicrosoftOpenTelemetryFormat()
		{
			var longMessage = "This is a message longer than 1024 characters: \\ Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum. Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem. Maecenas nec odio et ante tincidunt tempus. Donec vitae sapien ut libero venenatis faucibus. Nullam quis ante. Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. Donec sodales sagittis magna. Sed consequat, leo eget bibendum sodales, augue velit cursus nunc,";

			var logger = LoggerFactory.Instance.CreateLogger(nameof(TestStructured_MicrosoftOpenTelemetryFormat), "Send", CreateConfig(TransferrerLogFormat.Structured));
			var exceptionMessage = string.Empty;
			try
			{
				throw new Exception("Test exception");
			}
			catch (Exception ex)
			{
				exceptionMessage = ex.ToString();
				logger.Error(longMessage, ex);
			}
			LogManager.Shutdown();

			var logDetails = File.ReadAllLines(new DirectoryInfo(StructPath).GetFiles().First().FullName)
				.Where(x => !string.IsNullOrEmpty(x))
				.Select(x => (JObject)JsonConvert.DeserializeObject(x, new JsonSerializerSettings{ DateParseHandling = DateParseHandling.None }))
				.FirstOrDefault();

			Assert.Multiple(() =>
			{
				Assert.That(logDetails, Is.Not.Null);
				Assert.That(logDetails["@timestamp"]?.Value<string>() ?? string.Empty, Does.Match(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z"));
				Assert.That(logDetails["Body"]?.Value<string>().Length ?? 0, Is.EqualTo(1024));
				Assert.That(logDetails["Body"]?.Value<string>() ?? string.Empty, Is.EqualTo(longMessage.Substring(0, 1024)));
				Assert.That(logDetails["Exception"]?.Value<string>() ?? string.Empty, Is.EqualTo(exceptionMessage));
				Assert.That(logDetails["CategoryName"]?.Value<string>() ?? string.Empty, Is.EqualTo("TestStructured_MicrosoftOpenTelemetryFormat.Send"));
				Assert.That(logDetails["Severity"]?.Value<string>() ?? string.Empty, Is.EqualTo("Error"));
				Assert.That(logDetails["Attributes"]?["OriginalMessage"]?.Value<string>() ?? string.Empty, Is.EqualTo(longMessage + Environment.NewLine + exceptionMessage));
			});
		}

		private static void LoggingTask(int index, string logPath, string structPath)
		{
			var config = new XmlDocument();
			config.LoadXml(
				$"<Config>" +
				$"<LogFormat>{TransferrerLogFormat.Both}</LogFormat>" +
				$"<LogDir>{logPath}</LogDir>" +
				$"<StructuredLogDir>{structPath}</StructuredLogDir>" +
				$"<LogMaxSize>11</LogMaxSize><LogMaxCount>11</LogMaxCount>" +
				$"<LogLevel>{TransferrerLogLevel.Trace}</LogLevel>" +
				$"</Config>");

			var logger = LoggerFactory.Instance.CreateLogger(nameof(LoggerFactory_CreateLogger_Multi_Thread), "Receive", config);
			logger.Info($"{index} --- Test {Thread.CurrentThread.ManagedThreadId}");
			logger.Info($"{index} --- Test {Thread.CurrentThread.ManagedThreadId}");
			logger.Info($"{index} --- Test {Thread.CurrentThread.ManagedThreadId}");
			logger.Info($"{index} --- Test {Thread.CurrentThread.ManagedThreadId}");
			logger.Info($"{index} --- Test {Thread.CurrentThread.ManagedThreadId}");
			logger.Info($"{index} --- Test {Thread.CurrentThread.ManagedThreadId}");
			logger.Info($"{index} --- Test {Thread.CurrentThread.ManagedThreadId}");
			logger.Info($"{index} --- Test {Thread.CurrentThread.ManagedThreadId}");
			logger.Info($"{index} --- Test {Thread.CurrentThread.ManagedThreadId}");
			logger.Info($"{index} --- Test {Thread.CurrentThread.ManagedThreadId}");
		}
	}
}
