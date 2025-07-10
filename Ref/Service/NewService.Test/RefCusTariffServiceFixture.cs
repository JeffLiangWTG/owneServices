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
	public class RefCusTariffServiceFixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);
		[Test]
		public void GetDeletedTariff()
		{
			var tariffType = repo.Create(() => new RefCusTariffType { ZZI_PK = Guid.NewGuid() });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = tariffType.ZZI_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });

			var tariff1 = repo.Create(() => new RefCusTariffView { ZZ1_ZZI_TariffType = tariffType.ZZI_PK, RVC_DataSetId = DataSetId, RVC_ParentPK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD1"), RVC_LastUpdatedUTC = Now });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = tariff1.RVC_LastUpdatedUTC, RVC_DataSetId = DataSetId, RVC_ParentCode = "ZZ1", RVC_ParentPK = tariff1.RVC_ParentPK, RVC_IsPublished = true });
			repo.Create(() => new RefCusRate { ZZ2_DataSetPK = tariff1.RVC_ParentPK, ZZ2_ZZ1_Tariff = tariff1.RVC_ParentPK });
			var tariff2 = repo.Create(() => new RefCusTariffView { ZZ1_ZZI_TariffType = tariffType.ZZI_PK, RVC_DataSetId = DataSetId, RVC_ParentPK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD2"), RVC_LastUpdatedUTC = Now, RVC_Deleted = true });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = tariff2.RVC_LastUpdatedUTC, RVC_DataSetId = DataSetId, RVC_ParentCode = "ZZ1", RVC_ParentPK = tariff2.RVC_ParentPK, RVC_IsPublished = true });
			repo.Create(() => new RefCusRate { ZZ2_DataSetPK = tariff2.RVC_ParentPK, ZZ2_ZZ1_Tariff = tariff2.RVC_ParentPK });
			var tariff3 = repo.Create(() => new RefCusTariffView { ZZ1_ZZI_TariffType = tariffType.ZZI_PK, RVC_DataSetId = DataSetId, RVC_ParentPK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD3"), RVC_LastUpdatedUTC = Now });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = tariff3.RVC_LastUpdatedUTC, RVC_DataSetId = DataSetId, RVC_ParentCode = "ZZ1", RVC_ParentPK = tariff3.RVC_ParentPK, RVC_IsPublished = true });
			repo.Create(() => new RefCusRate { ZZ2_DataSetPK = tariff3.RVC_ParentPK, ZZ2_ZZ1_Tariff = tariff3.RVC_ParentPK });

			var dataSets = GetDataSets(null, Now).ToArray();
			Assert.That(dataSets[0].RefCusRates.Length, Is.EqualTo(1));
			Assert.IsNull(dataSets[1].RefCusRates);
			Assert.That(dataSets[2].RefCusRates.Length, Is.EqualTo(1));
		}

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var tariffType1 = repo.Create(() => new RefCusTariffType { ZZI_PK = Guid.NewGuid() });
			var tariff1 = repo.Create(() => new RefCusTariffView
			{
				ZZ1_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"),
				ZZ1_TariffCode = "BB",
				ZZ1_ZZI_TariffType = tariffType1.ZZI_PK,
				RVC_LastUpdatedUTC = Now,
				RVC_ParentPK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"),
				RVC_DataSetId = DataSetId
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = tariffType1.ZZI_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });

			var tariffType2 = repo.Create(() => new RefCusTariffType { ZZI_PK = Guid.NewGuid() });
			var tariff2 = repo.Create(() => new RefCusTariffView
			{
				ZZ1_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"),
				ZZ1_TariffCode = "CC",
				ZZ1_ZZI_TariffType = tariffType2.ZZI_PK,
				RVC_LastUpdatedUTC = Now,
				RVC_ParentPK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"),
				RVC_DataSetId = DataSetId
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = tariffType2.ZZI_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });

			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZ1_TariffCode).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetDataCoreLinqCanBeTranslated_RefCusTariffService()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repository = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repository.Add(new RefDataGrouping
				{
					ZZZ_PK = new Guid("52672560-9B5B-40DD-8115-DC8B178157B0"),
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

				var refCusTariffType = new RefCusTariffType
				{
					ZZI_PK = new Guid("8EF10807-5409-47D0-A543-956891F3441E"),
					ZZI_Description = "test tariff type",
					ZZI_TariffType = "CC",
					ZZI_ZZZ_NKDataGrouping = "AU",
					ZZI_ZZ9_NKNomenclatureGroupType = "CDS"
				};
				repository.Add(refCusTariffType);
				await repository.SaveChangesAsync();

				repository.Add(new RefCusTariff
				{
					ZZ1_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"),
					ZZ1_Description = "test tariff",
					ZZ1_ZZZ_NKDataGrouping = "AU",
					ZZ1_TariffCode = "BB",
					ZZ1_CompositeKeyOnZZ5 = "01",
					ZZ1_ZZF_NKTaxOrFeeCode = "VAT",
					ZZ1_StartDate = Now,
					ZZ1_EndDate = Now.AddDays(1),
					ZZ1_ZZI_TariffType = refCusTariffType.ZZI_PK
				});
				await repository.SaveChangesAsync();

				var service = new RefCusTariffService(repository, new RefCusApplicabilityService(repository), new RefCusConditionService(repository), new RefCusTariffBRCharacteristicService(repository));
				var refCusTariffEnumerable = service.GetData(null, Now, CheckpointHelper.Create(new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5")), 1, DataSetId).OrderBy(x => x.ZZ1_TariffCode);
				Assert.DoesNotThrow(() =>
				{
					var refCusTariffResults = refCusTariffEnumerable.ToArray();
					Assert.NotNull(refCusTariffResults);
				});
			}
		}

		[Test]
		public void GetRefCusVATApplicabilityData()
		{
			var tariff = CreateTariff(Now.AddDays(1));
			var vat1 = repo.Create(() => new RefCusVATApplicability
			{
				ZX5_ZZ1_Tariff = tariff.RVC_ParentPK,
				ZX5_DataSetPK = tariff.RVC_ParentPK,
				ZX5_ZZA_TradeGroup = tradeGroup.ZZA_PK,
				ZX5_AdditionalCode = "A",
				ZX5_Description = "B",
				ZX5_EndDate = Now.AddDays(1),
				ZX5_StartDate = Now,
				ZX5_ZZF_NKTaxOrFeeCode = "V",
				ZX5_ZZZ_NKDataGrouping = "ZA",
				ZX5_VATCategory = "A001"
			});
			var national = repo.Create(() => new RefCusTariffNationalCode
			{
				ZZW_PK = Guid.NewGuid(),
				ZZW_ZZ1_Tariff = tariff.RVC_ParentPK,
			});
			var vat2 = repo.Create(() => new RefCusVATApplicability
			{
				ZX5_ZZW_TariffNationalCode = national.ZZW_PK,
				ZX5_DataSetPK = tariff.RVC_ParentPK,
				ZX5_AdditionalCode = "A",
				ZX5_Description = "B",
				ZX5_EndDate = Now.AddDays(1),
				ZX5_StartDate = Now,
				ZX5_ZZF_NKTaxOrFeeCode = "V",
				ZX5_ZZZ_NKDataGrouping = "ZA"
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusVATApplicabilities.ElementAt(0);
			Assert.That(result.ZX5_AdditionalCode, Is.EqualTo(vat1.ZX5_AdditionalCode));
			Assert.That(result.ZX5_Description, Is.EqualTo(vat1.ZX5_Description));
			Assert.That(result.ZX5_EndDate, Is.EqualTo(vat1.ZX5_EndDate));
			Assert.That(result.ZX5_StartDate, Is.EqualTo(vat1.ZX5_StartDate));
			Assert.That(result.ZX5_ZZF_NKTaxOrFeeCode, Is.EqualTo(vat1.ZX5_ZZF_NKTaxOrFeeCode));
			Assert.That(result.ZX5_ZZZ_NKDataGrouping, Is.EqualTo(vat1.ZX5_ZZZ_NKDataGrouping));
			Assert.That(result.ZX5_VATCategory, Is.EqualTo(vat1.ZX5_VATCategory));
			Assert.That(result.RefCusTradeGroup.ZZA_TradeGroup, Is.EqualTo(tradeGroup.ZZA_TradeGroup));
			Assert.That(result.ZX5_DataSetId, Is.EqualTo(DataSetId));
			result = dataSets.ElementAt(0).RefCusTariffNationalCodes.ElementAt(0).RefCusVATApplicabilities.ElementAt(0);
			Assert.That(result.ZX5_AdditionalCode, Is.EqualTo(vat2.ZX5_AdditionalCode));
			Assert.That(result.ZX5_Description, Is.EqualTo(vat2.ZX5_Description));
			Assert.That(result.ZX5_EndDate, Is.EqualTo(vat2.ZX5_EndDate));
			Assert.That(result.ZX5_StartDate, Is.EqualTo(vat2.ZX5_StartDate));
			Assert.That(result.ZX5_ZZF_NKTaxOrFeeCode, Is.EqualTo(vat2.ZX5_ZZF_NKTaxOrFeeCode));
			Assert.That(result.ZX5_ZZZ_NKDataGrouping, Is.EqualTo(vat2.ZX5_ZZZ_NKDataGrouping));
			Assert.That(result.ZX5_DataSetId, Is.EqualTo(DataSetId));
			Assert.IsNull(result.RefCusTradeGroup);
		}

		[Test]
		public void GetRefCusTariffNationalCodeData()
		{
			var tariff = CreateTariff(Now.AddDays(1));
			var national = repo.Create(() => new RefCusTariffNationalCode
			{
				ZZW_PK = Guid.NewGuid(),
				ZZW_ZZ1_Tariff = tariff.RVC_ParentPK,
				ZZW_Description = "A",
				ZZW_StartDate = Now,
				ZZW_EndDate = Now.AddDays(1),
				ZZW_NationalCode = "B",
				ZZW_ZZZ_NKDataGrouping = "ZA",
				ZZW_ZZF_NKTaxOrFeeCode = "VAT"
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusTariffNationalCodes.ElementAt(0);
			Assert.That(result.ZZW_Description, Is.EqualTo(national.ZZW_Description));
			Assert.That(result.ZZW_StartDate, Is.EqualTo(national.ZZW_StartDate));
			Assert.That(result.ZZW_EndDate, Is.EqualTo(national.ZZW_EndDate));
			Assert.That(result.ZZW_NationalCode, Is.EqualTo(national.ZZW_NationalCode));
			Assert.That(result.ZZW_ZZZ_NKDataGrouping, Is.EqualTo(national.ZZW_ZZZ_NKDataGrouping));
			Assert.That(result.ZZW_ZZF_NKTaxOrFeeCode, Is.EqualTo(national.ZZW_ZZF_NKTaxOrFeeCode));
		}

		[Test]
		public void GetRefCusTariffBRCharacteristicData()
		{
			var tariff = CreateTariff(Now.AddDays(1));
			var bRCharacteristic = repo.Create(() => new RefCusTariffBRCharacteristic
			{
				ZB1_PK = Guid.NewGuid(),
				ZB1_CharacteristicType = "ABC",
				ZB1_ZZ1_Tariff = tariff.RVC_ParentPK,
				ZB1_DataSetCode = "ZZ1",
				ZB1_DataSetPK = tariff.RVC_ParentPK,
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
			var tariff = CreateTariff(Now.AddDays(1));
			var brCharacteristic = repo.Create(() => new RefCusTariffBRCharacteristic
			{
				ZB1_PK = Guid.NewGuid(),
				ZB1_ZZ1_Tariff = tariff.RVC_ParentPK,
				ZB1_DataSetCode = "ZZ1",
				ZB1_DataSetPK = tariff.RVC_ParentPK,
			});

			var bRCharacteristicValue = repo.Create(() => new RefCusTariffBRCharacteristicValue
			{
				ZB2_PK = Guid.NewGuid(),
				ZB2_DataSetPK = tariff.RVC_ParentPK,
				ZB2_DataSetCode = "ZZ1",
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
			var tariff = CreateTariff(Now.AddDays(1));
			var brCharacteristic = repo.Create(() => new RefCusTariffBRCharacteristic
			{
				ZB1_PK = Guid.NewGuid(),
				ZB1_ZZ1_Tariff = tariff.RVC_ParentPK,
				ZB1_DataSetCode = "ZZ1",
				ZB1_DataSetPK = tariff.RVC_ParentPK,
			});

			var bRCharacteristicAttribute = repo.Create(() => new RefCusTariffBRCharacteristicAttribute
			{
				ZB3_PK = Guid.NewGuid(),
				ZB3_DataSetPK = tariff.RVC_ParentPK,
				ZB3_DataSetCode = "ZZ1",
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
		public void GetRefCusConditionValueData()
		{
			var tariff = CreateTariff(Now.AddDays(1));
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = tariff.RVC_ParentPK,
				ZX1_DataSetCode = "ZZ1",
				ZX1_ZZ1_Tariff = tariff.RVC_ParentPK,
				ZX1_ZX2_ConditionType = conditionType.ZX2_PK,
			});
			var value = repo.Create(() => new RefCusConditionValue
			{
				ZX3_DataSetPK = tariff.RVC_ParentPK,
				ZX3_DataSetCode = "ZZ1",
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
			var tariff = CreateTariff(Now.AddDays(1));
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = tariff.RVC_ParentPK,
				ZX1_DataSetCode = "ZZ1",
				ZX1_ZZ1_Tariff = tariff.RVC_ParentPK,
				ZX1_ZX2_ConditionType = conditionType.ZX2_PK,
			});
			var language = repo.Create(() => new RefCusConditionLanguage
			{
				ZXJ_DataSetPK = tariff.RVC_ParentPK,
				ZXJ_DataSetCode = "ZZ1",
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
		public void GetRefCusConditionData()
		{
			var tariff = CreateTariff(Now.AddDays(1));
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = tariff.RVC_ParentPK,
				ZX1_DataSetCode = "ZZ1",
				ZX1_ZZ1_Tariff = tariff.RVC_ParentPK,
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
			repo.Create(() => new RefCusConditionValue
			{
				ZX3_DataSetPK = tariff.RVC_ParentPK,
				ZX3_DataSetCode = "ZZ1",
				ZX3_ZX4_ValueType = valueType.ZX4_PK,
				ZX3_LogicalORWithinGroup = 0,
				ZX3_Value = "A",
				ZX3_ZX1_Condition = condition.ZX1_PK
			});
			repo.Create(() => new RefCusConditionValue
			{
				ZX3_DataSetPK = tariff.RVC_ParentPK,
				ZX3_DataSetCode = "ZZ1",
				ZX3_ZX4_ValueType = valueType.ZX4_PK,
				ZX3_LogicalORWithinGroup = 0,
				ZX3_Value = "A",
				ZX3_ZX1_Condition = Guid.NewGuid()
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
			Assert.AreEqual(1, result.RefCusConditionValues.Length);
		}

		[Test]
		public void GetTariffCusExcludedTradeGroupData()
		{
			var tariff = CreateTariff(Now.AddDays(1));
			var rate = repo.Create(() => new RefCusRate
			{
				ZZ2_PK = Guid.NewGuid(),
				ZZ2_ZZ1_Tariff = tariff.RVC_ParentPK,
				ZZ2_DataSetPK = tariff.RVC_ParentPK
			});
			var applicability = repo.Create(() => new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZ2_Rate = rate.ZZ2_PK,
				ZZT_DataSetPK = tariff.RVC_ParentPK,
				ZZT_DataSetCode = "ZZ1",
			});
			var excluded = repo.Create(() => new RefCusExcludedTradeGroup
			{
				ZZC_DataSetPK = tariff.RVC_ParentPK,
				ZZC_DataSetCode = "ZZ1",
				ZZC_ZZT_Applicability = applicability.ZZT_PK,
				ZZC_ZZA_TradeGroup = tradeGroup.ZZA_PK
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusRates.ElementAt(0).RefCusApplicabilities.ElementAt(0).RefCusExcludedTradeGroups.ElementAt(0);
			Assert.That(result.RefCusTradeGroup.ZZA_TradeGroup, Is.EqualTo(tradeGroup.ZZA_TradeGroup));
		}

		[Test]
		public void GetTariffApplicabilityData()
		{
			var tariff = CreateTariff(Now.AddDays(1));
			var rate = repo.Create(() => new RefCusRate
			{
				ZZ2_PK = Guid.NewGuid(),
				ZZ2_ZZ1_Tariff = tariff.RVC_ParentPK,
				ZZ2_DataSetPK = tariff.RVC_ParentPK
			});
			var applicability1 = repo.Create(() => new RefCusApplicability
			{
				ZZT_ZZ2_Rate = rate.ZZ2_PK,
				ZZT_DataSetPK = tariff.RVC_ParentPK,
				ZZT_DataSetCode = "ZZ1",
				ZZT_AdditionalCode = "A",
				ZZT_StartDate = Now,
				ZZT_EndDate = Now.AddDays(1),
				ZZT_OrderNumber = "1",
				ZZT_ZZA_TradeGroup = tradeGroup.ZZA_PK,
			});
			var condition = repo.Create(() => new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_DataSetPK = tariff.RVC_ParentPK,
				ZX1_DataSetCode = "ZZ1",
				ZX1_ZZ1_Tariff = tariff.RVC_ParentPK,
				ZX1_ZX2_ConditionType = conditionType.ZX2_PK
			});
			var applicability2 = repo.Create(() => new RefCusApplicability
			{
				ZZT_ZX1_Conditions = condition.ZX1_PK,
				ZZT_DataSetPK = tariff.RVC_ParentPK,
				ZZT_DataSetCode = "ZZ1",
				ZZT_AdditionalCode = "A",
				ZZT_StartDate = Now,
				ZZT_EndDate = Now.AddDays(1),
				ZZT_OrderNumber = "1",
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusRates.ElementAt(0).RefCusApplicabilities.ElementAt(0);
			Assert.That(result.ZZT_AdditionalCode, Is.EqualTo(applicability1.ZZT_AdditionalCode));
			Assert.That(result.ZZT_StartDate, Is.EqualTo(applicability1.ZZT_StartDate));
			Assert.That(result.ZZT_EndDate, Is.EqualTo(applicability1.ZZT_EndDate));
			Assert.That(result.ZZT_OrderNumber, Is.EqualTo(applicability1.ZZT_OrderNumber));
			Assert.That(result.RefCusTradeGroup.ZZA_TradeGroup, Is.EqualTo(tradeGroup.ZZA_TradeGroup));
			result = dataSets.ElementAt(0).RefCusConditions.ElementAt(0).RefCusApplicabilities.ElementAt(0);
			Assert.That(result.ZZT_AdditionalCode, Is.EqualTo(applicability2.ZZT_AdditionalCode));
			Assert.That(result.ZZT_StartDate, Is.EqualTo(applicability2.ZZT_StartDate));
			Assert.That(result.ZZT_EndDate, Is.EqualTo(applicability2.ZZT_EndDate));
			Assert.That(result.ZZT_OrderNumber, Is.EqualTo(applicability2.ZZT_OrderNumber));
			Assert.IsNull(result.RefCusTradeGroup);
		}

		[Test]
		public void GetLatestTariffRelationshipData()
		{
			var tariff = CreateTariff(Now.AddDays(1));
			var relationship = repo.Create(() => new RefCusTariffRelationship
			{
				ZZH_PK = Guid.NewGuid(),
				ZZH_TariffCode = "AA",
				ZZH_DataSetPK = tariff.RVC_ParentPK,
				ZZH_DataSetCode = "ZZ1",
			});
			var applicability = repo.Create(() => new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_DataSetPK = tariff.RVC_ParentPK,
				ZZT_DataSetCode = "ZZ1",
				ZZT_AdditionalCode = "ABC",
				ZZT_ZZH_TariffRelationship = relationship.ZZH_PK
			});
			var tariffType = repo.Create(() => new RefCusTariffType
			{
				ZZI_PK = Guid.NewGuid(),
				ZZI_Description = "AA",
				ZZI_TariffType = "BB",
				ZZI_ZZZ_NKDataGrouping = "AU"
			});
			relationship.ZZH_ZZI_TariffType = tariffType.ZZI_PK;
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = tariffType.ZZI_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusTariffRelationships.ElementAt(0);
			Assert.That(result.ZZH_TariffCode, Is.EqualTo(relationship.ZZH_TariffCode));
			Assert.That(result.RefCusTariffType.ZZI_Description, Is.EqualTo(tariffType.ZZI_Description));
			Assert.That(result.RefCusTariffType.ZZI_TariffType, Is.EqualTo(tariffType.ZZI_TariffType));
			Assert.That(result.RefCusTariffType.ZZI_ZZZ_NKDataGrouping, Is.EqualTo(tariffType.ZZI_ZZZ_NKDataGrouping));
			Assert.That(result.RefCusApplicabilities.Length, Is.GreaterThan(0));
			Assert.That(result.RefCusApplicabilities.ElementAt(0).ZZT_AdditionalCode, Is.EqualTo(applicability.ZZT_AdditionalCode));
		}

		[Test]
		public void GetLatestTariffUOMData()
		{
			var tariff = CreateTariff(Now.AddDays(1));
			var uom = repo.Create(() => new RefCusTariffUOM
			{
				ZZ8_Type = "AA",
				ZZ8_UOM = "BB",
				ZZ8_ZZ1_Tariff = tariff.ZZ1_PK,
				ZZ8_ZZA_TradeGroup = tradeGroup.ZZA_PK,
				ZZ8_DataSetPK = tariff.ZZ1_PK,
				ZZ8_DataSetCode = "ZZ1"
			});

			var national = repo.Create(() => new RefCusTariffNationalCode
			{
				ZZW_PK = Guid.NewGuid(),
				ZZW_ZZ1_Tariff = tariff.RVC_ParentPK,
			});

			var uom2 = repo.Create(() => new RefCusTariffUOM
			{
				ZZ8_Type = "CC",
				ZZ8_UOM = "DD",
				ZZ8_ZZW_TariffNationalCode = national.ZZW_PK,
				ZZ8_ZZA_TradeGroup = tradeGroup.ZZA_PK,
				ZZ8_DataSetPK = tariff.ZZ1_PK,
				ZZ8_DataSetCode = "ZZ1"
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusTariffUOMs.ElementAt(0);
			Assert.That(result.ZZ8_Type, Is.EqualTo(uom.ZZ8_Type));
			Assert.That(result.ZZ8_UOM, Is.EqualTo(uom.ZZ8_UOM));

			result = dataSets.ElementAt(0).RefCusTariffNationalCodes.ElementAt(0).RefCusTariffUOMs.ElementAt(0);
			Assert.That(result.ZZ8_Type, Is.EqualTo(uom2.ZZ8_Type));
			Assert.That(result.ZZ8_UOM, Is.EqualTo(uom2.ZZ8_UOM));
		}

		[Test]
		public void GetLatestTariffAdditionalCodeData()
		{
			var tariff = CreateTariff(Now.AddDays(1));
			var addCodePk1 = Guid.NewGuid();
			var addCodePk2 = Guid.NewGuid();
			var additionalCode1 = repo.Create(() => new RefCusTariffAdditionalCode
			{
				ZY2_PK = addCodePk1,
				ZY2_AdditionalCode = "ANY",
				ZY2_Description = "DESC",
				ZY2_IsMandatory = true,
				ZY2_ZZ1_Tariff = tariff.RVC_ParentPK,
				ZY2_DataSetCode = "ZZ1",
				ZY2_ZZZ_NKDataGrouping = "ZA",
				ZY2_DataSetPK = tariff.RVC_ParentPK,
				ZY2_ZY3_NKCategory = "CAT"
			});

			var additionalCodeLanguageToAddCode1 = repo.Create(() => new RefCusTariffAdditionalCodeLanguage
			{
				ZY4_Description = "LANG",
				ZY4_ZY2_TariffAdditionalCode = addCodePk1,
				ZY4_PK = Guid.NewGuid(),
				ZY4_DataSetCode = "ZZ1",
				ZY4_DataSetPK = tariff.RVC_ParentPK
			});

			var additionalCode2 = repo.Create(() => new RefCusTariffAdditionalCode
			{
				ZY2_PK = addCodePk2,
				ZY2_AdditionalCode = "TWO",
				ZY2_Description = "DESC2",
				ZY2_IsMandatory = true,
				ZY2_ZZ1_Tariff = tariff.RVC_ParentPK,
				ZY2_DataSetCode = "ZZ1",
				ZY2_ZZZ_NKDataGrouping = "ZA",
				ZY2_DataSetPK = tariff.RVC_ParentPK,
				ZY2_ZY3_NKCategory = "CAT"
			});

			var additionalCodeLanguageToAddCode2 = repo.Create(() => new RefCusTariffAdditionalCodeLanguage
			{
				ZY4_Description = "NEW",
				ZY4_ZY2_TariffAdditionalCode = addCodePk2,
				ZY4_PK = Guid.NewGuid(),
				ZY4_DataSetCode = "ZZ1",
				ZY4_DataSetPK = tariff.RVC_ParentPK
			});

			var applicability = repo.Create(() => new RefCusApplicability
			{
				ZZT_ZY2_AdditionalCode = addCodePk1,
				ZZT_DataSetPK = tariff.RVC_ParentPK,
				ZZT_DataSetCode = "ZZ1",
				ZZT_AdditionalCode = "A",
				ZZT_StartDate = Now,
				ZZT_EndDate = Now.AddDays(1),
				ZZT_OrderNumber = "1",
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.IsNotNull(result.RefCusTariffAdditionalCodes);
			Assert.Greater(result.RefCusTariffAdditionalCodes.Length, 0);

			var addCodeResult1 = result.RefCusTariffAdditionalCodes[0];
			Assert.IsNotNull(addCodeResult1.RefCusTariffAdditionalCodeLanguages);
			Assert.AreEqual(addCodeResult1.RefCusTariffAdditionalCodeLanguages.Length, 1);
			var addCodeLanguage1 = addCodeResult1.RefCusTariffAdditionalCodeLanguages[0];
			Assert.That(addCodeLanguage1.ZY4_Description, Is.EqualTo(additionalCodeLanguageToAddCode1.ZY4_Description));
			Assert.IsNotNull(addCodeResult1.RefCusApplicabilities);
			Assert.Greater(addCodeResult1.RefCusApplicabilities.Length, 0);

			var addCodeResult2 = result.RefCusTariffAdditionalCodes[1];
			Assert.IsNotNull(addCodeResult2.RefCusTariffAdditionalCodeLanguages);
			Assert.AreEqual(addCodeResult2.RefCusTariffAdditionalCodeLanguages.Length, 1);
			var addCodeLanguage2 = addCodeResult2.RefCusTariffAdditionalCodeLanguages[0];
			Assert.That(addCodeLanguage2.ZY4_Description, Is.EqualTo(additionalCodeLanguageToAddCode2.ZY4_Description));
			Assert.IsNotNull(addCodeResult2.RefCusApplicabilities);
			Assert.AreEqual(addCodeResult2.RefCusApplicabilities.Length, 0);
		}

		[Test]
		public void GetLatestTariffRateData()
		{
			var tariff = CreateTariff(Now.AddDays(1));
			var rate = repo.Create(() => new RefCusRate
			{
				ZZ2_StartDate = Now,
				ZZ2_EndDate = Now.AddDays(1),
				ZZ2_RateFormula = "BB",
				ZZ2_SelectorFormula = "CC",
				ZZ2_DataSetPK = tariff.RVC_ParentPK,
				ZZ2_ZZ1_Tariff = tariff.RVC_ParentPK
			});
			var rateUOM = repo.Create(() => new RefCusRateUOM
			{
				ZXG_UOM = "KG",
				ZXG_ZZ2_Rate = rate.ZZ2_PK,
				ZXG_DataSetPK = tariff.RVC_ParentPK
			});
			rate.RefCusRateUOMs = new[] { rateUOM };
			var national = repo.Create(() => new RefCusTariffNationalCode
			{
				ZZW_PK = Guid.NewGuid(),
				ZZW_ZZ1_Tariff = tariff.RVC_ParentPK,
			});
			var rate1 = repo.Create(() => new RefCusRate
			{
				ZZ2_StartDate = Now,
				ZZ2_EndDate = Now.AddDays(1),
				ZZ2_RateFormula = "BB",
				ZZ2_SelectorFormula = "CC",
				ZZ2_DataSetPK = tariff.RVC_ParentPK,
				ZZ2_ZZW_TariffNationalCode = national.ZZW_PK,
				ZZ2_ZY1_RateCode = rateCode.ZY1_PK,
				ZZ2_ZZS_Preference = preference.ZZS_PK
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusRates.ElementAt(0);
			Assert.That(result.ZZ2_StartDate, Is.EqualTo(rate.ZZ2_StartDate));
			Assert.That(result.ZZ2_EndDate, Is.EqualTo(rate.ZZ2_EndDate));
			Assert.That(result.ZZ2_RateFormula, Is.EqualTo(rate.ZZ2_RateFormula));
			Assert.That(result.ZZ2_SelectorFormula, Is.EqualTo(rate.ZZ2_SelectorFormula));
			Assert.That(result.RefCusRateUOMs[0].ZXG_UOM, Is.EqualTo(rate.RefCusRateUOMs.First().ZXG_UOM));
			result = dataSets.ElementAt(0).RefCusTariffNationalCodes.ElementAt(0).RefCusRates.ElementAt(0);
			Assert.That(result.ZZ2_StartDate, Is.EqualTo(rate1.ZZ2_StartDate));
			Assert.That(result.ZZ2_EndDate, Is.EqualTo(rate1.ZZ2_EndDate));
			Assert.That(result.ZZ2_RateFormula, Is.EqualTo(rate1.ZZ2_RateFormula));
			Assert.That(result.ZZ2_SelectorFormula, Is.EqualTo(rate1.ZZ2_SelectorFormula));
			Assert.That(result.RefCusPreference.ZZS_Description, Is.EqualTo(preference.ZZS_Description));
			Assert.That(result.RefCusRateCode.ZY1_Description, Is.EqualTo(rateCode.ZY1_Description));
		}

		[Test]
		public void GetLatestTariffAttributeData()
		{
			var tariff = CreateTariff(Now.AddDays(1));
			var attr = repo.Create(() => new RefCusTariffAttribute
			{
				ZZ3_DataSetPK = tariff.RVC_ParentPK,
				ZZ3_ZZ1_Tariff = tariff.RVC_ParentPK,
				ZZ3_Value = "VAL1",
			});
			var national = repo.Create(() => new RefCusTariffNationalCode
			{
				ZZW_PK = Guid.NewGuid(),
				ZZW_ZZ1_Tariff = tariff.RVC_ParentPK,
			});
			var attr1 = repo.Create(() => new RefCusTariffAttribute
			{
				ZZ3_DataSetPK = tariff.RVC_ParentPK,
				ZZ3_ZZW_TariffNationalCode = national.ZZW_PK,
				ZZ3_Value = "VAL2",
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusTariffAttributes.ElementAt(0);
			Assert.That(1, Is.EqualTo(dataSets.ElementAt(0).RefCusTariffAttributes.Length));
			Assert.That(result.ZZ3_Name, Is.EqualTo(attr.ZZ3_Name));
			Assert.That(result.ZZ3_Value, Is.EqualTo(attr.ZZ3_Value));
			result = dataSets.ElementAt(0).RefCusTariffNationalCodes.ElementAt(0).RefCusTariffAttributes.ElementAt(0);
			Assert.That(1, Is.EqualTo(dataSets.ElementAt(0).RefCusTariffNationalCodes.ElementAt(0).RefCusTariffAttributes.Length));
			Assert.That(result.ZZ3_Name, Is.EqualTo(attr1.ZZ3_Name));
			Assert.That(result.ZZ3_Value, Is.EqualTo(attr1.ZZ3_Value));
		}

		[Test]
		public void GetLatestTariffLanguageData()
		{
			var tariff = CreateTariff(Now.AddDays(1));
			var language = repo.Create(() => new RefCusTariffLanguage
			{
				ZX7_PK = Guid.NewGuid(),
				ZX7_ZZ1_Tariff = tariff.ZZ1_PK,
				ZX7_ZX6_NKLanguage = "EN",
				ZX7_Description = "English"
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusTariffLanguages.ElementAt(0);
			Assert.That(1, Is.EqualTo(dataSets.ElementAt(0).RefCusTariffLanguages.Length));
			Assert.That(result.ZX7_ZX6_NKLanguage, Is.EqualTo(language.ZX7_ZX6_NKLanguage));
			Assert.That(result.ZX7_Description, Is.EqualTo(language.ZX7_Description));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatestRefCusTariffFilter(int? daysOffset, int rtOffset, string[] expected)
		{
			CreateTariff(Now, "AA");
			CreateTariff(Now.AddDays(2), "BB");

			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZ1_TariffCode).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatestRefCusTariffData()
		{
			var tariffType = repo.Create(() => new RefCusTariffType { ZZI_PK = Guid.NewGuid(), ZZI_TariffType = "Type", ZZI_Description = "AA", ZZI_ZZZ_NKDataGrouping = "ZA" });
			var tariffPK = Guid.NewGuid();
			var tariff = repo.Create(() => new RefCusTariffView
			{
				ZZ1_PK = tariffPK,
				ZZ1_ZZI_TariffType = tariffType.ZZI_PK,
				ZZ1_TariffCode = "Code",
				ZZ1_IAMUnique = 1,
				ZZ1_Description = "DES",
				ZZ1_StartDate = Now,
				ZZ1_EndDate = Now.AddDays(1),
				ZZ1_ZZF_NKTaxOrFeeCode = "F",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				RVC_ParentPK = tariffPK,
				RVC_LastUpdatedUTC = Now.AddDays(1),
				RVC_DataSetId = DataSetId
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = tariffType.ZZI_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });

			CreateChild<RefCusRate>(nameof(RefCusRate.ZZ2_ZZ1_Tariff), tariff.ZZ1_PK);
			CreateChild<RefCusTariffRelationship>(nameof(RefCusTariffRelationship.ZZH_ZZ1_Tariff), tariff.ZZ1_PK);

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZ1_TariffCode, Is.EqualTo(tariff.ZZ1_TariffCode));
			Assert.That(result.ZZ1_IAMUnique, Is.EqualTo(tariff.ZZ1_IAMUnique));
			Assert.That(result.ZZ1_Description, Is.EqualTo(tariff.ZZ1_Description));
			Assert.That(result.ZZ1_StartDate, Is.EqualTo(tariff.ZZ1_StartDate));
			Assert.That(result.ZZ1_EndDate, Is.EqualTo(tariff.ZZ1_EndDate));
			Assert.That(result.ZZ1_ZZF_NKTaxOrFeeCode, Is.EqualTo(tariff.ZZ1_ZZF_NKTaxOrFeeCode));
			Assert.That(result.ZZ1_ZZZ_NKDataGrouping, Is.EqualTo(tariff.ZZ1_ZZZ_NKDataGrouping));
			Assert.That(result.RefCusTariffType.ZZI_Description, Is.EqualTo(tariffType.ZZI_Description));
			Assert.That(result.RefCusTariffType.ZZI_ZZZ_NKDataGrouping, Is.EqualTo(tariffType.ZZI_ZZZ_NKDataGrouping));
			Assert.That(result.RefCusTariffType.ZZI_TariffType, Is.EqualTo(tariffType.ZZI_TariffType));
		}

		[Test]
		public void GetAllTariffData()
		{
			var tariff1 = CreateTariff(Now.AddDays(1), "AA");
			var tariff2 = CreateTariff(Now.AddDays(2), "BB");
			var tariff3 = CreateTariff(Now.AddDays(3), "CC");

			var dataSets = GetDataSets(Now, null, null, 2).ToList();
			Assert.AreEqual(3, dataSets.Count);
		}

		RefCusTariffView CreateTariff(DateTime dateTime, string code = null)
		{
			var tariffType = repo.Create(() => new RefCusTariffType { ZZI_PK = Guid.NewGuid() });
			var tariffPK = Guid.NewGuid();
			var tariff = repo.Create(() => new RefCusTariffView { ZZ1_PK = tariffPK, ZZ1_TariffCode = code, ZZ1_ZZI_TariffType = tariffType.ZZI_PK, RVC_LastUpdatedUTC = dateTime, RVC_ParentPK = tariffPK, RVC_DataSetId = DataSetId });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = tariffType.ZZI_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = tariffPK, RVC_ParentCode = "ZZ1", RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = tariff.RVC_LastUpdatedUTC, RVC_IsPublished = true });
			return tariff;
		}

		IEnumerable<Models.RefCusTariff> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusTariff> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null, int chunkSize = 1)
		{
			var service = new RefCusTariffService(repo, new RefCusApplicabilityService(repo), new RefCusConditionService(repo), new RefCusTariffBRCharacteristicService(repo));
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, chunkSize, DataSetId).OrderBy(x => x.ZZ1_TariffCode);
		}

		T CreateChild<T>(string parentProperty, Guid parentPk, string codeProperty = null, string code = null) where T : new()
		{
			return Helper.CreateChild<T>(repo, parentProperty, parentPk, codeProperty, code);
		}

		DateTime Now;
		ObjectReferenceDataRepository repo;
		RefCusTradeGroup tradeGroup;
		RefCusConditionType conditionType;
		RefCusConditionValueType valueType;
		RefCusPreference preference;
		RefCusRateCode rateCode;
		RefCusRateType rateType;
		RefLanguageType languageType;
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusTariff);

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
			preference = repo.Create(() => new RefCusPreference
			{
				ZZS_PK = Guid.NewGuid(),
				ZZS_Description = "A"
			});
			rateType = repo.Create(() => new RefCusRateType
			{
				ZZR_PK = Guid.NewGuid(),
				ZZR_Description = "A"
			});
			rateCode = repo.Create(() => new RefCusRateCode
			{
				ZY1_PK = Guid.NewGuid(),
				ZY1_Description = "A",
				ZY1_ZZR_RateType = rateType.ZZR_PK
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
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = preference.ZZS_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = rateType.ZZR_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = languageType.ZX6_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
		}
	}
}
