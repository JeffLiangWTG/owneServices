using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.AESAdditionalInformation.Business;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	class AESAdditionalInformationDataParserTest : CommonXmlDataParserTest<AESAdditionalInformationDataParser>
	{
		protected override Stream GetXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_AdditionalInformation.xml");
		}

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_AdditionalInformation-Err2.xml");
		}

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_AdditionalInformation-Err3.xml");
		}

		protected override string RDEntityAttributeValue => Constants.AESAdditionalInformation.RDEntityAttributeValue;
	}
}
