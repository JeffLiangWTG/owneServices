using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Data.SqlClient;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test;

[TestFixture]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
[TransactionedTestCase]
class StagingRepositoryTest
{
	[Test]
	public void DeleteDisconnectedScenario()
	{
		Guid pk = Guid.Parse("DB0F1CC1-D030-4232-BA4A-7794920BE013");
		var rateType = new RefCusRateType
		{
			ZZR_ZZZ_NKDataGrouping = "ZA",
			ZZR_RateType = "DTY",
			ZZR_PK = pk,
			ZZR_IsPayable = true,
			ZZR_Description = "Duty",
			ZZR_CustomsValueFormula = "0",
			ZZR_RX_NKFormulaCurrency = ""
		};
		using (var repo = new StagingRepository(_builder))
		{
			repo.Add(rateType);
			repo.SaveChanges();
		}
		using (var repo = new StagingRepository(_builder))
		{
			rateType.ZZR_Description = "DutyUpdated";
			repo.Remove(rateType);
			repo.SaveChanges();
		}
		using (var repo = new StagingRepository(_builder))
		{
			Assert.That(repo.Get<RefCusRateType>().Count(x => x.ZZR_PK == pk && x.ZZR_Description == "Duty"), Is.EqualTo(0));
		}
	}

	[Test]
	public void DeleteConnectedScenario()
	{
		Guid pk = Guid.Parse("8FEBB35F-95C0-4E05-940E-64C412AAC6CD");
		using (var repo = new StagingRepository(_builder))
		{
			var rateType = new RefCusRateType
			{
				ZZR_ZZZ_NKDataGrouping = "ZA",
				ZZR_RateType = "DTY",
				ZZR_PK = pk,
				ZZR_IsPayable = true,
				ZZR_Description = "Duty",
				ZZR_CustomsValueFormula = "0",
				ZZR_RX_NKFormulaCurrency = ""
			};
			repo.Add(rateType);
			repo.SaveChanges();
		}
		using (var repo = new StagingRepository(_builder))
		{
			var rateType = repo.Get<RefCusRateType>().FirstOrDefault(x => x.ZZR_PK == pk);
			repo.Remove(rateType);
			repo.SaveChanges();
		}
		using (var repo = new StagingRepository(_builder))
		{
			Assert.That(repo.Get<RefCusRateType>().Count(x => x.ZZR_PK == pk && x.ZZR_Description == "Duty"), Is.EqualTo(0));
		}
	}

	[Test]
	public void UpdateDisconnectedScenario()
	{
		Guid pk = Guid.Parse("6A3952AE-30E2-42E4-B886-E81187344697");
		var rateType = new RefCusRateType
		{
			ZZR_ZZZ_NKDataGrouping = "ZA",
			ZZR_RateType = "DTY",
			ZZR_PK = pk,
			ZZR_IsPayable = true,
			ZZR_Description = "Duty",
			ZZR_CustomsValueFormula = "0",
			ZZR_RX_NKFormulaCurrency = ""
		};
		using (var repo = new StagingRepository(_builder))
		{
			repo.Add(rateType);
			repo.SaveChanges();
		}
		using (var repo = new StagingRepository(_builder))
		{
			rateType.ZZR_Description = "DutyUpdated";
			repo.Update(rateType);
			repo.SaveChanges();
			var temp = repo.Get<RefCusRateType>().Where(x => x.ZZR_PK == pk);
			Assert.That(repo.Get<RefCusRateType>().FirstOrDefault(x => x.ZZR_PK == pk)?.ZZR_Description, Is.EqualTo("DutyUpdated"));
		}
		using (var repo = new StagingRepository(_builder))
		{
			var temp = repo.Get<RefCusRateType>().Where(x => x.ZZR_PK == pk);
			Assert.That(repo.Get<RefCusRateType>().FirstOrDefault(x => x.ZZR_PK == pk)?.ZZR_Description, Is.EqualTo("DutyUpdated"));
		}
	}

	[Test]
	public void SourceDataCreatedTimeIsNotMinValue()
	{
		Guid pk = Guid.Parse("5B10E6F8-2493-47DE-8ADF-9DB910452352");
		var sourceData = new SourceData
		{
			SDA_PK = pk,
			SDA_Source = "INT",
			SDA_Filename = "C:\test",
			SDA_Filetype = "XML",
			SDA_ContentType = "URD",
			SDA_ContentText = "<test></test>",
			SDA_Status = "QUE",
			SDA_SourceTime = DateTime.UtcNow,
			SDA_SubSource = "sub"
		};
		using (var repo = new StagingRepository(_builder))
		{
			repo.Add(sourceData);
			repo.SaveChanges();
		}
		using (var repo = new StagingRepository(_builder))
		{
			Assert.That(repo.Get<SourceData>().FirstOrDefault(x => x.SDA_PK == pk)!.SDA_CreatedTime, Is.Not.EqualTo(DateTime.MinValue));
		}
	}

