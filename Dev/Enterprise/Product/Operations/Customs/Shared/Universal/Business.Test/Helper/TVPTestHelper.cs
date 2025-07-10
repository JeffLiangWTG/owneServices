using System.Collections.Generic;
using System.Data;
using CargoWise.Data;

namespace Enterprise.Customs.Universal.Testing.Helper
{
	class TVPTestHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static IEnumerable<TableTypesColumnInfo> GetTableTypesColumnInfoFromDb(string tableTypeName)
		{
			var result = new List<TableTypesColumnInfo>();
			var sql = $@"
				SELECT
					t.name   [TableTypeName]
					,SCHEMA_NAME(t.schema_id)   [SchemaName]
					,c.name   [ColumnName]
					,y.name   [DataType]
					,c.max_length   [MaxLength]
					,c.precision   [Precision]
				FROM
					sys.table_types t
					INNER JOIN sys.columns c on c.object_id = t.type_table_object_id
					INNER JOIN sys.types y ON y.user_type_id = c.user_type_id
				WHERE
					t.is_user_defined = 1
					AND t.is_table_type = 1
					AND SCHEMA_NAME(t.schema_id) + '.'  + t.name = '{tableTypeName}'";

			using (var command = Db.Connection.Command(sql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add(PopulateTableTypesColumnInfo(reader));
				}
			}

			return result;
		}

		static TableTypesColumnInfo PopulateTableTypesColumnInfo(IDataReader reader)
		{
			return new TableTypesColumnInfo { TableTypeName = reader.GetString(reader.GetOrdinal("TableTypeName")), SchemaName = reader.GetString(reader.GetOrdinal("SchemaName")), ColumnName = reader.GetString(reader.GetOrdinal("ColumnName")), DataType = reader.GetString(reader.GetOrdinal("DataType")), MaxLength = reader.GetInt16(reader.GetOrdinal("MaxLength")), Precision = reader.GetByte(reader.GetOrdinal("Precision")) };
		}

		public class TableTypesColumnInfo
		{
			public string TableTypeName
			{
				get;
				set;
			}

			public string SchemaName
			{
				get;
				set;
			}

			public string ColumnName
			{
				get;
				set;
			}

			public string DataType
			{
				get;
				set;
			}

			public int MaxLength
			{
				get;
				set;
			}

			public byte Precision
			{
				get;
				set;
			}
		}
	}
}
