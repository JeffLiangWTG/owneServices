using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class RuleAddWI00178368TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusTariffRule t Join RefCusTariffUOMRule u on u.ZZ8_ZZ1_Tariff = t.ZZ1_PK Where t.ZZ1_TariffCode = '82032020' And t.ZZ1_ZZZ_NKDataGrouping = 'ZA' And u.ZZ8_Type = 'CU2' And u.ZZ8_UOM = 'MM'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RuleAddWI00178368Transformation(4);
		}

		protected override void PrepareTestData()
		{
		}
	}
}
