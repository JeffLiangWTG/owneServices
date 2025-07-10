using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Utilities
{
	public class EntityTypeHelper : IEntityTypeHelper
	{
		readonly Dictionary<Type, List<PropertyInfo>> entityPropertyInfos = new Dictionary<Type, List<PropertyInfo>>();
		readonly Dictionary<Type, PropertyInfo> entityPrimaryKeyPropertyInfos = new Dictionary<Type, PropertyInfo>();
		readonly IObsoletePropertiesLookup obsoletePropertiesLookup;

		public EntityTypeHelper(IObsoletePropertiesLookup obsoletePropertiesLookup)
		{
			Argument.NotNull(obsoletePropertiesLookup, nameof(obsoletePropertiesLookup));
			this.obsoletePropertiesLookup = obsoletePropertiesLookup;
		}

		public string GetTableCode(Type entityType)
		{
			Argument.NotNull(entityType, nameof(entityType));
			var properties = GetProperties(entityType);
			foreach (var propertyInfo in properties)
			{
				var index = propertyInfo.Name.IndexOf("_PK", StringComparison.Ordinal);
				if (index > 0)
				{
					return propertyInfo.Name.Substring(0, index);
				}
			}

			return string.Empty;
		}

		public PropertyInfo GetPrimaryKeyProperty(Type entityType)
		{
			Argument.NotNull(entityType, nameof(entityType));
			if (!entityPrimaryKeyPropertyInfos.ContainsKey(entityType))
			{
				var properties = GetProperties(entityType);
				foreach (var propertyInfo in properties)
				{
					var index = propertyInfo.Name.IndexOf("_PK", StringComparison.Ordinal);
					if (index > 0)
					{
						entityPrimaryKeyPropertyInfos[entityType] = propertyInfo;
						break;
					}
				}
			}

			return entityPrimaryKeyPropertyInfos.TryGetValue(entityType, out var primaryKeyInfo) ? primaryKeyInfo : null;
		}

		public PropertyInfo GetProperty(Type entityType, string propertyName)
		{
			Argument.NotNull(entityType, nameof(entityType));
			var properties = GetProperties(entityType);

			PropertyInfo propertyInfo = null;
			if (!string.IsNullOrEmpty(propertyName))
			{
				propertyInfo = properties.FirstOrDefault(
					x => string.Compare(x.Name, propertyName, StringComparison.OrdinalIgnoreCase) == 0);

				if (propertyInfo == null)
				{
					var mappedPropertyName = obsoletePropertiesLookup.LookupObsoleteName(propertyName);
					if (!string.IsNullOrEmpty(mappedPropertyName))
					{
						return GetProperty(entityType, mappedPropertyName);
					}
				}
			}

			return propertyInfo;
		}

		public List<PropertyInfo> GetProperties(Type entityType)
		{
			Argument.NotNull(entityType, nameof(entityType));
			if (!entityPropertyInfos.ContainsKey(entityType))
			{
				entityPropertyInfos[entityType] = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
			}

			return entityPropertyInfos[entityType];
		}

		public PropertyInfo GetForeignKeyProperty(Type entityType, string childTableCode, string parentTableCode)
		{
			Argument.NotNull(entityType, nameof(entityType));
			Argument.NotNullOrEmpty(childTableCode, nameof(childTableCode));
			Argument.NotNullOrEmpty(parentTableCode, nameof(parentTableCode));
			var properties = GetProperties(entityType);
			return properties?.FirstOrDefault(x => x.Name.StartsWith($"{childTableCode}_{parentTableCode}", StringComparison.InvariantCultureIgnoreCase));
		}
	}
}
