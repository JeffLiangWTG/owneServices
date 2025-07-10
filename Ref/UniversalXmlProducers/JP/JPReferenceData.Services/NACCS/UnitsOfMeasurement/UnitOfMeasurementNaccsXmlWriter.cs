namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class UnitOfMeasurementNaccsXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan Units of Measurement";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_UnitOfMeasurement";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.UnitOfMeasurementCsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new UnitOfMeasurementRefCusCodeListParser();
	}
}
