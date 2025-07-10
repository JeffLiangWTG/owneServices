using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using CargoWise.RefDbRepo.EUReferenceData.TypeOfMeansOfTransport.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.TypeOfMeansOfTransport.Tests
{
	class TypeOfMeansOfTransportXMLProducerTest : CommonXmlProducerTest<TypeOfMeansOfTransportXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.TypeOfMeansOfTransportUrl;

		protected override string ExpectedXML =>
			TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.TypeOfMeansOfTransport.TestFiles.Output.EUICS2TypeOfMeansOfTransport.xml");

		protected override string OutputFileName => "EUICS2_TypeOfMeansOfTransport.xml";

		protected override Stream ZipFileForTest =>
			TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.TypeOfMeansOfTransport.TestFiles.Input.RD_ICS2_TypeOfMeansOfTransport.zip");
	}
}
