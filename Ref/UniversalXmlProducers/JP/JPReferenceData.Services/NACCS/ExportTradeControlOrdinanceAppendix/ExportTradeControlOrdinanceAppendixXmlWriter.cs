namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ExportTradeControlOrdinanceAppendixXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan Export Trade Control Ordinance Appendix";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_ExportTradeControlOrdinanceAppendix";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.ExportTradeControlOrdinanceAppendixCsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new ExportTradeControlOrdinanceAppendixParser();
	}
}
