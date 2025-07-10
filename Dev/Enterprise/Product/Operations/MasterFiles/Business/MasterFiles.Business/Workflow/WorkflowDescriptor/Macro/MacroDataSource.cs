using System;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class MacroDataSource
	{
		public static class Constants
		{
			public const string Prefix = "_DataSource";
		}

		public static bool HasDataSourcePrefix(ZString fieldPath, out ZString dataSourceTypeName, out ZString fieldName)
		{
			dataSourceTypeName = ZString.Empty;
			fieldName = Utilities.TrimExpression(fieldPath);

			var fields = fieldName.Split('.');
			var hasDataSourcePrefix = fields.Any() && fields[0] == Constants.Prefix;
			if (hasDataSourcePrefix && fields.Length > 1)
			{
				dataSourceTypeName = fields[1];
				fieldName = string.Join(".", fields.Skip(2).ToArray());
			}
			return hasDataSourcePrefix;
		}

		public static ZString GetDataSourceError(ZString fieldPath, Type[] rootTypes, out Type dataSourceType, out ZString fieldName)
		{
			dataSourceType = null;
			var errorMessage = ZString.Empty;

			if (HasDataSourcePrefix(fieldPath, out var dataSourceTypeName, out fieldName))
			{
				if (dataSourceTypeName.IsEmpty)
				{
					errorMessage = EmptyDataSourceTypeError;
				}
				else if (!RootTypesIncludeDataSourceType(rootTypes, dataSourceTypeName, out dataSourceType))
				{
					errorMessage = GetDataSourceTypeNotFoundMessage(dataSourceTypeName);
				}
			}

			return errorMessage;
		}

		static bool RootTypesIncludeDataSourceType(Type[] rootTypes, ZString dataSourceTypeName, out Type type)
		{
			type = rootTypes.FirstOrDefault(x =>
			{
				var attribute = (AdditionalRootTypeAttribute)x.GetCustomAttribute(typeof(AdditionalRootTypeAttribute), true);
				return attribute != null && attribute.DataSourceTypeName == dataSourceTypeName;
			});
			return type != null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Text Not Translated")]
		public const string EmptyDataSourceTypeError = "The _DataSource is empty.";

		public static string GetDataSourceTypeNotFoundMessage(ZString dataSourceTypeName) => $"The _DataSource '{dataSourceTypeName}' is not found.";
	}
}
