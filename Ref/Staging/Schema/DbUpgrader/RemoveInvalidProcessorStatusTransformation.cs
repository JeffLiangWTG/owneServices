using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class RemoveInvalidProcessorStatusTransformation : DataTransformation, IDataTransformationTask
	{
		public RemoveInvalidProcessorStatusTransformation(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var conditionSql = @"
SELECT count(*) FROM ProcessorStatus WHERE PRC_PK NOT IN 
(
	SELECT PRC_PK FROM ProcessorStatus JOIN QRTZ_JOB_DETAILS 
		ON PRC_JobName=JOB_NAME AND PRC_JobGroup=JOB_GROUP AND PRC_SchedName=SCHED_NAME
);
";
			var count = DbHelper.ExecuteScalar(trans, conditionSql);
			if ((int)count > 0)
			{
				var populateSql = @"
DELETE FROM ProcessorStatus WHERE PRC_PK NOT IN
(
	SELECT PRC_PK FROM ProcessorStatus JOIN QRTZ_JOB_DETAILS 
		ON PRC_JobName=JOB_NAME AND PRC_JobGroup=JOB_GROUP AND PRC_SchedName=SCHED_NAME
);
";
				DbHelper.ExecuteNonQuery(trans, populateSql);
			}
		}
	}
}
