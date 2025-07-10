using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class ManifestPreviousDocumentTypeProducerTest : NCTSCodeListXmlProducerAbstractTest<ManifestPreviousDocumentTypeProducer>
	{
		protected override ManifestPreviousDocumentTypeProducer Producer => new ManifestPreviousDocumentTypeProducer();

		protected override string InputFileName => "RD_NCTS-P5_PreviousDocumentType.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_DC40M.xml";

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_PreviousDocumentType.zip";
	}
}
