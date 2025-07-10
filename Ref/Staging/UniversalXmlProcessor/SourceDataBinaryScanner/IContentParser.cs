using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;

namespace CargoWise.RefDbRepo.SourceDataBinaryScanner
{
	public interface IContentParser
	{
		Task CompressedContent(SourceData sourceData, IUniversalXmlParser uxmlParser, IStagingRepository stagingRepository);
		Task XmlContent(SourceData sourceData, IUniversalXmlParser uxmlParser, IStagingRepository stagingRepository);
	}
}
