namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ContainerTypeXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan NACCS Container Type";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_ContainerType";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.ContainerTypeCsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new ContainerTypeRefCusCodeListParser();
	}
}
