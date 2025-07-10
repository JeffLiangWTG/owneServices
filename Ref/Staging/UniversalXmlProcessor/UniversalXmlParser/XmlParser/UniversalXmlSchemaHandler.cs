using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Models;
using CargoWise.RefDbRepo.UniversalXmlParser.Resources;

namespace CargoWise.RefDbRepo.UniversalXmlParser
{
	public class UniversalXmlSchemaHandler : IUniversalXmlSchemaHandler
	{
		public List<EntityTypeMetaData> ParseSchemaXmlToEntityTypes(string schemaXml, out XDocument schemaXDocument)
		{
			schemaXDocument = null;

			if (string.IsNullOrEmpty(schemaXml))
			{
				return new List<EntityTypeMetaData>();
			}

			schemaXDocument = XDocument.Parse(schemaXml);

			var result = new List<EntityTypeMetaData>();
			foreach (var element in schemaXDocument.Descendants(Constants.UniversalXmlSchema.EntityTypeElement))
			{
				var nameAttribute = element?.Attribute(Constants.UniversalXmlSchema.EntityTypeNameAttribute);
				if (string.IsNullOrEmpty(nameAttribute?.Value))
				{
					continue;
				}

				var constantValues = new Dictionary<string, string>();
				var defaultValues = new Dictionary<string, string>();

				var properties = element.Descendants("Property");
				foreach (var property in properties)
				{
					var propertyName = property?.Attribute("Name")?.Value;
					if (!string.IsNullOrEmpty(propertyName))
					{
						ExtractPresetValue(property, propertyName, constantValues, "ConstantValue");
						ExtractPresetValue(property, propertyName, defaultValues, "DefaultValue");
					}
				}

				result.Add(new EntityTypeMetaData
				{
					Name = nameAttribute.Value,
					ConstantValues = constantValues,
					DefaultValues = defaultValues
				});
			}

			return result;
		}

		static void ExtractPresetValue(XElement property, string propertyName, IDictionary<string, string> presetValues, string attributeName)
		{
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));
			Argument.NotNull(presetValues, nameof(presetValues));
			Argument.NotNullOrEmpty(attributeName, nameof(attributeName));

			var attributeValue = property?.Attribute(attributeName)?.Value;
			if (!string.IsNullOrEmpty(attributeValue))
			{
				presetValues.Add(propertyName, attributeValue);
			}
		}
	}
}
