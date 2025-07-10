using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class DataFixRefStlScriptColumnsNotNullTransformation : DataTransformation, IDataTransformationTask
	{
		public DataFixRefStlScriptColumnsNotNullTransformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
Update RefStlScript set STL_CompanyCode='' where STL_CompanyCode is null;
Update RefStlScript set STL_BranchCode ='' where STL_BranchCode is null;
Update RefStlScript set STL_CreatingUserCode ='' where STL_CreatingUserCode is null;
Update RefStlScript set STL_BillingReference1 ='' where STL_BillingReference1 is null;
Update RefStlScript set STL_BillingReference2 ='' where STL_BillingReference2 is null;
Update RefStlScript set STL_BillingReference3 ='' where STL_BillingReference3 is null;
Update RefStlScript set STL_BillingReference4 ='' where STL_BillingReference4 is null;
Update RefStlScript set STL_AdditionalRefs ='' where STL_AdditionalRefs is null;
Update RefStlScript set STL_PreparationScript ='' where STL_PreparationScript is null;
Update RefStlScript set STL_WhereClause ='' where STL_WhereClause is null;
Update RefStlScript set STL_MinCW1Version ='' where STL_MinCW1Version is null;
Update RefStlScript set STL_MaxCW1Version ='' where STL_MaxCW1Version is null;
";

			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
