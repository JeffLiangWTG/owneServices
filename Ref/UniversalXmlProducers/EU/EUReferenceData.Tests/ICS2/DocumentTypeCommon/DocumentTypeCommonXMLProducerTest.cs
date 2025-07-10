using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.DocumentTypeCommon.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.EUReferenceData.Tests;

namespace CargoWise.RefDbRepo.EUReferenceData.DocumentTypeCommon.Tests
{
	class DocumentTypeCommonXMLProducerTest : CommonXmlProducerTest<DocumentTypeCommonXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.DocumentTypeCommonDownloadUrl;

		protected override Stream ZipFileForTest => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.DocumentTypeCommon.TestFiles.Input.RD_ICS2_DocumentTypeCommon.zip");

		protected override string OutputFileName => "EUICS2_DocumentTypeCommon.xml";

		protected override string ExpectedXML => TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.DocumentTypeCommon.TestFiles.Output.EUICS2_DocumentTypeCommon.xml");
	}
}
