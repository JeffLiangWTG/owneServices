using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.ICS2CountryCodeICS2MS.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.ICS2CountryCodeICS2MS.Tests
{
	[TestFixture]
	class CountryCodeICS2MSDataParserTest : CommonXmlDataParserTest<CountryCodeICS2MSDataParser>
	{
		protected override Stream GetXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.CountryCodeICS2MS.TestFiles.Input.RD_ICS2_CountryCodeICS2MS.xml");
		}

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.CountryCodeICS2MS.TestFiles.Input.RD_ICS2_CountryCodeICS2MS_Invalid.xml");

		}

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.CountryCodeICS2MS.TestFiles.Input.RD_ICS2_CountryCodeICS2MS_EntityNotFound.xml");
		}

		protected override string RDEntityAttributeValue => Constants.CountryCodeICS2MS.RDEntityAttributeValue;
	}
}
