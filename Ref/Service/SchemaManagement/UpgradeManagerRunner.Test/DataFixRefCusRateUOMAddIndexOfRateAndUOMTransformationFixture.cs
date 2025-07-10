using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixRefCusRateUOMAddIndexOfRateAndUOMTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"select count(*) from RefCusRateUOM";
				Assert.That((int)cmd.ExecuteScalar(), Is.EqualTo(5));
				cmd.CommandText = @"select count(*) from RefCusRateUOM where ZXG_UOM = 'AAA'";
				Assert.That((int)cmd.ExecuteScalar(), Is.EqualTo(1));
				cmd.CommandText = @"select count(*) from RefCusRateUOM where ZXG_UOM = 'DDD'";
				Assert.That((int)cmd.ExecuteScalar(), Is.EqualTo(2));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixRefCusRateUOMAddIndexOfRateAndUOMTransformation(116);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
ALTER INDEX IX_RefCusRateUOM_ZXG_ZZ2_Rate_ZXG_UOM ON RefCusRateUOM DISABLE

INSERT INTO [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
VALUES (NEWID(), 23, 'RefCusTariff', 'RefCusTariff','ZZ1',0);

INSERT INTO [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES (newid(), 'EUN', 'European Union', null);

INSERT INTO [dbo].[RefCusRateType] (ZZR_PK,ZZR_RateType,ZZR_Description,ZZR_IsPayable,ZZR_ZZZ_NKDataGrouping,ZZR_RX_NKFormulaCurrency,ZZR_CustomsValueFormula,ZZR_IsExport)
VALUES ('D2E07963-932F-4BB4-A2E1-9C90F8848C23','DTY', 'Duty',1, 'EUN', '', 'CV',0);

INSERT INTO [dbo].[RefCusTariffType] (ZZI_PK,ZZI_TariffType,ZZI_Description,ZZI_ZZ9_NKNomenclatureGroupType,ZZI_ZZZ_NKDataGrouping,ZZI_ZZR_RateType,ZZI_HasFormulaSpecificQuestions)
VALUES ('FC6DB681-F5F4-4B2F-9020-84AFF4F8BE98', 'IMP', 'Import Tariff', '', 'EUN', 'D2E07963-932F-4BB4-A2E1-9C90F8848C23', 0);

INSERT INTO [dbo].[RefCusTradeGroup] (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
VALUES
('73DD6854-4168-4D35-B9B9-3F8F99DB0108', 'AD',' ERGA OMNES', '1958-01-01 00:00:00', '2079-06-06 23:59:00','EUN'),
(newid(), '1011',' ERGA OMNES1', '1958-01-01 00:00:00', '2079-06-06 23:59:00','EUN');

INSERT INTO [dbo].[RefCusRateCode] (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description, ZY1_InternalUse)
VALUES
('9FA822CF-6930-4708-B1A7-30951017A039', 'A00', 'D2E07963-932F-4BB4-A2E1-9C90F8848C23', 'Customs duties on industrial products', 0),
('923A7DAB-B740-47FC-83D1-166903B57351', 'EAR', 'D2E07963-932F-4BB4-A2E1-9C90F8848C23', 'Customs duties on industrial products', 0),
('2C25340D-BF6E-40D0-880D-CCFE3019699A', 'A01', 'D2E07963-932F-4BB4-A2E1-9C90F8848C23', 'Customs duties on industrial products', 0);

INSERT INTO [dbo].[RefCusTariff] (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_IAMUnique, ZZ1_Description, ZZ1_StartDate, ZZ1_EndDate, ZZ1_PublishedDate, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_ZZZ_NKDataGrouping, ZZ1_CompositeKeyOnZZ5)
VALUES('8BABA764-4DEE-41AD-A9A8-A890017DA960', 'FC6DB681-F5F4-4B2F-9020-84AFF4F8BE98', 'Z2P0GV0QS00', '0', 'GUAVA, PRESERVED BY SALT ONLY', '2022-06-19 00:00:00.000', '2079-06-06 23:59:00.000', '2022-04-07', '', 'EUN', '')

INSERT INTO [dbo].[RefCusRate] (ZZ2_PK, ZZ2_ZZ1_Tariff, ZZ2_ZZW_TariffNationalCode, ZZ2_StartDate, ZZ2_EndDate, ZZ2_ZY1_RateCode, ZZ2_RateFormula, ZZ2_ZZS_Preference, ZZ2_SelectorFormula, ZZ2_ZZZ_NKDataGrouping, ZZ2_DataSetPK, ZZ2_DataSetCode, ZZ2_RateFormulaDerivedFrom, ZZ2_RX_NKCurrencyOverride)
VALUES
('DF10A05D-748B-41CF-8C11-1F83E86DB74A', '8BABA764-4DEE-41AD-A9A8-A890017DA960', NULL, '2016-10-06 00:00:00.000', '2079-06-06 23:59:00.000', '9FA822CF-6930-4708-B1A7-30951017A039', '0', null, '', 'EUN', '8BABA764-4DEE-41AD-A9A8-A890017DA960', 'ZZ1', NULL, ''),
('5C249768-AF56-4250-A94A-13864D28A0E6', '8BABA764-4DEE-41AD-A9A8-A890017DA960', NULL, '2016-11-06 00:00:00.000', '2079-06-06 23:59:00.000', '923A7DAB-B740-47FC-83D1-166903B57351', '1', null, '', 'EUN', '8BABA764-4DEE-41AD-A9A8-A890017DA960', 'ZZ1', NULL, ''),
('9223359C-F4DB-4DE2-A1AD-37FB9D2DC356', '8BABA764-4DEE-41AD-A9A8-A890017DA960', NULL, '2016-12-06 00:00:00.000', '2079-06-06 23:59:00.000', '2C25340D-BF6E-40D0-880D-CCFE3019699A', '2', null, '', 'EUN', '8BABA764-4DEE-41AD-A9A8-A890017DA960', 'ZZ1', NULL, '');

INSERT INTO [RefCusRateUOM]
(ZXG_PK,ZXG_ZZ2_Rate, ZXG_UOM, ZXG_DataSetPK, ZXG_DataSetCode)
VALUES
(NEWID(), 'DF10A05D-748B-41CF-8C11-1F83E86DB74A', 'AAA', '8BABA764-4DEE-41AD-A9A8-A890017DA960', 'ZZ1'),
(NEWID(), 'DF10A05D-748B-41CF-8C11-1F83E86DB74A', 'AAA', '8BABA764-4DEE-41AD-A9A8-A890017DA960', 'ZZ1'),
(NEWID(), '5C249768-AF56-4250-A94A-13864D28A0E6', 'BBB', '8BABA764-4DEE-41AD-A9A8-A890017DA960', 'ZZ1'),
(NEWID(), '5C249768-AF56-4250-A94A-13864D28A0E6', 'CCC', '8BABA764-4DEE-41AD-A9A8-A890017DA960', 'ZZ1'),
(NEWID(), '9223359C-F4DB-4DE2-A1AD-37FB9D2DC356', 'DDD', '8BABA764-4DEE-41AD-A9A8-A890017DA960', 'ZZ1'),
(NEWID(), '5C249768-AF56-4250-A94A-13864D28A0E6', 'DDD', '8BABA764-4DEE-41AD-A9A8-A890017DA960', 'ZZ1');
";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
