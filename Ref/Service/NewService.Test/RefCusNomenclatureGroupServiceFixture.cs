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
	class RefCusNomenclatureGroupServiceFixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);
		static string TblPrefix => "ZZ5";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusNomenclatureGroup);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusNomenclatureGroup { ZZ5_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZ5_Value = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZ5_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusNomenclatureGroup { ZZ5_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZ5_Value = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZ5_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZ5_Value).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetDataCoreLinqCanBeTranslated_RefCusNomenclatureGroupService()
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
				repository.Add(new RefCusNomenclatureGroupType
				{
					ZZ9_PK = new Guid("8DE9A0F3-4E87-4A19-8711-1D52C1044DB8"),
					ZZ9_Description = "cds test type",
					ZZ9_GroupType = "CDS"
				});
				await repository.SaveChangesAsync();

				repository.Add(new RefCusNomenclatureGroup
				{
					ZZ5_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"),
					ZZ5_Value = "BB",
					ZZ5_CompositeKey = "test composite key",
					ZZ5_Description = "test refcusNomencaltureGroup",
					ZZ5_StartDate = Now,
					ZZ5_EndDate = Now.AddDays(1),
					ZZ5_ZZZ_NKDataGrouping = "AU",
					ZZ5_ZZ9_NKNomenclatureGroupType = "CDS"
				});
				await repository.SaveChangesAsync();

				var refCusNomenclatureGroupService = new RefCusNomenclatureGroupService(repository, new RefCusConditionService(repository), new RefCusApplicabilityService(repository), new RefCusTariffBRCharacteristicService(repository));
				var refCusNomenclatureGroupEnumerable = refCusNomenclatureGroupService.GetData(null, Now, CheckpointHelper.Create(new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5")), null, DataSetId).OrderBy(x => x.ZZ5_Value);
				Assert.DoesNotThrow(() =>
				{
					var refCusNomenclatureGroupResults = refCusNomenclatureGroupEnumerable.ToArray();
					Assert.NotNull(refCusNomenclatureGroupResults);
				});
			}
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_NomenclatureGroup_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			CreateGroup(Now, "AA");
			CreateGroup(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZ5_Value).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_NomenclatureGroupNote_Data()
		{
			var nomenclatureGroup = CreateGroup(Now.AddDays(1));
			var note = repo.Create(() => new RefCusNomenclatureGroupNote
			{
				ZZL_PK = Guid.NewGuid(),
				ZZL_ZX6_NKLanguage = "AA",
				ZZL_Note = "BB",
				ZZL_NoteType = "CC",
				ZZL_ZZZ_NKDataGrouping = "AU",
				ZZL_ZZ5_NomenclatureGroup = nomenclatureGroup.ZZ5_PK
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusNomenclatureGroupNotes.ElementAt(0);
			Assert.That(result.ZZL_ZX6_NKLanguage, Is.EqualTo(note.ZZL_ZX6_NKLanguage));
			Assert.That(result.ZZL_Note, Is.EqualTo(note.ZZL_Note));
			Assert.That(result.ZZL_NoteType, Is.EqualTo(note.ZZL_NoteType));
			Assert.That(result.ZZL_ZZZ_NKDataGrouping, Is.EqualTo(note.ZZL_ZZZ_NKDataGrouping));
		}

		[Test]
		public void GetLatest_NomenclatureGroup_Data()
		{
			var nomenclatureGroup = repo.Create(() => new RefCusNomenclatureGroup
			{
				ZZ5_PK = Guid.NewGuid(),
				ZZ5_CompositeKey = "AA",
				ZZ5_Description = "BB",
				ZZ5_StartDate = Now,
				ZZ5_EndDate = Now.AddDays(1),
				ZZ5_ZZZ_NKDataGrouping = "AU",
				ZZ5_Value = "D"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = nomenclatureGroup.ZZ5_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZ5_CompositeKey, Is.EqualTo(nomenclatureGroup.ZZ5_CompositeKey));
			Assert.That(result.ZZ5_Description, Is.EqualTo(nomenclatureGroup.ZZ5_Description));
			Assert.That(result.ZZ5_EndDate, Is.EqualTo(nomenclatureGroup.ZZ5_EndDate));
			Assert.That(result.ZZ5_StartDate, Is.EqualTo(nomenclatureGroup.ZZ5_StartDate));
			Assert.That(result.ZZ5_Value, Is.EqualTo(nomenclatureGroup.ZZ5_Value));
		}

		[Test]
		public void Get_RefCusNomenclatureLanguage_Data()
		{
			var nomenclatureGroup = CreateGroup(Now.AddDays(1));
			var language = repo.Create(() => new RefCusNomenclatureLanguage
			{
				ZX8_PK = Guid.NewGuid(),
				ZX8_ZZ5_NomenclatureGroup = nomenclatureGroup.ZZ5_PK,
				ZX8_ZX6_NKLanguage = "EN",
				ZX8_Description = "English"
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusNomenclatureLanguages.ElementAt(0);
			Assert.That(result.ZX8_Description, Is.EqualTo(language.ZX8_Description));
			Assert.That(result.ZX8_ZX6_NKLanguage, Is.EqualTo(language.ZX8_ZX6_NKLanguage));
		}

		[Test]
		public void Get_RefCusConditionValue_Data()
		{
			var nomenclatureGroup = CreateGroup(Now.AddDays(1));
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = nomenclatureGroup.ZZ5_PK,
				ZX1_DataSetCode = "ZZ5",
				ZX1_ZZ5_Nomenclature = nomenclatureGroup.ZZ5_PK,
				ZX1_ZX2_ConditionType = conditionType.ZX2_PK,
			});
			var value = repo.Create(() => new RefCusConditionValue
			{
				ZX3_DataSetPK = nomenclatureGroup.ZZ5_PK,
				ZX3_DataSetCode = "ZZ5",
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
			var nomenclatureGroup = CreateGroup(Now.AddDays(1));
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = nomenclatureGroup.ZZ5_PK,
				ZX1_DataSetCode = "ZZ5",
				ZX1_ZZ5_Nomenclature = nomenclatureGroup.ZZ5_PK,
				ZX1_ZX2_ConditionType = conditionType.ZX2_PK,
			});
			var value = repo.Create(() => new RefCusConditionLanguage
			{
				ZXJ_DataSetPK = nomenclatureGroup.ZZ5_PK,
				ZXJ_DataSetCode = "ZZ5",
				ZXJ_ZX6_NKLanguage = "EN",
				ZXJ_ZX1_Condition = condition.ZX1_PK,
				ZXJ_Comment = "A",
				ZXJ_Source = "XXX"
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusConditions.ElementAt(0).RefCusConditionLanguages.ElementAt(0);
			Assert.That(result.ZXJ_Comment, Is.EqualTo(value.ZXJ_Comment));
			Assert.That(result.ZXJ_Source, Is.EqualTo(value.ZXJ_Source));
			Assert.That(result.ZXJ_ZX6_NKLanguage, Is.EqualTo(value.ZXJ_ZX6_NKLanguage));
		}

		[Test]
		public void Get_TariffCusExcludedTradeGroup_Data()
		{
			var nomenclatureGroup = CreateGroup(Now.AddDays(1));
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = nomenclatureGroup.ZZ5_PK,
				ZX1_DataSetCode = "ZZ5",
				ZX1_ZZ1_Tariff = nomenclatureGroup.ZZ5_PK,
				ZX1_ZX2_ConditionType = conditionType.ZX2_PK
			});
			var applicability = repo.Create(() => new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZX1_Conditions = condition.ZX1_PK,
				ZZT_DataSetPK = nomenclatureGroup.ZZ5_PK,
				ZZT_DataSetCode = "ZZ5",
			});
			var excluded = repo.Create(() => new RefCusExcludedTradeGroup
			{
				ZZC_DataSetPK = nomenclatureGroup.ZZ5_PK,
				ZZC_DataSetCode = "ZZ5",
				ZZC_ZZT_Applicability = applicability.ZZT_PK,
				ZZC_ZZA_TradeGroup = tradeGroup.ZZA_PK
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusConditions.ElementAt(0).RefCusApplicabilities.ElementAt(0).RefCusExcludedTradeGroups.ElementAt(0);
			Assert.That(result.RefCusTradeGroup.ZZA_TradeGroup, Is.EqualTo(tradeGroup.ZZA_TradeGroup));
		}

		[Test]
		public void Get_RefCusCondition_Data()
		{
			var nomenclatureGroup = CreateGroup(Now.AddDays(1));
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = nomenclatureGroup.ZZ5_PK,
				ZX1_DataSetCode = "ZZ5",
				ZX1_ZZ5_Nomenclature = nomenclatureGroup.ZZ5_PK,
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
		public void GetRefCusTariffBRCharacteristicData()
		{
			var group = CreateGroup(Now.AddDays(1));
			var bRCharacteristic = repo.Create(() => new RefCusTariffBRCharacteristic
			{
				ZB1_PK = Guid.NewGuid(),
				ZB1_CharacteristicType = "ABC",
				ZB1_ZZ5_Nomenclature = group.ZZ5_PK,
				ZB1_DataSetPK = group.ZZ5_PK,
				ZB1_DataSetCode = "ZZ5",
				ZB1_Style = "K",
				ZB1_MaxLength = 10,
				ZB1_DecimalPlaces = 4,
				ZB1_Code = "XYZ",
				ZB1_Text = "TXT",
				ZB1_StartDate = Now,
				ZB1_EndDate = Now.AddDays(1),
				ZB1_IsImport = true,
				ZB1_IsExport = false,
				ZB1_IsMandatory = true,
				ZB1_IsConditioningAttribute = false,
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusTariffBRCharacteristics.ElementAt(0);
			Assert.That(result.ZB1_CharacteristicType, Is.EqualTo(bRCharacteristic.ZB1_CharacteristicType));
			Assert.That(result.ZB1_Style, Is.EqualTo(bRCharacteristic.ZB1_Style));
			Assert.That(result.ZB1_MaxLength, Is.EqualTo(bRCharacteristic.ZB1_MaxLength));
			Assert.That(result.ZB1_DecimalPlaces, Is.EqualTo(bRCharacteristic.ZB1_DecimalPlaces));
			Assert.That(result.ZB1_Code, Is.EqualTo(bRCharacteristic.ZB1_Code));
			Assert.That(result.ZB1_Text, Is.EqualTo(bRCharacteristic.ZB1_Text));
			Assert.That(result.ZB1_StartDate, Is.EqualTo(bRCharacteristic.ZB1_StartDate));
			Assert.That(result.ZB1_EndDate, Is.EqualTo(bRCharacteristic.ZB1_EndDate));
			Assert.That(result.ZB1_IsImport, Is.EqualTo(bRCharacteristic.ZB1_IsImport));
			Assert.That(result.ZB1_IsExport, Is.EqualTo(bRCharacteristic.ZB1_IsExport));
			Assert.That(result.ZB1_IsMandatory, Is.EqualTo(bRCharacteristic.ZB1_IsMandatory));
			Assert.That(result.ZB1_IsConditioningAttribute, Is.EqualTo(bRCharacteristic.ZB1_IsConditioningAttribute));
		}

		[Test]
		public void GetRefCusTariffBRCharacteristicValueData()
		{
			var group = CreateGroup(Now.AddDays(1));
			var brCharacteristic = repo.Create(() => new RefCusTariffBRCharacteristic
			{
				ZB1_PK = Guid.NewGuid(),
				ZB1_ZZ5_Nomenclature = group.ZZ5_PK,
				ZB1_DataSetPK = group.ZZ5_PK,
				ZB1_DataSetCode = "ZZ5",
			});

			var bRCharacteristicValue = repo.Create(() => new RefCusTariffBRCharacteristicValue
			{
				ZB2_PK = Guid.NewGuid(),
				ZB2_DataSetPK = group.ZZ5_PK,
				ZB2_DataSetCode = "ZZ5",
				ZB2_ZB1_Characteristic = brCharacteristic.ZB1_PK,
				ZB2_Value = "VAL",
				ZB2_Description = "DES",
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusTariffBRCharacteristics.ElementAt(0).RefCusTariffBRCharacteristicValues.ElementAt(0);
			Assert.That(result.ZB2_Value, Is.EqualTo(bRCharacteristicValue.ZB2_Value));
			Assert.That(result.ZB2_Description, Is.EqualTo(bRCharacteristicValue.ZB2_Description));
		}
		[Test]
		public void GetRefCusTariffBRCharacteristicAttributeData()
		{
			var group = CreateGroup(Now.AddDays(1));
			var brCharacteristic = repo.Create(() => new RefCusTariffBRCharacteristic
			{
				ZB1_PK = Guid.NewGuid(),
				ZB1_ZZ5_Nomenclature = group.ZZ5_PK,
				ZB1_DataSetPK = group.ZZ5_PK,
				ZB1_DataSetCode = "ZZ5",
			});

			var bRCharacteristicAttribute = repo.Create(() => new RefCusTariffBRCharacteristicAttribute
			{
				ZB3_PK = Guid.NewGuid(),
				ZB3_DataSetPK = group.ZZ5_PK,
				ZB3_DataSetCode = "ZZ5",
				ZB3_ZB1_Characteristic = brCharacteristic.ZB1_PK,
				ZB3_Name = "XYZ",
				ZB3_Code = "CD",
				ZB3_Value = "VAL"
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusTariffBRCharacteristics.ElementAt(0).RefCusTariffBRCharacteristicAttributes.ElementAt(0);
			Assert.That(result.ZB3_Name, Is.EqualTo(bRCharacteristicAttribute.ZB3_Name));
			Assert.That(result.ZB3_Code, Is.EqualTo(bRCharacteristicAttribute.ZB3_Code));
			Assert.That(result.ZB3_Value, Is.EqualTo(bRCharacteristicAttribute.ZB3_Value));
		}

		[Test]
		public void Get_TariffApplicability_Data()
		{
			var nomenclatureGroup = CreateGroup(Now.AddDays(1));
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = nomenclatureGroup.ZZ5_PK,
				ZX1_DataSetCode = "ZZ5",
				ZX1_ZZ1_Tariff = nomenclatureGroup.ZZ5_PK,
				ZX1_ZX2_ConditionType = conditionType.ZX2_PK
			});
			var applicability = repo.Create(() => new RefCusApplicability
			{
				ZZT_ZX1_Conditions = condition.ZX1_PK,
				ZZT_DataSetPK = nomenclatureGroup.ZZ5_PK,
				ZZT_DataSetCode = "ZZ5",
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

		RefCusNomenclatureGroup CreateGroup(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusNomenclatureGroup { ZZ5_PK = Guid.NewGuid(), ZZ5_Value = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZ5_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusNomenclatureGroup> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusNomenclatureGroup> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusNomenclatureGroupService(repo, new RefCusConditionService(repo), new RefCusApplicabilityService(repo), new RefCusTariffBRCharacteristicService(repo));
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZ5_Value);
		}

		DateTime Now;
		ObjectReferenceDataRepository repo;
		RefCusTradeGroup tradeGroup;
		RefCusConditionType conditionType;
		RefCusConditionValueType valueType;
		RefLanguageType languageType;

		[SetUp]
		public void SetUp()
		{
			Now = DateTime.UtcNow;
			repo = new ObjectReferenceDataRepository();
			tradeGroup = repo.Create(() => new RefCusTradeGroup
			{
				ZZA_PK = Guid.NewGuid(),
				ZZA_TradeGroup = "A"
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
			languageType = repo.Create(() => new RefLanguageType
			{
				ZX6_PK = Guid.NewGuid(),
				ZX6_Language = "EN",
				ZX6_Description = "English"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = tradeGroup.ZZA_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = conditionType.ZX2_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = valueType.ZX4_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = languageType.ZX6_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
		}
	}
}
