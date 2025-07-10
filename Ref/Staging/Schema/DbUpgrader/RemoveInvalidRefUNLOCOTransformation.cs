using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class RemoveInvalidRefUNLOCOTransformation : DataTransformation, IDataTransformationTask
	{
		public RemoveInvalidRefUNLOCOTransformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"DELETE FROM dbo.RefUNLOCO WHERE RL_Code NOT LIKE '[A-Z][A-Z][A-Z0-9][A-Z0-9][A-Z0-9]'";

			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
