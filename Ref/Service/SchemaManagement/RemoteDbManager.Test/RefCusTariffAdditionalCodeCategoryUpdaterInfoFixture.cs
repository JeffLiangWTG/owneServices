using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusTariffAdditionalCodeCategoryUpdaterInfoFixture
	{
		[TestCase(DbSchema.RemoteDbCollationCS)]
		[TestCase(DbSchema.RemoteDb)]
		[TransactionedTestCase]
		public void RefCusTariffAdditionalCodeCategoryUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa')

INSERT RefCusTariffAdditionalCodeCategory (ZY3_PK, ZY3_Category, ZY3_Description, ZY3_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'A', 'AAA', 'ZA'),
(NEWID(), 'B', 'BBB', 'ZA')", trans);
					var info = new RefCusTariffAdditionalCodeCategoryUpdaterInfo_1();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZY3_PK, ZY3_Category, ZY3_Description, ZY3_ZZZ_NKDataGrouping, Deleted)
VALUES (NEWID(), 'A', 'XXX', 'ZA', 0),
(NEWID(), 'B', 'BBB', 'ZA', 1),
(NEWID(), 'C', 'CCC', 'ZA', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAdditionalCodeCategory WHERE ZY3_Category = 'C'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAdditionalCodeCategory WHERE ZY3_Category = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTariffAdditionalCodeCategory WHERE ZY3_Category = 'A' AND ZY3_Description = 'XXX'", trans));
				}
			}
		}
	}
}
