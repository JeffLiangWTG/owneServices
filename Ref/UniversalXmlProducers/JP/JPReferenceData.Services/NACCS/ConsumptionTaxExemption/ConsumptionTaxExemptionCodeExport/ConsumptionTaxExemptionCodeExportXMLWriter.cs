namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ConsumptionTaxExemptionCodeExportXMLWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan Consumption Tax Exemption Code (Export)";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_ConsumptionTaxExemptionCodeExport";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.ConsumptionTaxExemptionCodeExportFileDownloadParentUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new ConsumptionTaxExemptionCodeExportDataParser();

		protected override bool IsStartDateUseDefaultValue => false;

		protected override bool IsEndUseDefaultValue => false;
	}
}
