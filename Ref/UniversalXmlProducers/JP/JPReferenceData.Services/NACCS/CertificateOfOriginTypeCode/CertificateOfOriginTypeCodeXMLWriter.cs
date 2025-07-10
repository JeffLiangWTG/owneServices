namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class CertificateOfOriginTypeCodeXMLWriter : NaccsXmlWriter
	{
		protected override string DataSource => "JP NACCS Certificate Of Origin Type Code";

		protected override string FileNameWithoutExtension => "CertificateOfOriginTypeCode";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.CertificateOfOriginTypeCodeFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new CertificateOfOriginTypeCodeDataParser();

		protected override bool IsCodeTypeConstant => false;

		protected override bool ZZE_ValueIsKey => true;
	}
}
