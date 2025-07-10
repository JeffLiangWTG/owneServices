using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class RefCusProfileQuestionRenameColumnTransformation : DataTransformation, IDataTransformationTask
	{
		public RefCusProfileQuestionRenameColumnTransformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF EXISTS(
	SELECT * FROM sys.tables tab 
	INNER JOIN sys.columns col ON tab.object_id = col.object_id
	WHERE tab.name = 'RefCusProfileQuestion' AND col.name = 'XQ2_Code'
)
BEGIN
	EXEC sp_rename 'RefCusProfileQuestion.XQ2_Code', 'XQ2_QuestionCode', 'COLUMN'
END;";

			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
