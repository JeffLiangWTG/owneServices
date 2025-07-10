using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDataRepo.Ent.Client.Exporter
{
	public class ReferenceXMLMappingProvider : IReferenceXMLMappingProvider
	{
		public ReferenceXMLMappingProvider()
		{ }

		public IEnumerable<Tuple<MappedType, string>> GetMappedPropertyPath(PropertyInfo propertyInfo)
		{
			var mappedDeclareType = GetMappedType(propertyInfo.DeclaringType);
			var mappingAttribute = propertyInfo.DeclaringType.GetCustomAttribute<ReferenceXMLMappingTypeAttribute>()?
				.MappingType.GetProperty(propertyInfo.Name)?.GetCustomAttribute<ReferenceXMLMappingAttribute>();
			if (mappingAttribute != null)
			{
				yield return Tuple.Create(MappedType.Direct, mappingAttribute.PropertyPath);
			}
			else if (propertyInfo.PropertyType == typeof(Guid) || propertyInfo.PropertyType == typeof(Guid?))
			{
				var propertyComponents = propertyInfo.Name.Split('_');
				var propertyPrefix = propertyComponents[0] + "_" + propertyComponents[1];
				foreach (var property in mappedDeclareType.GetProperties())
				{
					if (property.Name.StartsWith(propertyPrefix))
					{
						yield return Tuple.Create(MappedType.Expand, property.Name);
					}
				}
			}
			else if (propertyInfo.PropertyType.IsGenericType
				&& typeof(IDataSetStorage).IsAssignableFrom(propertyInfo.PropertyType.GetGenericArguments()[0]))
			{
				var storageRelatedType = propertyInfo.PropertyType.GetGenericArguments()[0];
				var xmlRelatedType = GetMappedType(storageRelatedType);
				var property = mappedDeclareType.GetProperties().FirstOrDefault(x => x.PropertyType.IsArray
					&& x.PropertyType.GetElementType().Equals(xmlRelatedType));
				if (property != null)
				{
					yield return Tuple.Create(MappedType.Direct, property.Name);
				}
			}
			else
			{
				yield return Tuple.Create(MappedType.Direct, propertyInfo.Name);
			}
		}

		public Type GetMappedType(Type storageType)
		{
			var mappingTypeName = storageType.Name.Substring(1);
			var mappingAttr = storageType.GetCustomAttribute<ReferenceXMLMappingAttribute>();
			if (mappingAttr != null)
			{
				mappingTypeName = mappingAttr.PropertyPath;
			}
			var result = typeof(RefDataRepoModelEntityType).Assembly
				.DefinedTypes?.FirstOrDefault(x => typeof(RefDataRepoModelEntityType).IsAssignableFrom(x) &&
				x.Name.Equals(mappingTypeName));
			return result;
		}
	}
}
