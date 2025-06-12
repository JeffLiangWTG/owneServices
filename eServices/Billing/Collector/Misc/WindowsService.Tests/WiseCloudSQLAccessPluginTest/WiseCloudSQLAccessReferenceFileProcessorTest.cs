using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.WiseCloudSQLAccess;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using CargoWise.eServices.Billing.Tests.Common;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WTG.ErrorReporting;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Tests.WiseCloudSQLAccessPluginTest
{
	[TestFixture]
	public class WiseCloudSQLAccessReferenceFileProcessorTest
	{
		#region ProcessReferenceFile

		[Test]
		public void TestReferenceFileParsing_Success()
		{
			var mockLog = new Mock<ILogger>();
			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, "WiseCloudSQLAccessReferenceFile.csv");
			try
			{
				var content = WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(path, errorReportingClientMock.Object, mockLog.Object);
				var success = content != null && content.Count > 0;
				Assert.That(success, Is.True);
			}
			finally
			{
				File.Delete(path);
			}
		}

		[Test]
		public void TestReferenceFileWithIncorrectExtension()
		{
			var mockLog = new Mock<ILogger>();
			var content = new Dictionary<string, List<string>>();

			Assert.That(() =>
			{
				content = WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile("WiseCloudSQLAccessReferenceFile.xlsx", errorReportingClientMock.Object, mockLog.Object);
			}, Throws.TypeOf<NotSupportedException>().With.Message.EqualTo("The extension of the reference file is incorrect. Please provide a valid .csv file."));

			Assert.That(content, Is.Empty);
		}

		[Test]
		public void TestReferenceFileIsInvalid_FileDoesNotExist()
		{
			var path = Path.Combine(Path.GetTempPath(), "NonExistentFile.csv");
			TestReferenceFileIsInvalidCore(path, "Reference file path does not exist.");
		}

		[Test]
		public void TestReferenceFileIsInvalid_HeaderCountIsWrong()
		{
			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, "SampleMapping.csv", "Customer IP Address,Firewall Object Name", "Customer IP Address,Firewall Object Name,Another Random Header");
			TestReferenceFileIsInvalidCore(path, "Reference file has incorrect column count");
		}

		[Test]
		public void TestReferenceFileIsInvalid_HeaderNameIsWrong_CustomerIPAddress()
		{
			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, "SampleMapping.csv", "Customer IP Address", "Some Random Header");
			TestReferenceFileIsInvalidCore(path, "Reference file has incorrect column names");
		}

		[Test]
		public void TestReferenceFileIsInvalid_HeaderNameIsWrong_FirewallObjectName()
		{
			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, "SampleMapping.csv", "Firewall Object Name", "Some Random Header");
			TestReferenceFileIsInvalidCore(path, "Reference file has incorrect column names");
		}

		static void TestReferenceFileIsInvalidCore(string path, string expectedMessage)
		{
			try
			{
				WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(path, new Mock<IErrorReportingClient>().Object, new Mock<ILogger>().Object);
				Assert.Fail("Should throw");
			}
			catch (Exception e) when (e is FileNotFoundException || e is InvalidDataException)
			{
				Assert.That(e.Message, Is.EqualTo(expectedMessage));
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void TestProcessReferenceFile_SampleMappingCsv_ShouldReportToIssueManager(bool fileIsReadOnly)
		{
			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, "SampleMapping.csv");
			try
			{
				var mockLog = new Mock<ILogger>(MockBehavior.Strict);
				mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Information, "Ambiguous records found: Customers bbbBBB, bbbBBB, bbbBB1, dddDDD share same IP 2.2.2.2"));
				mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Error, "Reference file has incorrect customer code or IP format: 1111.1.1.1,Customer-aaaAAA-02"));
				mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Error, "Reference file has incorrect customer code or IP format: 1.111.1.1111,Customer-aaaAAA-03"));
				mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Error, "Reference file has incorrect customer code or IP format: 1.111.1.11.11,Customer-aaaAAA-04"));
				mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Error, "Reference file has incorrect customer code or IP format: 1.1.a.1,Customer-aaaAAA-05"));
				mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Error, "Reference file has incorrect customer code or IP format: 1.1.1.1 -1.1.1.2-1.1.1.5,Customer-aaaAAA-06"));
				mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Error, "Reference file has incorrect customer code or IP format: 1.1.1.1/123,Customer-aaaAAA-07"));
				mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Error, "Reference file has incorrect customer code or IP format: 2.2.2.2,Customer-bbbBB-02"));
				mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Error, "Reference file has incorrect customer code or IP format: 2.2.2.2,Customer-bbbBBBB-03"));
				mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Error, "Reference file has incorrect customer code or IP format: 2.2.2.2,Customer-bbbBBB-0Y"));
				mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Error, "Reference file has incorrect customer code or IP format: 2.2.2.2,CustomeR-bbbBBB-04"));
				mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Error, "Please replace subnet in 4.4.4.4/20 with IP range. Customer code: Customer-bbbBBB-02"));

				mockLog.Setup(LoggerTestHelper.GetLogSetupExpression<FormatException>(LogLevel.Error, "Reference file has invalid lines. Check log for which lines are invalid."));
				mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Warning, "Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'"));

				var content = new Dictionary<string, List<string>>();

				if (fileIsReadOnly)
				{
					using (File.Open(path, FileMode.Open, FileAccess.Write, FileShare.Read))
					{
						content = WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(path, errorReportingClientMock.Object, mockLog.Object);
					}
				}
				else
				{
					content = WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(path, errorReportingClientMock.Object, mockLog.Object);
				}

				Assert.That(content.Count, Is.EqualTo(6));
				Assert.That(content.TryGetValue("209.54.12.208", out var value) && value.First() == "aaaAAA");
				Assert.That(content.TryGetValue("209.54.12.209", out  value) && value.First() == "aaaAAA");
				Assert.That(content.TryGetValue("209.54.12.210", out  value) && value.First() == "aaaAAA");
				Assert.That(content.TryGetValue("3.3.3.3", out  value) && value.First() == "cccCCC");
				Assert.That(content.TryGetValue("3.13.13.13", out  value) && value.First() == "cccCCC");
				Assert.That(content.TryGetValue("4.4.4.4/20", out  value) && value.First() == "XXXXXX");

				mockLog.Verify(m => m.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Exactly(11));
				mockLog.Verify(LoggerTestHelper.GetLogSetupExpression<FormatException>(LogLevel.Error, "Reference file has invalid lines. Check log for which lines are invalid."), Times.Once());
				mockLog.Verify(LoggerTestHelper.GetLogSetupExpression(LogLevel.Warning, "Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'"), Times.Once());
			}
			finally
			{
				File.Delete(path);
			}
		}

		#endregion

		#region TestIP

		[Test]
		public void TestLongToIP()
		{
			var longIp = 2098072580;
			Assert.That(WiseCloudSQLAccessReferenceFileProcessor.LongToIP4(longIp), Is.EqualTo("125.14.12.4"));
		}

		[Test]
		public void TestIp4ToLong()
		{
			var stringIp = "125.14.12.4";
			Assert.That(WiseCloudSQLAccessReferenceFileProcessor.Ip4ToLong(stringIp), Is.EqualTo(2098072580));
		}

		[Test]
		public void TestInvalidIp4ToLongShouldThrowException()
		{
			var stringIp = "1.1.1";
			Assert.That(() =>
			{
				WiseCloudSQLAccessReferenceFileProcessor.Ip4ToLong(stringIp);
			}, Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Invalid IP : 1.1.1"));
		}

		#endregion

		#region Setup
		[SetUp]
		public void SetUp()
		{
			errorReportingClientMock = new Mock<IErrorReportingClient>();
			errorReportingClientMock.Setup(x => x.ServiceUri).Returns(new Uri("", UriKind.Relative));
		}
		#endregion

		Mock<IErrorReportingClient> errorReportingClientMock;
	}
}
