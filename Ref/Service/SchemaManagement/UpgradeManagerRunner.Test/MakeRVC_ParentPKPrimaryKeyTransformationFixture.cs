using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class MakeRVC_ParentPKPrimaryKeyTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefDbVersionControl Where RVC_ParentPK = 'ED0CE102-2C22-4995-9F99-8D5825517E52'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"Select Count(1) from RefDbVersionControl Where RVC_ParentPK = '55960FA5-F0C2-44BB-B9FB-5F49734D46BF'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new MakeRVC_ParentPKPrimaryKeyTransformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
ALTER TABLE dbo.RefDbVersionControl
DROP CONSTRAINT PK_RefDbVersionControl;";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
				cmd.CommandText = @"
INSERT INTO RefDbVersionControl (RVC_ParentPK, RVC_ParentCode)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52', 'ZZ1'),
('ED0CE102-2C22-4995-9F99-8D5825517E52', 'ZZ1'),
('55960FA5-F0C2-44BB-B9FB-5F49734D46BF', 'ZZ1')
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
