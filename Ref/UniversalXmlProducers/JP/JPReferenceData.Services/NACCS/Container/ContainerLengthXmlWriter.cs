namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ContainerLengthXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan NACCS Container Length";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_ContainerLength";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.ContainerLengthAndHeightCsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new ContainerLengthRefCusCodeListParser();
	}
}
