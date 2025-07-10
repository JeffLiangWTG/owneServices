using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusMapUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusMapUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefCusMapType (ZZP_PK, ZZP_MapType, ZZP_Direction, ZZP_Description, ZZP_IsReadonly)
VALUES (NEWID(), 'A', 'BTH', 'AAA', 1)

INSERT RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa')

INSERT RefCusMap (ZZM_PK, ZZM_ZZP_NKMapType, ZZM_CW1orCommercialValue, ZZM_CustomsValue, ZZM_StartDate, ZZM_EndDate, ZZM_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'A', 'A', 'AAA', '2017/1/1', '2017/12/12', 'ZA'),
(NEWID(), 'A', 'B', 'BBB', '2017/1/1', '2017/12/12', 'ZA')", trans);
					var info = new RefCusMapUpdaterInfo_1();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZZM_PK, ZZM_ZZP_NKMapType, ZZM_CW1orCommercialValue, ZZM_CustomsValue, ZZM_StartDate, ZZM_EndDate, ZZM_ZZZ_NKDataGrouping, Deleted)
VALUES (NEWID(), 'A', 'A', 'AAA', '2016/1/1', '2017/12/12', 'ZA', 0),
(NEWID(), 'A', 'B', 'BBB', '2017/1/1', '2017/12/12', 'ZA', 1),
(NEWID(), 'A', 'C', 'CCC', '2017/1/1', '2017/12/12', 'ZA', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusMap WHERE ZZM_CW1orCommercialValue = 'C'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusMap WHERE ZZM_CW1orCommercialValue = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusMap WHERE ZZM_CW1orCommercialValue = 'A' AND ZZM_StartDate = '2016/1/1'", trans));
				}
			}
		}
	}
}
