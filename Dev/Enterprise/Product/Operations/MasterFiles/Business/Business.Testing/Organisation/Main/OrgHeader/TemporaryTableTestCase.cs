using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.ServiceTasks;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TemporaryTableTestCase : TestCase
	{
		const string ArbitrarySqlCode = "SELECT TOP 5 OH_PK FROM OrgHeader";

		public void TestTableLifespan()
		{
			const string tableName = "#myTempTable";
			using (var table = new TemporaryTableCache(Db.Connection, ArbitrarySqlCode, tableName))
			{
				AssertNotNull(table.Load("select * from <tablename>"));

				Assert("Table should exist", TableExists(Db.Connection, tableName));
			}

			Assert("Table should no longer exist", !TableExists(Db.Connection, tableName));
		}

		[UseSnapshotProtection]
		public void TestRowCountIsntCachingWhenItShouldnt()
		{
			using (var table = new TemporaryTableCache(Db.Connection, "SELECT TOP 5 * FROM RefCountryStates", "#myTempTable"))
			{
				AssertEquals("There should be 5 items in the table", 5, table.RowCount);
				AssertEquals("Should have deleted everything", 5, table.Load("DELETE FROM <tablename> OUTPUT deleted.RW_PK").Count);
				AssertEquals("There should be no more items in the table", 0, table.RowCount);
			}
		}

		public void TestTableIsCaching()
		{
			using (var table = new TemporaryTableCache(Db.Connection, "SELECT NEWID() AS PK", "#myTempTable"))
			{
				var firstResult = (ZGuid)table.Load("SELECT * FROM <tablename>").Select(dynamic => dynamic["PK"]).Single();
				var secondResult = (ZGuid)table.Load("SELECT * FROM <tablename>").Select(dynamic => dynamic["PK"]).Single();

				AssertEquals(firstResult, secondResult);
			}
		}

		public void TestTableIsNotGlobal()
		{
			const string tableName = "#myTempTable";
			using (var otherConnection = Db.NewExtraConnectionToMainDb())
			using (var table = new TemporaryTableCache(otherConnection, ArbitrarySqlCode, tableName))
			{
				AssertNotNull(table.Load("select * from <tablename>"));

				Assert(TableExists(otherConnection, tableName));
				Assert(!TableExists(Db.Connection, tableName));
			}
		}

		public void TestTempTablePerformsCorrectQuery()
		{
			using (var table = new TemporaryTableCache(Db.Connection, "SELECT * FROM RefCountryStates WHERE RW_Description LIKE 'NEW %'", "#myTempTable"))
			{
				var allStatesThatStartWithNew = table.Load("SELECT TOP 7 * FROM <tablename>")
					.Select(dynamic => dynamic["RW_Description"])
					.Cast<ZString>()
					.ToList();

				Assert("Should have results", allStatesThatStartWithNew.Count == 7);
				foreach (var state in allStatesThatStartWithNew)
				{
					AssertStartsWith("All should start with 'New '", "New ", state);
				}
			}
		}

		public void TestGetCount()
		{
			using (var table = new TemporaryTableCache(Db.Connection, "SELECT TOP 5 * FROM RefCountryStates", "#myTempTable"))
			{
				AssertEquals(5, table.RowCount);
			}
		}

		bool TableExists(DbConnection connection, string tableName)
		{
			using (var command = connection.Command(String.Format("SELECT OBJECT_ID('tempdb..{0}')", tableName)))
			{
				return command.ExecuteScalar() is int;
			}
		}
	}
}
