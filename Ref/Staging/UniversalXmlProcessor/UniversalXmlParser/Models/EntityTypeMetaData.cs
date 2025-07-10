using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Models
{
	public class EntityTypeMetaData
	{
		public string  Name { get; set; }
		public IDictionary<string, string> ConstantValues { get; set; }
		public IDictionary<string, string> DefaultValues { get; set; }
	}
}
