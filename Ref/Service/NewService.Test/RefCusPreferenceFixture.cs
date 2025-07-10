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
	public class RefCusPreferenceServiceFixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);
		static string TblPrefix => "ZZS";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusPreference);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusPreference { ZZS_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZS_Description = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZS_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusPreference { ZZS_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZS_Description = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZS_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZS_Description).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetDataCoreLinqCanBeTranslated_RefCusPreferenceService()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repository = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repository.Add(new RefDataGrouping
				{
					ZZZ_PK = new Guid("74900788-5100-468A-939C-0078A41220EE"),
					ZZZ_DataGrouping = "AU",
					ZZZ_Description = "AU DataGrouping",
					ZZZ_ZZZ_NKGrouping = "AU"
				});
				await repository.SaveChangesAsync();

				repository.Add(new RefCusPreference
				{
					ZZS_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"),
					ZZS_Description = "BB",
					ZZS_Preference = "URD",
					ZZS_ZZZ_NKDataGrouping = "AU"
				});
				await repository.SaveChangesAsync();

				var service = new RefCusPreferenceService(repository, new RefCusConditionService(repository), new RefCusApplicabilityService(repository));
				var refCusPreferenceEnumerable = service.GetData(null, Now, CheckpointHelper.Create(new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5")), null, DataSetId).OrderBy(x => x.ZZS_Description);
				Assert.DoesNotThrow(() =>
				{
					var refCusPreferenceResults = refCusPreferenceEnumerable.ToArray();
					Assert.NotNull(refCusPreferenceResults);
				});
			}
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Preference_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreatePreference(Now, "AA");
			var p2 = CreatePreference(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZS_Description).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Preference_Data()
		{
			var p = CreatePreference(Now.AddDays(1), "AA");
			p.ZZS_Description = "BB";
			p.ZZS_ZZZ_NKDataGrouping = "AU";
			p.ZZS_Preference = "T";

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZS_Description, Is.EqualTo("BB"));
			Assert.That(result.ZZS_ZZZ_NKDataGrouping, Is.EqualTo("AU"));
			Assert.That(result.ZZS_Preference, Is.EqualTo("T"));
		}

		[Test]
		public void GetLatest_PreferenceLanguage_Data()
		{
			var preference = CreatePreference(Now.AddDays(1));
			var language = repo.Create(() => new RefCusPreferenceLanguage
			{
				ZX9_PK = Guid.NewGuid(),
				ZX9_ZZS_Preference = preference.ZZS_PK,
				ZX9_ZX6_NKLanguage = "EN",
				ZX9_Description = "English"
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusPreferenceLanguages.ElementAt(0);
			Assert.That(1, Is.EqualTo(dataSets.ElementAt(0).RefCusPreferenceLanguages.Length));
			Assert.That(result.ZX9_ZX6_NKLanguage, Is.EqualTo(language.ZX9_ZX6_NKLanguage));
			Assert.That(result.ZX9_Description, Is.EqualTo(language.ZX9_Description));
		}

		[Test]
		public void Get_RefCusConditionValue_Data()
		{
			var preference = CreatePreference(Now.AddDays(1));
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = preference.ZZS_PK,
				ZX1_DataSetCode = "ZZS",
				ZX1_ZZS_Preference = preference.ZZS_PK,
				ZX1_ZX2_ConditionType = conditionType.ZX2_PK,
			});
			var value = repo.Create(() => new RefCusConditionValue
			{
				ZX3_DataSetPK = preference.ZZS_PK,
				ZX3_DataSetCode = "ZZS",
				ZX3_ZX4_ValueType = valueType.ZX4_PK,
				ZX3_LogicalORWithinGroup = 0,
				ZX3_Value = "A",
				ZX3_ZX1_Condition = condition.ZX1_PK
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusConditions.ElementAt(0).RefCusConditionValues.ElementAt(0);
			Assert.That(result.ZX3_LogicalORWithinGroup, Is.EqualTo(value.ZX3_LogicalORWithinGroup));
			Assert.That(result.ZX3_Value, Is.EqualTo(value.ZX3_Value));
			Assert.That(result.RefCusConditionValueType.ZX4_Description, Is.EqualTo(valueType.ZX4_Description));
		}

		[Test]
		public void GetRefCusConditionLanguageData()
		{
			var preference = CreatePreference(Now.AddDays(1));
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = preference.ZZS_PK,
				ZX1_DataSetCode = "ZZS",
				ZX1_ZZS_Preference = preference.ZZS_PK,
				ZX1_ZX2_ConditionType = conditionType.ZX2_PK,
			});
			var language = repo.Create(() => new RefCusConditionLanguage
			{
				ZXJ_DataSetPK = preference.ZZS_PK,
				ZXJ_DataSetCode = "ZZS",
				ZXJ_ZX6_NKLanguage = "EN",
				ZXJ_ZX1_Condition = condition.ZX1_PK,
				ZXJ_Comment = "A",
				ZXJ_Source = "XXX"
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusConditions.ElementAt(0).RefCusConditionLanguages.ElementAt(0);
			Assert.That(result.ZXJ_Comment, Is.EqualTo(language.ZXJ_Comment));
			Assert.That(result.ZXJ_Source, Is.EqualTo(language.ZXJ_Source));
			Assert.That(result.ZXJ_ZX6_NKLanguage, Is.EqualTo(language.ZXJ_ZX6_NKLanguage));
		}

		[Test]
		public void Get_RefCusCondition_Data()
		{
			var preference = CreatePreference(Now.AddDays(1));
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = preference.ZZS_PK,
				ZX1_DataSetCode = "ZZS",
				ZX1_ZZS_Preference = preference.ZZS_PK,
				ZX1_ZX2_ConditionType = conditionType.ZX2_PK,
				ZX1_ConditionValueTrueMeansStop = true,
				ZX1_Comment = "A",
				ZX1_EndDate = Now.AddDays(1),
				ZX1_StartDate = Now,
				ZX1_IsExport = true,
				ZX1_IsImport = true,
				ZX1_Source = "B",
				ZX1_ZZZ_NKDataGrouping = "ZA",
				ZX1_LogicalANDWithinGroup = 1
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusConditions.ElementAt(0);
			Assert.That(result.ZX1_Comment, Is.EqualTo(condition.ZX1_Comment));
			Assert.That(result.ZX1_EndDate, Is.EqualTo(condition.ZX1_EndDate));
			Assert.That(result.ZX1_StartDate, Is.EqualTo(condition.ZX1_StartDate));
			Assert.That(result.ZX1_IsExport, Is.EqualTo(condition.ZX1_IsExport));
			Assert.That(result.ZX1_IsImport, Is.EqualTo(condition.ZX1_IsImport));
			Assert.That(result.ZX1_Source, Is.EqualTo(condition.ZX1_Source));
			Assert.That(result.ZX1_ZZZ_NKDataGrouping, Is.EqualTo(condition.ZX1_ZZZ_NKDataGrouping));
			Assert.That(result.ZX1_LogicalANDWithinGroup, Is.EqualTo(condition.ZX1_LogicalANDWithinGroup));
			Assert.That(result.RefCusConditionType.ZX2_ConditionType, Is.EqualTo(conditionType.ZX2_ConditionType));
		}

		[Test]
		public void Get_TariffApplicability_Data()
		{
			var preference = CreatePreference(Now.AddDays(1));
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = preference.ZZS_PK,
				ZX1_DataSetCode = "ZZS",
				ZX1_ZZ1_Tariff = preference.ZZS_PK,
				ZX1_ZX2_ConditionType = conditionType.ZX2_PK
			});
			var applicability = repo.Create(() => new RefCusApplicability
			{
				ZZT_ZX1_Conditions = condition.ZX1_PK,
				ZZT_DataSetPK = preference.ZZS_PK,
				ZZT_DataSetCode = "ZZS",
				ZZT_AdditionalCode = "A",
				ZZT_StartDate = Now,
				ZZT_EndDate = Now.AddDays(1),
				ZZT_OrderNumber = "1",
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusConditions.ElementAt(0).RefCusApplicabilities.ElementAt(0);
			Assert.That(result.ZZT_AdditionalCode, Is.EqualTo(applicability.ZZT_AdditionalCode));
			Assert.That(result.ZZT_StartDate, Is.EqualTo(applicability.ZZT_StartDate));
			Assert.That(result.ZZT_EndDate, Is.EqualTo(applicability.ZZT_EndDate));
			Assert.That(result.ZZT_OrderNumber, Is.EqualTo(applicability.ZZT_OrderNumber));
			Assert.IsNull(result.RefCusTradeGroup);
		}

		[Test]
		public void Get_TariffCusExcludedTradeGroup_Data()
		{
			var preference = CreatePreference(Now.AddDays(1));
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = preference.ZZS_PK,
				ZX1_DataSetCode = "ZZS",
				ZX1_ZZ1_Tariff = preference.ZZS_PK,
				ZX1_ZX2_ConditionType = conditionType.ZX2_PK
			});
			var applicability = repo.Create(() => new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZX1_Conditions = condition.ZX1_PK,
				ZZT_DataSetPK = preference.ZZS_PK,
				ZZT_DataSetCode = "ZZS",
			});
			var excluded = repo.Create(() => new RefCusExcludedTradeGroup
			{
				ZZC_DataSetPK = preference.ZZS_PK,
				ZZC_DataSetCode = "ZZS",
				ZZC_ZZT_Applicability = applicability.ZZT_PK,
				ZZC_ZZA_TradeGroup = tradeGroup.ZZA_PK
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusConditions.ElementAt(0).RefCusApplicabilities.ElementAt(0).RefCusExcludedTradeGroups.ElementAt(0);
			Assert.That(result.RefCusTradeGroup.ZZA_TradeGroup, Is.EqualTo(tradeGroup.ZZA_TradeGroup));
		}

		RefCusPreference CreatePreference(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusPreference { ZZS_PK = Guid.NewGuid(), ZZS_Description = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZS_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusPreference> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusPreference> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusPreferenceService(repo, new RefCusConditionService(repo), new RefCusApplicabilityService(repo));
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZS_Description);
		}

		DateTime Now;
		ObjectReferenceDataRepository repo;
		RefLanguageType languageType;
		RefCusConditionType conditionType;
		RefCusConditionValueType valueType;
		RefCusTradeGroup tradeGroup;

		[SetUp]
		public void SetUp()
		{
			Now = DateTime.UtcNow;
			repo = new ObjectReferenceDataRepository();
			languageType = repo.Create(() => new RefLanguageType
			{
				ZX6_PK = Guid.NewGuid(),
				ZX6_Language = "EN",
				ZX6_Description = "English"
			});
			conditionType = repo.Create(() => new RefCusConditionType
			{
				ZX2_PK = Guid.NewGuid(),
				ZX2_ConditionType = "A"
			});
			valueType = repo.Create(() => new RefCusConditionValueType
			{
				ZX4_PK = Guid.NewGuid(),
				ZX4_Description = "AA"
			});
			tradeGroup = repo.Create(() => new RefCusTradeGroup
			{
				ZZA_PK = Guid.NewGuid(),
				ZZA_TradeGroup = "A"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = languageType.ZX6_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = conditionType.ZX2_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = valueType.ZX4_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = tradeGroup.ZZA_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
		}
	}
}
