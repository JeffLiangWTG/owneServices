using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusRulingServiceFixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);
		static string TblPrefix => "ZZX";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusRuling);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusRuling { ZZX_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZX_RulingNumber = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZX_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusRuling { ZZX_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZX_RulingNumber = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZX_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZX_RulingNumber).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetDataCoreLinqCanBeTranslated_ReferenceDataServiceBase()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repository = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repository.Add(new RefCusRuling
				{
					ZZX_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"),
					ZZX_RulingNumber = "BB",
					ZZX_Description = "test refCusRuling",
					ZZX_RN_NKCountryCode = "NZ",
					ZZX_RulingType = "ABC"
				});
				await repository.SaveChangesAsync();

				var service = new RefCusRulingService(repository);
				var refCusRulingEnumerable = service.GetData(null, Now, CheckpointHelper.Create(new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5")), null, DataSetId).OrderBy(x => x.ZZX_RulingNumber);
				Assert.DoesNotThrow(() =>
				{
					var refCusRulingResults = refCusRulingEnumerable.ToArray();
					Assert.NotNull(refCusRulingResults);
				});
			}
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Ruling_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateRuling(Now, "AA");
			var p2 = CreateRuling(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZX_RulingNumber).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Ruling_Data()
		{
			var p = repo.Create(() => new RefCusRuling
			{
				ZZX_PK = Guid.NewGuid(),
				ZZX_RN_NKCountryCode = "NZ",
				ZZX_RulingType = "GST",
				ZZX_Description = "D"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZZX_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZX_RN_NKCountryCode, Is.EqualTo(p.ZZX_RN_NKCountryCode));
			Assert.That(result.ZZX_RulingNumber, Is.EqualTo(p.ZZX_RulingNumber));
			Assert.That(result.ZZX_RulingType, Is.EqualTo(p.ZZX_RulingType));
			Assert.That(result.ZZX_Description, Is.EqualTo(p.ZZX_Description));
		}

		[Test]
		public void GetLatest_RulingConfig_Data()
		{
			var ruling = CreateRuling(Now.AddDays(1), "Ruling Number");
			var rulingConfig = repo.Create(() => new RefCusRulingConfig
			{
				ZZY_PK = Guid.NewGuid(),
				ZZY_ZZX_CusRuling = ruling.ZZX_PK,
				ZZY_Type = "T",
				ZZY_Rate = 1,
				ZZY_Value = "V"
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusRulingConfigs.ElementAt(0);
			Assert.That(1, Is.EqualTo(dataSets.ElementAt(0).RefCusRulingConfigs.Length));
			Assert.That(result.ZZY_Type, Is.EqualTo(rulingConfig.ZZY_Type));
			Assert.That(result.ZZY_Rate, Is.EqualTo(rulingConfig.ZZY_Rate));
			Assert.That(result.ZZY_Value, Is.EqualTo(rulingConfig.ZZY_Value));
		}

		RefCusRuling CreateRuling(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusRuling { ZZX_PK = Guid.NewGuid(), ZZX_RulingNumber = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZX_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusRuling> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusRuling> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusRulingService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZX_RulingNumber);
		}

		static void SetUpRepo<T>(Mock<ObjectReferenceDataRepository> repoMock, params T[] objs) where T : class
		{
			repoMock.Setup(x => x.Get<T>()).Returns(objs.AsQueryable());
		}

		DateTime Now;
		ObjectReferenceDataRepository repo;
		[SetUp]
		public void SetUp()
		{
			Now = DateTime.UtcNow;
			repo = new ObjectReferenceDataRepository();
		}
	}
}
