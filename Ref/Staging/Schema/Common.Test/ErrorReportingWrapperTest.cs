using System;
using System.Reflection;
using CargoWise.RefDbRepo.Common.ErrorReporting;
using CargoWise.RefDbRepo.Staging.Common.ErrorReporting;
using Moq;
using NUnit.Framework;
using WTG.ErrorReporting;

namespace CargoWise.RefDbRepo.Staging.Common.Test
{
	[TestFixture]
	public class ErrorReportingWrapperTest
	{
		[Test]
		public void AppendBaseKey()
		{
			var mockObjects = GetMockObjects();
			mockObjects.ErrorReportingWrapper.AppendBaseKey("SourceDataAppName:AAA");
			mockObjects.ErrorReportingWrapper.AppendBaseKey("SubSource:BBB");
			mockObjects.ErrorReportingWrapper.AppendDescription("test error.");

			mockObjects.ErrorReportingWrapper.PostCrashReport();
			mockObjects.MockErrorReportBuilder.Verify(x => x.BuildErrorReport(It.Is<InvalidOperationException>(e => e.Message == "test error.\r\n"),
				"SourceDataAppName:AAA\r\nSubSource:BBB\r\n", "test error.\r\n", true));
		}

		[TestCase(1)]
		[TestCase(150)]
		[TestCase(1000)]
		public void AppendDescription(int messageCount)
		{
			var errMessage = "This is one Error Message";
			var mockObjects = GetMockObjects();
			for (int i = 0; i < messageCount; i++)
			{
				mockObjects.ErrorReportingWrapper.AppendDescription(errMessage);
			}
			mockObjects.ErrorReportingWrapper.PostCrashReport();

			mockObjects.MockErrorReportBuilder.Verify(x => x.BuildErrorReport(It.Is<InvalidOperationException>(e => GetRepeatStringCount(e.Message, errMessage) == Math.Min(messageCount, Application.ErrorReportMaxCount)), "", It.IsAny<string>(), true));
		}

		[Test]
		public void AddException()
		{
			var mockObjects = GetMockObjects();
			var exception = new ArgumentException("argument not null.");
			SetExceptionStackTrace(exception, "stackTrace for test.");
			mockObjects.ErrorReportingWrapper.AppendBaseKey("SourceDataAppName:AAA");
			mockObjects.ErrorReportingWrapper.AppendBaseKey("SubSource:BBB");
			mockObjects.ErrorReportingWrapper.AppendDescription("an error happens.");
			mockObjects.ErrorReportingWrapper.AddException(exception);

			mockObjects.ErrorReportingWrapper.PostCrashReport();
			var stack = exception.StackTrace;
			mockObjects.MockErrorReportBuilder.Verify(x => x.BuildErrorReport(It.Is<ArgumentException>(e => e.Message == exception.Message),
				"SourceDataAppName:AAA\r\nSubSource:BBB\r\n", "an error happens.\r\n", true));
		}

		[TestCase("", false)]
		[TestCase(null, false)]
		[TestCase("\r\n", false)]
		[TestCase(" \r\n", false)]
		[TestCase(" Test", true)]
		public void HasErrorToReport_CheckDescription(string message, bool hasError)
		{
			using (var wrapper = new ErrorReportingWrapper())
			{
				Assert.False(wrapper.HasErrorToReport);
				wrapper.AppendDescription(message);
				Assert.AreEqual(hasError, wrapper.HasErrorToReport);
			}
		}

		[Test]
		public void HasErrorToReport_CheckException()
		{
			using (var wrapper = new ErrorReportingWrapper())
			{
				Assert.False(wrapper.HasErrorToReport);
				wrapper.AddException(new Exception("Test"));
				Assert.True(wrapper.HasErrorToReport);
			}
		}

		[Test]
		public void PostCrashReport()
		{
			var mockObjects = GetMockObjects();

			mockObjects.ErrorReportingWrapper.AppendDescription("This is one Error Message");
			mockObjects.ErrorReportingWrapper.PostCrashReport();

			mockObjects.MockErrorReportBuilder.Verify(x => x.BuildErrorReport(It.Is<InvalidOperationException>(e => e.Message == "This is one Error Message\r\n"), "", It.IsAny<string>(), true));
		}

