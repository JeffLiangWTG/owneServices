using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.CLASETMessageProcessor.Test;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class CLASETMessageProcessorIntegrationTest
{
	[Test]
	public void GetMessages_NoException()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		var connectionString = TestConnectionString.GetAdmin(dbName);
		using var staging = new StagingRepository(connectionString);
		var processor = new CLASETMessageProcessor(staging, ".");
		Assert.DoesNotThrow(() => processor.Process());
	}
}
