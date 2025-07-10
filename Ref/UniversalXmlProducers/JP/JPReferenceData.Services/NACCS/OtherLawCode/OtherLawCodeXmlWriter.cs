namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class OtherLawCodeXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan Other Law Code";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_OtherLawCode";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.OtherLawCodeCsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new OtherLawCodeParser();
	}
}
