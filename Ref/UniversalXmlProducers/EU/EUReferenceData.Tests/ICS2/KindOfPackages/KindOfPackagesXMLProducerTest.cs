using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.KindOfPackages.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.KindOfPackages.Tests
{
	[TestFixture]
	sealed class KindOfPackagesXMLProducerTest : CommonXmlProducerTest<KindOfPackagesXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.KindOfPackagesDownloadUrl;

		protected override Stream ZipFileForTest => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.KindOfPackages.TestFiles.Input.RD_ICS2_KindOfPackages.zip");

		protected override string OutputFileName => "EUICS2_KindOfPackages.xml";

		protected override string ExpectedXML => TestHelper.ReadManifestResourceContentAsString("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.KindOfPackages.TestFiles.Output.EUICS2_KindOfPackages.xml");
	}
}
