using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	[TransactionedTestCase]
	class RefCusTradeGroupUpdaterInfo_3Fixture
	{
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusTradeGroupUpdaterInfo_3(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
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
(NEWID(), 'BC766697-B57B-48B2-ADA7-1DAB147F9AE6', 'DD', '1900-01-01','2079-06-06', 'DDD');

ALTER TABLE RefCusApplicability NOCHECK CONSTRAINT CK_RefCusApplicability_ZZT_ZZ2_Rate_ZZT_ZX1_Conditions_ZZT_ZY2_AdditionalCode_ZZT_ZZH_TariffRelationship;
INSERT RefCusApplicability (ZZT_PK,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_ZZA_SecondTradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (NEWID(),'2022-01-01 00:00:00','2022-12-31 23:59:00','345F5663-4AB1-4F53-8405-98DD6B2CFFA1','6CC699A9-137C-4843-8E0E-E89CA9692EF5','001','A');
", trans);
					var info = new RefCusTradeGroupUpdaterInfo_3();
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
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZA_SecondTradeGroup IS NOT NULL", trans));

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroup WHERE ZZA_TradeGroup = 'AAA' AND ZZA_Description = 'XXXX'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroup WHERE ZZA_TradeGroup = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroup WHERE ZZA_TradeGroup = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroup WHERE ZZA_TradeGroup = 'DDD'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroupCountry WHERE ZZB_Description = 'YYY'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroupCountry WHERE ZZB_Description = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroupCountry WHERE ZZB_Description = 'CCC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTradeGroupCountry WHERE ZZB_Description = 'DDD'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZA_SecondTradeGroup IS NOT NULL", trans));
				}
			}
		}

		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void DeleteDuplicateRefCusApplicabilityWhenColumnIsNullableAndIncludeInUniqueIndex(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa')

INSERT RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
VALUES (NEWID(), 'WW', 'WWW')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping)
VALUES ('F0409A45-B2E2-4712-B759-0F1ED9D61BA1', '1P1', '1P1','EUN')

INSERT RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping) 
VALUES ('B67FF53D-2C47-493E-9E4A-ACB5BE78261A', 'AAA', 'AAAA','1900-01-01','2079-06-06', 'ZA'),
('6CC699A9-137C-4843-8E0E-E89CA9692EF5', 'BBB', 'BBBB','1900-01-01','2079-06-06', 'ZA'),
('BC766697-B57B-48B2-ADA7-1DAB147F9AE6', 'DDD', 'DDDD','1900-01-01','2079-06-06', 'ZA')

INSERT RefCusTradeGroupCountry (ZZB_PK,ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate,ZZB_Description)
VALUES (NEWID(), 'B67FF53D-2C47-493E-9E4A-ACB5BE78261A', 'AA', '1900-01-01','2079-06-06', 'AAA'),
(NEWID(), '6CC699A9-137C-4843-8E0E-E89CA9692EF5', 'BB', '1900-01-01','2079-06-06', 'BBB'),
(NEWID(), 'BC766697-B57B-48B2-ADA7-1DAB147F9AE6', 'DD', '1900-01-01','2079-06-06', 'DDD')

insert into RefCusTariff(ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_IAMUnique, ZZ1_Description, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_ZZZ_NKDataGrouping, ZZ1_CompositeKeyOnZZ5)
values('16D0B6B6-E2EA-468A-8F09-1DD8D88C5820', 'F0409A45-B2E2-4712-B759-0F1ED9D61BA1', 'MMF036', 0, 'MUTTON TRUNK BONELESS FROZEN2', '2022-06-12 00:00:00.000', '2079-06-06 22:22:00.000', '1237', 'SG', '1236')

insert into RefCusRate(ZZ2_PK, ZZ2_ZZ1_Tariff, ZZ2_ZZW_TariffNationalCode, ZZ2_StartDate, ZZ2_EndDate, ZZ2_ZY1_RateCode, ZZ2_RateFormula, ZZ2_ZZZ_NKDataGrouping, ZZ2_RX_NKCurrencyOverride) 
values('6ECA26A4-2CA5-4D06-BF28-49B38A60D293', '16D0B6B6-E2EA-468A-8F09-1DD8D88C5820', NULL, '2018-12-20 00:00:00.000', '2079-06-06 12:49:00.000', NULL, '0', 'EUN', '')

Insert into RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZX1_Conditions, ZZT_ZY2_AdditionalCode, ZZT_StartDate, ZZT_EndDate, ZZT_ZZA_TradeGroup, ZZT_ZZA_SecondTradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber)
values (NEWID(), '6ECA26A4-2CA5-4D06-BF28-49B38A60D293', NULL, NULL, '2019-02-23 00:00:00', '2079-06-06 13:59:00', NULL, NULL, '', '')

Insert into RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZX1_Conditions, ZZT_ZY2_AdditionalCode, ZZT_StartDate, ZZT_EndDate, ZZT_ZZA_TradeGroup, ZZT_ZZA_SecondTradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber)
values (NEWID(), '6ECA26A4-2CA5-4D06-BF28-49B38A60D293', NULL, NULL, '2019-02-23 00:00:00', '2079-06-06 13:59:00', 'B67FF53D-2C47-493E-9E4A-ACB5BE78261A', NULL, '', '')

", trans);
					var info = new RefCusTradeGroupUpdaterInfo_3();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"

INSERT #TempRefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping,Deleted) 
VALUES ('345F5663-4AB1-4F53-8405-98DD6B2CFFA9', 'AAA', 'AAAA','1900-01-01','2079-06-06', 'ZA', 1),
('6CC699A9-137C-4843-8E0E-E89CA9692EF9', 'BBB', 'BBBB','1900-01-01','2079-06-06', 'ZA',1),
('BC766697-B57B-48B2-ADA7-1DAB147F9AE9', 'CCC', 'CCCC','1900-01-01','2079-06-06', 'ZA', 0),
(NEWID(), 'DDD', 'DDDD','1900-01-01','2079-06-06', 'ZA', 0)

Delete RefCusApplicability where ZZT_ZZA_TradeGroup = '345F5663-4AB1-4F53-8405-98DD6B2CFFA9';
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.DoesNotThrow(() => TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans));
				}
			}
		}

	}
}
