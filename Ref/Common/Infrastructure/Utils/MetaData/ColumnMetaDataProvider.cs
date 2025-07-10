using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Common.Utils;

public class ColumnMetaDataProvider(string connectionString, string dbName) : IColumnMetaDataProvider, IDisposable
{
	public List<ColumnMetaData> GetMetaData(string objectName)
	{
		try
		{
			return GetMetaDataCore(objectName);
		}
		catch (SqlException ex)
		{
			Console.Error.WriteLine(ex.Message);
			return [];
		}
	}

	List<ColumnMetaData> GetMetaDataCore(string objectName)
	{
		var columnDefinitions = new List<ColumnMetaData>();

		connection.OpenIfNeeded();
		connection.ChangeDatabaseIfNeeded(dbName);
		using var cmd = CreateMetaDataCommand(objectName);
		using var reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			columnDefinitions.Add(MapColumnMetaData(reader));
		}

		return columnDefinitions;
	}

	SqlCommand CreateMetaDataCommand(string objectName)
	{
		var cmd = connection.CreateCommand();
		cmd.CommandText = """
						SELECT
						    c.name AS ColumnName,
						    t.name AS DataType,
						    c.max_length AS ColumnSize,
						    c.precision AS NumericPrecision,
						    c.scale AS NumericScale,
						    c.is_nullable AS IsNullable
						FROM sys.columns c
						JOIN sys.types t ON c.user_type_id = t.user_type_id
						WHERE c.object_id = OBJECT_ID(@ViewName)
						ORDER BY c.column_id
						""";
		cmd.Parameters.AddWithValue("@ViewName", objectName);
		return cmd;
	}

	static ColumnMetaData MapColumnMetaData(SqlDataReader reader)
	{
		return new ColumnMetaData
		{
			ColumnName = reader.GetString(0),
			DataType = reader.GetString(1),
			ColumnSize = reader.GetInt16(2),
			NumericPrecision = reader.GetByte(3),
			NumericScale = reader.GetByte(4),
			IsNullable = reader.GetBoolean(5)
		};
	}

	public void Dispose()
	{
		connection.Dispose();
	}

	readonly SqlConnection connection = new(connectionString);
}
