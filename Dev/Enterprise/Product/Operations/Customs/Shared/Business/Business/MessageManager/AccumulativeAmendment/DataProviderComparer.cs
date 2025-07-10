using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.Types;

namespace Enterprise.Customs.Business.AccumulativeAmendment
{
	public class DataProviderComparer<TDataProviderInterface>
	{
		public Entity Compare(TDataProviderInterface originalSnapshot, TDataProviderInterface currentSnapshot)
		{
			return Compare(originalSnapshot, currentSnapshot, new DataItemComparer());
		}

		public Entity Compare(TDataProviderInterface originalSnapshot, TDataProviderInterface currentSnapshot, DataItemComparer dataComparer)
		{
			var entity = new Entity();
			PopulateEntityFieldAndSubEntity(entity, CompareObject(originalSnapshot, currentSnapshot, typeof(TDataProviderInterface), dataComparer));
			PopulateEntity(entity, typeof(TDataProviderInterface).Name, IsEntityUpdated(entity) ? EntityAmendType.Update : EntityAmendType.NoChange);
			return entity;
		}

		Tuple<IEnumerable<EntityField>, IEnumerable<Entity>> CompareObject(object originalSource, object currentSource, Type providerType, DataItemComparer dataComparer)
		{
			var fieldsToUpdate = new List<EntityField>();
			var entitiesToUpdate = new List<Entity>();
			var isOriginalNull = originalSource == null;
			var isCurrentNull = currentSource == null;
			foreach (var propertyInfo in GetInterfaceMembersPropertyInfos(providerType))
			{
				var originalValue = isOriginalNull ? null : propertyInfo.GetValue(originalSource);
				var currentValue = isCurrentNull ? null : propertyInfo.GetValue(currentSource);
				var isXMLIgnore = Attribute.IsDefined(propertyInfo, typeof(XmlIgnoreAttribute));
				if ((originalValue != null || currentValue != null) && !isXMLIgnore)
				{
					var propertyType = propertyInfo.PropertyType;
					var isIEnumerableType = false;
					if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>)))
					{
						propertyType = propertyType.GetGenericArguments().FirstOrDefault();
						isIEnumerableType = true;
					}

