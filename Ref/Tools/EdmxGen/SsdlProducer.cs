using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.EdmxGen
{
	public class SsdlProducer : ISSDLProducer
	{
		public string SSDLFilePath { get; set; }
		readonly string[] _tablesToIgnore;
		readonly ITablesConfig _tablesConfig;

		public SsdlProducer(ITablesConfig yamlHelper, string[] tablesToIgnore)
		{
			Argument.NotNull(tablesToIgnore, nameof(tablesToIgnore));
			Argument.GreaterThan(tablesToIgnore.Length, 0, nameof(tablesToIgnore.Length));

			Argument.NotNull(yamlHelper, nameof(yamlHelper));

			_tablesToIgnore = tablesToIgnore;
			_tablesConfig = yamlHelper;
		}

		public XDocument LoadDocument()
		{
			Argument.NotNullOrEmpty(SSDLFilePath, nameof(SSDLFilePath));
			var document = XDocument.Load(SSDLFilePath);
			RemoveTablesToIgnoreFromSchema(document);
			RemoveSpecialAssociations(document);
			ConfigureTables(document);
			return document;
		}

		static void CreateAssociation(IEnumerable<XElement> associations, ILogicalRelationship logicalRelationship)
		{
			Argument.NotNull(associations, nameof(associations));
			Argument.NotNull(logicalRelationship, nameof(logicalRelationship));
			var firstAssociation = associations.FirstOrDefault();
			if (firstAssociation != null)
			{
				var firstElementWithData = firstAssociation.Elements().FirstOrDefault();

				var firstAssociationAssociation = firstElementWithData.Attribute(XName.Get("Type"))?.Value?.Replace(firstElementWithData.FirstAttribute.Value, "");
				var firstAssociationNamespace = firstElementWithData.GetDefaultNamespace();
				var newAssociation = new XElement(firstAssociationNamespace + Constants.Elements.Association,
											 new XAttribute("Name", logicalRelationship.GetAssociationName()),
										 new XElement(firstAssociationNamespace + "End",
											 new XAttribute("Role", logicalRelationship.Table),
											 new XAttribute("Type", firstAssociationAssociation + logicalRelationship.Table),
											 new XAttribute("Multiplicity", logicalRelationship.IsNullable ? "0..1" : "1")),
										 new XElement(firstAssociationNamespace + "End",
											 new XAttribute("Role", logicalRelationship.ReferencedTable),
											 new XAttribute("Type", firstAssociationAssociation + logicalRelationship.ReferencedTable),
											 new XAttribute("Multiplicity", "*")),
										 new XElement(firstAssociationNamespace + Constants.Elements.ReferentialConstraint,
												 new XElement(firstAssociationNamespace + Constants.Elements.Principal,
													 new XAttribute("Role", logicalRelationship.Table),
													 new XElement(firstAssociationNamespace + Constants.Elements.PropertyRef,
														 new XAttribute("Name", logicalRelationship.Column ?? ""))),
												 new XElement(firstAssociationNamespace + Constants.Elements.Dependent,
													 new XAttribute("Role", logicalRelationship.ReferencedTable),
													 new XElement(firstAssociationNamespace + Constants.Elements.PropertyRef,
														 new XAttribute("Name", logicalRelationship.ReferencedColumn)))));

				firstAssociation.AddBeforeSelf(newAssociation);
			}
		}

		static void CreateAssociationSet(IEnumerable<XElement> associationSets, ILogicalRelationship logicalRelationship)
		{
			Argument.NotNull(associationSets, nameof(associationSets));
			Argument.NotNull(logicalRelationship, nameof(logicalRelationship));

			var firstAssociationSet = associationSets.FirstOrDefault();
			if (firstAssociationSet != null)
			{
				var firstAssociationAssociation = firstAssociationSet.LastAttribute.Value.Replace(firstAssociationSet.FirstAttribute.Value, "");
				var firstAssociationNamespace = firstAssociationSet.GetDefaultNamespace();

				var newAssociationSet = new XElement(firstAssociationNamespace + Constants.Elements.AssociationSet,
												new XAttribute("Name", logicalRelationship.GetAssociationName()),
												new XAttribute("Association", firstAssociationAssociation + logicalRelationship.GetAssociationName()),
											new XElement(firstAssociationNamespace + "End",
												new XAttribute("Role", logicalRelationship.Table),
												new XAttribute("EntitySet", logicalRelationship.Table)),
											new XElement(firstAssociationNamespace + "End",
												new XAttribute("Role", logicalRelationship.ReferencedTable),
												new XAttribute("EntitySet", logicalRelationship.ReferencedTable)));

				firstAssociationSet.AddBeforeSelf(newAssociationSet);
			}
		}

		static void RemoveSpecialAssociations(XDocument document)
		{
			Argument.NotNull(document, nameof(document));
			var rootElement = GetRootElement(document);
			var entityContainer = rootElement.Elements().FirstOrDefault();

			foreach (var association in IgnoredTables.StagingAssociations)
			{
				entityContainer.Elements().FirstOrDefault(o => o.FirstAttribute.Value == association)?.Remove();
				rootElement.Elements().FirstOrDefault(o => o.Name.LocalName == Constants.Elements.Association && o.FirstAttribute.Value == association)?.Remove();
			}
		}

		void ConfigureTables(XDocument document)
		{
			Argument.NotNull(document, nameof(document));

			var rootElement = GetRootElement(document);
			var tblConfigs = _tablesConfig.GetTablesConfigurations();
			foreach (var tblConfig in tblConfigs)
			{
				var tblElement = rootElement.Elements().FirstOrDefault(o => o.Name.LocalName == Constants.Elements.EntityType && o.FirstAttribute.Value == tblConfig.Key);

				if (tblElement == null)
				{
					continue;
				}

				foreach (var clnConfig in tblConfig.Value)
				{
					if (clnConfig.Configuration != null)
					{
						var clnElement = tblElement.Elements().FirstOrDefault(o => o.Name.LocalName == Constants.Elements.Property && o.FirstAttribute.Value == clnConfig.Column);
						if (clnElement == null)
						{
							continue;
						}
						var config = clnConfig.GetConfigurationValue();
						if (config == null)
						{
							continue;
						}
						clnElement.SetAttributeValue(config.Item1, config.Item2);
					}

					if (clnConfig.LogicalRelationships != null)
					{
						foreach (var logicalRelationship in clnConfig.LogicalRelationships)
						{
							var parentTableElement = rootElement.Elements().FirstOrDefault(x => x.Name.LocalName == Constants.Elements.EntityType && x.FirstAttribute.Value == logicalRelationship.Table);
							if (parentTableElement != null)
							{
								var entityContainer = rootElement.Elements().FirstOrDefault();

								var pkColumn = parentTableElement.Elements().FirstOrDefault(x => x.Name.LocalName == Constants.Elements.Property && x.FirstAttribute.Value.EndsWith("PK", System.StringComparison.CurrentCultureIgnoreCase));
								logicalRelationship.Column = pkColumn.FirstAttribute.Value;

								var associationSets = entityContainer.Elements().Where(x => x.Name.LocalName == Constants.Elements.AssociationSet);
								if (associationSets.Any(x => x.FirstAttribute.Value == logicalRelationship.GetAssociationName()))
								{
									continue;
								}

								CreateAssociationSet(associationSets, logicalRelationship);

								var associations = rootElement.Elements().Where(x => x.Name.LocalName == Constants.Elements.Association);
								if (associations.Any(x => x.FirstAttribute.Value == logicalRelationship.GetAssociationName()))
								{
									continue;
								}

								CreateAssociation(associations, logicalRelationship);
							}
						}
					}
				}
			}
		}

		static XElement GetRootElement(XDocument document)
		{
			Argument.NotNull(document, nameof(document));

			return document.Elements().FirstOrDefault();
		}

		public void RemoveTablesToIgnoreFromSchema(XDocument document)
		{
			Argument.NotNull(document, nameof(document));
			var rootElement = GetRootElement(document);
			var entityContainer = rootElement.Elements().FirstOrDefault();

			foreach (var tbl in _tablesToIgnore)
			{
				entityContainer.Elements().FirstOrDefault(o => o.FirstAttribute.Value == tbl)?.Remove();
				rootElement.Elements().FirstOrDefault(o => o.Name.LocalName == Constants.Elements.EntityType && o.FirstAttribute.Value == tbl)?.Remove();
			}
		}

		public void AddEntitiesToEdmx(XDocument edmxDocument, XDocument xmlChunkDocument, ElementNodeType nodeType)
		{
			var rootElement = GetRootElement(xmlChunkDocument);
			EdmxHelper.InsertContentInDocumentElement(edmxDocument, nodeType, rootElement);
		}

		public void SetSSDLFile(string[] files)
		{
			Argument.NotNull(files, nameof(files));
			SSDLFilePath = files.FirstOrDefault(o => o.EndsWith(Constants.Extensions.Storage, System.StringComparison.OrdinalIgnoreCase));
		}
	}
}
