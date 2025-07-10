using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class RefCusProfileQuestionRenameColumnTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(1) FROM sys.tables tab 
	INNER JOIN sys.columns col ON tab.object_id = col.object_id
	WHERE tab.name = 'RefCusProfileQuestion' AND col.name = 'XQ2_QuestionCode';";
				Assert.That((int)cmd.ExecuteScalar(), Is.EqualTo(1));

				cmd.CommandText = @"SELECT COUNT(1) FROM sys.tables tab 
	INNER JOIN sys.check_constraints ckc ON tab.object_id = ckc.parent_object_id
	WHERE tab.name = 'RefCusProfileQuestion' AND ckc.name = 'CK_RefCusProfileQuestion_XQ2_QuestionCode';";
				Assert.That((int)cmd.ExecuteScalar(), Is.EqualTo(1));

				cmd.CommandText = @"SELECT COUNT(1)
	FROM sys.indexes ind
	WHERE ind.name = 'IX_RefCusProfileQuestion_XQ2_QuestionCode_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate';";
				Assert.That((int)cmd.ExecuteScalar(), Is.EqualTo(1));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RefCusProfileQuestionRenameColumnTransformation(120);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
ALTER TABLE RefCusProfileQuestion DROP CONSTRAINT CK_RefCusProfileQuestion_XQ2_QuestionCode;
DROP INDEX IX_RefCusProfileQuestion_XQ2_QuestionCode_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate ON RefCusProfileQuestion;

EXEC sp_rename 'RefCusProfileQuestion.XQ2_QuestionCode', 'XQ2_Code', 'COLUMN';

EXEC('
	ALTER TABLE RefCusProfileQuestion ADD CONSTRAINT CK_RefCusProfileQuestion_XQ2_Code CHECK (XQ2_Code <> '''');
');
EXEC('
	CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfileQuestion_XQ2_Code_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate 
	ON RefCusProfileQuestion (XQ2_Code ASC, XQ2_XXX_ProfileType ASC, XQ2_ZZZ_NKDataGrouping ASC, XQ2_StartDate ASC);
');
";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
