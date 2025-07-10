using System;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	[TestFixture]
	sealed class FiscalExchangeRatesProcessManagerTest
	{
		[Test]
		public void ProcessCorrectXml()
		{
			var errorCollector = new StringBuilder();

			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFileAsXmlDocument(It.IsAny<string>())).Returns(TestHelper.GetResourceContentAsXmlDocument("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.FiscalExchangeRates.Input.correctInput.xml"));

			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var processManager = new Mock<FiscalExchangeRatesProcessManager>();
			processManager.Setup(x => x.GetDownloadManager()).Returns(downloadManager);

			var webClientProcessManager = new FiscalExchangeRatesWebClientProcessManager(new FiscalExchangeRatesBuilder(errorCollector));
			processManager.Setup(x => x.GetFiscalExchangeRatesBuilder()).Returns(webClientProcessManager.GetFiscalExchangeRatesBuilder());
			processManager.Object.RunProcess(TempFolder, errorCollector, new DateTime(2021, 01, 21, 10, 23, 55));
			downloadManager.Dispose();

			var generatedFile = Path.Combine(TempFolder, "RefExchangeRateZZ_NL Fiscal Exchange Rates_102355000.xml");
			if (File.Exists(generatedFile))
			{
				var generatedXml = File.ReadAllText(generatedFile);
				var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.FiscalExchangeRates.Output.RefExchangeRateZZ_FiscalExchangeRates_Correct.xml");

				Assert.AreEqual(expectedXml, generatedXml, "XML-file does not contain the correct information for the Fiscal Exchange Rates.");
			}
			else
			{
				if (errorCollector.Length > 0)
				{
					Assert.Fail($"No XML file was generated. Error occurred: {errorCollector}");
				}
				else
				{
					Assert.Fail("No XML File was generated.");
				}
			}
		}

		[Test]
		public void InvalidXml()
		{
			var errorCollector = new StringBuilder();

			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFileAsXmlDocument(It.IsAny<string>())).Returns(TestHelper.GetResourceContentAsXmlDocument("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.FiscalExchangeRates.Output.RefExchangeRateZZ_NLFiscalExchangeRates_133125000.xml"));

			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var processManager = new Mock<FiscalExchangeRatesProcessManager>();
			processManager.Setup(x => x.GetDownloadManager()).Returns(downloadManager);

			var webClientProcessManager = new FiscalExchangeRatesWebClientProcessManager(new FiscalExchangeRatesBuilder(errorCollector));
			processManager.Setup(x => x.GetFiscalExchangeRatesBuilder()).Returns(webClientProcessManager.GetFiscalExchangeRatesBuilder());
			processManager.Object.RunProcess(TempFolder, errorCollector, new DateTime(2021, 01, 21, 10, 23, 55));
			downloadManager.Dispose();

			Assert.That(errorCollector.ToString(), Does.Contain("Downloaded file is not a valid xml for Fiscal Exchange Rates"));
		}

		[Test]
		public void NoExchangeRatesInXml()
		{
			var errorCollector = new StringBuilder();

			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFileAsXmlDocument(It.IsAny<string>())).Returns(TestHelper.GetResourceContentAsXmlDocument("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.FiscalExchangeRates.Input.NoFiscalExchangeRates.xml"));
			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var processManager = new Mock<FiscalExchangeRatesProcessManager>();
			processManager.Setup(x => x.GetDownloadManager()).Returns(downloadManager);

			var webClientProcessManager = new FiscalExchangeRatesWebClientProcessManager(new FiscalExchangeRatesBuilder(errorCollector));
			processManager.Setup(x => x.GetFiscalExchangeRatesBuilder()).Returns(webClientProcessManager.GetFiscalExchangeRatesBuilder());
			processManager.Object.RunProcess(TempFolder, errorCollector, new DateTime(2021, 01, 21, 10, 23, 55));
			downloadManager.Dispose();

			Assert.That(errorCollector.ToString(), Does.Contain("Downloaded file does not contain Fiscal Exchange Rates"));
		}

		[Test]
		public void ReadFiscalExchangeRates()
		{
			var xmlDocument = TestHelper.GetResourceContentAsXmlDocument("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.FiscalExchangeRates.Input.correctInput.xml");
			var rateNodes = FiscalExchangeRatesProcessManager.ReadFiscalExchangeRates(xmlDocument);

			Assert.AreEqual(32, rateNodes.Count, "Not all nodes are read from the xml");
			Assert.AreEqual("USD", rateNodes[0].Currency);
			Assert.AreEqual(1.2101m, rateNodes[0].Rate);
		}

		[Test]
		public void ReadFiscalExchangeRatesNoRates()
		{
			var xmlDocument = TestHelper.GetResourceContentAsXmlDocument("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.FiscalExchangeRates.Input.NoFiscalExchangeRates.xml");
			var rateNodes = FiscalExchangeRatesProcessManager.ReadFiscalExchangeRates(xmlDocument);

			Assert.AreEqual(0, rateNodes.Count, "No nodes should be read from the xml.");
		}

		[Test]
		public void ReadFiscalExchangeRatesInvalidXml()
		{
			var xmlDocument = TestHelper.GetResourceContentAsXmlDocument("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.FiscalExchangeRates.Output.RefExchangeRateZZ_NLFiscalExchangeRates_133125000.xml");
			var processManager = new Mock<FiscalExchangeRatesProcessManager>();

			var ex = Assert.Throws<ProcessingException>(() => FiscalExchangeRatesProcessManager.ReadFiscalExchangeRates(xmlDocument));
			Assert.That(ex.Message, Is.EqualTo("Downloaded file is not a valid xml for Fiscal Exchange Rates"));
		}

		[SetUp]
		public void Setup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
	}
}
