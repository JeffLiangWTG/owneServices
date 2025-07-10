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
	class RefCusProfileQuestionServiceFixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);
		static string TblPrefix => "XQ2";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusProfileQuestion);
		static short TariffTypeId => Helper.GetDataSetId(DataSet.RefCusTariffType);
		static short ProfileTypeId => Helper.GetDataSetId(DataSet.RefCusProfileType);

		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDD4", new[] { "AAA", "BBB" })]
		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDD8", new[] { "BBB" })]
		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var profileType = CreateProfileType("T1", "P1");
			var dataset1 = repo.Create(() => new RefCusProfileQuestion { XQ2_PK = new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD6"), XQ2_QuestionCode = "AAA", XQ2_XXX_ProfileType = profileType.XXX_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.XQ2_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusProfileQuestion { XQ2_PK = new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD9"), XQ2_QuestionCode = "BBB", XQ2_XXX_ProfileType = profileType.XXX_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.XQ2_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK))).Select(x => x.XQ2_Code).ToArray();
			Assert.AreEqual(expected, dataSets);
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetDataCoreLinqCanBeTranslated_RefCusProfileQuestionService()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repository = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repository.Add(new RefDataGrouping
				{
					ZZZ_PK = new Guid("434B59C1-B3F4-479E-9785-66AF43DDDB91"),
					ZZZ_DataGrouping = "AU",
					ZZZ_Description = "AU DataGrouping",
					ZZZ_ZZZ_NKGrouping = "AU"
				});
				repository.Add(new RefCusNomenclatureGroupType
				{
					ZZ9_PK = new Guid("5EF01900-4C9E-418E-95D3-10AB74A716E9"),
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

				repository.Add(new RefCusProfileQuestion
				{
					XQ2_PK = new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD6"),
					XQ2_QuestionCode = "AAA",
					XQ2_XXX_ProfileType = refCusProfileType.XXX_PK,
					XQ2_Name = "test question",
					XQ2_Text = "test question text",
					XQ2_Note = "test question note",
					XQ2_ZZZ_NKDataGrouping = "AU",
					XQ2_AnswerDataType = "STRING",
					XQ2_AnswerMask = "*",
					XQ2_StartDate = Now,
					XQ2_EndDate = Now.AddDays(1)
				});
				await repository.SaveChangesAsync();

				var service = new RefCusProfileQuestionService(repository);
				var refCusProfileQuestionEnumerable = service.GetData(null, Now, CheckpointHelper.Create(new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD4")), null, DataSetId).OrderBy(x => x.XQ2_Code);
				Assert.DoesNotThrow(() =>
				{
					var refCusProfileQuestionResults = refCusProfileQuestionEnumerable.ToArray();
					Assert.NotNull(refCusProfileQuestionResults);
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
			Assert.AreEqual(expected, dataSets.Select(x => x.XQ2_Code).ToArray());
		}

		[Test]
		public void GetLatest_ProfileQuestion_Data()
		{
			var data = CreateData(Now.AddDays(1), "AAA");
			data.XQ2_AnswerDataType = "STRING";
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.AreEqual("AAA", result.XQ2_Code);
			Assert.AreEqual("STRING", result.XQ2_AnswerDataType);
		}

		[Test]
		public void GetLatest_ProfileQuestion_AnswerList_Data()
		{
			var profileQuestion = CreateData(Now, "CCC");
			var answerList1 = repo.Create(() => new RefCusProfileQuestionAnswerList
			{
				XQ4_PK = Guid.NewGuid(),
				XQ4_XQ2_Question = profileQuestion.XQ2_PK,
				XQ4_Value = "Value 1",
				XQ4_Description = "Desc 1"
			});
			var answerList2 = repo.Create(() => new RefCusProfileQuestionAnswerList
			{
				XQ4_PK = Guid.NewGuid(),
				XQ4_XQ2_Question = profileQuestion.XQ2_PK,
				XQ4_Value = "Value 2",
				XQ4_Description = "Desc 2"
			});

			var answerListLanguage1 = repo.Create(() => new RefCusProfileQuestionAnswerListLanguage
			{
				XAL_PK = Guid.NewGuid(),
				XAL_XQ4_QuestionAnswer = answerList1.XQ4_PK,
				XAL_ZX6_NKLanguage = "EN",
				XAL_DataSetCode = TblPrefix,
				XAL_DataSetPK = answerList1.XQ4_XQ2_Question
			});
			var answerListLanguage2 = repo.Create(() => new RefCusProfileQuestionAnswerListLanguage
			{
				XAL_PK = Guid.NewGuid(),
				XAL_XQ4_QuestionAnswer = answerList1.XQ4_PK,
				XAL_ZX6_NKLanguage = "DE",
				XAL_DataSetCode = TblPrefix,
				XAL_DataSetPK = answerList1.XQ4_XQ2_Question
			});

			var dataSets = GetDataSets(Now.AddHours(-1)).ToList();
			Assert.AreEqual(1, dataSets.Count);
			var result = dataSets.First(x => !x.Deleted);
			Assert.AreEqual("CCC", result.XQ2_Code);
			Assert.AreEqual(2, result.RefCusProfileQuestionAnswerLists.Length);
			CollectionAssert.AreEquivalent(new[] { "Value 1", "Value 2" }, result.RefCusProfileQuestionAnswerLists.Select(x => x.XQ4_Value));
			var answerList = result.RefCusProfileQuestionAnswerLists.First(x => x.XQ4_Value == "Value 1");
			Assert.AreEqual(2, answerList.RefCusProfileQuestionAnswerListLanguages.Length);
			CollectionAssert.AreEquivalent(new[] { "EN", "DE" }, answerList.RefCusProfileQuestionAnswerListLanguages.Select(x => x.XAL_ZX6_NKLanguage));
		}

		[Test]
		public void GetLatest_ProfileQuestion_Attribute_Data()
		{
			var profileQuestion = CreateData(Now, "CCC");
			var attribute1 = repo.Create(() => new RefCusProfileQuestionAttribute
			{
				XQ3_PK = Guid.NewGuid(),
				XQ3_XQ2_Question = profileQuestion.XQ2_PK,
				XQ3_Name = "Name 1",
				XQ3_Value = "Value 1"
			});
			var attribute2 = repo.Create(() => new RefCusProfileQuestionAttribute
			{
				XQ3_PK = Guid.NewGuid(),
				XQ3_XQ2_Question = profileQuestion.XQ2_PK,
				XQ3_Name = "Name 2",
				XQ3_Value = "Value 2"
			});

			var dataSets = GetDataSets(Now.AddHours(-1)).ToList();
			Assert.AreEqual(1, dataSets.Count);
			var result = dataSets.First(x => !x.Deleted);
			Assert.AreEqual("CCC", result.XQ2_Code);
			Assert.AreEqual(2, result.RefCusProfileQuestionAttributes.Length);
			CollectionAssert.AreEquivalent(new[] { "Name 1", "Name 2" }, result.RefCusProfileQuestionAttributes.Select(x => x.XQ3_Name));
		}

		[Test]
		public void GetLatest_ProfileQuestion_Language_Data()
		{
			var profileQuestion = CreateData(Now, "CCC");
			var language1 = repo.Create(() => new RefCusProfileQuestionLanguage
			{
				XQL_PK = Guid.NewGuid(),
				XQL_XQ2_Question = profileQuestion.XQ2_PK,
				XQL_Name = "Name 1",
				XQL_ZX6_NKLanguage = "AA"
			});
			var language2 = repo.Create(() => new RefCusProfileQuestionLanguage
			{
				XQL_PK = Guid.NewGuid(),
				XQL_XQ2_Question = profileQuestion.XQ2_PK,
				XQL_Name = "Name 2",
				XQL_ZX6_NKLanguage = "BB"
			});

			var dataSets = GetDataSets(Now.AddHours(-1)).ToList();
			Assert.AreEqual(1, dataSets.Count);
			var result = dataSets.First(x => !x.Deleted);
			Assert.AreEqual("CCC", result.XQ2_Code);
			Assert.AreEqual(2, result.RefCusProfileQuestionLanguages.Length);
			CollectionAssert.AreEquivalent(new[] { "Name 1", "Name 2" }, result.RefCusProfileQuestionLanguages.Select(x => x.XQL_Name));
		}

		RefCusProfileType CreateProfileType(string tariffType, string profileType)
		{
			var refCusTariffType = repo.Create(() => new RefCusTariffType { ZZI_PK = Guid.NewGuid(), ZZI_TariffType = tariffType });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = TariffTypeId, RVC_ParentPK = refCusTariffType.ZZI_PK, RVC_ParentCode = "ZZI", RVC_IsPublished = true });
			var refCusProfileType = repo.Create(() => new RefCusProfileType { XXX_PK = Guid.NewGuid(), XXX_ProfileType = profileType, XXX_ZZI_TariffType = refCusTariffType.ZZI_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = ProfileTypeId, RVC_ParentPK = refCusProfileType.XXX_PK, RVC_ParentCode = "XXX", RVC_IsPublished = true });
			return refCusProfileType;
		}

		RefCusProfileQuestion CreateData(DateTime dateTime, string code)
		{
			var profileType = CreateProfileType(code, code);
			var result = repo.Create(() => new RefCusProfileQuestion { XQ2_PK = Guid.NewGuid(), XQ2_QuestionCode = code, XQ2_XXX_ProfileType = profileType.XXX_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.XQ2_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusProfileQuestion> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusProfileQuestion> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusProfileQuestionService(repo);
			return service.GetData(dateTime, runtime ?? DateTime.UtcNow.AddDays(10), checkpoint, null, DataSetId).OrderBy(x => x.XQ2_Code);
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
