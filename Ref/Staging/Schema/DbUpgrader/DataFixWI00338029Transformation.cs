using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class DataFixWI00338029Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00338029Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
UPDATE RefCusProcedure SET ZZ6_IntoWarehouse = (CASE WHEN ZZ6_IntoWarehouse = '1' THEN 'Y' ELSE 'N' END) WHERE ZZ6_IntoWarehouse NOT IN ('Y','N','I');
UPDATE RefCusProcedure SET ZZ6_OutOfWarehouse = (CASE WHEN ZZ6_OutOfWarehouse = '1' THEN 'Y' ELSE 'N' END) WHERE ZZ6_OutOfWarehouse NOT IN ('Y','N','I');
";

			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
