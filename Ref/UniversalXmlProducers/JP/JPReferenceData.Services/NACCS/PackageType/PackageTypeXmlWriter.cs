namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class PackageTypeXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan Package Types";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_PackageType";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.PackageTypeCsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new PackageTypeRefCusCodeListParser();
	}
}
