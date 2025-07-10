using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public class ClientStorageInterfacesAndTableColumnsInSync : TestCase
	{
		public void TestAllInterfacesAndTablesAreSynchronisedForDb()
		{
			var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
			var assemblyPath = Path.Combine(binFolder, "CargoWise.RefDataRepo.Ent.Client.dll");
			var typeIDataSetStorage = typeof(IDataSetStorage);
			var dataSetStorageTypes =
				Assembly.LoadFrom(assemblyPath)
				.GetTypes()
				.Where(x => x != typeIDataSetStorage && typeIDataSetStorage.IsAssignableFrom(x));

			var sharedAssemblyPath = Path.Combine(binFolder, "CargoWise.RefDbRepo.Client.Common.dll");
			var dataSetStorageTypes2 = Assembly.LoadFrom(sharedAssemblyPath).GetTypes()
				.Where(x => x != typeIDataSetStorage && typeIDataSetStorage.IsAssignableFrom(x));
			dataSetStorageTypes = dataSetStorageTypes.Union(dataSetStorageTypes2);
			var refDataSetStorageTypes = dataSetStorageTypes.Except(new[] { typeof(IUNDGCommonData), typeof(IZZUNDGSubstance), typeof(IOrgHeader), typeof(IStmNote) });
			if (Db.Connection.State == ConnectionState.Broken || Db.Connection.State == ConnectionState.Closed)
			{
				Db.Connection.EnsureIsOpen();
			}
			var odRefDbHelper = new DBHelper(((IDbConnectionInternals)Db.Connection).ADOConnection);
			ValidateDataSetStorageTypesAreInSyncWithDbColumns(refDataSetStorageTypes, odRefDbHelper);
		}

		readonly Dictionary<Type, string[]> interfacePropertiesIgnoreList = new Dictionary<Type, string[]>()
		{
			// Put interface and properties pair here if you want to skip the check
			// Make sure the Property specified exist for the corresponding Interface or it will fail the test
			// Eg. 
			// { typeof(IUNDGAttribute), new string[] { "DA_PK", "DA_Language"} }  good
			// { typeof(IUNDGAttribute), new string[] { "DA_PK", "NOT EXIST"} }  bad
		};

		readonly Dictionary<string, string[]> tableColumnIgnoreList = new Dictionary<string, string[]>()
		{
			// Put table and column pair here if you want to skip the check
			// Make sure the column specified exist for the corresponding table or it will fail the test
			// Eg. 
			//{ "RefCusTariffUOM", new string[] { "ZZ8_ZZZ_NKDataGrouping" } } good
			//{ "RefCusTariffUOM", new string[] { "Wheee NOT EXIST" } } bad
			{ "RefUNLOCO", new string[] { "RL_IsUpdatable" } },
			{ "RefTimeZone", new string[] { "R2_OffsetFromUtc" } },
			{ "RefUNLOCOUtcOffset", new string[] { "RLO_OffsetFromUtc" } },
			{ "RefLanguageText", new string[] { "RLT_IsClientOverridden" } },
			// *These RefAirline properties are user-entered data. DO NOT accidentally add it to the interface.
			{ "RefAirline", new string[] { "RM_LabelShortName", "RM_IsCASSControlled", "RM_ContactNameOCIIdentifier", "RM_ContactPhoneOCIIdentifier", "RM_ReservationsContactName", "RM_ReservationsContactTitle", "RM_EmergencyTeletype", "RM_ReservationsDeptTeletype", "RM_ReservationsContactTeletype", "RM_EmergencyContactName" , "RM_EmergencyContactTitle", "RM_AccountingSecondaryFlag", "RM_AirlinePrefixSecondaryFlag", "RM_IsUpdatable" } },
			{ "RefComplianceList", new string[] { "RCL_IsExcluded" } },
			{ "RefCountry", new string[] { "RN_IsSanctioned" } },
			{ "RefFacility", new string[] { "RFT_IsValid" } },
			{ "RefFacilityLocalCode", new string[] { "RFL_IsValid" } },
			{ "RefComplianceCommodityAlert", new string[] { "RCR_CommodityRiskStatus" } },
			{ "UNDGCountryReferencePivot", new string[] { "DCP_StorageInstruction", "DCP_TankStorageInstructionRetentionTray" } },
			{ "UNDGAttribute", new string[] { "DA_Standard", "DA_UNNO", "DA_Variant" } },
		};

		readonly IEnumerable<string> columnEndsWithPatternIgnoreList = new[]
		{
			// Put column "EndsWith" pattern here if you want to skip the check
			"_RowVersion",
			"_AutoVersion",
			"_SystemCreateTimeUtc",
			"_SystemCreateUser",
			"_SystemCreateBranch",
			"_SystemCreateDepartment",
			"_SystemLastEditTimeUtc",
			"_SystemLastEditUser",
		};

		#region Helpers

		void ValidateDataSetStorageTypesAreInSyncWithDbColumns(IEnumerable<Type> dataSetStorageTypes, DBHelper dbHelper)
		{
			foreach (var type in dataSetStorageTypes)
			{
				ValidateDataSetStorageTypeIsInSyncWithDbColumns(type, dbHelper);
			}
		}

		void ValidateDataSetStorageTypeIsInSyncWithDbColumns(Type dataSetStorageType, IDBHelper dbHelper)
		{
			var propertyNames = dataSetStorageType.GetProperties().Where(x => !x.PropertyType.Name.Contains("IEnumerable")).Select(x => x.Name);
			var tableName = SharedSQLBuilder.GetTableName(dataSetStorageType);
			var columnNames = GetAllColumnsForTable(tableName, dbHelper);

			Assert($"Table {tableName} should have at least one column", columnNames.Any());

			string[] ignoredProperies = null;
			if (interfacePropertiesIgnoreList.ContainsKey(dataSetStorageType))
			{
				ignoredProperies = interfacePropertiesIgnoreList[dataSetStorageType];
			}

			if (ignoredProperies != null)
			{
				foreach (var propertyToIgnore in ignoredProperies)
				{
					AssertContainsWithErrorMsg(propertyToIgnore, propertyNames, "Check InterfacePropertiesIgnoreList to make sure the property name is specified correctly");
				}
			}

			string[] ignoredColumns = null;
			if (tableColumnIgnoreList.ContainsKey(tableName))
			{
				ignoredColumns = tableColumnIgnoreList[tableName];
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

				if (columnEndsWithPatternIgnoreList.Any(pattern => dbColumn.EndsWith(pattern)))
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
						Assert($"Please remove {ignoredColumn} from the TableColumnIgnoreList", false);
					}
				}
			}
		}

		IEnumerable<string> GetAllColumnsForTable(string tableName, IDBHelper dbHelper)
		{
			var sql = $@"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='{tableName}'";

			using (var reader = dbHelper.ExecuteReader(sql))
			{
				while (reader.Read())
				{
					yield return (string)reader[0];
				}
			}
		}

		void AssertContainsWithErrorMsg(string expected, IEnumerable<string> actual, string addtionalMsg = "")
		{
			Assert($"[{string.Join(", ", actual)}] does not contain {expected};" + addtionalMsg, actual.Contains(expected));
		}

		#endregion
	}
}
