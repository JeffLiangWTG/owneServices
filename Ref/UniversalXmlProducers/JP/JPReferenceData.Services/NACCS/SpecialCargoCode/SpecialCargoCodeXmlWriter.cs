namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class SpecialCargoCodeXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Special Cargo Code";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_SpecialCargoCode";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.SpecialCargoCodeCsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new SpecialCargoCodeParser();
	}
}
