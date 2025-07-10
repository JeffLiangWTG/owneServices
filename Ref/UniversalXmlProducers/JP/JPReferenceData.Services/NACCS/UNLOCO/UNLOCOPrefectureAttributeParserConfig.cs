namespace CargoWise.RefDbRepo.JPReferenceData.Services;

public class UNLOCOPrefectureAttributeParserConfig : RefCusCodeListAttributeParserConfig
{
	public override string ZZE_ZXE_NKName => "Prefecture";

	public override bool NeedAttributeInThisRow(string[] columns) => !string.IsNullOrEmpty(columns[6]);

	public override string GetZZE_Value(string[] columns) => columns[6];
}
