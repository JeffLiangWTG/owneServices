using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class FixMeursingApplicationDataSetPKTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				var sqlText = @"SELECT ZZT_DataSetPK FROM RefCusApplicability WHERE ZZT_PK = '{0}'";
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sqlText, "F8773DAD-1C92-410C-B086-E9DB9745FFD5");
				Assert.AreEqual(new Guid("EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC"), cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sqlText, "3A4CCFCB-E110-4C71-A54E-4ED8E1288FB4");
				Assert.AreEqual(new Guid("07997E51-18F5-453C-B6BB-B6EBFA51239B"), cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sqlText, "09ACB2C0-9669-42BA-A93B-E3B5BD5D4FDC");
				Assert.AreEqual(new Guid("871BFD17-FC13-4BBD-BE9D-A52DB51D7653"), cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new FixMeursingApplicationDataSetPKTransformation(0);
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

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZZ_NKDataGrouping)
VALUES
	('EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC', 'B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', '7000', 'X', '', '2002-01-01 00:00:00', '2079-06-06 23:59:00', 'EUN'),
	('07997E51-18F5-453C-B6BB-B6EBFA51239B', 'B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', '7001', 'X', '', '2003-01-01 00:00:00', '2079-06-06 23:59:00', 'EUN'),
	('871BFD17-FC13-4BBD-BE9D-A52DB51D7653', 'B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', '7002', 'X', '', '2004-01-01 00:00:00', '2079-06-06 23:59:00', 'EUN')

INSERT INTO RefCusRate (ZZ2_PK, ZZ2_ZZ1_Tariff, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
VALUES
	('3C83CF77-B0B3-4FD5-8458-0478BAE86113', 'EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC', 0, '1900-01-01 00:00:00', '2079-06-06 23:59:00'),
	('2FF072C9-0981-4727-B1A4-0959E6524EE8', '07997E51-18F5-453C-B6BB-B6EBFA51239B', 0, '1900-01-01 00:00:00', '2079-06-06 23:59:00'),
	('9C471FB0-089A-47A6-B09B-209110D15A01', '871BFD17-FC13-4BBD-BE9D-A52DB51D7653', 0, '1900-01-01 00:00:00', '2079-06-06 23:59:00')

INSERT INTO RefCusApplicability (ZZT_PK, ZZT_ZZ2_Rate, ZZT_AdditionalCode,ZZT_OrderNumber, ZZT_StartDate, ZZT_EndDate)
VALUES
	('F8773DAD-1C92-410C-B086-E9DB9745FFD5', '3C83CF77-B0B3-4FD5-8458-0478BAE86113', '', '','1900-01-01 00:00:00', '2079-06-06 23:59:00'),
	('3A4CCFCB-E110-4C71-A54E-4ED8E1288FB4', '2FF072C9-0981-4727-B1A4-0959E6524EE8', '', '','1900-01-01 00:00:00', '2079-06-06 23:59:00'),
	('09ACB2C0-9669-42BA-A93B-E3B5BD5D4FDC', '9C471FB0-089A-47A6-B09B-209110D15A01', '', '','1900-01-01 00:00:00', '2079-06-06 23:59:00')

UPDATE RefCusApplicability SET ZZT_DataSetPK = newid()
";
				cmd.ExecuteNonQuery();
			}
		}
	}
}
