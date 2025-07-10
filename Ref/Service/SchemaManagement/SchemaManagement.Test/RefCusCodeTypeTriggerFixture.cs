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
	class RefCusCodeTypeTriggerFixture
	{
		[Test]
		public void TestTriggersOnZZK_ZZZ_NKDataGrouping_ThrowException()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var codeType = new RefCusCodeType
				{
					ZZK_PK = Guid.NewGuid(),
					ZZK_CodeType = "AA",
					ZZK_Description = "Description",
					ZZK_ZZZ_NKDataGrouping = ""
				};
				context.RefCusCodeTypes.Add(codeType);
				AssertThrowSqlException(context);
			}
		}

		[Test]
		public void TestTriggersOnZZK_ZZZ_NKDataGrouping_DoesNot_ThrowException()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var codeType = new RefCusCodeType
				{
					ZZK_PK = Guid.NewGuid(),
					ZZK_CodeType = "AA",
					ZZK_Description = "Description",
					ZZK_ZZZ_NKDataGrouping = "ZA"
				};
				context.RefCusCodeTypes.Add(codeType);
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
			Assert.That(exception.Message.StartsWith("A RefCusCodeType was inserted/updated with empty ZZK_ZZZ_NKDataGrouping"));
		}

		void PrepareDb(string dbName)
		{
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				context.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_PK = Guid.NewGuid(),
					ZZZ_DataGrouping = "ZA",
					ZZZ_Description = "South Africar"
				});
				context.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_PK = Guid.NewGuid(),
					ZZZ_DataGrouping = "AU",
					ZZZ_Description = "Australia"
				});
				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE {nameof(RefDbVersionControl)}");
			}
		}
	}
}
