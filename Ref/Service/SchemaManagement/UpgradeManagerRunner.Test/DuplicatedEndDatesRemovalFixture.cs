using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DuplicatedEndDatesRemovalFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusTariff WHERE ZZ1_TariffCode = '7000' AND ZZ1_EndDate = '2079-06-06 23:59:00'";
				Assert.AreEqual(1, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DuplicatedEndDatesRemoval(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
DROP INDEX IX_RefCusTariff_ZZ1_ZZZ_NKDataGrouping_ZZ1_TariffCode_ZZ1_ZZI_TariffType_ZZ1_EndDate_ZZ1_IAMUnique ON RefCusTariff;

INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (newid(), 'EUN', 'X')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping)
VALUES ('B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', 'MEU', 'Meursing Rates', 'EUN')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_IAmUnique, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZZ_NKDataGrouping)
VALUES
	('EE19F19D-A5C4-4DE3-8E6D-8BA82CD147DC', 0, 'B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', '7000', 'X', '', '2002-01-01 00:00:00', '2079-06-06 23:59:00', 'EUN'),
	('07997E51-18F5-453C-B6BB-B6EBFA51239B', 0, 'B6DDF07B-ECFB-48DD-B4C3-EEA11DB9EE09', '7000', 'X', '', '2003-01-01 00:00:00', '2079-06-06 23:59:00', 'EUN')
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
