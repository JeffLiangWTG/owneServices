using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.ICS2FunctionalErrorCodes.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.ICS2FunctionalErrorCodes
{
	[TestFixture]
	class ICS2FunctionalErrorCodesDataParserTest : CommonXmlDataParserTest<ICS2FunctionalErrorCodesDataParser>
	{
		protected override Stream GetXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.ICS2FunctionalErrorCodes.TestFiles.Input.RD_ICS2_ICS2FunctionalErrorCodes.xml");
		}

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.ICS2FunctionalErrorCodes.TestFiles.Input.RD_ICS2_ICS2FunctionalErrorCodes-Err2.xml");
		}

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.ICS2FunctionalErrorCodes.TestFiles.Input.RD_ICS2_ICS2FunctionalErrorCodes-Err3.xml");
		}

		protected override string RDEntityAttributeValue => Constants.ICS2FunctionalErrorCodes.RDEntityAttributeValue;

	}
}
