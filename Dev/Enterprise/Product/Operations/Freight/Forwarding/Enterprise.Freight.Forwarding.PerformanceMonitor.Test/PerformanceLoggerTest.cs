using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Freight.Forwarding.Logging;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using NLog;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.Freight.Forwarding.PerformanceMonitor.Test
{
	class PerformanceLoggerTest : TestCase
	{
		public void TestGetLogger()
		{
			var logger = PerformanceLogger.GetLogger();

			AssertNotNull(logger);
			AssertEquals(PerformanceLogger.PerformanceLogTargetName, logger.Name);
			AssertEquals(true, logger.IsInfoEnabled);
			AssertEquals(true, logger.IsWarnEnabled);
			AssertEquals(true, logger.IsFatalEnabled);
		}

		public void TestLogFilePath()
		{
			var logger = PerformanceLogger.GetLogger();
			AssertNotNull(logger);
			AssertEquals(false, Directory.Exists(expectedLogFileDir));

			// Act
			logger.Info(nameof(TestLogFilePath));

			PerformanceLogger.Flush();
			PerformanceLogger.Shutdown();

			// Assert
			AssertEquals(true, Directory.Exists(expectedLogFileDir));
			var logFiles = Directory.GetFiles(expectedLogFileDir, $"{expectedLogFilePrefix}*");
			AssertNotNull(logFiles);
			AssertEquals(1, logFiles.Length);
		}

		public void TestLogEventTracker()
		{
			var message = "message1";
			var operation = "operation1";
			var objectType = "objectType1";
			var pk = "pk1";
			var numberOfRecords = 1;
			var duration = TimeSpan.FromSeconds(10);
			var expectedHostName = System.Environment.MachineName;
			var expectedDomainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
			var expectedProcessId = Process.GetCurrentProcess().Id;

			// Act
			PerformanceLogger.Info(message, operation, objectType, pk, numberOfRecords, duration);

			LogManager.Shutdown();

			// Assert
			AssertEquals(true, Directory.Exists(expectedLogFileDir));
			var logFiles = Directory.GetFiles(expectedLogFileDir, $"{expectedLogFilePrefix}*");
			AssertNotNull(logFiles);
			AssertEquals(1, logFiles.Length);
			var content = File.ReadAllText(logFiles[0]);
			var performanceInfo = JsonSerializer.Deserialize<PerformanceInfo>(content);
			CombineAssertions(() =>
			{
				AssertNotNull(performanceInfo);
				AssertNotEquals(DateTime.MinValue, performanceInfo.Timestamp);
				AssertEquals(expectedDomainName, performanceInfo.Domain);
				AssertEquals(expectedHostName, performanceInfo.Host);
				AssertEquals(expectedProcessId, performanceInfo.Pid);
				AssertEquals(Db.ServerName, performanceInfo.Server);
				AssertEquals(Db.DatabaseName, performanceInfo.Database);
				AssertEquals(expectedEnterpriseCode, performanceInfo.Code);
				AssertEquals(objectType, performanceInfo.Object);
				AssertEquals(pk, performanceInfo.Pk);
				AssertEquals(numberOfRecords, performanceInfo.Records);
				AssertEquals(Convert.ToInt32(duration.TotalMilliseconds), performanceInfo.Duration);
				AssertEquals(operation, performanceInfo.Op);
				AssertEquals(ReleaseInfo.Instance.VersionNumber.ToVersion(), performanceInfo.Version);
				AssertEquals(message, performanceInfo.Message);

				var jsonFields = typeof(PerformanceInfo)
					.GetProperties()
					.Select(property => property.GetCustomAttribute<JsonPropertyNameAttribute>())
					.Where(jsonProperty => jsonProperty is not null);
				jsonFields.ForEach(jsonfield => AssertContains(jsonfield.Name, content));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var productRegistrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			productRegistrationKey.EnterpriseCodeForTest = "ENT";
			productRegistrationKey.ServerCodeForTest = "TST";
			expectedLogFileDir = CommonProgramData.GetCargoWiseDirectory("PerformanceTracker", Db.ServerName, Db.DatabaseName);
			expectedLogFilePrefix = Invariant($"{productRegistrationKey.EnterpriseCode}{productRegistrationKey.ServerCode}_{Process.GetCurrentProcess().Id}_");
			expectedEnterpriseCode = $"{productRegistrationKey.EnterpriseCode}{productRegistrationKey.ServerCode}";

			if (Directory.Exists(expectedLogFileDir))
			{
				LogManager.Shutdown();
				Directory.Delete(expectedLogFileDir, recursive: true);
			}

			typeof(PerformanceLogger).TypeInitializer.Invoke(null, null);
		}

		protected override void TearDown()
		{
			LogManager.Shutdown();

			base.TearDown();
		}

		string expectedLogFileDir;
		string expectedLogFilePrefix;
		string expectedEnterpriseCode;
	}
}
