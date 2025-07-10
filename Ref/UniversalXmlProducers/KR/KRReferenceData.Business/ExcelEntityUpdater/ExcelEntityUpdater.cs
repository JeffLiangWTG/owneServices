using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class ExcelEntityUpdater<T> where T : RefDataRepoModelEntityType
	{
		public ExcelEntityUpdater(EntityConfiguration configuration, IAdditionalDataUpdater<T> additionalDataUpdater, ITopEntityLookupManager<T> topEntityLookupManager = null)
		{
			this.configuration = configuration;
			this.additionalDataUpdater = additionalDataUpdater;
			this.topEntityLookupManager = topEntityLookupManager;
			valuesToTreatEmpty = configuration.ValuesToTreatEmpty?.Split(',').ToList() ?? new List<string>();
		}
		readonly EntityConfiguration configuration;
		readonly IAdditionalDataUpdater<T> additionalDataUpdater;
		readonly ITopEntityLookupManager<T> topEntityLookupManager;
		readonly List<string> valuesToTreatEmpty;

		public List<T> Update(IWorkbook workbook)
		{
			var sheetIndex = configuration.EntityTypeExcelColumnMapping.SheetIndex;
			var startRow = configuration.EntityTypeExcelColumnMapping.StartRow;
			var sheet = workbook.GetSheetAt(sheetIndex);
			var result = new List<T>();

			if (configuration.EntityTypeExcelColumnMapping != null)
			{
				var rowIndex = startRow;
				IRow row;
				var topEntitiesByKey = new Dictionary<string, T>(sheet.PhysicalNumberOfRows);

				while ((row = sheet.GetRow(rowIndex)) != null)
				{
					if (additionalDataUpdater?.IsDataRowValid(row, configuration) ?? true)
					{
						var entityToUpdate = UpdateEntityFromRow(row, topEntitiesByKey);
						if (additionalDataUpdater != null)
						{
							additionalDataUpdater.UpdateAdditionally(entityToUpdate, row, configuration);

							if (configuration.Rules != null)
							{
								foreach (var rule in configuration.Rules)
								{
									additionalDataUpdater.UpdateRule(entityToUpdate, rule);
								}
							}
							additionalDataUpdater.RegisterUpdated(row, configuration);
						}
					}
					rowIndex++;
				}

				result.AddRange(topEntitiesByKey.Values.AsEnumerable());
			}
			return result;
		}

		T UpdateEntityFromRow(IRow row, Dictionary<string, T> topEntitiesByKey)
		{
			T result = null;
			object targetEntity;
			var mapping = configuration.EntityTypeExcelColumnMapping;
			var subEntities = EntityConfigurationManager.GetSubEntityList(configuration, typeof(T).Name);
			var topEntitykeyDetails = new KeyLookupDetails<T>(configuration, row);
			var topEntityKeyString = topEntitykeyDetails.GetCombinedKeyValues();

			foreach (var entity in mapping.EntityTypes)
			{
				var entityType = EntityConfigurationManager.GetEntityType(entity.Name);
				if (entityType == null)
				{
					throw new ArgumentException($"EntityType {entity.Name} does not exist.");
				}
				var isEntityUpdated = false;
				var isTopEntity = entityType == typeof(T);
				if (isTopEntity)
				{
					if (topEntityLookupManager == null)
					{
						result = (T)Activator.CreateInstance(typeof(T));
					}
					else
					{
						result = topEntityLookupManager.GetTopEntity(topEntitiesByKey, topEntitykeyDetails);
					}

					if (result == null)
					{
						break;
					}

					if (!string.IsNullOrEmpty(topEntityKeyString) && !topEntitiesByKey.TryGetValue(topEntityKeyString, out T dummyResult))
					{
						topEntitiesByKey.Add(topEntityKeyString, result);
					}
					targetEntity = result;
				}
				else
				{
					targetEntity = Activator.CreateInstance(entityType);
				}

				isEntityUpdated = UpdateTargetEntityAccordingToMapping(row, entity, targetEntity);

				if (!isTopEntity && isEntityUpdated)
				{
					CreateParentIfNeeded(entity.Name, subEntities);
					AddToEntityList(targetEntity, subEntities, entity.Name);
				}
			}

			SetSubEntities(subEntities, result);

			return result;
		}

		void SetSubEntities(Dictionary<string, object> subEntities, T topEntity)
		{
			foreach (string name in subEntities.Keys)
			{
				if (Count(subEntities[name]) > 0)
				{
					EntityRelationships.TryGetValue(name, out var parentEntityName);
					var parentEntityType = EntityConfigurationManager.GetEntityType(parentEntityName);
					var parentEntity = GetParentEntity(parentEntityName, subEntities, topEntity);
					var subEntityArraryPropertyInfo = parentEntityType.GetProperty(EntityConfigurationManager.GetEntityNameInPlural(name));
					var entityType = EntityConfigurationManager.GetEntityType(name);
					var subEntityArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(entityType).Invoke(null, new[] { subEntities[name] });
					var existingSubentityArray = subEntityArraryPropertyInfo.GetValue(parentEntity);

					if (existingSubentityArray != null)
					{
						var subEntityDataType = existingSubentityArray.GetType().GetElementType();
						var concatenated = typeof(Enumerable).GetMethod(nameof(Enumerable.Concat)).MakeGenericMethod(subEntityDataType).Invoke(null, new[] { existingSubentityArray, subEntityArray });
						var mergedArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(subEntityDataType).Invoke(null, new[] { concatenated });
						subEntityArraryPropertyInfo.SetValue(parentEntity, mergedArray);
					}
					else
					{
						subEntityArraryPropertyInfo.SetValue(parentEntity, subEntityArray);
					}
				}
			}
		}

		void CreateParentIfNeeded(string entityName, Dictionary<string, object> subEntities)
		{
			if (entityName != typeof(T).Name)
			{
				EntityRelationships.TryGetValue(entityName, out var parentEntityName);

				if (parentEntityName == null)
				{
					throw new ArgumentException("There is no parent entity name for " + entityName);
				}

				subEntities.TryGetValue(parentEntityName, out var listOfParent);
				if (listOfParent != null && Count(listOfParent) == 0)
				{
					var parentEntityType = EntityConfigurationManager.GetEntityType(parentEntityName);
					var parentEntity = Activator.CreateInstance(parentEntityType);
					AddToEntityList(parentEntity, subEntities, parentEntityName);
				}
				CreateParentIfNeeded(parentEntityName, subEntities);
			}
		}

		static RefDataRepoModelEntityType GetParentEntity(string parentEntityName, Dictionary<string, object> subEntities, RefDataRepoModelEntityType topEntity)
		{
			RefDataRepoModelEntityType result = null;
			if (parentEntityName == topEntity.GetType().Name)
			{
				result = topEntity;
			}
			else
			{
				var childList = subEntities[parentEntityName];

				if (Count(childList) == 1)
				{
					result = (RefDataRepoModelEntityType)childList.GetType().GetProperty("Item").GetValue(childList, new object[] { 0 });
				}
			}
			return result;
		}

		bool UpdateTargetEntityAccordingToMapping(IRow row, EntityType entity, object targetEntity)
		{
			var isEntityUpdated = false;
			var entityType = targetEntity.GetType();

			foreach (var property in entity.Properties)
			{
				if (property.IsNonPersistent)
				{
					continue;
				}
				else if (string.IsNullOrEmpty(property.ConstantValue))
				{
					var cell = row.GetCell(property.ExcelColumn);
					cell?.SetCellType(CellType.String);
					var cellValue = cell?.StringCellValue ?? string.Empty;
					if (!string.IsNullOrEmpty(cellValue) && !valuesToTreatEmpty.Any(v => v == cellValue))
					{
						var transformedValue = TransformDataIfRequired(cellValue, property);
						entityType.GetProperty(property.Name).SetValue(targetEntity, transformedValue);
						isEntityUpdated = true;
					}
				}
				else
				{
					entityType.GetProperty(property.Name).SetValue(targetEntity, property.ConstantValue);
					isEntityUpdated = true;
				}
			}
			return isEntityUpdated;
		}

		static object TransformDataIfRequired(string value, Property property)
		{
			object result = null;
			switch (property.Type)
			{
				case Constants.DataType.Datetime:
					if (DateTime.TryParseExact(value, property.DateTimeFormat, null, DateTimeStyles.AssumeLocal, out DateTime dateValue))
					{
						result = property.IsEndDate ? new DateTime(dateValue.Year, dateValue.Month, dateValue.Day, 23, 59, 00) : dateValue;
					}
					break;
				default:
					result = value.Trim();
					break;
			}
			return result;
		}

		Dictionary<string, string> EntityRelationships => entityRelationships ?? (entityRelationships = EntityConfigurationManager.GetEntityRelationship(configuration));
		Dictionary<string, string> entityRelationships;

		static void AddToEntityList(object entity, Dictionary<string, object> subEntities, string entityName)
		{
			if (subEntities.TryGetValue(entityName, out var list))
			{
				list.GetType().GetMethod("Add").Invoke(list, new[] { entity });
			}
		}

		static int Count(object targetPropertyList)
		{
			return (int)targetPropertyList.GetType().GetProperty("Count").GetValue(targetPropertyList);
		}
	}
}
