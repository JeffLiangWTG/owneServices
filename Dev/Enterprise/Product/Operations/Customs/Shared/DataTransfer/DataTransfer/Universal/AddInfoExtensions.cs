using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions
{
	public struct SchemaColumnAndMaxLength
	{
		public SchemaColumnAndMaxLength(SchemaColumn column, int maxLength)
		{
			this.Column = Argument.NotNull(column, "column");
			this.MaxLength = maxLength;
		}

		public readonly SchemaColumn Column;
		public readonly int MaxLength;
	}

	public static class AddInfoExtensions
	{
		public static void UpdateAddInfoFromString(this IAddInfoManager manager, string addInfoString)
		{
			if (manager != null)
			{
				var addInfo = manager.AddInfo;
				if (addInfo != null)
				{
					addInfo.UpdateAddInfoFromString(addInfoString);
				}
			}
		}

		public static void SetUpdateFromAddInfoSerialisationFlag(this IAddInfoManager manager, bool isEnabled)
		{
			if (manager != null)
			{
				var addInfo = manager.AddInfo;
				if (addInfo != null)
				{
					addInfo.IsUpdateRelatedPropertyInfoDisabled = !isEnabled;
				}
			}
		}

		public static void UpdateRelatedPropertyInfo(this IAddInfoManager manager)
		{
			if (manager != null)
			{
				var addInfo = manager.AddInfo;
				if (addInfo != null)
				{
					addInfo.UpdateRelatedPropertyInfo();
				}
			}
		}

		public static IReadOnlyDictionary<string, SchemaColumn> GetAddInfoChildSchemaDictionary(this IAddInfoChildSupporter addInfoChildSupporter)
		{
			var childForeignKeyColumn = addInfoChildSupporter.ChildForeignKeyColumn;
			var tableName = childForeignKeyColumn.TableName;
			return addInfoChildSupporter.Factory.GetCachedValue(tableName, () =>
			{
				var prefixLength = childForeignKeyColumn.ColumnPrefix.Length + 1;
				return childForeignKeyColumn.TableSchema.All.OrderBy(x => x.Name)
					.Where(x => x.ColumnType != SchemaColumnType.Guid && x.ColumnType != SchemaColumnType.Binary && !Schema.IsSystemColumn(x.Name)).ToDictionary(x => x.Name.Substring(prefixLength));
			});
		}

		public static Dictionary<string, SchemaColumn> GetAddInfoSchemaDictionary<T>(this BusinessObjectFactory factory, ITableSchema addInfoTableSchema)
			where T : BusinessObject
		{
			return GetAddInfoSchemaDictionary(factory, typeof(T), addInfoTableSchema);
		}

		public static Dictionary<string, SchemaColumnAndMaxLength> GetAddInfoSchemaWithMaxLengthDictionary(this BusinessObjectFactory factory, Type businessObjectType, ITableSchema addInfoTableSchema)
		{
			return GetAddInfoSchemaDictionary(factory, businessObjectType, addInfoTableSchema, (column, info) => new SchemaColumnAndMaxLength(column, info.MaxLength));
		}

		public static Dictionary<string, SchemaColumn> GetAddInfoSchemaDictionary(this BusinessObjectFactory factory, Type businessObjectType, ITableSchema addInfoTableSchema)
		{
			return GetAddInfoSchemaDictionary(factory, businessObjectType, addInfoTableSchema, (column, _) => column);
		}

		public static Dictionary<string, T> GetAddInfoSchemaDictionary<T>(this BusinessObjectFactory factory, Type businessObjectType, ITableSchema addInfoTableSchema, Func<SchemaColumn, ZPropertyInfo, T> getSchemaColumn)
		{
			return factory.GetCachedValue($"{businessObjectType.FullName}_{typeof(T).Name}_AddInfoDictionary", () =>
			{
				var result = new Dictionary<string, T>();
				var bizObj = factory.GetNull(businessObjectType);
				var addInfo = (bizObj as IAddInfoManager)?.AddInfo;

				foreach (SchemaColumn column in addInfoTableSchema.All)
				{
					var name = column.Name;
					if (!Schema.IsSystemColumn(name))
					{
						var info = bizObj.ZPropertyInfoHash.GetPropertySafe(name);
						if (info != null)
						{
							var originalKey = name.Substring(column.ColumnPrefix.Length + 1);
							var mappingKey = addInfo?.GetKey(name) ?? originalKey;

							if (result.ContainsKey(mappingKey))
							{
								ErrorReporter.ReportOnce($"Duplicate AddInfo MappingKey: {bizObj.GetType().FullName}-{name}-{info.Name}-{mappingKey}");
							}
							else if (getSchemaColumn != null)
							{
								try
								{
									result.Add(mappingKey, getSchemaColumn(column, info));
								}
								catch (Exception ex) when (!ex.IsCriticalException())
								{
									ErrorReporter.ReportOnce("AddInfoSchemaFailure", FormattableString.Invariant($"Error getting mapping for type [{businessObjectType.FullName}] for property [{name}]"), ex);
									throw;
								}
							}
						}
					}
				}

				return result;
			});
		}

		public static void Update(this Dictionary<ZString, ZString> addInfos, ZString key, IZType value)
		{
			if (addInfos != null)
			{
				if (value.IsEmpty)
				{
					addInfos.Remove(key);
				}
				else
				{
					if (addInfos.ContainsKey(key))
					{
						addInfos[key] = BaseAddInfo.GetStringRepresentation(value);
					}
					else
					{
						addInfos.Add(key, BaseAddInfo.GetStringRepresentation(value));
					}
				}
			}
		}

		public static Dictionary<ZString, ZString> GetAddInfos(this IColumnIndexer row, SchemaStringColumn addInfoColumn)
		{
			return AddInfoParser.CreateDictionaryWithAddInfoString(row.GetValue(addInfoColumn));
		}

		public static ZString GetValue(this Dictionary<ZString, ZString> addInfos, SchemaStringColumn column)
		{
			return GetValue<ZString>(addInfos, column);
		}

		public static ZDecimal GetValue(this Dictionary<ZString, ZString> addInfos, SchemaDecimalColumn column)
		{
			return GetValue<ZDecimal>(addInfos, column);
		}

		public static ZBool GetValue(this Dictionary<ZString, ZString> addInfos, SchemaBoolColumn column)
		{
			return GetValue<ZBool>(addInfos, column);
		}

		public static ZDateTime GetValue(this Dictionary<ZString, ZString> addInfos, SchemaDateTimeColumn column)
		{
			return GetValue<ZDateTime>(addInfos, column);
		}

		public static ZInt GetValue(this Dictionary<ZString, ZString> addInfos, SchemaIntColumn column)
		{
			return GetValue<ZInt>(addInfos, column);
		}

		public static ZShort GetValue(this Dictionary<ZString, ZString> addInfos, SchemaShortColumn column)
		{
			return GetValue<ZShort>(addInfos, column);
		}

		public static ZGuid GetValue(this Dictionary<ZString, ZString> addInfos, SchemaGuidColumn column)
		{
			return GetValue<ZGuid>(addInfos, column);
		}

		static TSource GetValue<TSource>(this Dictionary<ZString, ZString> addInfos, SchemaColumn column)
		{
			ZString value;
			if (!addInfos.TryGetValue(column.Name.Substring(column.ColumnPrefix.Length + 1), out value))
			{
				value = ZString.Empty;
			}
			return (TSource)ZDataType.ObjectToZType(typeof(TSource), value);
		}
		public static Dictionary<ZString, AddInfoDescAttribute> GetAddInfoDescAttribute(BusinessObject bizObj)
		{
			var prefixLength = bizObj.TablePrefix.Length + 1;
			var bizObjType = bizObj.GetType();
			return bizObj.Factory.GetCachedValue(bizObjType.FullName, () =>
			{
				var result = new Dictionary<ZString, AddInfoDescAttribute>();

				foreach (var property in bizObjType.GetProperties())
				{
					AddInfoDescAttribute[] attributes = (AddInfoDescAttribute[])property.GetCustomAttributes(typeof(AddInfoDescAttribute), true);
					if (attributes.Length > 1)
					{
						throw new ArgumentException(bizObjType.Name + "." + property.Name + " has too many 'AddInfoDescAttribute'; it should have one.");
					}
					else if (attributes.Length == 1)
					{
						result.Add(property.Name.Substring(prefixLength), attributes[0]);
					}
				}
				return result;
			});
		}
	}
}
