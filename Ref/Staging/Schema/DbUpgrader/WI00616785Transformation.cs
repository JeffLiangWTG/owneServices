using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class WI00616785Transformation : DataTransformation, IDataTransformationTask
	{
		public WI00616785Transformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
WITH CTE AS (
SELECT ROW_NUMBER() OVER (PARTITION BY DPR_ParentPK, DPR_SubSource ORDER BY DPR_PublicationTime DESC) AS RowNum
FROM DataProcessingResult
)
DELETE FROM CTE WHERE RowNum > 1
";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
