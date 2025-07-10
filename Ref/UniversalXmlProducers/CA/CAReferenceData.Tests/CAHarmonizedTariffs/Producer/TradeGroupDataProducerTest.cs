using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class TradeGroupDataProducerTest : TariffProducerTest
	{
		protected override string FunctionCode => Constants.ProgramFunctions.CATradeGroup;

		protected override string NothingNewPublishedMessage => $"CA Trade Group: Nothing new published since last process. Skip processing this time.";

		[Test]
		public void TestQueryDataAndParseToXMLFileWhenDownloadFileFaild()
		{
			var checker = new PreProcessChecker(FunctionCode);
			DownloaderMock.Setup(x => x.GetTradeGroupEffectiveDateAndUrl(It.IsAny<string>(), It.IsAny<string>())).Throws(new System.Exception("File downloads failed.URL: xxx"));
			LogBuilder.Clear();
			checker.MarkAsProcessRequired();
			TariffProducer.QueryDataAndParseToXMLFile();
			Assert.That(LogBuilder.ToString(), Does.Not.Contain("Error processing in Trade Group data : File downloads failed.URL: xxx"));
		}

		protected override void CreateDownloaderMockData()
		{
			DownloaderMock.Setup(x => x.GetTradeGroupEffectiveDateAndUrl(It.IsAny<string>(), It.IsAny<string>())).Returns((new System.DateTime(2021, 12, 08), ""));
			DownloaderMock.Setup(x => x.DownloadFile("", It.IsAny<string>())).Returns(false);
		}

		protected override IProducer CreateNewProducer()
		{
			return new TradeGroupDataProducer(LogBuilder, DownloaderMock.Object);
		}
	}
}
