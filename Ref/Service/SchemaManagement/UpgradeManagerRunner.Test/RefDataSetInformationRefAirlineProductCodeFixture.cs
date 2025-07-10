using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class RefDataSetInformationRefAirlineProductCodeFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = "SELECT RDS_DataSetId FROM RefDataSetInformation WHERE RDS_TableName = 'RefAirlineProductCode'";
			Assert.AreEqual(54, DbHelper.ExecuteScalar(Transaction, sql));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RefDataSetInformationRefAirlineProductCode(0);
		}

		protected override void PrepareTestData()
		{
			new PopulateRefDataSetInitialInformation(0).Run(Transaction);
		}
	}
}
