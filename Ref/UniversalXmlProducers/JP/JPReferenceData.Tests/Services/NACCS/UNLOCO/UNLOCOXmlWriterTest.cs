using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests;

sealed class UNLOCOXmlWriterTest : BaseNaccsXmlWriterTest
{
	protected override string InputFilePath => @"TestFiles\iatalocode.csv";

	protected override string ExpectedOutputFilePath => @"TestFiles\Expected_RefCusCodeList_JP_UNLOCO.xml";

	protected override string ActualOutputFilePath => $"RefCusCodeList_JP_UNLOCO.xml";

	protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.UNLOCOAndIATACsvFileDownloadUrl;

	protected override NaccsXmlWriter GetNaccsXmlWriter() => new UNLOCOXmlWriter();
}
