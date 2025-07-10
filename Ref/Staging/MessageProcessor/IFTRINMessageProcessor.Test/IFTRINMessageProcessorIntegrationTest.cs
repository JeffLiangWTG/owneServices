using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.IFTRINMessageProcessor.Test;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class IFTRINMessageProcessorIntegrationTest
{
	[Test]
	public void GetMessages_NoException()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		var connectionString = TestConnectionString.GetAdmin(dbName);
		using var staging = new StagingRepository(connectionString);
		var processor = new SGIFTRINMessageProcessor(staging, ".");
		Assert.DoesNotThrow(() => processor.Process());
	}
}
