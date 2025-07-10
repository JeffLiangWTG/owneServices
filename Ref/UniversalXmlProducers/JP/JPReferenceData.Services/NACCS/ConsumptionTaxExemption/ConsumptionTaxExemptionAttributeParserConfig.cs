namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ConsumptionTaxExemptionAttributeParserConfig : RefCusCodeListAttributeParserConfig
	{
		public override string ZZE_ZXE_NKName => "DetailedDescription";

		public override string GetZZE_Value(string[] columns) => columns[2];
	}
}
