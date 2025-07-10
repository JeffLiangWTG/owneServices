using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class BeforePermitApplicationReasonCodeXmlWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\bpshou.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_BeforePermitApplicationReasonCode.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_BeforePermitApplicationReasonCode.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.BeforePermitApplicationReasonCodeCsvDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new BeforePermitApplicationReasonCodeXmlWriter();
	}
}
