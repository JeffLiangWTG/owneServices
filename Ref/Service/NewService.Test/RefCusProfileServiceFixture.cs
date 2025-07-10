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
	class RefCusProfileServiceFixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);
		static string TblPrefix => "XX0";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusProfile);
		static short TariffTypeId => Helper.GetDataSetId(DataSet.RefCusTariffType);
		static short ProfileTypeId => Helper.GetDataSetId(DataSet.RefCusProfileType);

		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDD4", new[] { "AAA", "BBB" })]
		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDD8", new[] { "BBB" })]
		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var profileType = CreateProfileType("T1", "P1");
			var dataset1 = repo.Create(() => new RefCusProfile { XX0_PK = new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD6"), XX0_QuestionCode = "AAA", XX0_XXX_ProfileType = profileType.XXX_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.XX0_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusProfile { XX0_PK = new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD9"), XX0_QuestionCode = "BBB", XX0_XXX_ProfileType = profileType.XXX_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.XX0_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK))).Select(x => x.XX0_QuestionCode).ToArray();
			Assert.AreEqual(expected, dataSets);
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetDataCoreLinqCanBeTranslated_RefCusProfileService()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repository = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repository.Add(new RefDataGrouping
				{
					ZZZ_PK = new Guid("0C518318-C656-411A-92DC-AFC7413A6E25"),
					ZZZ_DataGrouping = "AU",
					ZZZ_Description = "AU DataGrouping",
					ZZZ_ZZZ_NKGrouping = "AU"
				});
				repository.Add(new RefCusNomenclatureGroupType
				{
					ZZ9_PK = new Guid("CB7A45B5-0467-4D69-A237-2AB8814DBBA9"),
					ZZ9_Description = "test nomenclature group type",
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
			
				var refCusProfileType = new RefCusProfileType
				{
					XXX_PK = Guid.NewGuid(),
					XXX_ProfileType = "AAA",
					XXX_Description = "test profile type",
					XXX_ZZI_TariffType = refCusTariffType.ZZI_PK,
					XXX_ZZZ_NKDataGrouping = "AU"
				};
				repository.Add(refCusProfileType);
				await repository.SaveChangesAsync();

				repository.Add(new RefCusProfile
				{
					XX0_PK = new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD6"),
					XX0_QuestionCode = "AAA",
					XX0_XXX_ProfileType = refCusProfileType.XXX_PK,
					XX0_TariffCode = "10",
					XX0_ZZZ_NKDataGrouping = "AU",
					XX0_StartDate = Now,
					XX0_EndDate = Now.AddDays(1)
				});
				await repository.SaveChangesAsync();

				var service = new RefCusProfileService(repository);
				var refCusProfileEnumerable = service.GetData(null, Now, CheckpointHelper.Create(new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD4")), null, DataSetId).OrderBy(x => x.XX0_QuestionCode);
				Assert.DoesNotThrow(() =>
				{
					var refCusProfileResults = refCusProfileEnumerable.ToArray();
					Assert.NotNull(refCusProfileResults);
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
			var data1 = CreateData(Now, "AAA");
			var data2 = CreateData(Now.AddDays(2), "BBB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.AreEqual(expected, dataSets.Select(x => x.XX0_QuestionCode).ToArray());
		}

		[Test]
		public void GetLatest_Profile_Data()
		{
			var data = CreateData(Now.AddDays(1), "AAA");
			data.XX0_TariffCode = "1001";
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.AreEqual("AAA", result.XX0_QuestionCode);
			Assert.AreEqual("1001", result.XX0_TariffCode);
		}

		[Test]
		public void GetLatest_Profile_ProfileAttribute_Data()
		{
			var profile = CreateData(Now, "CCC");
			var profile_Deleted = CreateData(Now, "DDD", true);
			var profileAttribute1 = repo.Create(() => new RefCusProfileAttribute
			{
				XXY_PK = Guid.NewGuid(),
				XXY_XX0_Profile = profile.XX0_PK,
				XXY_Name = "Name1",
				XXY_Value = "Value1"
			});
			var profileAttribute2 = repo.Create(() => new RefCusProfileAttribute
			{
				XXY_PK = Guid.NewGuid(),
				XXY_XX0_Profile = profile.XX0_PK,
				XXY_Name = "Name2",
				XXY_Value = "Value2"
			});
			var profileAttribute3 = repo.Create(() => new RefCusProfileAttribute
			{
				XXY_PK = Guid.NewGuid(),
				XXY_XX0_Profile = profile_Deleted.XX0_PK,
				XXY_Name = "Name3",
				XXY_Value = "Value3"
			});

			var dataSets = GetDataSets(Now.AddHours(-1)).ToList();
			Assert.AreEqual(2, dataSets.Count);
			var result = dataSets.First(x => !x.Deleted);
			Assert.AreEqual("CCC", result.XX0_QuestionCode);
			Assert.AreEqual(2, result.RefCusProfileAttributes.Length);
			CollectionAssert.AreEquivalent(new[] { "Name1", "Name2" }, result.RefCusProfileAttributes.Select(x => x.XXY_Name));
			result = dataSets.First(x => x.Deleted);
			Assert.AreEqual("DDD", result.XX0_QuestionCode);
			Assert.AreEqual(0, result.RefCusProfileAttributes.Length);
		}

		[TestCase(false)]
		[TestCase(true)]
		public void GetLatest_Data_Deleted(bool deleted)
		{
			var profileType1 = CreateProfileType("AA1", "BB1", true, true);
			var profileType2 = CreateProfileType("AA2", "BB2", true, false);
			var profileType3 = CreateProfileType("AA3", "BB3", false, true);
			var profileType4 = CreateProfileType("AA4", "BB4", false, false);

			var profile1 = repo.Create(() => new RefCusProfile { XX0_PK = Guid.NewGuid(), XX0_QuestionCode = "AAA", XX0_XXX_ProfileType = profileType1.XXX_PK });
			var profile2 = repo.Create(() => new RefCusProfile { XX0_PK = Guid.NewGuid(), XX0_QuestionCode = "BBB", XX0_XXX_ProfileType = profileType2.XXX_PK });
			var profile3 = repo.Create(() => new RefCusProfile { XX0_PK = Guid.NewGuid(), XX0_QuestionCode = "CCC", XX0_XXX_ProfileType = profileType3.XXX_PK });
			var profile4 = repo.Create(() => new RefCusProfile { XX0_PK = Guid.NewGuid(), XX0_QuestionCode = "DDD", XX0_XXX_ProfileType = profileType4.XXX_PK });
			var versionControl1 = new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = profile1.XX0_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true, RVC_Deleted = deleted };
			var versionControl2 = new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = profile2.XX0_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true, RVC_Deleted = deleted };
			var versionControl3 = new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = profile3.XX0_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true, RVC_Deleted = deleted };
			var versionControl4 = new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = profile4.XX0_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true, RVC_Deleted = deleted };
			var versionControls = new[] { versionControl1, versionControl2, versionControl3, versionControl4 };
			Array.ForEach(versionControls, x => repo.Create(() => x));

			var dataSets = GetDataSets(Now.AddDays(-1)).ToList();
			Assert.AreEqual(1, dataSets.Count);
			var result = dataSets.FirstOrDefault(x => x.XX0_QuestionCode == "AAA");
			Assert.Null(result);
			result = dataSets.FirstOrDefault(x => x.XX0_QuestionCode == "BBB");
			Assert.Null(result);
			result = dataSets.FirstOrDefault(x => x.XX0_QuestionCode == "CCC");
			Assert.Null(result);
			result = dataSets.First(x => x.XX0_QuestionCode == "DDD");
			Assert.AreEqual(deleted, result.Deleted);
			Assert.NotNull(result.RefCusProfileType);
			Assert.NotNull(result.RefCusProfileType.RefCusTariffType);
		}

		RefCusProfileType CreateProfileType(string tariffType, string profileType, bool deletedTariffType = false, bool deletedProfileType = false)
		{
			var refCusTariffType = repo.Create(() => new RefCusTariffType { ZZI_PK = Guid.NewGuid(), ZZI_TariffType = tariffType });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = TariffTypeId, RVC_ParentPK = refCusTariffType.ZZI_PK, RVC_ParentCode = "ZZI", RVC_IsPublished = true, RVC_Deleted = deletedTariffType });
			var refCusProfileType = repo.Create(() => new RefCusProfileType { XXX_PK = Guid.NewGuid(), XXX_ProfileType = profileType, XXX_ZZI_TariffType = refCusTariffType.ZZI_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = ProfileTypeId, RVC_ParentPK = refCusProfileType.XXX_PK, RVC_ParentCode = "XXX", RVC_IsPublished = true, RVC_Deleted = deletedProfileType });
			return refCusProfileType;
		}

		RefCusProfile CreateData(DateTime dateTime, string questionCode, bool deleted = false)
		{
			var profileType = CreateProfileType(questionCode, questionCode);
			var result = repo.Create(() => new RefCusProfile { XX0_PK = Guid.NewGuid(), XX0_QuestionCode = questionCode, XX0_XXX_ProfileType = profileType.XXX_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.XX0_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true, RVC_Deleted = deleted });
			return result;
		}

		IEnumerable<Models.RefCusProfile> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusProfile> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusProfileService(repo);
			return service.GetData(dateTime, runtime ?? DateTime.UtcNow.AddDays(10), checkpoint, null, DataSetId).OrderBy(x => x.XX0_QuestionCode);
		}

		DateTime Now = DateTime.UtcNow;
		ObjectReferenceDataRepository repo;

		[SetUp]
		public void SetUp()
		{
			repo = new ObjectReferenceDataRepository();
		}
	}
}
