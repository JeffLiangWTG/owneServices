using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	[TransactionedTestCase]
	class BulkInsertCoreFixture
	{
		[Test]
		public async Task BulkInsertAsync()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var connectionString = TestConnectionString.GetAdmin(dbName);
			await using (var dbContext = new StagingDbContext(connectionString, false))
			{
				var codeType = new RefCusCodeType
				{
					ZZK_PK = Guid.Parse("5DE9BE79-E26D-47C3-8961-5B1E4BA4B7E5"),
					ZZK_CodeType = "AA",
					ZZK_Description = "AA Type",
					ZZK_IsReadonly = true,
					ZZK_ZZZ_NKDataGrouping = "AA"
				};
				var bulkInsertCore = new BulkInsertCore();
				var result = await bulkInsertCore.BulkInsertAsync(dbContext, [codeType], 100);
				Assert.AreEqual(1, result);
			}

			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var codeTypes = stagingRepo.Get<RefCusCodeType>().ToArray();
				Assert.AreEqual(1, codeTypes.Length);
				Assert.AreEqual("AA", codeTypes[0].ZZK_CodeType);
			}
		}
	}
}
