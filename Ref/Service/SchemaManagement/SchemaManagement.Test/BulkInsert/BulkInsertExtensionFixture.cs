using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	[TransactionedTestCase]
	class BulkInsertExtensionFixture
	{
		[Test]
		public async Task BulkInsertAsync()
		{
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				await context.BulkInsertAsync(LanguageTypes);
			}

			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(DbName)))
			using (var cmd = conn.CreateCommand())
			{
				await conn.OpenAsync();
				cmd.CommandText = @"SELECT Count(*) from RefLanguageType;";
				Assert.AreEqual(3, await cmd.ExecuteScalarAsync());
			}
		}

		[Test]
		public async Task BulkInsertAsync_GeometryData()
		{
			using (var context1 = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				await context1.BulkInsertAsync(PortPolygons);
			}

			using (var context2 = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				var portPolygonsFromDb = context2.RefPortPolygons;
				Assert.AreEqual(2, portPolygonsFromDb.Count());
				for (int i = 0; i < 2; i++)
				{
					Assert.AreEqual(PortPolygons[i].RPP_PortId, portPolygonsFromDb.First(x => x.RPP_PK == PortPolygons[i].RPP_PK).RPP_PortId);
					Assert.AreEqual(PortPolygons[i].RPP_SerializedPolygon, portPolygonsFromDb.First(x => x.RPP_PK == PortPolygons[i].RPP_PK).RPP_SerializedPolygon);
				}
			}
		}

		[Test]
		public async Task BulkInsertAsync_RefCusRateCode()
		{
			var rateTypePK = Guid.NewGuid();
			var dataGrouping = new RefDataGrouping { ZZZ_PK = Guid.NewGuid(), ZZZ_DataGrouping = "IE", ZZZ_Description = "group" };
			var rateType = new RefCusRateType { ZZR_PK = rateTypePK, ZZR_RateType = "IMP", ZZR_Description = "Import", ZZR_IsPayable = true, ZZR_ZZZ_NKDataGrouping = "IE", ZZR_RX_NKFormulaCurrency = "", ZZR_CustomsValueFormula = "", ZZR_IsExport = false };
			var rateCodes = new RefCusRateCode[]
			{
				new RefCusRateCode { ZY1_PK = Guid.NewGuid(), ZY1_RateCode = "1A1", ZY1_ZZR_RateType = rateTypePK, ZY1_Description = "desc 1", ZY1_InternalUse = false, ZY1_ZZZ_NKDataGrouping = "IE" },
				new RefCusRateCode { ZY1_PK = Guid.NewGuid(), ZY1_RateCode = "1B1", ZY1_ZZR_RateType = rateTypePK, ZY1_Description = "desc 2", ZY1_InternalUse = false, ZY1_ZZZ_NKDataGrouping = "IE" }
			};
			using (var context1 = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				await context1.BulkInsertAsync(new RefDataGrouping[] { dataGrouping });
				await context1.BulkInsertAsync(new RefCusRateType[] { rateType });
				await context1.BulkInsertAsync(rateCodes);
			}

			using (var context2 = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				var rateCodeFromDb = context2.RefCusRateCodes;
				Assert.AreEqual(2, rateCodeFromDb.Count());
				for (int i = 0; i < 2; i++)
				{
					Assert.AreEqual(rateCodes[i].ZY1_RateCode, rateCodeFromDb.First(x => x.ZY1_PK == rateCodes[i].ZY1_PK).ZY1_RateCode);
					Assert.AreEqual(rateCodes[i].ZY1_Description, rateCodeFromDb.First(x => x.ZY1_PK == rateCodes[i].ZY1_PK).ZY1_Description);
				}
			}
		}

		[Test]
		public async Task BulkInsertAsync_TransactionCommit()
		{
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				var dbConnection = context.Database.GetDbConnection();
				await dbConnection.OpenAsync();
				using (var transaction = await dbConnection.BeginTransactionAsync())
				{
					await context.BulkInsertAsync(LanguageTypes, transaction);
					await transaction.CommitAsync();
				}
			}

			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(DbName)))
			using (var cmd = conn.CreateCommand())
			{
				await conn.OpenAsync();
				cmd.CommandText = @"SELECT Count(*) from RefLanguageType;";
				Assert.AreEqual(3, await cmd.ExecuteScalarAsync());
			}
		}

		[Test]
		public async Task BulkInsertAsync_TransactionRollback()
		{
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				var dbConnection = context.Database.GetDbConnection();
				await dbConnection.OpenAsync();
				using (var transaction = await dbConnection.BeginTransactionAsync())
				{
					await context.BulkInsertAsync(LanguageTypes, transaction);
					await transaction.RollbackAsync();
				}
			}

			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(DbName)))
			using (var cmd = conn.CreateCommand())
			{
				await conn.OpenAsync();
				cmd.CommandText = @"SELECT Count(*) from RefLanguageType;";
				Assert.AreEqual(0, await cmd.ExecuteScalarAsync());
			}
		}

		RefLanguageType[] LanguageTypes =
		[
			new()
			{
				ZX6_PK = Guid.NewGuid(),
				ZX6_Language = "Eng",
				ZX6_Description = "English"
			},
			new()
			{
				ZX6_PK = Guid.NewGuid(),
				ZX6_Language = "Fan",
				ZX6_Description = "Fanch"
			},
			new()
			{
				ZX6_PK = Guid.NewGuid(),
				ZX6_Language = "CH",
				ZX6_Description = "Chinese"
			}
		];

		RefPortPolygon[] PortPolygons { get; } =
		[
			new()
			{
				RPP_PK = Guid.NewGuid(),
				RPP_PortId = 1,
				RPP_SerializedPolygon = new Point(-122.333056, 47.609722) { SRID = 4326 }
			},
			new()
			{
				RPP_PK = Guid.NewGuid(),
				RPP_PortId = 2,
				RPP_SerializedPolygon = new Point(-101.333056, 23.123456) { SRID = 4326 }
			}
		];

		string DbName { get; } = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
	}
}