	[Test]
	public void DatabaseExists()
	{
		//invalid db
		var dbName = "NonExistDb";
		var builderNew = TestConnectionString.GetAdmin(dbName);
		using (var repo = new StagingRepository(builderNew))
		{
			Assert.IsFalse(repo.DatabaseExists);
		}

		//existing db
		using (var repo = new StagingRepository(_builder))
		{
			Assert.IsTrue(repo.DatabaseExists);
		}
	}

	[Test]
	public void GetTemporalStartAndEndTime()
	{
		var accTaxRate = new RefAccTaxRate { ZAT_PK = Guid.NewGuid(), ZAT_RN_NKCountry = "AU", ZAT_ReferenceRateType = "RRT" };
		var applicationAttributeType = new RefApplicationAttributeType { RAT_PK = Guid.NewGuid(), RAT_Description = "any", RAT_Type = "any" };
		var applicationAttribute = new RefApplicationAttribute { RAA_PK = Guid.NewGuid(), RAA_RAT_NKType = "any" };
		using (var repo = new StagingRepository(_builder))
		{
			repo.Add(accTaxRate);
			repo.Add(applicationAttributeType);
			repo.Add(applicationAttribute);
			repo.SaveChanges();
		}
		using (var repo = new StagingRepository(_builder))
		{
			var accTaxRateFromDb = repo.Get<RefAccTaxRate>().FirstOrDefault(x => x.ZAT_PK == accTaxRate.ZAT_PK);
			var temporalData = repo.GetTemporalStartAndEndTime(accTaxRateFromDb);
			Assert.That(!temporalData.HasValue);

			var applicationAttributeFromDb = repo.Get<RefApplicationAttribute>().FirstOrDefault(x => x.RAA_PK == applicationAttribute.RAA_PK);
			temporalData = repo.GetTemporalStartAndEndTime(applicationAttributeFromDb);
			Assert.That(temporalData.HasValue);
			Assert.That(temporalData.Value.sysStartTime, Is.Not.Null);
			Assert.That(temporalData.Value.sysEndTime, Is.Not.Null);
		}
	}

	[Test]
	public void UpdateConnectedScenario()
	{
		Guid pk = Guid.Parse("3B4901EE-CDE1-46BD-B156-73B2CCE12E6E");
		using (var repo = new StagingRepository(_builder))
		{
			var rateType = new RefCusRateType
			{
				ZZR_ZZZ_NKDataGrouping = "ZA",
				ZZR_RateType = "DTY",
				ZZR_PK = pk,
				ZZR_IsPayable = true,
				ZZR_Description = "Duty",
				ZZR_CustomsValueFormula = "0",
				ZZR_RX_NKFormulaCurrency = ""
			};
			repo.Add(rateType);
			repo.SaveChanges();
			Assert.That(repo.AffectedRecords == 1);
		}
		using (var repo = new StagingRepository(_builder))
		{
			var rateType = repo.Get<RefCusRateType>().FirstOrDefault(x => x.ZZR_PK == pk);
			rateType.ZZR_Description = "DutyUpdated";
			repo.Update(rateType);
			repo.SaveChanges();
		}
		using (var repo = new StagingRepository(_builder))
		{
			Assert.That(repo.Get<RefCusRateType>().FirstOrDefault(x => x.ZZR_PK == pk)?.ZZR_Description, Is.EqualTo("DutyUpdated"));
		}
	}

	[Test]
	public void AddOrUpdate()
	{
		var pk = Guid.Parse("206A9058-EBD7-44ED-9637-52CCD9DF112A");
		using (var repo = new StagingRepository(_builder))
		{
			var rateType = new RefCusRateType
			{
				ZZR_ZZZ_NKDataGrouping = "ZA",
				ZZR_RateType = "DTY",
				ZZR_PK = pk,
				ZZR_IsPayable = true,
				ZZR_Description = "Duty",
				ZZR_CustomsValueFormula = "0",
				ZZR_RX_NKFormulaCurrency = ""
			};
			repo.AddOrUpdate(rateType);
			repo.SaveChanges();
			Assert.That(repo.AffectedRecords == 1);
		}
		using (var repo = new StagingRepository(_builder))
		{
			var rateType = repo.Get<RefCusRateType>().FirstOrDefault(x => x.ZZR_PK == pk);
			rateType.ZZR_Description = "DutyUpdated";
			repo.AddOrUpdate(rateType);
			repo.SaveChanges();
		}
		using (var repo = new StagingRepository(_builder))
		{
			Assert.That(repo.Get<RefCusRateType>().FirstOrDefault(x => x.ZZR_PK == pk)?.ZZR_Description, Is.EqualTo("DutyUpdated"));
		}
	}

