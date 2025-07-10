using System;
using System.IO;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test
{
	[TestFixture]
	sealed class ConsoleErrorLoggerFixture
	{
		[Test]
		public void LogSimpleMessage()
		{
			using var sw = new StringWriter();
			Console.SetError(sw);

			logger.Log("Test Message");
			Assert.AreEqual("Test Message\r\n", sw.ToString());
		}

		[TestCase(null, typeof(ArgumentNullException))]
		[TestCase("", typeof(ArgumentException))]
		public void LogSimpleMessage_CannotBeNullOrEmptyGuardClause(string logMessage, Type expectedExceptionType)
		{
			Assert.Throws(expectedExceptionType, () => logger.Log(logMessage));
		}

		[Test]
		public void LogException()
		{
			using var sw = new StringWriter();
			Console.SetError(sw);

			logger.Log(new InvalidOperationException("My Exception message"));
			Assert.AreEqual("InvalidOperationException: My Exception message\r\n", sw.ToString());
		}

		[Test]
		public void LogException_CannotBeNullGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => logger.Log(exception: null));
		}

		[Test]
		public void LogMessageAndException()
		{
			using var sw = new StringWriter();
			Console.SetError(sw);

			logger.Log(new InvalidOperationException("My Exception message"), "Test Message");
			Assert.AreEqual("Test Message\r\nInvalidOperationException: My Exception message\r\n", sw.ToString());
		}

		[Test]
		public void LogMessageAndException_CannotBeNullOrEmptyGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => logger.Log(null, "Test Message"));
			Assert.Throws<ArgumentNullException>(() => logger.Log(new Exception(), null));
			Assert.Throws<ArgumentException>(() => logger.Log(new Exception(), ""));
		}

		[SetUp]
		public void Setup()
		{
			logger = new ConsoleErrorLogger();
		}

		ILogger logger;
	}
}
