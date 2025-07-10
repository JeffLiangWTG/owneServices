namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class OtherLawCodeAttributeParserIsBondedConfig : RefCusCodeListAttributeParserConfig
	{
		public override string ZZE_ZXE_NKName => "IsBonded";

		public override string GetZZE_Value(string[] columns) => columns[5] == "○" ? Constants.YesNoList.Yes : Constants.YesNoList.No;
	}
}
