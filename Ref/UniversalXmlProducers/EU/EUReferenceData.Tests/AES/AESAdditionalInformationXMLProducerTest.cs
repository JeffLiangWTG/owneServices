using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.AESAdditionalInformation.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles
{
	[TestFixture]
	class AESAdditionalInformationXMLProducerTest : CommonXmlProducerTest<AESAdditionalInformationXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.AESAdditionalInformationUrl;

		protected override Stream ZipFileForTest => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_AdditionalInformation.zip");

		protected override string OutputFileName => "EUAES_AdditionalInformation.xml";

		protected override string ExpectedXML => TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Output.EUAES_AdditionalInformation.xml");
	}
}
