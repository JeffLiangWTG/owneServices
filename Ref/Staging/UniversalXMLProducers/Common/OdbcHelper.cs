using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common
{
	public static class OdbcHelper
	{
		public static string GetStringValue(OdbcDataReader mdbReader, int index, int maxLength)
		{
			Argument.NotNull(mdbReader, nameof(mdbReader));
			object value = mdbReader.GetValue(index);
			if (value is string)
			{
				string str = value as string;
				if (maxLength > 0 && str.Length > maxLength)
				{
					str = str.Substring(0, maxLength);
				}
				return str.Trim();
			}
			return string.Empty;
		}

		/// <param name="con">Odbc Connection</param>
		/// <param name="tableName">table name would be name of the first table when value is null </param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Dynamic table name")]
		public static Dictionary<string, int> CreateColumnIndexesByTableName(OdbcConnection con, string tableName = null)
		{
			Argument.NotNull(con, nameof(con));
			var schemaTableName = GetTableNameFromDB(con,tableName);
			if (string.IsNullOrEmpty(schemaTableName))
			{
				throw new ArgumentException(schemaTableName, $"No {schemaTableName} table found.");
			}
			using (var cmdMdb = new OdbcCommand($"SELECT TOP 1 * FROM [{schemaTableName}]", con))
			using (var mdbReader = cmdMdb.ExecuteReader())
			{
				var schemaTable = mdbReader.GetSchemaTable();
				var columnIndexes = SetColumnIndexesByDataTable(schemaTable);
				return columnIndexes;
			}
		}

		/// <param name="con">Odbc Connection</param>
		/// <param name="tableName">table name would be name of the first table when value is null</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static string GetTableNameFromDB(OdbcConnection con, string tableName = null)
		{
			Argument.NotNull(con, nameof(con));
			var tables = con.GetSchema("tables");
			var schemaTableName = string.Empty;
			bool isTableExist = false;
			foreach (DataRow row in tables.Rows)
			{
				var tableType = row["TABLE_TYPE"];
				if (tableType.ToString() == "SYSTEM TABLE")
				{
					continue;
				}
				schemaTableName = row["TABLE_NAME"].ToString();
				if (tableName == null || schemaTableName == tableName)
				{
					isTableExist = true;
					break;
				}
			}
			if (!isTableExist)
			{
				throw new ArgumentException(schemaTableName, $"No {schemaTableName} table found.");
			}
			return schemaTableName;
		}

		public static Dictionary<string, int> SetColumnIndexesByDataTable(DataTable schemaTable)
		{
			Argument.NotNull(schemaTable, nameof(schemaTable));
			Dictionary<string, int> columnIndexes = new Dictionary<string, int>();
			foreach (DataRow row in schemaTable.Rows)
			{
				var columnName = row["ColumnName"];
				var columnIndex = row["ColumnOrdinal"];
				columnIndexes.Add(columnName.ToString(), (int)columnIndex);
			}
			return columnIndexes;
		}
	}
}
