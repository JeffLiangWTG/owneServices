namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class DutyExemptionAttributeParserConfig : RefCusCodeListAttributeParserConfig
	{
		public const int AttributeValueIndex = 3;

		public override string ZZE_ZXE_NKName => "DetailedDescription";

		public override string GetZZE_Value(string[] columns) => columns[AttributeValueIndex];
	}
}
