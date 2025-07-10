using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class MergeMeursingTariffsTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;

				var startDateSql = @"SELECT ZZ2_StartDate FROM RefCusRate WHERE ZZ2_PK = '{0}'";
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, startDateSql, "3C83CF77-B0B3-4FD5-8458-0478BAE86113");
				Assert.AreEqual(new DateTime(2002, 01, 01), cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, startDateSql, "2FF072C9-0981-4727-B1A4-0959E6524EE8");
				Assert.AreEqual(new DateTime(2003, 01, 01), cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, startDateSql, "9C471FB0-089A-47A6-B09B-209110D15A01");
				Assert.AreEqual(new DateTime(2004, 01, 01), cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, startDateSql, "D0F90CA0-749E-4E9E-A468-FB07319A0497");
				Assert.AreEqual(new DateTime(1900, 01, 01), cmd.ExecuteScalar());

				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusRate WHERE ZZ2_ZZ1_Tariff = 'EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC' AND ZZ2_DataSetPK = 'EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC'";
				Assert.AreEqual(3, cmd.ExecuteScalar());

				var deleteSql = @"SELECT RVC_Deleted FROM RefDbVersionControl WHERE RVC_ParentPK = '{0}'";
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, deleteSql, "EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC");
				Assert.AreEqual(false, cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, deleteSql, "07997E51-18F5-453C-B6BB-B6EBFA51239B");
				Assert.AreEqual(true, cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, deleteSql, "871BFD17-FC13-4BBD-BE9D-A52DB51D7653");
				Assert.AreEqual(true, cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, deleteSql, "1360EB47-2A43-47A1-B468-2452824AC6A6");
				Assert.AreEqual(false, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new MergeMeursingTariffsTransformation(1);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"
DROP INDEX IX_RefCusTariff_ZZ1_ZZZ_NKDataGrouping_ZZ1_TariffCode_ZZ1_ZZI_TariffType_ZZ1_EndDate_ZZ1_IAMUnique ON RefCusTariff;

INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (newid(), 'EUN', 'X')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping)
VALUES 
	('DB6ABE21-CE4A-4091-A7BE-307BFF9E1F60', 'IMP', 'Import Tariff', 'EUN'),
	('B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', 'MEU', 'Meursing Rates', 'EUN')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZZ_NKDataGrouping)
VALUES
	('EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC', 'B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', '7000', 'X', '', '2002-01-01 00:00:00', '2079-06-06 23:59:00', 'EUN'),
	('07997E51-18F5-453C-B6BB-B6EBFA51239B', 'B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', '7000', 'X', '', '2003-01-01 00:00:00', '2079-06-06 23:59:00', 'EUN'),
	('871BFD17-FC13-4BBD-BE9D-A52DB51D7653', 'B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', '7000', 'X', '', '2004-01-01 00:00:00', '2079-06-06 23:59:00', 'EUN'),
	('1360EB47-2A43-47A1-B468-2452824AC6A6', 'DB6ABE21-CE4A-4091-A7BE-307BFF9E1F60', '7000', 'X', '', '2017-01-01 00:00:00', '2079-06-06 23:59:00', 'EUN')

INSERT INTO RefCusRate (ZZ2_PK, ZZ2_ZZ1_Tariff, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
VALUES
	('3C83CF77-B0B3-4FD5-8458-0478BAE86113', 'EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC', 0, '1900-01-01 00:00:00', '2079-06-06 23:59:00'),
	('2FF072C9-0981-4727-B1A4-0959E6524EE8', '07997E51-18F5-453C-B6BB-B6EBFA51239B', 0, '1900-01-01 00:00:00', '2079-06-06 23:59:00'),
	('9C471FB0-089A-47A6-B09B-209110D15A01', '871BFD17-FC13-4BBD-BE9D-A52DB51D7653', 0, '1900-01-01 00:00:00', '2079-06-06 23:59:00'),
	('D0F90CA0-749E-4E9E-A468-FB07319A0497', '1360EB47-2A43-47A1-B468-2452824AC6A6', 0, '1900-01-01 00:00:00', '2079-06-06 23:59:00')
";
				cmd.ExecuteNonQuery();
			}
		}
	}
}
