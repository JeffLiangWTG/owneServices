namespace CargoWise.RefDbRepo.JPReferenceData.Services;

public class UNLOCOXmlWriter : NaccsXmlWriter
{
	protected override string DataSource => "Japan UNLOCO";

	protected override string FileNameWithoutExtension => "RefCusCodeList_JP_UNLOCO";

	protected override string DownloadUrl => AppConfig.NACCS.CodeLists.UNLOCOAndIATACsvFileDownloadUrl;

	protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new UNLOCORefCusCodeListParser();
}
