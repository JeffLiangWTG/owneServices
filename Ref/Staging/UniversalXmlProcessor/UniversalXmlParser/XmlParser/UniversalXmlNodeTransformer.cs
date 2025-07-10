using System.Linq;
using System.Xml;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.UniversalXmlParser
{
	public class UniversalXmlNodeTransformer : IUniversalXmlNodeTransformer
	{
		public UniversalXmlNodeTransformer(ISchemaMapper[] schemaMappers)
		{
			SchemaMappers = schemaMappers;
		}

		public ISchemaMapper[] SchemaMappers { get; }

		public void Transform(XmlNode xmlNode)
		{
			foreach (var schemaMapper in SchemaMappers)
			{
				var originalEntityTypeNode = xmlNode.SelectSingleNode($"EntityType[@Name='{schemaMapper.NameMapper.originalName}']");
				if (originalEntityTypeNode == null)
				{
					continue;
				}
				var transformedEntityTypeNode = xmlNode.SelectSingleNode($"EntityType[@Name='{schemaMapper.NameMapper.transformedName}']");
				transformedEntityTypeNode = transformedEntityTypeNode == null
										? CreateTransformEntityTypeNode(schemaMapper, originalEntityTypeNode)
										: CombineEntityTypeNode(originalEntityTypeNode, transformedEntityTypeNode);
				TransformEntityTypeNode(schemaMapper, transformedEntityTypeNode);
			}
		}

		static XmlNode CreateTransformEntityTypeNode(ISchemaMapper schemaMapper, XmlNode originalEntityTypeNode)
		{
			var transformedEntityTypeNode = originalEntityTypeNode.CloneNode(true);
			originalEntityTypeNode.ParentNode.InsertAfter(transformedEntityTypeNode, originalEntityTypeNode);

			transformedEntityTypeNode.Attributes["Name"].Value = schemaMapper.NameMapper.transformedName;
			var parentNode = originalEntityTypeNode.ParentNode;
			parentNode.InsertAfter(transformedEntityTypeNode, originalEntityTypeNode);

			return transformedEntityTypeNode;
		}

		static XmlNode CombineEntityTypeNode(XmlNode originalEntityTypeNode, XmlNode transformedEntityTypeNode)
		{
			var allLeafNodes = originalEntityTypeNode.SelectNodes("Property | Key/PropertyRef");
			foreach (XmlNode childNode in allLeafNodes)
			{
				var parentNode = childNode.Name == "Property" ? transformedEntityTypeNode : transformedEntityTypeNode.SelectSingleNode("Key");
				parentNode?.AppendChild(childNode.Clone());
			}
			return transformedEntityTypeNode;
		}

		void TransformEntityTypeNode(ISchemaMapper schemaMapper, XmlNode transformedEntityTypeNode)
		{
			var entityTypeName = transformedEntityTypeNode.Attributes["Name"].Value;
			var allLeafNodes = transformedEntityTypeNode.SelectNodes("Property | Key/PropertyRef");

			for (int i = allLeafNodes.Count - 1; i >= 0; i--)
			{
				var childNode = allLeafNodes[i];
				TransformPropertyOrPropertyRef(schemaMapper, childNode, entityTypeName);
			}
		}

		void TransformPropertyOrPropertyRef(ISchemaMapper schemaMapper, XmlNode xmlNode, string entityTypeName)
		{
			var originalName = xmlNode.Attributes["Name"].Value;
			var transformedProperty = schemaMapper.PropertyMappers.SingleOrDefault(x => x.originalProperty == originalName).transformedProperty;
			bool existTransformedProperty = xmlNode.ParentNode.SelectSingleNode($"Property[@Name='{transformedProperty}'] | PropertyRef[@Name='{transformedProperty}']") != null;

			var relatedSchemaMapper = SchemaMappers.FirstOrDefault(o => o.NameMapper.originalName == originalName);
			transformedProperty = transformedProperty ??
								((relatedSchemaMapper != null
									&& relatedSchemaMapper.NameMapper.transformedName != entityTypeName
									&& schemaMapper.NameMapper.originalName != originalName)
								? relatedSchemaMapper.NameMapper.transformedName
								: null);
			if (!string.IsNullOrEmpty(transformedProperty) && !existTransformedProperty)
			{
				xmlNode.Attributes["Name"].Value = transformedProperty;
				if (xmlNode.Attributes["Type"] != null && xmlNode.Attributes["Type"].Value == originalName)
				{
					xmlNode.Attributes["Type"].Value = transformedProperty;
				}
			}
			else if (relatedSchemaMapper != null || existTransformedProperty)
			{
				xmlNode.ParentNode.RemoveChild(xmlNode);
			}
		}
	}
}
