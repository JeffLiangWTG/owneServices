using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using CargoWise.eHub.Core.Logging;
using Common.Logging.Log4Net;
using Common.Logging.Simple;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.Logging
{
	[TestClass]
	public class LoggerHelpersTests
	{
		[DeploymentItem("LoggingBase.config")]
		[DeploymentItem("Logging.config")]
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void LoggerHelpers_GetHostLogger()
		{
			// No Configuration
			LoggerHelpers_GetHostLogger_Theory("LoggingBase.config", "HOSTNAME", null, "HOSTNAME",
				"HOSTNAME\\HOSTNAME.log", "2MB", 10, Level.Info);
			// No Configuration with Component
			LoggerHelpers_GetHostLogger_Theory("LoggingBase.config", "HOSTNAME", "COMPONENT",
				"HOSTNAME#COMPONENT", "HOSTNAME\\HOSTNAME#COMPONENT.log", "2MB", 10, Level.Info);
			// Section Defaults
			LoggerHelpers_GetHostLogger_Theory("Logging.config", "HOSTNAME", null,
				"HOSTNAME", "\\LogDirs\\HostDefault\\HOSTNAME\\HOSTNAME.log", "500KB", 5, Level.Debug);
			// Named Logger Overrides
			LoggerHelpers_GetHostLogger_Theory("Logging.config", "OVERRIDE", null,
				"OVERRIDE", "\\LogDirs\\HostDefault\\OVERRIDE\\OVERRIDE.log", "2GB", 2, Level.Trace);
		}

		public void LoggerHelpers_GetHostLogger_Theory(string config, string hostname, string component, string logName,
			string logFile, string fileSize, int sizeRoll, Level logLevel)
		{
			Console.WriteLine(JsonConvert.SerializeObject(new { TestData = new { config, hostname, component, logName, logFile, fileSize, sizeRoll, logLevel } }));

			// Arrange
			var mockEnvironment = MockRepository.GeneratePartialMock<LoggerEnvironment>();
			mockEnvironment.Stub(x => x.GetHostName()).Return(hostname);
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
			var logger = component == null ? LoggerHelpers.GetHostLogger() : LoggerHelpers.GetHostLogger(component);

			// Assert
			mockEnvironment.AssertWasCalled(x => x.GetRollingFileAppender(Arg.Is(logName), Arg.Is(logFile),
				Arg<PatternLayout>.Is.Anything,
				Arg<LoggerConfigItem>.Matches(c => c.MaximumFileSize == fileSize && c.MaxSizeRollBackups == sizeRoll),
				Arg.Is(logLevel)));

			Assert.IsInstanceOfType(logger, typeof(Log4NetLogger));
			Assert.IsTrue(logger.IsInfoEnabled);
			Assert.IsTrue(logLevel > Level.Debug ? !logger.IsDebugEnabled : logger.IsDebugEnabled);
			Assert.IsTrue(logLevel > Level.Trace ? !logger.IsTraceEnabled : logger.IsTraceEnabled);

			logger.Info("Test Success\r\n");
			LoggerHelpers.logRepository.Clear();
			((Logger)log4net.LogManager.GetLogger(logName).Logger).RemoveAllAppenders();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void LoggerHelpers_GetHostLogger_NotBizTalkError()
		{
			// Arrange
			var exception = new InvalidOperationException("Operation not valid for non-BizTalk host");
			var mockEnvironment = MockRepository.GenerateMock<LoggerEnvironment>();
			mockEnvironment.Stub(x => x.GetHostName()).Throw(exception);
			LoggerHelpers.Environment = mockEnvironment;

			// Act
			var logger = LoggerHelpers.GetHostLogger();

			// Assert
			Assert.IsInstanceOfType(logger, typeof(NoOpLogger));

			LoggerHelpers.logRepository.Clear();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void LoggerHelpers_GetHostLogger_ConfigError()
		{
			// Arrange
			var exception = new NotImplementedException();
			var mockEnvironment = MockRepository.GenerateMock<LoggerEnvironment>();
			mockEnvironment.Stub(x => x.GetHostName()).Return("HOSTNAME");
			mockEnvironment.Stub(x => x.GetExeConfig()).Throw(exception);
			LoggerHelpers.Environment = mockEnvironment;

			// Act
			var logger = LoggerHelpers.GetHostLogger();

			// Assert
			Assert.IsInstanceOfType(logger, typeof(NoOpLogger));

			LoggerHelpers.logRepository.Clear();
		}

		[DeploymentItem("LoggingBase.config")]
		[DeploymentItem("Logging.config")]
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void LoggerHelpers_GetPipelineLogger()
		{
			// No Configuration
			LoggerHelpers_GetPipelineLogger_Theory("LoggingBase.config", null, "RECEIVELOCATION", null, false, false, null, "RECEIVELOCATION+Receive", "RECEIVELOCATION+Receive\\RECEIVELOCATION+Receive.log", "2MB", 10, Level.Info);
			// No Configuration with Component
			LoggerHelpers_GetPipelineLogger_Theory("LoggingBase.config", null, null, "SENDPORTNAME", false, true, "COMPONENT", "SENDPORTNAME+Send#COMPONENT", "SENDPORTNAME+Send\\SENDPORTNAME+Send#COMPONENT.log", "2MB", 10, Level.Info);
			// Section Defaults
			LoggerHelpers_GetPipelineLogger_Theory("Logging.config", "OUTLOCATION", "RECEIVELOCATION", null, true, false, null, "RECEIVELOCATION+Receive", "\\LogDirs\\PipelineDefault\\RECEIVELOCATION+Receive\\RECEIVELOCATION+Receive.log", "600KB", 6, Level.Debug);
			// Named Logger Overrides
			LoggerHelpers_GetPipelineLogger_Theory("Logging.config", "OUTLOCATION", "RECEIVELOCATION", "OVERRIDE", false, false, null, "OVERRIDE+Send", "\\LogDirs\\PipelineDefault\\OVERRIDE+Send\\OVERRIDE+Send.log", "3GB", 3, Level.Trace);
		}

		public void LoggerHelpers_GetPipelineLogger_Theory(string config, string outLocation, string receiveLocation,
			string sendPortName, bool isReqResp, bool wasSolResp, string component, string logName,
			string logFile, string fileSize, int sizeRoll, Level logLevel)
		{
			Console.WriteLine(JsonConvert.SerializeObject(new { TestData = new { config, outLocation, receiveLocation,
				sendPortName, isReqResp, wasSolResp, component, logName, logFile, fileSize, sizeRoll, logLevel.Name } }));

			// Arrange
			var message = new MessageFactory().CreateMessage();
			message.Context.Write("OutboundTransportLocation", "http://schemas.microsoft.com/BizTalk/2003/system-properties", outLocation);
			message.Context.Write("IsRequestResponse", "http://schemas.microsoft.com/BizTalk/2003/system-properties", isReqResp);
			message.Context.Write("WasSolicitResponse", "http://schemas.microsoft.com/BizTalk/2003/system-properties", wasSolResp);
			message.Context.Write("ReceiveLocationName", "http://schemas.microsoft.com/BizTalk/2003/system-properties", receiveLocation);
			message.Context.Write("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties", sendPortName);
			
			var mockEnvironment = MockRepository.GeneratePartialMock<LoggerEnvironment>();
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
			var logger = component == null ? LoggerHelpers.GetPipelineLogger(message) : LoggerHelpers.GetPipelineLogger(message, component);

			// Assert
			mockEnvironment.AssertWasCalled(x => x.GetRollingFileAppender(Arg.Is(logName), Arg.Is(logFile),
				Arg<PatternLayout>.Is.Anything,
				Arg<LoggerConfigItem>.Matches(c => c.MaximumFileSize == fileSize && c.MaxSizeRollBackups == sizeRoll),
				Arg.Is(logLevel)));

			Assert.IsInstanceOfType(logger, typeof(Log4NetLogger));
			Assert.IsTrue(logger.IsInfoEnabled);
			Assert.IsTrue(logLevel > Level.Debug ? !logger.IsDebugEnabled : logger.IsDebugEnabled);
			Assert.IsTrue(logLevel > Level.Trace ? !logger.IsTraceEnabled : logger.IsTraceEnabled);

			logger.Info("Test Success\r\n");
			LoggerHelpers.logRepository.Clear();
			((Logger)log4net.LogManager.GetLogger(logName).Logger).RemoveAllAppenders();
		}
	}
}