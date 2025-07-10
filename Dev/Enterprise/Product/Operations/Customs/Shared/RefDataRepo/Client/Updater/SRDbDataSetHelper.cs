using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common;
using Microsoft.SqlServer.Types;
using SequentialGuid;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public static class SRDbDataSetHelper
	{
		public static DataTable CreateDataTable(string tableName, IDBHelper dBHelper)
		{
			var result = new DataTable(tableName);
			var sql = $"SELECT * FROM {tableName} WHERE 1=0";
			dBHelper.Fill(sql, result);
			return result;
		}

		public static void AddServerData(IEnumerable<DataTable> serverDataTables, DataTableMapping mapping, JsonObject dataSet)
		{
			var pkColumns = serverDataTables.ToDictionary(x => x.TableName, y => y.Columns.Cast<DataColumn>().FirstOrDefault(x => x.ColumnName.EndsWith("_PK")).ColumnName);
			AddServerDataCore(serverDataTables, pkColumns, mapping, dataSet, null, null);
		}

		static bool IsJTokenNull(JsonNode node)
		{
			return (node == null) ||
				   (node.GetValueKind() == JsonValueKind.Array && node.AsArray().Count == 0) ||
				   (node.GetValueKind() == JsonValueKind.Object && node.AsObject().Count == 0) ||
				   (node.GetValueKind() == JsonValueKind.Null);
		}

		static Guid AddServerDataCore(IEnumerable<DataTable> serverDataTables, Dictionary<string, string> pkColums, DataTableMapping mapping, JsonObject dataSet, string parentFKColumnName, Guid? parentPK)
		{
			var pk = Guid.Empty;
			if (dataSet != null)
			{
				var dataTable = serverDataTables.FirstOrDefault(x => x.TableName == mapping.TableName);
				var dataRow = dataTable.NewRow();
				pk = SequentialSqlGuidGenerator.Instance.NewGuid();
				dataRow[pkColums[dataTable.TableName]] = pk;
				if (!string.IsNullOrEmpty(parentFKColumnName))
				{
					dataRow[parentFKColumnName] = (Guid)parentPK;
				}

				foreach (var property in dataSet)
				{
					if (property.Value is JsonObject jsonObject && !IsJTokenNull(property.Value))
					{
						if (mapping.RelatedTableNames != null && mapping.RelatedTableNames.ContainsKey(property.Key))
						{
							var referenceMapping = mapping.RelatedTableNames[property.Key];
							var referencePK = AddServerDataCore(serverDataTables, pkColums, referenceMapping, jsonObject, null, null);
							var referenceFKColumn = mapping.RelatedFKColumnNames[property.Key];
							dataRow[referenceFKColumn] = referencePK;
						}
					}
					else if (property.Value is JsonArray jsonArray && (bool?)dataSet["Deleted"] != true && !IsJTokenNull(property.Value))
					{
						if (mapping.RelatedTableNames != null && mapping.RelatedTableNames.ContainsKey(property.Key))
						{
							var childMapping = mapping.RelatedTableNames[property.Key];
							var fkColumn = mapping.RelatedFKColumnNames.ContainsKey(property.Key) ? mapping.RelatedFKColumnNames[property.Key] : string.Empty;
							foreach (var childData in jsonArray)
							{
								if (childData is not JsonObject childJsonObject)
								{
									continue;
								}
								AddServerDataCore(serverDataTables, pkColums, childMapping, childJsonObject, fkColumn, pk);
							}
						}
					}
					else if (dataTable.Columns.Contains(property.Key))
					{
						var dataType = dataTable.Columns[property.Key].DataType;
						if (IsJTokenNull(property.Value))
						{
							dataRow[property.Key] = DBNull.Value;
						}
						else
						{
							dataRow[property.Key] = dataType == typeof(SqlGeography) ?
							SqlGeography.Parse(property.Value.ToString()) :
							JsonSerializer.Deserialize(property.Value, dataType);
						}
					}
				}
				dataTable.Rows.Add(dataRow);
			}
			return pk;
		}
	}
}
