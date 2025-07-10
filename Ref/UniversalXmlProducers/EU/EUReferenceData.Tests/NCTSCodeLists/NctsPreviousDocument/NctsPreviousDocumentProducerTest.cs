using System.Collections.Generic;
using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsPreviousDocumentProducerTest : NctsManyToOneCodeListXmlProducerAbstractTest<NctsPreviousDocumentProducer>
	{
		protected override NctsPreviousDocumentProducer Producer => new NctsPreviousDocumentProducer();

		protected override IEnumerable<CodeListInformation> CodeListDetails => new CodeListInformation[]
		{
			new CodeListInformation
			{
				InputFileName = "RD_NCTS-P5_PreviousDocumentType.zip",
				DownloadURL = "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_PreviousDocumentType.zip",
			},
			new CodeListInformation
			{
				InputFileName = "RD_NCTS-P5_PreviousDocumentExportType.zip",
				DownloadURL = "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_PreviousDocumentExportType.zip",
			}
		};

		protected override string OutputFileName => "RefCusCodeListZZ_EUN_DC40N.xml";
	}
}
