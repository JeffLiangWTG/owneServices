using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusProfileTypeServiceFixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);
		static string TblPrefix => "XXX";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusProfileType);
		static short TariffTypeId => Helper.GetDataSetId(DataSet.RefCusTariffType);

		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDD4", new[] { "AAA", "BBB" })]
		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDD8", new[] { "BBB" })]
		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var tariffType = repo.Create(() => new RefCusTariffType { ZZI_PK = Guid.NewGuid() });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = TariffTypeId, RVC_ParentPK = tariffType.ZZI_PK, RVC_ParentCode = "ZZI", RVC_IsPublished = true });
			var dataset1 = repo.Create(() => new RefCusProfileType { XXX_PK = new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD6"), XXX_ProfileType = "AAA", XXX_ZZI_TariffType = tariffType.ZZI_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.XXX_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusProfileType { XXX_PK = new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD9"), XXX_ProfileType = "BBB", XXX_ZZI_TariffType = tariffType.ZZI_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.XXX_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK))).Select(x => x.XXX_ProfileType).ToArray();
			Assert.That(dataSets, Is.EqualTo(expected));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetDataCoreLinqCanBeTranslated_RefCusProfileTypeService()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repository = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repository.Add(new RefDataGrouping
				{
					ZZZ_PK = new Guid("F5C7C4EE-36FF-4952-BDF0-FF70010F89D2"),
					ZZZ_DataGrouping = "AU",
					ZZZ_Description = "AU DataGrouping",
					ZZZ_ZZZ_NKGrouping = "AU"
				});
				repository.Add(new RefCusNomenclatureGroupType
				{
					ZZ9_PK = new Guid("5B72886F-14B5-4AE9-BFF8-AB4282B71511"),
					ZZ9_Description = "cds test type",
					ZZ9_GroupType = "CDS"
				});
				await repository.SaveChangesAsync();

				var refCusTariffType = new RefCusTariffType
				{
					ZZI_PK = Guid.NewGuid(),
					ZZI_Description = "test tariff type",
					ZZI_TariffType = "CC",
					ZZI_ZZZ_NKDataGrouping = "AU",
					ZZI_ZZ9_NKNomenclatureGroupType = "CDS"
				};
				repository.Add(refCusTariffType);
				await repository.SaveChangesAsync();

				repository.Add(new RefCusProfileType
				{
					XXX_PK = new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD6"),
					XXX_ProfileType = "AAA",
					XXX_Description = "test refCusProfileType",
					XXX_ZZI_TariffType = refCusTariffType.ZZI_PK,
					XXX_ZZZ_NKDataGrouping = "AU"
				});
				await repository.SaveChangesAsync();

				var service = new RefCusProfileTypeService(repository);
				var refCusProfileTypeEnumerable = service.GetData(null, Now, CheckpointHelper.Create(new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD4")), null, DataSetId).OrderBy(x => x.XXX_ProfileType);
				Assert.DoesNotThrow(() =>
				{
					var refCusProfileTypeResults = refCusProfileTypeEnumerable.ToArray();
					Assert.NotNull(refCusProfileTypeResults);
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
			var profileType1 = CreateData(Now, "AAA");
			var profileType2 = CreateData(Now.AddDays(2), "BBB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.XXX_ProfileType).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Data()
		{
			var profileType = CreateData(Now.AddDays(1), "ZZZ");
			profileType.XXX_Description = "Test";
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.XXX_ProfileType, Is.EqualTo("ZZZ"));
			Assert.That(result.XXX_Description, Is.EqualTo("Test"));
		}

		[Test]
		public void GetLatest_Data_Deleted()
		{
			var tariffType = repo.Create(() => new RefCusTariffType { ZZI_PK = Guid.NewGuid(), ZZI_TariffType = "T1" });
			var deletedTariffType = repo.Create(() => new RefCusTariffType { ZZI_PK = Guid.NewGuid(), ZZI_TariffType = "T2" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = TariffTypeId, RVC_ParentPK = tariffType.ZZI_PK, RVC_ParentCode = "ZZI", RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = TariffTypeId, RVC_ParentPK = deletedTariffType.ZZI_PK, RVC_ParentCode = "ZZI", RVC_IsPublished = true, RVC_Deleted = true });

			var profileType = repo.Create(() => new RefCusProfileType { XXX_PK = Guid.NewGuid(), XXX_ProfileType = "A1", XXX_ZZI_TariffType = tariffType.ZZI_PK });
			var profileType_DeletedTariffType = repo.Create(() => new RefCusProfileType { XXX_PK = Guid.NewGuid(), XXX_ProfileType = "A2", XXX_ZZI_TariffType = deletedTariffType.ZZI_PK });
			var deletedProfileType = repo.Create(() => new RefCusProfileType { XXX_PK = Guid.NewGuid(), XXX_ProfileType = "A3", XXX_ZZI_TariffType = tariffType.ZZI_PK });
			var deletedProfileType_DeletedTariffType = repo.Create(() => new RefCusProfileType { XXX_PK = Guid.NewGuid(), XXX_ProfileType = "A4", XXX_ZZI_TariffType = deletedTariffType.ZZI_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = profileType.XXX_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = profileType_DeletedTariffType.XXX_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = deletedProfileType.XXX_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true, RVC_Deleted = true });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = deletedProfileType_DeletedTariffType.XXX_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true, RVC_Deleted = true });

			var dataSets = GetDataSets(Now.AddDays(-1)).ToList();
			Assert.AreEqual(2, dataSets.Count);
			var result = dataSets.First(x => x.XXX_ProfileType == "A1");
			Assert.False(result.Deleted);
			Assert.NotNull(result.RefCusTariffType);
			Assert.NotNull("T1", result.RefCusTariffType.ZZI_TariffType);
			result = dataSets.First(x => x.XXX_ProfileType == "A3");
			Assert.True(result.Deleted);
			Assert.NotNull(result.RefCusTariffType);
			Assert.NotNull("T2", result.RefCusTariffType.ZZI_TariffType);
		}

		RefCusProfileType CreateData(DateTime dateTime, string profileType)
		{
			var tariffType = repo.Create(() => new RefCusTariffType { ZZI_PK = Guid.NewGuid() });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = TariffTypeId, RVC_ParentPK = tariffType.ZZI_PK, RVC_ParentCode = "ZZI", RVC_IsPublished = true });
			var result = repo.Create(() => new RefCusProfileType { XXX_PK = Guid.NewGuid(), XXX_ProfileType = profileType, XXX_ZZI_TariffType = tariffType.ZZI_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.XXX_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusProfileType> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusProfileType> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusProfileTypeService(repo);
			return service.GetData(dateTime, runtime ?? DateTime.UtcNow.AddDays(10), checkpoint, null, DataSetId).OrderBy(x => x.XXX_ProfileType);
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
