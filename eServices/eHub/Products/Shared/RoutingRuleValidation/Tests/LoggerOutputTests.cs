using log4net.Appender;
using log4net.Layout;
using log4net.Core;
using log4net.Config;
using log4net;
using NUnit.Framework;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Channels;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.LogTests
{

	public class TestAppender : AppenderSkeleton
	{
		public string LastMessage { get; private set; }

		protected override void Append(LoggingEvent loggingEvent)
		{
			LastMessage = RenderLoggingEvent(loggingEvent);
		}

		public void Clear()
		{
			LastMessage = null;
		}
	}

	public class LoggerTests
	{
		private TestAppender _testAppender;
		private log4net.ILog _log;

		public LoggerTests()
		{
			_testAppender = new TestAppender();
			var layout = new PatternLayout
			{
				//Same pattern has been applied in RRW web config
				ConversionPattern = "%date{yyyy-MM-dd'T'HH:mm:ss.fffzzz} %level Source=%C%n   %message%n   %exception%n"
			};

			layout.AddConverter("level", typeof(LevelPatternConverter));

			layout.ActivateOptions();
			_testAppender.Layout = layout;
			_testAppender.ActivateOptions();

			BasicConfigurator.Configure(_testAppender);
			_log = LogManager.GetLogger(typeof(LoggerTests));
		}

		[Test]
		public void TestLoggingInfo()
		{

			_log.Info("Information has been logged");

			string message = @"INF Source=CargoWise.eHub.Products.Shared.RoutingRuleValidation.LogTests.LoggerTests
   Information has been logged";

			Assert.IsTrue(_testAppender.LastMessage.Contains(message));
		}

		[Test]
		public void TestLoggingException()
		{
			try
			{
				throw new NotImplementedException();
			}
			catch (Exception ex)
			{
				_log.Info("Information has been logged", ex);
			}


			string message = @"INF Source=CargoWise.eHub.Products.Shared.RoutingRuleValidation.LogTests.LoggerTests
   Information has been logged
   System.NotImplementedException: The method or operation is not implemented.
   at CargoWise.eHub.Products.Shared.RoutingRuleValidation.LogTests.LoggerTests.TestLoggingException()";

			Console.WriteLine(message);
			Console.Write(_testAppender.LastMessage);

			Assert.IsTrue(_testAppender.LastMessage.Contains(message));
		}

		[Test]
		public void TestLoggingDebug()
		{

			_log.Debug("Information has been logged");

			string message = @"DBG Source=CargoWise.eHub.Products.Shared.RoutingRuleValidation.LogTests.LoggerTests
   Information has been logged";
			Console.WriteLine(message);
			Console.WriteLine(_testAppender.LastMessage);

			Assert.IsTrue(_testAppender.LastMessage.Contains(message));
		}

		[Test]
		public void TestLoggingError()
		{

			_log.Error("Information has been logged");

			string message =@"ERR Source=CargoWise.eHub.Products.Shared.RoutingRuleValidation.LogTests.LoggerTests
   Information has been logged";

			Assert.IsTrue(_testAppender.LastMessage.Contains(message));
		}

		[Test]
		public void TestLoggingWarning()
		{
			_log.Warn("Information has been logged");

			string message =@"WRN Source=CargoWise.eHub.Products.Shared.RoutingRuleValidation.LogTests.LoggerTests
   Information has been logged";

			Assert.IsTrue(_testAppender.LastMessage.Contains(message));
		}

		[Test]
		public void TestLoggingFatal()
		{
			_log.Fatal("Information has been logged");

			string message =@"FAL Source=CargoWise.eHub.Products.Shared.RoutingRuleValidation.LogTests.LoggerTests
   Information has been logged";

			Assert.IsTrue(_testAppender.LastMessage.Contains(message));
		}

		[Test]
		public void TestLoggingUnknown()
		{
			var unknownLevel = new log4net.Core.Level(9999, "Unknown");
			var logEvent = new log4net.Core.LoggingEvent(new log4net.Core.LoggingEventData
			{
				Level = unknownLevel,
				Message = "Information has been logged",
				TimeStampUtc = DateTime.Now
			});

			_testAppender.DoAppend(logEvent);

			string expectedMessage = $@"{logEvent.TimeStamp:yyyy-MM-dd'T'HH:mm:ss.fffzzz} UKN Source=?
   Information has been logged";

			Assert.IsTrue(_testAppender.LastMessage.Contains(expectedMessage));
		}

		[Test]
		public void TestLoggingTrace()
		{
			var Level = new log4net.Core.Level(9999, "TRACE");
			var logEvent = new log4net.Core.LoggingEvent(new log4net.Core.LoggingEventData
			{
				Level = Level,
				Message = "Information has been logged",
				TimeStampUtc = DateTime.Now
			});

			_testAppender.DoAppend(logEvent);

			string expectedMessage = $@"{logEvent.TimeStamp:yyyy-MM-dd'T'HH:mm:ss.fffzzz} TRC Source=?
   Information has been logged";

			Assert.IsTrue(_testAppender.LastMessage.Contains(expectedMessage));
		}

	}
}
