namespace CargoWise.RefDbRepo.JPReferenceData.Services;

public class UNLOCOLanguageParserConfig : RefCusCodeListLanguageParserConfig
{
	public override string GetZXA_Description(string[] columns) => columns[7];
}
