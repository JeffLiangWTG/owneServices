using System;
using System.Data;
using System.Data.Odbc;
using System.IO;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.Test
{
	[TestFixture]
	[Platform("64-bit", Reason = "Only support 64 bit ODBC driver")]
	[Property("DAT:CapabilityRequirements", (int)RefDbRepoMachineCapabilityRequirements.CanConnectToOdbc)]
	public class OdbcHelperFixture
	{
		string emptyMdbFilePath;

		[TestCase("Dummy")]
		public void TestCreateColumnIndexesByTableName(string tableName)
		{
			using (var odbcConnection = OdbcConnectionHelper.GetOdbcConnection(emptyMdbFilePath))
			{
				odbcConnection.Open();
				var columnIndexes = OdbcHelper.CreateColumnIndexesByTableName(odbcConnection, tableName);
				Assert.True(columnIndexes.ContainsKey("ID"));
			}
		}

		[Test]
		public void TestSetColumnIndexesByDataTable()
		{
			using (var dummyTable = new DataTable("DummyTable"))
			{
				dummyTable.Columns.Add("ColumnOrdinal", typeof(int));
				dummyTable.Columns.Add("ColumnName", typeof(string));
				dummyTable.Rows.Add(1, "RefCountry");
				dummyTable.Rows.Add(2, "RefState");
				var columnIndexes = OdbcHelper.SetColumnIndexesByDataTable(dummyTable);
				Assert.True(columnIndexes.ContainsKey("RefCountry"));
				Assert.True(columnIndexes.ContainsKey("RefState"));
			}
		}

		[Test]
		public void TestGetStringValue()
		{
			using (var odbcConnection = OdbcConnectionHelper.GetOdbcConnection(emptyMdbFilePath))
			{
				odbcConnection.Open();
				var columnIndexes = OdbcHelper.CreateColumnIndexesByTableName(odbcConnection, "Dummy");
				using (var cmdMdb = new OdbcCommand($"SELECT TOP 1 * FROM Dummy", odbcConnection))
				using (var mdbReader = cmdMdb.ExecuteReader())
				{
					while (mdbReader.Read())
					{
						var stringData = OdbcHelper.GetStringValue(mdbReader, columnIndexes["StringData"], 2);
						Assert.AreEqual(2, stringData.Length);
						Assert.AreEqual("Re", stringData);
					}
				}
			}
		}

		[SetUp]
		public void SetUp()
		{
			emptyMdbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Empty.mdb");
		}
	}
}
