using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class BondedAreaCodeXmlWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\hozei.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_BondedAreaCode.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_BondedAreaCode.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.BondedAreaCodeCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new BondedAreaCodeXmlWriter();

		protected override string BasePageDownloadUrl => AppConfig.NACCS.CodeLists.BondedAreaCodeHomePageUrl;

		protected override string BasePageHtmlFilePath => "CargoWise.RefDbRepo.JPReferenceData.Tests.TestFiles.BondedAreaCodeBasePage.html";
	}
}
