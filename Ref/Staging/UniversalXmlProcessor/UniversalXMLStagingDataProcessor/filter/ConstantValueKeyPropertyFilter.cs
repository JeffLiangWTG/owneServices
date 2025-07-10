using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class ConstantValueKeyPropertyFilter : IKeyPropertyFilter
	{
		readonly Type safeEntityType;
		readonly IEnumerable<KeyProperty> constantValueKeyProperties;

		public ConstantValueKeyPropertyFilter(Type safeEntityType, KeyProperty[] keyPropertiesOfTheEntityType)
		{
			this.safeEntityType = safeEntityType;
			EnsurePropertiesBelongToSafeEntityType(safeEntityType, keyPropertiesOfTheEntityType);
			constantValueKeyProperties = keyPropertiesOfTheEntityType.Where(o => !string.IsNullOrEmpty(o.ConstantValue));
		}

		static void EnsurePropertiesBelongToSafeEntityType(Type safeEntityType, KeyProperty[] keyProperties)
		{
			var tablePrefix = safeEntityType.GetTablePrefix();
			foreach (var keyProperty in keyProperties)
			{
				if (!string.IsNullOrEmpty(keyProperty.Name) && !keyProperty.Name.StartsWith(tablePrefix, StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidOperationException($"{keyProperty.Name} is not one direct property of {safeEntityType.Name}");
				}
			}
		}

		public IEnumerable<IStagingDataWrapper> GetFilteredEntities(IEnumerable<IStagingDataWrapper> entities)
		{
			if (entities == null)
			{
				return Enumerable.Empty<IStagingDataWrapper>();
			}
			var filteredEntities = entities;
			foreach (var keyProperty in constantValueKeyProperties)
			{
				var propertyName = keyProperty.Name;
				var propertyOperation = keyProperty.Operation;
				var propertyType = safeEntityType.GetProperty(propertyName)?.PropertyType;
				var constantValue = keyProperty.ConstantValue;
				filteredEntities = filteredEntities.Where(entity =>
				{
					var entityPropertyValue = entity.GetWrapperValue(propertyName);
					return CompareHelper.MatchPropertyValues(propertyType, constantValue, entityPropertyValue.ConvertToSafeValue(propertyType), propertyOperation, StringComparison.OrdinalIgnoreCase);
				});
			}
			return filteredEntities;
		}

		public IEnumerable<object> GetFilteredSafeObjects(IEnumerable<object> safeObjects)
		{
			if (safeObjects == null)
			{
				return Array.Empty<object>();
			}
			var filteredSafeObjects = safeObjects;
			foreach (var keyProperty in constantValueKeyProperties)
			{
				var propertyName = keyProperty.Name;
				var propertyOperation = keyProperty.Operation;
				var propertyType = safeEntityType.GetProperty(propertyName)?.PropertyType;
				var constantValue = keyProperty.ConstantValue;
				filteredSafeObjects = filteredSafeObjects.Where(safeDataObject =>
				{
					var safeObjectPropertyValue = safeEntityType.GetProperty(propertyName)?.GetValue(safeDataObject);
					return CompareHelper.MatchPropertyValues(propertyType, constantValue, safeObjectPropertyValue.ConvertToSafeValue(propertyType), propertyOperation, StringComparison.OrdinalIgnoreCase);
				});
			}
			return filteredSafeObjects;
		}
	}
}
