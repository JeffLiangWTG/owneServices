using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusCodeListUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusCodeListUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa')

INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES(NEWID(), 'DE', 'German')

INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'CUSOF', 'AAA', 'ZA')

INSERT INTO RefCusCodeListAttributeName(ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_IsMandatory, ZXE_AllowDuplicates, ZXE_IsValueMandatory)
VALUES ('481E2D99-CA7F-4F65-8E08-32806A57B41A', 'ROLE', 'Role', 'CUSOF', 'ZA', 0, 0, 0)

INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES 
('3EE3B8C0-2F82-450A-AC0D-DA727B3F3C05', 'CUSOF', '4701', 'JOHN F KENNEDY AIRPORT, N', '2018-01-19', '2079-06-06 23:59:00', 'ZA'),
('72BA9BF6-BD6D-49E0-AB1F-33D7159076BE', 'CUSOF', 'AAA', 'GARY, I', '2018-01-19', '2079-06-06 23:59:00', 'ZA')

INSERT INTO RefCusCodeListLanguage(ZXA_PK, ZXA_ZX6_NKLanguage, ZXA_ZZD_CodeList, ZXA_Description)
VALUES (NEWID(), 'DE', '3EE3B8C0-2F82-450A-AC0D-DA727B3F3C05', 'Desc1GRM'),
(NEWID(), 'DE', '72BA9BF6-BD6D-49E0-AB1F-33D7159076BE', 'Desc1DE')

INSERT INTO RefCusCodeListAttribute(ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value)
VALUES ('3D983217-172A-46E0-8683-E441A6D4AFFD', '3EE3B8C0-2F82-450A-AC0D-DA727B3F3C05', 'ROLE', 'EXP'),
('59098390-64D3-4F3E-A7BC-B8404C591058', '72BA9BF6-BD6D-49E0-AB1F-33D7159076BE', 'ROLE', 'VAA')

INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList, ZZU_ZZE_Attribute)
VALUES 
(NEWID(), 'ROA', '3EE3B8C0-2F82-450A-AC0D-DA727B3F3C05', NULL),
(NEWID(), 'SEA', NULL, '3D983217-172A-46E0-8683-E441A6D4AFFD'),
(NEWID(), 'AIR', '72BA9BF6-BD6D-49E0-AB1F-33D7159076BE', NULL),
(NEWID(), 'RAI', NULL, '59098390-64D3-4F3E-A7BC-B8404C591058')
", trans);
					var info = new RefCusCodeListUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO #TempRefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping, Deleted)
VALUES 
('7304B058-45AC-44E6-A12C-11B84014F8A8', 'CUSOF', '4701', 'JOHN F KENNEDY AIRPORT, N', '2018-01-19', '2079-06-06 23:59:00', 'ZA', 1),
('1D59B08B-F312-4FC1-93AB-7D89A1586082', 'CUSOF', 'AAA', 'AAAA', '2018-01-19', '2079-06-06 23:59:00', 'ZA', 0),
('60831E33-ADF8-4725-A4C1-E9B7E9A3767F', 'CUSOF', 'CCC', 'CCCC', '2018-01-19', '2079-06-06 23:59:00', 'ZA', 0)

INSERT INTO #TempRefCusCodeListLanguage(ZXA_PK, ZXA_ZX6_NKLanguage, ZXA_ZZD_CodeList, ZXA_Description)
VALUES (newid(), 'DE', '1D59B08B-F312-4FC1-93AB-7D89A1586082', 'BBB'),
(newid(), 'DE', '60831E33-ADF8-4725-A4C1-E9B7E9A3767F', 'CCC')

INSERT INTO #TempRefCusCodeListAttribute(ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value)
VALUES ('6022CA14-4D62-4D67-8ACB-796693335DF1', '60831E33-ADF8-4725-A4C1-E9B7E9A3767F', 'ROLE', 'CCC'),
('9377E911-5171-4B8B-B810-C80F07E9A2C6', '1D59B08B-F312-4FC1-93AB-7D89A1586082', 'ROLE', 'XXX')

INSERT INTO #TempRefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList, ZZU_ZZE_Attribute)
VALUES 
(NEWID(), 'AIR', '60831E33-ADF8-4725-A4C1-E9B7E9A3767F', NULL),
(NEWID(), 'RAI', NULL, '6022CA14-4D62-4D67-8ACB-796693335DF1'),
(NEWID(), 'MAI', '60831E33-ADF8-4725-A4C1-E9B7E9A3767F', NULL),
(NEWID(), 'FIX', NULL, '6022CA14-4D62-4D67-8ACB-796693335DF1')
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);

					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeList WHERE ZZD_Code = '4701'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListLanguage WHERE ZXA_Description = 'Desc1GRM'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttribute WHERE ZZE_Value = 'EXP'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeOrAttributeTransportMode WHERE ZZU_TransportMode = 'ROA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeOrAttributeTransportMode WHERE ZZU_TransportMode = 'SEA'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeList WHERE ZZD_Code = 'AAA' AND ZZD_Description = 'AAAA'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListLanguage WHERE ZXA_Description = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttribute WHERE ZZE_Value = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeOrAttributeTransportMode JOIN RefCusCodeList ON ZZU_ZZD_CodeList = ZZD_PK WHERE ZZU_TransportMode = 'AIR'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeOrAttributeTransportMode JOIN RefCusCodeListAttribute ON ZZU_ZZE_Attribute = ZZE_PK WHERE ZZU_TransportMode = 'RAI'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeList WHERE ZZD_Code = 'CCC' AND ZZD_Description = 'CCCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListLanguage WHERE ZXA_Description = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttribute WHERE ZZE_Value = 'XXX'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeOrAttributeTransportMode WHERE ZZU_TransportMode = 'MAI' AND ZZU_ZZD_CodeList IS NOT NULL AND ZZU_ZZE_Attribute IS NULL", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeOrAttributeTransportMode WHERE ZZU_TransportMode = 'FIX' AND ZZU_ZZD_CodeList IS NULL AND ZZU_ZZE_Attribute IS NOT NULL", trans));
				}
			}
		}
	}
}
