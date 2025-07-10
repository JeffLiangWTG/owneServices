namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class CertificateOfOriginTypePreferencesRefCusCodeListAttributeParserConfig : RefCusCodeListAttributeParserConfig
	{
		public CertificateOfOriginTypePreferencesRefCusCodeListAttributeParserConfig(string attributeToWrite)
		{
			attribute = attributeToWrite;
		}
		readonly string attribute;

		public override string ZZE_ZXE_NKName => "Preference";
		public override string GetZZE_Value(string[] columns) => attribute;
	}
}
