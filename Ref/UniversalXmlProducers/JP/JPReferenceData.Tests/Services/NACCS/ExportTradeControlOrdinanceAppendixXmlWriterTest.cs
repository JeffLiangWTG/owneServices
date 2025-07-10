using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class ExportTradeControlOrdinanceAppendixXmlWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\boukan-e1.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_ExportTradeControlOrdinanceAppendix.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_ExportTradeControlOrdinanceAppendix.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.ExportTradeControlOrdinanceAppendixCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new ExportTradeControlOrdinanceAppendixXmlWriter();
	}
}
