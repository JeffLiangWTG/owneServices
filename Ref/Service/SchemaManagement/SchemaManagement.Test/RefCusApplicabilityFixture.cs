using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	[CreateDatabase("F46929C23F0B4F11BCB97246B0703FC4", DbSchema.RefDbRepoSafe, ActionTargets.Test)]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public class RefCusApplicabilityFixture
	{
		private string dbName => CreateDatabaseAttribute.DbNamePrefix + "F46929C23F0B4F11BCB97246B0703FC4";

		[SetUp]
		public void Setup()
		{
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				context.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_PK = Guid.NewGuid(),
					ZZZ_DataGrouping = "ZA",
					ZZZ_Description = "South Africa"
				});
				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(RefDbVersionControl)}");
			}
		}

		[Test]
		public void PrimaryKey_ShouldThrowException_WhenDuplicatePKInserted()
		{
			var pk = Guid.NewGuid();
			var tradeGroupGuid = Guid.NewGuid();
			var secondTradeGroupGuid = Guid.NewGuid();
			var tariffPk = Guid.NewGuid();
			var ratePk = Guid.NewGuid();
			var tariffTypePk = Guid.NewGuid();

			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var tariff = new RefCusTariff
				{
					ZZ1_PK = tariffPk,
					ZZ1_TariffCode = "811010",
					ZZ1_Description = "Unwrought antimony; powders",
					ZZ1_StartDate = DateTime.Now.AddDays(-1),
					ZZ1_EndDate = DateTime.Now.AddDays(1),
					ZZ1_ZZF_NKTaxOrFeeCode = "VATA",
					ZZ1_ZZZ_NKDataGrouping = "ZA",
					ZZ1_ZZI_TariffType = tariffTypePk,
					ZZ1_CompositeKeyOnZZ5 = string.Empty
				};

				var rate = new RefCusRate
				{
					ZZ2_PK = ratePk,
					ZZ2_ZZ1_Tariff = tariffPk,
					ZZ2_StartDate = DateTime.Now.AddDays(-1),
					ZZ2_EndDate = DateTime.Now.AddDays(1),
					ZZ2_RateFormula = "0.15 * VFD",
					ZZ2_SelectorFormula = "pp='EFTA'",
					ZZ2_ZZZ_NKDataGrouping = "ZA",
					ZZ2_RX_NKCurrencyOverride = "",
					ZZ2_DataSetPK = tariffPk
				};

				var tariffType = new RefCusTariffType
				{
					ZZI_PK = tariffTypePk,
					ZZI_TariffType = "TYPE1",
					ZZI_Description = "Tariff Type 1",
					ZZI_ZZZ_NKDataGrouping = "ZA"
				};

				var tradeGroup = new RefCusTradeGroup
				{
					ZZA_PK = tradeGroupGuid,
					ZZA_TradeGroup = "TG1",
					ZZA_Description = "TradeGroup 1",
					ZZA_ZZZ_NKDataGrouping = "ZA"
				};

				var secondTradeGroup = new RefCusTradeGroup
				{
					ZZA_PK = secondTradeGroupGuid,
					ZZA_TradeGroup = "TG2",
					ZZA_Description = "Second trade group",
					ZZA_ZZZ_NKDataGrouping = "ZA"
				};

				var entity1 = new RefCusApplicability
				{
					ZZT_PK = pk,
					ZZT_StartDate = DateTime.Now,
					ZZT_EndDate = DateTime.Now.AddDays(1),
					ZZT_ZZ2_Rate = ratePk,
					ZZT_ZZA_TradeGroup = tradeGroupGuid,
					ZZT_ZZA_SecondTradeGroup = secondTradeGroupGuid,
					ZZT_DataSetPK = Guid.NewGuid(),
					ZZT_AdditionalCode = "",
					ZZT_OrderNumber = ""
				};

				context.AddRange(tariff, rate, tariffType, tradeGroup, secondTradeGroup, entity1);
				context.SaveChanges();
			}

			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var duplicateEntity = new RefCusApplicability
				{
					ZZT_PK = pk,
					ZZT_StartDate = DateTime.Now,
					ZZT_EndDate = DateTime.Now.AddDays(2),
					ZZT_ZZ2_Rate = ratePk,
					ZZT_ZZA_TradeGroup = tradeGroupGuid,
					ZZT_ZZA_SecondTradeGroup = secondTradeGroupGuid,
					ZZT_DataSetPK = Guid.NewGuid(),
					ZZT_AdditionalCode = "",
					ZZT_OrderNumber = ""
				};

				context.Add(duplicateEntity);

				Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Expected PK constraint violation on ZZT_PK");
			}
		}

		[Test]
		public void StartDateShouldBeBeforeEndDate_ShouldThrowCheckConstraintViolation()
		{
			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));
			var entity = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_StartDate = new DateTime(2025, 12, 31),
				ZZT_EndDate = new DateTime(2025, 01, 01)
			};
			context.Add(entity);
			Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Expected constraint: StartDate < EndDate");
		}

		[Test]
		public void MutuallyExclusiveFields_ShouldThrowCheckConstraintViolation_WhenMultipleFieldsSet()
		{
			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));
			var entity = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZ2_Rate = Guid.NewGuid(),
				ZZT_ZX1_Conditions = Guid.NewGuid(),
				ZZT_StartDate = DateTime.Now,
				ZZT_EndDate = DateTime.Now.AddDays(1),
			};
			context.Add(entity);
			Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Expected mutually exclusive constraint violation.");
		}

		[Test]
		public void AdditionalCodeOrderNumberCheck_ShouldThrowCheckConstraintViolation_WhenNonEmptyFieldsProvided()
		{
			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));
			var entity = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZY2_AdditionalCode = Guid.NewGuid(),
				ZZT_AdditionalCode = "ABC",
				ZZT_OrderNumber = "123",
				ZZT_StartDate = DateTime.Now,
				ZZT_EndDate = DateTime.Now.AddDays(1),
			};
			context.Add(entity);
			Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Expected constraint violation on AdditionalCode/OrderNumber fields.");
		}

		[Test]
		public void ForeignKey_RefCusRate_ShouldThrowOnInvalidReference()
		{
			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));
			var entity = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZ2_Rate = Guid.NewGuid(),
				ZZT_StartDate = DateTime.Now,
				ZZT_EndDate = DateTime.Now.AddDays(1)
			};
			context.Add(entity);
			Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Expected FK violation on RefCusRate.");
		}

		[Test]
		public void ForeignKey_RefCusCondition_ShouldThrowOnInvalidReference()
		{
			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));
			var entity = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZX1_Conditions = Guid.NewGuid(),
				ZZT_StartDate = DateTime.Now,
				ZZT_EndDate = DateTime.Now.AddDays(1)
			};
			context.Add(entity);
			Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Expected FK violation on RefCusCondition.");
		}

		[Test]
		public void ForeignKey_RefCusTradeGroup_ShouldThrowOnInvalidReference()
		{
			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));
			var entity = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZA_TradeGroup = Guid.NewGuid(),
				ZZT_StartDate = DateTime.Now,
				ZZT_EndDate = DateTime.Now.AddDays(1)
			};
			context.Add(entity);
			Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Expected FK violation on RefCusTradeGroup.");
		}

		[Test]
		public void ForeignKey_RefCusTradeGroup2_ShouldThrowOnInvalidReference()
		{
			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));
			var entity = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZA_SecondTradeGroup = Guid.NewGuid(),
				ZZT_StartDate = DateTime.Now,
				ZZT_EndDate = DateTime.Now.AddDays(1)
			};
			context.Add(entity);
			Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Expected FK violation on RefCusTradeGroup2.");
		}

		[Test]
		public void ForeignKey_RefCusTariffAdditionalCode_ShouldThrowOnInvalidReference()
		{
			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));
			var entity = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZY2_AdditionalCode = Guid.NewGuid(),
				ZZT_StartDate = DateTime.Now,
				ZZT_EndDate = DateTime.Now.AddDays(1)
			};
			context.Add(entity);
			Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Expected FK violation on RefCusTariffAdditionalCode.");
		}

		[Test]
		public void ForeignKey_RefCusTariffRelationship_ShouldThrowOnInvalidReference()
		{
			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));
			var entity = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZH_TariffRelationship = Guid.NewGuid(),
				ZZT_StartDate = DateTime.Now,
				ZZT_EndDate = DateTime.Now.AddDays(1)
			};
			context.Add(entity);
			Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Expected FK violation on RefCusTariffRelationship.");
		}

		[Test]
		public void CheckConstraintViolation_ShouldThrowException()
		{
			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));

			var invalidEntity = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZ2_Rate = Guid.NewGuid(),
				ZZT_ZX1_Conditions = Guid.NewGuid(),
				ZZT_EndDate = new DateTime(2079, 06, 06),
				ZZT_StartDate = new DateTime(1900, 01, 01),
				ZZT_ZZA_TradeGroup = Guid.NewGuid(),
				ZZT_ZZH_TariffRelationship = Guid.NewGuid(),
			};

			context.Add(invalidEntity);

			Assert.Throws<DbUpdateException>(
				() => context.SaveChanges(),
				"Expected check constraint violation due to multiple mutually exclusive fields being set.");
		}

		[Test]
		public void DuplicateTariffRelationshipCombination_ShouldThrowIndexViolation()
		{
			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));

			var tariffRelId = Guid.NewGuid();
			var tradeGroupGuid = Guid.NewGuid();
			var tariffId = Guid.NewGuid();
			var tariffTypeId = Guid.Parse("9F27344A-EB2C-4252-B1FD-FEF676B4BE8F");

			var tariffType = new RefCusTariffType
			{
				ZZI_PK = tariffTypeId,
				ZZI_TariffType = "TYPE1",
				ZZI_Description = "Standard Type",
				ZZI_ZZZ_NKDataGrouping = "ZA"
			};

			var tariff = new RefCusTariff
			{
				ZZ1_PK = tariffId,
				ZZ1_TariffCode = "811010",
				ZZ1_Description = "Unwrought antimony; powders",
				ZZ1_StartDate = DateTime.Now.AddDays(-1),
				ZZ1_EndDate = DateTime.Now.AddDays(1),
				ZZ1_ZZF_NKTaxOrFeeCode = "VATA",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				ZZ1_ZZI_TariffType = tariffTypeId,
				ZZ1_CompositeKeyOnZZ5 = string.Empty
			};

			var tariffRelationship = new RefCusTariffRelationship
			{
				ZZH_PK = tariffRelId,
				ZZH_TariffCode = "95043010",
				ZZH_ZZ1_Tariff = tariffId,
				ZZH_ZZI_TariffType = tariffTypeId,
				ZZH_DataSetPK = Guid.NewGuid(),
				ZZH_DataSetCode = "ZZH",
			};

			var tradeGroup = new RefCusTradeGroup
			{
				ZZA_PK = tradeGroupGuid,
				ZZA_TradeGroup = "TG1",
				ZZA_Description = "TradeGroup 1",
				ZZA_ZZZ_NKDataGrouping = "ZA"
			};

			var nationalCode = new RefCusTariffNationalCode
			{
				ZZW_PK = Guid.NewGuid(),
				ZZW_Description = "A",
				ZZW_EndDate = new DateTime(2079, 06, 06),
				ZZW_StartDate = new DateTime(1900, 01, 01),
				ZZW_ZZ1_Tariff = tariffId,
				ZZW_ZZF_NKTaxOrFeeCode = "T",
				ZZW_NationalCode = "A",
				ZZW_ZZZ_NKDataGrouping = "ZA"
			};

			context.AddRange(tariffType, tariff, tariffRelationship, tradeGroup, nationalCode);
			context.SaveChanges();

			var applicability1 = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = "C",
				ZZT_EndDate = new DateTime(2079, 06, 06),
				ZZT_StartDate = new DateTime(1900, 01, 01),
				ZZT_OrderNumber = "2",
				ZZT_ZZA_TradeGroup = tradeGroupGuid,
				ZZT_ZZH_TariffRelationship = tariffRelationship.ZZH_PK,
				ZZT_DataSetPK = nationalCode.ZZW_ZZ1_Tariff
			};

			context.Add(applicability1);
			context.SaveChanges();

			var applicability2 = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = "C",
				ZZT_EndDate = new DateTime(2079, 06, 06),
				ZZT_StartDate = new DateTime(1900, 01, 01),
				ZZT_OrderNumber = "2",
				ZZT_ZZA_TradeGroup = tradeGroupGuid,
				ZZT_ZZH_TariffRelationship = tariffRelationship.ZZH_PK,
				ZZT_DataSetPK = nationalCode.ZZW_ZZ1_Tariff
			};

			Assert.Throws<DbUpdateException>(() =>
			{
				context.Add(applicability2);
				context.SaveChanges();
			}, "Expected unique index violation for TariffRelationship + StartDate + TradeGroup.");
		}

	}
}
