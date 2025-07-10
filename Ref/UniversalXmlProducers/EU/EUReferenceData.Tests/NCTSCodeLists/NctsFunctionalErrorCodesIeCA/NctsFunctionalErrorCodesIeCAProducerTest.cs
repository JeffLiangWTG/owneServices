using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsFunctionalErrorCodesIeCAProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsFunctionalErrorCodesIeCAProducer>
	{
		protected override NctsFunctionalErrorCodesIeCAProducer Producer => new NctsFunctionalErrorCodesIeCAProducer();

		protected override string InputFileName => "RD_NCTS-P5_FunctionalErrorCodesIeCA.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL180.xml";

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_FunctionalErrorCodesIeCA.zip";
	}
}
