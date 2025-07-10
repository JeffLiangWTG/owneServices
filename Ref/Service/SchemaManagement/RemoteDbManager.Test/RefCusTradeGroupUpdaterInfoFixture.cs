using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusTradeGroupUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusTradeGroupUpdaterInfo(DbSchema dbSchema)
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

INSERT RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
VALUES (NEWID(), 'WW', 'WWW')

INSERT RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping) 
VALUES ('345F5663-4AB1-4F53-8405-98DD6B2CFFA1', 'AAA', 'AAAA','1900-01-01','2079-06-06', 'ZA'),
('6CC699A9-137C-4843-8E0E-E89CA9692EF5', 'BBB', 'BBBB','1900-01-01','2079-06-06', 'ZA'),
('BC766697-B57B-48B2-ADA7-1DAB147F9AE6', 'DDD', 'DDDD','1900-01-01','2079-06-06', 'ZA')

INSERT RefCusTradeGroupCountry (ZZB_PK,ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate,ZZB_Description)
VALUES (NEWID(), '345F5663-4AB1-4F53-8405-98DD6B2CFFA1', 'AA', '1900-01-01','2079-06-06', 'AAA'),
(NEWID(), '6CC699A9-137C-4843-8E0E-E89CA9692EF5', 'BB', '1900-01-01','2079-06-06', 'BBB'),
(NEWID(), 'BC766697-B57B-48B2-ADA7-1DAB147F9AE6', 'DD', '1900-01-01','2079-06-06', 'DDD')", trans);
					var info = new RefCusTradeGroupUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT #TempRefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping,Deleted) 
VALUES ('345F5663-4AB1-4F53-8405-98DD6B2CFFA9', 'AAA', 'XXXX','1900-01-01','2079-06-06', 'ZA', 0),
('6CC699A9-137C-4843-8E0E-E89CA9692EF9', 'BBB', 'BBBB','1900-01-01','2079-06-06', 'ZA',1),
('BC766697-B57B-48B2-ADA7-1DAB147F9AE9', 'CCC', 'CCCC','1900-01-01','2079-06-06', 'ZA', 0),
(NEWID(), 'DDD', 'DDDD','1900-01-01','2079-06-06', 'ZA', 0)

INSERT #TempRefCusTradeGroupCountry (ZZB_PK,ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate,ZZB_Description)
VALUES (NEWID(), '345F5663-4AB1-4F53-8405-98DD6B2CFFA9', 'AA', '1900-01-01','2079-06-06', 'YYY'),
(NEWID(), 'BC766697-B57B-48B2-ADA7-1DAB147F9AE9', 'CC', '1900-01-01','2079-06-06', 'CCC')
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroup WHERE ZZA_TradeGroup = 'AAA' AND ZZA_Description = 'XXXX'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroup WHERE ZZA_TradeGroup = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroup WHERE ZZA_TradeGroup = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroup WHERE ZZA_TradeGroup = 'DDD'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroupCountry WHERE ZZB_Description = 'YYY'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroupCountry WHERE ZZB_Description = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroupCountry WHERE ZZB_Description = 'CCC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroupCountry WHERE ZZB_Description = 'DDD'", trans));
				}
			}
		}
	}
}
