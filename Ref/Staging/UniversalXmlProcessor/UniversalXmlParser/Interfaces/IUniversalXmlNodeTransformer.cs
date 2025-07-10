using System.Xml;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.UniversalXmlParser
{
	public interface IUniversalXmlNodeTransformer
	{
		ISchemaMapper[] SchemaMappers { get; }
		void Transform(XmlNode xmlNode);
	}
}
