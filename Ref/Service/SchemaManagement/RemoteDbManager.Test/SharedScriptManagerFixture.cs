using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class SharedScriptManagerFixture
	{
		[Test]
		public void TestTableViewScriptDictionary()
		{
			var type = typeof(ITableScript);
			var types = type.Assembly.GetTypes()
				.Where(x => type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
				.ToList();
			foreach (var table in types)
			{
				var instance = Activator.CreateInstance(table);

				var dict = ((ITableScript)Activator.CreateInstance(table)).TableViewScriptDictionary;

				foreach (var key in dict.Keys)
				{
					Assert.IsTrue(dict[key].Contains(FormattableString.Invariant($"{SharedDbSchemaChange.TableViewVersionSuffix}{key}")));
				}
			}
		}

		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void TestAllTableColumnsAppearInAtLeastOneView(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			var exclusionTables = new[] { nameof(RefUNLOCO) };
			var exclusionColumns = new[] { "ZX5_DataSetId" };
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();

				var type = typeof(ITableScript);
				var types = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name == "CargoWise.RefDbRepo.RemoteDbManager").SelectMany(x => x.GetTypes())
					.Where(x => type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
					.ToList();
				foreach (var table in types)
				{
					var columnList = new List<string>();
					var instance = (ITableScript)Activator.CreateInstance(table);
					var dict = instance.TableViewScriptDictionary;

					var tableName = instance.TableName;

					if (exclusionTables.Contains(tableName))
					{
						continue;
					}
					var sql = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{tableName}'";

					using (var cmd = conn.CreateCommand())
					{
						cmd.CommandText = sql;
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								var columnName = reader["COLUMN_NAME"].ToString();
								if (exclusionColumns.Contains(columnName))
								{
									continue;
								}
								columnList.Add(columnName);
							}
						}
					}
					foreach (var column in columnList)
					{
						bool isFound = false;
						foreach (var kvp in dict)
						{
							if (kvp.Value.Contains(column))
							{
								isFound = true;
							}
						}
						Assert.IsTrue(isFound, $"{column} in table {tableName} cannot be found in any TableViews");
					}
				}
			}
		}
	}
}
