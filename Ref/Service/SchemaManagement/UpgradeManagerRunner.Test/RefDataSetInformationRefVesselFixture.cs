using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class RefDataSetInformationRefVesselFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = "SELECT RDS_DataSetId FROM RefDataSetInformation WHERE RDS_TableName = 'RefVessel'";
			Assert.AreEqual(46, DbHelper.ExecuteScalar(Transaction, sql));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RefDataSetInformationRefVessel(0);
		}

		protected override void PrepareTestData()
		{
			new PopulateRefDataSetInitialInformation(0).Run(Transaction);
		}
	}
}
