using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class QuartzJobTypeTransformationTask : DataTransformation, IDataTransformationTask
	{
		public QuartzJobTypeTransformationTask(int version) : base(version) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF EXISTS (SELECT 1 FROM QRTZ_JOB_DETAILS WHERE JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.QuartzAppRunner, CargoWise.RefDbRepo.Staging.Schedulers')
BEGIN
	UPDATE QRTZ_JOB_DETAILS
	SET JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzAppRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common'
	WHERE JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.QuartzAppRunner, CargoWise.RefDbRepo.Staging.Schedulers'
END

IF EXISTS (SELECT 1 FROM QRTZ_JOB_DETAILS WHERE JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.QuartzDataUpdater, CargoWise.RefDbRepo.Staging.Schedulers')
BEGIN
	UPDATE QRTZ_JOB_DETAILS
	SET JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzDataUpdater, CargoWise.RefDbRepo.Staging.Schedulers.Common'
	WHERE JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.QuartzDataUpdater, CargoWise.RefDbRepo.Staging.Schedulers'
END

IF EXISTS (SELECT 1 FROM QRTZ_JOB_DETAILS WHERE JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers')
BEGIN
	UPDATE QRTZ_JOB_DETAILS
	SET JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common'
	WHERE JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers'
END

IF EXISTS (SELECT 1 FROM QRTZ_JOB_DETAILS WHERE JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.SingleInstanceQuartzAppRunner, CargoWise.RefDbRepo.Staging.Schedulers')
BEGIN
	UPDATE QRTZ_JOB_DETAILS
	SET JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.Common.SingleInstanceQuartzAppRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common'
	WHERE JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.SingleInstanceQuartzAppRunner, CargoWise.RefDbRepo.Staging.Schedulers'
END
";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
