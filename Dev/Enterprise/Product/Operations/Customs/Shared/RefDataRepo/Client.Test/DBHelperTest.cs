using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Providers.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using Moq;
using NUnit.Framework;
using SqlBulkCopyOptions = CargoWise.Data.Providers.Common.SqlBulkCopyOptions;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class DBHelperTest : TransactionedTestCase
	{
		public void TestGetAllUniqueIndexes()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				((ICurrentDbControl)connection).UseDatabase(RefDbTableNameResolver.SingleRefDatabaseName);
				var myHelper = new DBHelper(((IDbConnectionInternals)connection).ADOConnection, bulkCopyProvider.Object);
				using (var tran = myHelper.BeginTransaction())
				{
					var indexColumnsArray = myHelper.GetAllUniqueIndexes(tran, nameof(RefCusNomenclatureGroup));
					AssertEquals(2, indexColumnsArray.Count());
					var indexWithValue = indexColumnsArray.Single(x => x.Length == 5).Select(x => x.Column).ToArray();
					var indexWithoutValue = indexColumnsArray.Single(x => x.Length == 4).Select(x => x.Column).ToArray();
					AssertArrayEqualsByElements(new[] { "ZZ5_ZZZ_NKDataGrouping", "ZZ5_ZZ9_NKNomenclatureGroupType", "ZZ5_StartDate", "ZZ5_CompositeKey", "ZZ5_Value" }, indexWithValue);
					AssertArrayEqualsByElements(new[] { "ZZ5_ZZZ_NKDataGrouping", "ZZ5_ZZ9_NKNomenclatureGroupType", "ZZ5_StartDate", "ZZ5_CompositeKey" }, indexWithoutValue);
				}
			}
		}

		public void TestGetAllUniqueIndexesWithFilter()
		{
			var indexColumnsArray = dbHelper.GetAllUniqueIndexes<IRefShippingLine>(Transaction).ToArray();
			AssertEquals(3, indexColumnsArray.Length);
			AssertEquals("RSL_CargoWiseOneCode", indexColumnsArray.First(x => x.All(y => y.Column == "RSL_CargoWiseOneCode")).Select(x => x.Column).First());
			AssertEquals("RSL_CarrierName", indexColumnsArray.First(x => x.All(y => y.Column == "RSL_CarrierName")).Select(x => x.Column).First());
			AssertEquals("RSL_StandardCarrierAlphaCode", indexColumnsArray.First(x => x.All(y => y.Column == "RSL_StandardCarrierAlphaCode")).Select(x => x.Column).First());
			AssertEquals("([RSL_StandardCarrierAlphaCode]<>'')", indexColumnsArray.First(x => x.All(y => y.Filter == "([RSL_StandardCarrierAlphaCode]<>'')")).Select(x => x.Filter).First());
		}

		public void TestGetAllUniqueIndexesWithLazy()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				((ICurrentDbControl)connection).UseDatabase(RefDbTableNameResolver.SingleRefDatabaseName);
				var myHelper = new DBHelper(((IDbConnectionInternals)connection).ADOConnection, bulkCopyProvider.Object);
				using (var tran = myHelper.BeginTransaction())
				{
					var columnsArray = myHelper.GetAllUniqueIndexes(tran, nameof(RefCusNomenclatureGroup));
					AssertEquals(2, columnsArray.Count());

					var sql = $@"
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefCusNomenclatureGroup_NKDataGrouping_NKNomenclatureGroupType_StartDate_EndDate_CompositeKey_Value] ON [dbo].[{nameof(RefCusNomenclatureGroup)}]
(
	[ZZ5_ZZZ_NKDataGrouping] ASC,
	[ZZ5_ZZ9_NKNomenclatureGroupType] ASC,
	[ZZ5_StartDate] ASC,
	[ZZ5_EndDate] ASC,
	[ZZ5_CompositeKey] ASC,
	[ZZ5_Value] ASC
)";
					myHelper.ExecuteNonQuery(sql, tran, null);

					var count = 0;
					using (var reader = myHelper.ExecuteReader($"EXEC sys.sp_helpindex @objname = N'{nameof(RefCusNomenclatureGroup)}'", transaction: tran))
					{
						while (reader.Read())
						{
							var name = (string)reader[0];
							if (name.StartsWith("IX"))
							{
								count++;
							}
						}
					}
					AssertEquals(3, count);

					columnsArray = myHelper.GetAllUniqueIndexes(tran, nameof(RefCusNomenclatureGroup));
					AssertEquals(2, columnsArray.Count());
				}
			}
		}

		public void TestCodeShouldBeRemovedIfFKInDatabase()
		{
			var sql = $@"SELECT  obj.name AS FK_NAME,
	tab1.name AS [table],
	tab2.name AS [referenced_table]
FROM sys.foreign_key_columns fkc
INNER JOIN sys.objects obj
	ON obj.object_id = fkc.constraint_object_id
INNER JOIN sys.tables tab1
	ON tab1.object_id = fkc.parent_object_id
INNER JOIN sys.schemas sch
	ON tab1.schema_id = sch.schema_id
INNER JOIN sys.columns col1
	ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id
INNER JOIN sys.tables tab2
	ON tab2.object_id = fkc.referenced_object_id
INNER JOIN sys.columns col2
	ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id
WHERE tab1.name = '{SharedSQLBuilder.GetTableNameForOffline(typeof(RefCusTaxOrFee))}' AND tab2.name = '{SharedSQLBuilder.GetTableNameForOffline(typeof(RefCusTaxOrFeeType))}'";

			var count = 0;
			using (var reader = dbHelper.ExecuteReader(sql, transaction: Transaction))
			{
				while (reader.Read())
				{
					count++;
				}
			}

			Assert("You must delete the code in DBHelper.cs class, method GetReferencedForeignKeys with the //re-visit comment. You must delete this test as well.", count == 0);
		}

		public void TestLoadAndSaveDbExtendedProperty()
		{
			dbHelper.SaveDbExtendedProperty("Test", "T", Transaction);
			var result = dbHelper.LoadDbExtendedProperty("Test", Transaction);
			AssertEquals("T", result);
		}

		public void TestGetUniqueConstraintColumnsSqlWithFilteredIndex()
		{
			TestConnection.ExecuteNonQuery(@"
CREATE TABLE [dbo].[FilteredIndexTestTable](
	[PK] [uniqueidentifier] NOT NULL default newid(),
	[FilterField] [varchar](1) NOT NULL default '') ON [PRIMARY]
");
			TestConnection.ExecuteNonQuery(@"
CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__FilterField] ON [dbo].[FilteredIndexTestTable]
([FilterField] ASC) WHERE ([FilterField]<>'');
");
			var query = SQLBuilder.GetUniqueConstraintColumnsSql("'FilteredIndexTestTable'");
			var result = TestConnection.ExecuteScalar($@"Select Count(*) from ({query}) testtable");
			TestConnection.ExecuteNonQuery("DROP INDEX NR_UX__FilterField on FilteredIndexTestTable");
			TestConnection.ExecuteNonQuery("DROP TABLE FilteredIndexTestTable");
			AssertEquals(1, (int)result);
		}

		public void TestGetForeignKeysFromDbCaching()
		{
			dbHelper.SetAllStorageTypes(new Type[]
				{
					typeof(ITestTable)
				});
			IEnumerable<ForeignKeyRelationship> foreignKeys;
			Transaction.Commit();
			using (var transaction = dbHelper.BeginTransaction())
			{
				foreignKeys = dbHelper.GetReferencedForeignKeysFromDb(transaction).Where(x => x.Table == typeof(ITestTable));
				AssertEquals(0, foreignKeys.Count());

				var cmdNewTable = $@"CREATE TABLE TestTable (
	T1_PK UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
	T1_ZZ1 UNIQUEIDENTIFIER REFERENCES RefAirLine (RM_PK)
)";
				dbHelper.ExecuteNonQuery(cmdNewTable, transaction);
				transaction.Commit();
			}
			using (var transaction = dbHelper.BeginTransaction())
			{
				foreignKeys = dbHelper.GetReferencedForeignKeysFromDb(transaction).Where(x => x.Table == typeof(ITestTable));
				AssertEquals(0, foreignKeys.Count());
			}
			//maintain cache over new instances
			dbHelper = new DBHelper(((IDbConnectionInternals)TestConnection).ADOConnection, bulkCopyProvider.Object);
			using (var transaction = dbHelper.BeginTransaction())
			{
				foreignKeys = dbHelper.GetReferencedForeignKeysFromDb(transaction).Where(x => x.Table == typeof(ITestTable));
				AssertEquals(0, foreignKeys.Count());
			}

			dbHelper.ResetFKCache();
		}

		public void TestGetReferencedForeignKeys()
		{
			dbHelper.SetAllStorageTypes(null);
			Transaction.Commit();
			IEnumerable<ForeignKeyRelationship> fkRelationshipsFromDb;
			using (var transaction = dbHelper.BeginTransaction())
			{
				fkRelationshipsFromDb = dbHelper.GetReferencedForeignKeysFromDb(transaction);
			}
			var fkRelationships = dbHelper.GetReferencedForeignKeys();
			AssertEquals("ForeignKeyRelationships should be the same with database.", fkRelationshipsFromDb.Count(), fkRelationships.Count());
			foreach (var fkRelationship in fkRelationshipsFromDb)
			{
				Assert($"ForeignKeyRelationships should contain the foreign key relationship. Table: {fkRelationship.Table.Name}, Column: {fkRelationship.Column}, ReferencedTable: {fkRelationship.ReferencedTable.Name}, ReferencedColumn: {fkRelationship.ReferencedColumn}",
					fkRelationships.Any(x => x.Table == fkRelationship.Table && x.Column == fkRelationship.Column && x.ReferencedTable == fkRelationship.ReferencedTable && x.ReferencedColumn == fkRelationship.ReferencedColumn && x.IsNullable == fkRelationship.IsNullable && x.ColumnPartOfUniqueIndex == fkRelationship.ColumnPartOfUniqueIndex));
			}
		}

		public void TestGetFKColumn()
		{
			AssertEquals(nameof(IUserDummyDependentStorage.D2_D1), dbHelper.GetFKColumn(typeof(IDummyStorage), typeof(IDummyDependentStorage)));

			var property = typeof(RefTimeZoneSet).GetProperty(nameof(RefTimeZoneSet.RefTimeZoneStandardZone));
			var fkColumn = dbHelper.GetFKColumn(typeof(IRefTimeZoneSet), typeof(IRefTimeZone), property);
			AssertEquals(nameof(IRefTimeZoneSet.R3_R2_StandardZone), fkColumn);

			property = typeof(RefTimeZoneSet).GetProperty(nameof(RefTimeZoneSet.RefTimeZoneDaylightSavingZone));
			fkColumn = dbHelper.GetFKColumn(typeof(IRefTimeZoneSet), typeof(IRefTimeZone), property);
			AssertEquals(nameof(IRefTimeZoneSet.R3_R2_DaylightSavingZone), fkColumn);
		}

		public async void TestBulkInsertAsyncDoesNotReportTimeoutException()
		{
			var tableName = "RefTestTable";
			var table = new DataTable(tableName);
			table.Columns.Add("T1_PK", typeof(Guid));
			table.Columns.Add("T1_Code", typeof(string));
			table.Rows.Add(Guid.Parse("5A79076C-CC56-3A30-C69F-08DB72DD55BC"), "TEST");

			var sqlError = SqlExceptionBuilder.CreateSqlError(6522, byte.MaxValue, byte.MinValue, TestConnection.ServerName, "Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.", "", 0);
			var sqlException = SqlExceptionBuilder.CreateSqlException(SqlExceptionBuilder.CreateSqlErrorCollection(sqlError), new Win32Exception("The wait operation timed out"));
			var sqlBulkCopy = new Mock<ISqlBulkCopy>();
			sqlBulkCopy.SetupGet(x => x.ColumnMappings).Returns(new Dictionary<string, string>());
			sqlBulkCopy.Setup(x => x.WriteToServerAsync(It.IsAny<DataTable>())).Throws(sqlException);
			bulkCopyProvider.Setup(x => x.GetSqlBulkCopy(It.IsAny<SqlConnection>(), SqlBulkCopyOptions.Default, It.IsAny<SqlTransaction>())).Returns(sqlBulkCopy.Object);

			Exception exception = null;
			try
			{
				await dbHelper.BulkInsertAsync(tableName, table, Transaction);
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			AssertNotNull(exception);
			AssertType<RefApplicationException>(exception);
			AssertEquals(false, ((RefApplicationException)exception).ReportIssue);

			exception = null;
			sqlBulkCopy.SetupGet(x => x.ColumnMappings).Returns(new Dictionary<string, string>());
			try
			{
				await dbHelper.BulkInsertAsync(tableName, table, Transaction, tableName);
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			AssertNotNull(exception);
			AssertType<RefApplicationException>(exception);
			AssertEquals(false, ((RefApplicationException)exception).ReportIssue);
		}

		interface ITestTable : IDataSetStorage
		{
			Guid TT_PK { get; }
			Guid TT_ZZ1 { get; }
		}

		DBHelper dbHelper;
		Mock<ISqlBulkCopyProvider> bulkCopyProvider;
		IDbTransaction Transaction => ((IDbConnectionInternals)TestConnection).ADOTransaction;

		protected override void SetUp()
		{
			base.SetUp();
			bulkCopyProvider = new Mock<ISqlBulkCopyProvider>();
			dbHelper = new DBHelper(((IDbConnectionInternals)TestConnection).ADOConnection, bulkCopyProvider.Object);
		}

		protected override void TearDown()
		{
			base.TearDown();
			dbHelper.ExecuteNonQuery(@"IF EXISTS(SELECT 1 FROM sys.objects where name = 'TestTable')	DROP TABLE TestTable", Transaction);
		}
	}
}
