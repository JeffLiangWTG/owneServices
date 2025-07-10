using System;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Common.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	[TestFixture]
	class AuthenticationLoggerFixture
	{
		[Test]
		public void LogInfo()
		{
			Exception exception = null;
			Func<string, Exception, string> formatter = (s, e) => s + e?.Message;
			authenticationLogger.Log(LogLevel.Information, new EventId(), "Info: ", exception, formatter);
			log.Verify(x => x.Info("Info: "));

			exception = new Exception("Unexpected exception");
			authenticationLogger.Log(LogLevel.Information, new EventId(), "Info: ", exception, formatter);
			log.Verify(x => x.Info("Info: Unexpected exception"));
		}

		[Test]
		public void LogWarning()
		{
			Exception exception = null;
			Func<string, Exception, string> formatter = (s, e) => s + e?.Message;
			authenticationLogger.Log(LogLevel.Warning, new EventId(), "Warning: ", exception, formatter);
			log.Verify(x => x.Warn("Warning: "));

			exception = new Exception("Unexpected exception");
			authenticationLogger.Log(LogLevel.Warning, new EventId(), "Warning: ", exception, formatter);
			log.Verify(x => x.Warn("Warning: Unexpected exception", exception));
		}

		[Test]
		public void LogError()
		{
			Exception exception = null;
			Func<string, Exception, string> formatter = (s, e) => s + e?.Message;
			authenticationLogger.Log(LogLevel.Error, new EventId(), "Error: ", exception, formatter);
			log.Verify(x => x.Error("Error: "));

			exception = new Exception("Unexpected exception");
			authenticationLogger.Log(LogLevel.Error, new EventId(), "Error: ", exception, formatter);
			log.Verify(x => x.Error("Error: Unexpected exception", exception));
		}

		[Test]
		public void IsEnabled()
		{
			var logLevels = Enum.GetValues(typeof(LogLevel)) as LogLevel[];
			foreach (var logLevel in logLevels)
			{
				var isEnabled = logLevel >= LogLevel.Information;
				Assert.AreEqual(isEnabled, authenticationLogger.IsEnabled(logLevel));
			}
		}

		[SetUp]
		public void Setup()
		{
			log = new Mock<ILog>();
			logWrapper = new Mock<ILogWrapper>();
			logWrapper.Setup(x => x.GetLog(nameof(AuthenticationLogger))).Returns(log.Object);
			authenticationLogger = new AuthenticationLogger(logWrapper.Object);
		}

		Mock<ILog> log;
		Mock<ILogWrapper> logWrapper;
		AuthenticationLogger authenticationLogger;
	}
}
