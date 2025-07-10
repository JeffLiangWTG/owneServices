using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class AuthorisationTypeDataParserTest : CommonXmlDataParserTest<AuthorisationTypeDataParser>
	{
		protected override Stream GetXMLFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.CCI.TestFiles.Input.RD_CCI_AuthorisationType.xml");

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.CCI.TestFiles.Input.RD_CCI_AuthorisationType_Invalid.xml");
		}

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.CCI.TestFiles.Input.RD_CCI_AuthorisationType_EntityNotFound.xml");
		}

		protected override string RDEntityAttributeValue => Constants.AuthorisationType.RDEntityAttributeValue;
	}
}
