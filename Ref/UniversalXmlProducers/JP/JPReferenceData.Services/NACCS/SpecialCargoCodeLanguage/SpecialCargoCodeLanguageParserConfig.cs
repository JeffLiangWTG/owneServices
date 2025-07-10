namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	sealed class SpecialCargoCodeLanguageParserConfig : RefCusCodeListLanguageParserConfig
	{
		public SpecialCargoCodeLanguageParserConfig(SpecialCargoCodeLanguageParser parser)
		{
			this.parser = parser;
		}

		readonly SpecialCargoCodeLanguageParser parser;

		public override string GetZXA_Description(string[] columns)
		{
			var code = columns[2];
			return parser.CodeWithEngDescriptionDictionary.TryGetValue(code, out var result) ? result : code;
		}
	}
}
