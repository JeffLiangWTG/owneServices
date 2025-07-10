using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations
{
	public class MakeRVC_ParentPKPrimaryKeyTransformation : DataTransformation, IDataTransformationTask
	{
		public MakeRVC_ParentPKPrimaryKeyTransformation(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
WITH dup AS (
	SELECT *, ROW_NUMBER() OVER (PARTITION BY RVC_ParentPK ORDER BY RVC_LastUpdatedUTC DESC) AS rn
	FROM RefDbVersionControl
)
DELETE dup WHERE rn > 1
";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
