namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class DutyExemptionRefundCodeXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan Duty Exemption Refund Code";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_DutyExemptionRefundCode";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.DutyExemptionRefundCodeCsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new DutyExemptionRefundCodeParser();
	}
}
