using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public static class EntityConfigurationManager
	{
		public static T DeserializeXML<T>(string configFilePath)
		{
			T result;
			var serializer = new XmlSerializer(typeof(T));
			using (var xmlReader = XmlReader.Create(configFilePath))
			{
				result = (T)serializer.Deserialize(xmlReader);
			}
			return result;
		}

		public static XmlWriterConfiguration GetWriterConfiguration(EntityConfiguration configuration)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			foreach (var entityType in configuration.EntityTypes)
			{
				switch (entityType.Name)
				{
					case nameof(RefCusTariff):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusTariff>(entityType));
						break;
					case nameof(RefCusTariffUOM):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusTariffUOM>(entityType));
						break;
					case nameof(RefCusTariffLanguage):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusTariffLanguage>(entityType));
						break;
					case nameof(RefCusTariffAttribute):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusTariffAttribute>(entityType));
						break;
					case nameof(RefCusCodeList):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusCodeList>(entityType));
						break;
					case nameof(RefCusCodeListAttribute):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusCodeListAttribute>(entityType));
						break;
					case nameof(RefCusCodeListLanguage):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusCodeListLanguage>(entityType));
						break;
					case nameof(RefCusPreference):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusPreference>(entityType));
						break;
					case nameof(RefCusMap):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusMap>(entityType));
						break;
					case nameof(RefCusCondition):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusCondition>(entityType));
						break;
					case nameof(RefCusConditionValue):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusConditionValue>(entityType));
						break;
					case nameof(RefCusTradeGroup):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusTradeGroup>(entityType));
						break;
					case nameof(RefCusTradeGroupCountry):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusTradeGroupCountry>(entityType));
						break;
					case nameof(RefCusNomenclatureGroup):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusNomenclatureGroup>(entityType));
						break;
					case nameof(RefCusNomenclatureLanguage):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusNomenclatureLanguage>(entityType));
						break;
					case nameof(RefCusRate):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusRate>(entityType));
						break;
					case nameof(RefCusRateUOM):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusRateUOM>(entityType));
						break;
					case nameof(RefCusApplicability):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusApplicability>(entityType));
						break;
					case nameof(RefCusTariffAdditionalCode):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusTariffAdditionalCode>(entityType));
						break;
					case nameof(RefCusTariffAdditionalCodeLanguage):
						writerConfiguration.IncludeEntityTypeConfiguration(GetConfiguration<RefCusTariffAdditionalCodeLanguage>(entityType));
						break;
				}
			}
			return writerConfiguration;
		}

		static EntityTypeConfiguration<T> GetConfiguration<T>(EntityType entityType)
		{
			var result = new EntityTypeConfiguration<T>(entityType.Data);
			var keyList = entityType.Key.ToList();
			foreach (var property in entityType.Properties)
			{
				var isComplexEntity = IsComplexEntity(property);
				var propertyName = isComplexEntity ? GetEntityNameInPlural(property.Name) : property.Name;
				var keyColumnOfThisEntity = !isComplexEntity ? keyList.SingleOrDefault(x => x.Name == propertyName) : null;
				var isKeyColumn = keyColumnOfThisEntity != null;
				if (keyColumnOfThisEntity != null)
				{
					keyList.Remove(keyColumnOfThisEntity);//will be added below if 'isKeyColumn' is true
				}

				if (!string.IsNullOrEmpty(property.ConstantValue))
				{
					result.IncludeColumnWithConstantValue(propertyName, isKeyColumn, property.ConstantValue);
				}
				else if (property.DefaultValue != null)
				{
					result.IncludeColumnWithDefaultValue(propertyName, isKeyColumn, property.DefaultValue);
				}
				else
				{
					result.IncludeColumn(propertyName, isKeyColumn);
				}
			}

			foreach (var key in keyList)
			{
				result.IncludeKey(key.Name);
			}
			return result;
		}

		public static string GetEntityNameInPlural(string entityName)
		{
			var result = string.Empty;
			if (entityName != null)
			{
				if (entityName.EndsWith("y", StringComparison.Ordinal))
				{
					result = string.Concat(entityName.Substring(0, entityName.Length - 1), "ies");
				}
				else
				{
					result = string.Concat(entityName, "s");
				}
			}
			return result;
		}

		public static Dictionary<string, string> GetEntityRelationship(EntityConfiguration configuration)
		{
			var result = new Dictionary<string, string>();
			foreach (var entity in configuration.EntityTypes)
			{
				foreach (var property in entity.Properties)
				{
					if (IsComplexEntity(property))
					{
						if (!result.TryGetValue(property.Name, out var parent))
						{
							result.Add(property.Name, entity.Name);
						}
						else
						{
							throw new InvalidConstraintException($"EntityType {entity.Name} has more than one parent, {parent} and {property.Name}.");
						}
					}
				}
			}
			return result;
		}

		public static Dictionary<string, object> GetSubEntityList(EntityConfiguration configuration, string topEntityName)
		{
			var result = new Dictionary<string, object>();
			foreach (var entity in configuration.EntityTypes.Where(x => x.Name != topEntityName))
			{
				var entityType = GetEntityType(entity.Name);
				if (entityType == null)
				{
					throw new ArgumentException($"EntityType {entity.Name} does not exist.");
				}
				result.Add(entity.Name, Activator.CreateInstance(typeof(List<>).MakeGenericType(new[] { entityType })));
			}
			return result;
		}

		public static Type GetEntityType(string entityTypeName)
		{
			var tariffType = typeof(RefCusTariff);
			var assembly = tariffType.Assembly;
			var namespaceString = tariffType.Namespace;
			return assembly.GetType(namespaceString + "." + entityTypeName);
		}

		static bool IsComplexEntity(Property property) => property.Name == property.Type;
	}
}
