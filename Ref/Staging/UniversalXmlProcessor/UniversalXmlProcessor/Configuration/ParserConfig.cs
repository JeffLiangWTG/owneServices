using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;

namespace CargoWise.RefDbRepo.UniversalXmlProcessor.Configuration
{
	public class ParserConfig : IParserConfig
	{
		public int BulkInsertSize { get; set; }
	}
}
