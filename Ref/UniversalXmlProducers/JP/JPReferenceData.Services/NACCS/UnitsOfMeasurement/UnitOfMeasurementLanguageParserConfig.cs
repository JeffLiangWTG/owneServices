namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class UnitOfMeasurementLanguageParserConfig : RefCusCodeListLanguageParserConfig
	{
		public override string GetZXA_Description(string[] columns) => columns[4];
	}
}
