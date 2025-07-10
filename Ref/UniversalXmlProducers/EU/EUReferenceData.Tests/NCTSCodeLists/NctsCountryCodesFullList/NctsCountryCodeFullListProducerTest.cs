using System;
using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsCountryCodeFullListProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsCountryCodesFullListProducer>
	{
		protected override NctsCountryCodesFullListProducer Producer => new NctsCountryCodesFullListProducer();

		protected override string InputFileName => "RD_NCTS-P5_CountryCodesFullList.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_NC008.xml";

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_CountryCodesFullList.zip";
	}
}
