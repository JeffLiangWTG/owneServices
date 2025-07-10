using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class RefCusProfileQuestionRenameColumnTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = @"SELECT COUNT(1) FROM sys.tables tab 
	INNER JOIN sys.columns col ON tab.object_id = col.object_id
	WHERE tab.name = 'RefCusProfileQuestion' AND col.name = 'XQ2_QuestionCode'";
			Assert.That((int)DbHelper.ExecuteScalar(Transaction, sql), Is.EqualTo(1));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RefCusProfileQuestionRenameColumnTransformation(22);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
EXEC sp_rename 'RefCusProfileQuestion.XQ2_QuestionCode', 'XQ2_Code', 'COLUMN';
";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
