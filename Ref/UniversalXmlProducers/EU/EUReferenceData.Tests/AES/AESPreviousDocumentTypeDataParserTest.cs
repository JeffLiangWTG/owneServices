using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.AESPreviousDocumentType.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class AESPreviousDocumentTypeDataParserTest : CommonXmlDataParserTest<AESPreviousDocumentTypeDataParser>
	{
		protected override string RDEntityAttributeValue => "PreviousDocumentType";

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_PreviousDocumentType-Err2.xml");
		}

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_PreviousDocumentType-Err3.xml");
		}

		protected override Stream GetXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_PreviousDocumentType.xml");
		}
	}
}
