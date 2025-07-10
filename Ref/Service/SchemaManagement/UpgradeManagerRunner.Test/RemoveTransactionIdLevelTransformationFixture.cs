using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class RemoveTransactionIdLevelTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = "SELECT COUNT(*) FROM DataSetChangeHistory WHERE DCH_TransactionIdStartTime = '1900-01-01'";
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, sql));
			sql = "SELECT COUNT(*) FROM DataSetChangeHistory WHERE DCH_TransactionIdStartTime = '1900-01-02'";
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, sql));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RemoveTransactionIdLevelTransformation(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'DataSetChangeHistory' AND column_name = 'DCH_TransactionIdLevel')
BEGIN
	ALTER TABLE DataSetChangeHistory
	ADD DCH_TransactionIdLevel INT;
	ALTER TABLE DataSetChangeHistory
	DROP CONSTRAINT DF_DataSetChangeHistory_DCH_TransactionIdStartTime;
	DROP INDEX UX_DataSetChangeHistory_DCH_ParentCode_DCH_ParentPK_DCH_TransactionIdLevel_DCH_TransactionIdStartTime ON DataSetChangeHistory;
	ALTER TABLE DataSetChangeHistory
	DROP COLUMN DCH_TransactionIdStartTime;
END";
			DbHelper.ExecuteNonQuery(Transaction, sql);
			sql = @"
INSERT DataSetChangeHistory (DCH_ParentCode, DCH_ParentPK, DCH_TransactionId, DCH_TransactionIdLevel)
VALUES ('XXX', newid(), 1, 0),
('XXX', newid(), 1, 1)";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
