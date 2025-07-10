using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class CCIPreviousDocumentTypeDataParserTest : CommonXmlDataParserTest<CCIPreviousDocumentTypeDataParser>
	{
		protected override Stream GetXMLFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.CCI.TestFiles.Input.RD_CCI_PreviousDocumentType.xml");

		protected override Stream GetInvalidXMLFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.CCI.TestFiles.Input.RD_CCI_PreviousDocumentType_Invalid.xml");

		protected override Stream GetRDEntityNotFoundErrorFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.CCI.TestFiles.Input.RD_CCI_PreviousDocumentType_EntityNotFound.xml");

		protected override string RDEntityAttributeValue => Constants.CCIPreviousDocumentType.RDEntityAttributeValue;
	}
}
