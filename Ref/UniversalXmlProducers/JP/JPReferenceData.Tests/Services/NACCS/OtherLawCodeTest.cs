using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class OtherLawCodeTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\tahour.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_OtherLawCode.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_OtherLawCode.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.OtherLawCodeCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new OtherLawCodeXmlWriter();
	}
}
