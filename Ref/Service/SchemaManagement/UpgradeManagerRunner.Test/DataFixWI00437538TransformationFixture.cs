using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00437538TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select ZZ1_Description from RefCusTariff Where ZZ1_PK = 'EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo("Testing Transform Description"));

				cmd.CommandText = @"Select ZZ1_Description from RefCusTariff Where ZZ1_PK = 'EEA8ED73-76BD-40FA-8BDB-02E4683207F6'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo("Testing|Transform|Description"));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00437538Transformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (newid(), 'EUN', 'X')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping)
VALUES ('B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', 'IMP', 'EU', 'EUN')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping)
VALUES ('5377E8DE-6A52-44DF-8B35-2BB0D97B52C6', 'EXP', 'EU', 'EUN')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZZ_NKDataGrouping)
VALUES ('EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC', 'B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', 'CRS2018', 'Testing|Transform|Description', '', '2002-01-01 00:00:00', '2079-06-06 23:59:00', 'EUN')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZZ_NKDataGrouping)
VALUES ('EEA8ED73-76BD-40FA-8BDB-02E4683207F6', '5377E8DE-6A52-44DF-8B35-2BB0D97B52C6', 'CRS2021', 'Testing|Transform|Description', '', '2002-01-01 00:00:00', '2079-06-06 23:59:00', 'EUN')
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
