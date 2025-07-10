using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class RemoveDuplicatedMEURatesFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = "SELECT COUNT(*) FROM RefCusRate";
				Assert.AreEqual(1, cmd.ExecuteScalar());
				cmd.CommandText = "SELECT COUNT(*) FROM RefCusApplicability";
				Assert.AreEqual(2, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RemoveDuplicatedMEURates(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"
INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (newid(), 'EUN', 'X')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping)
VALUES ('B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', 'MEU', 'Meursing Rates', 'EUN')

INSERT INTO RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_RX_NKFormulaCurrency, ZZR_CustomsValueFormula)
VALUES ('876001AA-7151-48B2-96A4-6507D86AD1E6', 'DTY', 'Duty', 'EUN', 'EUR', 'CV')

INSERT INTO RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description)
VALUES ('505623E0-14A9-445D-A834-5EBF0879E7E4', 'EA', '876001AA-7151-48B2-96A4-6507D86AD1E6', 'Agricultural Component')

INSERT INTO RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
VALUES
	('3260C9FC-2DEA-479B-83C7-FD8DE8EBC5BA', 'AZ', 'Azerbaijan', '1900-01-01 00:00:00', '2079-06-06 23:59:00', 'EUN'),
	('D5D1C6F6-20E2-44A3-9485-A1E566251B07', 'UY', 'Uruguay', '1900-01-01 00:00:00', '2079-06-06 23:59:00', 'EUN')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZZ_NKDataGrouping)
VALUES
	('EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC', 'B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', '7000', 'X', '', '2002-01-01 00:00:00', '2079-06-06 23:59:00', 'EUN')

INSERT INTO RefCusRate (ZZ2_PK, ZZ2_ZZ1_Tariff, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate, ZZ2_ZY1_RateCode)
VALUES
	('3C83CF77-B0B3-4FD5-8458-0478BAE86113', 'EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC', 0, '2002-01-01 00:00:00', '2079-06-06 23:59:00', '505623E0-14A9-445D-A834-5EBF0879E7E4'),
	('2FF072C9-0981-4727-B1A4-0959E6524EE8', 'EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC', 0, '2002-01-01 00:00:00', '2079-06-06 23:59:00', '505623E0-14A9-445D-A834-5EBF0879E7E4'),
	('9C471FB0-089A-47A6-B09B-209110D15A01', 'EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC', 0, '2002-01-01 00:00:00', '2079-06-06 23:59:00', '505623E0-14A9-445D-A834-5EBF0879E7E4')

INSERT INTO RefCusApplicability (ZZT_PK, ZZT_ZZ2_Rate, ZZT_AdditionalCode,ZZT_OrderNumber, ZZT_StartDate, ZZT_EndDate, ZZT_ZZA_TradeGroup)
VALUES
	('F8773DAD-1C92-410C-B086-E9DB9745FFD5', '3C83CF77-B0B3-4FD5-8458-0478BAE86113', '1', '1','2002-01-01 00:00:00', '2079-06-06 23:59:00', '3260C9FC-2DEA-479B-83C7-FD8DE8EBC5BA'),
	('3A4CCFCB-E110-4C71-A54E-4ED8E1288FB4', '3C83CF77-B0B3-4FD5-8458-0478BAE86113', '2', '2','2002-01-01 00:00:00', '2079-06-06 23:59:00', 'D5D1C6F6-20E2-44A3-9485-A1E566251B07'),
	('09ACB2C0-9669-42BA-A93B-E3B5BD5D4FDC', '2FF072C9-0981-4727-B1A4-0959E6524EE8', '', '','1900-01-01 00:00:00', '2079-06-06 23:59:00', '3260C9FC-2DEA-479B-83C7-FD8DE8EBC5BA'),
	('6A6218B7-0EB4-4787-AC28-49D2CD97D1CA', '9C471FB0-089A-47A6-B09B-209110D15A01', '', '','1900-01-01 00:00:00', '2079-06-06 23:59:00', 'D5D1C6F6-20E2-44A3-9485-A1E566251B07')
";
				cmd.ExecuteNonQuery();
			}
		}
	}
}
