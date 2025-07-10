using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[TestFixture]
	class StagingDbSchemaFixture
	{
		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void NoDiffWhenSchemaHasNoChanges()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var schemaUpgrade = new SchemaUpgrade("StagingDb.dacpac", TestConnectionString.DataSource, null, null);
			var diff = schemaUpgrade.GetDiffSql(dbName);
			Assert.That(diff, Is.EqualTo(string.Empty));
		}
	}
}
