namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ImportApprovalCertificateNumberXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Import Approval Certificate Number";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_ImportApprovalCertificateNumber";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.ImportApprovalCertificateNumberFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new ImportApprovalCertificateNumberParser();

		protected override bool ZZE_ValueIsKey => true;

		protected override bool IsCodeTypeConstant => false;
	}
}
