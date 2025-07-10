namespace CargoWise.RefDbRepo.JPReferenceData.Services;

public class UNLOCOAttributeParser : RefCusCodeListAttributeParser
{
	protected override RefCusCodeListAttributeParserConfig[] GetConfigsCore() => [new UNLOCOPrefectureAttributeParserConfig(), new UNLOCOOpenPortFlagAttributeParserConfig(), new UNLOCOIATAAttributeParserConfig()];
}
