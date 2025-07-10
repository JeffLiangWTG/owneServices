namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class OtherLawCodeAttributeParser : RefCusCodeListAttributeParser
	{
		protected override RefCusCodeListAttributeParserConfig[] GetConfigsCore() => new RefCusCodeListAttributeParserConfig[] { new OtherLawCodeAttributeParserIsImportConfig(), new OtherLawCodeAttributeParserIsExportConfig(), new OtherLawCodeAttributeParserIsBondedConfig() };
	}
}
