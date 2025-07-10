using System.Linq;
using CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Tests
{
	class CUSNumberSOAPResponseParserTest
	{
		[Test]
		public void ParsePage()
		{
			var content = TestHelper.ReadManifestResourceContentAsString("CargoWise.RefDbRepo.EUReferenceData.Tests.CUSNumbers.TestFiles.Input.Soap_1_1.xml");
			var actual = new CUSNumberSOAPResponseParser(content).Parse();
			Assert.Multiple(() =>
			{
				Assert.That(actual, Has.Count.EqualTo(10), "CUSNumber Objects");
				Assert.That(actual.Sum(c => c.Translations.Count()), Is.EqualTo(166), "Translations");
			});
		}
	}
}
