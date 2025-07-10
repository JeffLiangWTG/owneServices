using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class IATAXmlWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\iatalocode.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_IATACode.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_IATACode.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.UNLOCOAndIATACsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new IATAXmlWriter();
	}
}
