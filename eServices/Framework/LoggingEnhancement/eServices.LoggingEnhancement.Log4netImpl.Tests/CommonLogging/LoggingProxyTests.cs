
using System;
using System.Collections.Generic;
using Common.Logging;
using eServices.LoggingEnhancement.Log4netImpl.CommonLogging;
using Moq;
using NUnit.Framework;

namespace eServices.LoggingEnhancement.Tests.CommonLogging
{
	public class LoggingProxyTests
	{
		private Mock<ILog> _mockLog;
		private Mock<IVariablesContext> _mockVariablesContext;
		private LoggingProxy _proxy;

		[SetUp]
		public void SetUp()
		{
			_mockVariablesContext = new Mock<IVariablesContext>();
			_mockLog = new Mock<ILog>();
			_mockLog.Setup(x => x.ThreadVariablesContext).Returns(_mockVariablesContext.Object);

			_proxy = new LoggingProxy(_mockLog.Object)
			{
				ContextData = new Dictionary<string, object> { {"activityId", "activityValue"} }
			};
		}

		[TestCase(true)]
		[TestCase(false)]
		public void TestTrace(bool isEnabled)
		{
			_mockLog.Setup(x => x.IsTraceEnabled).Returns(isEnabled);
			var exception = new Exception("test exception");

			_proxy.Trace("test");
			_proxy.Trace("test", exception);
			_proxy.TraceFormat("test");
			_proxy.TraceFormat("test", exception);

			var times = Times.Never();
			if (isEnabled)
			{
				times = Times.AtLeastOnce();
			}

			_mockLog.Verify(x => x.Trace(It.IsAny<string>()), times);
			_mockLog.Verify(x => x.Trace(It.IsAny<string>(), It.IsAny<Exception>()), times);
			_mockLog.Verify(x => x.TraceFormat(It.IsAny<string>()), times);
			_mockLog.Verify(x => x.TraceFormat(It.IsAny<string>(), It.IsAny<Exception>()), times);

			_mockVariablesContext.Verify(x => x.Set("activityId", " [activityId=activityValue]"), times);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void TestDebug(bool isEnabled)
		{
			_mockLog.Setup(x => x.IsDebugEnabled).Returns(isEnabled);
			var exception = new Exception("test exception");

			_proxy.Debug("test");
			_proxy.Debug("test", exception);
			_proxy.DebugFormat("test");
			_proxy.DebugFormat("test", exception);

			var times = Times.Never();
			if (isEnabled)
			{
				times = Times.AtLeastOnce();
			}

			_mockLog.Verify(x => x.Debug(It.IsAny<string>()), times);
			_mockLog.Verify(x => x.Debug(It.IsAny<string>(), It.IsAny<Exception>()), times);
			_mockLog.Verify(x => x.DebugFormat(It.IsAny<string>()), times);
			_mockLog.Verify(x => x.DebugFormat(It.IsAny<string>(), It.IsAny<Exception>()), times);

			_mockVariablesContext.Verify(x => x.Set("activityId", " [activityId=activityValue]"), times);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void TestInfo(bool isEnabled)
		{
			_mockLog.Setup(x => x.IsInfoEnabled).Returns(isEnabled);
			var exception = new Exception("test exception");

			_proxy.Info("test");
			_proxy.Info("test", exception);
			_proxy.InfoFormat("test");
			_proxy.InfoFormat("test", exception);

			var times = Times.Never();
			if (isEnabled)
			{
				times = Times.AtLeastOnce();
			}

			_mockLog.Verify(x => x.Info(It.IsAny<string>()), times);
			_mockLog.Verify(x => x.Info(It.IsAny<string>(), It.IsAny<Exception>()), times);
			_mockLog.Verify(x => x.InfoFormat(It.IsAny<string>()), times);
			_mockLog.Verify(x => x.InfoFormat(It.IsAny<string>(), It.IsAny<Exception>()), times);

			_mockVariablesContext.Verify(x => x.Set("activityId", " [activityId=activityValue]"), times);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void TestWarn(bool isEnabled)
		{
			_mockLog.Setup(x => x.IsWarnEnabled).Returns(isEnabled);
			var exception = new Exception("test exception");

			_proxy.Warn("test");
			_proxy.Warn("test", exception);
			_proxy.WarnFormat("test");
			_proxy.WarnFormat("test", exception);

			var times = Times.Never();
			if (isEnabled)
			{
				times = Times.AtLeastOnce();
			}

			_mockLog.Verify(x => x.Warn(It.IsAny<string>()), times);
			_mockLog.Verify(x => x.Warn(It.IsAny<string>(), It.IsAny<Exception>()), times);
			_mockLog.Verify(x => x.WarnFormat(It.IsAny<string>()), times);
			_mockLog.Verify(x => x.WarnFormat(It.IsAny<string>(), It.IsAny<Exception>()), times);

			_mockVariablesContext.Verify(x => x.Set("activityId", " [activityId=activityValue]"), times);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void TestError(bool isEnabled)
		{
			_mockLog.Setup(x => x.IsErrorEnabled).Returns(isEnabled);
			var exception = new Exception("test exception");

			_proxy.Error("test");
			_proxy.Error("test", exception);
			_proxy.ErrorFormat("test");
			_proxy.ErrorFormat("test", exception);

			var times = Times.Never();
			if (isEnabled)
			{
				times = Times.AtLeastOnce();
			}
			_mockLog.Verify(x => x.Error(It.IsAny<string>()), times);
			_mockLog.Verify(x => x.Error(It.IsAny<string>(), It.IsAny<Exception>()), times);
			_mockLog.Verify(x => x.ErrorFormat(It.IsAny<string>()), times);
			_mockLog.Verify(x => x.ErrorFormat(It.IsAny<string>(), It.IsAny<Exception>()), times);

			_mockVariablesContext.Verify(x => x.Set("activityId", " [activityId=activityValue]"), times);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void TestFatal(bool isEnabled)
		{
			_mockLog.Setup(x => x.IsFatalEnabled).Returns(isEnabled);
			var exception = new Exception("test exception");

			_proxy.Fatal("test");
			_proxy.Fatal("test", exception);
			_proxy.FatalFormat("test");
			_proxy.FatalFormat("test", exception);

			var times = Times.Never();
			if (isEnabled)
			{
				times = Times.AtLeastOnce();
			}

			_mockLog.Verify(x => x.Fatal(It.IsAny<string>()), times);
			_mockLog.Verify(x => x.Fatal(It.IsAny<string>(), It.IsAny<Exception>()), times);
			_mockLog.Verify(x => x.FatalFormat(It.IsAny<string>()), times);
			_mockLog.Verify(x => x.FatalFormat(It.IsAny<string>(), It.IsAny<Exception>()), times);

			_mockVariablesContext.Verify(x => x.Set("activityId", " [activityId=activityValue]"), times);
		}

		[Test]
		public void TestFluentCall()
		{
			_mockLog.Setup(x => x.IsFatalEnabled).Returns(true);
			var exception = new Exception("test exception");

			_proxy.WithContext("activityId", "activityValue3")
				.WithContext("messageId", "00001")
				.Fatal("test", exception);

			_mockVariablesContext.Verify(x => x.Set("activityId", " [activityId=activityValue3]"), Times.Once);
			_mockVariablesContext.Verify(x => x.Set("messageId", " [messageId=00001]"), Times.Once);
		}
	}
}
