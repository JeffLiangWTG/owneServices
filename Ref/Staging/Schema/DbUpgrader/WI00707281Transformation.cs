using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class WI00707281Transformation : DataTransformation, IDataTransformationTask
	{
		public WI00707281Transformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
DELETE QRTZ_JOB_DETAILS
WHERE JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers'
OR JOB_CLASS_NAME = 'CargoWise.RefDbRepo.Staging.Schedulers.SingleInstanceQuartzAppRunner, CargoWise.RefDbRepo.Staging.Schedulers'
";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
