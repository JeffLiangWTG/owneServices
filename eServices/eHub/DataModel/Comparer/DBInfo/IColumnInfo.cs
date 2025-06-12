using System;

namespace CargoWise.eHub.DataModel.Comparer.DBInfo
{
	public interface IColumnInfo
	{
		string ColumnName { get; set; }

		string TypeName { get; set; }

		Type ClrColumnType { get; set; }

		bool IsNullable { get; set; }

		string ReferencedTableName { get; set; }
	}
}
