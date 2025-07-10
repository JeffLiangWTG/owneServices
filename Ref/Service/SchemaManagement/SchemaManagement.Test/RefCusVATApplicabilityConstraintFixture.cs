using System;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	[TransactionedTestCase]
	public class RefCusVATApplicabilityConstraintFixture
	{
		[Test]
		public void TestConstraintOnZX5_VATCategory_ThrowException()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			PrepareDb(dbName);
			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));
			var vatApplicability1 = PrepareData("FR", "");
			var vatApplicability2 = PrepareData("FR", "0001");

			context.RefCusVATApplicabilities.Add(vatApplicability1);
			Assert.DoesNotThrow(() => context.SaveChanges());

			context.RefCusVATApplicabilities.Remove(vatApplicability1);
			context.RefCusVATApplicabilities.Add(vatApplicability2);
			AssertThrowSqlException(context);
		}

		[Test]
		public void TestConstraintOnZX5_VATCategory_DoesNot_ThrowException()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			PrepareDb(dbName);
			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));
			var vatApplicability = PrepareData("FR", "A001");

			context.RefCusVATApplicabilities.Add(vatApplicability);
			Assert.DoesNotThrow(() => context.SaveChanges());
		}

		[TestCase("ZA", "", false)]
		[TestCase("ZA", "A001", true)]
		[TestCase("FR", "", false)]
		[TestCase("FR", "A001", false)]
		public void TestConstraintOnZX5_VATCategory_ZX5_ZZZ_NKDataGrouping(string dataGrouping, string category, bool throwException)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			PrepareDb(dbName);
			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));
			var data = PrepareData(dataGrouping, category);
			context.RefCusVATApplicabilities.Add(data);
			if (throwException)
			{
				AssertThrowSqlException(context);
			}
			else
			{
				Assert.DoesNotThrow(() => context.SaveChanges());
			}
		}

		void AssertThrowSqlException(SafeDbContext context)
		{
			Exception exception = null;
			try
			{
				context.SaveChanges();
			}
			catch (Exception ex)
			{
				exception = ex;
			}

			Assert.NotNull(exception);
			while (exception.InnerException != null)
			{
				exception = exception.InnerException;
			}
			Assert.That(exception.GetType(), Is.EqualTo(typeof(SqlException)));
			Assert.That(exception.Message.Contains("The INSERT statement conflicted with the CHECK constraint"));
		}

		protected RefCusVATApplicability PrepareData(string dataGrouping, string category)
		{
			var vatApplicability = new RefCusVATApplicability
			{
				ZX5_PK = Guid.NewGuid(),
				ZX5_AdditionalCode = "1",
				ZX5_Description = "A",
				ZX5_ZZ1_Tariff = tariffGuid,
				ZX5_StartDate = DateTime.Now.AddDays(-1),
				ZX5_EndDate = DateTime.Now.AddDays(1),
				ZX5_ZZF_NKTaxOrFeeCode = "MINA",
				ZX5_ZZZ_NKDataGrouping = dataGrouping,
				ZX5_VATCategory = category
			};
			return vatApplicability;
		}

		Guid tariffGuid;

		protected void PrepareDb(string dbName)
		{
			var rateTypeGuid = Guid.NewGuid();
			var tariffTypeGuid = Guid.NewGuid();
			tariffGuid = Guid.NewGuid();

			using var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName));
			context.RefDataGroupings.Add(new RefDataGrouping
			{
				ZZZ_PK = Guid.NewGuid(),
				ZZZ_DataGrouping = "ZA",
				ZZZ_Description = "South Africar"
			});
			context.RefDataGroupings.Add(new RefDataGrouping
			{
				ZZZ_PK = Guid.NewGuid(),
				ZZZ_DataGrouping = "FR",
				ZZZ_Description = "France"
			});
			context.RefCusTaxOrFeeTypes.Add(new RefCusTaxOrFeeType
			{
				ZX0_PK = Guid.NewGuid(),
				ZX0_TaxOrFeeType = "OTH",
				ZX0_Description = "Other"
			});
			context.SaveChanges();

			context.RefCusRateTypes.Add(new RefCusRateType
			{
				ZZR_PK = rateTypeGuid,
				ZZR_RateType = "EXC",
				ZZR_Description = "Excise",
				ZZR_IsPayable = true,
				ZZR_ZZZ_NKDataGrouping = "ZA",
				ZZR_CustomsValueFormula = "",
				ZZR_RX_NKFormulaCurrency = string.Empty
			});
			context.RefCusTariffTypes.Add(new RefCusTariffType
			{
				ZZI_PK = tariffTypeGuid,
				ZZI_TariffType = "1P1",
				ZZI_Description = "Schedule 1 Part 1",
				ZZI_ZZZ_NKDataGrouping = "ZA",
				ZZI_HasFormulaSpecificQuestions = false,
				ZZI_ZZR_RateType = rateTypeGuid,
				ZZI_ZZ9_NKNomenclatureGroupType = "",
			});
			context.RefCusTariffs.Add(new RefCusTariff
			{
				ZZ1_PK = tariffGuid,
				ZZ1_TariffCode = "811010",
				ZZ1_Description = "Unwrought antimony; powders",
				ZZ1_StartDate = DateTime.Now.AddDays(-1),
				ZZ1_EndDate = DateTime.Now.AddDays(1),
				ZZ1_ZZF_NKTaxOrFeeCode = "MINA",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				ZZ1_ZZI_TariffType = tariffTypeGuid,
				ZZ1_CompositeKeyOnZZ5 = string.Empty
			});
			context.RefCusTaxOrFees.Add(new RefCusTaxOrFee
			{
				ZZF_PK = Guid.NewGuid(),
				ZZF_Value = 0.04M,
				ZZF_Code = "MINA",
				ZZF_Description = "Minima",
				ZZF_StartDate = new DateTime(1900, 1, 1),
				ZZF_EndDate = new DateTime(2079, 6, 6),
				ZZF_ZZZ_NKDataGrouping = "ZA",
				ZZF_ZX0_NKTaxOrFeeType = "OTH"
			});
			context.SaveChanges();
			context.Database.ExecuteSqlRaw($"DELETE {nameof(RefDbVersionControl)}");
		}
	}
}
