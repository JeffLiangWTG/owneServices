using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class SqlDurationInterceptorFixture
{
	[Test]
	public async Task Test_LogSqlDuration()
	{
		var stagingDbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		var stagingDbConnectionString = TestConnectionString.GetAdmin(stagingDbName);
		using var stagingRepository = new StagingRepository(stagingDbConnectionString, interceptors: [new SqlDurationInterceptor()]);
		using var stagingDataProvider = new StagingDataProvider(stagingRepository);

		var originalOut = Console.Out;
		try
		{
			await using var sw = new StringWriter();
			Console.SetOut(sw);
			var sourceData = new SourceData
			{
				SDA_SubSource = "test",
				SDA_SourceTime = DateTime.Now
			};
			await stagingDataProvider.UpdateAllExpiredDataProcessingResultAsync(sourceData);
			
			Assert.That(sw.ToString(), Does.StartWith("$$SqlPerformance$$:UpdateAllExpiredDataProcessingResultAsync"));
		}
		finally
		{
			Console.SetOut(originalOut);
		}
	}
}
