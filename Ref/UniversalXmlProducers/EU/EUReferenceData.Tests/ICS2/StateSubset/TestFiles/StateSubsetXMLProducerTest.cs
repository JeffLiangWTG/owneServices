using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using CargoWise.RefDbRepo.EUReferenceData.StateSubset.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.StateSubset.Tests
{
	class StateSubsetXMLProducerTest : CommonXmlProducerTest<StateSubsetXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.StateSubsetUrl;

		protected override string ExpectedXML =>
			TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.StateSubset.TestFiles.Output.EUICS2StateSubset.xml");

		protected override string OutputFileName => "EUICS2_StateSubset.xml";

		protected override Stream ZipFileForTest =>
			TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.StateSubset.TestFiles.Input.RD_ICS2_StateSubset.zip");
	}
}
