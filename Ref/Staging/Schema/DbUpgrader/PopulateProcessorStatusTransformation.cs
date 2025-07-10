using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class PopulateProcessorStatusTransformation : DataTransformation, IDataTransformationTask
	{
		public PopulateProcessorStatusTransformation(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var conditionSql = @"
SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='ProcessorStatus' AND COLUMN_NAME = 'PRC_Processor';
";
			var count = DbHelper.ExecuteScalar(trans, conditionSql);
			if ((int)count > 0)
			{
				var populateSql = @"
UPDATE prc
SET prc.PRC_SchedName = job.SCHED_NAME,
	prc.PRC_JobName = job.JOB_NAME,
	prc.PRC_JobGroup = job.JOB_GROUP
FROM ProcessorStatus prc
JOIN QRTZ_JOB_DETAILS job ON prc.PRC_Processor = job.JOB_NAME;
";
				DbHelper.ExecuteNonQuery(trans, populateSql);
			}
		}
	}
}
