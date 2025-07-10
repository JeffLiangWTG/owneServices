using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.AESNationality.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class AESNationalityXMLProducerTest : CommonXmlProducerTest<AESNationalityXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.AESNationalityUrl;

		protected override Stream ZipFileForTest => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_Nationality.zip");

		protected override string OutputFileName => "EUAES_Nationality.xml";

		protected override string ExpectedXML => TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Output.EUAES_Nationality.xml");
	}
}
