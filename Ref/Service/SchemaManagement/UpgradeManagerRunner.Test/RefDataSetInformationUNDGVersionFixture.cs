using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class RefDataSetInformationUNDGVersionFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = "SELECT RDS_DataSetId FROM RefDataSetInformation WHERE RDS_TableName = 'UNDGVersion'";
			Assert.That(DbHelper.ExecuteScalar(Transaction, sql), Is.EqualTo(74));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RefDataSetInformationUNDGVersion(0);
		}

		protected override void PrepareTestData()
		{
			new PopulateRefDataSetInitialInformation(0).Run(Transaction);
		}
	}
}
