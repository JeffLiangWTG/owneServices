using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefCusProfileQuestionRenameColumnTransformation : DataTransformation, IDataTransformationTask
	{
		public RefCusProfileQuestionRenameColumnTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF EXISTS(
	SELECT NULL FROM sys.tables tab 
	INNER JOIN sys.check_constraints ckc ON tab.object_id = ckc.parent_object_id
	WHERE tab.name = 'RefCusProfileQuestion' AND ckc.name = 'CK_RefCusProfileQuestion_XQ2_Code'
)
BEGIN
	ALTER TABLE RefCusProfileQuestion DROP CONSTRAINT CK_RefCusProfileQuestion_XQ2_Code;
END

IF EXISTS(
	SELECT null
	FROM sys.indexes ind
	WHERE ind.name = 'IX_RefCusProfileQuestion_XQ2_Code_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate'
)
BEGIN
	DROP INDEX IX_RefCusProfileQuestion_XQ2_Code_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate ON RefCusProfileQuestion;
END

IF EXISTS(
	SELECT * FROM sys.tables tab 
	INNER JOIN sys.columns col ON tab.object_id = col.object_id
	WHERE tab.name = 'RefCusProfileQuestion' AND col.name = 'XQ2_Code'
)
BEGIN
	EXEC sp_rename 'RefCusProfileQuestion.XQ2_Code', 'XQ2_QuestionCode', 'COLUMN';
END

IF NOT EXISTS(
	SELECT NULL FROM sys.tables tab 
	INNER JOIN sys.check_constraints ckc ON tab.object_id = ckc.parent_object_id
	WHERE tab.name = 'RefCusProfileQuestion' AND ckc.name = 'CK_RefCusProfileQuestion_XQ2_QuestionCode'
)
BEGIN
	EXEC('
		ALTER TABLE RefCusProfileQuestion ADD CONSTRAINT CK_RefCusProfileQuestion_XQ2_QuestionCode CHECK (XQ2_QuestionCode <> '''');
	');
END
IF NOT EXISTS(
	SELECT null
	FROM sys.indexes ind
	WHERE ind.name = 'IX_RefCusProfileQuestion_XQ2_QuestionCode_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate'
)
BEGIN
	EXEC('
		CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfileQuestion_XQ2_QuestionCode_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate 
		ON RefCusProfileQuestion (XQ2_QuestionCode ASC, XQ2_XXX_ProfileType ASC, XQ2_ZZZ_NKDataGrouping ASC, XQ2_StartDate ASC);
	');
END
";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
