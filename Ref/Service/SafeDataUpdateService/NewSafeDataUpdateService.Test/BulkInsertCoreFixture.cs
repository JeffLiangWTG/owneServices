using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	[CreateDatabase("8D9A7098D2FA48E790BBFB0121D588A8", DbSchema.RefDbRepoSafe, ActionTargets.Test)]
	class BulkInsertCoreFixture
	{
		[Test]
		public void BulkInsertThrowException()
		{
			using (var entities = new SafeDbContext(_connectionString))
			{
				var data = new List<Dummy>
			{
				new Dummy() { XXX_PK = Guid.NewGuid(), XXX_StringProperty = "XXX" }
			};

				var bulkInsertCore = new BulkInsertCoreForTest();
				Assert.ThrowsAsync<InvalidOperationException>(async () =>
				{
					await bulkInsertCore.BulkInsertTest<Dummy>(data, entities);
				}).Message.Contains("Cannot access destination table 'Dummy'");
			}
		}

		[Test]
		public async Task SaveChangeWithBulkInsertCoreDeletesRecordsFlaggedAsDeletesOnRefDbVersionControl()
		{
			var toBeDeletedShippingLinePk = Guid.NewGuid();
			var toBeDeletedShippingLineMessagingRequirementPk = Guid.NewGuid();
			using (var entities = new SafeDbContext(_connectionString))
			{
				entities.RefShippingLineMessagingRequirementTypes.Add(new RefShippingLineMessagingRequirementType
				{
					RST_Code = "AAA",
					RST_Description = "Testing",
					RST_PK = Guid.NewGuid()
				});
				entities.RefShippingLines.Add(new RefShippingLine
				{
					RSL_PK = toBeDeletedShippingLinePk,
					RSL_CargoWiseOneCode = "CW1",
					RSL_CarrierName = "ADD",
					RSL_StandardCarrierAlphaCode = "ALPA",
					RSL_CargoSphereRatesAvailable = false,
					RSL_ContainerAutomationAvailable = false,
					RSL_IsActive = true,
					RSL_OceanCarrierMessagingAvailable = true,
					RSL_EHubIds = "ids",
					RSL_GlobalSailingScheduleAvailable = false,
					RSL_InvoiceAvailable = false,
					RSL_IsCW1User = true,
					RSL_IsNVO = true,
				});
				entities.RefShippingLineMessagingRequirements.Add(new RefShippingLineMessagingRequirement
				{
					RSR_PK = toBeDeletedShippingLineMessagingRequirementPk,
					RSR_IsBookingRequest = true,
					RSR_IsShippingInstruction = false,
					RSR_RSL_ShippingLine = toBeDeletedShippingLinePk,
					RSR_RST_NKType = "AAA"
				});
				await entities.SaveChangesAsync();
			}
			using (var entities = new SafeDbContext(_connectionString))
			{
				var refDbVersionControl = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == toBeDeletedShippingLinePk);
				refDbVersionControl.RVC_Deleted = true;
				await entities.SaveChangesAsync();
			}
			var newShippingLinePk = Guid.NewGuid();
			var addedShippingLineRecords = new List<RefShippingLine>
			{
				new RefShippingLine
				{
					RSL_PK = newShippingLinePk,
					RSL_CargoWiseOneCode = "CW1",
					RSL_CarrierName = "ADD",
					RSL_StandardCarrierAlphaCode = "ALPA",
					RSL_CargoSphereRatesAvailable = false,
					RSL_ContainerAutomationAvailable = false,
					RSL_IsActive = true,
					RSL_OceanCarrierMessagingAvailable = true,
					RSL_EHubIds = "ids",
					RSL_GlobalSailingScheduleAvailable = false,
					RSL_InvoiceAvailable = false,
					RSL_IsCW1User = true,
					RSL_IsNVO = true,
				}
			};
			using (var entities = new SafeDbContext(_connectionString))
			{
				var bulkInsert = new BulkInsertCore();
				var dbEntry = entities.Entry(addedShippingLineRecords.First());
				var dicEntriesState = new Dictionary<EntityEntry, EntityState>();
				dicEntriesState.Add(dbEntry, EntityState.Added);
				var modifiedEntries = new EntityEntry[] { dbEntry };

				Assert.That(async () => await bulkInsert.SaveChangeWithBulkInsertCore(modifiedEntries, dicEntriesState, entities), Throws.Nothing);
			}
			using (var entities = new SafeDbContext(_connectionString))
			{
				Assert.That(entities.RefShippingLines.Any(x => x.RSL_PK == toBeDeletedShippingLinePk), Is.False);
				Assert.That(entities.RefShippingLineMessagingRequirements.Any(x => x.RSR_PK == toBeDeletedShippingLineMessagingRequirementPk), Is.False);
				Assert.That(entities.RefShippingLines.Any(x => x.RSL_PK == newShippingLinePk), Is.True);
				Assert.That(entities.RefShippingLineMessagingRequirementTypes.Any(), Is.True);
			}
		}

		[Test]
		public async Task SaveChangeWithBulkInsertCoreDeletesRecordsFlaggedAsDeletesOnRefDbVersionControlThreeLevels()
		{
			var toBeDeletedTariffPk = Guid.NewGuid();
			var toBeDeletedRatePk = Guid.NewGuid();
			var toBeDeletedApplicabilityPk = Guid.NewGuid();
			var toBeDeletedConditionPk = Guid.NewGuid();

			var tariffStartDate = new DateTime(2022, 10, 10);
			var tariffEndDate = new DateTime(2022, 10, 10).AddDays(5);
			var tariffTypePk = Guid.NewGuid();
			using (var entities = new SafeDbContext(_connectionString))
			{
				var rateCodePk = Guid.NewGuid();
				var rateTypePk = Guid.NewGuid();
				var tradeGroupPk = Guid.NewGuid();
				var conditionTypePk = Guid.NewGuid();
				entities.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_DataGrouping = "EUN",
					ZZZ_Description = "Europe",
					ZZZ_PK = Guid.NewGuid(),
				});
				await entities.SaveChangesAsync();
				entities.RefCurrencies.Add(new RefCurrency
				{
					RX_PK = Guid.NewGuid(),
					RX_Code = "USD",
					RX_Desc = "Dollar",
					RX_IsActive = true,
					RX_ISOSubUnitRatio = 1,
					RX_SubUnitName = "cents",
					RX_SubUnitRatio = 2,
					RX_UnitName = "dollars",
					RX_Symbol = "$"
				});
				entities.RefCusNomenclatureGroupTypes.Add(new RefCusNomenclatureGroupType
				{
					ZZ9_PK = Guid.NewGuid(),
					ZZ9_Description = "DESC",
					ZZ9_GroupType = "GT"
				});
				entities.RefCusRateTypes.Add(new RefCusRateType
				{
					ZZR_PK = rateTypePk,
					ZZR_CustomsValueFormula = "A",
					ZZR_Description = "Desc",
					ZZR_IsPayable = true,
					ZZR_RateType = "RT",
					ZZR_ZZZ_NKDataGrouping = "EUN",
					ZZR_RX_NKFormulaCurrency = "USD"
				});
				entities.RefCusRateCodes.Add(new RefCusRateCode
				{
					ZY1_PK = rateCodePk,
					ZY1_Description = "RC",
					ZY1_InternalUse = true,
					ZY1_RateCode = "RC",
					ZY1_ZZR_RateType = rateTypePk,
				});
				entities.RefCusTariffTypes.Add(new RefCusTariffType
				{
					ZZI_PK = tariffTypePk,
					ZZI_Description = "TP",
					ZZI_TariffType = "TTP",
					ZZI_HasFormulaSpecificQuestions = true,
					ZZI_ZZZ_NKDataGrouping = "EUN",
					ZZI_ZZR_RateType = rateTypePk,
					ZZI_ZZ9_NKNomenclatureGroupType = "GT",
				});
				entities.RefCusTariffs.Add(new RefCusTariff
				{
					ZZ1_PK = toBeDeletedTariffPk,
					ZZ1_Description = "Tariff to be deleted",
					ZZ1_CompositeKeyOnZZ5 = "01.01",
					ZZ1_StartDate = tariffStartDate,
					ZZ1_EndDate = tariffEndDate,
					ZZ1_IAMUnique = 1,
					ZZ1_PublishedDate = tariffStartDate,
					ZZ1_TariffCode = "0101",
					ZZ1_ZZI_TariffType = tariffTypePk,
					ZZ1_ZZZ_NKDataGrouping = "EUN",
					ZZ1_ZZF_NKTaxOrFeeCode = ""
				});
				entities.RefCusRates.Add(new RefCusRate
				{
					ZZ2_DataSetCode = "ZZ1",
					ZZ2_DataSetPK = toBeDeletedTariffPk,
					ZZ2_EndDate = tariffEndDate,
					ZZ2_StartDate = tariffStartDate,
					ZZ2_PK = toBeDeletedRatePk,
					ZZ2_RateFormula = "Form",
					ZZ2_SelectorFormula = "Select",
					ZZ2_ZZ1_Tariff = toBeDeletedTariffPk,
					ZZ2_ZZZ_NKDataGrouping = "EUN",
					ZZ2_ZY1_RateCode = rateCodePk,
					ZZ2_RateFormulaDerivedFrom = "a",
					ZZ2_ZZS_Preference = null,
					ZZ2_ZZW_TariffNationalCode = null,
					ZZ2_RX_NKCurrencyOverride = ""
				});
				entities.RefCusTradeGroups.Add(new RefCusTradeGroup
				{
					ZZA_Description = "Desc",
					ZZA_EndDate = tariffEndDate,
					ZZA_PK = tradeGroupPk,
					ZZA_StartDate = tariffStartDate,
					ZZA_TradeGroup = "TG",
					ZZA_ZZZ_NKDataGrouping = "EUN"
				});
				entities.RefCusApplicabilities.Add(new RefCusApplicability
				{
					ZZT_AdditionalCode = "1",
					ZZT_DataSetCode = "ZZ1",
					ZZT_DataSetPK = toBeDeletedTariffPk,
					ZZT_OrderNumber = "1",
					ZZT_EndDate = tariffEndDate,
					ZZT_PK = toBeDeletedApplicabilityPk,
					ZZT_StartDate = tariffStartDate,
					ZZT_ZX1_Conditions = null,
					ZZT_ZZ2_Rate = toBeDeletedRatePk,
					ZZT_ZY2_AdditionalCode = null,
					ZZT_ZZA_SecondTradeGroup = null,
					ZZT_ZZA_TradeGroup = tradeGroupPk
				});
				entities.RefCusConditionTypes.Add(new RefCusConditionType
				{
					ZX2_PK = conditionTypePk,
					ZX2_ConditionClass = "CLASS",
					ZX2_ConditionType = "T",
					ZX2_Description = "DESc",
					ZX2_ZZZ_NKDataGrouping = "EUN"
				});
				entities.RefCusConditionCodes.Add(new RefCusConditionCode
				{
					ZY7_PK = Guid.NewGuid(),
					ZY7_ConditionCode = "AAA",
					ZY7_Description = "D",
					ZY7_ZZZ_NKDataGrouping = "EUN"
				});
				entities.RefCusConditions.Add(new RefCusCondition
				{
					ZX1_PK = toBeDeletedConditionPk,
					ZX1_Comment = "Condition A",
					ZX1_ConditionValueTrueMeansStop = true,
					ZX1_DataSetCode = "ZZ1",
					ZX1_DataSetPK = toBeDeletedTariffPk,
					ZX1_EndDate = tariffEndDate,
					ZX1_IsExport = false,
					ZX1_IsImport = true,
					ZX1_LogicalANDWithinGroup = 1,
					ZX1_Source = "EUN Source",
					ZX1_StartDate = tariffStartDate,
					ZX1_ZZ1_Tariff = toBeDeletedTariffPk,
					ZX1_ZZS_Preference = null,
					ZX1_ZZ5_Nomenclature = null,
					ZX1_ZZZ_NKDataGrouping = "EUN",
					ZX1_ZX2_ConditionType = conditionTypePk,
					ZX1_ZY7_NKConditionCode = "AAA",
					ZX1_AdditionalComment = "comment"
				});
				await entities.SaveChangesAsync();
			}
			using (var entities = new SafeDbContext(_connectionString))
			{
				var refDbVersionControl = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == toBeDeletedTariffPk);
				refDbVersionControl.RVC_Deleted = true;
				await entities.SaveChangesAsync();
			}
			var newTariffPkSameAsDeleted = Guid.NewGuid();
			var newTariffPkNotInDb = Guid.NewGuid();
			var addedTariffRecords = new List<RefCusTariff>
			{
				new RefCusTariff
				{
					ZZ1_PK = newTariffPkSameAsDeleted,
					ZZ1_Description = "new Tariff",
					ZZ1_CompositeKeyOnZZ5 = "01.01",
					ZZ1_StartDate = tariffStartDate,
					ZZ1_EndDate = tariffEndDate,
					ZZ1_IAMUnique = 1,
					ZZ1_PublishedDate = tariffStartDate,
					ZZ1_TariffCode = "0101",
					ZZ1_ZZI_TariffType = tariffTypePk,
					ZZ1_ZZZ_NKDataGrouping = "EUN",
					ZZ1_ZZF_NKTaxOrFeeCode = ""
				},
				new RefCusTariff
				{
					ZZ1_PK = newTariffPkNotInDb,
					ZZ1_Description = "new Tariff",
					ZZ1_CompositeKeyOnZZ5 = "02.02",
					ZZ1_StartDate = tariffStartDate.AddDays(2),
					ZZ1_EndDate = tariffEndDate.AddDays(2),
					ZZ1_IAMUnique = 1,
					ZZ1_PublishedDate = tariffStartDate.AddDays(2),
					ZZ1_TariffCode = "0202",
					ZZ1_ZZI_TariffType = tariffTypePk,
					ZZ1_ZZZ_NKDataGrouping = "EUN",
					ZZ1_ZZF_NKTaxOrFeeCode = ""
				}
			};
			using (var entities = new SafeDbContext(_connectionString))
			{
				var bulkInsert = new BulkInsertCore();
				var dicEntriesState = new Dictionary<EntityEntry, EntityState>();
				addedTariffRecords.ForEach(x =>
				{
					var dbEntry = entities.Entry(x);
					dicEntriesState.Add(dbEntry, EntityState.Added);
				});
				var modifiedEntries = dicEntriesState.Keys.ToArray();

				Assert.That(async () => await bulkInsert.SaveChangeWithBulkInsertCore(modifiedEntries, dicEntriesState, entities), Throws.Nothing);
			}
			using (var entities = new SafeDbContext(_connectionString))
			{
				Assert.That(entities.RefCusTariffs.Any(x => x.ZZ1_PK == toBeDeletedTariffPk), Is.False);
				Assert.That(entities.RefCusRates.Any(x => x.ZZ2_PK == toBeDeletedRatePk), Is.False);
				Assert.That(entities.RefCusConditions.Any(x => x.ZX1_PK == toBeDeletedConditionPk), Is.False);
				Assert.That(entities.RefCusApplicabilities.Any(x => x.ZZT_PK == toBeDeletedApplicabilityPk), Is.False);

				Assert.That(entities.RefCusTariffs.Any(x => x.ZZ1_PK == newTariffPkNotInDb), Is.True);
				Assert.That(entities.RefCusTariffs.Any(x => x.ZZ1_PK == newTariffPkSameAsDeleted), Is.True);
			}
		}

		[Test]
		public async Task SaveChangeWithBulkInsertCoreDeletesRecordsFlaggedAsDeletesRefCusTaxOrFeeTypeNKRelationship()
		{
			var toBeDeletedTaxOrFeeType = Guid.NewGuid();
			using (var entities = new SafeDbContext(_connectionString))
			{
				//dataset: "RefCusTaxOrFeeType", "RefCusTaxOrFee", "RefCusTaxOrFeeLanguage"
				var taxOrFeePk = Guid.NewGuid();
				entities.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_DataGrouping = "EUN",
					ZZZ_Description = "Europe",
					ZZZ_PK = Guid.NewGuid(),
				});
				entities.RefLanguageTypes.Add(new RefLanguageType
				{
					ZX6_PK = Guid.NewGuid(),
					ZX6_Description = "English",
					ZX6_Language = "EN"
				});
				entities.RefCusTaxOrFeeTypes.Add(new RefCusTaxOrFeeType
				{
					ZX0_PK = toBeDeletedTaxOrFeeType,
					ZX0_Description = "TP1",
					ZX0_TaxOrFeeType = "TP"
				});
				await entities.SaveChangesAsync();
				entities.RefCusTaxOrFees.Add(new RefCusTaxOrFee
				{
					ZZF_PK = taxOrFeePk,
					ZZF_Code = "CD",
					ZZF_ZX0_NKTaxOrFeeType = "TP",
					ZZF_Description = "Desc",
					ZZF_EndDate = DateTime.UtcNow.AddDays(1),
					ZZF_Maximum = 1,
					ZZF_Minimum = 1,
					ZZF_StartDate = DateTime.UtcNow,
					ZZF_Threshold = 0,
					ZZF_Value = 10,
					ZZF_ZZZ_NKDataGrouping = "EUN"
				});
				entities.RefCusTaxOrFeeLanguages.Add(new RefCusTaxOrFeeLanguage
				{
					ZXU_PK = Guid.NewGuid(),
					ZXU_Description = "Desc",
					ZXU_ZZF_TaxOrFee = taxOrFeePk,
					ZXU_ZX6_NKLanguage = "EN"
				});
				await entities.SaveChangesAsync();
			}
			using (var entities = new SafeDbContext(_connectionString))
			{
				var refDbVersionControl = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == toBeDeletedTaxOrFeeType);
				refDbVersionControl.RVC_Deleted = true;
				await entities.SaveChangesAsync();
			}
			var newTaxOrFeeType = Guid.NewGuid();
			var addedTaxOrFeeType = new List<RefCusTaxOrFeeType>
			{
				new RefCusTaxOrFeeType
				{
					ZX0_PK = newTaxOrFeeType,
					ZX0_Description = "Desc2",
					ZX0_TaxOrFeeType = "TP"
				}
			};
			using (var entities = new SafeDbContext(_connectionString))
			{
				var bulkInsert = new BulkInsertCore();
				var dbEntry = entities.Entry(addedTaxOrFeeType.First());
				var dicEntriesState = new Dictionary<EntityEntry, EntityState>();
				dicEntriesState.Add(dbEntry, EntityState.Added);
				var modifiedEntries = new EntityEntry[] { dbEntry };

				Assert.That(async () => await bulkInsert.SaveChangeWithBulkInsertCore(modifiedEntries, dicEntriesState, entities), Throws.Nothing);
			}
			using (var entities = new SafeDbContext(_connectionString))
			{
				Assert.That(entities.RefCusTaxOrFeeTypes.Any(x => x.ZX0_PK == toBeDeletedTaxOrFeeType), Is.False);
				Assert.That(entities.RefCusTaxOrFeeTypes.Any(x => x.ZX0_PK == newTaxOrFeeType), Is.True);
			}
		}

		[Test]
		public async Task SaveChangeWithBulkInsertCoreDeletesRecordsFlaggedAsDeletesRefCusRateTypeEdgeCase()
		{
			var toBeDeletedRateType = Guid.NewGuid();
			using (var entities = new SafeDbContext(_connectionString))
			{
				//dataset: "RefCusRateType", "RefCusRateCode", "RefCusRateCodeLanguage", "RefCusRateTypeLanguage"
				var rateCodePk = Guid.NewGuid();
				entities.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_DataGrouping = "EUN",
					ZZZ_Description = "Europe",
					ZZZ_PK = Guid.NewGuid(),
				});
				entities.RefLanguageTypes.Add(new RefLanguageType
				{
					ZX6_PK = Guid.NewGuid(),
					ZX6_Description = "English",
					ZX6_Language = "EN"
				});
				await entities.SaveChangesAsync();
				entities.RefCusRateTypes.Add(new RefCusRateType
				{
					ZZR_PK = toBeDeletedRateType,
					ZZR_Description = "TP1",
					ZZR_RateType = "TP",
					ZZR_CustomsValueFormula = "",
					ZZR_IsPayable = true,
					ZZR_RX_NKFormulaCurrency = "",
					ZZR_ZZZ_NKDataGrouping = "EUN"
				});
				entities.RefCusRateCodes.Add(new RefCusRateCode
				{
					ZY1_PK = rateCodePk,
					ZY1_RateCode = "CD",
					ZY1_Description = "TP",
					ZY1_ZZR_RateType = toBeDeletedRateType,
					ZY1_InternalUse = true
				});
				entities.RefCusRateCodeLanguages.Add(new RefCusRateCodeLanguage
				{
					ZXC_PK = Guid.NewGuid(),
					ZXC_Description = "Desc",
					ZXC_ZY1_RateCode = rateCodePk,
					ZXC_ZX6_NKLanguage = "EN"
				});
				entities.RefCusRateTypeLanguages.Add(new RefCusRateTypeLanguage
				{
					ZXT_PK = Guid.NewGuid(),
					ZXT_Description = "Desc",
					ZXT_ZZR_RateType = toBeDeletedRateType,
					ZXT_ZX6_NKLanguage = "EN"
				});
				await entities.SaveChangesAsync();
			}
			using (var entities = new SafeDbContext(_connectionString))
			{
				var refDbVersionControl = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == toBeDeletedRateType);
				refDbVersionControl.RVC_Deleted = true;
				await entities.SaveChangesAsync();
			}
			var newRateType = Guid.NewGuid();
			var addedRateType = new List<RefCusRateType>
			{
				new RefCusRateType
				{
					ZZR_PK = newRateType,
					ZZR_Description = "TP2",
					ZZR_RateType = "TP",
					ZZR_CustomsValueFormula = "",
					ZZR_IsPayable = true,
					ZZR_RX_NKFormulaCurrency = "",
					ZZR_ZZZ_NKDataGrouping = "EUN"
				}
			};
			using (var entities = new SafeDbContext(_connectionString))
			{
				var bulkInsert = new BulkInsertCore();
				var dbEntry = entities.Entry(addedRateType.First());
				var dicEntriesState = new Dictionary<EntityEntry, EntityState>();
				dicEntriesState.Add(dbEntry, EntityState.Added);
				var modifiedEntries = new EntityEntry[] { dbEntry };

				Assert.That(async () => await bulkInsert.SaveChangeWithBulkInsertCore(modifiedEntries, dicEntriesState, entities), Throws.Nothing);
			}
			using (var entities = new SafeDbContext(_connectionString))
			{
				Assert.That(entities.RefCusRateTypes.Any(x => x.ZZR_PK == toBeDeletedRateType), Is.False);
				Assert.That(entities.RefCusRateTypes.Any(x => x.ZZR_PK == newRateType), Is.True);
			}
		}

		[Test]
		public async Task SaveChangeWithBulkInsertCoreThrowsExceptionWhenDeleteMoreThan3Duplicates()
		{
			var toBeDeletedRecords = new List<RefCusCodeType>();
			for (var i = 0; i <= 3; i++)
			{
				var codeType = new RefCusCodeType
				{
					ZZK_PK = Guid.NewGuid(),
					ZZK_CodeType = $"v{i}",
					ZZK_ZZZ_NKDataGrouping = "ZA",
					ZZK_Description = $"Description of V{i}"
				};
				toBeDeletedRecords.Add(codeType);
			}
			using (var entities = new SafeDbContext(_connectionString))
			{
				entities.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_PK = Guid.NewGuid(),
					ZZZ_DataGrouping = "ZA",
					ZZZ_Description = "South Africa"
				});
				await entities.SaveChangesAsync();
				entities.RefCusCodeTypes.AddRange(toBeDeletedRecords);
				await entities.SaveChangesAsync();
			}
			using (var entities = new SafeDbContext(_connectionString))
			{
				var codeTypePks = toBeDeletedRecords.Select(x => x.ZZK_PK).ToArray();
				var refDbVersionControls = entities.RefDbVersionControls.Where(x => codeTypePks.Contains(x.RVC_ParentPK));
				foreach (var refDbVersionControl in refDbVersionControls)
				{
					refDbVersionControl.RVC_Deleted = true;
				}
				await entities.SaveChangesAsync();
			}
			var addedRecords = toBeDeletedRecords;
			addedRecords.ForEach(x => x.ZZK_PK = Guid.NewGuid());
			addedRecords.ForEach(x => x.ZZK_CodeType = x.ZZK_CodeType.ToUpperInvariant());
			using (var entities = new SafeDbContext(_connectionString))
			{
				var bulkInsert = new BulkInsertCore();
				var dicEntriesState = new Dictionary<EntityEntry, EntityState>();
				var modifiedEntries = new List<EntityEntry>();
				foreach (var record in addedRecords)
				{
					var dbEntry = entities.Entry(record);
					dicEntriesState.Add(dbEntry, EntityState.Added);
					modifiedEntries.Add(dbEntry);
				}
				Assert.That(async () => await bulkInsert.SaveChangeWithBulkInsertCore(modifiedEntries.ToArray(), dicEntriesState, entities), Throws.Exception.TypeOf<SqlException>().With.Message.Contains("The duplicate key value is"));
			}
		}

		[Test]
		public async Task SaveChangeWithBulkInsertCoreWhenDeleteRateWithApplicability()
		{
			var tariffPk = Guid.NewGuid();
			var ratePk = Guid.NewGuid();
			var appPk = Guid.NewGuid();

			var tariffStartDate = new DateTime(2022, 10, 10);
			var tariffEndDate = new DateTime(2022, 10, 10).AddDays(5);
			var tariffTypePk = Guid.NewGuid();
			using (var entities = new SafeDbContext(_connectionString))
			{
				var rateCodePk = Guid.NewGuid();
				var rateTypePk = Guid.NewGuid();
				var tradeGroupPk = Guid.NewGuid();
				entities.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_DataGrouping = "EUN",
					ZZZ_Description = "Europe",
					ZZZ_PK = Guid.NewGuid(),
				});
				await entities.SaveChangesAsync();
				entities.RefCusRateTypes.Add(new RefCusRateType
				{
					ZZR_PK = rateTypePk,
					ZZR_CustomsValueFormula = "A",
					ZZR_Description = "Desc",
					ZZR_IsPayable = true,
					ZZR_RateType = "RT",
					ZZR_ZZZ_NKDataGrouping = "EUN",
					ZZR_RX_NKFormulaCurrency = "USD"
				});
				entities.RefCusRateCodes.Add(new RefCusRateCode
				{
					ZY1_PK = rateCodePk,
					ZY1_Description = "RC",
					ZY1_InternalUse = true,
					ZY1_RateCode = "RC",
					ZY1_ZZR_RateType = rateTypePk,
				});
				entities.RefCusTariffTypes.Add(new RefCusTariffType
				{
					ZZI_PK = tariffTypePk,
					ZZI_Description = "TP",
					ZZI_TariffType = "TTP",
					ZZI_HasFormulaSpecificQuestions = true,
					ZZI_ZZZ_NKDataGrouping = "EUN",
					ZZI_ZZR_RateType = rateTypePk,
					ZZI_ZZ9_NKNomenclatureGroupType = "GT",
				});
				entities.RefCusTariffs.Add(new RefCusTariff
				{
					ZZ1_PK = tariffPk,
					ZZ1_Description = "Tariff to be deleted",
					ZZ1_CompositeKeyOnZZ5 = "01.01",
					ZZ1_StartDate = tariffStartDate,
					ZZ1_EndDate = tariffEndDate,
					ZZ1_IAMUnique = 1,
					ZZ1_PublishedDate = tariffStartDate,
					ZZ1_TariffCode = "0101",
					ZZ1_ZZI_TariffType = tariffTypePk,
					ZZ1_ZZZ_NKDataGrouping = "EUN",
					ZZ1_ZZF_NKTaxOrFeeCode = ""
				});
				entities.RefCusRates.Add(new RefCusRate
				{
					ZZ2_DataSetCode = "ZZ1",
					ZZ2_DataSetPK = tariffPk,
					ZZ2_EndDate = tariffEndDate,
					ZZ2_StartDate = tariffStartDate,
					ZZ2_PK = ratePk,
					ZZ2_RateFormula = "Form",
					ZZ2_SelectorFormula = "Select",
					ZZ2_ZZ1_Tariff = tariffPk,
					ZZ2_ZZZ_NKDataGrouping = "EUN",
					ZZ2_ZY1_RateCode = rateCodePk,
					ZZ2_RateFormulaDerivedFrom = "a",
					ZZ2_ZZS_Preference = null,
					ZZ2_ZZW_TariffNationalCode = null,
					ZZ2_RX_NKCurrencyOverride = ""
				});
				entities.RefCusTradeGroups.Add(new RefCusTradeGroup
				{
					ZZA_Description = "Desc",
					ZZA_EndDate = tariffEndDate,
					ZZA_PK = tradeGroupPk,
					ZZA_StartDate = tariffStartDate,
					ZZA_TradeGroup = "TG",
					ZZA_ZZZ_NKDataGrouping = "EUN"
				});
				entities.RefCusApplicabilities.Add(new RefCusApplicability
				{
					ZZT_AdditionalCode = "1",
					ZZT_DataSetCode = "ZZ1",
					ZZT_DataSetPK = tariffPk,
					ZZT_OrderNumber = "1",
					ZZT_EndDate = tariffEndDate,
					ZZT_PK = appPk,
					ZZT_StartDate = tariffStartDate,
					ZZT_ZX1_Conditions = null,
					ZZT_ZZ2_Rate = ratePk,
					ZZT_ZY2_AdditionalCode = null,
					ZZT_ZZA_SecondTradeGroup = null,
					ZZT_ZZA_TradeGroup = tradeGroupPk
				});
				await entities.SaveChangesAsync();
			}

			var ratesToBeDelted = new List<RefCusRate>
			{
				new RefCusRate
				{
					ZZ2_DataSetCode = "ZZ1",
					ZZ2_DataSetPK = tariffPk,
					ZZ2_EndDate = tariffEndDate,
					ZZ2_StartDate = tariffStartDate,
					ZZ2_PK = ratePk,
					ZZ2_RateFormula = "Form",
					ZZ2_SelectorFormula = "Select",
					ZZ2_ZZ1_Tariff = tariffPk,
					ZZ2_ZZZ_NKDataGrouping = "EUN",
					ZZ2_RateFormulaDerivedFrom = "a",
					ZZ2_ZZS_Preference = null,
					ZZ2_ZZW_TariffNationalCode = null,
					ZZ2_RX_NKCurrencyOverride = ""
				}
			};
			var appsToBeDeleted = new List<RefCusApplicability>
			{
				new RefCusApplicability
				{
					ZZT_AdditionalCode = "1",
					ZZT_DataSetCode = "ZZ1",
					ZZT_DataSetPK = tariffPk,
					ZZT_OrderNumber = "1",
					ZZT_EndDate = tariffEndDate,
					ZZT_PK = appPk,
					ZZT_StartDate = tariffStartDate,
					ZZT_ZX1_Conditions = null,
					ZZT_ZZ2_Rate = ratePk,
					ZZT_ZY2_AdditionalCode = null,
					ZZT_ZZA_SecondTradeGroup = null,
				}
			};
			using (var entities = new SafeDbContext(_connectionString))
			{
				var bulkInsert = new BulkInsertCore();
				var dicEntriesState = new Dictionary<EntityEntry, EntityState>();
				appsToBeDeleted.ForEach(x =>
				{
					var dbEntry = entities.Entry(x);
					dicEntriesState.Add(dbEntry, EntityState.Deleted);
				});
				ratesToBeDelted.ForEach(x =>
				{
					var dbEntry = entities.Entry(x);
					dicEntriesState.Add(dbEntry, EntityState.Deleted);
				});
				var modifiedEntries = dicEntriesState.Keys.ToArray();

				Assert.That(async () => await bulkInsert.SaveChangeWithBulkInsertCore(modifiedEntries, dicEntriesState, entities), Throws.Nothing);
			}
			using (var entities = new SafeDbContext(_connectionString))
			{
				Assert.That(entities.RefCusTariffs.Any(x => x.ZZ1_PK == tariffPk), Is.True);
				Assert.That(entities.RefCusApplicabilities.Any(x => x.ZZT_PK == appPk), Is.False);
				Assert.That(entities.RefCusRates.Any(x => x.ZZ2_PK == ratePk), Is.False);
			}
		}

		[Test]
		public async Task SaveChangeWithBulkInsertCoreWhenAppIsUpdatedAndRateIsDeleted()
		{
			var tariffPk = Guid.NewGuid();
			var ratePk1 = Guid.NewGuid();
			var ratePk2 = Guid.NewGuid();
			var appPk = Guid.NewGuid();

			var tariffStartDate = new DateTime(2022, 10, 10);
			var tariffEndDate = new DateTime(2022, 10, 10).AddDays(5);
			var tariffTypePk = Guid.NewGuid();
			using (var entities = new SafeDbContext(_connectionString))
			{
				var rateCodePk = Guid.NewGuid();
				var rateTypePk = Guid.NewGuid();
				var tradeGroupPk = Guid.NewGuid();
				entities.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_DataGrouping = "EUN",
					ZZZ_Description = "Europe",
					ZZZ_PK = Guid.NewGuid(),
				});
				await entities.SaveChangesAsync();
				entities.RefCusRateTypes.Add(new RefCusRateType
				{
					ZZR_PK = rateTypePk,
					ZZR_CustomsValueFormula = "A",
					ZZR_Description = "Desc",
					ZZR_IsPayable = true,
					ZZR_RateType = "RT",
					ZZR_ZZZ_NKDataGrouping = "EUN",
					ZZR_RX_NKFormulaCurrency = "USD"
				});
				entities.RefCusRateCodes.Add(new RefCusRateCode
				{
					ZY1_PK = rateCodePk,
					ZY1_Description = "RC",
					ZY1_InternalUse = true,
					ZY1_RateCode = "RC",
					ZY1_ZZR_RateType = rateTypePk,
				});
				entities.RefCusTariffTypes.Add(new RefCusTariffType
				{
					ZZI_PK = tariffTypePk,
					ZZI_Description = "TP",
					ZZI_TariffType = "TTP",
					ZZI_HasFormulaSpecificQuestions = true,
					ZZI_ZZZ_NKDataGrouping = "EUN",
					ZZI_ZZR_RateType = rateTypePk,
					ZZI_ZZ9_NKNomenclatureGroupType = "GT",
				});
				entities.RefCusTariffs.Add(new RefCusTariff
				{
					ZZ1_PK = tariffPk,
					ZZ1_Description = "Tariff to be deleted",
					ZZ1_CompositeKeyOnZZ5 = "01.01",
					ZZ1_StartDate = tariffStartDate,
					ZZ1_EndDate = tariffEndDate,
					ZZ1_IAMUnique = 1,
					ZZ1_PublishedDate = tariffStartDate,
					ZZ1_TariffCode = "0101",
					ZZ1_ZZI_TariffType = tariffTypePk,
					ZZ1_ZZZ_NKDataGrouping = "EUN",
					ZZ1_ZZF_NKTaxOrFeeCode = ""
				});
				entities.RefCusRates.Add(new RefCusRate
				{
					ZZ2_DataSetCode = "ZZ1",
					ZZ2_DataSetPK = tariffPk,
					ZZ2_EndDate = tariffEndDate,
					ZZ2_StartDate = tariffStartDate,
					ZZ2_PK = ratePk1,
					ZZ2_RateFormula = "Form",
					ZZ2_SelectorFormula = "Select",
					ZZ2_ZZ1_Tariff = tariffPk,
					ZZ2_ZZZ_NKDataGrouping = "EUN",
					ZZ2_ZY1_RateCode = rateCodePk,
					ZZ2_RateFormulaDerivedFrom = "a",
					ZZ2_ZZS_Preference = null,
					ZZ2_ZZW_TariffNationalCode = null,
					ZZ2_RX_NKCurrencyOverride = ""
				});
				entities.RefCusRates.Add(new RefCusRate
				{
					ZZ2_DataSetCode = "ZZ1",
					ZZ2_DataSetPK = tariffPk,
					ZZ2_EndDate = tariffEndDate,
					ZZ2_StartDate = tariffStartDate,
					ZZ2_PK = ratePk2,
					ZZ2_RateFormula = "Form",
					ZZ2_SelectorFormula = "Select",
					ZZ2_ZZ1_Tariff = tariffPk,
					ZZ2_ZZZ_NKDataGrouping = "EUN",
					ZZ2_ZY1_RateCode = rateCodePk,
					ZZ2_RateFormulaDerivedFrom = "a",
					ZZ2_ZZS_Preference = null,
					ZZ2_ZZW_TariffNationalCode = null,
					ZZ2_RX_NKCurrencyOverride = ""
				});
				entities.RefCusTradeGroups.Add(new RefCusTradeGroup
				{
					ZZA_Description = "Desc",
					ZZA_EndDate = tariffEndDate,
					ZZA_PK = tradeGroupPk,
					ZZA_StartDate = tariffStartDate,
					ZZA_TradeGroup = "TG",
					ZZA_ZZZ_NKDataGrouping = "EUN"
				});
				entities.RefCusApplicabilities.Add(new RefCusApplicability
				{
					ZZT_AdditionalCode = "1",
					ZZT_DataSetCode = "ZZ1",
					ZZT_DataSetPK = tariffPk,
					ZZT_OrderNumber = "1",
					ZZT_EndDate = tariffEndDate,
					ZZT_PK = appPk,
					ZZT_StartDate = tariffStartDate,
					ZZT_ZX1_Conditions = null,
					ZZT_ZZ2_Rate = ratePk1,
					ZZT_ZY2_AdditionalCode = null,
					ZZT_ZZA_SecondTradeGroup = null,
					ZZT_ZZA_TradeGroup = tradeGroupPk
				});
				await entities.SaveChangesAsync();
			}

			var ratesToBeDelted = new List<RefCusRate>
			{
				new RefCusRate
				{
					ZZ2_DataSetCode = "ZZ1",
					ZZ2_DataSetPK = tariffPk,
					ZZ2_EndDate = tariffEndDate,
					ZZ2_StartDate = tariffStartDate,
					ZZ2_PK = ratePk1,
					ZZ2_RateFormula = "Form",
					ZZ2_SelectorFormula = "Select",
					ZZ2_ZZ1_Tariff = tariffPk,
					ZZ2_ZZZ_NKDataGrouping = "EUN",
					ZZ2_RateFormulaDerivedFrom = "a",
					ZZ2_ZZS_Preference = null,
					ZZ2_ZZW_TariffNationalCode = null,
					ZZ2_RX_NKCurrencyOverride = ""
				}
			};
			var appsToBeUpdated = new List<RefCusApplicability>
			{
				new RefCusApplicability
				{
					ZZT_AdditionalCode = "1",
					ZZT_DataSetCode = "ZZ1",
					ZZT_DataSetPK = tariffPk,
					ZZT_OrderNumber = "1",
					ZZT_EndDate = tariffEndDate,
					ZZT_PK = appPk,
					ZZT_StartDate = tariffStartDate,
					ZZT_ZX1_Conditions = null,
					ZZT_ZZ2_Rate = ratePk2,
					ZZT_ZY2_AdditionalCode = null,
					ZZT_ZZA_SecondTradeGroup = null,
				}
			};
			using (var entities = new SafeDbContext(_connectionString))
			{
				var bulkInsert = new BulkInsertCore();
				var dicEntriesState = new Dictionary<EntityEntry, EntityState>();
				appsToBeUpdated.ForEach(x =>
				{
					var dbEntry = entities.Entry(x);
					dicEntriesState.Add(dbEntry, EntityState.Modified);
				});
				ratesToBeDelted.ForEach(x =>
				{
					var dbEntry = entities.Entry(x);
					dicEntriesState.Add(dbEntry, EntityState.Deleted);
				});
				var modifiedEntries = dicEntriesState.Keys.ToArray();

				Assert.That(async () => await bulkInsert.SaveChangeWithBulkInsertCore(modifiedEntries, dicEntriesState, entities), Throws.Nothing);
			}
			using (var entities = new SafeDbContext(_connectionString))
			{
				Assert.That(entities.RefCusTariffs.Any(x => x.ZZ1_PK == tariffPk), Is.True);
				Assert.That(entities.RefCusApplicabilities.Any(x => x.ZZT_PK == appPk), Is.True);
				Assert.That(entities.RefCusRates.Any(x => x.ZZ2_PK == ratePk1), Is.False);
			}
		}

		[Test]
		public async Task SaveChangeWithBulkInsertCore_RefCusRateCode()
		{
			var rateTypePK = Guid.NewGuid();
			var dataGrouping = new RefDataGrouping { ZZZ_PK = Guid.NewGuid(), ZZZ_DataGrouping = "IE", ZZZ_Description = "group" };
			var rateType = new RefCusRateType { ZZR_PK = rateTypePK, ZZR_RateType = "IMP", ZZR_Description = "Import", ZZR_IsPayable = true, ZZR_ZZZ_NKDataGrouping = "IE", ZZR_RX_NKFormulaCurrency = "", ZZR_CustomsValueFormula = "", ZZR_IsExport = false };
			var rateCodes = new RefCusRateCode[]
			{
				new RefCusRateCode { ZY1_PK = Guid.NewGuid(), ZY1_RateCode = "1A1", ZY1_ZZR_RateType = rateTypePK, ZY1_Description = "desc 1", ZY1_InternalUse = false, ZY1_ZZZ_NKDataGrouping = "IE" },
				new RefCusRateCode { ZY1_PK = Guid.NewGuid(), ZY1_RateCode = "1B1", ZY1_ZZR_RateType = rateTypePK, ZY1_Description = "desc 2", ZY1_InternalUse = false, ZY1_ZZZ_NKDataGrouping = "IE" }
			};
			using (var entities = new SafeDbContext(_connectionString))
			{
				entities.RefDataGroupings.Add(dataGrouping);
				await entities.SaveChangesAsync();
				entities.RefCusRateTypes.AddRange(rateType);
				await entities.SaveChangesAsync();
			}

			using (var entities = new SafeDbContext(_connectionString))
			{
				var bulkInsert = new BulkInsertCore();
				var dicEntriesState = new Dictionary<EntityEntry, EntityState>();
				var modifiedEntries = new List<EntityEntry>();
				foreach (var rateCode in rateCodes)
				{
					var dbEntry = entities.Entry(rateCode);
					dicEntriesState.Add(dbEntry, EntityState.Added);
					modifiedEntries.Add(dbEntry);
				}
				Assert.DoesNotThrowAsync(async () => await bulkInsert.SaveChangeWithBulkInsertCore(modifiedEntries.ToArray(), dicEntriesState, entities));
			}
		}

		[Test]
		public async Task SaveChangesDoesNotThrowDeadlock_RefCusProfileQuestion()
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				var command = connection.CreateCommand();
				command.CommandText = "DISABLE TRIGGER ALL ON [dbo].[RefCusProfileQuestion]";
				command.ExecuteNonQuery();
			}

			var profileTypePK = Guid.NewGuid();
			await CreateRefCusProfileType(profileTypePK);

			using (var entities1 = new SafeDbContext(_connectionString))
			using (var entities2 = new SafeDbContext(_connectionString))
			{
				var dicEntriesState1 = new Dictionary<EntityEntry, EntityState>();
				var dicEntriesState2 = new Dictionary<EntityEntry, EntityState>();
				var modifiedEntries1 = new List<EntityEntry>();
				var modifiedEntries2 = new List<EntityEntry>();
				for (var i = 0; i < 20; i++)
				{
					var code = $"ATT_{i}";
					var questionPK = Guid.NewGuid();
					var question = CreateRefCusProfileQuestion(profileTypePK, questionPK, code);
					var questionAttribute = new RefCusProfileQuestionAttribute
					{
						XQ3_PK = Guid.NewGuid(),
						XQ3_Name = code,
						XQ3_Value = code,
						XQ3_XQ2_Question = questionPK
					};

					if (i < 10)
					{
						var questionEntry = entities1.Entry(question);
						var questionAttributeEntry = entities1.Entry(questionAttribute);
						dicEntriesState1.Add(questionEntry, EntityState.Added);
						dicEntriesState1.Add(questionAttributeEntry, EntityState.Added);
						modifiedEntries1.Add(questionEntry);
						modifiedEntries1.Add(questionAttributeEntry);
					}
					else
					{
						var questionEntry = entities2.Entry(question);
						var questionAttributeEntry = entities2.Entry(questionAttribute);
						dicEntriesState2.Add(questionEntry, EntityState.Added);
						dicEntriesState2.Add(questionAttributeEntry, EntityState.Added);
						modifiedEntries2.Add(questionEntry);
						modifiedEntries2.Add(questionAttributeEntry);
					}
				}

				var bulkInsert1 = new BulkInsertCore();
				var bulkInsert2 = new BulkInsertCore();
				Assert.DoesNotThrow(() =>
				{
					var task1 = Task.Run(() => bulkInsert1.SaveChangeWithBulkInsertCore(modifiedEntries1.ToArray(), dicEntriesState1, entities1));
					var task2 = Task.Run(() => bulkInsert2.SaveChangeWithBulkInsertCore(modifiedEntries2.ToArray(), dicEntriesState2, entities2));
					Task.WaitAll(task1, task2);
				});
			}

			using (var entities = new SafeDbContext(_connectionString))
			{
				var questions = entities.Set<RefCusProfileQuestion>().Where(x => x.XQ2_XXX_ProfileType == profileTypePK);
				var questionAttributes = entities.Set<RefCusProfileQuestionAttribute>().Where(x => questions.Select(y => y.XQ2_PK).Contains(x.XQ3_XQ2_Question));
				Assert.AreEqual(20, questions.Count());
				Assert.AreEqual(20, questionAttributes.Count());
			}
		}

		[Test]
		public async Task SaveChangesWithBulkInsertCore_DuplicatedOne_ModifyAndDelete()
		{
			var tariffPk = Guid.NewGuid();
			var ratePk = Guid.NewGuid();

			var tariffStartDate = new DateTime(2022, 10, 10);
			var tariffEndDate = new DateTime(2022, 10, 10).AddDays(5);
			var tariffTypePk = Guid.NewGuid();
			var rateCodePk = Guid.NewGuid();
			var rateTypePk = Guid.NewGuid();
			var tradeGroupPk = Guid.NewGuid();
			var duplicatedRate = new RefCusRate
			{
				ZZ2_DataSetCode = "ZZ1",
				ZZ2_DataSetPK = tariffPk,
				ZZ2_EndDate = tariffEndDate,
				ZZ2_StartDate = tariffStartDate,
				ZZ2_PK = ratePk,
				ZZ2_RateFormula = "Form",
				ZZ2_SelectorFormula = "Select",
				ZZ2_ZZ1_Tariff = tariffPk,
				ZZ2_ZZZ_NKDataGrouping = "EUN",
				ZZ2_ZY1_RateCode = rateCodePk,
				ZZ2_RateFormulaDerivedFrom = "a",
				ZZ2_ZZS_Preference = null,
				ZZ2_ZZW_TariffNationalCode = null,
				ZZ2_RX_NKCurrencyOverride = ""
			};
			await using (var entities = new SafeDbContext(_connectionString))
			{
				entities.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_DataGrouping = "EUN",
					ZZZ_Description = "Europe",
					ZZZ_PK = Guid.NewGuid(),
				});
				await entities.SaveChangesAsync();
				entities.RefCusRateTypes.Add(new RefCusRateType
				{
					ZZR_PK = rateTypePk,
					ZZR_CustomsValueFormula = "A",
					ZZR_Description = "Desc",
					ZZR_IsPayable = true,
					ZZR_RateType = "RT",
					ZZR_ZZZ_NKDataGrouping = "EUN",
					ZZR_RX_NKFormulaCurrency = "USD"
				});
				entities.RefCusRateCodes.Add(new RefCusRateCode
				{
					ZY1_PK = rateCodePk,
					ZY1_Description = "RC",
					ZY1_InternalUse = true,
					ZY1_RateCode = "RC",
					ZY1_ZZR_RateType = rateTypePk,
				});
				entities.RefCusTariffTypes.Add(new RefCusTariffType
				{
					ZZI_PK = tariffTypePk,
					ZZI_Description = "TP",
					ZZI_TariffType = "TTP",
					ZZI_HasFormulaSpecificQuestions = true,
					ZZI_ZZZ_NKDataGrouping = "EUN",
					ZZI_ZZR_RateType = rateTypePk,
					ZZI_ZZ9_NKNomenclatureGroupType = "GT",
				});
				entities.RefCusTariffs.Add(new RefCusTariff
				{
					ZZ1_PK = tariffPk,
					ZZ1_Description = "Tariff to be deleted",
					ZZ1_CompositeKeyOnZZ5 = "01.01",
					ZZ1_StartDate = tariffStartDate,
					ZZ1_EndDate = tariffEndDate,
					ZZ1_IAMUnique = 1,
					ZZ1_PublishedDate = tariffStartDate,
					ZZ1_TariffCode = "0101",
					ZZ1_ZZI_TariffType = tariffTypePk,
					ZZ1_ZZZ_NKDataGrouping = "EUN",
					ZZ1_ZZF_NKTaxOrFeeCode = ""
				});
				entities.RefCusRates.Add(duplicatedRate);
				entities.RefCusTradeGroups.Add(new RefCusTradeGroup
				{
					ZZA_Description = "Desc",
					ZZA_EndDate = tariffEndDate,
					ZZA_PK = tradeGroupPk,
					ZZA_StartDate = tariffStartDate,
					ZZA_TradeGroup = "TG",
					ZZA_ZZZ_NKDataGrouping = "EUN"
				});
				await entities.SaveChangesAsync();
			}

			var modifiedRate = new RefCusRate
			{
				ZZ2_DataSetCode = "ZZ1",
				ZZ2_DataSetPK = tariffPk,
				ZZ2_EndDate = new DateTime(2023, 10, 15),
				ZZ2_StartDate = tariffStartDate,
				ZZ2_PK = ratePk,
				ZZ2_RateFormula = "Form",
				ZZ2_SelectorFormula = "Select",
				ZZ2_ZZ1_Tariff = tariffPk,
				ZZ2_ZZZ_NKDataGrouping = "EUN",
				ZZ2_RateFormulaDerivedFrom = "a",
				ZZ2_ZZS_Preference = null,
				ZZ2_ZZW_TariffNationalCode = null,
				ZZ2_RX_NKCurrencyOverride = ""
			};
			await using (var entities = new SafeDbContext(_connectionString))
			{
				var bulkInsert = new BulkInsertCore();
				var dbModifiedEntry = entities.Entry(modifiedRate);
				var dbDeletedEntry = entities.Entry(duplicatedRate);
				var dicEntriesState = new Dictionary<EntityEntry, EntityState>();
				dicEntriesState.Add(dbModifiedEntry, EntityState.Modified);
				dicEntriesState.Add(dbDeletedEntry, EntityState.Deleted);
				var entries = new EntityEntry[] { dbModifiedEntry, dbDeletedEntry };

				Assert.That(async () => await bulkInsert.SaveChangeWithBulkInsertCore(entries, dicEntriesState, entities), Throws.Nothing);
			}
			await using (var entities = new SafeDbContext(_connectionString))
			{
				Assert.That(entities.RefCusRates.Any(x => x.ZZ2_PK == ratePk), Is.True);
			}
		}

		[Test]
		public async Task SaveChangesWithBulkInsertCore_DuplicatedOne_InsertAndDelete()
		{
			var tariffPk = Guid.NewGuid();
			var ratePk = Guid.NewGuid();

			var tariffStartDate = new DateTime(2022, 10, 10);
			var tariffEndDate = new DateTime(2022, 10, 10).AddDays(5);
			var tariffTypePk = Guid.NewGuid();
			var rateCodePk = Guid.NewGuid();
			var rateTypePk = Guid.NewGuid();
			var tradeGroupPk = Guid.NewGuid();
			var duplicatedRate = new RefCusRate
			{
				ZZ2_DataSetCode = "ZZ1",
				ZZ2_DataSetPK = tariffPk,
				ZZ2_EndDate = tariffEndDate,
				ZZ2_StartDate = tariffStartDate,
				ZZ2_PK = ratePk,
				ZZ2_RateFormula = "Form",
				ZZ2_SelectorFormula = "Select",
				ZZ2_ZZ1_Tariff = tariffPk,
				ZZ2_ZZZ_NKDataGrouping = "EUN",
				ZZ2_ZY1_RateCode = rateCodePk,
				ZZ2_RateFormulaDerivedFrom = "a",
				ZZ2_ZZS_Preference = null,
				ZZ2_ZZW_TariffNationalCode = null,
				ZZ2_RX_NKCurrencyOverride = ""
			};
			await using (var entities = new SafeDbContext(_connectionString))
			{
				entities.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_DataGrouping = "EUN",
					ZZZ_Description = "Europe",
					ZZZ_PK = Guid.NewGuid(),
				});
				await entities.SaveChangesAsync();
				entities.RefCusRateTypes.Add(new RefCusRateType
				{
					ZZR_PK = rateTypePk,
					ZZR_CustomsValueFormula = "A",
					ZZR_Description = "Desc",
					ZZR_IsPayable = true,
					ZZR_RateType = "RT",
					ZZR_ZZZ_NKDataGrouping = "EUN",
					ZZR_RX_NKFormulaCurrency = "USD"
				});
				entities.RefCusRateCodes.Add(new RefCusRateCode
				{
					ZY1_PK = rateCodePk,
					ZY1_Description = "RC",
					ZY1_InternalUse = true,
					ZY1_RateCode = "RC",
					ZY1_ZZR_RateType = rateTypePk,
				});
				entities.RefCusTariffTypes.Add(new RefCusTariffType
				{
					ZZI_PK = tariffTypePk,
					ZZI_Description = "TP",
					ZZI_TariffType = "TTP",
					ZZI_HasFormulaSpecificQuestions = true,
					ZZI_ZZZ_NKDataGrouping = "EUN",
					ZZI_ZZR_RateType = rateTypePk,
					ZZI_ZZ9_NKNomenclatureGroupType = "GT",
				});
				entities.RefCusTariffs.Add(new RefCusTariff
				{
					ZZ1_PK = tariffPk,
					ZZ1_Description = "Tariff to be deleted",
					ZZ1_CompositeKeyOnZZ5 = "01.01",
					ZZ1_StartDate = tariffStartDate,
					ZZ1_EndDate = tariffEndDate,
					ZZ1_IAMUnique = 1,
					ZZ1_PublishedDate = tariffStartDate,
					ZZ1_TariffCode = "0101",
					ZZ1_ZZI_TariffType = tariffTypePk,
					ZZ1_ZZZ_NKDataGrouping = "EUN",
					ZZ1_ZZF_NKTaxOrFeeCode = ""
				});
				entities.RefCusRates.Add(duplicatedRate);
				entities.RefCusTradeGroups.Add(new RefCusTradeGroup
				{
					ZZA_Description = "Desc",
					ZZA_EndDate = tariffEndDate,
					ZZA_PK = tradeGroupPk,
					ZZA_StartDate = tariffStartDate,
					ZZA_TradeGroup = "TG",
					ZZA_ZZZ_NKDataGrouping = "EUN"
				});
				await entities.SaveChangesAsync();
			}

			var addedRate = new RefCusRate
			{
				ZZ2_DataSetCode = "ZZ1",
				ZZ2_DataSetPK = tariffPk,
				ZZ2_EndDate = new DateTime(2023, 10, 15),
				ZZ2_StartDate = tariffStartDate,
				ZZ2_PK = ratePk,
				ZZ2_RateFormula = "Form",
				ZZ2_SelectorFormula = "Select",
				ZZ2_ZZ1_Tariff = tariffPk,
				ZZ2_ZZZ_NKDataGrouping = "EUN",
				ZZ2_RateFormulaDerivedFrom = "a",
				ZZ2_ZZS_Preference = null,
				ZZ2_ZZW_TariffNationalCode = null,
				ZZ2_RX_NKCurrencyOverride = ""
			};
			await using (var entities = new SafeDbContext(_connectionString))
			{
				var bulkInsert = new BulkInsertCore();
				var dbAddedEntry = entities.Entry(addedRate);
				var dbDeletedEntry = entities.Entry(duplicatedRate);
				var dicEntriesState = new Dictionary<EntityEntry, EntityState>();
				dicEntriesState.Add(dbAddedEntry, EntityState.Added);
				dicEntriesState.Add(dbDeletedEntry, EntityState.Deleted);
				var entries = new EntityEntry[] { dbAddedEntry, dbDeletedEntry };

				Assert.That(async () => await bulkInsert.SaveChangeWithBulkInsertCore(entries, dicEntriesState, entities), Throws.Nothing);
			}
			await using (var entities = new SafeDbContext(_connectionString))
			{
				Assert.That(entities.RefCusRates.Any(x => x.ZZ2_PK == ratePk), Is.True);
			}
		}

		[Test]
		public async Task SaveChangesWithBulkInsertCore_DuplicatedOne_Modify_InsertAndDelete()
		{
			var tariffPk = Guid.NewGuid();
			var ratePk = Guid.NewGuid();

			var tariffStartDate = new DateTime(2022, 10, 10);
			var tariffEndDate = new DateTime(2022, 10, 10).AddDays(5);
			var tariffTypePk = Guid.NewGuid();
			var rateCodePk = Guid.NewGuid();
			var rateTypePk = Guid.NewGuid();
			var tradeGroupPk = Guid.NewGuid();
			var duplicatedRate = new RefCusRate
			{
				ZZ2_DataSetCode = "ZZ1",
				ZZ2_DataSetPK = tariffPk,
				ZZ2_EndDate = tariffEndDate,
				ZZ2_StartDate = tariffStartDate,
				ZZ2_PK = ratePk,
				ZZ2_RateFormula = "Form",
				ZZ2_SelectorFormula = "Select",
				ZZ2_ZZ1_Tariff = tariffPk,
				ZZ2_ZZZ_NKDataGrouping = "EUN",
				ZZ2_ZY1_RateCode = rateCodePk,
				ZZ2_RateFormulaDerivedFrom = "a",
				ZZ2_ZZS_Preference = null,
				ZZ2_ZZW_TariffNationalCode = null,
				ZZ2_RX_NKCurrencyOverride = ""
			};
			await using (var entities = new SafeDbContext(_connectionString))
			{
				entities.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_DataGrouping = "EUN",
					ZZZ_Description = "Europe",
					ZZZ_PK = Guid.NewGuid(),
				});
				await entities.SaveChangesAsync();
				entities.RefCusRateTypes.Add(new RefCusRateType
				{
					ZZR_PK = rateTypePk,
					ZZR_CustomsValueFormula = "A",
					ZZR_Description = "Desc",
					ZZR_IsPayable = true,
					ZZR_RateType = "RT",
					ZZR_ZZZ_NKDataGrouping = "EUN",
					ZZR_RX_NKFormulaCurrency = "USD"
				});
				entities.RefCusRateCodes.Add(new RefCusRateCode
				{
					ZY1_PK = rateCodePk,
					ZY1_Description = "RC",
					ZY1_InternalUse = true,
					ZY1_RateCode = "RC",
					ZY1_ZZR_RateType = rateTypePk,
				});
				entities.RefCusTariffTypes.Add(new RefCusTariffType
				{
					ZZI_PK = tariffTypePk,
					ZZI_Description = "TP",
					ZZI_TariffType = "TTP",
					ZZI_HasFormulaSpecificQuestions = true,
					ZZI_ZZZ_NKDataGrouping = "EUN",
					ZZI_ZZR_RateType = rateTypePk,
					ZZI_ZZ9_NKNomenclatureGroupType = "GT",
				});
				entities.RefCusTariffs.Add(new RefCusTariff
				{
					ZZ1_PK = tariffPk,
					ZZ1_Description = "Tariff to be deleted",
					ZZ1_CompositeKeyOnZZ5 = "01.01",
					ZZ1_StartDate = tariffStartDate,
					ZZ1_EndDate = tariffEndDate,
					ZZ1_IAMUnique = 1,
					ZZ1_PublishedDate = tariffStartDate,
					ZZ1_TariffCode = "0101",
					ZZ1_ZZI_TariffType = tariffTypePk,
					ZZ1_ZZZ_NKDataGrouping = "EUN",
					ZZ1_ZZF_NKTaxOrFeeCode = ""
				});
				entities.RefCusRates.Add(duplicatedRate);
				entities.RefCusTradeGroups.Add(new RefCusTradeGroup
				{
					ZZA_Description = "Desc",
					ZZA_EndDate = tariffEndDate,
					ZZA_PK = tradeGroupPk,
					ZZA_StartDate = tariffStartDate,
					ZZA_TradeGroup = "TG",
					ZZA_ZZZ_NKDataGrouping = "EUN"
				});
				await entities.SaveChangesAsync();
			}
			var modifiedRate = new RefCusRate
			{
				ZZ2_DataSetCode = "ZZ1",
				ZZ2_DataSetPK = tariffPk,
				ZZ2_EndDate = new DateTime(2023, 10, 15),
				ZZ2_StartDate = tariffStartDate,
				ZZ2_PK = ratePk,
				ZZ2_RateFormula = "Form",
				ZZ2_SelectorFormula = "Select",
				ZZ2_ZZ1_Tariff = tariffPk,
				ZZ2_ZZZ_NKDataGrouping = "EUN",
				ZZ2_RateFormulaDerivedFrom = "a",
				ZZ2_ZZS_Preference = null,
				ZZ2_ZZW_TariffNationalCode = null,
				ZZ2_RX_NKCurrencyOverride = ""
			};

			var addedRate = new RefCusRate
			{
				ZZ2_DataSetCode = "ZZ1",
				ZZ2_DataSetPK = tariffPk,
				ZZ2_EndDate = new DateTime(2023, 10, 15),
				ZZ2_StartDate = tariffStartDate,
				ZZ2_PK = Guid.NewGuid(),
				ZZ2_RateFormula = "Form",
				ZZ2_SelectorFormula = "Select",
				ZZ2_ZZ1_Tariff = tariffPk,
				ZZ2_ZZZ_NKDataGrouping = "EUN",
				ZZ2_RateFormulaDerivedFrom = "a",
				ZZ2_ZZS_Preference = null,
				ZZ2_ZZW_TariffNationalCode = null,
				ZZ2_RX_NKCurrencyOverride = ""
			};
			await using (var entities = new SafeDbContext(_connectionString))
			{
				var bulkInsert = new BulkInsertCore();
				var dbAddedEntry = entities.Entry(addedRate);
				var dbModifiedEntry = entities.Entry(modifiedRate);
				var dbDeletedEntry = entities.Entry(duplicatedRate);
				var dicEntriesState = new Dictionary<EntityEntry, EntityState>();
				dicEntriesState.Add(dbAddedEntry, EntityState.Added);
				dicEntriesState.Add(dbModifiedEntry, EntityState.Modified);
				dicEntriesState.Add(dbDeletedEntry, EntityState.Deleted);
				var entries = new EntityEntry[] { dbAddedEntry, dbModifiedEntry, dbDeletedEntry };

				Assert.That(async () => await bulkInsert.SaveChangeWithBulkInsertCore(entries, dicEntriesState, entities), Throws.Nothing);
			}
			await using (var entities = new SafeDbContext(_connectionString))
			{
				Assert.That(entities.RefCusRates.Any(x => x.ZZ2_PK == ratePk), Is.True);
			}
		}

		[Test]
		public async Task SaveChangesWithDeletedRecord_RefCusProfileQuestion()
		{
			var profileTypePK = Guid.NewGuid();
			var deletedQuestionPK = Guid.NewGuid();
			var newQuestionPK = Guid.NewGuid();
			await CreateRefCusProfileType(profileTypePK);

			var code = "ABC_1234";
			using (var entities = new SafeDbContext(_connectionString))
			{
				var deletedQuestion = CreateRefCusProfileQuestion(profileTypePK, deletedQuestionPK, code);
				entities.RefCusProfileQuestions.Add(deletedQuestion);
				await entities.SaveChangesAsync();

				var refDbVersionControl = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == deletedQuestionPK);
				refDbVersionControl.RVC_Deleted = true;
				await entities.SaveChangesAsync();
			}

			using (var entities = new SafeDbContext(_connectionString))
			{
				var dicEntriesState = new Dictionary<EntityEntry, EntityState>();
				var modifiedEntries = new List<EntityEntry>();
				var question = CreateRefCusProfileQuestion(profileTypePK, newQuestionPK, code);
				var questionEntry = entities.Entry(question);
				dicEntriesState.Add(questionEntry, EntityState.Added);
				modifiedEntries.Add(questionEntry);
				var bulkInsert = new BulkInsertCore();
				Assert.DoesNotThrowAsync(async () => await bulkInsert.SaveChangeWithBulkInsertCore(modifiedEntries.ToArray(), dicEntriesState, entities));
			}

			using (var entities = new SafeDbContext(_connectionString))
			{
				var question = entities.Set<RefCusProfileQuestion>().FirstOrDefault(x => x.XQ2_PK == deletedQuestionPK);
				var refDbVersionControl = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == deletedQuestionPK);
				Assert.IsNull(question);
				Assert.IsNull(refDbVersionControl);
				question = entities.Set<RefCusProfileQuestion>().FirstOrDefault(x => x.XQ2_PK == newQuestionPK);
				refDbVersionControl = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == newQuestionPK);
				Assert.IsNotNull(question);
				Assert.IsNotNull(refDbVersionControl);
			}
		}

		[Test]
		public void SaveEditedUser_WhenBulkInsertCoreUserIdIsNotNull()
		{
			const string testUserId = "testUserId";
			var refAccTaxRateUserView = new RefAccTaxRateUserView
			{
				ZAT_PK = Guid.NewGuid(),
				ZAT_RN_NKCountry = "CR",
				ZAT_ReferenceRateType = "LOW3",
				ZAT_StartDate = new DateTime(1900, 01, 01),
				ZAT_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZAT_RateNumerator = 19,
				ZAT_RateDenominator = 1,
			};

			using (var entities = new SafeDbContext(_connectionString))
			{
				var dicEntriesState = new Dictionary<EntityEntry, EntityState>();
				var modifiedEntries = new List<EntityEntry>();
				var questionEntry = entities.Entry(refAccTaxRateUserView);
				dicEntriesState.Add(questionEntry, EntityState.Added);
				modifiedEntries.Add(questionEntry);
				var bulkInsert = new BulkInsertCore(testUserId);
				Assert.DoesNotThrowAsync(async () => await bulkInsert.SaveChangeWithBulkInsertCore(modifiedEntries.ToArray(), dicEntriesState, entities));
			}

			using (var entities = new SafeDbContext(_connectionString))
			{
				var view = entities.RefAccTaxRateUserViews.FirstOrDefault(x => x.ZAT_PK == refAccTaxRateUserView.ZAT_PK);
				var refDbVersionControl = entities.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == refAccTaxRateUserView.ZAT_PK);
				Assert.That(view, Is.Not.Null);
				Assert.That(refDbVersionControl?.RVC_LastEditedUser, Is.EqualTo(testUserId));
			}
		}

		async Task CreateRefCusProfileType(Guid profileTypePK)
		{
			var tariffTypePK = Guid.NewGuid();
			var dataGrouping = new RefDataGrouping { ZZZ_PK = Guid.NewGuid(), ZZZ_DataGrouping = "ZZ", ZZZ_Description = "ZZ Group" };
			var tariffType = new RefCusTariffType { ZZI_PK = tariffTypePK, ZZI_TariffType = "TST", ZZI_Description = "Test", ZZI_ZZZ_NKDataGrouping = "ZZ", ZZI_ZZ9_NKNomenclatureGroupType = "" };
			var profileType = new RefCusProfileType { XXX_PK = profileTypePK, XXX_ZZI_TariffType = tariffTypePK, XXX_ProfileType = "Test", XXX_Description = "Test Profile Type", XXX_ZZZ_NKDataGrouping = "ZZ" };
			using (var entities = new SafeDbContext(_connectionString))
			{
				entities.Add(dataGrouping);
				await entities.SaveChangesAsync();
				entities.Add(tariffType);
				await entities.SaveChangesAsync();
				entities.Add(profileType);
				await entities.SaveChangesAsync();
			}
		}

		RefCusProfileQuestion CreateRefCusProfileQuestion(Guid profileTypePK, Guid questionPK, string code)
		{
			return new RefCusProfileQuestion
			{
				XQ2_PK = questionPK,
				XQ2_XXX_ProfileType = profileTypePK,
				XQ2_QuestionCode = code,
				XQ2_AnswerDataType = "STRING",
				XQ2_AnswerMask = "",
				XQ2_Name = code,
				XQ2_Text = code,
				XQ2_Note = code,
				XQ2_StartDate = new DateTime(2025, 1, 1),
				XQ2_EndDate = new DateTime(2027, 6, 6),
				XQ2_ZZZ_NKDataGrouping = "ZZ"
			};
		}

		string _connectionString;

		[SetUp]
		public void SetUp()
		{
			var dbName = CreateDatabaseAttribute.DbNamePrefix + "8D9A7098D2FA48E790BBFB0121D588A8";
			_connectionString = TestConnectionString.GetAdmin(dbName);
		}

		class BulkInsertCoreForTest : BulkInsertCore
		{
			public async Task BulkInsertTest<T>(IEnumerable<object> data, SafeDbContext safeDbContext)
			{
				await BulkInsertAsync<T>(data, safeDbContext);
			}
		}
	}
}
