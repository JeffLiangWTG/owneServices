namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class BeforePermitApplicationReasonCodeXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan Before Permit Application Reason Code";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_BeforePermitApplicationReasonCode";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.BeforePermitApplicationReasonCodeCsvDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new BeforePermitApplicationReasonCodeParser();
	}
}
