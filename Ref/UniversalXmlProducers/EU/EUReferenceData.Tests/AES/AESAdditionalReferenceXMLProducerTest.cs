using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.AESAdditionalReference.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles
{
	[TestFixture]
	class AESAdditionalReferenceXMLProducerTest : CommonXmlProducerTest<AESAdditionalReferenceXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.AESAdditionalReferenceUrl;

		protected override Stream ZipFileForTest => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_AdditionalReference.zip");

		protected override string OutputFileName => "EUAES_AdditionalReference.xml";

		protected override string ExpectedXML => TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Output.EUAES_AdditionalReference.xml");
	}
}
