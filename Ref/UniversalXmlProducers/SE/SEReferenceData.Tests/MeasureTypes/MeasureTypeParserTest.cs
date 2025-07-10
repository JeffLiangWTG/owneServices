using System.Net.Http;
using CargoWise.RefDbRepo.SEReferenceData.Business;
using CargoWise.RefDbRepo.SEReferenceData.Business.MeasureTypes;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SEReferenceData.Tests
{
	[TestFixture]
	sealed class MeasureTypeParserTest : XmlParserTest<measureType1>
	{
		protected override string GetInputFileUrl() => "https://distr.tullverket.se/tulltaxan/xml/tot/MeasureType_b84e4dd9-9403-4463-89fc-80c6fbc31452_231101.xml.gz.pgp";

		protected override string GetInputFileLocalPath() => "CargoWise.RefDbRepo.SEReferenceData.Tests.MeasureTypes.Input.MeasureType_b84e4dd9-9403-4463-89fc-80c6fbc31452_231101.xml.gz.pgp";

		protected override string GetUniversalXmlOutputPath() => "CargoWise.RefDbRepo.SEReferenceData.Tests.MeasureTypes.Output.RefCusConditionType_SE.xml";

		protected override string GetNoErrorsMessage() => string.Empty;

		protected override DownloadExportXml CreateXmlProducer(HttpClient client) => new DownloadExportXml(client);

		protected override XmlParser<measureType1> CreateXmlParser() => new MeasureTypeParser();

		protected override string GetFilePrefix() => ApplicationConfig.FilePrefix_MeasureType;
	}
}
