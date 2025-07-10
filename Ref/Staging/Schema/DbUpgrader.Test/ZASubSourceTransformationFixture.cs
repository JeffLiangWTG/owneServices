using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class ZASubSourceTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = @"SELECT COUNT(*) FROM SourceData WHERE SDA_SubSource = ''";
			Assert.AreEqual(0, DbHelper.ExecuteScalar(Transaction, sql));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new ZASubSourceTransformation(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO SourceData (SDA_PK, SDA_Source, SDA_FileType, SDA_ContentText, SDA_Status, SDA_SubSource, SDA_ContentType, SDA_SourceTime)
VALUES
	(newid(), 'ZAA', 'TXT', '', 'QUE', '', 'PRO', '2018-11-12'),
	(newid(), 'ZAA', 'TXT', '', 'QUE', '', 'MES', '2018-11-12')
";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
