namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ImportTradeControlOrdinanceAppendixXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan Import Trade Control Ordiance Appendix";
		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_ImportTradeControlOrdinanceAppendix";
		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.ImportTradeControlOrdinanceAppendixCsvFileDownloadUrl;
		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new ImportTradeControlOrdinanceAppendixRefCusCodeListParser();
	}
}
