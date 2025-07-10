using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class RemoveRefClientUniqueConstraintTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			Assert.AreEqual(0, DbHelper.ExecuteScalar(Transaction, CheckUniqueConstraintExistsSql));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RemoveRefClientUniqueConstraintTransformation(0);
		}

		protected override void PrepareTestData()
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS(
	SELECT NULL FROM sys.tables tab
	INNER JOIN sys.key_constraints ckc ON tab.object_id=ckc.parent_object_id
	WHERE tab.name = '{0}' AND ckc.type = 'UQ'
)
BEGIN
	ALTER TABLE {0} ADD UNIQUE NONCLUSTERED ( {1} ASC) ON [PRIMARY]
END", "RefClient", "RCT_ClientID");
			DbHelper.ExecuteNonQuery(Transaction, sql);
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, CheckUniqueConstraintExistsSql));
		}

		const string CheckUniqueConstraintExistsSql = @"SELECT COUNT(*) FROM sys.tables tab
INNER JOIN sys.key_constraints ckc ON tab.object_id=ckc.parent_object_id
WHERE tab.name = 'RefClient' AND ckc.type = 'UQ'";
	}
}
