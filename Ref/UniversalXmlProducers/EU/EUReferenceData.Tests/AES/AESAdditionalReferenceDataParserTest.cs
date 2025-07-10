using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.AESAdditionalReference.Business;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	class AESAdditionalReferenceDataParserTest : CommonXmlDataParserTest<AESAdditionalReferenceDataParser>
	{
		protected override Stream GetXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_AdditionalReference.xml");
		}

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_AdditionalReference-Err2.xml");
		}

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_AdditionalReference-Err3.xml");
		}

		protected override string RDEntityAttributeValue => Constants.AESAdditionalReference.RDEntityAttributeValue;
	}
}
