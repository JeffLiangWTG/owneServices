using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.UniversalXmlParser
{
	public class UniversalXmlTransformer : IUniversalXmlTransformer
	{
		readonly IUniversalXmlTransformerValidator universalXmlTransformerValidator;
		XmlDocument xmlDoc;
		XmlNode schemaNode;

		public UniversalXmlTransformer(IUniversalXmlTransformerValidator universalXmlTransformerValidator)
		{
			this.universalXmlTransformerValidator = universalXmlTransformerValidator;
		}

		public string Transform(string xml)
		{
			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);
			var transformers = new List<IUniversalXmlNodeTransformer>();
			var transformMode = universalXmlTransformerValidator.GetTransformMode(xmlDoc);
			if (transformMode == TransformMode.None)
			{
				return xml;
			}
			if (transformMode.HasFlag(TransformMode.Transform_RateAndApp))
			{
				transformers.Add(new UniversalXmlNodeTransformer(SchemaMapperHelper.GetSchemaMappers_RateAndApplicabilityGroup()));
			}
			if (transformMode.HasFlag(TransformMode.Transform_CondAndApp))
			{
				transformers.Add(new UniversalXmlNodeTransformer(SchemaMapperHelper.GetSchemaMappers_ConditionAndApplicabilityGroup()));
			}
			if (transformers.Count > 0)
			{
				return TransformCore(transformers, transformMode);
			}
			return xml;
		}

		string TransformCore(List<IUniversalXmlNodeTransformer> transformers, TransformMode transformMode)
		{
			schemaNode = xmlDoc.SelectSingleNode("//Schema");
			var originalSchemaNode = xmlDoc.CreateElement("OriginalSchema");
			originalSchemaNode.InnerXml = schemaNode.InnerXml;
			schemaNode.ParentNode.InsertAfter(originalSchemaNode, schemaNode);

			foreach (var transformer in transformers)
			{
				transformer.Transform(schemaNode);
			}

			CleanOriginalXmlNodeInSchemaIfNeeded(transformers, transformMode);
			RelinkEntities(transformMode);
			AddEntityWithoutApplicabilityNodes(transformMode, originalSchemaNode);
			return XElement.Parse(xmlDoc.OuterXml).ToString(SaveOptions.DisableFormatting);
		}

		void CleanOriginalXmlNodeInSchemaIfNeeded(List<IUniversalXmlNodeTransformer> transformers, TransformMode transformMode)
		{
			foreach (var transformer in transformers)
			{
				foreach (var schemaMapper in transformer.SchemaMappers)
				{
					var originalEntityTypeNode = schemaNode.SelectSingleNode($"EntityType[@Name='{schemaMapper.NameMapper.originalName}']");
					if (originalEntityTypeNode != null &&
						(!transformMode.HasFlag(TransformMode.Transform_KeepApp) || !(schemaMapper.NameMapper.originalName == nameof(RefCusApplicability) || schemaMapper.NameMapper.originalName == nameof(RefCusExcludedTradeGroup))))
					{
						originalEntityTypeNode.ParentNode.RemoveChild(originalEntityTypeNode);
					}
				}
			}
		}

		void RelinkEntities(TransformMode transformMode)
		{
			var relinkEntitiesList = new List<(string originalClassName, string[] transformedClassNames)>();
			if (transformMode.HasFlag(TransformMode.Transform_RateAndApp))
			{
				relinkEntitiesList.Add((nameof(RefCusRate), [nameof(RefCusRateApplicability), nameof(RefCusRateWithoutApplicability)]));
			}
			if (transformMode.HasFlag(TransformMode.Transform_CondAndApp))
			{
				relinkEntitiesList.Add((nameof(RefCusCondition), [nameof(RefCusConditionApplicability), nameof(RefCusConditionWithoutApplicability)]));
			}
			foreach (var item in relinkEntitiesList)
			{
				var propertyRefsAndProperties = schemaNode.SelectNodes($"EntityType/Key/PropertyRef[@Name='{item.originalClassName}'] | EntityType/Property[@Name='{item.originalClassName}']");
				foreach (XmlNode node in propertyRefsAndProperties)
				{
					Array.ForEach(item.transformedClassNames, transformedName =>
					{
						var transformedNode = node.CloneNode(true);
						node.ParentNode.InsertAfter(transformedNode, node);
						transformedNode.Attributes["Name"].Value = transformedName;
						if (transformedNode.Attributes["Type"] != null)
						{
							transformedNode.Attributes["Type"].Value = transformedName;
						}
					});
					node.ParentNode.RemoveChild(node);
				}
			}
		}

		void AddEntityWithoutApplicabilityNodes(TransformMode transformMode, XmlNode originalSchemaNode)
		{
			if (transformMode.HasFlag(TransformMode.Transform_RateAndApp))
			{
				AddEntityWithoutApplicabilityNodesCore(originalSchemaNode, SchemaMapperHelper.RateWithoutApplicabilityNameMapper);
			}
			if (transformMode.HasFlag(TransformMode.Transform_CondAndApp))
			{
				AddEntityWithoutApplicabilityNodesCore(originalSchemaNode, SchemaMapperHelper.ConditionWithoutApplicabilityNameMapper);
			}
		}

		void AddEntityWithoutApplicabilityNodesCore(XmlNode originalSchemaNode, (string originalName, string transformedName) mappedSchema)
		{
			var entityNode = originalSchemaNode.SelectSingleNode($"EntityType[@Name='{mappedSchema.originalName}']");
			var transformedEntityNode = entityNode.CloneNode(true);
			transformedEntityNode.Attributes["Name"].Value = mappedSchema.transformedName;
			var applicabilityPropertyRefNode = transformedEntityNode.SelectSingleNode($"Key/PropertyRef[@Name='{nameof(RefCusApplicability)}']");
			if (applicabilityPropertyRefNode != null)
			{
				transformedEntityNode.FirstChild.RemoveChild(applicabilityPropertyRefNode);
			}
			var applicabilityPropertyNode = transformedEntityNode.SelectSingleNode($"Property[@Name='{nameof(RefCusApplicability)}']");
			if (applicabilityPropertyNode != null)
			{
				transformedEntityNode.RemoveChild(applicabilityPropertyNode);
			}
			schemaNode.AppendChild(transformedEntityNode);

			AddRelatedChildNodesWithoutApplicability(originalSchemaNode, transformedEntityNode);
		}

		void AddRelatedChildNodesWithoutApplicability(XmlNode originalSchemaNode, XmlNode parentEntityNode)
		{
			if (originalSchemaNode == null || parentEntityNode == null)
			{
				return;
			}

			foreach (XmlNode childEntityNode in parentEntityNode.SelectNodes("Property"))
			{
				var childEntityName = childEntityNode.Attributes["Name"].Value;
				var childEntityType = childEntityNode.Attributes["Type"].Value;
				if (childEntityName == nameof(RefCusApplicability))
				{
					continue;
				}
				if (childEntityName.Contains('_') || childEntityName != childEntityType)
				{
					continue;
				}

				var entityNode = originalSchemaNode.SelectSingleNode($"EntityType[@Name='{childEntityName}']");
				if (entityNode != null)
				{
					var clonedEntityNode = entityNode.CloneNode(true);
					schemaNode.AppendChild(clonedEntityNode);
					AddRelatedChildNodesWithoutApplicability(originalSchemaNode, clonedEntityNode);
				}
			}
		}
	}
}
