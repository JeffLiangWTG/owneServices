using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.ICS2CountryCodeICS2MS.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.EUReferenceData.Tests;

namespace CargoWise.RefDbRepo.EUReferenceData.ICS2CountryCodeICS2MS.Tests
{
	class CountryCodeICS2MSXMLProducerTest : CommonXmlProducerTest<CountryCodeICS2MSXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.ICS2CountryCodeICS2MSUrl;

		protected override string ExpectedXML =>
			TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.CountryCodeICS2MS.TestFiles.Output.EUICS2_CountryCodeICS2MS.xml");

		protected override string OutputFileName => "EUICS2_CountryCodeICS2MS.xml";

		protected override Stream ZipFileForTest =>
			TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.CountryCodeICS2MS.TestFiles.Input.RD_ICS2_CountryCodeICS2MS.zip");
	}
}
