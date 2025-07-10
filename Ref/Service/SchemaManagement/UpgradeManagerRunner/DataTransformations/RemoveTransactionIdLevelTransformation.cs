using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RemoveTransactionIdLevelTransformation : DataTransformation, IDataTransformationTask
	{
		public RemoveTransactionIdLevelTransformation(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'DataSetChangeHistory' AND column_name = 'DCH_TransactionIdStartTime')
BEGIN
	ALTER TABLE DataSetChangeHistory
	ADD DCH_TransactionIdStartTime DATETIME;
END";
			DbHelper.ExecuteNonQuery(trans, sql);

			sql = "UPDATE DataSetChangeHistory SET DCH_TransactionIdStartTime = DATEADD(day, DCH_TransactionIdLevel, '1900-01-01');";
			DbHelper.ExecuteNonQuery(trans, sql, 600);
		}
	}
}
