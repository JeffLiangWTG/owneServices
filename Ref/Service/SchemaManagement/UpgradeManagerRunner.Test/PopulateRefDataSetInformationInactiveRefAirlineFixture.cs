using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class PopulateRefDataSetInformationInactiveRefAirlineFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = "SELECT RDS_DataSetId FROM RefDataSetInformation WHERE RDS_TableName = 'RefAirline' AND RDS_DataSetName = 'InactiveRefAirline'";
			Assert.AreEqual(202, DbHelper.ExecuteScalar(Transaction, sql));

			sql = "SELECT COUNT(*) FROM RefDataSetInformationDefinition WHERE RDD_DataSetId = 202";
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, sql));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new PopulateRefDataSetInformationInactiveRefAirline(0);
		}

		protected override void PrepareTestData()
		{
			new PopulateRefDataSetInitialInformation(0).Run(Transaction);
			var sql = @"
INSERT RefDataSetInformation(RDS_PK, RDS_DataSetId, RDS_DataSetName, RDS_TableName, RDS_DataSetTableCode, RDS_PriorityLevel)
	VALUES(newid(), 43, 'RefAirline', 'RefAirline', 'RM', 0);
";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
