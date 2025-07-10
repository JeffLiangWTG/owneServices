namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class PackageTypeLanguageParserConfig : RefCusCodeListLanguageParserConfig
	{
		public override string GetZXA_Description(string[] columns) => columns[2];
	}
}
