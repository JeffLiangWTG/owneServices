using System;
using System.Reflection;

namespace CargoWise.eHub.DataModel.Comparer.DBInfo
{
	public class ColumnInfo : IColumnInfo
	{
		public ColumnInfo(string sqlColumnName, string sqlTypeName, bool isNullable, PropertyInfo clrProperty = null, string referencedTableName = null)
		{
			const string maxTypeEnding = "(max)";

			ColumnName = sqlColumnName;
			TypeName = sqlTypeName.EndsWith(maxTypeEnding)
				? sqlTypeName.Substring(0, sqlTypeName.Length - maxTypeEnding.Length)
				: sqlTypeName;

			if (clrProperty != null)
			{
				ClrColumnType = clrProperty.PropertyType;
			}

			IsNullable = isNullable;

			ReferencedTableName = referencedTableName;
		}

		public string ColumnName { get; set; }

		public string TypeName { get; set; }

		public string ClrColumName { get; set; }

		public Type ClrColumnType { get; set; }

		public bool IsNullable { get; set; }

		public string ReferencedTableName { get; set; }

		public override string ToString()
		{
			return $"SqlColumnName: {ColumnName}, SqlTypeName: {TypeName}, ClrColumName: {ClrColumName}, ClrColumnType: {ClrColumnType}, IsNullable: {IsNullable}, ReferencedTableName: {ReferencedTableName}";
		}
	}
}
