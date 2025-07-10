using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.UniversalXmlParser.Models;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Interfaces
{
	public interface IUniversalXmlSchemaHandler
	{
		List<EntityTypeMetaData> ParseSchemaXmlToEntityTypes(string schemaXml, out XDocument schemaXDocument);
	}
}
