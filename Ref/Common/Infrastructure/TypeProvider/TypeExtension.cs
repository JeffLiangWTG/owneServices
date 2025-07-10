using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace CargoWise.RefDbRepo.Common.TypeProvider
{
	public static class TypeExtension
	{
		public static PropertyInfo GetPKPropertyInfo(this Type entityType)
		{
			Argument.Argument.NotNull(entityType, nameof(entityType));
			var propertyInfos = entityType.GetProperties();
			var pkPropertyInfo = propertyInfos.FirstOrDefault(propertyInfo => propertyInfo.Name.Equals("RVC_ParentPK", StringComparison.OrdinalIgnoreCase))
				?? propertyInfos.FirstOrDefault(propertyInfo => propertyInfo.Name.EndsWith("_PK", StringComparison.OrdinalIgnoreCase))
				?? propertyInfos.FirstOrDefault(propertyInfo => propertyInfo.Name.EndsWith("ID", StringComparison.OrdinalIgnoreCase));
			return pkPropertyInfo;
		}

		public static PropertyInfo GetDataSetPKPropertyInfo(this Type entityType)
		{
			Argument.Argument.NotNull(entityType, nameof(entityType));
			var result = entityType.GetProperties().FirstOrDefault(x => x.Name.EndsWith("_DataSetPK", StringComparison.OrdinalIgnoreCase))
				?? entityType.GetProperties().FirstOrDefault(x => x.Name.EndsWith("_ZZ1_Tariff", StringComparison.OrdinalIgnoreCase));
			return result;
		}

		public static PropertyInfo GetFKPropertyInfo(this Type entityType, Type relatedType)
		{
			Argument.Argument.NotNull(entityType, nameof(entityType));
			Argument.Argument.NotNull(relatedType, nameof(relatedType));
			return GetFKPropertyInfo(entityType, relatedType.GetTablePrefix());
		}

		public static PropertyInfo GetFKPropertyInfo(this Type entityType, string relatedTypeTblPrefix)
		{
			Argument.Argument.NotNull(entityType, nameof(entityType));
			Argument.Argument.NotNullOrEmpty(relatedTypeTblPrefix, nameof(relatedTypeTblPrefix));
			return entityType.GetProperties().FirstOrDefault(x => (x.PropertyType == typeof(Guid)
				|| x.PropertyType == typeof(Guid?))
				&& (x.Name.StartsWith(entityType.GetTablePrefix() + "_" + relatedTypeTblPrefix, StringComparison.OrdinalIgnoreCase)
				|| x.Name.StartsWith(entityType.GetTablePrefix() + "_ParentPK", StringComparison.OrdinalIgnoreCase)));
		}

		public static bool ContainsIsActiveColumn(this Type entityType)
		{
			Argument.Argument.NotNull(entityType, nameof(entityType));
			return entityType.GetProperties().FirstOrDefault(x => x.Name.EndsWith(IsActiveSuffix, StringComparison.OrdinalIgnoreCase)) != null;
		}

		/// <summary>
		/// Return if one type related to another
		/// </summary>
		/// <param name="entityType"></param>
		/// <param name="relatedType"></param>
		/// <param name="includeParentPK">True means if entity contains property ends with ParentPK, function will return true</param>
		/// <returns></returns>
		public static bool IsRelatedTo(this Type entityType, Type relatedType, bool includeParentPK)
		{
			if (entityType == relatedType)
			{
				return false;
			}
			var parentFK = GetNKorFKPropertyInfo(entityType, relatedType, includeParentPK);
			if (parentFK != null)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Return NK or FK property info relates to the type
		/// </summary>
		/// <param name="entityType"></param>
		/// <param name="relatedType"></param>
		/// <param name="includeParentPK">True means if entity contains property with ParentPK, funciton will return that property info</param>
		/// <returns></returns>
		public static PropertyInfo GetNKorFKPropertyInfo(this Type entityType, Type relatedType, bool includeParentPK)
		{
			Argument.Argument.NotNull(entityType, nameof(entityType));
			Argument.Argument.NotNull(relatedType, nameof(relatedType));
			return GetNKorFKPropertyInfo(entityType, relatedType.GetTablePrefix(), includeParentPK);
		}

		static PropertyInfo GetNKorFKPropertyInfo(this Type entityType, string relatedTypeTblPrefix, bool includeParentPK)
		{
			Argument.Argument.NotNull(entityType, nameof(entityType));
			Argument.Argument.NotNullOrEmpty(relatedTypeTblPrefix, nameof(relatedTypeTblPrefix));
			if (includeParentPK)
			{
				return entityType.GetProperties().FirstOrDefault(x => (x.PropertyType == typeof(Guid)
				|| x.PropertyType == typeof(Guid?) || x.PropertyType == typeof(string))
				&& (x.Name.StartsWith(entityType.GetTablePrefix() + "_" + relatedTypeTblPrefix, StringComparison.OrdinalIgnoreCase)
				|| x.Name.StartsWith(entityType.GetTablePrefix() + "_ParentPK", StringComparison.OrdinalIgnoreCase)));
			}
			else
			{
				return entityType.GetProperties().FirstOrDefault(x => (x.PropertyType == typeof(Guid)
				|| x.PropertyType == typeof(Guid?) || x.PropertyType == typeof(string))
				&& (x.Name.StartsWith(entityType.GetTablePrefix() + "_" + relatedTypeTblPrefix, StringComparison.OrdinalIgnoreCase)));
			}
		}

		public static PropertyInfo GetNavigationPropertyInfo(this Type entityType, Type relatedType)
		{
			Argument.Argument.NotNull(entityType, nameof(entityType));
			Argument.Argument.NotNull(relatedType, nameof(relatedType));

			return entityType.GetProperties().FirstOrDefault(x => x.PropertyType.IsAssignableFrom(relatedType));
		}

		public static PropertyInfo GetCollectionNavigationPropertyInfo(this Type entityType, Type relatedType)
		{
			Argument.Argument.NotNull(entityType, nameof(entityType));
			Argument.Argument.NotNull(relatedType, nameof(relatedType));

			var relatedEntityPropertyType = typeof(ICollection<>).MakeGenericType(relatedType);
			return entityType.GetProperties().FirstOrDefault(x => relatedEntityPropertyType.IsAssignableFrom(x.PropertyType));
		}

		public static IEnumerable<PropertyInfo> GetCollectionNavigationPropertyInfos(this Type entityType)
		{
			Argument.Argument.NotNull(entityType, nameof(entityType));

			var tablePrefix = entityType.GetTablePrefix();
			return entityType.GetProperties().Where(x =>
				!x.Name.StartsWith(tablePrefix, StringComparison.OrdinalIgnoreCase)
				&& typeof(IEnumerable).IsAssignableFrom(x.PropertyType));
		}

		public static Type GetElementTypeOfCollection(this Type collectionType)
		{
			Argument.Argument.NotNull(collectionType, nameof(collectionType));

			var genericCollectionInterface = collectionType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
			return genericCollectionInterface?.GetGenericArguments()[0];
		}

		public static string GetPKPropertyName(this Type entityType)
		{
			Argument.Argument.NotNull(entityType, nameof(entityType));
			var result = GetPKPropertyInfo(entityType)?.Name;
			return result;
		}

		public static string GetTablePrefix(this Type entityType)
		{
			Argument.Argument.NotNull(entityType, nameof(entityType));

			var pkColumn = GetPKPropertyName(entityType);
			var idx = -1;
			if (pkColumn.EndsWith("_PK", StringComparison.OrdinalIgnoreCase))
			{
				idx = pkColumn.IndexOf("_PK", StringComparison.OrdinalIgnoreCase);
			}
			else if (pkColumn.EndsWith("_ParentPK", StringComparison.OrdinalIgnoreCase))
			{
				idx = pkColumn.IndexOf("_ParentPK", StringComparison.OrdinalIgnoreCase);
			}
			else if (pkColumn.EndsWith("_ID", StringComparison.OrdinalIgnoreCase))
			{
				idx = pkColumn.IndexOf("_ID", StringComparison.OrdinalIgnoreCase);
			}
			var result = pkColumn.Substring(0, idx);
			return result;
		}

		public static Guid GetPKValue(this object obj)
		{
			Argument.Argument.NotNull(obj, nameof(obj));

			var pkProperty = obj.GetType().GetPKPropertyInfo();
			var result = pkProperty.GetValue(obj);
			return (Guid)result;
		}

		public static bool IsExpirableType(this Type entityType)
		{
			return entityType.GetProperties().Any(x => x.Name.EndsWith("StartDate", StringComparison.OrdinalIgnoreCase));
		}

		public static bool IsSystemType(this Type propertyType)
		{
			return propertyType.Namespace.Equals("System", StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsBoolType(this Type propertyType)
		{
			return propertyType == typeof(bool);
		}

		public static bool IsStringType(this Type propertyType)
		{
			return propertyType == typeof(string);
		}

		public static bool IsIntegerType(this Type propertyType)
		{
			return (propertyType == typeof(int)) || (propertyType == typeof(long));
		}

		public static bool IsNumericType(this Type propertyType)
		{
			return
				(propertyType == typeof(decimal)) ||
				(propertyType == typeof(float)) ||
				(propertyType == typeof(double));
		}

		public static bool IsDateTime(this Type propertyType)
		{
			return propertyType == typeof(DateTime);
		}

		public static bool IsNullable(this Type propertyType)
		{
			return propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(Nullable<>));
		}

		public static bool IsNonPersistent(this Type entityType)
		{
			Argument.Argument.NotNull(entityType, nameof(entityType));

			var customAttributes = entityType.GetCustomAttributes();
			return customAttributes.Any(x => x is NonPersistentObject);
		}

		public static object ChangeType(string value, Type targetType)
		{
			Argument.Argument.NotNull(targetType, nameof(targetType));

			object targetValue = null;
			if (targetType.IsNullable())
			{
				if (string.IsNullOrEmpty(value))
				{
					return null;
				}

				var underlyingType = Nullable.GetUnderlyingType(targetType);
				var underlyingValue = ChangeType(value, underlyingType);
				if (underlyingValue != null)
				{
					var genericArgs = targetType.GetGenericArguments();

					targetValue = Convert.ChangeType(underlyingValue, genericArgs.First(), CultureInfo.InvariantCulture);
				}
			}
			else
			{
				if (targetType == typeof(Guid))
				{
					Guid guid;
					if (Guid.TryParse(value, out guid))
					{
						targetValue = guid;
					}
					else
					{
						targetValue = Guid.NewGuid();
					}
				}
				else if (targetType == typeof(bool))
				{
					switch (value)
					{
						case "0":
							targetValue = false;
							break;
						case "1":
							targetValue = true;
							break;
						default:
							targetValue = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
							break;
					}
				}
				else if (targetType == typeof(Geometry))
				{
					targetValue = ConvertToGeometry(4326, value);
				}
				else if (targetType == typeof(byte[]))
				{
					targetValue = Convert.FromBase64String(value);
				}
				else
				{
					targetValue = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
				}
			}

			return targetValue;
		}

		public static Geometry ConvertToGeometry(int srid, string wellKnowText)
		{
			var reader = new WKTReader(NtsGeometryServicesProvider.GetGeometryServices(srid));
			return reader.Read(wellKnowText);
		}

		public static Type GetTypeFromBaseType(this Type baseType, string entityName)
		{
			Argument.Argument.NotNullOrEmpty(entityName, nameof(entityName));
			return baseType.Assembly
				.DefinedTypes?.FirstOrDefault(x => x.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));
		}

		const string IsActiveSuffix = "_IsActive";
	}
}
