namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class IATAAttributeParser : RefCusCodeListAttributeParser
	{
		protected override RefCusCodeListAttributeParserConfig[] GetConfigsCore() => new RefCusCodeListAttributeParserConfig[] { new IATAAttributeParserConfig() };
	}
}
