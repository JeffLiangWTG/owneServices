using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.EdmxGen
{
	public static class EdmxHelper
	{
		public static XDocument LoadDocument(string path)
		{
			Argument.NotNullOrEmpty(path, nameof(path));
			using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				return LoadDocument(file, path);
			}
		}

		public static XDocument LoadDocument(Stream fileStream, string path)
		{
			Argument.NotNull(fileStream, nameof(fileStream));

			var document = XDocument.Load(fileStream);

			if (document == null)
			{
				throw new FileNotFoundException($"File in specified path: {path} was not found");
			}

			var rootElement = GetEdmxRootElement(document);

			var storageModelsNode = GetStorageModels(rootElement);
			if (storageModelsNode == null)
			{
				throw new ArgumentException($"StorageModel node was not found");
			}
			var conceptualNode = GetConceptualModels(rootElement);
			if (conceptualNode == null)
			{
				throw new ArgumentException($"Conceptual node was not found");
			}
			var mappingsNode = GetMappings(rootElement);
			if (mappingsNode == null)
			{
				throw new ArgumentException($"Mappings node was not found");
			}

			RemoveContentFromElement(storageModelsNode);
			RemoveContentFromElement(conceptualNode);
			RemoveContentFromElement(mappingsNode);

			return document;
		}

		public static XElement GetEdmxRootElement(XDocument document)
		{
			Argument.NotNull(document, nameof(document));

			return document.Root.Elements().First();
		}

		public static XDocument GetMergeableDocument(XDocument document, ElementNodeType elementNodeType)
		{
			Argument.NotNull(document, nameof(document));

			var rootElement = GetEdmxRootElement(document);
			if (rootElement == null)
			{
				throw new ArgumentException("Wrong merge document. This document has no root or it is not an edmx.");
			}

			switch (elementNodeType)
			{
				case ElementNodeType.StorageModels:
					return new XDocument(GetStorageModels(rootElement)?.Nodes());
				case ElementNodeType.ConceptualModels:
					return new XDocument(GetConceptualModels(rootElement)?.Nodes());
				case ElementNodeType.Mappings:
					return new XDocument(GetMappings(rootElement)?.Nodes());
			}
			return null;
		}

		public static XElement GetStorageModels(XElement rootElement)
		{
			Argument.NotNull(rootElement, nameof(rootElement));

			return rootElement.Elements().FirstOrDefault(x => x.Name.LocalName == Constants.EdmxHeaders.StorageModels);
		}

		public static XElement GetConceptualModels(XElement rootElement)
		{
			Argument.NotNull(rootElement, nameof(rootElement));

			return rootElement.Elements().FirstOrDefault(x => x.Name.LocalName == Constants.EdmxHeaders.ConceptualModels);
		}

		public static XElement GetMappings(XElement rootElement)
		{
			Argument.NotNull(rootElement, nameof(rootElement));

			return rootElement.Elements().FirstOrDefault(x => x.Name.LocalName == Constants.EdmxHeaders.Mappings);
		}

		static void RemoveContentFromElement(XElement element)
		{
			Argument.NotNull(element, nameof(element));

			element.Elements().Remove();
		}

		public static void InsertContentInDocumentElement(XDocument document, ElementNodeType elementType, XElement content)
		{
			Argument.NotNull(document, nameof(document));
			Argument.NotNull(content, nameof(content));

			if (content.Name.LocalName != Constants.Elements.Schema && content.Name.LocalName != Constants.Elements.Mapping)
			{
				throw new ArgumentException("Wrong content");
			}

			var rootElement = GetEdmxRootElement(document);
			XElement elementToAdd = null;

			switch (elementType)
			{
				case ElementNodeType.StorageModels:
					elementToAdd = GetStorageModels(rootElement);
					break;
				case ElementNodeType.ConceptualModels:
					elementToAdd = GetConceptualModels(rootElement);
					break;
				case ElementNodeType.Mappings:
					elementToAdd = GetMappings(rootElement);
					break;
			}
			if (elementToAdd != null)
			{
				elementToAdd.Add(content);
			}
		}

		public static void GenerateEmbeddedResources(string edmxFilePath)
		{
			Argument.NotNullOrEmpty(edmxFilePath, nameof(edmxFilePath));

			var directoryPath = Path.GetDirectoryName(edmxFilePath);
			var fileName = Path.GetFileNameWithoutExtension(edmxFilePath);
			var xDocument = XDocument.Load(edmxFilePath);
			foreach (ElementNodeType nodeType in Enum.GetValues(typeof(ElementNodeType)))
			{
				var resourceFilePath = string.Empty;
				var document = new XDocument();
				switch (nodeType)
				{
					case ElementNodeType.StorageModels:
						resourceFilePath = Path.Combine(directoryPath, fileName + Constants.Extensions.Storage);
						document = GetMergeableDocument(xDocument, ElementNodeType.StorageModels);
						break;
					case ElementNodeType.ConceptualModels:
						resourceFilePath = Path.Combine(directoryPath, fileName + Constants.Extensions.ConceptualModels);
						document = GetMergeableDocument(xDocument, ElementNodeType.ConceptualModels);
						break;
					case ElementNodeType.Mappings:
						resourceFilePath = Path.Combine(directoryPath, fileName + Constants.Extensions.Mapping);
						document = GetMergeableDocument(xDocument, ElementNodeType.Mappings);
						break;
				}

				document.Declaration = new XDeclaration("1.0", "utf-8", null);
				document.Save(resourceFilePath);
				Console.WriteLine($"Generated {resourceFilePath} successfully");
			}
		}

		public static void SaveSafeEdmx(XDocument edmx, FileStream file)
		{
			Argument.NotNull(edmx, nameof(edmx));
			Argument.NotNull(file, nameof(file));

			var root = GetEdmxRootElement(edmx);
			var conceptual = GetConceptualModels(root);
			var mapping = GetMappings(root);

			var tablesToRename = new Tuple<string, string>[]
			{
					Tuple.Create("RefCountryState", "RefCountryStates")
			};
			EdmxManualFixes.RefTimeZoneSetNavigationPropertiesFix(conceptual);
			EdmxManualFixes.RenameTablesWithPluralizationProblem(tablesToRename, conceptual, mapping);

			MergeAndSave(edmx, file);
		}

		public static void SaveStagingEdmx(XDocument edmx, FileStream file)
		{
			Argument.NotNull(edmx, nameof(edmx));
			Argument.NotNull(file, nameof(file));

			var root = GetEdmxRootElement(edmx);
			var conceptual = GetConceptualModels(root);
			var mapping = GetMappings(root);

			var tablesToRename = new Tuple<string, string>[]
			{
					Tuple.Create("RefCountryState", "RefCountryStates"),
					Tuple.Create("ProcessorStatu", "ProcessorStatus")
			};

			EdmxManualFixes.RefTimeZoneSetNavigationPropertiesFix(conceptual);
			EdmxManualFixes.RenameTablesWithPluralizationProblem(tablesToRename, conceptual, mapping);

			MergeAndSave(edmx, file);
		}

		static void MergeAndSave(XDocument source, FileStream file)
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(file, nameof(file));

			file.Position = 0;
			var target = XDocument.Load(file);
			Merge(target, source);

			file.SetLength(0);
			target.Save(file);
		}

		public static void Merge(XDocument target, XDocument source)
		{
			Argument.NotNull(target, nameof(target));
			Argument.NotNull(source, nameof(source));
			Merge(target.Root, source.Root);
		}

		static void Merge(XElement target, XElement source)
		{
			Argument.NotNull(target, nameof(target));
			Argument.NotNull(source, nameof(source));
			if (source.Name.LocalName == "Key")
			{
				return;
			}
			foreach (var srcAttr in source.Attributes())
			{
				var targetAttr = target.Attribute(srcAttr.Name);
				if (targetAttr == null)
				{
					target.Add(srcAttr);
				}
				else
				{
					targetAttr.Value = srcAttr.Value;
				}
			}

			var targetAux = target.Elements().Where(x => GetKey(x) != null).ToList();

			foreach (var srcElement in source.Elements())
			{
				var childKeyAttribute = GetKey(srcElement);
				XElement targetElement = null;
				if (childKeyAttribute != null)
				{
					targetElement = target.Elements().FirstOrDefault(x => x.Name == srcElement.Name &&
					   x.Attribute(childKeyAttribute.Name).Value == childKeyAttribute.Value);

					targetAux.Remove(targetElement);
				}
				else
				{
					targetElement = target.Element(srcElement.Name);
				}
				if (targetElement == null)
				{
					target.Add(srcElement);
				}
				else
				{
					Merge(targetElement, srcElement);
				}
			}
			if (targetAux.Any())
			{
				targetAux.ForEach(x =>
				{
					if (AvoidableElementsOnDelete.Contains(x.Name.LocalName))
					{
						var key = GetKey(x);
						if (key.Value.EndsWith("view", StringComparison.OrdinalIgnoreCase) || key.Value.EndsWith("views", StringComparison.OrdinalIgnoreCase))
						{
							return;
						}
					}
					else if (x.Name.LocalName == Constants.Elements.FunctionImport)
					{
						return;
					}
					x.Remove();
				});
			}
			var srcNodeText = source.Nodes().OfType<XText>().FirstOrDefault();
			if (srcNodeText != null)
			{
				var targetNodeText = target.Nodes().OfType<XText>().FirstOrDefault();
				if (targetNodeText == null)
				{
					target.Add(srcNodeText);
				}
				else
				{
					targetNodeText.Value = srcNodeText.Value;
				}
			}
		}

		static string[] AvoidableElementsOnDelete => new string[] { Constants.Elements.Association, Constants.Elements.AssociationSet, Constants.Elements.NavigationProperty };

		static XAttribute GetKey(XElement element)
		{
			Argument.NotNull(element, nameof(element));
			return element.Attribute(XName.Get("Name")) ?? element.Attribute(XName.Get("Role"));
		}
	}
}
