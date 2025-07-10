using System;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	public abstract class TariffProducerTest
	{
		protected StringWriter ConsoleOutput { get; set; }
		protected TextWriter OriginalConsoleOutput { get; set; }

		protected StringBuilder LogBuilder { get; set; }

		protected Mock<IFileDownloader> DownloaderMock { get; set; }

		protected IProducer TariffProducer { get; set; }

		protected abstract string FunctionCode { get; }

		protected abstract string NothingNewPublishedMessage { get; }

		[Test]
		public virtual void TestNothingNewPublished()
		{
			var checker = new PreProcessChecker(FunctionCode);
			var expectedMessage = NothingNewPublishedMessage;
			LogBuilder.Clear();
			ConsoleOutput.GetStringBuilder().Clear();
			checker.MarkAsProcessRequired();
			TariffProducer.QueryDataAndParseToXMLFile();
			Assert.That(LogBuilder.ToString(), Does.Not.Contain(expectedMessage));
			Assert.That(ConsoleOutput.ToString(), Does.Not.Contain(expectedMessage));

			LogBuilder.Clear();
			ConsoleOutput.GetStringBuilder().Clear();
			TariffProducer.QueryDataAndParseToXMLFile();
			Assert.That(LogBuilder.ToString(), Does.Not.Contain(expectedMessage));
			Assert.That(ConsoleOutput.ToString(), Does.Contain(expectedMessage));

			LogBuilder.Clear();
			ConsoleOutput.GetStringBuilder().Clear();
			checker.MarkAsProcessRequired();
			TariffProducer.QueryDataAndParseToXMLFile();
			Assert.That(LogBuilder.ToString(), Does.Not.Contain(expectedMessage));
			Assert.That(ConsoleOutput.ToString(), Does.Not.Contain(expectedMessage));
		}

		[SetUp]
		public virtual void SetUp()
		{
			ConsoleOutput = new StringWriter();
			OriginalConsoleOutput = Console.Out;
			Console.SetOut(ConsoleOutput);

			LogBuilder = new StringBuilder();
			DownloaderMock = new Mock<IFileDownloader>();
			CreateDownloaderMockData();
			TariffProducer = CreateNewProducer();
		}

		protected abstract IProducer CreateNewProducer();

		protected abstract void CreateDownloaderMockData();


		[TearDown]
		public void Cleanup()
		{
			Console.SetOut(OriginalConsoleOutput);
			ConsoleOutput.Dispose();
		}
	}
}
