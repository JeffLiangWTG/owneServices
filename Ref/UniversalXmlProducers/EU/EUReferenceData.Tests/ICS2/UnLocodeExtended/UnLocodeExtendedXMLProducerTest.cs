using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business.ICS2;
using CargoWise.RefDbRepo.EUReferenceData.Services;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class UnLocodeExtendedXMLProducerTest : CommonXmlProducerTest<UnLocodeExtendedXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.ICS2UnLocodeExtendedUrl;
		protected override string ExpectedXML => TestHelper.ReadManifestResourceContentAsString("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.UnLocodeExtended.TestFiles.Output.EUICS2_UnLocodeExtended.xml");
		protected override string OutputFileName => "EUICS2_UnLocodeExtended.xml";
		protected override Stream ZipFileForTest => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.UnLocodeExtended.TestFiles.Input.RD_ICS2_UnLocodeExtended.zip");
	}
}
