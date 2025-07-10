namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class PackageTypeLanguageParser : RefCusCodeListLanguageParser
	{
		public override string ZXA_ZX6_NKLanguage => "JP";
		protected override RefCusCodeListLanguageParserConfig[] GetConfigsCore() => new RefCusCodeListLanguageParserConfig[] { new PackageTypeLanguageParserConfig() };
	}
}
