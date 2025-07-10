namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class PackageTypeAttributeParser : RefCusCodeListAttributeParser
	{
		protected override RefCusCodeListAttributeParserConfig[] GetConfigsCore() => new RefCusCodeListAttributeParserConfig[] { new PackageTypeAttributeParserConfig() };
	}
}
