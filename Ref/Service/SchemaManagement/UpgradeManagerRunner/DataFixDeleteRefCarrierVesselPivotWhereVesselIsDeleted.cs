using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixDeleteRefCarrierVesselPivotWhereVesselIsDeleted : DataTransformation, IDataTransformationTask
	{
		public DataFixDeleteRefCarrierVesselPivotWhereVesselIsDeleted(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
DELETE cvp
FROM
RefCarrierVesselPivot cvp
JOIN RefVesselZZ on ZZO_PK = cvp.ZZQ_ZZO
JOIN RefDbVersionControl on RVC_ParentPK = ZZO_PK and RVC_Deleted = 1
";

			DbHelper.ExecuteNonQuery(trans, sql, 600);
		}
	}
}
