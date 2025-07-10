using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class RecursiveDeletionFixture
	{
		string GetConnectionString()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			var connectionString = TestConnectionString.GetAdmin(dbName);
			return connectionString;
		}

		[Test]
		public async Task RecursiveDeletion()
		{
			var connectionString = GetConnectionString();
			using var entities = new ReferenceDataRepository(false, connectionString);
			var refDataGrouping = new RefDataGrouping
			{
				ZZZ_PK = Guid.NewGuid(),
				ZZZ_DataGrouping = "ZA",
				ZZZ_Description = "South Africa"
			};
			entities.Add(refDataGrouping);
			await entities.SaveChangesAsync(null);

			var tariffType = new RefCusTariffType
			{
				ZZI_Description = "type",
				ZZI_PK = Guid.NewGuid(),
				ZZI_TariffType = "TA",
				ZZI_ZZZ_NKDataGrouping = "ZA",
				ZZI_HasFormulaSpecificQuestions = true,
				ZZI_ZZ9_NKNomenclatureGroupType = ""
			};
			entities.Add(tariffType);
			await entities.SaveChangesAsync(null);

			var tariff = new RefCusTariff
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_ZZI_TariffType = tariffType.ZZI_PK,
				ZZ1_TariffCode = "020322",
				ZZ1_IAMUnique = 0,
				ZZ1_Description = "Description",
				ZZ1_StartDate = new DateTime(2024, 1, 1),
				ZZ1_EndDate = new DateTime(2079, 6, 6),
				ZZ1_ZZF_NKTaxOrFeeCode = "VAT",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				ZZ1_CompositeKeyOnZZ5 = ""
			};
			var refCusRate = new RefCusRate
			{
				ZZ2_PK = Guid.NewGuid(),
				ZZ2_ZZ1_Tariff = tariff.ZZ1_PK,
				ZZ2_StartDate = new DateTime(1900, 1, 1),
				ZZ2_EndDate = new DateTime(2000, 1, 1),
				ZZ2_RateFormula = "Rate",
				ZZ2_SelectorFormula = ">",
				ZZ2_ZZZ_NKDataGrouping = "BR",
				ZZ2_RX_NKCurrencyOverride = ""
			};
			entities.Add(tariff);
			entities.Add(refCusRate);
			await entities.SaveChangesAsync(null);

			Assert.That(entities.Get<RefCusTariff>().Any());
			Assert.That(entities.Get<RefCusTariffType>().Any());
			Assert.That(entities.Get<RefCusRate>().Any());

			await entities.RecursiveDeleteAsync(entities.Get<RefCusTariff>());

			Assert.That(!entities.Get<RefCusTariff>().Any());
			Assert.That(!entities.Get<RefCusRate>().Any());
		}
	}
}
