using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	[TransactionedTestCase]
	class RefCusExcludedTradeGroupFixture
	{
		[Test]
		public void TestExcludedTradeGroupShouldHaveDataSetPKAfterInsert()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = Sql;
					cmd.ExecuteNonQuery();

					cmd.CommandText = "SELECT COUNT(*) FROM RefCusExcludedTradeGroup WHERE ZZC_DataSetPK = 'EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
				}
			}
		}

		const string Sql = @"
INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (newid(), 'CA', 'X')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping)
VALUES ('B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', 'SIMA', 'SIMA', 'CA')

INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_Description, ZZA_ZZZ_NKDataGrouping) 
VALUES
	('59586763-668B-4127-9E34-56560C40FDE5','CN','CN','CA'),
	('E6ABAB8A-7201-4BE5-9F3F-317C3C0CEBE5','KR','KR','CA'),
	('D582C56E-2D9C-441B-9B02-F2E06BD7E2AC','VN','VN','CA')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZZ_NKDataGrouping)
VALUES ('EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC', 'B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', 'CRS2018', 'X', '', '2002-01-01 00:00:00', '2079-06-06 23:59:00', 'CA')

INSERT INTO RefCusRate (ZZ2_PK, ZZ2_ZZ1_Tariff, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
VALUES ('3C83CF77-B0B3-4FD5-8458-0478BAE86113', 'EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC', 0, '1900-01-01 00:00:00', '2079-06-06 23:59:00')

INSERT INTO RefCusApplicability (ZZT_PK, ZZT_ZZ2_Rate, ZZT_AdditionalCode,ZZT_OrderNumber, ZZT_StartDate, ZZT_EndDate, ZZT_ZZA_TradeGroup)
VALUES
	('F8773DAD-1C92-410C-B086-E9DB9745FFD5', '3C83CF77-B0B3-4FD5-8458-0478BAE86113', '', '','1900-01-01 00:00:00', '2079-06-06 23:59:00', '59586763-668B-4127-9E34-56560C40FDE5'),
	('3A4CCFCB-E110-4C71-A54E-4ED8E1288FB4', '3C83CF77-B0B3-4FD5-8458-0478BAE86113', '', '','1900-01-01 00:00:00', '2079-06-06 23:59:00', 'E6ABAB8A-7201-4BE5-9F3F-317C3C0CEBE5'),
	('09ACB2C0-9669-42BA-A93B-E3B5BD5D4FDC', '3C83CF77-B0B3-4FD5-8458-0478BAE86113', '', '','1900-01-01 00:00:00', '2079-06-06 23:59:00', 'D582C56E-2D9C-441B-9B02-F2E06BD7E2AC')

INSERT INTO RefCusExcludedTradeGroup (ZZC_PK, ZZC_ZZT_Applicability, ZZC_ZZA_TradeGroup)
VALUES ('5888261D-51B4-47CE-8743-6D8DFC587122', 'F8773DAD-1C92-410C-B086-E9DB9745FFD5', '59586763-668B-4127-9E34-56560C40FDE5')
";
	}
}
