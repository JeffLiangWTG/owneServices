using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class TariffView_V2Fixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void TariffView_V2FixtureInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'EUN', 'European Union'),
(NEWID(), 'DE', 'Germany')

INSERT RefCusNomenclatureGroupType (ZZ9_PK, ZZ9_GroupType, ZZ9_Description)
VALUES(NEWID(), 'CN', 'European Union Combined Nomenclature')

INSERT RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZ9_NKNomenclatureGroupType, ZZI_ZZZ_NKDataGrouping)
VALUES ('8896ED1C-5506-54C2-AFEC-08DC19EA3DD4', 'IMP', 'Import Tariff', 'CN', 'GB')

INSERT RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_IAMUnique, ZZ1_Description, ZZ1_StartDate, ZZ1_EndDate, ZZ1_PublishedDate, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_ZZZ_NKDataGrouping, ZZ1_CompositeKeyOnZZ5)
VALUES('D5592ED8-3406-54C2-C52B-08DC1E49D9A2', '8896ED1C-5506-54C2-AFEC-08DC19EA3DD4', '7616999099', '0', 'Other', '2010-07-01', '2023-12-30', '2023-12-20', '', 'EUN', '15.76..16.9.9.20.10.110.20'),
('D6FE80A8-5406-54C2-B226-08DC1AC391A9', '8896ED1C-5506-54C2-AFEC-08DC19EA3DD4', '7616999099', '0', 'Other', '2024-01-01', '2079-06-06', '2024-01-20', '', 'EUN', '15.76..16.9.9.20.20.90.20')

INSERT RefCusTariffNationalCode (ZZW_PK, ZZW_ZZ1_Tariff, ZZW_NationalCode, ZZW_Description, ZZW_ZZF_NKTaxOrFeeCode, ZZW_StartDate, ZZW_EndDate, ZZW_PublishedDate, ZZW_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'D5592ED8-3406-54C2-C52B-08DC1E49D9A2', '0', 'andere', 'REG', '2012-05-12', '2079-06-06', '2023-02-21', 'DE'),
(NEWID(), 'D6FE80A8-5406-54C2-B226-08DC1AC391A9', '0', 'andere', 'REG', '2012-05-12', '2079-06-06', '2024-01-11', 'DE')

", trans);

					string viewName = "TariffView_V2";
					Assert.AreEqual(2, TestDBHelper.ExecuteScalar(conn, $"SELECT COUNT(*) FROM {viewName} WHERE ZZ1_ZZZ_NKDataGrouping = 'DE'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, $"SELECT COUNT(*) FROM {viewName} WHERE ZZ1_EndDate > '2079-01-01' AND ZZ1_ZZZ_NKDataGrouping = 'DE'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, $"SELECT COUNT(*) FROM {viewName} WHERE ZZ1_StartDate < '2011-01-01' AND ZZ1_ZZZ_NKDataGrouping = 'DE'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, $"SELECT COUNT(*) FROM {viewName} WHERE ZZ1_PublishedDate = '2023-12-20' AND ZZ1_ZZZ_NKDataGrouping = 'DE'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, $"SELECT COUNT(*) FROM {viewName} WHERE ZZ1_PublishedDate = '2024-01-20' AND ZZ1_ZZZ_NKDataGrouping = 'DE'", trans));
				}
			}
		}
	}
}
