using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class PopulateDataChangeHistoryForExistingRecordsFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			Assert.AreEqual(3, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(*) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZD'"));
			Assert.AreEqual(2, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(*) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZAT'"));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new PopulateDataChangeHistoryForExistingRecords(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','AU','AU','AU');

INSERT INTO RefCusCodeType(ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES(NEWID(), 'AQISP', 'EXDOCS Quarantine Place Code', 1, 0, 'AU')

INSERT INTO RefCusCodeList(ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
VALUES('DCEDF991-FDF6-4880-9EDB-23B6EAD313B5', 'AQISP', 'TST', 'Testing1', 'AU', '2018-04-09 00:00:00', '2018-07-23 00:00:00')
INSERT INTO RefCusCodeList(ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
VALUES('CBF3264B-542A-45F7-8BF9-2B35040DEBC9', 'AQISP', 'CD2', 'Testing2', 'AU', '2018-04-09 00:00:00', '2018-07-22 23:59:00')
INSERT INTO RefCusCodeList(ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
VALUES('ED942838-31CC-47EB-8220-4DB05CA7D070', 'AQISP', 'CD3', 'Testing3', 'AU', '2018-04-09 00:00:00', '2018-07-22 23:59:00')

INSERT INTO RefAccTaxRate (ZAT_PK, ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate, ZAT_EndDate)
VALUES('9AE96C18-3662-40F1-8DAB-8693F2595B5F', 'AU', 'STD', '2019-04-22', '2019-04-23'),
('079479E1-0E58-42B6-9C83-1680877123A6', 'AU', 'MED', '2019-04-22', '2019-04-24')
";

			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
