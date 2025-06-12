using CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract;
using Common.Logging;
using Common.Logging.Configuration;
using Common.Logging.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using System.IO;
using System;
using System.Text;

namespace CargoWise.eHub.Core.Tests.HttpRetry
{
	[TestClass]
	public class LoggerExtensionTests
	{
		[TestMethod]
		public void TestDebugLogging()
		{
			NameValueCollection properties = new NameValueCollection();
			properties["level"] = "Trace";
			properties["showLogName"] = "true";
			var loggerAdapter = new ConsoleOutLoggerFactoryAdapter(properties);

			var builder = new StringBuilder();
			using (var stringWriter = new StringWriter(builder))
			{
				Console.SetOut(stringWriter);
				var log = loggerAdapter.GetLogger(this.GetType().Name);
				log.Debug(() => "Debug Logging");
			}

			Assert.IsTrue(builder.ToString().Contains("[DEBUG] LoggerExtensionTests - Debug Logging"));
		}

		[TestMethod]
		public void TestWarnLogging()
		{
			NameValueCollection properties = new NameValueCollection();
			properties["level"] = "ALL";
			properties["showLogName"] = "true";
			var loggerAdapter = new ConsoleOutLoggerFactoryAdapter(properties);

			var builder = new StringBuilder();
			using (var stringWriter = new StringWriter(builder))
			{
				Console.SetOut(stringWriter);
				var log = loggerAdapter.GetLogger(this.GetType().Name);
				log.Warn(() => "Warn Logging");
			}

			Assert.IsTrue(builder.ToString().Contains("[WARN]  LoggerExtensionTests - Warn Logging"));
		}

		[TestMethod]
		public void TestErrorLogging()
		{
			NameValueCollection properties = new NameValueCollection();
			properties["level"] = "Trace";
			properties["showLogName"] = "true";
			var loggerAdapter = new ConsoleOutLoggerFactoryAdapter(properties);

			var builder = new StringBuilder();
			using (var stringWriter = new StringWriter(builder))
			{
				Console.SetOut(stringWriter);
				var log = loggerAdapter.GetLogger(this.GetType().Name);
				log.Error(() => "Error Logging");
			}

			Assert.IsTrue(builder.ToString().Contains("[ERROR] LoggerExtensionTests - Error Logging"));
		}

		[TestMethod]
		public void TestTraceLogging()
		{
			NameValueCollection properties = new NameValueCollection();
			properties["level"] = "Trace";
			properties["showLogName"] = "true";
			var loggerAdapter = new ConsoleOutLoggerFactoryAdapter(properties);

			var builder = new StringBuilder();
			using (var stringWriter = new StringWriter(builder))
			{
				Console.SetOut(stringWriter);
				var log = loggerAdapter.GetLogger(this.GetType().Name);
				log.Trace(() => "Trace Logging");
			}

			Assert.IsTrue(builder.ToString().Contains("[TRACE] LoggerExtensionTests - Trace Logging"));
		}

		[TestMethod]
		public void TestInfoLogging()
		{
			NameValueCollection properties = new NameValueCollection();
			properties["level"] = "Trace";
			properties["showLogName"] = "true";
			var loggerAdapter = new ConsoleOutLoggerFactoryAdapter(properties);

			var builder = new StringBuilder();
			using (var stringWriter = new StringWriter(builder))
			{
				Console.SetOut(stringWriter);
				var log = loggerAdapter.GetLogger(this.GetType().Name);
				log.Info(() => "Info Logging");
			}

			Assert.IsTrue(builder.ToString().Contains("[INFO]  LoggerExtensionTests - Info Logging"));
		}
	}
}
