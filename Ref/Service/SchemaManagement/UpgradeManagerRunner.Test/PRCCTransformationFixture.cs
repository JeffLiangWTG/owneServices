using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class PRCCTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"select count(*) from RefCusTariffRule t join refcusraterule r on r.[ZZ2_ZZ1_Tariff] = t.zz1_pk where t.zz1_tariffcode = '460170300' and r.zz2_rateformula = '1P1'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"select count(*) from RefCusTariffRule t join refcusraterule r on r.[ZZ2_ZZ1_Tariff] = t.zz1_pk where t.zz1_tariffcode = '460170304' and r.zz2_rateformula = '1P1+12A'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"select count(*) from RefCusTariffRule t  where t.zz1_tariffcode = '460170302' ";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new PRCCTransformation(1);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
insert into RefCusTariffRule (zz1_pk,zz1_tariffcode,zz1_ZZZ_NKDataGrouping) values ('367C4D48-255B-451B-8F6D-1F85A3B40D81','460170300','ZA');
insert into refcusraterule(zz2_pk,zz2_zz1_tariff,zz2_rateformula,zz2_selectorformula,ZZ2_ZZZ_NKDataGrouping) values ('55FFD8C9-EE01-40CD-AA79-1DCB751BF91D','367C4D48-255B-451B-8F6D-1F85A3B40D81','{""Rebate Amount""}','','ZA');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
