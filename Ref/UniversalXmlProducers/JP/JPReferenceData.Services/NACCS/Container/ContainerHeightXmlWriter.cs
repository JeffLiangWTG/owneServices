namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ContainerHeightXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan NACCS Container Height";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_ContainerHeight";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.ContainerLengthAndHeightCsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new ContainerHeightRefCusCodeListParser();
	}
}
