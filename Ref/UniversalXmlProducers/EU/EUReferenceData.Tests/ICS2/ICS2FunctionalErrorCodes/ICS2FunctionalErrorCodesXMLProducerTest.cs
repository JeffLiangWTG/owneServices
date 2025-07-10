using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.ICS2FunctionalErrorCodes.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.ICS2FunctionalErrorCodes
{
	[TestFixture]
	class ICS2FunctionalErrorCodesXMLProducerTest : CommonXmlProducerTest<ICS2FunctionalErrorCodesXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.ICS2FunctionalErrorCodesUrl;

		protected override Stream ZipFileForTest => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.ICS2FunctionalErrorCodes.TestFiles.Input.RD_ICS2_ICS2FunctionalErrorCodes.zip");

		protected override string OutputFileName => "EUICS2_ICS2FunctionalErrorCodes.xml";

		protected override string ExpectedXML => TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.ICS2FunctionalErrorCodes.TestFiles.Output.EUICS2_ICS2FunctionalErrorCodes.xml");
	}
}
