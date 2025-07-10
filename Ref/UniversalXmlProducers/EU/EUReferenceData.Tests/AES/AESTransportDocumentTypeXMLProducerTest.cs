using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.AESTransportDocumentType.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles
{
	[TestFixture]
	class AESTransportDocumentTypeXMLProducerTest : CommonXmlProducerTest<AESTransportDocumentTypeXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.AESTransportDocumentTypeUrl;

		protected override Stream ZipFileForTest => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_TransportDocumentType.zip");

		protected override string OutputFileName => "EUAES_TransportDocumentType.xml";

		protected override string ExpectedXML => TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Output.EUAES_TransportDocumentType.xml");
	}
}
