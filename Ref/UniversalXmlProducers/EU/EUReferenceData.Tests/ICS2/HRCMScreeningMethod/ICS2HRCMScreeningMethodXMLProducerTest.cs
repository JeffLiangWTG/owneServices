using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.ICS2HRCMScreeningMethod.Business.Tests
{
	[TestFixture]
	class ICS2HRCMScreeningMethodXMLProducerTest : CommonXmlProducerTest<ICS2HRCMScreeningMethodXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.ICS2HRCMScreeningMethodUrl;

		protected override string ExpectedXML =>
			TestHelper.ReadManifestResourceContentAsString("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.HRCMScreeningMethod.TestFiles.Output.EUICS2_HRCMScreeningMethod.xml");

		protected override string OutputFileName => "EUICS2_HRCMScreeningMethod.xml";

		protected override Stream ZipFileForTest =>
			TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.HRCMScreeningMethod.TestFiles.Input.RD_ICS2_HRCMScreeningMethod.zip");
	}
}
