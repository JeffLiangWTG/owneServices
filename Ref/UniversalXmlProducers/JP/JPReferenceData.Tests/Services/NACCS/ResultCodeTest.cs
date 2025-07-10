
using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class ResultCodeTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\err_all.zip";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_ResultCode.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_ResultCode.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.ResultCodeZipDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new ResultCodeXmlWriter();

		protected override string BasePageDownloadUrl => AppConfig.NACCS.CodeLists.ResultCodeHomePageUrl;

		protected override string BasePageHtmlFilePath => "CargoWise.RefDbRepo.JPReferenceData.Tests.TestFiles.ResultCodeBasePage.html";
	}
}
