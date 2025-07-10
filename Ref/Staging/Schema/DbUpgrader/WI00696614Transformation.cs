using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class WI00696614Transformation : DataTransformation, IDataTransformationTask
	{
		public WI00696614Transformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			DbHelper.SetSystemVersioningOff(trans, "QRTZ_FIRED_TRIGGERS");
			DbHelper.SetSystemVersioningOff(trans, "QRTZ_CRON_TRIGGERS");
			DbHelper.SetSystemVersioningOff(trans, "QRTZ_SIMPLE_TRIGGERS");
			DbHelper.SetSystemVersioningOff(trans, "QRTZ_TRIGGERS");
			DbHelper.SetSystemVersioningOff(trans, "QRTZ_JOB_DETAILS");
		}
	}
}
