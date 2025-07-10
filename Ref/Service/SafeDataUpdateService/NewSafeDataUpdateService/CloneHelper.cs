using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class CloneHelper
	{
		public IEnumerable<CloneProcessResult> Clone(object existingObject, object newObject, CloneProcessObject cloneProcessObject, IDictionary<Type, List<Type>> genericCollectionDictionary)
		{
			Argument.NotNull(existingObject, nameof(existingObject));
			Argument.NotNull(newObject, nameof(newObject));
			Argument.NotNull(cloneProcessObject, nameof(cloneProcessObject));

			var cloneResults = new List<CloneProcessResult>();
			var exceptTheseRecords = cloneProcessObject.ExceptionListForCloning ?? new Dictionary<string, List<Guid>>();
			var type = existingObject.GetType();
			if (type.BaseType != typeof(object))
			{
				type = type.BaseType;
			}
			var propertiesToBeCloned = type.GetProperties().Where(p => genericCollectionDictionary[type].Contains(p.PropertyType));
			foreach (var prop in propertiesToBeCloned)
			{
				var fieldValue = prop.GetValue(existingObject);
				if (fieldValue == null)
				{
					prop.SetValue(newObject, null);
				}
				else
				{
					Guid? expirableAncestorPK = null;
					expirableAncestorPK = newObject.GetType().IsExpirableType() ? newObject.GetPKValue() : expirableAncestorPK;
					var elementType = prop.PropertyType.GetGenericArguments()[0];
					var clonedCollection = this.InvokeGenericMethod(nameof(CloneCollection), elementType, new object[] { fieldValue, newObject, exceptTheseRecords, genericCollectionDictionary, expirableAncestorPK });
					if (clonedCollection as List<CloneProcessResult> != null)
					{
						cloneResults.AddRange(clonedCollection as List<CloneProcessResult>);
					}
				}
			}
			cloneResults.ForEach(x => x.DataSetPK = cloneProcessObject.NewRecordPk);
			return cloneResults;
		}

#if DEBUG
		public
#endif
		List<CloneProcessResult> CloneCollection<T>(ICollection<T> original, object newObject, IDictionary<string, List<Guid>> exceptTheseRecords, IDictionary<Type, List<Type>> dict, Guid? expirableAncestorPK) where T : class
		{
			var cloneResult = new List<CloneProcessResult>();
			var tablePrefix = typeof(T).GetTablePrefix();
			foreach (T element in original.ToList().Where(o => !exceptTheseRecords.ContainsKey(tablePrefix) || !exceptTheseRecords[tablePrefix].Any(x => o.GetPKValue() == x)))
			{
				if (IsDataExpired(element, newObject, exceptTheseRecords))
				{
					continue;
				}
				var record = CloneCore(element, newObject, exceptTheseRecords, dict, expirableAncestorPK, cloneResult);
				cloneResult.Add(new CloneProcessResult { ClonedRecordTypeName = typeof(T).Name, ClonedRecord = record, OriginalRecordPK = element.GetPKValue(), ClonedRecordPK = record.GetPKValue(), ClonedRecordExpirableAncestorPK = expirableAncestorPK });
			}
			return cloneResult;
		}

		T CloneCore<T>(T objToBeCloned, object newObject, IDictionary<string, List<Guid>> exceptTheseRecords, IDictionary<Type, List<Type>> dict, Guid? expirableAncestorPK, List<CloneProcessResult> cloneResult) where T : class
		{
			var result = (T)Activator.CreateInstance(typeof(T));
			var resultType = typeof(T);
			var newRecordTablePrefix = newObject.GetType().GetTablePrefix();
			var newRecordPk = newObject.GetPKValue();
			var fields = resultType.GetProperties();
			var fieldTableCode = resultType.GetTablePrefix();
			var fieldPkColumn = resultType.GetPKPropertyName();

			var resultPK = Guid.NewGuid();
			foreach (var field in fields)
			{
				var propertyType = field.PropertyType;
				if (propertyType.IsValueType || propertyType == typeof(string))
				{
					//if PK then set new GUID.
					if (propertyType == typeof(Guid) && field.Name == fieldPkColumn)
					{
						//set new guid and make sure it's not the parent record.
						if (!fieldPkColumn.StartsWith(newRecordTablePrefix, StringComparison.OrdinalIgnoreCase))
						{
							field.SetValue(result, resultPK);
						}
					}
					//if it is the referenced column, then set the new parent pk.
					else if ((propertyType == typeof(Guid?) || propertyType == typeof(Guid)) && field.Name.StartsWith($"{fieldTableCode}_{newRecordTablePrefix}", StringComparison.OrdinalIgnoreCase))
					{
						//care for current field value, for "nullable" parenting
						//e.g.: TradeGroup, SecondTradeGroup have the same structure but one might be null
						var currentFieldValue = field.GetValue(objToBeCloned);
						if (currentFieldValue != null)
						{
							field.SetValue(result, newRecordPk);
						}
					}
					else
					{
						field.SetValue(result, field.GetValue(objToBeCloned));
					}
				}
				else if (dict.ContainsKey(resultType) && dict[resultType].Contains(propertyType))
				{
					var fieldValue = field.GetValue(objToBeCloned);
					if (fieldValue == null)
					{
						field.SetValue(result, null);
					}
					else
					{
						expirableAncestorPK = resultType.IsExpirableType() ? resultPK : expirableAncestorPK;
						var elementType = propertyType.GetGenericArguments()[0];
						var clonedCollection = this.InvokeGenericMethod(nameof(CloneCollection), elementType, new object[] { fieldValue, result, exceptTheseRecords, dict, expirableAncestorPK });
						if (clonedCollection as List<CloneProcessResult> != null)
						{
							cloneResult.AddRange(clonedCollection as List<CloneProcessResult>);
						}
					}
				}
			}
			return result;
		}

		bool IsDataExpired(object objToBeCloned, object parentObject, IDictionary<string, List<Guid>> exceptTheseRecords)
		{
			var parentType = parentObject.GetType();
			var clonedType = objToBeCloned.GetType();
			if (clonedType.BaseType != typeof(object))
			{
				clonedType = clonedType.BaseType;
			}
			var newObjectTablePrefix = parentType.GetTablePrefix();
			var toBeClonedObjectTablePrefix = clonedType.GetTablePrefix();
			var startDateProperty = parentType.GetProperty($"{newObjectTablePrefix}_StartDate");
			var endDateProperty = clonedType.GetProperty($"{toBeClonedObjectTablePrefix}_EndDate");
			if (startDateProperty != null && endDateProperty != null)
			{
				var startDateValueProperty = startDateProperty.GetValue(parentObject);
				var endDateValueProperty = endDateProperty.GetValue(objToBeCloned);
				if (startDateProperty.PropertyType == typeof(DateTime?) || endDateProperty.PropertyType == typeof(DateTime?))
				{
					if (startDateValueProperty == null || endDateValueProperty == null)
					{
						return false;
					}
				}
				var parentStartDate = Convert.ToDateTime(startDateValueProperty, CultureInfo.InvariantCulture);
				var endDate = Convert.ToDateTime(endDateValueProperty, CultureInfo.InvariantCulture);
				if (endDate < parentStartDate)
				{
					return true;
				}

				if (EffectiveDateRangeTypes.Contains(clonedType))
				{
					var applicabilities = clonedType.GetProperty("RefCusApplicabilities")?.GetValue(objToBeCloned) as ICollection<RefCusApplicability>;
					if (applicabilities != null && applicabilities.Count > 0)
					{
						var clonedTypeIsExpired = true;
						foreach (var applicability in applicabilities)
						{
							var effectiveEndDate = endDate < applicability.ZZT_EndDate ? endDate : applicability.ZZT_EndDate;
							if (effectiveEndDate < parentStartDate)
							{
								if (!exceptTheseRecords.ContainsKey("ZZT"))
								{
									exceptTheseRecords.Add("ZZT", new List<Guid>());
								}
								exceptTheseRecords["ZZT"].Add(applicability.GetPKValue());
							}
							else
							{
								clonedTypeIsExpired = false;
							}
						}
						return clonedTypeIsExpired;
					}
				}
			}
			return false;
		}

		readonly Type[] EffectiveDateRangeTypes = new[] { typeof(RefCusRate), typeof(RefCusCondition) };
	}
}
