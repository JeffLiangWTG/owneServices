using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.AdditionalInformationCodeSubset.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.AdditionalInformationCodeSubset.Tests
{
	[TestFixture]
	class AdditionalInformationCodeSubsetXMLProducerTest : CommonXmlProducerTest<AdditionalInformationCodeSubsetXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.AdditionalInformationCodeSubsetUrl;

		protected override Stream ZipFileForTest => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.AdditionalInformationCodeSubset.TestFiles.Input.RD_ICS2_AdditionalInformationCodeSubset.zip");

		protected override string OutputFileName => "EUICS2_AdditionalInformationCodeSubset.xml";

		protected override string ExpectedXML => TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.AdditionalInformationCodeSubset.TestFiles.Output.EUICS2_AdditionalInformationCodeSubset.xml");
	}
}
