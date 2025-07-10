namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class MSXDocumentTypeXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan Document Types";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_MSXDocumentType";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.MSXDocumentTypeCsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new MSXDocumentTypeRefCusCodeListParser();
	}
}
