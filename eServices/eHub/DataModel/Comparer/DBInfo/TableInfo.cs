using System;
using System.Collections.Generic;

namespace CargoWise.eHub.DataModel.Comparer.DBInfo
{
	public class TableInfo : ITableInfo
	{
		public TableInfo(string tableName, string schemaName, Type clrClassType, IList<IColumnInfo> columnInfos)
		{
			TableName = tableName;
			SchemaName = schemaName;
			ColumnInfos = columnInfos;
			ClrClassType = clrClassType;
		}

		public string SchemaName { get; set; }

		public string TableName { get; set; }

		public string CombinedName { get { return string.Format("[{0}].[{1}]", SchemaName, TableName); } }

		public Type ClrClassType { get; set; }

		public IList<IColumnInfo> ColumnInfos { get; set; }

		public override string ToString()
		{
			return string.Format("Name: {0}.{1}, NormalCols: {2}", SchemaName, TableName, ColumnInfos.Count);
		}
	}
}
