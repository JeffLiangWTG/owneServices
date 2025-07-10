using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusRateTypeUpdaterInfo_3Fixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusRateTypeUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa')

INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES ('494D2ACB-742E-4C34-906B-E01EAD292F6F', 'EN', 'EN')

INSERT INTO RefCusRateType (ZZR_PK,ZZR_RateType,ZZR_Description,ZZR_IsPayable,ZZR_ZZZ_NKDataGrouping,ZZR_CustomsValueFormula, ZZR_IsExport, ZZR_RX_NKFormulaCurrency)
VALUES ('58F19F31-B7BB-48E3-9ED9-76733B831C1B', 'DTY', 'Duty', 1, 'ZA', '', 0, 'GBP'),
('56B11480-8664-4304-A665-EB74ACFAD13D', 'REA', 'REA - AY Tax Rebate', 0, 'ZA', '', 1, 'CNY')

INSERT INTO RefCusRateCode (ZY1_PK,ZY1_RateCode,ZY1_ZZR_RateType,ZY1_Description, ZY1_InternalUse) VALUES
('9FBDD063-4839-4C9F-A602-8A28A5897724','ADFM','58F19F31-B7BB-48E3-9ED9-76733B831C1B','Flour Duty',1),
('E4475805-DFE1-45B7-9D1C-BB05AFBEF055','BBB','56B11480-8664-4304-A665-EB74ACFAD13D','Flour Duty - Reduced',1)

INSERT INTO RefCusRateCodeLanguage (ZXC_PK, ZXC_ZX6_NKLanguage, ZXC_ZY1_RateCode, ZXC_Description) VALUES
('78BA88D9-0E4B-4A63-8F4C-D643934568C2', 'EN', '9FBDD063-4839-4C9F-A602-8A28A5897724', 'AAA'),
('17A3CFBA-A702-4A22-BA10-0FF835E0290E', 'EN', 'E4475805-DFE1-45B7-9D1C-BB05AFBEF055', 'BBB')

INSERT INTO RefCusRateTypeLanguage (ZXT_PK, ZXT_ZX6_NKLanguage, ZXT_ZZR_RateType, ZXT_Description) VALUES
(NEWID(), 'EN', '58F19F31-B7BB-48E3-9ED9-76733B831C1B', 'AAA'),
(NEWID(), 'EN', '56B11480-8664-4304-A665-EB74ACFAD13D', 'BBB')

", trans);
					var info = new RefCusRateTypeUpdaterInfo_3();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO #TempRefCusRateType (ZZR_PK,ZZR_RateType,ZZR_Description,ZZR_IsPayable,ZZR_ZZZ_NKDataGrouping,ZZR_CustomsValueFormula, ZZR_IsExport, ZZR_RX_NKFormulaCurrency, Deleted)
VALUES (NEWID(), 'DTY', 'Duty', 1, 'ZA', '', 1, 'CAD', 1),
('AF911C38-C847-49C6-B276-3F8A543A4A51', 'REA', 'BBBB', 0, 'ZA', '', 0, 'GBP', 0),
('EC024218-B784-4B21-95FF-112B0A3FC43A', 'CCC', 'CCCC', 0, 'ZA', '', 1, 'EUR', 0)

INSERT INTO #TempRefCusRateCode (ZY1_PK,ZY1_RateCode,ZY1_ZZR_RateType,ZY1_Description,ZY1_InternalUse) VALUES
('6B14C883-0E54-4C44-B69B-183E84CF7F32','BBB','AF911C38-C847-49C6-B276-3F8A543A4A51','XBBB',1),
('7A04BADA-BE27-4AC9-841E-17551E034664','CCC','EC024218-B784-4B21-95FF-112B0A3FC43A','XCCC',1)

INSERT INTO #TempRefCusRateCodeLanguage (ZXC_PK, ZXC_ZX6_NKLanguage, ZXC_ZY1_RateCode, ZXC_Description) VALUES
(NEWID(), 'EN', '6B14C883-0E54-4C44-B69B-183E84CF7F32', 'XBBB'),
(NEWID(), 'EN', '7A04BADA-BE27-4AC9-841E-17551E034664', 'XCCC')

INSERT INTO #TempRefCusRateTypeLanguage (ZXT_PK, ZXT_ZX6_NKLanguage, ZXT_ZZR_RateType, ZXT_Description) VALUES
(NEWID(), 'EN', 'AF911C38-C847-49C6-B276-3F8A543A4A51', 'XBBB'),
(NEWID(), 'EN', 'EC024218-B784-4B21-95FF-112B0A3FC43A', 'XCCC')
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);

					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateType WHERE ZZR_RateType = 'DTY'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateCode WHERE ZY1_RateCode = 'ADFM'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateCodeLanguage WHERE ZXC_Description = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateTypeLanguage WHERE ZXT_Description = 'AAA'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateType WHERE ZZR_RateType = 'REA' AND ZZR_Description = 'BBBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateCode WHERE ZY1_RateCode = 'BBB' AND ZY1_Description = 'XBBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateCodeLanguage WHERE ZXC_Description = 'XBBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateTypeLanguage WHERE ZXT_Description = 'XBBB'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateType WHERE ZZR_RateType = 'CCC' AND ZZR_Description = 'CCCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateCode WHERE ZY1_RateCode = 'CCC' AND ZY1_Description = 'XCCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateCodeLanguage WHERE ZXC_Description = 'XCCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRateTypeLanguage WHERE ZXT_Description = 'XCCC'", trans));
				}
			}
		}
	}
}
