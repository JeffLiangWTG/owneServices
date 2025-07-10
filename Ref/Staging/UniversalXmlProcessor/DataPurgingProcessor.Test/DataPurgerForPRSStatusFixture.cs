using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DataPurgingProcessor.Test
{
	[TestFixture]
	public class DataPurgerForPRSStatusFixture
	{
		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void Purge()
		{
			var builderString = GetEntityConnectionBuilderString();
			var source1ID = Guid.NewGuid();
			var source2ID = Guid.NewGuid();
			var source3ID = Guid.NewGuid();
			var contentText = @"<UniversalReferenceData>
<DataSource>CN Partial Tariff</DataSource>
<PublicationTime>2021-01-23T00:00:00</PublicationTime>
<UpdateType>Partial</UpdateType>
<Schema>
<EntityType Data=""true"" Name=""RefCusTariff"">
<Key>
<PropertyRef Name=""ZZ1_TariffCode""/>
<PropertyRef Name=""ZZ1_ZZI_NKTariffType""/>
<PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping""/>
<PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping""/>
</Key>
<Property Name=""RefCusCondition"" Type=""RefCusCondition""/>
<Property Name=""ZZ1_Description"" Type=""nvarchar""/>
<Property Name=""ZZ1_EndDate"" Type=""datetime"" DefaultValue=""2079-06-06T23:59:00""/>
<Property Name=""ZZ1_StartDate"" Type=""datetime""/>
<Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35""/>
<Property Name=""ZZ1_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""3""/>
<Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5""/>
<Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CN""/>
<Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CN""/>
</EntityType>
<EntityType Data=""true"" Name=""RefCusCondition"">
<Key>
<PropertyRef Name=""ZX1_Comment""/>
<PropertyRef Name=""ZX1_ZX2_NKConditionType""/>
<PropertyRef Name=""ZX1_ZX2_ZZZ_NKDataGrouping""/>
<PropertyRef Name=""ZX1_ZZZ_NKDataGrouping""/>
</Key>
<Property Name=""RefCusConditionValue"" Type=""RefCusConditionValue""/>
<Property Name=""ZX1_Comment"" Type=""nvarchar""/>
<Property Name=""ZX1_ConditionValueTrueMeansStop"" Type=""bit""/>
<Property Name=""ZX1_EndDate"" Type=""smalldatetime"" DefaultValue=""2079-06-06T23:59:00""/>
<Property Name=""ZX1_IsExport"" Type=""bit""/>
<Property Name=""ZX1_IsImport"" Type=""bit""/>
<Property Name=""ZX1_StartDate"" Type=""smalldatetime""/>
<Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""5""/>
<Property Name=""ZX1_ZX2_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CN""/>
<Property Name=""ZX1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CN""/>
</EntityType>
<EntityType Data=""true"" Name=""RefCusConditionValue"">
<Key>
<PropertyRef Name=""ZX3_Value""/>
<PropertyRef Name=""ZX3_ZX4_NKValueType""/>
<PropertyRef Name=""ZX3_ZX4_ZZZ_NKDataGrouping""/>
</Key>
<Property Name=""ZX3_LogicalORWithinGroup"" Type=""tinyint"" ConstantValue=""0""/>
<Property Name=""ZX3_Value"" Type=""nvarchar"" MaxLength=""500""/>
<Property Name=""ZX3_ZX4_NKValueType"" Type=""varchar"" MaxLength=""5""/>
<Property Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" Type=""varchar"" DefaultValue=""CN"" MaxLength=""3""/>
</EntityType>
</Schema>
</UniversalReferenceData>";
			using (var repo = new StagingRepository(builderString))
			{
				var tariff1 = new RefCusTariff
				{
					ZZ1_PK = Guid.NewGuid(),
					ZZ1_TariffCode = "811010",
					ZZ1_Description = "Unwrought antimony; powders",
					ZZ1_StartDate = DateTime.Now.AddDays(-1),
					ZZ1_EndDate = DateTime.Now.AddDays(1),
					ZZ1_ZZF_NKTaxOrFeeCode = "VAT",
					ZZ1_ZZZ_NKDataGrouping = "ZA",
					ZZ1_CompositeKeyOnZZ5 = string.Empty,
					ZZ1_ZZI_ZZZ_NKDataGrouping = "ZA"
				};
				var condition = new RefCusCondition
				{
					ZX1_PK = Guid.NewGuid(),
					ZX1_ZZ1_Tariff = tariff1.ZZ1_PK,
					ZX1_ZZZ_NKDataGrouping = "ZA",
					ZX1_Comment = "A",
					ZX1_EndDate = new DateTime(2079, 06, 06),
					ZX1_StartDate = new DateTime(1900, 01, 01),
					ZX1_ZX2_NKConditionType = "C1",
					ZX1_ZX2_ZZZ_NKDataGrouping = "ZA",
					ZX1_Source = "B",
					ZX1_AdditionalComment = "comment",
					ZX1_ZY7_NKConditionCode = "AAA"
				};
				var conditionValue = new RefCusConditionValue
				{
					ZX3_LogicalORWithinGroup = 0,
					ZX3_PK = Guid.NewGuid(),
					ZX3_ZX1_Condition = condition.ZX1_PK,
					ZX3_ZX4_NKValueType = "V1",
					ZX3_ZX4_ZZZ_NKDataGrouping = "ZA",
					ZX3_Value = "T"
				};

				var tariff2 = new RefCusTariff
				{
					ZZ1_PK = Guid.NewGuid(),
					ZZ1_TariffCode = "811020",
					ZZ1_Description = "XXX",
					ZZ1_StartDate = DateTime.Now.AddDays(-1),
					ZZ1_EndDate = DateTime.Now.AddDays(1),
					ZZ1_ZZF_NKTaxOrFeeCode = "VAT",
					ZZ1_ZZZ_NKDataGrouping = "ZA",
					ZZ1_ZZI_ZZZ_NKDataGrouping = "ZA",
					ZZ1_CompositeKeyOnZZ5 = string.Empty
				};
				var source1 = new SourceData
				{
					SDA_PK = source1ID,
					SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
					SDA_ContentText = contentText,
					SDA_Status = StatusProvider.GetMERStatus(),
					SDA_Source = DataSourceConstants.Source.InternalWebsite,
					SDA_SubSource = "sub source"
				};
				var source2 = new SourceData
				{
					SDA_PK = source2ID,
					SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
					SDA_ContentText = contentText,
					SDA_Status = StatusProvider.GetERRStatus(),
					SDA_Source = DataSourceConstants.Source.InternalWebsite,
					SDA_SubSource = "sub source"
				};
				var source3 = new SourceData
				{
					SDA_PK = source3ID,
					SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
					SDA_ContentText = contentText,
					SDA_Status = StatusProvider.GetMERStatus(),
					SDA_Source = DataSourceConstants.Source.InternalWebsite,
					SDA_SubSource = "sub source"
				};
				var info1 = new DataProcessingInformation
				{
					DPI_ID = Guid.NewGuid(),
					DPI_ParentPk = tariff1.ZZ1_PK,
					DPI_ParentTableCode = "ZZ1",
					DPI_SourceId = source1.SDA_PK,
					DPI_Status = "PRS"
				};
				var info2 = new DataProcessingInformation
				{
					DPI_ID = Guid.NewGuid(),
					DPI_ParentPk = tariff2.ZZ1_PK,
					DPI_ParentTableCode = "ZZ1",
					DPI_SourceId = source2.SDA_PK,
					DPI_Status = "PRS"
				};
				var info3 = new DataProcessingInformation
				{
					DPI_ID = Guid.NewGuid(),
					DPI_ParentTableCode = "INT",
					DPI_SourceId = source3.SDA_PK,
					DPI_Status = "ERR"
				};
				repo.Add(source1);
				repo.Add(source2);
				repo.Add(source3);
				repo.Add(tariff1);
				repo.Add(tariff2);
				repo.Add(condition);
				repo.Add(conditionValue);
				repo.Add(info1);
				repo.Add(info2);
				repo.Add(info3);
				repo.SaveChanges();
				// SDA_CreatedTime is set to StoreGeneratedPattern=Computed in table_config.yaml, it will be ignored when saveChanged() by EF.
				repo.ExecuteSqlCommand(
					$"UPDATE {nameof(SourceData)} SET {nameof(SourceData.SDA_CreatedTime)} = @CreatedTime WHERE {nameof(SourceData.SDA_PK)} = @SourceID",
					new SqlParameter("@CreatedTime", System.Data.SqlDbType.DateTime2) { Value = DateTime.UtcNow.AddMonths(1) },
					new SqlParameter("@SourceID", System.Data.SqlDbType.UniqueIdentifier) { Value = source3ID });
			}
			using (var repo = new StagingRepository(builderString))
			{
				var purger = new DataPurgerForPRSStatus(repo);
				purger.Purge();
				Assert.AreEqual(repo.Get<RefCusTariff>().ToArray().Length, 0);
				Assert.AreEqual(repo.Get<RefCusCondition>().ToArray().Length, 0);
				Assert.AreEqual(repo.Get<RefCusConditionValue>().ToArray().Length, 0);
				Assert.AreEqual(repo.Get<DataProcessingInformation>().Where(x => x.DPI_SourceId == source1ID).ToArray().Length, 0);
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void CanPurgeZaTariffsSubSource()
		{
			var source1ID = Guid.NewGuid();
			var source2ID = Guid.NewGuid();
			var builderString = GetEntityConnectionBuilderString();
			var contentText = "ANYTHING THAT IS NOT AN XML";
			using (var repo = new StagingRepository(builderString))
			{
				var tariff1 = new RefCusTariff
				{
					ZZ1_PK = Guid.NewGuid(),
					ZZ1_TariffCode = "811010",
					ZZ1_Description = "Unwrought antimony; powders",
					ZZ1_StartDate = DateTime.UtcNow.AddDays(-1),
					ZZ1_EndDate = DateTime.UtcNow.AddDays(1),
					ZZ1_ZZF_NKTaxOrFeeCode = "VAT",
					ZZ1_ZZZ_NKDataGrouping = "ZA",
					ZZ1_CompositeKeyOnZZ5 = string.Empty,
					ZZ1_ZZI_ZZZ_NKDataGrouping = "ZA"
				};
				var refCusTariffAttribute1 = new RefCusTariffAttribute
				{
					ZZ3_ZZ1_Tariff = tariff1.ZZ1_PK,
					ZZ3_Name = "CheckDigit",
					ZZ3_Value = "0",
					ZZ3_PK = Guid.NewGuid()
				};
				var refCusTariffUOM = new RefCusTariffUOM
				{
					ZZ8_PK = Guid.NewGuid(),
					ZZ8_ZZ1_Tariff = tariff1.ZZ1_PK,
					ZZ8_Type = "CU1",
					ZZ8_UOM = "LI",
					ZZ8_ZZA_ZZZ_NKDataGrouping = "ZA",
					ZZ8_ZZZ_NKDataGrouping = "ZA"
				};
				var refCusTariffRelationship = new RefCusTariffRelationship
				{
					ZZH_PK = Guid.NewGuid(),
					ZZH_ZZ1_Tariff = tariff1.ZZ1_PK,
					ZZH_ZZI_NKTariffType = "15A",
					ZZH_TariffCode = "27101202",
					ZZH_ZZI_ZZZ_NKDataGrouping = "ZA"
				};
				var rate1 = new RefCusRate
				{
					ZZ2_PK = Guid.NewGuid(),
					ZZ2_RateFormula = "ANy",
					ZZ2_StartDate = DateTime.UtcNow.AddDays(-1),
					ZZ2_SelectorFormula = "ANY",
					ZZ2_EndDate = DateTime.UtcNow.AddDays(1),
					ZZ2_ZZ1_Tariff = tariff1.ZZ1_PK,
					ZZ2_RateFormulaDerivedFrom = "A",
					ZZ2_ZZS_ZZZ_NKDataGrouping = "ZA",
					ZZ2_ZY1_NKRateCode = "RT",
					ZZ2_ZY1_ZZR_NKRateType = "TR",
					ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping = "ZA",
					ZZ2_ZZZ_NKDataGrouping = "ZA",
					ZZ2_RX_NKCurrencyOverride = "USD"
				};
				var applicability = new RefCusApplicability
				{
					ZZT_PK = Guid.NewGuid(),
					ZZT_StartDate = rate1.ZZ2_StartDate,
					ZZT_EndDate = rate1.ZZ2_EndDate,
					ZZT_ZZ2_Rate = rate1.ZZ2_PK,
					ZZT_AdditionalCode = "2",
					ZZT_OrderNumber = "1",
					ZZT_ZZA_ZZZ_NKDataGrouping = "ZA",
					ZZT_ZZA_NKTradeGroup = "ZA"
				};
				var excludedTradeGroup = new RefCusExcludedTradeGroup
				{
					ZZC_PK = Guid.NewGuid(),
					ZZC_ZZA_ZZZ_NKDataGrouping = "ZA",
					ZZC_ZZT_Applicability = applicability.ZZT_PK,
					ZZC_ZZA_NKTradeGroup = "ZA"
				};
				var source1 = new SourceData
				{
					SDA_PK = source1ID,
					SDA_ContentType = "PRO",
					SDA_ContentText = contentText,
					SDA_Status = StatusProvider.GetMERStatus(),
					SDA_Source = DataSourceConstants.Source.InternalWebsite,
					SDA_SubSource = "ZA Tariffs"
				};
				var info1 = new DataProcessingInformation
				{
					DPI_ID = Guid.NewGuid(),
					DPI_ParentPk = tariff1.ZZ1_PK,
					DPI_ParentTableCode = "ZZ1",
					DPI_SourceId = source1.SDA_PK,
					DPI_Status = "PRS"
				};
				var source2 = new SourceData
				{
					SDA_PK = source2ID,
					SDA_ContentType = "PRO",
					SDA_ContentText = contentText,
					SDA_Status = StatusProvider.GetMERStatus(),
					SDA_Source = DataSourceConstants.Source.InternalWebsite,
					SDA_SubSource = "ZA Tariffs"
				};
				var info2 = new DataProcessingInformation
				{
					DPI_ID = Guid.NewGuid(),
					DPI_ParentTableCode = "ZZ1",
					DPI_SourceId = source2.SDA_PK,
					DPI_Status = "PRS"
				};

				repo.Add(source1);
				repo.Add(source2);
				repo.Add(tariff1);
				repo.Add(refCusTariffAttribute1);
				repo.Add(refCusTariffRelationship);
				repo.Add(refCusTariffUOM);
				repo.Add(rate1);
				repo.Add(applicability);
				repo.Add(excludedTradeGroup);
				repo.Add(info1);
				repo.Add(info2);
				repo.SaveChanges();
				// SDA_CreatedTime is set to StoreGeneratedPattern=Computed in table_config.yaml, it will be ignored when saveChanged() by EF.
				repo.ExecuteSqlCommand(
					$"UPDATE {nameof(SourceData)} SET {nameof(SourceData.SDA_CreatedTime)} = @CreatedTime WHERE {nameof(SourceData.SDA_PK)} = @SourceID",
					new SqlParameter("@CreatedTime", System.Data.SqlDbType.DateTime2) { Value = DateTime.UtcNow.AddMonths(1) },
					new SqlParameter("@SourceID", System.Data.SqlDbType.UniqueIdentifier) { Value = source2ID });
			}
			using (var repo = new StagingRepository(builderString))
			{
				var purger = new DataPurgerForPRSStatus(repo);
				Assert.DoesNotThrow(() => purger.Purge());
				Assert.AreEqual(repo.Get<RefCusTariff>().ToArray().Length, 0);
				Assert.AreEqual(repo.Get<RefCusTariffAttribute>().ToArray().Length, 0);
				Assert.AreEqual(repo.Get<RefCusTariffRelationship>().ToArray().Length, 0);
				Assert.AreEqual(repo.Get<RefCusTariffUOM>().ToArray().Length, 0);

				Assert.AreEqual(repo.Get<RefCusRate>().ToArray().Length, 0);
				Assert.AreEqual(repo.Get<RefCusApplicability>().ToArray().Length, 0);
				Assert.AreEqual(repo.Get<RefCusExcludedTradeGroup>().ToArray().Length, 0);
				Assert.AreEqual(repo.Get<DataProcessingInformation>().Where(x => x.DPI_SourceId == source1ID).ToArray().Length, 0);
			}
			using (var repo = new StagingRepository(builderString))
			{
				var savedSource1 = repo.Get<SourceData>().FirstOrDefault(x => x.SDA_PK == source1ID);
				Assert.IsTrue(savedSource1.IsSourceDataFinalStatus());
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void ThrowsExceptionWhenSubSourceNotConfigured()
		{
			var builderString = GetEntityConnectionBuilderString();
			var source1 = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_ContentText = "ANY",
				SDA_Filetype = "TXT",
				SDA_Status = StatusProvider.GetMERStatus(),
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_SubSource = "UNKNOWN",
				SDA_ContentType = "PRO"
			};
			var source2 = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_ContentText = "ANY",
				SDA_Filetype = "TXT",
				SDA_Status = StatusProvider.GetMERStatus(),
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_SubSource = "UNKNOWN",
				SDA_ContentType = "PRO"
			};
			var info1 = new DataProcessingInformation
			{
				DPI_ID = Guid.NewGuid(),
				DPI_ParentPk = Guid.NewGuid(),
				DPI_ParentTableCode = "ZZ1",
				DPI_SourceId = source1.SDA_PK,
				DPI_Status = "PRS"
			};
			var info2 = new DataProcessingInformation
			{
				DPI_ID = Guid.NewGuid(),
				DPI_ParentPk = Guid.NewGuid(),
				DPI_ParentTableCode = "ZZ1",
				DPI_SourceId = source2.SDA_PK,
				DPI_Status = "PRS"
			};
			using (var repo = new StagingRepository(builderString))
			{
				repo.Add(source1);
				repo.Add(source2);
				repo.Add(info1);
				repo.Add(info2);
				repo.SaveChanges();
			}
			using (var repo = new StagingRepository(builderString))
			{
				var helper = new DataPurgerForPRSStatus(repo);
				Assert.Throws<NotSupportedException>(() => helper.Purge(), "SourceData with SDA_SubSource=UNKNOWN not configured and cannot be purged");
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task PurgeDataProcessingClone()
		{
			var sourcePK = Guid.NewGuid();
			var builderString = GetEntityConnectionBuilderString();
			using (var stagingRepo = new StagingRepository(builderString))
			{
				var newPK = Guid.NewGuid();
				var expiredPK = Guid.NewGuid();
				var clone1 = new DataProcessingClone { DPC_PK = Guid.NewGuid(), DPC_NewRecordPK = newPK, DPC_ExpiredRecordPK = expiredPK, DPC_TableCode = "ZZ1", DPC_SourceId = sourcePK, DPC_Status = "PRS" };
				var clone2 = new DataProcessingClone { DPC_PK = Guid.NewGuid(), DPC_NewRecordPK = newPK, DPC_ExpiredRecordPK = expiredPK, DPC_TableCode = "ZZ1", DPC_SourceId = sourcePK, DPC_Status = "QUE" };
				var clone3 = new DataProcessingClone { DPC_PK = Guid.NewGuid(), DPC_NewRecordPK = newPK, DPC_ExpiredRecordPK = expiredPK, DPC_TableCode = "ZZ1", DPC_SourceId = sourcePK, DPC_Status = "ERR" };
				var clone4 = new DataProcessingClone { DPC_PK = Guid.NewGuid(), DPC_NewRecordPK = newPK, DPC_ExpiredRecordPK = expiredPK, DPC_TableCode = "ZZ1", DPC_SourceId = Guid.Empty, DPC_Status = "PRS" };
				await stagingRepo.BulkInsertWithRetryAsync([clone1, clone2, clone3, clone4]);
				stagingRepo.SaveChanges();
			}

			using (var stagingRepo = new StagingRepository(builderString))
			{
				var purger = new DataPurgerForPRSStatus(stagingRepo);
				purger.Purge();

				var dpcRecords = stagingRepo.Get<DataProcessingClone>();
				Assert.AreEqual(4, dpcRecords.Count());
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void PurgeDataForNullParentPK()
		{
			var builderString = GetEntityConnectionBuilderString();
			var source1 = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_ContentType = "PRO",
				SDA_ContentText = "any",
				SDA_Status = StatusProvider.GetMERStatus(),
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_SubSource = "ZA Tariffs",
			};
			var source2 = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_ContentType = "PRO",
				SDA_ContentText = "any",
				SDA_Status = StatusProvider.GetMERStatus(),
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_SubSource = "ZA Tariffs",
			};
			var tariff1 = new RefCusTariff
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_TariffCode = "811010",
				ZZ1_Description = "Unwrought antimony; powders",
				ZZ1_StartDate = DateTime.Now.AddDays(-1),
				ZZ1_EndDate = DateTime.Now.AddDays(1),
				ZZ1_ZZF_NKTaxOrFeeCode = "VAT",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				ZZ1_CompositeKeyOnZZ5 = string.Empty,
				ZZ1_ZZI_ZZZ_NKDataGrouping = "ZA"
			};
			var tariff2 = new RefCusTariff
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_TariffCode = "811010",
				ZZ1_Description = "Unwrought antimony; powders",
				ZZ1_StartDate = DateTime.Now.AddDays(-1),
				ZZ1_EndDate = DateTime.Now.AddDays(1),
				ZZ1_ZZF_NKTaxOrFeeCode = "VAT",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				ZZ1_CompositeKeyOnZZ5 = string.Empty,
				ZZ1_ZZI_ZZZ_NKDataGrouping = "ZA"
			};
			var info1 = new DataProcessingInformation
			{
				DPI_ID = Guid.NewGuid(),
				DPI_ParentPk = null,
				DPI_ParentTableCode = "ZZ1",
				DPI_SourceId = source1.SDA_PK,
				DPI_Status = StatusProvider.GetPRSStatus()
			};
			var info2 = new DataProcessingInformation
			{
				DPI_ID = Guid.NewGuid(),
				DPI_ParentPk = tariff1.ZZ1_PK,
				DPI_ParentTableCode = "ZZ1",
				DPI_SourceId = source1.SDA_PK,
				DPI_Status = StatusProvider.GetPRSStatus()
			};
			var info3 = new DataProcessingInformation
			{
				DPI_ID = Guid.NewGuid(),
				DPI_ParentPk = null,
				DPI_ParentTableCode = "ZZ1",
				DPI_SourceId = source2.SDA_PK,
				DPI_Status = StatusProvider.GetPRSStatus()
			};
			var info4 = new DataProcessingInformation
			{
				DPI_ID = Guid.NewGuid(),
				DPI_ParentPk = tariff2.ZZ1_PK,
				DPI_ParentTableCode = "ZZ1",
				DPI_SourceId = source2.SDA_PK,
				DPI_Status = StatusProvider.GetPRSStatus()
			};

			using (var repo = new StagingRepository(builderString))
			{
				repo.Add(source1);
				repo.Add(source2);
				repo.Add(info1);
				repo.Add(info2);
				repo.Add(info3);
				repo.Add(info4);
				repo.Add(tariff1);
				repo.Add(tariff2);
				repo.SaveChanges();

				// SDA_CreatedTime is set to StoreGeneratedPattern=Computed in table_config.yaml, it will be ignored when saveChanged() by EF.
				repo.ExecuteSqlCommand(
					$"UPDATE {nameof(SourceData)} SET {nameof(SourceData.SDA_CreatedTime)} = @CreatedTime WHERE {nameof(SourceData.SDA_PK)} = @SourceID",
					new SqlParameter("@CreatedTime", System.Data.SqlDbType.DateTime2) { Value = DateTime.UtcNow.AddMonths(1) },
					new SqlParameter("@SourceID", System.Data.SqlDbType.UniqueIdentifier) { Value = source1.SDA_PK });
			}

			using (var repo = new StagingRepository(builderString))
			{
				var purger = new DataPurgerForPRSStatus(repo);
				purger.Purge();
				Assert.AreEqual(0, repo.Get<DataProcessingInformation>().Where(x => x.DPI_SourceId == source2.SDA_PK).ToArray().Length);
				Assert.AreEqual(0, repo.Get<RefCusTariff>().Where(x => x.ZZ1_PK == tariff2.ZZ1_PK).ToArray().Length);
			}
		}

		string GetEntityConnectionBuilderString()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			return TestConnectionString.GetAdmin(dbName);
		}
	}
}
