using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.AdditionalInformationCodeSubset.Business;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.AdditionalInformationCodeSubset.Tests
{
	[TestFixture]
	class AdditionalInformationCodeSubsetDataParserTest : CommonXmlDataParserTest<AdditionalInformationCodeSubsetDataParser>
	{
		protected override Stream GetXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.AdditionalInformationCodeSubset.TestFiles.Input.RD_ICS2_AdditionalInformationCodeSubset.xml");
		}

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.AdditionalInformationCodeSubset.TestFiles.Input.RD_ICS2_AdditionalInformationCodeSubset-Err2.xml");
		}

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.AdditionalInformationCodeSubset.TestFiles.Input.RD_ICS2_AdditionalInformationCodeSubset-Err3.xml");
		}

		protected override string RDEntityAttributeValue => Constants.AdditionalInformationSubsetCode.RDEntityAttributeValue;

	}
}
