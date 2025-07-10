using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class CCIPreviousDocumentTypeProducerTest : CommonXmlProducerTest<CCIPreviousDocumentTypeProducer>
	{
		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_CCI_PreviousDocumentType.zip";
		protected override string ExpectedXML => TestHelper.ReadManifestResourceContentAsString("CargoWise.RefDbRepo.EUReferenceData.Tests.CCI.TestFiles.Output.RefCusCodeListZZ_EUN_214IM.xml");
		protected override Stream ZipFileForTest => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.CCI.TestFiles.Input.RD_CCI_PreviousDocumentType.zip");
		protected override string OutputFileName => "RefCusCodeListZZ_EUN_214IM.xml";
		protected override string ExpectedRefCusCodeTypeXML => TestHelper.ReadManifestResourceContentAsString("CargoWise.RefDbRepo.EUReferenceData.Tests.CCI.TestFiles.Output.RefCusCodeTypeZZ_EUN_214IM.xml");
		protected override string RefCusCodeTypeOutputFileName => "RefCusCodeTypeZZ_EUN_214IM.xml";

	}
}
