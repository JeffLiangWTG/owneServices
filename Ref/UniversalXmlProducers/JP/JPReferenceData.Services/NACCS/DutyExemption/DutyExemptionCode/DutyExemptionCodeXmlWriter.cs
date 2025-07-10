namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class DutyExemptionCodeXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan Duty Exemption Code";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_DutyExemptionCode";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.DutyExemptionCodeCsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new DutyExemptionCodeParser();
	}
}
