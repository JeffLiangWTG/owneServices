using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.AESPreviousDocumentType.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles
{
	[TestFixture]
	sealed class AESPreviousDocumentTypeXMLProducerTest : CommonXmlProducerTest<AESPreviousDocumentTypeXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.AESPreviousDocumentTypeUrl;

		protected override Stream ZipFileForTest => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_PreviousDocumentType.zip");

		protected override string OutputFileName => "EUAES_PreviousDocumentType.xml";

		protected override string ExpectedXML => TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Output.EUAES_PreviousDocumentType.xml");
	}
}