	[Test]
	public async Task BulkInsertDbGeography()
	{
		using var repo = new StagingRepository(_builder);
		var unloco = new RefUNLOCO
		{
			RL_Code = "TESTT",
			RL_GeoLocation = new Point(10, 20) { SRID = 4326 },
			RL_PortName = "Test",
			RL_NameWithDiacriticals = "Test",
			RL_IATA = "TST",
			RL_IATARegionCode = "TST",
			RL_CoOrdinates = "12E 12N",
			RL_RN_NKCountryCode = "AU",
			RL_IsActive = true
		};
		await repo.BulkInsertWithRetryAsync(new[] { unloco });
		await repo.SaveChangesAsync();
		Assert.That(repo.Get<RefUNLOCO>().Count() == 1);
	}

	[Test]
	public void BulkInsertChecksConstraints()
	{
		using var repo = new StagingRepository(_builder);
		var tariffUOM = new RefCusTariffUOM
		{
			ZZ8_PK = Guid.NewGuid(),
			ZZ8_Type = "CU1",
			ZZ8_UOM = "U",
			ZZ8_ZZ1_Tariff = Guid.NewGuid(),
			ZZ8_ZZA_NKTradeGroup = "AU",
			ZZ8_ZZA_ZZZ_NKDataGrouping = "AU",
			ZZ8_ZZZ_NKDataGrouping = "AU"
		};
		Assert.That(async () => await repo.BulkInsertWithRetryAsync(new[] { tariffUOM }),
			Throws.Exception.TypeOf(typeof(SqlException)).With.Message.Contain(@"The INSERT statement conflicted with the FOREIGN KEY constraint"));
	}

	[Test]
	public void BulkInsertWithRetryAsync()
	{
		using var repo = new StagingRepository(_builder);
		var codeType = new RefCusCodeType
		{
			ZZK_PK = Guid.Parse("5DE9BE79-E26D-47C3-8961-5B1E4BA4B7E5"),
			ZZK_CodeType = "AA",
			ZZK_Description = "AA Type",
			ZZK_IsReadonly = true,
			ZZK_ZZZ_NKDataGrouping = "AA"
		};
		var codeTypes = new[] { codeType };
		var sqlException = new SqlExceptionBuilder().WithErrorNumber(KnownSqlExceptionsNumbers.Timeout).Build();
		var bulkInsertCoreMock = new Mock<IBulkInsertCore>();
		bulkInsertCoreMock.Setup(x => x.BulkInsertAsync(It.IsAny<StagingDbContext>(), codeTypes, 100, null)).Throws(sqlException);

		Assert.ThrowsAsync<SqlException>(async () => await repo.BulkInsertCore(bulkInsertCoreMock.Object, codeTypes, 100));
		bulkInsertCoreMock.Verify(x => x.BulkInsertAsync(It.IsAny<StagingDbContext>(), codeTypes, 100, null), Times.Once);

		bulkInsertCoreMock.Invocations.Clear();
		sqlException = new SqlExceptionBuilder().WithErrorNumber(KnownSqlExceptionsNumbers.TransactionDeadlock).Build();
		bulkInsertCoreMock.Setup(x => x.BulkInsertAsync(It.IsAny<StagingDbContext>(), codeTypes, 100, null)).Throws(sqlException);
		Assert.ThrowsAsync<SqlException>(async () => await repo.BulkInsertCore(bulkInsertCoreMock.Object, codeTypes, 100));
		bulkInsertCoreMock.Verify(x => x.BulkInsertAsync(It.IsAny<StagingDbContext>(), codeTypes, 100, null), Times.Exactly(3));
	}

	[Test]
	public void NoTableContainIsSystemField()
	{
		var result = new List<string>();
		using var connection = new SqlConnection(_builder);
		using var reader = connection.ExecuteReader(@"select t.name [TableName]
from sys.tables t
join sys.columns c on t.object_id = c.object_id
Where c.[name] like '%IsSystem'");
		while (reader.Read())
		{
			result.Add(reader.GetString(0));
		}

		Assert.AreEqual(0, result.Count, $"The following table(s) contains IsSystem columns: {string.Join(",", result)}");
	}

	string _builder;

	[SetUp]
	public void SetUp()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		_builder = TestConnectionString.GetAdmin(dbName);
	}
}
