namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	sealed class ConsumptionTaxExemptionAttributeParser : RefCusCodeListAttributeParser
	{
		protected override RefCusCodeListAttributeParserConfig[] GetConfigsCore() => new RefCusCodeListAttributeParserConfig[] { new ConsumptionTaxExemptionAttributeParserConfig() };
	}
}
