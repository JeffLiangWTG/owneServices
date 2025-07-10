using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using CargoWise.RefDbRepo.UniversalXmlParser.Models;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Interfaces
{
	public interface IUniversalXmlParser
	{
		Task ParseAsync(Stream stream, bool forceParsing);
		XDocument SchemaXDocument { get; }
		IList<EntityTypeMetaData> EntityTypes { get; }
	}
}
