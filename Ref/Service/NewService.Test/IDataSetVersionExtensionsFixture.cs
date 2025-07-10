using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class IDataSetVersionExtensionsFixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task FilterView()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repo = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repo.Add(new RefDataGrouping
				{
					ZZZ_PK = Guid.NewGuid(),
					ZZZ_DataGrouping = "ZA",
					ZZZ_Description = "South Africa"
				});
				await repo.SaveChangesAsync();
				var tariffTypeGuid = Guid.NewGuid();
				repo.Add(new RefCusTariffType
				{
					ZZI_PK = tariffTypeGuid,
					ZZI_TariffType = "1P1",
					ZZI_Description = "Schedule 1 Part 1",
					ZZI_ZZZ_NKDataGrouping = "ZA",
					ZZI_HasFormulaSpecificQuestions = false,
					ZZI_ZZR_RateType = null,
					ZZI_ZZ9_NKNomenclatureGroupType = "",
				});
				repo.Add(new RefCusTariff
				{
					ZZ1_PK = Guid.NewGuid(),
					ZZ1_TariffCode = "811010",
					ZZ1_Description = "Unwrought antimony; powders",
					ZZ1_StartDate = DateTime.Now.AddDays(-1),
					ZZ1_EndDate = DateTime.Now.AddDays(1),
					ZZ1_ZZF_NKTaxOrFeeCode = "VAT",
					ZZ1_ZZZ_NKDataGrouping = "ZA",
					ZZ1_ZZI_TariffType = tariffTypeGuid,
					ZZ1_CompositeKeyOnZZ5 = string.Empty
				});
				await repo.SaveChangesAsync();
				Assert.DoesNotThrow(() => repo.Get<RefCusTariffView>().FilterView(null, DateTime.UtcNow.AddDays(1), null, 1).ToArray());
				Assert.DoesNotThrow(() => repo.Get<RefCusTariffView>().FilterView(null, DateTime.UtcNow.AddDays(1), CheckpointHelper.Create(Guid.NewGuid()), 1).ToArray());
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void GetMaxLastUpdatedUTCGeneratedSqlQuery()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repo = new LoggingReferenceDataRepository(GetConnectionString(dbName)))
			{
				repo.Get<RefDataSetInformation>().Where(x => x.RDS_DataSetTableCode == "ZZI").GetMaxLastUpdatedUTC();
				var query = repo.GetRecentExecutedSqlQuery().Replace("\r", string.Empty);
				var substring = @"
      SELECT MAX([r].[RDS_LastUpdatedUTC])
      FROM [RefDataSetInformation] AS [r]
      WHERE [r].[RDS_DataSetTableCode] = 'ZZI'
".Replace("\r", string.Empty);

				Assert.That(query, Does.Contain(substring));
			}
		}
	}
}
