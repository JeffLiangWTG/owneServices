namespace CargoWise.RefDbRepo.JPReferenceData.Services;

public class UNLOCOOpenPortFlagAttributeParserConfig : RefCusCodeListAttributeParserConfig
{
	public override string ZZE_ZXE_NKName => "OpenPortFlag";

	public override string GetZZE_Value(string[] columns)
	{
		return columns[8].Trim() switch
		{
			"◎" => "開港（外航船舶用）",
			"○" => "不開港（外航船舶用港コードとしてシステム設定済）",
			_ => "内航船舶用コード（システム上外航船舶用としては未設定）"
		};
	}
}
