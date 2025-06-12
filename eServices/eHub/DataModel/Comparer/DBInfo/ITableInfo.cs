using System;
using System.Collections.Generic;

namespace CargoWise.eHub.DataModel.Comparer.DBInfo
{
	public interface ITableInfo
	{
		string SchemaName { get; set; }

		string TableName { get; set; }

		string CombinedName { get; }

		Type ClrClassType { get; set; }

		IList<IColumnInfo> ColumnInfos { get; set; }
	}
}
