using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.AESNationality.Business;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class AESNationalityDataParserTest : CommonXmlDataParserTest<AESNationalityDataParser>
	{
		protected override string RDEntityAttributeValue => Constants.AESNationality.RDEntityAttributeValue;

		protected override Stream GetInvalidXMLFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_Nationality-Err2.xml");

		protected override Stream GetRDEntityNotFoundErrorFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_Nationality-Err3.xml");

		protected override Stream GetXMLFile() => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.AES.TestFiles.Input.RD_AES_Nationality.xml");
	}
}
