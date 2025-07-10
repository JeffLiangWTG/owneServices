using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.AdditionalInformationCode.Business;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.AdditionalSupplyChainActorRoleCode.Tests
{
	[TestFixture]
	class AdditionalSupplyChainActorRoleCodeDataParserTest : CommonXmlDataParserTest<AdditionalSupplyChainActorRoleCodeDataParser>
	{
		protected override Stream GetXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.AdditionalSupplyChainActorRoleCode.TestFiles.Input.RD_ICS2_AdditionalSupplyChainActorRoleCode.xml");
		}

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.AdditionalSupplyChainActorRoleCode.TestFiles.Input.RD_ICS2_AdditionalSupplyChainActorRoleCode-Err2.xml");
		}

		protected override string RDEntityAttributeValue => Constants.AdditionalSupplyChainActorRoleCode.RDEntityAttributeValue;

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.AdditionalSupplyChainActorRoleCode.TestFiles.Input.RD_ICS2_AdditionalSupplyChainActorRoleCode-Err3.xml");
		}

		protected override bool IsAdditionalTranslationSupporter => true;
	}
}
