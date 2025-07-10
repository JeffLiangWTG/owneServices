using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixRefCusRateUOMAddIndexOfRateAndUOMTransformation : DataTransformation, IDataTransformationTask
	{
		public DataFixRefCusRateUOMAddIndexOfRateAndUOMTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
WITH DupRateUOM AS(
	SELECT ZXG_ZZ2_Rate, ZXG_UOM from RefCusRateUOM
	GROUP BY ZXG_ZZ2_Rate, ZXG_UOM
	HAVING COUNT(*) > 1
), ToDelete AS (
select a.*, ROW_NUMBER() OVER(PARTITION BY a.ZXG_ZZ2_Rate, a.ZXG_UOM ORDER BY a.ZXG_PK) as Row# from RefCusRateUOM a
join DupRateUOM d on a.ZXG_ZZ2_Rate = d.ZXG_ZZ2_Rate and a.ZXG_UOM = d.ZXG_UOM
)

Delete a from RefCusRateUOM a
Join ToDelete d on a.ZXG_PK = d.ZXG_PK
WHERE d.Row# > 1
";

			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
