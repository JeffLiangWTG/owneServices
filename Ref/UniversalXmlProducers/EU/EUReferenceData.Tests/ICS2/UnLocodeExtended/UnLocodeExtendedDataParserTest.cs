using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.Business.ICS2;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class UnLocodeExtendedDataParserTest : CommonXmlDataParserTest<UnLocodeExtendedDataParser>
	{
		protected override Stream GetXMLFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.UnLocodeExtended.TestFiles.Input.RD_ICS2_UnLocodeExtended.xml");

		protected override Stream GetInvalidXMLFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.UnLocodeExtended.TestFiles.Input.RD_ICS2_UnLocodeExtended_Invalid.xml");

		protected override Stream GetRDEntityNotFoundErrorFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.UnLocodeExtended.TestFiles.Input.RD_ICS2_UnLocodeExtended_EntityNotFound.xml");

		protected override string RDEntityAttributeValue => Constants.UnLocodeExtended.RDEntityAttributeValue;
	}
}
