using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.AdditionalInformationCode.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.AdditionalSupplyChainActorRoleCode.Tests
{
	[TestFixture]
	class AdditionalSupplyChainActorRoleCodeXMLProducerTest : CommonXmlProducerTest<AdditionalSupplyChainActorRoleCodeXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.AdditionalSupplyChainActorRoleCodeUrl;

		protected override string ExpectedXML =>
			TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.AdditionalSupplyChainActorRoleCode.TestFiles.Output.EUICS2_AdditionalSupplyChainActorRoleCode.xml");

		protected override string OutputFileName => "EUICS2_AdditionalSupplyChainActorRoleCode.xml";

		protected override Stream ZipFileForTest =>
			TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.AdditionalSupplyChainActorRoleCode.TestFiles.Input.RD_ICS2_AdditionalSupplyChainActorRoleCode.zip");
	}
}
