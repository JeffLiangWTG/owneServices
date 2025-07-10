using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DataPurgingProcessor.Test
{
	[TestFixture]
	public class DataPurgerForOrphanDataFixture
	{
		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void PurgeDataForOrphanData()
		{
			var builderString = GetEntityConnectionBuilderString();
			var source1ID = Guid.NewGuid();
			var source2ID = Guid.NewGuid();
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
					SDA_Status = StatusProvider.GetFINStatus(),
					SDA_Source = DataSourceConstants.Source.InternalWebsite,
					SDA_SubSource = "SubSource1"
				};
				var source2 = new SourceData
				{
					SDA_PK = source2ID,
					SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
					SDA_ContentText = contentText,
					SDA_Status = StatusProvider.GetFIEStatus(),
					SDA_Source = DataSourceConstants.Source.InternalWebsite,
					SDA_SubSource = "SubSource2"
				};
				var info2 = new DataProcessingInformation
				{
					DPI_ID = Guid.NewGuid(),
					DPI_ParentPk = tariff2.ZZ1_PK,
					DPI_ParentTableCode = "ZZ1",
					DPI_SourceId = source2.SDA_PK,
					DPI_Status = StatusProvider.GetERRStatus()
				};
				repo.Add(source1);
				repo.Add(source2);
				repo.Add(tariff1);
				repo.Add(tariff2);
				repo.Add(condition);
				repo.Add(conditionValue);
				repo.Add(info2);
				repo.SaveChanges();
			}

			using (var repo = new StagingRepository(builderString))
			{
				var purger = new DataPurgerForOrphanData(repo);
				purger.Purge();
				Assert.AreEqual(1, repo.Get<RefCusTariff>().ToArray().Length);
				Assert.AreEqual(0, repo.Get<RefCusCondition>().ToArray().Length);
				Assert.AreEqual(0, repo.Get<RefCusConditionValue>().ToArray().Length);
				Assert.AreEqual(0, repo.Get<DataProcessingInformation>().Where(x => x.DPI_SourceId == source1ID).ToArray().Length);
				Assert.AreEqual(1, repo.Get<DataProcessingInformation>().Where(x => x.DPI_SourceId == source2ID).ToArray().Length);
			}
		}

		string GetEntityConnectionBuilderString()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			return TestConnectionString.GetAdmin(dbName);
		}
	}
}
