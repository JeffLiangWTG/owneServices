using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDataRepo.Ent.Client.Exporter
{
	public class ValueRetrieval : IValueRetrieval
	{
		public ValueRetrieval(IDBHelper dbHelper)
		{
			Argument.NotNull(dbHelper, nameof(dbHelper));

			this.dbHelper = dbHelper;
		}

		readonly IDBHelper dbHelper;

		public IEnumerable<IDataRow> GetRelatedEntities(IDataRow data, Type storageType, Type storageRelatedType)
		{
			var pkColumn = SharedSQLBuilder.GetPKColumn(storageType);
			var fkColumns = SharedSQLBuilder.GetFKColumns(storageType, storageRelatedType);
			var relatedEntities = Enumerable.Empty<IDataRow>();
			foreach (var fkColumn in fkColumns)
			{
				relatedEntities = relatedEntities.Union(dbHelper.GetRecords(storageRelatedType, fkColumn, new[] { (Guid)data[pkColumn] }));
			}
			return relatedEntities;
		}

		IDataRow GetRelatedEntity(IDataRow data, string fkColumn, Type relatedStorateType)
		{
			Argument.NotNull(data, nameof(data));
			Argument.NotNullOrEmpty(fkColumn, nameof(fkColumn));
			Argument.NotNull(relatedStorateType, nameof(relatedStorateType));

			var pk = data[fkColumn];
			var relatedEntityPkColumn = SharedSQLBuilder.GetPKColumn(relatedStorateType);
			return dbHelper.GetRecords(relatedStorateType, relatedEntityPkColumn, new[] { (Guid)pk }).FirstOrDefault();
		}

		public object GetValue(IDataRow data, Type storageTypeName, Type valueType, string propertyName)
		{
			var propertyInfo = storageTypeName.GetProperty(propertyName);
			if (propertyInfo != null)
			{
				return GetValue(valueType, propertyInfo, data);
			}
			var propertyNameWithoutNK = propertyName.Replace("NK", string.Empty);
			var propertyWithoutNK = storageTypeName.GetProperty(propertyNameWithoutNK);
			if (propertyWithoutNK != null && !typeof(Guid?).IsAssignableFrom(propertyWithoutNK.PropertyType))
			{
				return GetValue(valueType, propertyWithoutNK, data);
			}
			var splits = propertyName.Split('_');
			if (splits.Length > 2 && splits[splits.Length - 1].StartsWith("NK", StringComparison.OrdinalIgnoreCase))
			{
				Type relatedType = GetTypeFromTblPrefix(storageTypeName, splits[1]);
				if (relatedType != null)
				{
					var fkColumn = SharedSQLBuilder.GetFKColumns(relatedType, storageTypeName).FirstOrDefault();
					if (!string.IsNullOrEmpty(fkColumn))
					{
						var relatedEntity = GetRelatedEntity(data, fkColumn, relatedType);
						if (relatedEntity != null)
						{
							var relatedPropertyName = propertyName.Substring(splits[0].Length + 1);
							return GetValue(relatedEntity, relatedType, valueType, relatedPropertyName);
						}
					}
				}
			}

			return null;
		}

		object GetValue(Type valueType, PropertyInfo propertyInfo, IDataRow data)
		{
			Argument.NotNull(valueType, nameof(valueType));
			Argument.NotNull(propertyInfo, nameof(propertyInfo));
			Argument.NotNull(data, nameof(data));

			if (valueType.IsAssignableFrom(propertyInfo.PropertyType))
			{
				return data[propertyInfo.Name];
			}
			else if (valueType.Equals(typeof(string)) && typeof(bool?).IsAssignableFrom(propertyInfo.PropertyType))
			{
				var boolVal = (bool?)data[propertyInfo.Name];
				var isIndex = propertyInfo.Name.IndexOf("_Is", StringComparison.OrdinalIgnoreCase);
				return boolVal.HasValue && boolVal.Value ? propertyInfo.Name.Substring(isIndex + 3) : null;
			}
			throw new NotSupportedException(FormattableString.Invariant($"Cannot convert value of type {propertyInfo.PropertyType} to type {valueType}"));
		}

		Type GetTypeFromTblPrefix(Type storageType, string tblPrefix)
		{
			return storageType.Assembly
				.DefinedTypes?.FirstOrDefault(x => typeof(IDataSetStorage).IsAssignableFrom(x) &&
				x.GetProperty(tblPrefix + "_PK") != null);
		}

		public Type GetStorageTypeFromName(string name)
		{
			var result = typeof(IZZRefCusCodeListCombined).Assembly
				.DefinedTypes?.FirstOrDefault(x => typeof(IDataSetStorage).IsAssignableFrom(x) &&
				x.Name.Equals("I" + name));
			return result;
		}
	}
}
