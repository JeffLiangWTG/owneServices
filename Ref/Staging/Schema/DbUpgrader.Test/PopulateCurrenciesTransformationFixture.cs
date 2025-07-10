using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class PopulateCurrenciesTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = @"SELECT COUNT(*) FROM NamedEntityClassification WHERE NEC_Class = 'CURRENCY' AND NEC_Language = 'EN'";
			Assert.Greater((int)DbHelper.ExecuteScalar(Transaction, sql), 0);
		}

		protected override IDataTransformationTask GetTask()
		{
			return new PopulateCurrenciesTransformation(0);
		}

		protected override void PrepareTestData()
		{
			DbHelper.ExecuteNonQuery(Transaction, "DELETE NamedEntityClassification WHERE NEC_Class = 'CURRENCY' AND NEC_Language = 'EN'");
		}
	}
}
