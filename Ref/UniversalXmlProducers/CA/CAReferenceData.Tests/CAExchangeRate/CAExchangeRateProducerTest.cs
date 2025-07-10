using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAExchangeRate;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	sealed class CAExchangeRateProducerTest : TestWithApplicationTestConfig
	{
		[Test]
		public void TestNothingNewPublished()
		{
			var outputFile = "CBSAExchangeRates.xml";
			var exportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + outputFile + "");
			var checker = new PreProcessChecker(Constants.ProgramFunctions.CAExchangeRate);
			var expectedMessage = $"Nothing new published since last process. Skip processing this time.";
			_builder.Clear();
			consoleOutput.GetStringBuilder().Clear();
			checker.MarkAsProcessRequired();
			parser.QueryDataAndParseToXMLFile(exportFilePath);
			Assert.That(_builder.ToString(), Does.Not.Contain(expectedMessage));
			Assert.That(consoleOutput.ToString(), Does.Not.Contain(expectedMessage));

			_builder.Clear();
			consoleOutput.GetStringBuilder().Clear();
			parser.QueryDataAndParseToXMLFile(exportFilePath);
			Assert.That(_builder.ToString(), Does.Not.Contain(expectedMessage));
			Assert.That(consoleOutput.ToString(), Does.Contain(expectedMessage));

			_builder.Clear();
			consoleOutput.GetStringBuilder().Clear();
			checker.MarkAsProcessRequired();
			parser.QueryDataAndParseToXMLFile(exportFilePath);
			Assert.That(_builder.ToString(), Does.Not.Contain(expectedMessage));
			Assert.That(consoleOutput.ToString(), Does.Not.Contain(expectedMessage));
		}

		[Test]
		public void TestParseExchangeRateIntoXML_RunsInBothWeekdayAndWeekend()
		{
			var outputFile = "CBSAExchangeRates.xml";
			var exportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + outputFile + "");
			var checker = new PreProcessChecker(Constants.ProgramFunctions.CAExchangeRate);
			var expectedMessage = $"CA CBP publishes rates for the next day's rate between 7:00 p.m. and 11:59 p.m. Sunday through Thursday.";
			_builder.Clear();
			consoleOutput.GetStringBuilder().Clear();
			checker.MarkAsProcessRequired();
			parser.GetNowInCanada = () => new DateTime(2025, 04, 15, 20, 00, 00);
			parser.QueryDataAndParseToXMLFile(exportFilePath);
			var message = _builder.ToString();
			Assert.That(message, Does.Not.Contain(expectedMessage));
			Assert.That(consoleOutput.ToString(), Does.Not.Contain(expectedMessage));

			_builder.Clear();
			consoleOutput.GetStringBuilder().Clear();
			parser.GetNowInCanada = () => new DateTime(2025, 04, 19, 00, 00, 00);
			parser.QueryDataAndParseToXMLFile(exportFilePath);
			message = _builder.ToString();
			Assert.That(message, Does.Not.Contain(expectedMessage));
			Assert.That(consoleOutput.ToString(), Does.Not.Contain(expectedMessage));
		}

		[Test]
		public void QueryDataAndParseToXMLFile_ParseExchangeRateIntoXMLReturnsFalse_SendEmail()
		{
			var outputFile = "CBSAExchangeRates.xml";
			var exportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + outputFile + "");
			var checker = new PreProcessChecker(Constants.ProgramFunctions.CAExchangeRate);
			EmailSender.SentEmails.Clear();
			parserMock.Setup(x => x.ParseExchangeRateIntoXML(It.IsAny<List<ForeignExchangeRates>>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(false);
			checker.MarkAsProcessRequired();
			parser.QueryDataAndParseToXMLFile(exportFilePath);
			Assert.That(EmailSender.SentEmails.Count, Is.EqualTo(1));

			EmailSender.SentEmails.Clear();
			parserMock.Setup(x => x.ParseExchangeRateIntoXML(It.IsAny<List<ForeignExchangeRates>>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(true);
			checker.MarkAsProcessRequired();
			parser.QueryDataAndParseToXMLFile(exportFilePath);
			Assert.That(EmailSender.SentEmails.Count, Is.EqualTo(0));
		}

		[Test]
		public void QueryDataAndParseToXMLFile_ExceptionThrown_SendEmail()
		{
			var outputFile = "CBSAExchangeRates.xml";
			var exportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + outputFile + "");
			var checker = new PreProcessChecker(Constants.ProgramFunctions.CAExchangeRate);
			EmailSender.SentEmails.Clear();
			callerMock.Setup(x => x.QueryAndParseResponse(new DateTime(2023, 02, 16, 20, 00, 00))).Throws(new Exception("Test exception"));
			checker.MarkAsProcessRequired();
			parser.QueryDataAndParseToXMLFile(exportFilePath);
			Assert.That(EmailSender.SentEmails.Count, Is.EqualTo(1));

			EmailSender.SentEmails.Clear();
			parserMock.Setup(x => x.ParseExchangeRateIntoXML(It.IsAny<List<ForeignExchangeRates>>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(true);
			checker.MarkAsProcessRequired();
			parser.QueryDataAndParseToXMLFile(exportFilePath);
			Assert.That(EmailSender.SentEmails.Count, Is.EqualTo(0));
		}

		[SetUp]
		public override void SetUp()
		{
			consoleOutput = new StringWriter();
			originalConsoleOutput = Console.Out;
			Console.SetOut(consoleOutput);

			var currency = new FromCurrency() { Value = "HKD" };
			ForeignExchangeRates exchangeRate = new ForeignExchangeRates()
			{
				Rate = "0.1",
				ExchangeRateEffectiveTimestamp = "2023-02-15T00:00:00.000Z",
				ExchangeRateExpiryTimestamp = "2023-02-15T23:59:59.000Z",
				FromCurrency = currency
			};

			_builder = new StringBuilder();
			callerMock = new Mock<IWebServiceCaller>();
			callerMock.Setup(x => x.QueryAndParseResponse(new DateTime(2023, 02, 16, 20, 00, 00))).Verifiable();
			callerMock.Setup(x => x.ExchangeRates).Returns(new List<ForeignExchangeRates>() { exchangeRate });
			parserMock = new Mock<ICAExchangeRateParser>();
			parser = new CAExchangeRateProducer(callerMock.Object, _builder, parserMock.Object)
			{
				GetNowInCanada = () => new DateTime(2023, 02, 16, 20, 00, 00),
			};
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Console.SetOut(originalConsoleOutput);
			consoleOutput.Dispose();
		}

		Mock<IWebServiceCaller> callerMock;
		Mock<ICAExchangeRateParser> parserMock;
		CAExchangeRateProducer parser;
		StringBuilder _builder;
		StringWriter consoleOutput;
		TextWriter originalConsoleOutput;
	}
}
