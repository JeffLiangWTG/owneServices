namespace CargoWise.RefDbRepo.JPReferenceData.Services;

public class UNLOCOIATAAttributeParserConfig : RefCusCodeListAttributeParserConfig
{
	public override string ZZE_ZXE_NKName => "IATA";

	public override string GetZZE_Value(string[] columns) => columns[9];

	public override bool NeedAttributeInThisRow(string[] columns) => !string.IsNullOrEmpty(columns[9]);
}
