using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using CargoWise.eHub.Core.Logging;
using Common.Logging.Log4Net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.Logging
{
	[TestClass]
	public class OrchestrationLoggerTests
	{
		[DeploymentItem("LoggingBase.config")]
		[DeploymentItem("Logging.config")]
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void OrchestrationLogger_GetLogger()
		{
			// No Configuration
			OrchestrationLogger_GetLogger_Theory("LoggingBase.config", "ORCHESTRATION", "ORCHESTRATION", "ORCHESTRATION\\ORCHESTRATION.log", "2MB", 10, Level.Info);
			// Section Defaults
			OrchestrationLogger_GetLogger_Theory("Logging.config", "ORCHESTRATION", "ORCHESTRATION", "\\LogDirs\\OrchestrationDefault\\ORCHESTRATION\\ORCHESTRATION.log", "700KB", 7, Level.Debug);
			// Named Logger Overrides
			OrchestrationLogger_GetLogger_Theory("Logging.config", "OVERRIDE", "OVERRIDE", "\\LogDirs\\OrchestrationDefault\\OVERRIDE\\OVERRIDE.log", "4GB", 4, Level.Trace);
		}

		public void OrchestrationLogger_GetLogger_Theory(string config, string orchestrationName, string logName,
			string logFile, string fileSize, int sizeRoll, Level logLevel)
		{
			Console.WriteLine(JsonConvert.SerializeObject(new { TestData = new { config, hostname = orchestrationName, logName, logFile, fileSize, sizeRoll, logLevel } }));

			// Arrange
			var mockEnvironment = MockRepository.GeneratePartialMock<LoggerEnvironment>();
			mockEnvironment.Stub(x => x.GetHostName()).Return(orchestrationName);
			if (config != null)
			{
				var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config);
				Assert.IsTrue(File.Exists(configPath), string.Format("Config file not found: {0}", configPath));
				mockEnvironment.Stub(x => x.GetExeConfig()).Return(ConfigurationManager.OpenMappedExeConfiguration(
					new ExeConfigurationFileMap { ExeConfigFilename = configPath }, ConfigurationUserLevel.None));
			}
			mockEnvironment.Stub(x => x.GetRollingFileAppender(null, null, null, null, null)).IgnoreArguments()
				.Do(new Func<string, string, PatternLayout, LoggerConfigItem, Level, AppenderSkeleton>(
					(name, file, layout, settings, level) =>
					{
						Console.WriteLine(JsonConvert.SerializeObject(new
						{
							GetRollingFileAppender = new
							{
								name,
								logFile = file,
								settings.MaximumFileSize,
								settings.MaxSizeRollBackups,
								level.Name
							}
						}));
						return new ConsoleAppender { Layout = new PatternLayout(), Threshold = level };
					}));
			LoggerHelpers.Environment = mockEnvironment;

			// Act
			var logger = new OrchestrationLogger(orchestrationName);

			// Assert
			mockEnvironment.AssertWasCalled(x => x.GetRollingFileAppender(Arg.Is(logName), Arg.Is(logFile),
				Arg<PatternLayout>.Is.Anything,
				Arg<LoggerConfigItem>.Matches(c => c.MaximumFileSize == fileSize && c.MaxSizeRollBackups == sizeRoll),
				Arg.Is(logLevel)));

			Assert.IsInstanceOfType(logger.Log, typeof(Log4NetLogger));
			Assert.IsTrue(logger.Log.IsInfoEnabled);
			Assert.IsTrue(logLevel > Level.Debug ? !logger.Log.IsDebugEnabled : logger.Log.IsDebugEnabled);
			Assert.IsTrue(logLevel > Level.Trace ? !logger.Log.IsTraceEnabled : logger.Log.IsTraceEnabled);

			logger.Log.Info("Test Success\r\n");
			LoggerHelpers.logRepository.Clear();
			((Logger)log4net.LogManager.GetLogger(logName).Logger).RemoveAllAppenders();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void OrchestrationLogger_Serialization()
		{
			var logger = new OrchestrationLogger("ORCHESTRATION");
			logger.Log.Error("Test Start");

			var formatter = new SoapFormatter();
			using (var ms = new MemoryStream())
			{
				formatter.Serialize(ms, logger);
				Assert.AreEqual(@"<SOAP-ENV:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:SOAP-ENC=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:clr=""http://schemas.microsoft.com/soap/encoding/clr/1.0"" SOAP-ENV:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
<SOAP-ENV:Body>
<a1:OrchestrationLogger id=""ref-1"" xmlns:a1=""http://schemas.microsoft.com/clr/nsassem/CargoWise.eHub.Core.Logging/CargoWise.eHub.Core.Logging%2C%20Version%3D3.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3D4f570df270576350"">
<OrchestrationName id=""ref-3"">ORCHESTRATION</OrchestrationName>
</a1:OrchestrationLogger>
</SOAP-ENV:Body>
</SOAP-ENV:Envelope>
", Encoding.UTF8.GetString(ms.ToArray()));

				ms.Position = 0;
				var deserialized = (OrchestrationLogger)formatter.Deserialize(ms);
				Assert.AreEqual("ORCHESTRATION", deserialized.OrchestrationName);

				deserialized.Log.Error("Test Success");
			}

			LoggerHelpers.logRepository.Clear();
		}
	}
}
