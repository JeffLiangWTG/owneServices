using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using Common.Logging;
using Moq;
using NUnit.Framework;
using WTG.ErrorReporting;

namespace CargoWise.eHub.BizTalkAdapters.Tests.Transferrer
{
	public class IssueManagerTests
	{
		private Mock<IssueManager> target;
		private Mock<IErrorReportingClient> reportClientMock;
		private Mock<ILog> loggerMock;
		private Mock<IOpaqueErrorReport> errorReportMock;
		private readonly Exception exception = new Exception("Test exception");
		private CancellationTokenSource tokenSource;

		[SetUp]
		public void SetUp()
		{
			reportClientMock = new Mock<IErrorReportingClient>();
			loggerMock = new Mock<ILog>();
			loggerMock.Setup(x => x.IsTraceEnabled).Returns(true);
			loggerMock.Setup(x => x.IsDebugEnabled).Returns(true);
			loggerMock.Setup(x => x.IsWarnEnabled).Returns(true);
			loggerMock.Setup(x => x.IsInfoEnabled).Returns(true);
			loggerMock.Setup(x => x.IsErrorEnabled).Returns(true);
			errorReportMock = new Mock<IOpaqueErrorReport>();
			tokenSource = new CancellationTokenSource();

			target = new Mock<IssueManager> { CallBase = true };
			target.Setup(x => x.CreateReportClient(It.IsAny<string>())).Returns(reportClientMock.Object);
			target.Setup(x => x.GetReportBuilder(It.IsAny<string>(), It.IsAny<Exception>())).Returns(errorReportMock.Object);
			target.Setup(x => x.GetConfig(It.IsAny<string>())).Returns<string>(s => s);
		}

		[Test]
		public async Task TestReportToIssueManagerAsync()
		{
			await target.Object.ReportToIssueManagerAsync("activity", "key", exception, loggerMock.Object, tokenSource.Token);

			target.Verify(x => x.GetReportBuilder("IssueManagerKey key", exception), Times.Once);
			target.Verify(x => x.CreateReportClient("IssueManagerUri"), Times.Once);
			reportClientMock.Verify(x => x.PostCrashReportAsync(errorReportMock.Object, tokenSource.Token), Times.Once);
			loggerMock.Verify(x => x.Trace("(activity) Start reporting to IssueManager", null));
			loggerMock.Verify(x => x.Trace("(activity) Finished reporting to IssueManager", null));
		}

		[Test]
		public async Task TestReportToIssueManagerAsync_NoIssueManagerUri()
		{
			target.Setup(x => x.GetConfig(It.IsAny<string>()))
				.Returns<string>(s => s.Equals("IssueManagerUri") ? string.Empty : s);
			await target.Object.ReportToIssueManagerAsync("activity", "key", exception, loggerMock.Object, tokenSource.Token);

			reportClientMock.Verify(x => x.PostCrashReportAsync(errorReportMock.Object, tokenSource.Token), Times.Never);
			loggerMock.Verify(x => x.Error("(activity) Missing app setting: 'IssueManagerUri', exception won't be reported", exception), Times.Once);
		}

		[Test]
		public async Task TestReportToIssueManagerAsync_NoIssueManagerKey()
		{
			target.Setup(x => x.GetConfig(It.IsAny<string>()))
				.Returns<string>(s => s.Equals("IssueManagerKey") ? string.Empty : s);
			await target.Object.ReportToIssueManagerAsync("activity", "key", exception, loggerMock.Object, tokenSource.Token);

			loggerMock.Verify(x => x.Warn("(activity) Missing app setting: 'IssueManagerKey'", null), Times.Once);
			target.Verify(x => x.GetReportBuilder("CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter key", exception), Times.Once);
			reportClientMock.Verify(x => x.PostCrashReportAsync(errorReportMock.Object, tokenSource.Token), Times.Once);
			loggerMock.Verify(x => x.Trace("(activity) Start reporting to IssueManager", null));
			loggerMock.Verify(x => x.Trace("(activity) Finished reporting to IssueManager", null));
		}

		[Test]
		public async Task TestReportToIssueManagerAsync_InnerException()
		{
			var innerException = new Exception("Network error");
			reportClientMock.Setup(x => x.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>())).Throws(innerException);

			await target.Object.ReportToIssueManagerAsync("activity", "key", exception, loggerMock.Object, tokenSource.Token);

			loggerMock.Verify(x => x.Error(It.Is<string>(y => y.StartsWith("(activity) Failed to post to IssueManager, innerException: System.Exception: Network error")), exception));
		}
	}
}