		[Test]
		public void PostCrashReport_MultipleDistinctExceptions()
		{
			var mockObjects = GetMockObjects();
			var ex1 = new ArgumentException("error 1.");
			SetExceptionStackTrace(ex1, "demo stackTrace 1.");
			var ex2 = new ArgumentException("the same with error 1.");
			SetExceptionStackTrace(ex2, "demo stackTrace 1.");
			var ex3 = new ArgumentException("error 3.");
			SetExceptionStackTrace(ex3, "demo stackTrace 3.");
			var ex4 = new ArgumentException("error 4.");

			mockObjects.ErrorReportingWrapper.AppendBaseKey("demo baseKey.");
			mockObjects.ErrorReportingWrapper.AppendDescription("multiple errors happen.");
			mockObjects.ErrorReportingWrapper.AddException(ex1);
			mockObjects.ErrorReportingWrapper.AddException(ex2);
			mockObjects.ErrorReportingWrapper.AddException(ex3);
			mockObjects.ErrorReportingWrapper.AddException(ex4);
			mockObjects.ErrorReportingWrapper.PostCrashReport();

			mockObjects.MockErrorReportBuilder.Verify(x => x.BuildErrorReport(It.Is<ArgumentException>(e => e.Message == ex1.Message), "demo baseKey.\r\n", "multiple errors happen.\r\n", true));
			mockObjects.MockErrorReportBuilder.Verify(x => x.BuildErrorReport(It.Is<ArgumentException>(e => e.Message == ex2.Message), "demo baseKey.\r\n", "multiple errors happen.\r\n", true), Times.Never);
			mockObjects.MockErrorReportBuilder.Verify(x => x.BuildErrorReport(It.Is<ArgumentException>(e => e.Message == ex3.Message), "demo baseKey.\r\n", "multiple errors happen.\r\n", true));
			mockObjects.MockErrorReportBuilder.Verify(x => x.BuildErrorReport(It.Is<ArgumentException>(e => e.Message == ex4.Message), "demo baseKey.\r\n", "multiple errors happen.\r\n", true));
		}

		[Test]
		public void PostErrorReportWithException()
		{
			var mockObjects = GetMockObjects();
			var exception = new ArgumentException("argument not null.");
			SetExceptionStackTrace(exception, "stackTrace for test.");

			mockObjects.ErrorReportingWrapper.PostCrashReport(exception, "key base", "error description");
			mockObjects.MockErrorReportBuilder.Verify(x => x.BuildErrorReport(It.Is<ArgumentException>(e => e.Message == exception.Message), "key base", "error description", true));
		}

		int GetRepeatStringCount(string originString, string matchString)
		{
			var count = 0;
			var index = originString.LastIndexOf(matchString);
			while (index >= 0)
			{
				count++;
				originString = originString.Substring(0, index);
				index = originString.LastIndexOf(matchString);
			}
			return count;
		}

		void SetExceptionStackTrace(Exception exception, string stackTrace)
		{
			var filedInfo = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);
			filedInfo.SetValue(exception, stackTrace);
		}

		(ErrorReportingWrapper ErrorReportingWrapper, Mock<IErrorReportBuilder> MockErrorReportBuilder) GetMockObjects()
		{
			var mockErrorReportBuilder = new Mock<IErrorReportBuilder>();
			var mockErrorReportingClientProvider = new Mock<IErrorReportingClientProvider>();
			var mockErrorReportingClient = new Mock<IErrorReportingClient>();
			mockErrorReportingClientProvider.Setup(x => x.CreateClient(It.IsAny<Uri>(), It.IsAny<TimeSpan?>())).Returns(mockErrorReportingClient.Object);

			var wrapper = new ErrorReportingWrapper(mockErrorReportBuilder.Object, mockErrorReportingClientProvider.Object);
			return (wrapper, mockErrorReportBuilder);
		}
	}
}
