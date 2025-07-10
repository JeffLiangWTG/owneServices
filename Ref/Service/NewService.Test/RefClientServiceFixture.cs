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
	class RefClientServiceFixture
	{
		static string TblPrefix => "RCT";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefClient);
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);

		[TestCase("2D06FD1A-307B-4A1A-B5BC-2946AFFA0874", new[] { "AAA", "BBB" })]
		[TestCase("2D06FD1A-307B-4A1A-B5BC-2946AFFA0878", new[] { "BBB" })]
		[TestCase("2D06FD1A-307B-4A1A-B5BC-2946AFFA087F", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefClient { RCT_PK = new Guid("2D06FD1A-307B-4A1A-B5BC-2946AFFA0876"), RCT_ClientID = "AAA" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.RCT_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefClient { RCT_PK = new Guid("2D06FD1A-307B-4A1A-B5BC-2946AFFA0879"), RCT_ClientID = "BBB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.RCT_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK))).Select(x => x.RCT_ClientID).ToArray();
			Assert.That(dataSets, Is.EqualTo(expected));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetDataCoreLinqCanBeTranslated_OneTableUpdateService()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repository = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repository.Add(new RefClient
				{
					RCT_PK = new Guid("2D06FD1A-307B-4A1A-B5BC-2946AFFA0876"),
					RCT_ClientID = "AAA",
					RCT_Certificate = Array.Empty<byte>(),
					RCT_Signature = Array.Empty<byte>(),
					RCT_LegacyCertificate = Array.Empty<byte>()
				});
				await repository.SaveChangesAsync();

				var service = new RefClientService(repository);
				var refClientEnumerable = service.GetData(null, Now, CheckpointHelper.Create(new Guid("2D06FD1A-307B-4A1A-B5BC-2946AFFA0874")), null, DataSetId).OrderBy(x => x.RCT_ClientID);
				Assert.DoesNotThrow(() =>
				{
					var refClientResults = refClientEnumerable.ToArray();
					Assert.NotNull(refClientResults);
				});
			}

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
			Assert.That(dataSets.Select(x => x.RCT_ClientID).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Data()
		{
			var p = CreateData(Now.AddDays(1), "ZZZ");
			p.RCT_Certificate = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RCT_ClientID, Is.EqualTo("ZZZ"));
			Assert.That(result.RCT_Certificate, Is.EqualTo(new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }));
		}

		RefClient CreateData(DateTime dateTime, string clientId)
		{
			var result = repo.Create(() => new RefClient { RCT_PK = Guid.NewGuid(), RCT_ClientID = clientId });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.RCT_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefClient> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefClient> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefClientService(repo);
			return service.GetData(dateTime, runtime ?? DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.RCT_ClientID);
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
