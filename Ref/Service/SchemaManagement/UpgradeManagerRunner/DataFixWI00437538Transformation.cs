using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00437538Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00437538Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
UPDATE t
SET t.ZZ1_Description = REPLACE(ZZ1_Description, '|', ' ')
FROM RefCusTariff t
JOIN RefCusTariffType tp on t.ZZ1_ZZI_TariffType = tp.ZZI_PK and t.ZZ1_ZZZ_NKDataGrouping = tp.ZZI_ZZZ_NKDataGrouping
where
ZZ1_ZZZ_NKDataGrouping = 'EUN'
AND ZZI_TariffType = 'IMP'
AND ZZ1_Description LIKE '%|%'";
			DbHelper.ExecuteNonQuery(trans, sql, 600);
		}
	}
}
