using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator.Test
{
	[TestFixture]
	class ClientStorageInterfacesAndTableColumnsInSync
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void TestAllInterfacesAndTablesAreSynchronisedForDb(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			var assemblyPath = Path.Combine(FolderHelper.GetBinFolder(), "CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator.dll");
			var typeIDataSetStorage = typeof(IDataSetStorage);
			var dataSetStorageTypes =
				Assembly.LoadFrom(assemblyPath)
				.GetTypes()
				.Where(x => x != typeIDataSetStorage && typeIDataSetStorage.IsAssignableFrom(x));
			var refDataSetStorageTypes = dataSetStorageTypes.Except(new[] { typeof(IStmNote) });

			using (var connection = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				connection.Open();
				ValidateDataSetStorageTypesAreInSyncWithDbColumns(refDataSetStorageTypes, connection);
			}
		}

		readonly Dictionary<Type, string[]> InterfacePropertiesIgnoreList = new Dictionary<Type, string[]>()
		{
			// Put interface and properties pair here if you want to skip the check
			// Make sure the Property specified exist for the corresponding Interface or it will fail the test
			// Eg.
			// { typeof(IUNDGAttribute), new string[] { "DA_PK", "DA_Language"} }  good
			// { typeof(IUNDGAttribute), new string[] { "DA_PK", "NOT EXIST"} }  bad
			// { typeof(IRefCusProfile), new string[] { "XX0_TariffCode" } }
		};

		readonly Dictionary<string, string[]> TableColumnIgnoreList = new Dictionary<string, string[]>()
		{
			// Put table and column pair here if you want to skip the check
			// Make sure the column specified exist for the corresponding table or it will fail the test
			// Eg.
			{ "RefCusVATApplicability", new string[] { "ZX5_ZZ1_ParentTariffOrNationalCode" } },
			{ "RefCusCodeListAttributeName", new string[] { "ZXE_ZZK_NKCodeTypeComputed" } },
			{ "RefCusCodeList", new string[] { "ZZD_ZZK_NKCodeTypeComputed" } },
			{ "RefCusCodeType", new string[] { "ZZK_CodeTypeComputed" } }
		};

		readonly IEnumerable<string> ColumnEndsWithPatternIgnoreList = new[]
		{
			// Put column "EndsWith" pattern here if you want to skip the check
			"_AutoVersion",
			"_SystemCreateTimeUtc",
			"_SystemCreateUser",
			"_SystemLastEditTimeUtc",
			"_SystemLastEditUser",
		};

		#region Helpers

		void ValidateDataSetStorageTypesAreInSyncWithDbColumns(IEnumerable<Type> dataSetStorageTypes, IDbConnection connection)
		{
			foreach (var type in dataSetStorageTypes)
			{
				try
				{
					ValidateDataSetStorageTypeIsInSyncWithDbColumns(type, connection);
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		void ValidateDataSetStorageTypeIsInSyncWithDbColumns(Type dataSetStorageType, IDbConnection connection)
		{
			var propertyNames = dataSetStorageType.GetProperties().Where(x => !x.PropertyType.Name.Contains("IEnumerable")).Select(x => x.Name);
			var tableName = dataSetStorageType.Name.Substring(1, dataSetStorageType.Name.Length - 1);
			var columnNames = GetAllColumnsForTable(tableName, connection);

			Assert.That(columnNames.Any(), $"Table {tableName} should have at least one column");

			string[] ignoredProperies = null;
			if (InterfacePropertiesIgnoreList.ContainsKey(dataSetStorageType))
			{
				ignoredProperies = InterfacePropertiesIgnoreList[dataSetStorageType];
			}

			if (ignoredProperies != null)
			{
				foreach (var propertyToIgnore in ignoredProperies)
				{
					AssertContainsWithErrorMsg(propertyToIgnore, propertyNames, "Check InterfacePropertiesIgnoreList to make sure the property name is specified correctly");
				}
			}

			string[] ignoredColumns = null;
			if (TableColumnIgnoreList.ContainsKey(tableName))
			{
				ignoredColumns = TableColumnIgnoreList[tableName];
			}
			if (ignoredColumns != null)
			{
				foreach (var ignoredColumn in ignoredColumns)
				{
					AssertContainsWithErrorMsg(ignoredColumn, columnNames, "Check TableColumnIgnoreList to make sure the column name is specified correctly");
				}
			}

			foreach (var interfaceProperty in propertyNames)
			{
				if (ignoredProperies != null && ignoredProperies.Contains(interfaceProperty))
				{
					continue;
				}

				AssertContainsWithErrorMsg(interfaceProperty, columnNames, $"The {interfaceProperty} exists in Interface but not in DB Column");
			}

			foreach (var dbColumn in columnNames)
			{
				if (ignoredColumns != null && ignoredColumns.Contains(dbColumn))
				{
					continue;
				}

				if (ColumnEndsWithPatternIgnoreList.Any(pattern => dbColumn.EndsWith(pattern)))
				{
					continue;
				}

				AssertContainsWithErrorMsg(dbColumn, propertyNames, $"The {dbColumn} exists in DB Column but not in Interface Properties of {dataSetStorageType.FullName}");
			}

			if (ignoredColumns != null)
			{
				foreach (var ignoredColumn in ignoredColumns)
				{
					if (propertyNames.Contains(ignoredColumn))
					{
						Assert.That(false, $"Please remove {ignoredColumn} from the TableColumnIgnoreList");
					}
				}
			}
		}

		IEnumerable<string> GetAllColumnsForTable(string tableName, IDbConnection connection)
		{
			var sql = $@"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='{tableName}'";

			using (var reader = connection.ExecuteReader(sql))
			{
				while (reader.Read())
				{
					yield return (string)reader[0];
				}
			}
		}

		void AssertContainsWithErrorMsg(string expected, IEnumerable<string> actual, string addtionalMsg = "")
		{
			Assert.That(actual.Contains(expected), $"[{string.Join(", ", actual)}] does not contain {expected};" + addtionalMsg);
		}

		#endregion
	}
}
