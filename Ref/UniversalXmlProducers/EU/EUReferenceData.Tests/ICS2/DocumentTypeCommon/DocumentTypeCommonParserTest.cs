using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.DocumentTypeCommon.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.DocumentTypeCommon.Tests
{
	[TestFixture]
	class DocumentTypeCommonDataParserTest : CommonXmlDataParserTest<DocumentTypeCommonDataParser>
	{
		protected override Stream GetXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.DocumentTypeCommon.TestFiles.Input.RD_ICS2_DocumentTypeCommon.xml");
		}

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.DocumentTypeCommon.TestFiles.Input.RD_ICS2_DocumentTypeCommon-Err2.xml");
		}

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.DocumentTypeCommon.TestFiles.Input.RD_ICS2_DocumentTypeCommon-Err3.xml");
		}

		protected override string RDEntityAttributeValue => Constants.DocumentTypeCommon.RDEntityAttributeValue;
	}
}
