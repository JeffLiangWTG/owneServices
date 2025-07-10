using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using FluentAssertions;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ProfitShareRedistributionLoggerTest : TestCase
	{
		public void TestLog()
		{
			var logger = new ProfitShareRedistributionLoggerForTest();
			Log(LogType.Debug);
			Log(LogType.Information);
			Log(LogType.Warning);
			Log(LogType.Error);

			void Log(LogType logType)
			{
				var logTypeString = logType.ToString();
				logger.Log(logType, $"{logTypeString} message 1");
				logger.Log(logType, $"{logTypeString} message 2");
			}

			logger.GetLogs(LogType.Debug).Should().BeEquivalentTo("Debug message 1", "Debug message 2");
			logger.GetLogs(LogType.Information).Should().BeEquivalentTo("Information message 1", "Information message 2");
			logger.GetLogs(LogType.Warning).Should().BeEquivalentTo("Warning message 1", "Warning message 2");
			logger.GetLogs(LogType.Error).Should().BeEquivalentTo("Error message 1", "Error message 2");

			Assert("This test uses FluentAssertions", true);
		}

		public void TestOnLoggingEvent()
		{
			var notifiedMessages = new List<string>();

			void OnLoggingHandler(object sender, ProfitShareRedistributeLoggingEventArgs eventArgs)
			{
				notifiedMessages.Add($"{eventArgs.Type.ToString()}: {eventArgs.Message}");
			}

			var logger = new ProfitShareRedistributionLoggerForTest();
			logger.OnLogging += OnLoggingHandler;

			logger.Log(LogType.Debug, "Message 1");
			logger.Log(LogType.Information, "Message 2");
			logger.Log(LogType.Warning, "Message 3");
			logger.Log(LogType.Error, "Message 4");

			notifiedMessages.Should().BeEquivalentTo("Debug: Message 1", "Information: Message 2", "Warning: Message 3", "Error: Message 4");

			Assert("This test uses FluentAssertions", true);
		}

		public void TestOnCompletedEvents()
		{
			var notifiedMessages = new List<string>();

			void OnIndividualCompleted(object sender, ProfitShareRedistributedEventArgs eventArgs)
			{
				notifiedMessages.Add($"IndividualCompleted: {eventArgs.ShipmentPK}");
			}

			void OnAllCompleted(object sender, EventArgs eventArgs)
			{
				notifiedMessages.Add("All Completed");
			}

			var logger = new ProfitShareRedistributionLoggerForTest();
			logger.IndividualCompleted += OnIndividualCompleted;
			logger.AllCompleted += OnAllCompleted;

			var guid1 = ZGuid.NewZGuid();
			logger.OnIndividualCompleted(new ProfitShareRedistributedEventArgs(guid1));
			var guid2 = ZGuid.NewZGuid();
			logger.OnIndividualCompleted(new ProfitShareRedistributedEventArgs(guid2));
			logger.OnAllCompleted(EventArgs.Empty);

			notifiedMessages.Should().BeEquivalentTo($"IndividualCompleted: {guid1}", $"IndividualCompleted: {guid2}", "All Completed");

			Assert("This test uses FluentAssertions", true);
		}

		public void TestDumpLogs()
		{
			var logger = new ProfitShareRedistributionLoggerForTest();
			logger.Log(LogType.Information, "Starting Logging - Information");
			logger.Log(LogType.Error, "Starting Logging - Error");
			logger.Log(LogType.Information, "Ending Logging - Information");
			logger.Log(LogType.Error, "Ending Logging - Error");

			var expectedLogSequence = @"Starting Logging - Information
Starting Logging - Error
Ending Logging - Information
Ending Logging - Error";

			AssertEquals(expectedLogSequence, logger.DumpLogs());
		}

		class ProfitShareRedistributionLoggerForTest : ProfitShareRedistributionLogger
		{
			public List<string> GetLogs(LogType logType)
				=> logs.Where(x => x.Type == logType).Select(x => x.Message).ToList();
		}
	}
}
