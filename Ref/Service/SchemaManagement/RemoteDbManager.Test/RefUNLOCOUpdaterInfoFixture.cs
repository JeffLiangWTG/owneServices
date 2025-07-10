using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefUNLOCOUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefUNLOCOUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefUNLOCO (RL_PK, RL_Code)
VALUES (NEWID(), 'FREBO'),
(NEWID(), 'EEARM'),
(NEWID(), 'MARBA')


INSERT RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc)
VALUES (NEWID(), 'FREBO', '2020-03-29 01:00:00.000', '2020-10-25 01:00:00.000', 120),
(NEWID(), 'EEARM', '2022-10-30 01:00:00.000', '2023-03-26 01:00:00.000', 60)
", trans);

					var info = new RefUNLOCOUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (RL_PK, RL_Code, Deleted)
VALUES (NEWID(), 'FREBO', 0),
(NEWID(), 'EEARM', 1),
(NEWID(), 'MARBA', 0)

INSERT {info.PrepareTemporaryTablesScripts.LastOrDefault().Key} (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc)
VALUES (NEWID(), 'FREBO', '2020-03-29 01:00:00.000', '2020-10-25 01:00:00.000', 120),
(NEWID(), 'MARBA', '2022-10-30 01:00:00.000', '2023-03-26 01:00:00.000', 60)
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefUNLOCO WHERE RL_Code = 'FREBO'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefUNLOCO WHERE RL_Code = 'TTTTT'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefUNLOCO WHERE RL_Code = 'MARBA'", trans));
				}
			}
		}
	}
}
