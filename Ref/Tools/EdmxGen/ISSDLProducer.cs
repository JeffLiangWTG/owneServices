using System.Xml.Linq;

namespace CargoWise.RefDbRepo.EdmxGen
{
	public interface ISSDLProducer
	{
		string SSDLFilePath { get; set; }

		XDocument LoadDocument();
		void AddEntitiesToEdmx(XDocument edmxDocument, XDocument xmlChunkDocument, ElementNodeType nodeType);
		void RemoveTablesToIgnoreFromSchema(XDocument document);
		void SetSSDLFile(string[] files);
	}
}
