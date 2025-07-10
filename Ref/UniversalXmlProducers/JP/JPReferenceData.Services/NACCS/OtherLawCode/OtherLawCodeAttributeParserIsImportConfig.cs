namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class OtherLawCodeAttributeParserIsImportConfig : RefCusCodeListAttributeParserConfig
	{
		public override string ZZE_ZXE_NKName => "IsImport";
		public override string GetZZE_Value(string[] columns) => columns[3] == "○" ? Constants.YesNoList.Yes : Constants.YesNoList.No;
	}
}
