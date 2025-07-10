using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using static Azure.Core.HttpHeader;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class MetadataProvider : IMetadataProvider
	{
		public MetadataProvider(string xmlContent, ICacheProvider cache, ICacheProvider cacheForOriginal, bool isAutoSchema = false)
		{
			Argument.NotNullOrEmpty(xmlContent, nameof(xmlContent));
			Argument.NotNull(cache, nameof(cache));

			this.cache = cache;
			FilterData = isAutoSchema;
			xml = XElement.Parse(xmlContent);
			var originalSchema = xml.Element("OriginalSchema");
			if (originalSchema != null)
			{
				var newXml = new XElement(xml);
				var schemaInNewXml = newXml.Element("Schema");
				schemaInNewXml.Remove();
				var originalSchemaInNewXml = newXml.Element("OriginalSchema");
				originalSchemaInNewXml.Name = "Schema";
				originalMetadataProvider = new MetadataProvider(newXml, cacheForOriginal, isAutoSchema);
				originalSchema.Remove();
			}
		}

		MetadataProvider(XElement xElement, ICacheProvider cache, bool isAutoSchema)
		{
			Argument.NotNull(xElement, nameof(xElement));
			Argument.NotNull(cache, nameof(cache));

			xml = xElement;
			this.cache = cache;
			FilterData = isAutoSchema;
		}

		readonly ICacheProvider cache;
		readonly XElement xml;

		const string SchemaElementName = "Schema";
		const string EntityTypeElementName = "EntityType";
		const string EntityNameAttributeName = "Name";
		const string KeyElementName = "Key";
		const string KeyOrderAttributeName = "Order";
		const string PropertyRefElementName = "PropertyRef";
		const string PropertyRefNameAttributeName = "Name";
		const string DataAttributeName = "Data";
		const string ConstantValueAttributeName = "ConstantValue";
		const string PropertyElementName = "Property";
		const string PropertyNameAttributeName = "Name";
		const string MandatoryAttributeName = "Mandatory";
		const string IAmUniqueSuffix = "_IAMUnique";
		const string EnableNullOrEmptyKeyMatchingAttributeName = "EnableNullOrEmptyKeyMatching";
		const string EnableExpirableAttributeName = "EnableExpirable";
		public const string AttributeOperation = "Op";
		public const string AttributeConstantValue = "ConstantValue";

		readonly IMetadataProvider originalMetadataProvider;
		public IMetadataProvider OriginalMetadataProvider => originalMetadataProvider;

		public KeyProperty[] GetKeys(string entityType)
		{
			Argument.NotNullOrEmpty(entityType, nameof(entityType));
			var value = cache.MetadataCache.GetOrAdd(GetFullKey(nameof(IMetadataProvider.GetKeys), entityType), key => new Lazy<object>(() =>
			{
				var entityTypeElement = GetEntityTypeElement(entityType);
				var keyElements = entityTypeElement.Elements(KeyElementName);
				var keyPropertyList = new List<KeyProperty>();
				foreach (var keyElement in keyElements)
				{
					var order = 0;
					var orderValue = keyElement.Attribute(KeyOrderAttributeName)?.Value;
					if (!string.IsNullOrEmpty(orderValue))
					{
						order = int.Parse(orderValue, CultureInfo.InvariantCulture);
					}
					keyPropertyList.AddRange(keyElement.Elements(PropertyRefElementName).Select(x => BuildKeyProperty(x, order)));
				}
				return keyPropertyList.ToArray();
			}));
			var result = (KeyProperty[])value?.Value;
			return result;
		}

		static KeyProperty BuildKeyProperty(XElement x, int keyOrder)
		{
			Argument.NotNull(x, nameof(x));

			var attributeName = x.Attribute(PropertyRefNameAttributeName);
			var result = new KeyProperty { Name = attributeName.Value, Order = keyOrder };
			var attribute = x.Attributes().FirstOrDefault(o => o.Name.LocalName == AttributeOperation);

			if (attribute != null)
			{
				result.Operation = (Operations)Enum.Parse(typeof(Operations), attribute.Value);
			}

			result.ConstantValue = x.Attributes().FirstOrDefault(o => o.Name.LocalName == AttributeConstantValue)?.Value;

			return result;
		}

		public string[] GetProperties(string entityType)
		{
			Argument.NotNullOrEmpty(entityType, nameof(entityType));
			var value = cache.MetadataCache.GetOrAdd(GetFullKey(nameof(IMetadataProvider.GetProperties), entityType), new Lazy<object>(() =>
			{
				var entityTypeElement = GetEntityTypeElement(entityType);
				return entityTypeElement.Elements(PropertyElementName).Select(x => x.Attribute(PropertyNameAttributeName).Value).ToArray();
			}));
			var result = (string[])value.Value;
			return result;
		}

		XElement GetEntityTypeElement(string entityType)
		{
			var entityTypeElement = xml.Element(SchemaElementName)?.Elements(EntityTypeElementName);
			return entityTypeElement?.FirstOrDefault(x => x.Attribute(EntityNameAttributeName).Value.Equals(entityType, StringComparison.OrdinalIgnoreCase));
		}

		public bool IsData(string entityType)
		{
			Argument.NotNullOrEmpty(entityType, nameof(entityType));
			return IsDataCore(entityType, false);
		}

		bool IsDataCore(string entityType, bool ignoreKeyOperations)
		{
			Argument.NotNullOrEmpty(entityType, nameof(entityType));

			var keys = GetKeys(entityType);
			if (!ignoreKeyOperations && keys.Any(o => o.Operation != Operations.Equals))
			{
				return false;
			}
			var value = cache.MetadataCache.GetOrAdd(GetFullKey(nameof(IMetadataProvider.IsData), entityType), key => new Lazy<object>(() =>
			{
				var entityTypeElement = GetEntityTypeElement(entityType);
				var dataAttribute = entityTypeElement.Attribute(DataAttributeName)?.Value;
				return dataAttribute != null && bool.Parse(dataAttribute);
			}))?.Value as bool?;
			return value.Value;
		}

		public bool EnableNullOrEmptyKeyMatching(string entityType)
		{
			Argument.NotNullOrEmpty(entityType, nameof(entityType));

			var value = cache.MetadataCache.GetOrAdd(GetFullKey(nameof(IMetadataProvider.EnableNullOrEmptyKeyMatching), entityType), key => new Lazy<object>(() =>
			{
				var entityTypeElement = GetEntityTypeElement(entityType);
				var attributeValue = entityTypeElement.Attribute(EnableNullOrEmptyKeyMatchingAttributeName)?.Value;
				return attributeValue == null || bool.Parse(attributeValue);
			}))?.Value as bool?;
			return value.Value;
		}

		static string GetFullKey(params string[] keyParts)
		{
			return string.Join("|", keyParts);
		}
		public string[] GetUpdatableProperties(string entityType, int keyOrder = 0)
		{
			Argument.NotNullOrEmpty(entityType, nameof(entityType));
			var keys = GetKeys(entityType)?.Where(x => x.Order == keyOrder).Select(x => x.Name);
			var value = cache.MetadataCache.GetOrAdd(GetFullKey(nameof(IMetadataProvider.GetUpdatableProperties), entityType), new Lazy<object>(() =>
			{
				var entityTypeElement = GetEntityTypeElement(entityType);
				var isData = IsDataCore(entityType, true);
				return entityTypeElement.Elements(PropertyElementName)
					.Where(x => "true".Equals(x.Attribute(DataAttributeName)?.Value, StringComparison.OrdinalIgnoreCase) || (isData && !"false".Equals(x.Attribute(DataAttributeName)?.Value, StringComparison.OrdinalIgnoreCase)))
					.Select(x => x.Attribute(PropertyNameAttributeName).Value)
					.Except(keys).ToArray();
			}));
			var result = (string[])value.Value;
			return result;
		}

		public Tuple<string, string>[] GetConstantPropertyNamesAndValues(string entityType)
		{
			Argument.NotNullOrEmpty(entityType, nameof(entityType));
			var value = cache.MetadataCache.GetOrAdd(GetFullKey(nameof(IMetadataProvider.GetConstantPropertyNamesAndValues), entityType), new Lazy<object>(() =>
			{
				var entityTypeElement = GetEntityTypeElement(entityType);
				return entityTypeElement.Elements(PropertyElementName).Where(x => x.Attribute(ConstantValueAttributeName) != null)
					.Select(x => Tuple.Create(x.Attribute(EntityNameAttributeName).Value, x.Attribute(ConstantValueAttributeName).Value)).ToArray();
			}));
			var result = (Tuple<string, string>[])value.Value;
			return result;
		}

		public bool IsMandatory(string entityType, string propertyName)
		{
			var value = cache.MetadataCache.GetOrAdd(GetFullKey(nameof(IMetadataProvider.IsMandatory), entityType, propertyName), key => new Lazy<object>(() =>
			{
				var entityTypeElement = GetEntityTypeElement(entityType);
				var mandatoryAttribute = entityTypeElement.Elements(PropertyElementName)
					.Where(x => propertyName.Equals(x.Attribute(PropertyNameAttributeName)?.Value, StringComparison.OrdinalIgnoreCase))
					.Select(x => x.Attribute(MandatoryAttributeName)).FirstOrDefault()?.Value;
				return mandatoryAttribute != null && bool.Parse(mandatoryAttribute);
			}))?.Value as bool?;
			return value.Value;
		}

		public bool ShouldCalculateIAmUnique(string entityType, string tblPrefix)
		{
			var value = cache.MetadataCache.GetOrAdd(GetFullKey(nameof(IMetadataProvider.ShouldCalculateIAmUnique), entityType), key => new Lazy<object>(() =>
			{
				var keys = GetKeys(entityType);
				var properties = GetProperties(entityType);
				var keysIncludeRelatedProperty = keys.Any(x => !x.Name.StartsWith(tblPrefix, StringComparison.OrdinalIgnoreCase));
				var schemaIncludeIAmUnique = properties.Any(x => x.EndsWith(IAmUniqueSuffix, StringComparison.OrdinalIgnoreCase));
				return keysIncludeRelatedProperty && !schemaIncludeIAmUnique;
			}));
			var result = (bool)value.Value;
			return result;
		}

		public void Validate()
		{
			foreach (var element in xml.Element(SchemaElementName)?.Elements(EntityTypeElementName))
			{
				var entityTypeName = element.Attribute(EntityNameAttributeName).Value;
				var entityType = typeof(RefCusTariff).GetTypeFromBaseType(entityTypeName);
				var tblPrefix = entityType.GetTablePrefix();
				if (!DataProviderHelper.IsExpirableType(entityType))
				{
					continue;
				}

				string GetKeyPropertyName(string name)
				{
					if (name.IndexOf(".", StringComparison.OrdinalIgnoreCase) > 0)
					{
						return name[..name.IndexOf(".", StringComparison.OrdinalIgnoreCase)];
					}
					return name;
				}

				var keys = GetKeys(entityTypeName).Where(x => !x.Name.StartsWith(tblPrefix, StringComparison.OrdinalIgnoreCase));
				var underCheckKeys = keys.Where(x => x.Name.IndexOf(".", StringComparison.OrdinalIgnoreCase) > 0).Select(x => x.Name);

				var validateResult = underCheckKeys.Where(y => y.IndexOf(".", StringComparison.OrdinalIgnoreCase) > 0);
				if (validateResult.Any() && IsNonPersistentFlatten(entityType))
				{// would remove this check after support it in non-persistent feature
					throw new RefDataProcessingException($"Not allow key property {string.Join(", ", validateResult)} under {entityTypeName}", ErrorCodes.NotSupportedKeyProperty);
				}
				var keyExpirableTypes = keys.Select(x => typeof(RefCusTariff).GetTypeFromBaseType(GetKeyPropertyName(x.Name)))
				.Where(DataProviderHelper.IsExpirableType);

				if (keyExpirableTypes.Count() > 1)
				{
					throw new RefDataProcessingException($"{entityTypeName} contains more than one expirable type as key property.", ErrorCodes.MultipleExpirableKeyProperties, [entityType]);
				}

				var notAllowedTypes = keyExpirableTypes.Where(x => !WhiteListOfExpirableTypeGroups.Contains((entityTypeName, x.Name)));
				if (notAllowedTypes.Any())
				{
					throw new RefDataProcessingException($"xml can't contains expirable type as key property {string.Join(";", notAllowedTypes.Select(x => $"{entityTypeName}-{x.Name}"))};\r\nexcept {string.Join(";", WhiteListOfExpirableTypeGroups.Select(x => $"{x.Parent}-{x.ExpirableChild}"))}.", ErrorCodes.IncorrectXmlSchema, notAllowedTypes);
				}
			}
		}

		public bool FilterData { get; private set; }
		public bool EnableExpirable
		{
			get
			{
				if (!enableExpirable.HasValue)
				{
					foreach (var expirableType in EnableExpireTypeConfiguration.TypesToEnableExpirable)
					{
						var entityTypeElement = GetEntityTypeElement(expirableType.Name);
						var attributeValue = entityTypeElement?.Attribute(EnableExpirableAttributeName)?.Value;
						enableExpirable = !string.IsNullOrEmpty(attributeValue) && bool.Parse(attributeValue);
						if (enableExpirable.Value)
						{
							break;
						}
					}
				}
				return enableExpirable.Value;
			}
		}
		bool? enableExpirable;

		readonly (string Parent, string ExpirableChild)[] WhiteListOfExpirableTypeGroups =
		[
			(nameof(RefCusRate), nameof(RefCusApplicability)),
			(nameof(RefCusCondition), nameof(RefCusApplicability))
		];

		static bool IsNonPersistentFlatten(Type entityType)
		{
			Argument.NotNull(entityType, nameof(entityType));
			return typeof(INonPersistentBusinessObjectFlatten).IsAssignableFrom(entityType);
		}
	}
}
