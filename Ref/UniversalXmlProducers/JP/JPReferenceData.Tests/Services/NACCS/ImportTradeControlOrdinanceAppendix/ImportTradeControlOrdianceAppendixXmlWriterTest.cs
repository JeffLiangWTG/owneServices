using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class ImportTradeControlOrdianceAppendixXmlWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\boukan-i.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedImportTradeControlOrdinanceAppendixXml.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_ImportTradeControlOrdinanceAppendix.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.ImportTradeControlOrdinanceAppendixCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new ImportTradeControlOrdinanceAppendixXmlWriter();
	}
}
