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
	class RefCusProfileQuestionPathwayServiceFixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);
		static string TblPrefix => "XQP";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusProfileQuestionPathway);
		static short TariffTypeId => Helper.GetDataSetId(DataSet.RefCusTariffType);
		static short ProfileTypeId => Helper.GetDataSetId(DataSet.RefCusProfileType);
		static short ProfileQuestionId => Helper.GetDataSetId(DataSet.RefCusProfileQuestion);

		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDD4", new[] { "AAA", "BBB" })]
		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDD8", new[] { "BBB" })]
		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var profileQuestion1 = CreateProfileQuestion("Code1");
			var profileQuestion2 = CreateProfileQuestion("Code2");
			var dataset1 = repo.Create(() => new RefCusProfileQuestionPathway { XQP_PK = new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD6"), XQP_Description = "AAA", XQP_XQ2_QuestionParent = profileQuestion1.XQ2_PK, XQP_XQ2_QuestionChild = profileQuestion2.XQ2_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.XQP_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusProfileQuestionPathway { XQP_PK = new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD9"), XQP_Description = "BBB", XQP_XQ2_QuestionParent = profileQuestion2.XQ2_PK, XQP_XQ2_QuestionChild = profileQuestion2.XQ2_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.XQP_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK))).Select(x => x.XQP_Description).ToArray();
			Assert.AreEqual(expected, dataSets);
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetDataCoreLinqCanBeTranslated_RefCusProfileQuestionPathwayService()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repository = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repository.Add(new RefDataGrouping
				{
					ZZZ_PK = new Guid("9DE30DFC-6398-4B5D-A263-362B2A1F183A"),
					ZZZ_DataGrouping = "AU",
					ZZZ_Description = "AU DataGrouping",
					ZZZ_ZZZ_NKGrouping = "AU"
				});
				repository.Add(new RefCusNomenclatureGroupType
				{
					ZZ9_PK = new Guid("2820C915-AA9E-4560-A7D7-E93A501E291B"),
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

				var parentQuestion = new RefCusProfileQuestion
				{
					XQ2_PK = Guid.NewGuid(),
					XQ2_QuestionCode = "AAA",
					XQ2_XXX_ProfileType = refCusProfileType.XXX_PK,
					XQ2_Name = "test question parent",
					XQ2_Text = "test question text",
					XQ2_Note = "test question note",
					XQ2_ZZZ_NKDataGrouping = "AU",
					XQ2_AnswerDataType = "STRING",
					XQ2_AnswerMask = "*",
					XQ2_StartDate = Now,
					XQ2_EndDate = Now.AddDays(1)
				};
				var childQuestion = new RefCusProfileQuestion
				{
					XQ2_PK = Guid.NewGuid(),
					XQ2_QuestionCode = "BBB",
					XQ2_XXX_ProfileType = refCusProfileType.XXX_PK,
					XQ2_Name = "test question child",
					XQ2_Text = "test question text",
					XQ2_Note = "test question note",
					XQ2_ZZZ_NKDataGrouping = "AU",
					XQ2_AnswerDataType = "STRING",
					XQ2_AnswerMask = "*",
					XQ2_StartDate = Now,
					XQ2_EndDate = Now.AddDays(1)
				};

				repository.Add(parentQuestion);
				repository.Add(childQuestion);
				await repository.SaveChangesAsync();

				repository.Add(new RefCusProfileQuestionPathway
				{
					XQP_PK = new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD6"),
					XQP_XQ2_QuestionParent = parentQuestion.XQ2_PK,
					XQP_XQ2_QuestionChild = childQuestion.XQ2_PK,
					XQP_Description = "AAA",
					XQP_ConditionToProceedFormula = "1=1",
					XQP_StartDate = Now,
					XQP_EndDate = Now.AddDays(1)
				});
				await repository.SaveChangesAsync();

				var service = new RefCusProfileQuestionPathwayService(repository);
				var refCusProfileQuestionPathwayEnumerable = service.GetData(null, Now, CheckpointHelper.Create(new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD4")), null, DataSetId).OrderBy(x => x.XQP_Description);
				Assert.DoesNotThrow(() =>
				{
					var refCusProfileQuestionPathwayResults = refCusProfileQuestionPathwayEnumerable.ToArray();
					Assert.NotNull(refCusProfileQuestionPathwayResults);
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
			Assert.AreEqual(expected, dataSets.Select(x => x.XQP_Description).ToArray());
		}

		[Test]
		public void GetLatest_ProfileQuestionPathway_Data()
		{
			var endDate = new DateTime(2024, 12, 31);
			var data = CreateData(Now.AddDays(1), "AAA");
			data.XQP_EndDate = endDate;
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.AreEqual("AAA", result.XQP_Description);
			Assert.AreEqual(endDate, result.XQP_EndDate);
		}

		RefCusProfileQuestion CreateProfileQuestion(string typeOrCode)
		{
			var refCusTariffType = repo.Create(() => new RefCusTariffType { ZZI_PK = Guid.NewGuid(), ZZI_TariffType = typeOrCode });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = TariffTypeId, RVC_ParentPK = refCusTariffType.ZZI_PK, RVC_ParentCode = "ZZI", RVC_IsPublished = true });
			var refCusProfileType = repo.Create(() => new RefCusProfileType { XXX_PK = Guid.NewGuid(), XXX_ProfileType = typeOrCode, XXX_ZZI_TariffType = refCusTariffType.ZZI_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = ProfileTypeId, RVC_ParentPK = refCusProfileType.XXX_PK, RVC_ParentCode = "XXX", RVC_IsPublished = true });
			var refCusProfileQuestion = repo.Create(() => new RefCusProfileQuestion { XQ2_PK = Guid.NewGuid(), XQ2_QuestionCode = typeOrCode, XQ2_XXX_ProfileType = refCusProfileType.XXX_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = ProfileQuestionId, RVC_ParentPK = refCusProfileQuestion.XQ2_PK, RVC_ParentCode = "XQ2", RVC_IsPublished = true });
			return refCusProfileQuestion;
		}

		RefCusProfileQuestionPathway CreateData(DateTime dateTime, string code)
		{
			var profileQuestion1 = CreateProfileQuestion(code + "1");
			var profileQuestion2 = CreateProfileQuestion(code + "2");
			var result = repo.Create(() => new RefCusProfileQuestionPathway { XQP_PK = Guid.NewGuid(), XQP_XQ2_QuestionParent = profileQuestion1.XQ2_PK, XQP_XQ2_QuestionChild = profileQuestion2.XQ2_PK, XQP_Description = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.XQP_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusProfileQuestionPathway> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusProfileQuestionPathway> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusProfileQuestionPathwayService(repo);
			return service.GetData(dateTime, runtime ?? DateTime.UtcNow.AddDays(10), checkpoint, null, DataSetId).OrderBy(x => x.XQP_Description);
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
