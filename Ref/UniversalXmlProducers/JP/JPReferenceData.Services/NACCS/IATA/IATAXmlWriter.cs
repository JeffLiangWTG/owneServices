namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class IATAXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan IATA Code";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_IATACode";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.UNLOCOAndIATACsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new IATARefCusCodeListParser();
	}
}