					if (propertyType.IsValueType)
					{
						if (!dataComparer.IsTheSame(originalValue, currentValue, propertyInfo))
						{
							var entityField = new EntityField();
							entityField.Name = propertyInfo.Name;
							entityField.Before = SerialiseToString(originalValue);
							entityField.After = SerialiseToString(currentValue);
							fieldsToUpdate.Add(entityField);
						}
					}
					else
					{
						if (isIEnumerableType)
						{
							var originalList = originalValue == null ? null : ((IEnumerable)originalValue).Cast<object>();
							var currentList = currentValue == null ? null : ((IEnumerable)currentValue).Cast<object>();
							entitiesToUpdate.AddRange(CompareList(propertyType, propertyInfo.Name, originalList, currentList, dataComparer));
						}
						else
						{
							var subEntity = new Entity();
							var amendType = originalValue == null ? EntityAmendType.Add : currentValue == null ? EntityAmendType.Delete : EntityAmendType.Update;
							object id = null;
							var idField = IDProvider.GetIDField(propertyType);
							if (idField != null)
							{
								id = (amendType == EntityAmendType.Add) ? GetID(currentValue, idField) : GetID(originalValue, idField);
							}
							PopulateEntity(subEntity, propertyType.Name, propertyInfo.Name, amendType, id);
							PopulateEntityFieldAndSubEntity(subEntity, CompareObject(originalValue, currentValue, propertyType, dataComparer));
							if (IsEntityUpdated(subEntity))
							{
								entitiesToUpdate.Add(subEntity);
							}
						}
					}
				}
			}
			return Tuple.Create<IEnumerable<EntityField>, IEnumerable<Entity>>(fieldsToUpdate, entitiesToUpdate);

			IEnumerable<PropertyInfo> GetInterfaceMembersPropertyInfos(Type type)
			{
				IEnumerable<PropertyInfo> result = type.GetProperties();
				if (type.IsInterface)
				{
					result = result.Concat(type.GetInterfaces().SelectMany(x => x.GetProperties()));
				}
				return result;
			}
		}
		string SerialiseToString(object value)
		{
			var result = SystemNullValue;
			if (value != null)
			{
				if (value is ZDate || value is ZDateTime || value is DateTime)
				{
					result = new ZDateTime(value).ToString(ZDateTime.LongTimeFormat, CultureInfo.InvariantCulture);
				}
				else
				{
					result = value.ToString();
				}
			}
			return result;
		}

		public const string SystemNullValue = "SYSTEM_NULL_VALUE";

		IEnumerable<Entity> CompareList(Type propertyType, string propertyName, IEnumerable<object> originalList, IEnumerable<object> currentList, DataItemComparer dataComparer)
		{
			var result = new List<Entity>();
			var idField = IDProvider.GetIDField(propertyType);
			if (idField != null)
			{
				List<object> addedItems = currentList != null ? new List<object>(currentList) : null;
				if (originalList != null)
				{
					foreach (var originalItem in originalList)
					{
						var subEntity = new Entity();
						var id = GetID(originalItem, idField);

						var matchedItem = currentList?.Where(x => GetID(x, idField).Equals(id)).FirstOrDefault();
						if (matchedItem != null)
						{
							addedItems.Remove(matchedItem);
							PopulateEntityFieldAndSubEntity(subEntity, CompareObject(originalItem, matchedItem, propertyType, dataComparer));
							if (IsEntityUpdated(subEntity))
							{
								result.Add(subEntity);
							}
						}
						else
						{
							result.Add(subEntity);
						}

						PopulateEntity(subEntity, propertyType.Name, propertyName, matchedItem != null ? EntityAmendType.Update : EntityAmendType.Delete, id);
					}
				}

				if (addedItems != null)
				{
					foreach (var addedItem in addedItems)
					{
						var subEntity = new Entity();
						PopulateEntity(subEntity, propertyType.Name, propertyName, EntityAmendType.Add, GetID(addedItem, idField));
						PopulateEntityFieldAndSubEntity(subEntity, CompareObject(null, addedItem, propertyType, dataComparer));
						result.Add(subEntity);
					}
				}
			}
			return result;
		}

		static object GetID(object x, PropertyInfo idField) => idField.GetValue(x);
		static bool IsEntityUpdated(Entity entity) => entity.Field != null || entity.SubEntity != null;
		static void PopulateEntity(Entity entity, string type, EntityAmendType amendType)
		{
			entity.Type = type;
			entity.AmendType = amendType;
			entity.AmendTypeSpecified = true;
		}

		static void PopulateEntity(Entity entity, string type, string name, EntityAmendType amendType, object id = null)
		{
			PopulateEntity(entity, type, amendType);
			entity.Name = name;
			if (id != null)
			{
				entity.ID = id.ToString();
			}
		}

		static void PopulateEntityFieldAndSubEntity(Entity entity, Tuple<IEnumerable<EntityField>, IEnumerable<Entity>> itemsToUpdate)
		{
			var fieldsToUpdate = itemsToUpdate.Item1;
			entity.Field = fieldsToUpdate.Any() ? fieldsToUpdate.ToArray() : null;

			var entitiesToUpdate = itemsToUpdate.Item2;
			entity.SubEntity = entitiesToUpdate.Any() ? entitiesToUpdate.ToArray() : null;
		}
	}

	public class DataItemComparer
	{
		public virtual bool IsTheSame(object originalValue, object currentValue, PropertyInfo property)
		{
			var result = object.Equals(originalValue, currentValue);
			if (!result)
			{
				if (originalValue == null && currentValue != null)
				{
					var valueAsIZType = currentValue as IZType;
					if (valueAsIZType != null)
					{
						var typeOfCurrentValue = currentValue.GetType();
						if (typeOfCurrentValue == typeof(ZDate) || typeOfCurrentValue == typeof(ZDateTime))
						{
							result = !valueAsIZType.IsValid;
						}
						else
						{
							result = valueAsIZType.IsDefault;
						}
					}
				}
			}
			return result;
		}
	}
}
