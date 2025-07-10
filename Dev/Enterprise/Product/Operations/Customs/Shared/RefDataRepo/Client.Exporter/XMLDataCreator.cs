using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDataRepo.Ent.Client.Exporter
{
	public class XMLDataCreator : IXMLDataCreator
	{
		public XMLDataCreator(IValueRetrieval valueRetrieval, IReferenceXMLMappingProvider mapping)
		{
			Argument.NotNull(valueRetrieval, nameof(valueRetrieval));
			Argument.NotNull(mapping, nameof(mapping));

			this.valueRetrieval = valueRetrieval;
			this.mapping = mapping;
		}

		readonly IValueRetrieval valueRetrieval;
		readonly IReferenceXMLMappingProvider mapping;

		public object Create(IDataRow data, Type storageType)
		{
			var xmlType = mapping.GetMappedType(storageType);
			var result = Activator.CreateInstance(xmlType);
			foreach (var property in storageType.GetProperties())
			{
				foreach (var propertyPath in mapping.GetMappedPropertyPath(property))
				{
					var storageRelatedType = property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>)) ?
						property.PropertyType.GetGenericArguments()[0] : null;
					if (storageRelatedType != null && typeof(IDataSetStorage).IsAssignableFrom(storageRelatedType))
					{
						var values = valueRetrieval.GetRelatedEntities(data, storageType, storageRelatedType);
						foreach (var value in values)
						{
							SetValue(result, propertyPath.Item2, _ => Create(value, storageRelatedType));
						}
					}
					else
					{
						SetValue(result, propertyPath.Item2, x => valueRetrieval.GetValue(data, storageType, x, propertyPath.Item1 == MappedType.Direct ? property.Name : propertyPath.Item2));
					}
				}
			}
			return result;
		}

		bool SetValue(object xmlData, string propertyPath, Func<Type, object> getValue)
		{
			Argument.NotNull(xmlData, nameof(xmlData));
			Argument.NotNullOrEmpty(propertyPath, nameof(propertyPath));
			Argument.NotNull(getValue, nameof(getValue));

			var propertyChain = propertyPath.Split('.');
			var propertyInfo = xmlData.GetType().GetProperty(propertyChain[0]);
			if (propertyInfo != null)
			{
				if (propertyChain.Length > 1)
				{
					var propertyElementType = propertyInfo.PropertyType.GetElementType();
					var relatedXmlData = Activator.CreateInstance(propertyElementType);
					var relatedDataPropertyPath = string.Join(".", propertyChain.Skip(1));
					if (SetValue(relatedXmlData, relatedDataPropertyPath, getValue))
					{
						return SetValue(xmlData, propertyChain[0], _ => relatedXmlData);
					}
				}
				else
				{
					var value = getValue(propertyInfo.PropertyType);
					if (value != null && (!propertyInfo.PropertyType.IsValueType || value != Activator.CreateInstance(propertyInfo.PropertyType)))
					{
						if (propertyInfo.PropertyType.IsArray)
						{
							var propertyElementType = propertyInfo.PropertyType.GetElementType();
							var existingValues = propertyInfo.GetValue(xmlData) as Array;
							var newValues = Array.CreateInstance(propertyElementType, 1 + (existingValues == null ? 0 : existingValues.Length));
							if (existingValues != null)
							{
								Array.Copy(existingValues, newValues, existingValues.Length);
							}
							newValues.SetValue(getValue(propertyInfo.PropertyType), newValues.Length - 1);
							propertyInfo.SetValue(xmlData, newValues);
						}
						else
						{
							propertyInfo.SetValue(xmlData, getValue(propertyInfo.PropertyType));
						}
						return true;
					}
				}
			}
			return false;
		}
	}
}
