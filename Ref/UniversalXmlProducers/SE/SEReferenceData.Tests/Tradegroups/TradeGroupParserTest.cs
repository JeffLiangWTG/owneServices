using System.Net.Http;
using CargoWise.RefDbRepo.SEReferenceData.Business;
using CargoWise.RefDbRepo.SEReferenceData.Business.TradeGroups;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SEReferenceData.Tests
{
	[TestFixture]
	sealed class TradeGroupParserTest : XmlParserTest<geographicalArea>
	{
		protected override string GetInputFileUrl() => "https://distr.tullverket.se/tulltaxan/xml/tot/GeographicalArea_b84e4dd9-9403-4463-89fc-80c6fbc31452_231101.xml.gz.pgp";

		protected override string GetInputFileLocalPath() => "CargoWise.RefDbRepo.SEReferenceData.Tests.Tradegroups.TestFiles.Input.GeographicalArea_b84e4dd9-9403-4463-89fc-80c6fbc31452_231101.xml.gz.pgp";

		protected override string GetUniversalXmlOutputPath() => "CargoWise.RefDbRepo.SEReferenceData.Tests.Tradegroups.TestFiles.Output.RefCusTradeGroup_SE.xml";

		protected override string GetNoErrorsMessage() => string.Empty;

		protected override DownloadExportXml CreateXmlProducer(HttpClient client) => new DownloadExportXml(client);

		protected override XmlParser<geographicalArea> CreateXmlParser() => new TradeGroupParser();

		protected override string GetFilePrefix() => ApplicationConfig.FilePrefix_GeographicalArea;
	}
}
