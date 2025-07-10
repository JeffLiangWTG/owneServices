using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Models;
using CargoWise.RefDbRepo.UniversalXmlParser.Resources;

namespace CargoWise.RefDbRepo.UniversalXmlParser
{
	public class UniversalXmlWrapper : IUniversalXmlWrapper
	{
		public UniversalXmlWrapper()
		{
			UnknownElements = new List<string>();
		}

		public DateTime? PublicationTime { get; set; }
		public string DataSource { get; set; }
		public string AppName { get; set; }
		public string ErrorMessage { get; set; }
		public string SchemaXml { get; set; }
		public string Metadata { get; set; }
		public string UpdateType { get; set; }
		public string InclusiveEndDate { get; set; }
		public List<string> UnknownElements { get; private set; }
		public string ErrorXml { get; set; }
		public string DependencyXml { get; set; }
		public List<XmlSchemaEntityRelationshipMap> XmlSchemaEntityRelationshipMapList { get; private set; }

		public void AddUnknownElements(string xmlContent)
		{
			if (!UnknownElements.Contains(xmlContent))
			{
				UnknownElements.Add(xmlContent);
			}
		}

		public bool IsValid(out string errorMessage)
		{
			if (!PublicationTime.HasValue)
			{
				errorMessage = $"{Constants.UniversalXmlMetadata.PublicationTime} element is missing.";
				return false;
			}

			return ValidateSchemaXmlContent(out errorMessage);
		}

#if DEBUG
		public
#endif
		bool ValidateSchemaXmlContent(out string errorMessage)
		{
			errorMessage = string.Empty;
			if (string.IsNullOrEmpty(SchemaXml))
			{
				errorMessage = $"{Constants.UniversalXml.SchemaElement} element is missing.";
				return false;
			}
			return ValidateExpirable(SchemaXml, out errorMessage);
		}

		bool ValidateExpirable(string schemaContentXml, out string errorMessage)
		{
			errorMessage = string.Empty;
			bool result = true;
			XDocument xDocument = null;
			var entityErrorMessages = new List<string>();
			foreach (var item in expirableEntities)
			{
				if (SchemaXml.Replace(" ", "").Contains($"EntityTypeName=\"{item.Name}\""))
				{
					xDocument = xDocument ?? XDocument.Parse(schemaContentXml);
					result &= ValidateExpirableEntity(xDocument, item, out var message);
					entityErrorMessages.Add(message);
				}
			}
			errorMessage = string.Join("\r\n", entityErrorMessages);
			return result;
		}

		//This code will eventually be merged with XMLWriter validation, any change here should be applied in the XmlWriter in the meantime.
		static bool ValidateExpirableEntity(XDocument xDocument, Type entityType, out string errorMessage)
		{
			errorMessage = string.Empty;
			var element = xDocument.Descendants(XName.Get("EntityType"))
			.FirstOrDefault(x => x.Attribute(XName.Get("Name")).Value == entityType.Name);
			if (element == null)
			{
				errorMessage = $"{entityType.Name} element is missing.";
				return false;
			}
			var expirableElement = element.Attribute("EnableExpirable");
			var properties = element.Elements(XName.Get("Property"));
			var tablePrefix = entityType.GetTablePrefix();
			var startDateElement = properties.FirstOrDefault(x => x.Attribute(XName.Get("Name"))?.Value == $"{tablePrefix}_StartDate");
			var endDateElement = properties.FirstOrDefault(x => x.Attribute(XName.Get("Name"))?.Value == $"{tablePrefix}_EndDate");
			if (expirableElement != null && expirableElement.Value.Equals("TRUE", StringComparison.OrdinalIgnoreCase)
				&& (startDateElement == null || endDateElement == null))
			{
				errorMessage = $"{entityType.Name}'s EnableExpirable attribute is set to true and is missing {tablePrefix}_StartDate and/or {tablePrefix}_EndDate element(s).";
				return false;
			}
			else if ((expirableElement == null || expirableElement.Value.Equals("FALSE", StringComparison.OrdinalIgnoreCase))
				&& (startDateElement != null || endDateElement != null))
			{
				errorMessage = $"{entityType.Name}'s EnableExpirable attribute is missing or set to false and must not have {tablePrefix}_StartDate and {tablePrefix}_EndDate elements.";
				return false;
			}
			return true;
		}

		readonly Type[] expirableEntities = { typeof(RefCusCodeListAttribute), typeof(RefCusTariffUOM) };

		public bool HasErrors()
		{
			return !string.IsNullOrEmpty(ErrorMessage);
		}

		public void PopulateXmlSchemaEntityRelationshipMapList()
		{
			XmlSchemaEntityRelationshipMapList = new List<XmlSchemaEntityRelationshipMap>();
			XmlSchemaEntityRelationshipMap currentXmlSchemaEntityRelationshipMap = null;
			string currentEntity = "";
			bool isValidCurrentEntity = false;
			using (var xmlReader = XmlReader.Create(new StringReader(SchemaXml.Replace("\r", "&#xD;"))))
			{
				while (!xmlReader.EOF)
				{
					xmlReader.Read();
					switch (xmlReader.NodeType)
					{
						case XmlNodeType.Element:
							if (xmlReader.Name == "EntityType")
							{
								currentEntity = xmlReader.GetAttribute("Name");
								if (XmlSchemaEntityRelationshipMapList.Count == 0 || XmlSchemaEntityRelationshipMapList.Any(x => x.HasChildEntity(currentEntity)))
								{
									currentXmlSchemaEntityRelationshipMap = new XmlSchemaEntityRelationshipMap(currentEntity);
									XmlSchemaEntityRelationshipMapList.Add(currentXmlSchemaEntityRelationshipMap);
									isValidCurrentEntity = true;
								}
								else
								{
									isValidCurrentEntity = false;
								}
							}
							else if (xmlReader.Name == "Property")
							{
								if (isValidCurrentEntity)
								{
									var name = xmlReader.GetAttribute("Name");
									var type = xmlReader.GetAttribute("Type");
									if (name == type)
									{
										currentXmlSchemaEntityRelationshipMap.ChildEntityList.Add(name);
									}
								}
							}
							break;
						case XmlNodeType.Attribute:
							break;
						default:
							break;
					}
				}
			}
			var parentEntityNameList = XmlSchemaEntityRelationshipMapList.Select(x => x.ParentEntityName);
			foreach (var entityRelationship in XmlSchemaEntityRelationshipMapList)
			{
				var originalList = entityRelationship.ChildEntityList;
				entityRelationship.ChildEntityList = entityRelationship.ChildEntityList.Intersect(parentEntityNameList).ToList();
				var missingEntities = originalList.Except(entityRelationship.ChildEntityList);
				if (missingEntities.Any())
				{
					ErrorMessage = string.Join("\r\n", missingEntities.Select(e => $"EntityType {e} element is missing.")).Trim();
				}
			}
		}
	}
}
