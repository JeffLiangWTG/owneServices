using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class TariffDataProducerTest : TariffProducerTest
	{
		protected override string FunctionCode => Constants.ProgramFunctions.CATariff;

		protected override string NothingNewPublishedMessage => $"CA Harmonized Tariff: Nothing new published since last process. Skip processing this time.";

		[Test]
		public void TestQueryDataAndParseToXMLFileWhenDownloadFileFaild()
		{
			var checker = new PreProcessChecker(FunctionCode);
			DownloaderMock.Setup(x => x.GetLastEditDateAndAccessDbUrl(It.IsAny<string>())).Throws(new System.Exception("File downloads failed.URL: xxx"));
			LogBuilder.Clear();
			checker.MarkAsProcessRequired();
			TariffProducer.QueryDataAndParseToXMLFile();
			Assert.That(LogBuilder.ToString(), Does.Not.Contain("Error processing in Harmonized Tariff data : File downloads failed.URL: xxx"));
		}

		protected override void CreateDownloaderMockData()
		{
			DownloaderMock.Setup(x => x.GetLastEditDateAndAccessDbUrl(It.IsAny<string>())).Returns((new System.DateTime(2021, 12, 08), ""));
			DownloaderMock.Setup(x => x.DownloadFile("", It.IsAny<string>())).Returns(false);
		}

		protected override IProducer CreateNewProducer()
		{
			return new TariffDataProducer(LogBuilder, DownloaderMock.Object);
		}
	}
}
