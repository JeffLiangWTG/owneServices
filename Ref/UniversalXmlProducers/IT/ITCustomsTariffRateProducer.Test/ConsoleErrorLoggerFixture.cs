using System;
using System.IO;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	[TestFixture]
	sealed class ConsoleErrorLoggerFixture
	{
		[Test]
		public void LogSimpleMessage()
		{
			using (var sw = new StringWriter())
			{
				Console.SetError(sw);

				logger.Log("Test Message");
				Assert.AreEqual("Test Message\r\n", sw.ToString());
			}
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
			using (var sw = new StringWriter())
			{
				Console.SetError(sw);

				logger.Log(new Exception("My Exception"));
				Assert.AreEqual("My Exception\r\n", sw.ToString());
			}
		}

		[Test]
		public void LogException_CannotBeNullGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => logger.Log(exception: null));
		}

		[Test]
		public void LogExceptionForSpecificTariff()
		{
			using (var sw = new StringWriter())
			{
				Console.SetError(sw);

				logger.Log("2711110000", new Exception("My Exception"));
				Assert.AreEqual("2711110000\r\n" + "My Exception\r\n", sw.ToString());
			}
		}

		[Test]
		public void LogExceptionForSpecificTariff_CannotBeNullOrEmptyGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => logger.Log(tariffCode: null, new Exception("")));
			Assert.Throws<ArgumentException>(() => logger.Log(tariffCode: "", new Exception("")));
			Assert.Throws<ArgumentNullException>(() => logger.Log(tariffCode: "123", exception: null));
		}

		[Test]
		public void LogParsingResultError()
		{
			using (var sw = new StringWriter())
			{
				Console.SetError(sw);

				var parsingResult = new ParsingResult(
					"7219331010",
					"An Error Occurred",
					"This is the error description");

				logger.Log(parsingResult);
				Assert.AreEqual(
					"TariffCode: 7219331010\r\n" +
					"ErrorMessage: An Error Occurred\r\n" +
					"StackTrace: CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.ConsoleErrorLoggerFixture : LogParsingResultError\r\n" +
					"ErrorDescription: This is the error description\r\n",
					sw.ToString());
			}
		}

		[Test]
		public void LogParsingResultError_CannotBeNullGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => logger.Log(parsingResult: null));
		}

		[SetUp]
		public void Setup()
		{
			logger = new ConsoleErrorLogger();
		}

		ILogger logger;
	}
}
