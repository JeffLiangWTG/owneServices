namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ConsumptionTaxExemptionReductionCodeXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan Consumption Tax Exemption Reduction Code";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_ConsumptionTaxExemptionReductionCode";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.ConsumptionTaxExemptionReductionCodeCsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new ConsumptionTaxExemptionReductionCodeDataParser();

		protected override bool IsStartDateUseDefaultValue => false;

		protected override bool IsEndUseDefaultValue => false;
	}
}
