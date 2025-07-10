using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class MSXDocumentTypeXmlWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\syoruikubun.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_MSXDocumentType.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_MSXDocumentType.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.MSXDocumentTypeCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new MSXDocumentTypeXmlWriter();
	}
}
