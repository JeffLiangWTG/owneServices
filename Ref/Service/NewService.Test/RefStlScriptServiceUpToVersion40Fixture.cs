using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefStlScriptServiceUpToVersion40Fixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);
		static string TblPrefix => "STL";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefStlScript);

		[TestCase("2D06FD1A-307B-4A1A-B5BC-2946AFFA0874", new[] { "AAA", "BBB" })]
		[TestCase("2D06FD1A-307B-4A1A-B5BC-2946AFFA0878", new[] { "BBB" })]
		[TestCase("2D06FD1A-307B-4A1A-B5BC-2946AFFA087F", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefStlScript { STL_PK = new Guid("2D06FD1A-307B-4A1A-B5BC-2946AFFA0876"), STL_FeatureCode = "AAA" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.STL_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefStlScript { STL_PK = new Guid("2D06FD1A-307B-4A1A-B5BC-2946AFFA0879"), STL_FeatureCode = "BBB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.STL_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK))).Select(x => x.STL_FeatureCode).ToArray();
			Assert.That(dataSets, Is.EqualTo(expected));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetDataCoreLinqCanBeTranslated_RefStlScriptServiceUpToVersion40()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repository = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repository.Add(new RefStlScript
				{
					STL_PK = new Guid("2D06FD1A-307B-4A1A-B5BC-2946AFFA0876"),
					STL_FeatureCode = "AAA",
					STL_RoleName = "test role",
					STL_ModuleName = "test module",
					STL_FunctionName = "test function",
					STL_FeatureName = "test feature",
					STL_DataGranularity = "TRN",
					STL_TransactionDateUtc = "ce.CE_SystemCreateTimeUtc",
					STL_GuidReference = "ulb.ULB_PK",
					STL_TransactionCount = "1",
					STL_FromClause = "FROM dbo.CE ce",
					STL_ActiveOn = "TST",
					STL_DateType = "DTE"
				});
				await repository.SaveChangesAsync();

				var service = new RefStlScriptService(repository);
				var refStlScriptEnumerable = service.GetDataUpToV40(null, Now, CheckpointHelper.Create(new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5")), null, DataSetId).OrderBy(x => x.STL_FeatureCode);
				Assert.DoesNotThrow(() =>
				{
					var refStlScriptResults = refStlScriptEnumerable.ToArray();
					Assert.NotNull(refStlScriptResults);
				});
			}
		}

		[Test]
		public void FilterWithLatestDatasetWithOldIndex()
		{
			var dataset1 = repo.Create(() => new RefStlScript { STL_PK = new Guid("2D06FD1A-307B-4A1A-B5BC-2946AFFA0876"), STL_FeatureCode = "AAA", STL_ActiveOn = "A", STL_MinCW1Version = "1", STL_MaxCW1Version = "2" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.STL_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefStlScript { STL_PK = new Guid("2D06FD1A-307B-4A1A-B5BC-2946AFFA0879"), STL_FeatureCode = "AAA", STL_ActiveOn = "A", STL_MinCW1Version = "1", STL_MaxCW1Version = "10" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now.AddDays(-10), RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.STL_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid("2D06FD1A-307B-4A1A-B5BC-2946AFFA0874")));
			Assert.That(dataSets.Count(), Is.EqualTo(1));
			Assert.That(dataSets.First().STL_MaxCW1Version, Is.EqualTo("2"));
		}

		[TestCase(-1, 3, new[] { "AAA", "BBB" })]
		[TestCase(1, 3, new[] { "BBB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AAA", "BBB" })]
		[TestCase(null, 1, new[] { "AAA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Data_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateData(Now, "AAA");
			var p2 = CreateData(Now.AddDays(2), "BBB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.STL_FeatureCode).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Data()
		{
			var p = CreateData(Now.AddDays(1), "ZZZ");
			p.STL_FeatureName = "Test";
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.STL_FeatureCode, Is.EqualTo("ZZZ"));
			Assert.That(result.STL_FeatureName, Is.EqualTo("Test"));
		}

		[Test]
		public void GetActiveDataOnlyWhenOldIndexHasBothActiveAndDeletedRecords()
		{
			var dataset1 = repo.Create(() => new RefStlScript { STL_PK = new Guid("{61708B31-C418-4402-8101-1CB0C7AF8FF6}"), STL_FeatureCode = "AAA", STL_ActiveOn = "TST" });
			var dataset2 = repo.Create(() => new RefStlScript { STL_PK = new Guid("{CF2D5B63-2020-4813-843C-4FB00C9AA5D2}"), STL_FeatureCode = "AAA", STL_ActiveOn = "TST", STL_MinCW1Version = "1"});
			var dataset3 = repo.Create(() => new RefStlScript { STL_PK = new Guid("{E2AE62B2-8A7D-43F8-AB0C-4A1936F00399}"), STL_FeatureCode = "AAA", STL_ActiveOn = "TST", STL_MinCW1Version = "1", STL_MaxCW1Version = "10" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.STL_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true, RVC_Deleted = true });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.STL_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now.AddDays(1), RVC_DataSetId = DataSetId, RVC_ParentPK = dataset3.STL_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null);
			Assert.AreEqual(1, dataSets.Count());
			Assert.AreEqual(false, dataSets.First().Deleted);
			Assert.AreEqual("10", dataSets.First().STL_MaxCW1Version);
		}

		RefStlScript CreateData(DateTime dateTime, string featureCode)
		{
			var result = repo.Create(() => new RefStlScript { STL_PK = Guid.NewGuid(), STL_FeatureCode = featureCode });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.STL_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefStlScript> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefStlScript> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefStlScriptService(repo);
			return service.GetDataUpToV40(dateTime, runtime ?? DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.STL_FeatureCode);
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
