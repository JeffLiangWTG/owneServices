using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Models
{
	public class XmlSchemaEntityRelationshipMap
	{
		public string ParentEntityName { get; private set; }
		public List<string> ChildEntityList { get; set; }

		public XmlSchemaEntityRelationshipMap(string parentEntityName)
		{
			ParentEntityName = parentEntityName;
			ChildEntityList = new List<string>();
		}

		public bool HasChildEntity(string childEntityName)
		{
			return ChildEntityList.Contains(childEntityName);
		}

		public bool HasSchemaCombination(string parentEntityName, string childEntityName = "")
		{
			return (parentEntityName.Equals(ParentEntityName, StringComparison.OrdinalIgnoreCase) && (string.IsNullOrEmpty(childEntityName) || HasChildEntity(childEntityName)));
		}
	}
}
