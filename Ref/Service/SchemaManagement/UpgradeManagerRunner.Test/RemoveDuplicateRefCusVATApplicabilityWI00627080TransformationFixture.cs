using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	public class RemoveDuplicateRefCusVATApplicabilityWI00627080TransformationFixture
	{
		[Test]
		[TransactionedTestCase]
		public void Run()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var connection = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				connection.Open();
				var dbCreator = new DbCreator(connection);
				dbCreator.ExcuteDbScript(dbName, @"
IF EXISTS (SELECT *  FROM sys.indexes  WHERE name='IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_EndDate_ZZA' 
    AND object_id = OBJECT_ID('[dbo].[RefCusVATApplicability]'))
BEGIN
    ALTER INDEX IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_EndDate_ZZA ON RefCusVATApplicability DISABLE
END");
				dbCreator.ExcuteDbScript(dbName, @"
IF EXISTS (SELECT *  FROM sys.indexes  WHERE name='IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate_ZZA' 
    AND object_id = OBJECT_ID('[dbo].[RefCusVATApplicability]'))
BEGIN
    ALTER INDEX IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate_ZZA ON RefCusVATApplicability DISABLE
END");
				PrepareData(dbCreator, dbName);
				using (var transaction = connection.BeginTransaction())
				{
					var task = new RemoveDuplicateRefCusVATApplicabilityWI00627080Transformation(1);
					task.Run(transaction);
					transaction.Commit();
				}
				using (var command = connection.CreateCommand())
				{
					command.CommandText = "SELECT COUNT(*) FROM RefCusVATApplicability WHERE ZX5_PK IN ('1EFD4AF1-D0D6-4D50-9787-0485919A7B9E', '2EFD4AF1-D0D6-4D50-9787-0485919A7B9E')";
					Assert.AreEqual(1, (int)command.ExecuteScalar());
					command.CommandText = "SELECT COUNT(*) FROM RefCusVATApplicability WHERE ZX5_PK IN ('3EFD4AF1-D0D6-4D50-9787-0485919A7B9E', '4EFD4AF1-D0D6-4D50-9787-0485919A7B9E')";
					Assert.AreEqual(1, (int)command.ExecuteScalar());
				}

				Assert.DoesNotThrow(() => dbCreator.ExcuteDbScript(dbName, @"
IF EXISTS (SELECT *  FROM sys.indexes  WHERE name='IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_EndDate_ZZA' 
    AND object_id = OBJECT_ID('[dbo].[RefCusVATApplicability]'))
BEGIN
    ALTER INDEX IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_EndDate_ZZA ON RefCusVATApplicability REBUILD
END"));
				Assert.DoesNotThrow(() => dbCreator.ExcuteDbScript(dbName, @"
IF EXISTS (SELECT *  FROM sys.indexes  WHERE name='IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate_ZZA' 
    AND object_id = OBJECT_ID('[dbo].[RefCusVATApplicability]'))
BEGIN
    ALTER INDEX IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate_ZZA ON RefCusVATApplicability REBUILD
END
"));
			}
		}

		void PrepareData(DbCreator dbCreator, string dbName)
		{
			var sql = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('20E244E7-D6FE-447E-B81B-486FC06E549D','T1','Test1',NULL),
('9232ACD1-89D3-4135-BDE5-45E38D0F2866','T2','Test2',NULL);

INSERT INTO RefCusTariffType (ZZI_PK,ZZI_TariffType,ZZI_Description,ZZI_ZZ9_NKNomenclatureGroupType,ZZI_ZZZ_NKDataGrouping)
VALUES ('FC6DB681-F5F4-4B2F-9020-84AFF4F8BE98','Type','Import Tariff','','T2');

INSERT INTO RefCusTaxOrFeeType (ZX0_PK, ZX0_TaxOrFeeType, ZX0_Description)
VALUES ('D5AAB67E-D76D-4A35-95E4-5A8274C6E2A7', 'XXX', 'Other');

INSERT INTO RefCusTaxOrFee (ZZF_PK,ZZF_Code,ZZF_Description,ZZF_Value,ZZF_StartDate,ZZF_EndDate,ZZF_ZZZ_NKDataGrouping,ZZF_ZX0_NKTaxOrFeeType)
VALUES ('15A2CCF0-16C6-4739-87A9-22E7AFB1B22F','Fee','Ridotta',0.10000000,'1900-01-01 00:00:00','2079-06-06 00:00:00','T2','XXX');

INSERT INTO RefCusTariff (ZZ1_PK,ZZ1_ZZI_TariffType,ZZ1_TariffCode,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_PublishedDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping)
VALUES ('9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D','FC6DB681-F5F4-4B2F-9020-84AFF4F8BE98','8713900000','Fee','2020-07-01 00:00:00','2079-06-06 23:59:00','2020-07-30 00:00:00','','T1');

INSERT INTO RefCusVATApplicability (ZX5_PK,ZX5_ZZ1_Tariff,ZX5_ZZW_TariffNationalCode,ZX5_ZZF_NKTaxOrFeeCode,ZX5_StartDate,ZX5_EndDate,ZX5_AdditionalCode,ZX5_Description,ZX5_ZZZ_NKDataGrouping,ZX5_DataSetPK,ZX5_DataSetCode)
VALUES 
('1EFD4AF1-D0D6-4D50-9787-0485919A7B9E', '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D', NULL, 'Fee', '2020-07-10 00:00:00', '2079-06-06 23:59:00', '', '', 'T1', '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D','ZZ1'),
('2EFD4AF1-D0D6-4D50-9787-0485919A7B9E', '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D', NULL, 'Fee', '2020-07-11 00:00:00', '2079-06-06 23:59:00', '', '', 'T2', '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D','ZZ1'),
('3EFD4AF1-D0D6-4D50-9787-0485919A7B9E', '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D', NULL, 'Fee', '2010-07-10 00:00:00', '2015-06-06 23:59:00', '', '', 'T1', '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D','ZZ1'),
('4EFD4AF1-D0D6-4D50-9787-0485919A7B9E', '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D', NULL, 'Fee', '2010-07-10 00:00:00', '2016-06-06 23:59:00', '', '', 'T2', '9977AB29-5B7A-4E6A-9BDF-BF0B0BB6240D','ZZ1')
";

			dbCreator.ExcuteDbScript(dbName, sql);
		}
	}
}
