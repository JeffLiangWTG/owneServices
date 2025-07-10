using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Tests
{
	[TestFixture]
	class RDEntryListCustomsOfficesParserTest
	{
		[Test]
		public void ParseXML()
		{
			var inputFile = TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.OfficeCodes.TestFiles.Input.COL-Generic-20201222_SingleCustomsOffice.txt");
			var parser = new RDEntryListCustomsOfficesParser(errorBuilder);
			var result = parser.ParseXML(XDocument.Load(inputFile));
			Assert.That(result.Count(), Is.EqualTo(1));
			Assert.That(errorBuilder.ToString(), Is.Empty);
		}

		[Test]
		public void InvalidDateErrorMessage() => AssertCorrectErrorMessage(@"Unable to import CustomsOffice-record due to missing ReferenceNumber, CountryCode, UsualName, Role or invalid Start-/EndDate.
DETAILS:
ReferenceNumber: AD000001
CountryCode: AD
UsualName: CUSTOMS OFFICE SANT JULIÀ DE LÒRIA
StartDate: 2020-13-23
EndDate: 2020-03-23
Roles count: 4
");

		[Test]
		public void EmptyReferenceNumberErrorMessage() => AssertCorrectErrorMessage(@"Unable to import CustomsOffice-record due to missing ReferenceNumber, CountryCode, UsualName, Role or invalid Start-/EndDate.
DETAILS:
ReferenceNumber: 
CountryCode: AD
UsualName: CUSTOMS OFFICE SANT JULIÀ DE LÒRIA
StartDate: 2020-01-23
EndDate: 2020-03-23
Roles count: 4
");

		[Test]
		public void EmptyCountryCodeErrorMessage() => AssertCorrectErrorMessage(@"Unable to import CustomsOffice-record due to missing ReferenceNumber, CountryCode, UsualName, Role or invalid Start-/EndDate.
DETAILS:
ReferenceNumber: AD000002
CountryCode: 
UsualName: CUSTOMS OFFICE SANT JULIÀ DE LÒRIA
StartDate: 2020-01-23
EndDate: 2020-03-23
Roles count: 4
");

		[Test]
		public void EmptyCustomsOfficeUsualNameErrorMessage() => AssertCorrectErrorMessage(@"Unable to import CustomsOffice-record due to missing ReferenceNumber, CountryCode, UsualName, Role or invalid Start-/EndDate.
DETAILS:
ReferenceNumber: AD000003
CountryCode: AD
UsualName: 
StartDate: 2020-01-23
EndDate: 2020-03-23
Roles count: 4
");

		[Test]
		public void MissingRoleErrorMessage() => AssertCorrectErrorMessage(@"Unable to import CustomsOffice-record due to missing ReferenceNumber, CountryCode, UsualName, Role or invalid Start-/EndDate.
DETAILS:
ReferenceNumber: AD000004
CountryCode: AD
UsualName: CUSTOMS OFFICE SANT JULIÀ DE LÒRIA
StartDate: 2020-01-23
EndDate: 2020-03-23
Roles count: 0
");

		void AssertCorrectErrorMessage(string expectedErrorMessageDetails)
		{
			var inputFile = TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.OfficeCodes.TestFiles.Input.COL-Generic-20201222_FAULTY.txt");
			var parser = new RDEntryListCustomsOfficesParser(errorBuilder);
			parser.ParseXML(XDocument.Load(inputFile));
			Assert.That(errorBuilder.ToString(), Does.Contain(expectedErrorMessageDetails));
		}

		[SetUp]
		public void Setup()
		{
			errorBuilder = new StringBuilder();
		}
		StringBuilder errorBuilder;
	}
}
