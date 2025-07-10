using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusProcedureUpdaterInfoFixture
	{
		[TestCase(DbSchema.RemoteDbCollationCS)]
		[TestCase(DbSchema.RemoteDb)]
		[TransactionedTestCase]
		public void RefCusProcedureUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping)
VALUES(NEWID(), 'ZA', 'South Africa', NULL)

INSERT INTO RefLanguageType(ZX6_PK, ZX6_Description, ZX6_Language)
VALUES(NEWID(), 'English', 'EN')

INSERT INTO RefCusProcedure (ZZ6_PK,ZZ6_Category,ZZ6_ProcedureCode,ZZ6_PreviousProcedureCode,ZZ6_Concession,ZZ6_Description,ZZ6_ZZZ_NKDataGrouping
,ZZ6_ShipmentType,ZZ6_CalculateDuty,ZZ6_Group,ZZ6_LandedCost,ZZ6_IntoWarehouse,ZZ6_OutOfWarehouse,ZZ6_StartDate,ZZ6_EndDate, ZZ6_CalculateVAT, ZZ6_IsGuaranteeConsumed, ZZ6_IsGuaranteeReleased
,ZZ6_IntoTemporaryImport, ZZ6_OutOfTemporaryImport, ZZ6_IntoTemporaryExport, ZZ6_OutOfTemporaryExport, ZZ6_IsTransit, ZZ6_IntoInwardProcessing, ZZ6_OutOfInwardProcessing, ZZ6_IntoOutwardProcessing, ZZ6_OutofOutwardProcessing)
VALUES ('1799FE44-08BE-42CF-B130-E89921BC3E85','A','AAA','15','4','ZZZ','ZA','EXW',1,'',0,'N','Y','1900-01-01','2079-06-06 23:59:00', 1, 'N', 'Y', 'N', 'N', 'N', 'N', 'Y', 'N', 'N', 'N', 'N'),
('D272AC2C-C3CB-4981-851E-764899C55D73','X','BBB','15','4','ZZZ','ZA','EXW',1,'',0,'N','Y','1900-01-01','2079-06-06 23:59:00', 1, 'N', 'Y', 'N', 'N', 'N', 'N', 'Y', 'N', 'N', 'N', 'N')

INSERT INTO RefCusProcedureAttribute (ZXB_PK,ZXB_ZZ6_ProcedureCode,ZXB_Name,ZXB_Value)
VALUES (NEWID(),'1799FE44-08BE-42CF-B130-E89921BC3E85','AA','AA'),
(NEWID(),'D272AC2C-C3CB-4981-851E-764899C55D73','BB','BB')

INSERT INTO RefCusProcedureLanguage (ZXV_PK,ZXV_ZZ6_Procedure,ZXV_ZX6_NKLanguage,ZXV_Description)
VALUES (NEWID(), '1799FE44-08BE-42CF-B130-E89921BC3E85', 'EN', 'AAA'),
(NEWID(), 'D272AC2C-C3CB-4981-851E-764899C55D73', 'EN', 'BBB')
", trans);
					var info = new RefCusProcedureUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO #TempRefCusProcedure (ZZ6_PK,ZZ6_Category,ZZ6_ProcedureCode,ZZ6_PreviousProcedureCode,ZZ6_Concession,ZZ6_Description,ZZ6_ZZZ_NKDataGrouping
,ZZ6_ShipmentType,ZZ6_CalculateDuty,ZZ6_Group,ZZ6_LandedCost,ZZ6_IntoWarehouse,ZZ6_OutOfWarehouse,ZZ6_StartDate,ZZ6_EndDate, ZZ6_CalculateVAT, ZZ6_IsGuaranteeConsumed, ZZ6_IsGuaranteeReleased
,ZZ6_IntoTemporaryImport, ZZ6_OutOfTemporaryImport, ZZ6_IntoTemporaryExport, ZZ6_OutOfTemporaryExport, ZZ6_IsTransit, ZZ6_IntoInwardProcessing, ZZ6_OutOfInwardProcessing, ZZ6_IntoOutwardProcessing, ZZ6_OutofOutwardProcessing, Deleted)
VALUES (NEWID(),'A','AAA','15','4','ZZZ','ZA','EXW',1,'',0,'N','Y','1900-01-01','2079-06-06 23:59:00', 1, 'N', 'Y', 'N', 'N', 'N', 'N', 'Y', 'N', 'N', 'N', 'N', 1),
('B870FB04-63EB-4B7A-84F6-D332B2992E3F','B','BBB','15','4','ZZZ','ZA','EXW',1,'',0,'N','Y','1900-01-01','2079-06-06 23:59:00', 1, 'N', 'Y', 'N', 'N', 'N', 'N', 'Y', 'N', 'N', 'N', 'N', 0),
('6F717A71-A9E0-411B-A405-FEAC61086A64','C','CCC','15','4','ZZZ','ZA','EXW',1,'',0,'N','Y','1900-01-01','2079-06-06 23:59:00', 1, 'N', 'Y', 'N', 'N', 'N', 'N', 'Y', 'N', 'N', 'N', 'N', 0)

INSERT INTO #TempRefCusProcedureAttribute (ZXB_PK,ZXB_ZZ6_ProcedureCode,ZXB_Name,ZXB_Value)
VALUES (NEWID(),'6F717A71-A9E0-411B-A405-FEAC61086A64','CC','CC'),
(NEWID(),'B870FB04-63EB-4B7A-84F6-D332B2992E3F','BB','XB')

INSERT INTO #TempRefCusProcedureLanguage (ZXV_PK,ZXV_ZZ6_Procedure,ZXV_ZX6_NKLanguage,ZXV_Description)
VALUES (NEWID(), '6F717A71-A9E0-411B-A405-FEAC61086A64', 'EN', 'CCC'),
(NEWID(), 'B870FB04-63EB-4B7A-84F6-D332B2992E3F', 'EN', 'XBB')
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusProcedure WHERE ZZ6_ProcedureCode = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusProcedureAttribute WHERE ZXB_Value = 'AA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusProcedureLanguage WHERE ZXV_Description = 'AAA'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusProcedure WHERE ZZ6_ProcedureCode = 'BBB' AND ZZ6_Category = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusProcedureAttribute WHERE ZXB_Value = 'XB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusProcedureLanguage WHERE ZXV_Description = 'XBB'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusProcedure WHERE ZZ6_ProcedureCode = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusProcedureAttribute WHERE ZXB_Value = 'CC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusProcedureLanguage WHERE ZXV_Description = 'CCC'", trans));
				}
			}
		}
	}
}
