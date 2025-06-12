using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using CargoWise.eHub.DataModel.eHubTransactions;
using Common.Logging;
using log4net;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.Shared.IssueManager.Tests
{
	public class IssueManagerTests
	{
		private IssueManager _issueManager;
		private NameValueCollection _appSettings;
		private Func<eHubTransactionsContext> _getContextFunc;
		private Mock<eHubTransactionsContext> _eHubContextMock;

		[SetUp]
		public void Setup()
		{
			_issueManager = new IssueManager();
			_appSettings = new NameValueCollection
			{
				{ "IssueManagerKey", "TestKey" },
				{ "IssueManagerUri", "TestUri" },
				{ "IssueManagerCacheExpiration", "5"}
			};

			_getContextFunc = IssueManager.GetContext;
			_eHubContextMock = new Mock<eHubTransactionsContext>();
			IssueManager.GetContext = () => _eHubContextMock.Object;
		}

		[TearDown]
		public void TearDown()
		{
			IssueManager.GetContext = _getContextFunc;
			IssueManager.Cache.Dispose();
			IssueManager.Cache = new MemoryCache("IssueManagerCache");
		}

		[Test]
		public void ReportToIssueManager_ShouldBeReported_WhenIssueIsNotRecent()
		{
			var loggerA = new Mock<Common.Logging.ILog>();
			var loggerB = new Mock<log4net.ILog>();
			var exception = new Exception("Exception!");

			int waitTime = int.Parse(_appSettings["IssueManagerCacheExpiration"]);

			_issueManager.ReportToIssueManager("Successful Report", exception, loggerA.Object, _appSettings);

			System.Threading.Thread.Sleep(1000 * waitTime + 50);
			_issueManager.ReportToIssueManager("Successful Report", exception, loggerA.Object, _appSettings);
			System.Threading.Thread.Sleep(1000 * waitTime + 50);
			_issueManager.ReportToIssueManager("Successful Report", exception, loggerB.Object, _appSettings);

			loggerA.Verify(x => x.Debug("Rate limit exceeded, skipping."), Times.Never);
			loggerB.Verify(x => x.Debug("Rate limit exceeded, skipping."), Times.Never);
		}

		[Test]
		public void ReportToIssueManager_ShouldNotBeReported_WhenIssueIsRecent()
		{
			var loggerA = new Mock<Common.Logging.ILog>();
			var loggerB = new Mock<log4net.ILog>();
			var exception = new Exception("Exception!");

			_issueManager.ReportToIssueManager("Report A", exception, loggerA.Object, _appSettings); // Success
			_issueManager.ReportToIssueManager("Report B", exception, loggerA.Object, _appSettings); // Success

			_issueManager.ReportToIssueManager("Report B", exception, loggerA.Object, _appSettings); // Should Fail
			_issueManager.ReportToIssueManager("Report B", exception, loggerB.Object, _appSettings); // Should Fail

			loggerA.Verify(x => x.Debug("Rate limit exceeded, skipping."), Times.Once);
			loggerB.Verify(x => x.Debug("Rate limit exceeded, skipping."), Times.Once);
		}

		[Test]
		public void ReportToIssueManager_WarnOfDefaultCacheExpiration()
		{
			var loggerA = new Mock<Common.Logging.ILog>();
			var loggerB = new Mock<log4net.ILog>();
			var exception = new Exception("Exception!");

			var _newAppSettings = new NameValueCollection
			{
				{ "IssueManagerKey", "TestKey" },
				{ "IssueManagerUri", "TestUri" },
			};

			_issueManager.ReportToIssueManager("Successful Report", exception, loggerA.Object, _newAppSettings);
			_issueManager.ReportToIssueManager("Successful Report", exception, loggerB.Object, _newAppSettings);

			loggerA.Verify(x => x.Warn("Missing app setting: 'IssueManagerCacheExpiration', using default value of 300 seconds"), Times.Once);
			loggerB.Verify(x => x.Warn("Missing app setting: 'IssueManagerCacheExpiration', using default value of 300 seconds"), Times.Once);
		}

		#region Common.Logging

		[Test]
		public void ReportToIssueManager_CommonLogging_LogExceptionFromAlertExcluded()
		{
			var logger = new Mock<Common.Logging.ILog>();
			var exception = new Exception("Test exception");
			var innerException = new Exception("Connection timeout");
			_eHubContextMock.SetupGet(x => x.eHubCodeMapValues).Throws(innerException);

			_issueManager.ReportToIssueManager("Test subject", exception, logger.Object, _appSettings);

			logger.Verify(x => x.Error("Caught exception when checking AlertExcluded", innerException), Times.Once);
		}

		#endregion

		#region log4net

		[Test]
		public void ReportToIssueManager_Log4Net_LogExceptionFromAlertExcluded()
		{
			var logger = new Mock<log4net.ILog>();
			var exception = new Exception("Test exception");
			var innerException = new Exception("Connection timeout");
			_eHubContextMock.SetupGet(x => x.eHubCodeMapValues).Throws(innerException);

			_issueManager.ReportToIssueManager("Test subject", exception, logger.Object, _appSettings);

			logger.Verify(x => x.Error("Caught exception when checking AlertExcluded", innerException), Times.Once);
		}

		#endregion
	}
}
