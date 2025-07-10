namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class OtherLawCodeAttributeParserIsExportConfig : RefCusCodeListAttributeParserConfig
	{
		public override string ZZE_ZXE_NKName => "IsExport";

		public override string GetZZE_Value(string[] columns) => columns[4] == "○" ? Constants.YesNoList.Yes : Constants.YesNoList.No;
	}
}
