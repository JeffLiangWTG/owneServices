using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;

namespace CargoWise.RefDbRepo.SourceDataBinaryScanner
{
	public class FileParserConfig : IParserConfig
	{
		public int BulkInsertSize { get; set; }
	}
}
