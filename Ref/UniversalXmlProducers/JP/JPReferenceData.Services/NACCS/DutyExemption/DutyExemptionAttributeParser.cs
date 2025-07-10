namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class DutyExemptionAttributeParser : RefCusCodeListAttributeParser
	{
		protected override RefCusCodeListAttributeParserConfig[] GetConfigsCore() => new RefCusCodeListAttributeParserConfig[] { new DutyExemptionAttributeParserConfig() };

	}
}
