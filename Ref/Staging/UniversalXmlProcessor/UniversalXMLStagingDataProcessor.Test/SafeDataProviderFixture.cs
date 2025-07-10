using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.OData.Edm;
using Moq;
using NUnit.Framework;
using SafeDataClient = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Stage = CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class SafeDataProviderFixture
	{
		[TestCase(true)]
		[TestCase(false)]
		public void TestUserOverride(bool userOverride)
		{
			var unloco = new RefUNLOCO { RL_UserOverride = userOverride };
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			Assert.AreEqual(userOverride, provider.IsUserOverride(unloco));
		}

		[Test]
		public void GetTypeFromTblPrefix()
		{
			var cache = CacheProvider.Create();
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cache, overlappingCalculator);
			Assert.AreEqual(typeof(RefCusTariff), provider.GetTypeFromTblPrefix("ZZ1"));
			Assert.True(cache.TableCodeAndTypeCache.ContainsKey("ZZ1"));
			Assert.True(cache.TableCodeAndTypeCache["ZZ1"].Value == typeof(RefCusTariff));
		}

		[Test]
		public void GetRelatedTypeAndNKPropertyNames()
		{
			var propertyNames = new[] { "ZZ2_StartDate", "ZZ2_EndDate", "ZZ2_ZY1_NKRateCode", "ZZ2_ZY1_ZZR_NKRateType",
			"ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping", "ZZ2_RateFormula", "ZZ2_ZZS_NKPreference", "ZZ2_ZZS_ZZZ_NKDataGrouping",
			"ZZ2_SelectorFormula", "ZZ2_ZZZ_NKDataGrouping", "RefCusApplicability" };
			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetProperties(nameof(RefCusTariff))).Returns(propertyNames);
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			var result = provider.GetRelatedTypeAndNKPropertyNames(nameof(RefCusTariff), metadataProvider.Object).ToArray();
			Assert.That(result[0].Item1, Is.EqualTo(typeof(RefCusRateCode)));
			Assert.That(result[0].Item2, Is.EqualTo(nameof(RefCusRate.ZZ2_ZY1_RateCode)));
			CollectionAssert.AreEqual(new[] { "ZZ2_ZY1_NKRateCode", "ZZ2_ZY1_ZZR_NKRateType", "ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping" }, result[0].Item3);
			Assert.That(result[1].Item1, Is.EqualTo(typeof(RefCusPreference)));
			Assert.That(result[1].Item2, Is.EqualTo(nameof(RefCusRate.ZZ2_ZZS_Preference)));
			CollectionAssert.AreEqual(new[] { "ZZ2_ZZS_NKPreference", "ZZ2_ZZS_ZZZ_NKDataGrouping" }, result[1].Item3);
		}

		[Test]
		public void GetRelatedTypeAndNKPropertyNames_MultipleNKsToSameType()
		{
			var propertyNames = new[] { "ZZT_ZZA_NKTradeGroup", "ZZT_ZZA_ZZZ_NKDataGrouping", "ZZT_ZZA_NKSecondTradeGroup", "ZZT_ZZA_ZZZ_NKSecondDataGrouping" };
			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetProperties(nameof(RefCusApplicability))).Returns(propertyNames);
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			var result = provider.GetRelatedTypeAndNKPropertyNames(nameof(RefCusApplicability), metadataProvider.Object).ToArray();
			Assert.AreEqual(2, result.Length);
			Assert.That(result[0].Item1, Is.EqualTo(typeof(RefCusTradeGroup)));
			Assert.That(result[0].Item2, Is.EqualTo(nameof(RefCusApplicability.ZZT_ZZA_TradeGroup)));
			CollectionAssert.AreEqual(new[] { "ZZT_ZZA_NKTradeGroup", "ZZT_ZZA_ZZZ_NKDataGrouping" }, result[0].Item3);
			Assert.That(result[1].Item1, Is.EqualTo(typeof(RefCusTradeGroup)));
			Assert.That(result[1].Item2, Is.EqualTo(nameof(RefCusApplicability.ZZT_ZZA_SecondTradeGroup)));
			CollectionAssert.AreEqual(new[] { "ZZT_ZZA_NKSecondTradeGroup", "ZZT_ZZA_ZZZ_NKSecondDataGrouping" }, result[1].Item3);

			propertyNames = new[] { "XQP_XQ2_NKQuestionParent", "XQP_XQ2_NKQuestionStartDateParent", "XQP_XQ2_ZZZ_NKDataGroupingParent", "XQP_XQ2_NKQuestionChild", "XQP_XQ2_NKQuestionStartDateChild", "XQP_XQ2_ZZZ_NKDataGroupingChild", "XQP_XQ2_XXX_NKProfileType", "XQP_XQ2_XXX_ZZZ_NKDataGrouping", "XQP_XQ2_XXX_ZZI_NKTariffType", "XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping" };
			metadataProvider.Setup(x => x.GetProperties(nameof(RefCusProfileQuestionPathway))).Returns(propertyNames);
			result = provider.GetRelatedTypeAndNKPropertyNames(nameof(RefCusProfileQuestionPathway), metadataProvider.Object).ToArray();
			Assert.AreEqual(2, result.Length);
			Assert.True(result.All(x => x.Item1 == typeof(RefCusProfileQuestion)));
			var relatedTypeAndProperties = result.First(x => x.Item2 == nameof(RefCusProfileQuestionPathway.XQP_XQ2_QuestionParent));
			CollectionAssert.AreEqual(new[] { "XQP_XQ2_NKQuestionParent", "XQP_XQ2_NKQuestionStartDateParent", "XQP_XQ2_ZZZ_NKDataGroupingParent", "XQP_XQ2_XXX_NKProfileType", "XQP_XQ2_XXX_ZZZ_NKDataGrouping", "XQP_XQ2_XXX_ZZI_NKTariffType", "XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping" }, relatedTypeAndProperties.Item3);
			relatedTypeAndProperties = result.First(x => x.Item2 == nameof(RefCusProfileQuestionPathway.XQP_XQ2_QuestionChild));
			CollectionAssert.AreEqual(new[] { "XQP_XQ2_NKQuestionChild", "XQP_XQ2_NKQuestionStartDateChild", "XQP_XQ2_ZZZ_NKDataGroupingChild", "XQP_XQ2_XXX_NKProfileType", "XQP_XQ2_XXX_ZZZ_NKDataGrouping", "XQP_XQ2_XXX_ZZI_NKTariffType", "XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping" }, relatedTypeAndProperties.Item3);
		}

		[Test]
		public void GetRelatedTypeAndNKPropertyNames_MultipleNKsToSameType_WithNoExplicitMapping()
		{
			var propertyNames = new[] { "ZZT_ZZA_NKTradeGroup1", "ZZT_ZZA_ZZZ_NKDataGrouping1" };
			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetProperties(nameof(RefCusApplicability))).Returns(propertyNames);
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			Assert.Throws(Is.TypeOf<RefDataProcessingException>().And.Message.StartsWith($@"There are multiple FKs between table prefix ZZT and table prefix ZZA. Please use explicit mapping in this case.
ErrorCode: {ErrorCodes.MultipleFKsBetweenTablePrefixes}")
				, () => provider.GetRelatedTypeAndNKPropertyNames(nameof(RefCusApplicability), metadataProvider.Object).ToArray());
		}

		[Test]
		public void GetRelatedTypeAndNKPropertyNames_WithInvalidPrefix()
		{
			var propertyNames = new[] { "ZZN_ExRateType", "ZZN_RX_NKExCurrency" };
			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetProperties(nameof(RefExchangeRateZZ))).Returns(propertyNames);
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			Assert.AreEqual(0, provider.GetRelatedTypeAndNKPropertyNames(nameof(RefExchangeRateZZ), metadataProvider.Object).ToArray().Length);
		}

		[Test]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		[TransactionedTestCase]
		public void GetRelatedTypeAndNKPropertyNames_ConsistentWithDb()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			var connectionString = TestConnectionString.GetAdmin(dbName);
			using var sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			var tableAndFKColumnsDic = new Dictionary<string, List<string>>();
			using var command = sqlConnection.CreateCommand();
			var sql = @"drop table if exists #TempFK
select parent_object_id as ParentID, referenced_object_id as ReferencedID, referenced_column_id as ReferencedColumnID into #TempFK
from sys.foreign_key_columns 
group by parent_object_id, referenced_object_id, referenced_column_id having count(*) > 1;

select OBJECT_NAME(parent_object_id) ParentTable, OBJECT_NAME(referenced_object_id) ReferencedTable, COL_NAME(parent_object_id, parent_column_id) ColumnName
from sys.foreign_key_columns fkc join #TempFK on parent_object_id = ParentID and referenced_object_id = ReferencedID and referenced_column_id = ReferencedColumnID
join sys.columns c on fkc.parent_object_id = c.object_id and fkc.parent_column_id = c.column_id
where c.system_type_id = 36";
			command.CommandText = sql;
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var table = $"{reader.GetString(0)}_{reader.GetString(1)}";
					if (!tableAndFKColumnsDic.ContainsKey(table))
					{
						tableAndFKColumnsDic[table] = new List<string>();
					}
					tableAndFKColumnsDic[table].Add(reader.GetString(2));
				}
			}

			var referencedTables = tableAndFKColumnsDic.Keys.Select(x => $"'{x.Split('_')[1]}'");
			sql = $@"select OBJECT_NAME(ic.object_id) TableName, COL_NAME(ic.object_id,ic.column_id) ColumnName 
from sys.indexes i join sys.index_columns ic on ic.index_id = i.index_id and ic.object_id = i.object_id
where is_unique = 1 and is_primary_key = 0 and OBJECT_NAME(ic.object_id) in ({string.Join(',', referencedTables)})";

			var tableAndUniqueColumnsDic = new Dictionary<string, List<string>>();
			command.CommandText = sql;
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var tableName = reader.GetString(0);
					var columnName = reader.GetString(1);
					if (!tableAndUniqueColumnsDic.ContainsKey(tableName))
					{
						tableAndUniqueColumnsDic[tableName] = new List<string>();
					}
					tableAndUniqueColumnsDic[tableName].Add(columnName);
				}
			}

			var metadataProvider = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			foreach (var table in tableAndFKColumnsDic.Keys)
			{
				var tableName = table.Split('_')[0];
				var referencedTableName = table.Split('_')[1];
				var tableType = typeof(Stage.RefCusTariff).Assembly.GetType($"{typeof(Stage.RefCusTariff).Namespace}.{tableName}");
				var tablePrefix = tableAndFKColumnsDic[table].First().Split('_')[0];
				var properties = tableType.GetProperties().Where(x => x.Name.StartsWith($"{tablePrefix}_")).Select(x => x.Name).ToArray();
				metadataProvider.Setup(x => x.GetProperties(tableType.Name)).Returns(properties);
				var result = provider.GetRelatedTypeAndNKPropertyNames(tableName, metadataProvider.Object).ToArray();
				Assert.AreEqual(tableAndFKColumnsDic[table].Count, result.Length, $"Missing Explicit or Shared NKMapping between {tableName} and {referencedTableName}");
				Assert.True(result.All(x => x.Item1.Name == referencedTableName), $"Missing Explicit or Shared NKMapping between {tableName} and {referencedTableName}");
				var relatedPropertyCount = result.First().Item3.Length;
				Assert.True(result.All(x => x.Item3.Length == relatedPropertyCount), $"{string.Join(',', result.Select(x => x.Item2))} should have same nk property names");

				var indexColumnCount = tableAndUniqueColumnsDic[referencedTableName].Count;
				foreach (var fkColumn in tableAndFKColumnsDic[table])
				{
					var relatedTypeAndProperties = result.First(x => x.Item2 == fkColumn);
					Assert.GreaterOrEqual(relatedTypeAndProperties.Item3.Length, indexColumnCount, $"There should be at least {indexColumnCount} nk mappings for column {fkColumn}");
				}
			}
		}

		[Test]
		public void GetRelatedEntity()
		{
			var safe = new Mock<ISafeRepository>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var nkPropertyNamesAndValues = new (string PropertyName, object PropertyValue)[] {
				("ZZ2_ZY1_NKRateCode", "1P1"),
				("ZZ2_ZY1_ZZR_NKRateType", "XXX"),
				("ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping", "ZA")
			};
			var rateType = new RefCusRateType { ZZR_RateType = "XXX", ZZR_ZZZ_NKDataGrouping = "ZA" };
			var rateCode1 = new RefCusRateCode { ZY1_RateCode = "1P1", RefCusRateType = rateType };
			var rateCode2 = new RefCusRateCode { ZY1_RateCode = "1P2", RefCusRateType = rateType };
			safe.Setup(x => x.Get<RefCusRateType>()).Returns(new[] { rateType }.AsQueryable());
			safe.Setup(x => x.Get<RefCusRateCode>()).Returns(new[] { rateCode1, rateCode2 }.AsQueryable());

			Assert.That(provider.GetRelatedEntity<RefCusRateCode>(nkPropertyNamesAndValues), Is.EqualTo(rateCode1));
		}

		[Test]
		public void GetRelatedEntity_NoMatch()
		{
			var safe = new Mock<ISafeRepository>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var nkPropertyNamesAndValues = new (string PropertyName, object PropertyValue)[] {
				("ZZ2_ZY1_NKRateCode", "1P1"),
				("ZZ2_ZY1_ZZR_NKRateType", "XXX"),
				("ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping", "ZA")
			};
			var rateType = new RefCusRateType { ZZR_RateType = "XXX", ZZR_ZZZ_NKDataGrouping = "ZA" };
			var rateCode2 = new RefCusRateCode { ZY1_RateCode = "1P2", RefCusRateType = rateType };
			safe.Setup(x => x.Get<RefCusRateType>()).Returns(new[] { rateType }.AsQueryable());
			safe.Setup(x => x.Get<RefCusRateCode>()).Returns(new[] { rateCode2 }.AsQueryable());
			var expectedMessage = $@"GetRelatedEntity returns 0 match for RefCusRateCode.
NkPropertyNamesAndValues: ZZ2_ZY1_NKRateCode = 1P1; ZZ2_ZY1_ZZR_NKRateType = XXX; ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping = ZA.
ErrorCode: {ErrorCodes.MultipleOrNoneRelatedEntities}";
			Assert.Throws(Is.TypeOf<RefDataProcessingException>().And.Message.StartsWith(expectedMessage), () => provider.GetRelatedEntity<RefCusRateCode>(nkPropertyNamesAndValues));
		}

		[Test]
		public void GetRelatedEntity_MultipleMatches()
		{
			var safe = new Mock<ISafeRepository>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var nkPropertyNamesAndValues = new (string PropertyName, object PropertyValue)[] {
				("ZZ2_ZY1_NKRateCode", "1P1"),
				("ZZ2_ZY1_ZZR_NKRateType", "XXX"),
				("ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping", "ZA")
			};
			var rateType = new RefCusRateType { ZZR_RateType = "XXX", ZZR_ZZZ_NKDataGrouping = "ZA" };
			var rateCode1 = new RefCusRateCode { ZY1_RateCode = "1P1", RefCusRateType = rateType };
			var rateCode2 = new RefCusRateCode { ZY1_RateCode = "1P2", RefCusRateType = rateType };
			var rateCode3 = new RefCusRateCode { ZY1_RateCode = "1P1", RefCusRateType = rateType };
			safe.Setup(x => x.Get<RefCusRateType>()).Returns(new[] { rateType }.AsQueryable());
			safe.Setup(x => x.Get<RefCusRateCode>()).Returns(new[] { rateCode1, rateCode2, rateCode3 }.AsQueryable());
			var expectedMessage = $@"GetRelatedEntity returns multiple matches for RefCusRateCode.
NkPropertyNamesAndValues: ZZ2_ZY1_NKRateCode = 1P1; ZZ2_ZY1_ZZR_NKRateType = XXX; ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping = ZA.
ErrorCode: {ErrorCodes.MultipleOrNoneRelatedEntities}";
			Assert.Throws(Is.TypeOf<RefDataProcessingException>().And.Message.StartsWith(expectedMessage), () => provider.GetRelatedEntity<RefCusRateCode>(nkPropertyNamesAndValues));
		}

		[Test]
		public void GetRelatedEntity_CaseInsensitive()
		{
			var safe = new Mock<ISafeRepository>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var nkPropertyNamesAndValues = new (string PropertyName, object PropertyValue)[] {
				("ZZ2_ZY1_NKRateCode", "1p1"),
				("ZZ2_ZY1_ZZR_NKRateType", "XXx"),
				("ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping", "ZA")
			};
			var rateType = new RefCusRateType { ZZR_RateType = "XXX", ZZR_ZZZ_NKDataGrouping = "ZA" };
			var rateCode1 = new RefCusRateCode { ZY1_RateCode = "1P1", RefCusRateType = rateType };
			var rateCode2 = new RefCusRateCode { ZY1_RateCode = "1P2", RefCusRateType = rateType };
			safe.Setup(x => x.Get<RefCusRateType>()).Returns(new[] { rateType }.AsQueryable());
			safe.Setup(x => x.Get<RefCusRateCode>()).Returns(new[] { rateCode1, rateCode2 }.AsQueryable());

			Assert.That(provider.GetRelatedEntity<RefCusRateCode>(nkPropertyNamesAndValues), Is.EqualTo(rateCode1));
		}

		[Test]
		public void GetRelatedEntity_MultipleGroups()
		{
			var safe = new Mock<ISafeRepository>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var nkPropertyNamesAndValues1 = new (string PropertyName, object PropertyValue)[] {
				("ZZT_ZZA_NKTradeGroup", "A"),
				("ZZT_ZZA_ZZZ_NKDataGrouping", "AU"),
			};
			var nkPropertyNamesAndValues2 = new (string PropertyName, object PropertyValue)[] {
				("ZZT_ZZA_NKSecondTradeGroup", "B"),
				("ZZT_ZZA_ZZZ_NKDataGrouping", "CA"),
			};
			var tradegroup1 = new RefCusTradeGroup { ZZA_TradeGroup = "A", ZZA_ZZZ_NKDataGrouping = "AU" };
			var tradegroup2 = new RefCusTradeGroup { ZZA_TradeGroup = "B", ZZA_ZZZ_NKDataGrouping = "CA" };
			safe.Setup(x => x.Get<RefCusTradeGroup>()).Returns(new[] { tradegroup1, tradegroup2 }.AsQueryable());

			Assert.That(provider.GetRelatedEntity<RefCusTradeGroup>(nkPropertyNamesAndValues1), Is.EqualTo(tradegroup1));
			Assert.That(provider.GetRelatedEntity<RefCusTradeGroup>(nkPropertyNamesAndValues2), Is.EqualTo(tradegroup2));
		}

		[Test]
		public void GetRelatedEntity_Null()
		{
			var safe = new Mock<ISafeRepository>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var nkPropertyNamesAndValues = new (string PropertyName, object PropertyValue)[] {
				("ZZ2_ZY1_NKRateCode", string.Empty),
				("ZZ2_ZY1_ZZR_NKRateType", string.Empty),
				("ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping", string.Empty)
			};
			Assert.IsNull(provider.GetRelatedEntity<RefCusRateCode>(nkPropertyNamesAndValues));
		}

		[Test]
		public void GetRelatedEntity_4Levels()
		{
			var startDate1 = new DateTime(2024, 1, 1).ToUTCDateTimeOffset();
			var startDate2 = new DateTime(2025, 1, 1).ToUTCDateTimeOffset();
			var tariffType1 = new RefCusTariffType { ZZI_TariffType = "TT1", ZZI_ZZZ_NKDataGrouping = "AU" };
			var tariffType2 = new RefCusTariffType { ZZI_TariffType = "TT2", ZZI_ZZZ_NKDataGrouping = "AU" };
			var profileType1 = new RefCusProfileType { XXX_PK = Guid.NewGuid(), XXX_ProfileType = "PT1", XXX_ZZZ_NKDataGrouping = "AU", RefCusTariffType = tariffType1 };
			var profileType2 = new RefCusProfileType { XXX_PK = Guid.NewGuid(), XXX_ProfileType = "PT1", XXX_ZZZ_NKDataGrouping = "AU", RefCusTariffType = tariffType2 };
			var profileQuestion1 = new RefCusProfileQuestion { XQ2_QuestionCode = "AA1", XQ2_ZZZ_NKDataGrouping = "AU", XQ2_StartDate = startDate1, XQ2_XXX_ProfileType = profileType1.XXX_PK, RefCusProfileType = profileType1 };
			var profileQuestion2 = new RefCusProfileQuestion { XQ2_QuestionCode = "AA1", XQ2_ZZZ_NKDataGrouping = "AU", XQ2_StartDate = startDate1, XQ2_XXX_ProfileType = profileType2.XXX_PK, RefCusProfileType = profileType2 };
			var profileQuestion3 = new RefCusProfileQuestion { XQ2_QuestionCode = "BB1", XQ2_ZZZ_NKDataGrouping = "ZA", XQ2_StartDate = startDate2, XQ2_XXX_ProfileType = profileType1.XXX_PK, RefCusProfileType = profileType1 };
			var profileQuestion4 = new RefCusProfileQuestion { XQ2_QuestionCode = "BB1", XQ2_ZZZ_NKDataGrouping = "ZA", XQ2_StartDate = startDate2, XQ2_XXX_ProfileType = profileType2.XXX_PK, RefCusProfileType = profileType2 };
			var safe = new Mock<ISafeRepository>();
			safe.Setup(x => x.Get<RefCusTariffType>()).Returns(new[] { tariffType1, tariffType2 }.AsQueryable());
			safe.Setup(x => x.Get<RefCusProfileType>()).Returns(new[] { profileType1, profileType2 }.AsQueryable());
			safe.Setup(x => x.Get<RefCusProfileQuestion>()).Returns(new[] { profileQuestion1, profileQuestion2, profileQuestion3, profileQuestion4 }.AsQueryable());
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var nkPropertyNamesAndValues1 = new (string PropertyName, object PropertyValue)[] {
							("XQP_XQ2_NKQuestionParent", "AA1"),
							("XQP_XQ2_NKQuestionStartDateParent", startDate1.DateTime),
							("XQP_XQ2_ZZZ_NKDataGroupingParent", "AU"),
							("XQP_XQ2_XXX_NKProfileType", "PT1"),
							("XQP_XQ2_XXX_ZZZ_NKDataGrouping", "AU"),
							("XQP_XQ2_XXX_ZZI_NKTariffType", "TT1"),
							("XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping", "AU")
						};
			var nkPropertyNamesAndValues2 = new (string PropertyName, object PropertyValue)[] {
							("XQP_XQ2_NKQuestionChild", "BB1"),
							("XQP_XQ2_NKQuestionStartDateChild", startDate2.DateTime),
							("XQP_XQ2_ZZZ_NKDataGroupingChild", "ZA"),
							("XQP_XQ2_XXX_NKProfileType", "PT1"),
							("XQP_XQ2_XXX_ZZZ_NKDataGrouping", "AU"),
							("XQP_XQ2_XXX_ZZI_NKTariffType", "TT1"),
							("XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping", "AU")
						};
			Assert.That(provider.GetRelatedEntity<RefCusProfileQuestion>(nkPropertyNamesAndValues1), Is.EqualTo(profileQuestion1));
			Assert.That(provider.GetRelatedEntity<RefCusProfileQuestion>(nkPropertyNamesAndValues2), Is.EqualTo(profileQuestion3));
		}

		[Test]
		public void GetRelatedEntity_ThrowExceptionIfMoreThan4Levels()
		{
			var safe = new Mock<ISafeRepository>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var columnName = "ZZ2_ZY1_ZZR_ZZ1_ZZI_NKTariffType";
			var nkPropertyNamesAndValues = new (string PropertyName, object PropertyValue)[] { (columnName, "AA") };
			var exception = Assert.Throws<RefDataProcessingException>(() => provider.GetRelatedEntity<RefCusRateCode>(nkPropertyNamesAndValues));
			Assert.True(exception.Message.StartsWith($"{columnName} has more than 4 levels and it's not supported."));
			Assert.True(exception.Message.Contains($"ErrorCode: {ErrorCodes.NotSupportedNKColumn}"));

			columnName = "XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping";
			nkPropertyNamesAndValues = new (string PropertyName, object PropertyValue)[] { (columnName, "AA") };
			exception = Assert.Throws<RefDataProcessingException>(() => provider.GetRelatedEntity<RefCusProfileQuestion>(nkPropertyNamesAndValues));
			Assert.False(exception.Message.Contains($"ErrorCode: {ErrorCodes.NotSupportedNKColumn}"));
			Assert.True(exception.Message.Contains($"ErrorCode: {ErrorCodes.MultipleOrNoneRelatedEntities}"));
		}

		[Test]
		public void GetData()
		{
			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var tarifTypePK = Guid.NewGuid();
			var tariff1 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var tariff2 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_StartDate = new DateTime(2017, 06, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var tariff3 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_StartDate = new DateTime(2018, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = Guid.NewGuid()
			};
			var tariff4 = new RefCusTariff
			{
				ZZ1_TariffCode = "002",
				ZZ1_StartDate = new DateTime(2018, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			safe.Setup(x => x.GetWithOptimizedExpand<RefCusTariff>()).Returns(new[] { tariff1, tariff2, tariff3, tariff4 }.AsQueryable());
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusTariff)))
				.Returns(new KeyProperty[] { new KeyProperty { Name = nameof(Stage.RefCusTariff.ZZ1_TariffCode) },
					new KeyProperty { Name = nameof(Stage.RefCusTariff.ZZ1_ZZI_NKTariffType) } });
			var wrapper1 = new Mock<IStagingDataWrapper>();
			wrapper1.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_TariffCode))).Returns("001");
			wrapper1.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_ZZI_TariffType))).Returns(tarifTypePK);
			var wrapper2 = new Mock<IStagingDataWrapper>();
			wrapper2.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_TariffCode))).Returns("002");
			wrapper2.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_ZZI_TariffType))).Returns(tarifTypePK);
			var result = provider.GetData<RefCusTariff>(new[] { wrapper1.Object, wrapper2.Object }, metadata.Object).ToArray();
			Assert.That(result, Contains.Item(tariff1));
			Assert.That(result, Contains.Item(tariff2));
			Assert.That(result, Contains.Item(tariff4));
		}

		[Test]
		public void GetDataWithStartsWithOperation()
		{
			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var tarifTypePK = Guid.NewGuid();
			var tariff1 = new RefCusTariff
			{
				ZZ1_TariffCode = "123456",
				ZZ1_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var tariff2 = new RefCusTariff
			{
				ZZ1_TariffCode = "123897",
				ZZ1_StartDate = new DateTime(2017, 06, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var tariff3 = new RefCusTariff
			{
				ZZ1_TariffCode = "992193",
				ZZ1_StartDate = new DateTime(2018, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var tariff4 = new RefCusTariff
			{
				ZZ1_TariffCode = "999123",
				ZZ1_StartDate = new DateTime(2018, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			safe.Setup(x => x.GetWithOptimizedExpand<RefCusTariff>()).Returns(new[] { tariff1, tariff2, tariff3, tariff4 }.AsQueryable());
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusTariff)))
				.Returns(new KeyProperty[] { new KeyProperty
				{
					Name = nameof(Stage.RefCusTariff.ZZ1_TariffCode),
					Operation = Operations.StartsWith
				}, new KeyProperty { Name = nameof(Stage.RefCusTariff.ZZ1_ZZI_NKTariffType) } });

			var wrapper1 = new Mock<IStagingDataWrapper>();
			wrapper1.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_TariffCode))).Returns("123");
			wrapper1.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_ZZI_TariffType))).Returns(tarifTypePK);
			var result = provider.GetData<RefCusTariff>(new[] { wrapper1.Object }, metadata.Object).ToArray();
			Assert.That(result, Contains.Item(tariff1));
			Assert.That(result, Contains.Item(tariff2));
		}

		[Test]
		public void GetDataIgnoresRelatedEntitiesAsKeys()
		{
			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var tarifTypePK = Guid.NewGuid();
			var tariff1 = new RefCusTariff
			{
				ZZ1_TariffCode = "123456",
				ZZ1_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			safe.Setup(x => x.GetWithOptimizedExpand<RefCusTariff>()).Returns(new[] { tariff1 }.AsQueryable());
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusTariff)))
				.Returns(new KeyProperty[] { new KeyProperty
				{
					Name = nameof(Stage.RefCusTariff.ZZ1_TariffCode),
					Operation = Operations.StartsWith
				} , new KeyProperty { Name = nameof(Stage.RefCusTariff.ZZ1_ZZI_NKTariffType) },
					new KeyProperty { Name = nameof(Stage.RefCusTariffRelationship) }
			});

			var wrapper1 = new Mock<IStagingDataWrapper>();
			wrapper1.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_TariffCode))).Returns("123");
			wrapper1.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_ZZI_TariffType))).Returns(tarifTypePK);
			var result = provider.GetData<RefCusTariff>(new[] { wrapper1.Object }, metadata.Object).ToArray();
			Assert.That(result, Contains.Item(tariff1));
		}

		[Test]
		public void GetDataWithMultipleKeys()
		{
			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var airline1 = new RefAirline
			{
				RM_EagleAddedAirlinePrefixOrAccountingCode = "001",
				RM_ThreeLetterCode = "AA1",
				RM_AirlineName1 = "Name1"
			};
			var airline2 = new RefAirline
			{
				RM_EagleAddedAirlinePrefixOrAccountingCode = "002",
				RM_ThreeLetterCode = "AA2",
				RM_AirlineName1 = "Name2"
			};
			var airline3 = new RefAirline
			{
				RM_EagleAddedAirlinePrefixOrAccountingCode = "003",
				RM_ThreeLetterCode = "AA3",
				RM_AirlineName1 = "Name3"
			};
			safe.Setup(x => x.GetWithOptimizedExpand<RefAirline>()).Returns(new[] { airline1, airline2, airline3 }.AsQueryable());
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefAirline.RM_EagleAddedAirlinePrefixOrAccountingCode))).Returns("001");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefAirline.RM_ThreeLetterCode))).Returns("AA2");
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefAirline))).Returns(new KeyProperty[0]);
			var result = provider.GetData<RefAirline>(new[] { wrapper.Object }, metadata.Object).ToArray();
			Assert.AreEqual(0, result.Length);

			metadata.Setup(x => x.GetKeys(nameof(Stage.RefAirline)))
				.Returns(new KeyProperty[] { new KeyProperty { Name = nameof(RefAirline.RM_EagleAddedAirlinePrefixOrAccountingCode), Order = 0 }, new KeyProperty { Name = nameof(RefAirline.RM_ThreeLetterCode), Order = 1 } });
			result = provider.GetData<RefAirline>(new[] { wrapper.Object }, metadata.Object).ToArray();
			Assert.AreEqual(2, result.Length);
			Assert.True(result.Contains(airline1));
			Assert.True(result.Contains(airline2));
		}

		[Test]
		public void GetDataInOneExecution()
		{
			var airline1 = new RefAirline
			{
				RM_EagleAddedAirlinePrefixOrAccountingCode = "001",
				RM_ThreeLetterCode = "AA1",
				RM_AirlineName1 = "Name1"
			};
			var airline2 = new RefAirline
			{
				RM_EagleAddedAirlinePrefixOrAccountingCode = "002",
				RM_ThreeLetterCode = "AA2",
				RM_AirlineName1 = "Name2"
			};
			var queryableAirlines = new[] { airline1, airline2 }.AsQueryable();
			var queryProvider = new Mock<IQueryProvider>();
			queryProvider.SetupSequence(x => x.CreateQuery<RefAirline>(It.IsAny<Expression>())).Returns(queryableAirlines).Returns(queryableAirlines.Take(1));
			var airlineQuery = new Mock<IQueryable<RefAirline>>();
			airlineQuery.Setup(x => x.Provider).Returns(queryProvider.Object);
			airlineQuery.Setup(x => x.Expression).Returns(queryableAirlines.Expression);

			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			safe.Setup(x => x.GetWithOptimizedExpand<RefAirline>()).Returns(airlineQuery.Object);
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefAirline.RM_EagleAddedAirlinePrefixOrAccountingCode))).Returns("001");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefAirline.RM_ThreeLetterCode))).Returns("AA2");
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefAirline)))
				.Returns(new KeyProperty[] { new KeyProperty { Name = nameof(RefAirline.RM_EagleAddedAirlinePrefixOrAccountingCode), Order = 0 }, new KeyProperty { Name = nameof(RefAirline.RM_ThreeLetterCode), Order = 1 } });

			var result = provider.GetData<RefAirline>(new[] { wrapper.Object }, metadata.Object).ToArray();
			Assert.AreEqual(2, result.Length);
		}

		[Test]
		public void Create()
		{
			var wrapper = new Mock<IStagingDataWrapper>();
			var safe = new Mock<ISafeRepository>();
			var metadataProvider = new Mock<IMetadataProvider>();
			var tariffTypePK = Guid.NewGuid();
			wrapper.Setup(x => x.GetWrapperValue(It.IsAny<string>())).Returns(null);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_TariffCode))).Returns("001");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_StartDate))).Returns(new DateTime(2017, 01, 01));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_ZZI_TariffType))).Returns(tariffTypePK);
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var result = provider.Create<RefCusTariff>(wrapper.Object, null, new List<object>(), metadataProvider.Object);
			Assert.That(result.ZZ1_TariffCode, Is.EqualTo("001"));
			Assert.That(result.ZZ1_StartDate, Is.EqualTo(new DateTime(2017, 01, 01).ToUTCDateTimeOffset()));
			Assert.That(result.ZZ1_ZZI_TariffType, Is.EqualTo(tariffTypePK));
			safe.Verify(x => x.Add(result));
		}

		[Test]
		public void CreateTariffWithPublishedDate()
		{
			var metadataProvider = new Mock<IMetadataProvider>();
			var wrapper = new Mock<IStagingDataWrapper>();
			var safe = new Mock<ISafeRepository>();
			var tariffTypePK = Guid.NewGuid();
			wrapper.Setup(x => x.GetWrapperValue(It.IsAny<string>())).Returns(null);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_TariffCode))).Returns("001");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_StartDate))).Returns(new DateTime(2017, 01, 01));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_ZZI_TariffType))).Returns(tariffTypePK);
			PublishDateProvider.SetPublishedDate(new DateTime(2015, 1, 1));
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var result = provider.Create<RefCusTariff>(wrapper.Object, null, new List<object>(), metadataProvider.Object);
			Assert.That(result.ZZ1_TariffCode, Is.EqualTo("001"));
			Assert.That(result.ZZ1_StartDate, Is.EqualTo(new DateTime(2017, 01, 01).ToUTCDateTimeOffset()));
			Assert.That(result.ZZ1_ZZI_TariffType, Is.EqualTo(tariffTypePK));
			Assert.That(result.ZZ1_PublishedDate, Is.EqualTo(new Date(2015, 01, 01)));
			safe.Verify(x => x.Add(result));
		}

		[Test]
		public void CreateTariffWithIAmUnique()
		{
			var cacheProvider = CacheProvider.Create();
			var cacheProviderForOriginal = CacheProvider.Create();

			var metaText = @"<UniversalReferenceData><DataSource>CN Tariff 1</DataSource><PublicationTime>2021-09-16T00:00:00</PublicationTime><UpdateType>Partial</UpdateType><Schema><EntityType Name='RefCusTariff' Data='true'><Key><PropertyRef Name='ZZ1_TariffCode' /><PropertyRef Name='ZZ1_ZZI_NKTariffType' /><PropertyRef Name='ZZ1_ZZZ_NKDataGrouping' /><PropertyRef Name='RefCusTariffAttribute.ZZ3_Name' ConstantValue='CheckDigit' /><PropertyRef Name='RefCusTariffAttribute.ZZ3_Value' /><PropertyRef Name='RefCusTariffRelationship.ZZH_TariffCode' /></Key><Property Name='RefCusTariffAttribute' Type='RefCusTariffAttribute' /><Property Name='RefCusTariffRelationship' Type='RefCusTariffRelationship' /><Property Name='RefCusTariffUOM' Type='RefCusTariffUOM' /><Property Name='ZZ1_Description' Type='nvarchar' /><Property Name='ZZ1_EndDate' Type='datetime' DefaultValue='2079 - 06 - 06T23:
			59:00' /><Property Name='ZZ1_StartDate' Type='datetime' /><Property Name='ZZ1_TariffCode' Type='varchar' MaxLength='35' /><Property Name='ZZ1_ZZF_NKTaxOrFeeCode' Type='varchar' MaxLength='3' /><Property Name='ZZ1_ZZI_NKTariffType' Type='varchar' MaxLength='5' /><Property Name='ZZ1_ZZI_ZZZ_NKDataGrouping' Type='varchar' MaxLength='3' ConstantValue='CN' /><Property Name='ZZ1_ZZZ_NKDataGrouping' Type='varchar' MaxLength='3' ConstantValue='CN' /></EntityType><EntityType Name='RefCusTariffRelationship' Data='true'><Key><PropertyRef Name='ZZH_TariffCode' /><PropertyRef Name='ZZH_ZZI_NKTariffType' /><PropertyRef Name='ZZH_ZZI_ZZZ_NKDataGrouping' /></Key><Property Name='ZZH_TariffCode' Type='varchar' MaxLength='35' /><Property Name='ZZH_ZZI_NKTariffType' Type='varchar' MaxLength='5' ConstantValue='HSN' /><Property Name='ZZH_ZZI_ZZZ_NKDataGrouping' Type='varchar' MaxLength='3' ConstantValue='CN' /></EntityType><EntityType Name='RefCusTariffUOM' Data='true'><Key><PropertyRef Name='ZZ8_Type' /><PropertyRef Name='ZZ8_ZZZ_NKDataGrouping' /></Key><Property Name='ZZ8_Type' Type='varchar' MaxLength='3' /><Property Name='ZZ8_UOM' Type='varchar' MaxLength='10' /><Property Name='ZZ8_ZZZ_NKDataGrouping' Type='varchar' MaxLength='3' ConstantValue='CN' /></EntityType><EntityType Name='RefCusTariffAttribute' Data='true'><Key><PropertyRef Name='ZZ3_Name' /></Key><Property Name='ZZ3_Name' Type='varchar' MaxLength='50' /><Property Name='ZZ3_Value' Type='nvarchar(max)' /></EntityType></Schema></UniversalReferenceData>";
			var metadata = new MetadataProvider(metaText, cacheProvider, cacheProviderForOriginal);

			var attWrapper = new Mock<IStagingDataWrapper>();
			attWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariffAttribute.ZZ3_Name))).Returns("CheckDigit");
			attWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariffAttribute.ZZ3_Value))).Returns("A");
			var wrapper = new Mock<IStagingDataWrapper>();
			var safe = new Mock<ISafeRepository>();
			var tariffTypePK = Guid.NewGuid();
			var tariff1 = new RefCusTariff
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_TariffCode = "123456",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				ZZ1_ZZI_TariffType = tariffTypePK,
				ZZ1_IAMUnique = 1
			};
			var tariff2 = new RefCusTariff
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_TariffCode = "123789",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				ZZ1_ZZI_TariffType = tariffTypePK,
				ZZ1_IAMUnique = 5
			};
			safe.Setup(x => x.Get<RefCusTariffAttribute>()).Returns(new RefCusTariffAttribute[0].AsQueryable());
			safe.Setup(x => x.Get<RefCusTariffRelationship>()).Returns(new RefCusTariffRelationship[0].AsQueryable());
			safe.Setup(x => x.Get<RefCusTariff>()).Returns(new RefCusTariff[] { tariff1, tariff2 }.AsQueryable());
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_TariffCode))).Returns("123456");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping))).Returns("ZA");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_ZZI_TariffType))).Returns(tariffTypePK);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_ZZI_TariffType))).Returns(tariffTypePK);
			wrapper.Setup(x => x.GetRelatedEntities(nameof(RefCusTariffAttribute))).Returns(new IStagingDataWrapper[] { attWrapper.Object });

			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var result = provider.Create<RefCusTariff>(wrapper.Object, null, new List<object>() { tariff1, tariff2 }, metadata);
			Assert.That(result.ZZ1_IAMUnique, Is.EqualTo(2));
		}

		[Test]
		public void CreateWithParent()
		{
			var metadataProvider = new Mock<IMetadataProvider>();
			var tariff = new RefCusTariff();
			var wrapper = new Mock<IStagingDataWrapper>();
			var safe = new Mock<ISafeRepository>();
			wrapper.Setup(x => x.GetWrapperValue(It.IsAny<string>())).Returns(null);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRate.ZZ2_RateFormula))).Returns("1+1");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRate.ZZ2_StartDate))).Returns(new DateTime(2017, 01, 01));
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var result = provider.Create<RefCusRate>(wrapper.Object, tariff, new List<object>(), metadataProvider.Object);
			Assert.That(result.ZZ2_RateFormula, Is.EqualTo("1+1"));
			Assert.That(result.ZZ2_StartDate, Is.EqualTo(new DateTime(2017, 01, 01).ToUTCDateTimeOffset()));
			Assert.That(tariff.RefCusRates.Contains(result));
		}

		[Test]
		public void CreateWithParent_NonPersistentObjects_ParentIsTariff()
		{
			var metadataProvider = new Mock<IMetadataProvider>();
			var tariff = new RefCusTariff();
			var wrapper = new Mock<IStagingDataWrapper>();
			var safe = new Mock<ISafeRepository>();
			wrapper.Setup(x => x.GetWrapperValue(It.IsAny<string>())).Returns(null);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRateApplicability.S01_AdditionalCode))).Returns("ABC");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRateApplicability.S01_StartDate))).Returns(new DateTime(2017, 01, 01));
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var result = provider.Create<RefCusRateApplicability>(wrapper.Object, tariff, new List<object>(), metadataProvider.Object);
			Assert.That(result.S01_AdditionalCode, Is.EqualTo("ABC"));
			Assert.That(result.S01_StartDate, Is.EqualTo(new DateTime(2017, 01, 01).ToUTCDateTimeOffset()));
			Assert.That(tariff.RefCusRateApplicabilities.Contains(result));
		}

		[Test]
		public void CreateWithParent_NonPersistentObjects_ParentIsTariffNationalCode()
		{
			var metadataProvider = new Mock<IMetadataProvider>();
			var tariffNationalCode = new RefCusTariffNationalCode();
			var wrapper = new Mock<IStagingDataWrapper>();
			var safe = new Mock<ISafeRepository>();
			wrapper.Setup(x => x.GetWrapperValue(It.IsAny<string>())).Returns(null);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRateApplicability.S01_AdditionalCode))).Returns("ABC");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRateApplicability.S01_StartDate))).Returns(new DateTime(2017, 01, 01));
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var result = provider.Create<RefCusRateApplicability>(wrapper.Object, tariffNationalCode, new List<object>(), metadataProvider.Object);
			Assert.That(result.S01_AdditionalCode, Is.EqualTo("ABC"));
			Assert.That(result.S01_StartDate, Is.EqualTo(new DateTime(2017, 01, 01).ToUTCDateTimeOffset()));
			Assert.That(tariffNationalCode.RefCusRateApplicabilities.Contains(result));
		}

		[Test]
		public void Expire()
		{
			var tariff = new RefCusTariff
			{
				ZZ1_EndDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset()
			};
			var wrapper = new Mock<IStagingDataWrapper>();
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(2018, 01, 01), new DateTime(2079, 06, 06));
			wrapper.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);
			var safe = new Mock<ISafeRepository>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			provider.Expire(tariff, wrapper.Object, new Mock<IMetadataProvider>().Object);
			Assert.AreEqual(new DateTime(2018, 01, 01).ToUTCDateTimeOffset(), tariff.ZZ1_EndDate);
			safe.Verify(x => x.Update(tariff));
		}

		[Test]
		public void Expire_RefCusTradeGroupCountry()
		{
			var tradeGroupCountry = new RefCusTradeGroupCountry
			{
				ZZB_EndDate = new Date(2027, 01, 01)
			};
			var wrapper = new Mock<IStagingDataWrapper>();
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(2018, 01, 01), new DateTime(2079, 06, 06));
			wrapper.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);
			var safe = new Mock<ISafeRepository>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			provider.Expire(tradeGroupCountry, wrapper.Object, new Mock<IMetadataProvider>().Object);
			Assert.AreEqual(new Date(2018, 01, 01), tradeGroupCountry.ZZB_EndDate);
			safe.Verify(x => x.Update(tradeGroupCountry));
		}

		[Test]
		public void Expire_WithExpirableChildren()
		{
			var rate = new RefCusRate { ZZ2_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			var trade1PK = Guid.NewGuid();
			var trade2PK = Guid.NewGuid();
			var app1 = new RefCusApplicability { ZZT_ZZA_TradeGroup = trade1PK, ZZT_StartDate = new DateTime(2000, 06, 06).ToUTCDateTimeOffset(), ZZT_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			var app2 = new RefCusApplicability { ZZT_ZZA_TradeGroup = trade2PK, ZZT_StartDate = new DateTime(2001, 06, 06).ToUTCDateTimeOffset(), ZZT_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			rate.RefCusApplicabilities.Add(app1);
			rate.RefCusApplicabilities.Add(app2);

			var appWrapper = new Mock<IStagingDataWrapper>();
			appWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusApplicability.ZZT_StartDate))).Returns(new DateTime(2018, 06, 27));
			appWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusApplicability.ZZT_ZZA_TradeGroup))).Returns(trade1PK);

			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRate.ZZ2_StartDate))).Returns(new DateTime(2018, 06, 27));
			wrapper.Setup(x => x.GetRelatedEntities(nameof(Stage.RefCusApplicability))).Returns(new[] { appWrapper.Object });
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(2018, 06, 27), new DateTime(2079, 06, 06));
			wrapper.Setup(x => x.GetDateTimeRange()).Returns(wrapperDateTimeRange);

			var metadata = new Mock<IMetadataProvider>();
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusRate))).Returns(new KeyProperty[] { new KeyProperty { Name = nameof(Stage.RefCusApplicability) } });
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusApplicability))).Returns(new KeyProperty[] { new KeyProperty { Name = nameof(Stage.RefCusApplicability.ZZT_ZZA_NKTradeGroup) } });

			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			provider.Expire(rate, wrapper.Object, metadata.Object);
			Assert.AreEqual(new DateTime(2018, 06, 27).ToUTCDateTimeOffset(), rate.ZZ2_EndDate);
			Assert.AreEqual(new DateTime(2079, 06, 06).ToUTCDateTimeOffset(), app1.ZZT_EndDate);
			Assert.AreEqual(new DateTime(2079, 06, 06).ToUTCDateTimeOffset(), app2.ZZT_EndDate);
			Assert.AreEqual(new DateTime(2000, 06, 06).ToUTCDateTimeOffset(), app1.ZZT_StartDate);
			Assert.AreEqual(new DateTime(2001, 06, 06).ToUTCDateTimeOffset(), app2.ZZT_StartDate);
		}

		[Test]
		public void GetRelatedData()
		{
			var rate = new RefCusRate { ZZ2_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			var app1 = new RefCusApplicability { ZZT_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			var app2 = new RefCusApplicability { ZZT_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			rate.RefCusApplicabilities.Add(app1);
			rate.RefCusApplicabilities.Add(app2);
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			var results = provider.GetRelatedData(rate, nameof(RefCusApplicability)).ToArray();
			Assert.AreEqual(app1, results[0]);
			Assert.AreEqual(app2, results[1]);
		}

		[Test]
		public void ShouldOverWriteAndNotExpire()
		{
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			Assert.False(provider.ShouldOverWriteAndNotExpire<RefCusTariff>());
			Assert.True(provider.ShouldOverWriteAndNotExpire<RefCusCodeList>());
			Assert.True(provider.ShouldOverWriteAndNotExpire<RefCusProcedure>());
			Assert.False(provider.ShouldOverWriteAndNotExpire<RefCusTariffNationalCode>());
		}

		[Test]
		public void IsExpirable()
		{
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			var safeDateTimeRange = new DateTimeRange(new DateTime(2016, 12, 31), new DateTime(2017, 12, 31));
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(2017, 01, 01), new DateTime(2017, 11, 30));
			Assert.False(provider.IsExpirable<RefCusTariffAttribute>(safeDateTimeRange, wrapperDateTimeRange));
			Assert.True(provider.IsExpirable<RefCusTariff>(safeDateTimeRange, wrapperDateTimeRange));
			safeDateTimeRange = new DateTimeRange(new DateTime(2017, 02, 02), new DateTime(2017, 12, 31));
			Assert.False(provider.IsExpirable<RefCusTariff>(safeDateTimeRange, wrapperDateTimeRange));
			safeDateTimeRange = new DateTimeRange(new DateTime(2016, 01, 31), new DateTime(2017, 12, 31));
			Assert.True(provider.IsExpirable<RefCusTariff>(safeDateTimeRange, wrapperDateTimeRange));
		}

		[Test]
		public void IsExpirable_NonExpirableTypes()
		{
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			var safeDateTimeRange = new DateTimeRange(new DateTime(2016, 12, 31), new DateTime(2017, 12, 31));
			var wrapperDateTimeRange = new DateTimeRange(new DateTime(2017, 01, 01), new DateTime(2017, 11, 30));
			Assert.False(provider.IsExpirable<RefCusCodeList>(safeDateTimeRange, wrapperDateTimeRange));
			Assert.That(provider.IsExpirable<RefCusTariffNationalCode>(safeDateTimeRange, wrapperDateTimeRange));
			safeDateTimeRange = new DateTimeRange(new DateTime(2017, 02, 02), new DateTime(2017, 12, 31));
			Assert.False(provider.IsExpirable<RefCusCodeList>(safeDateTimeRange, wrapperDateTimeRange));
			Assert.That(!provider.IsExpirable<RefCusTariffNationalCode>(safeDateTimeRange, wrapperDateTimeRange));
			safeDateTimeRange = new DateTimeRange(new DateTime(2016, 01, 31), new DateTime(2017, 12, 31));
			Assert.False(provider.IsExpirable<RefCusCodeList>(safeDateTimeRange, wrapperDateTimeRange));
			Assert.That(provider.IsExpirable<RefCusTariffNationalCode>(safeDateTimeRange, wrapperDateTimeRange));
		}

		[Test]
		public void GetIdenticalLevelHasChanges()
		{
			var tariff = new RefCusTariff
			{
				ZZ1_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_Description = "HELLO WORLD",
				ZZ1_IAMUnique = (short)2,
				ZZ1_TariffCode = "001"
			};
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_StartDate))).Returns(new DateTime(2017, 01, 01).ToUTCDateTimeOffset());
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_Description))).Returns("HELLO WORLD");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_IAMUnique))).Returns((short)2);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_TariffCode))).Returns("001");
			var metaDataProvider = new Mock<IMetadataProvider>();
			metaDataProvider.Setup(x => x.GetProperties(nameof(Stage.RefCusTariff))).Returns(new[]
			{
				nameof(Stage.RefCusTariff.ZZ1_StartDate),
				nameof(Stage.RefCusTariff.ZZ1_Description),
				nameof(Stage.RefCusTariff.ZZ1_IAMUnique),
				nameof(Stage.RefCusTariff.ZZ1_TariffCode)
			});
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			Assert.AreEqual((int)IdenticalLevel.Identical, provider.GetIdenticalLevel(tariff, wrapper.Object, metaDataProvider.Object));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_IAMUnique))).Returns((short)3);
			Assert.AreEqual((int)IdenticalLevel.HasChange, provider.GetIdenticalLevel(tariff, wrapper.Object, metaDataProvider.Object));
		}

		[Test]
		public void GetIdenticalLevelOnlyPublishedDateHasChanges()
		{
			var tariff = new RefCusTariff
			{
				ZZ1_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_PublishedDate = new Date(2018, 01, 01),
				ZZ1_IAMUnique = (short)2,
				ZZ1_TariffCode = "001"
			};
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_StartDate))).Returns(new DateTime(2017, 01, 01).ToUTCDateTimeOffset());
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_PublishedDate))).Returns(new DateTime(2018, 01, 01).ToUTCDateTimeOffset());
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_IAMUnique))).Returns((short)2);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_TariffCode))).Returns("001");
			var metaDataProvider = new Mock<IMetadataProvider>();
			metaDataProvider.Setup(x => x.GetProperties(nameof(Stage.RefCusTariff))).Returns(new[]
			{
				nameof(Stage.RefCusTariff.ZZ1_StartDate),
				nameof(Stage.RefCusTariff.ZZ1_IAMUnique),
				nameof(Stage.RefCusTariff.ZZ1_TariffCode)
			});
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			Assert.AreEqual((int)IdenticalLevel.Identical, provider.GetIdenticalLevel(tariff, wrapper.Object, metaDataProvider.Object));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_PublishedDate))).Returns(new DateTime(2019, 01, 01).ToUTCDateTimeOffset());
			Assert.AreEqual((int)IdenticalLevel.Identical, provider.GetIdenticalLevel(tariff, wrapper.Object, metaDataProvider.Object));
		}

		[Test]
		public void GetIdenticalLevelOnlyDescriptionHasChanges()
		{
			var tariff = new RefCusTariff
			{
				ZZ1_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_Description = "HELLO WORLD",
				ZZ1_IAMUnique = (short)2,
				ZZ1_TariffCode = "001"
			};
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_StartDate))).Returns(new DateTime(2017, 01, 01).ToUTCDateTimeOffset());
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_Description))).Returns("HELLO WORLD");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_IAMUnique))).Returns((short)2);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_TariffCode))).Returns("001");
			var metaDataProvider = new Mock<IMetadataProvider>();
			metaDataProvider.Setup(x => x.GetProperties(nameof(Stage.RefCusTariff))).Returns(new[]
			{
				nameof(Stage.RefCusTariff.ZZ1_StartDate),
				nameof(Stage.RefCusTariff.ZZ1_Description),
				nameof(Stage.RefCusTariff.ZZ1_IAMUnique),
				nameof(Stage.RefCusTariff.ZZ1_TariffCode)
			});
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			Assert.AreEqual((int)IdenticalLevel.Identical, provider.GetIdenticalLevel(tariff, wrapper.Object, metaDataProvider.Object));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_Description))).Returns("hello new world");
			Assert.AreEqual((int)IdenticalLevel.MainContentIdentical, provider.GetIdenticalLevel(tariff, wrapper.Object, metaDataProvider.Object));
		}

		[Test]
		public void GetIdenticalLevelDescriptionHasChanges_RefCusTradeGroupCountry()
		{
			var tradeGroupCountry = new RefCusTradeGroupCountry
			{
				ZZB_StartDate = new Date(2017, 01, 01),
				ZZB_EndDate = new Date(2079, 06, 06),
				ZZB_Description = "HELLO WORLD",
				ZZB_RN_NKTradeGroupCountryCode = "SS"
			};
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTradeGroupCountry.ZZB_StartDate))).Returns(new Date(2017, 01, 01));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTradeGroupCountry.ZZB_EndDate))).Returns(new Date(2079, 06, 06));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTradeGroupCountry.ZZB_Description))).Returns("HELLO WORLD");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode))).Returns("SS");
			var metaDataProvider = new Mock<IMetadataProvider>();
			metaDataProvider.Setup(x => x.GetProperties(nameof(Stage.RefCusTradeGroupCountry))).Returns(new[]
			{
				nameof(Stage.RefCusTradeGroupCountry.ZZB_StartDate),
				nameof(Stage.RefCusTradeGroupCountry.ZZB_EndDate),
				nameof(Stage.RefCusTradeGroupCountry.ZZB_Description),
				nameof(Stage.RefCusTradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode)
			});
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			Assert.AreEqual((int)IdenticalLevel.Identical, provider.GetIdenticalLevel(tradeGroupCountry, wrapper.Object, metaDataProvider.Object));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTradeGroupCountry.ZZB_Description))).Returns("hello new world");
			Assert.AreEqual((int)IdenticalLevel.HasChange, provider.GetIdenticalLevel(tradeGroupCountry, wrapper.Object, metaDataProvider.Object));
		}

		[Test]
		public void UpdateSafeObject()
		{
			var wrapper = new Mock<IStagingDataWrapper>();
			var safe = new Mock<ISafeRepository>();
			wrapper.Setup(x => x.GetWrapperValue(It.IsAny<string>())).Returns(null);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_Description))).Returns("HELLO WORLD");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_CompositeKeyOnZZ5))).Returns("1.1");
			wrapper.Setup(x => x.GetWrapperValue<DateTime?>(nameof(RefCusTariff.ZZ1_StartDate))).Returns(new DateTime(2018, 01, 01));

			var tariff = new RefCusTariff
			{
				ZZ1_Description = "HELLO",
				ZZ1_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_CompositeKeyOnZZ5 = "1.2"
			};
			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetUpdatableProperties(nameof(Stage.RefCusTariff), 0)).Returns(new[] { nameof(Stage.RefCusTariff.ZZ1_Description) });

			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			provider.Update(tariff, wrapper.Object, metadataProvider.Object, IdenticalLevel.HasChange);

			Assert.That(tariff.ZZ1_Description, Is.EqualTo("HELLO WORLD"));
			Assert.That(tariff.ZZ1_CompositeKeyOnZZ5, Is.EqualTo("1.2"));
			Assert.That(tariff.ZZ1_StartDate, Is.EqualTo(new DateTime(2017, 01, 01).ToUTCDateTimeOffset()));
		}

		[Test]
		public void UpdateNonExpirableSafeObject()
		{
			var wrapper = new Mock<IStagingDataWrapper>();
			var safe = new Mock<ISafeRepository>();
			wrapper.Setup(x => x.GetWrapperValue(It.IsAny<string>())).Returns(null);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusCodeList.ZZD_Description))).Returns("HELLO WORLD");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusCodeList.ZZD_StartDate))).Returns(new DateTime(2018, 01, 01));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusCodeList.ZZD_EndDate))).Returns(new DateTime(2018, 02, 01));

			var codelist = new RefCusCodeList
			{
				ZZD_Description = "HELLO",
				ZZD_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
				ZZD_EndDate = new DateTime(2017, 02, 01).ToUTCDateTimeOffset()
			};


			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetUpdatableProperties(nameof(Stage.RefCusCodeList), 0))
				.Returns(new[] { nameof(Stage.RefCusCodeList.ZZD_Description), nameof(Stage.RefCusCodeList.ZZD_StartDate), nameof(Stage.RefCusCodeList.ZZD_EndDate) });

			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			provider.Update(codelist, wrapper.Object, metadataProvider.Object, IdenticalLevel.HasChange);

			Assert.That(codelist.ZZD_Description, Is.EqualTo("HELLO WORLD"));
			Assert.That(codelist.ZZD_StartDate, Is.EqualTo(new DateTime(2018, 01, 01).ToUTCDateTimeOffset()));
			Assert.That(codelist.ZZD_EndDate, Is.EqualTo(new DateTime(2018, 02, 01).ToUTCDateTimeOffset()));
		}

		[Test]
		public void UpdateSafeObjectWithPublishedDate()
		{
			var wrapper = new Mock<IStagingDataWrapper>();
			var safe = new Mock<ISafeRepository>();
			wrapper.Setup(x => x.GetWrapperValue(It.IsAny<string>())).Returns(null);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_Description))).Returns("HELLO WORLD");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_CompositeKeyOnZZ5))).Returns("1.1");
			wrapper.Setup(x => x.GetWrapperValue<DateTime?>(nameof(RefCusTariff.ZZ1_StartDate))).Returns(new DateTime(2018, 01, 01));

			var tariff = new RefCusTariff
			{
				ZZ1_Description = "HELLO",
				ZZ1_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_CompositeKeyOnZZ5 = "1.2",
				ZZ1_PublishedDate = new Date(2010, 01, 01)
			};
			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetUpdatableProperties(nameof(Stage.RefCusTariff), 0)).Returns(new[] { nameof(Stage.RefCusTariff.ZZ1_Description) });
			PublishDateProvider.SetPublishedDate(new DateTime(2015, 1, 1));
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			provider.Update(tariff, wrapper.Object, metadataProvider.Object, IdenticalLevel.HasChange);

			Assert.That(tariff.ZZ1_Description, Is.EqualTo("HELLO WORLD"));
			Assert.That(tariff.ZZ1_CompositeKeyOnZZ5, Is.EqualTo("1.2"));
			Assert.That(tariff.ZZ1_StartDate, Is.EqualTo(new DateTime(2017, 01, 01).ToUTCDateTimeOffset()));
			Assert.That(tariff.ZZ1_PublishedDate, Is.EqualTo(new Date(2015, 01, 01)));
		}

		[Test]
		public void UpdateSafeObjectWithOnlyStartDateChange()
		{
			var wrapper = new Mock<IStagingDataWrapper>();
			var safe = new Mock<ISafeRepository>();
			wrapper.Setup(x => x.GetWrapperValue(It.IsAny<string>())).Returns(null);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_Description))).Returns("HELLO WORLD");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_CompositeKeyOnZZ5))).Returns("1.1");
			wrapper.Setup(x => x.GetWrapperValue<DateTime?>(nameof(RefCusTariff.ZZ1_StartDate))).Returns(new DateTime(2018, 01, 01));

			var tariff = new RefCusTariff
			{
				ZZ1_Description = "HELLO WORLD",
				ZZ1_StartDate = new DateTime(2019, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_CompositeKeyOnZZ5 = "1.1",
				ZZ1_PublishedDate = new Date(2010, 01, 01)
			};
			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetUpdatableProperties(nameof(Stage.RefCusTariff), 0)).Returns(new[] { nameof(Stage.RefCusTariff.ZZ1_Description) });
			PublishDateProvider.SetPublishedDate(new DateTime(2015, 1, 1));
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			provider.Update(tariff, wrapper.Object, metadataProvider.Object, IdenticalLevel.HasChange);

			Assert.That(tariff.ZZ1_Description, Is.EqualTo("HELLO WORLD"));
			Assert.That(tariff.ZZ1_CompositeKeyOnZZ5, Is.EqualTo("1.1"));
			Assert.That(tariff.ZZ1_StartDate, Is.EqualTo(new DateTime(2019, 01, 01).ToUTCDateTimeOffset()));
			Assert.That(tariff.ZZ1_PublishedDate, Is.EqualTo(new Date(2015, 01, 01)));
		}

		[Test]
		public void GetNewestObjectFromList_SpecifiedRelatedProperties()
		{
			var valueType1 = Guid.NewGuid();
			var valueType2 = Guid.NewGuid();
			var cond1 = new RefCusCondition();
			var cond2 = new RefCusCondition();
			cond1.RefCusConditionValues.Add(new RefCusConditionValue { ZX3_ZX4_ValueType = valueType1 });
			cond2.RefCusConditionValues.Add(new RefCusConditionValue { ZX3_ZX4_ValueType = valueType2 });

			var valueWrapper = new Mock<IStagingDataWrapper>();
			valueWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusConditionValue.ZX3_ZX4_ValueType))).Returns(valueType2);
			var condWrapper = new Mock<IStagingDataWrapper>();
			condWrapper.Setup(x => x.GetRelatedEntities(nameof(RefCusConditionValue))).Returns(new[] { valueWrapper.Object });

			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetKeys(nameof(Stage.RefCusCondition))).Returns(
				new[] { new KeyProperty { Name = "RefCusConditionValue.ZX3_ZX4_NKValueType" } });
			metadataProvider.Setup(x => x.EnableNullOrEmptyKeyMatching(nameof(Stage.RefCusCondition))).Returns(true);
			var safeObjs = new[] { cond1, cond2 };

			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			var result = provider.GetNewestObjectFromList<RefCusCondition>(safeObjs, condWrapper.Object, metadataProvider.Object);
			Assert.AreEqual(cond2, result[0][0]);
		}

		[Test]
		public void GetNewestObjectFromList_WithRelatedProperties()
		{
			var tradeGroupPK1 = Guid.NewGuid();
			var tradeGroupPK2 = Guid.NewGuid();
			var tradeGroupPK3 = Guid.NewGuid();
			var rate1 = new RefCusRate
			{
				ZZ2_RateFormula = "AB",
				ZZ2_StartDate = new DateTime(2000, 1, 1),
				ZZ2_EndDate = new DateTime(2001, 1, 1)
			};
			rate1.RefCusApplicabilities.Add(new RefCusApplicability { ZZT_ZZA_TradeGroup = tradeGroupPK1, ZZT_StartDate = new DateTime(2000, 1, 1), ZZT_EndDate = new DateTime(2001, 1, 1) });
			var rate2 = new RefCusRate
			{
				ZZ2_RateFormula = "AB",
				ZZ2_StartDate = new DateTime(2000, 1, 1),
				ZZ2_EndDate = new DateTime(2001, 1, 1)
			};
			rate2.RefCusApplicabilities.Add(new RefCusApplicability { ZZT_ZZA_TradeGroup = tradeGroupPK2, ZZT_StartDate = new DateTime(2000, 1, 1), ZZT_EndDate = new DateTime(2001, 1, 1) });
			var rate3 = new RefCusRate
			{
				ZZ2_RateFormula = "AB",
				ZZ2_StartDate = new DateTime(2000, 1, 1),
				ZZ2_EndDate = new DateTime(2001, 1, 1)
			};
			rate3.RefCusApplicabilities.Add(new RefCusApplicability { ZZT_ZZA_TradeGroup = tradeGroupPK3, ZZT_StartDate = new DateTime(2000, 1, 1), ZZT_EndDate = new DateTime(2001, 1, 1) });
			var safeObjs = new[] { rate1, rate2, rate3 };
			var appWrapper = new Mock<IStagingDataWrapper>();
			appWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusApplicability.ZZT_ZZA_TradeGroup))).Returns(tradeGroupPK2);
			var rateWrapper = new Mock<IStagingDataWrapper>();
			rateWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRate.ZZ2_RateFormula))).Returns("AB");
			rateWrapper.Setup(x => x.GetRelatedEntities(nameof(RefCusApplicability))).Returns(new[] { appWrapper.Object });

			var propertyNames = new KeyProperty[] {
				new KeyProperty { Name = nameof(Stage.RefCusRate.ZZ2_RateFormula) },
				new KeyProperty { Name = nameof(Stage.RefCusApplicability) } };

			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetKeys(nameof(Stage.RefCusApplicability))).Returns(new KeyProperty[] { new KeyProperty { Name = nameof(Stage.RefCusApplicability.ZZT_ZZA_NKTradeGroup) } });
			metadataProvider.Setup(x => x.GetKeys(nameof(Stage.RefCusRate))).Returns(propertyNames);

			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			var result = provider.GetNewestObjectFromList<RefCusRate>(safeObjs, rateWrapper.Object, metadataProvider.Object);
			Assert.AreEqual(rate2, result[0][0]);
		}

		[Test]
		public void RemoveMatchedRelatedObj()
		{
			var app1PK = Guid.NewGuid();
			var app2PK = Guid.NewGuid();
			var metadata = new Mock<IMetadataProvider>();
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusRate))).Returns(
				new[] { new KeyProperty { Name = nameof(Stage.RefCusApplicability) } });
			metadata.Setup(x => x.GetProperties(nameof(Stage.RefCusRate))).Returns(
				new[] { nameof(Stage.RefCusApplicability) });
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusApplicability))).Returns(
				new[] { new KeyProperty { Name = nameof(RefCusApplicability.ZZT_AdditionalCode) } });

			var appWrapper = new Mock<IStagingDataWrapper>();
			appWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusApplicability.ZZT_AdditionalCode))).Returns("AA");

			var safeRepo = new Mock<ISafeRepository>();

			var rate = new RefCusRate { ZZ2_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			var app1 = new RefCusApplicability { ZZT_PK = app1PK, ZZT_AdditionalCode = "AA", ZZT_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			var app2 = new RefCusApplicability { ZZT_PK = app2PK, ZZT_AdditionalCode = "BB", ZZT_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			rate.RefCusApplicabilities.Add(app1);
			rate.RefCusApplicabilities.Add(app2);

			var provider = new SafeDataProvider(safeRepo.Object, cacheProvider, overlappingCalculator);
			provider.RemoveMatchedRelatedObj<RefCusRate, RefCusApplicability>(rate, appWrapper.Object, metadata.Object);
			Assert.AreEqual(1, rate.RefCusApplicabilities.Count);
			Assert.AreEqual("BB", rate.RefCusApplicabilities.FirstOrDefault().ZZT_AdditionalCode);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetNewestObjectFromList(bool returnAllObjects)
		{
			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var tarifTypePK = Guid.NewGuid();
			var tariff1 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var tariff2 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_StartDate = new DateTime(2017, 06, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var tariff3 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_StartDate = new DateTime(2018, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = Guid.NewGuid()
			};
			var tariff4 = new RefCusTariff
			{
				ZZ1_TariffCode = "002",
				ZZ1_StartDate = new DateTime(2018, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var safeObjs = new[] { tariff1, tariff2, tariff3, tariff4 };
			safe.Setup(x => x.Get<RefCusTariff>()).Returns(new[] { tariff1, tariff2, tariff3, tariff4 }.AsQueryable());
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_TariffCode))).Returns("001");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_ZZI_TariffType))).Returns(tarifTypePK);
			var propertyNames = new KeyProperty[] {
				new KeyProperty { Name = nameof(Stage.RefCusTariff.ZZ1_TariffCode) },
				new KeyProperty { Name = nameof(Stage.RefCusTariff.ZZ1_ZZI_NKTariffType) } };
			metadata.Setup(x => x.GetKeys(nameof(RefCusTariff))).Returns(propertyNames);
			var result = provider.GetNewestObjectFromList<RefCusTariff>(safeObjs, wrapper.Object, metadata.Object, returnAllObjects);
			if (returnAllObjects)
			{
				Assert.That(result[0].Length == 2);
			}
			else
			{
				Assert.That(result[0].Length == 1);
				Assert.That(result[0].First(), Is.EqualTo(tariff2));
			}
		}

		[Test]
		public void GetSpecifiedDateTimeRangeObjectFromList()
		{
			var safe = new Mock<ISafeRepository>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var tarifTypePK = Guid.NewGuid();
			var tariff1 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_StartDate = new DateTime(2010, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_EndDate = new DateTime(2015, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var tariff2 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_StartDate = new DateTime(2016, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_EndDate = new DateTime(2020, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var wrapper = new Mock<IStagingDataWrapper>().Object;
			var metadata = new Mock<IMetadataProvider>().Object;
			var safeObjs = new[] { tariff1, tariff2 };
			var specifiedDateTimeRange = new DateTimeRange(new DateTime(2010, 01, 01), new DateTime(2015, 01, 01));
			var result = provider.GetSpecifiedDateTimeRangeObjectFromList<RefCusTariff>(safeObjs, specifiedDateTimeRange, wrapper, metadata);
			Assert.That(result == tariff1);
		}

		[Test]
		public void GetNewestObjectFromListWithStartsWithOperator()
		{
			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var tarifTypePK = Guid.NewGuid();
			var tariff1 = new RefCusTariff
			{
				ZZ1_TariffCode = "00100424",
				ZZ1_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var tariff2 = new RefCusTariff
			{
				ZZ1_TariffCode = "00100425",
				ZZ1_StartDate = new DateTime(2017, 06, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var tariff3 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_StartDate = new DateTime(2018, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = Guid.NewGuid()
			};
			var tariff4 = new RefCusTariff
			{
				ZZ1_TariffCode = "004032001",
				ZZ1_StartDate = new DateTime(2018, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var safeObjs = new[] { tariff1, tariff2, tariff3, tariff4 };
			safe.Setup(x => x.Get<RefCusTariff>()).Returns(new[] { tariff1, tariff2, tariff3, tariff4 }.AsQueryable());
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_TariffCode))).Returns("001");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_ZZI_TariffType))).Returns(tarifTypePK);
			var propertyNames = new KeyProperty[] { new KeyProperty
				{
					Name = nameof(Stage.RefCusTariff.ZZ1_TariffCode),
					Operation = Operations.StartsWith
				},
				new KeyProperty { Name = nameof(Stage.RefCusTariff.ZZ1_ZZI_NKTariffType) } };
			metadata.Setup(x => x.GetKeys(nameof(RefCusTariff))).Returns(propertyNames);

			var result = provider.GetNewestObjectFromList<RefCusTariff>(safeObjs, wrapper.Object, metadata.Object);
			Assert.That(result[0], !Contains.Item(tariff4));
		}

		[Test]
		public void GetNewestObjectFromListWithStartsWithOperatorGetNewestAndIgnoreExpired()
		{
			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var tarifTypePK = Guid.NewGuid();
			var tariff1 = new RefCusTariff
			{
				ZZ1_TariffCode = "010102",
				ZZ1_StartDate = new DateTime(1900, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_EndDate = new DateTime(2002, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var tariff2 = new RefCusTariff
			{
				ZZ1_TariffCode = "010102",
				ZZ1_StartDate = new DateTime(2002, 01, 02).ToUTCDateTimeOffset(),
				ZZ1_EndDate = new DateTime(2079, 01, 02).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var tariff3 = new RefCusTariff
			{
				ZZ1_TariffCode = "010103",
				ZZ1_StartDate = new DateTime(1900, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_EndDate = new DateTime(2003, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var tariff4 = new RefCusTariff
			{
				ZZ1_TariffCode = "010103",
				ZZ1_StartDate = new DateTime(2003, 01, 02).ToUTCDateTimeOffset(),
				ZZ1_EndDate = new DateTime(2079, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_ZZI_TariffType = tarifTypePK
			};
			var safeObjs = new[] { tariff1, tariff2, tariff3, tariff4 };
			safe.Setup(x => x.Get<RefCusTariff>()).Returns(new[] { tariff1, tariff2, tariff3, tariff4 }.AsQueryable());
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_TariffCode))).Returns("0101");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_ZZI_TariffType))).Returns(tarifTypePK);
			var propertyNames = new KeyProperty[] { new KeyProperty
				{
					Name = nameof(Stage.RefCusTariff.ZZ1_TariffCode),
					Operation = Operations.StartsWith
				},
				new KeyProperty { Name = nameof(Stage.RefCusTariff.ZZ1_ZZI_NKTariffType) } };
			metadata.Setup(x => x.GetKeys(nameof(RefCusTariff))).Returns(propertyNames);

			var result = provider.GetNewestObjectFromList<RefCusTariff>(safeObjs, wrapper.Object, metadata.Object);
			Assert.That(result[0], !Contains.Item(tariff1));
			Assert.That(result[0], Contains.Item(tariff2));
			Assert.That(result[0], !Contains.Item(tariff3));
			Assert.That(result[0], Contains.Item(tariff4));
		}

		[Test]
		public void GetNewestObjectFromList_ReturnAllError()
		{
			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var taxOrFeeTypeNK = "TST";
			var tax1 = new RefCusTaxOrFee
			{
				ZZF_Maximum = 12223,
				ZZF_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
				ZZF_ZX0_NKTaxOrFeeType = taxOrFeeTypeNK
			};

			var safeObjs = new[] { tax1 };
			safe.Setup(x => x.Get<RefCusTaxOrFee>()).Returns(new[] { tax1 }.AsQueryable());
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTaxOrFee.ZZF_Maximum))).Returns(12);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTaxOrFee.ZZF_ZX0_NKTaxOrFeeType))).Returns(taxOrFeeTypeNK);
			var propertyNames = new KeyProperty[] { new KeyProperty
				{
					Name = nameof(Stage.RefCusTaxOrFee.ZZF_Code),
					Operation = Operations.StartsWith
				},
				new KeyProperty { Name = nameof(Stage.RefCusTaxOrFee.ZZF_ZX0_NKTaxOrFeeType) } };
			metadata.Setup(x => x.GetKeys(nameof(RefCusTaxOrFee))).Returns(propertyNames);
			var expectedMessage = $@"Do not support 'return all' objects for key properties where Operation is other than 'Equals'.
Key properties other than equals: ZZF_Code StartsWith .
ErrorCode: {ErrorCodes.ReturnAllObjectsForOperationOtherThanEqual}";
			Assert.Throws(Is.TypeOf<RefDataProcessingException>().And.Message.StartsWith(expectedMessage), () => provider.GetNewestObjectFromList<RefCusTaxOrFee>(safeObjs, wrapper.Object, metadata.Object, true));
		}

		[Test]
		public void GetNewestObjectFromListWithStartsWithOperatorUsingNonStringKeyThrowsException()
		{
			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var taxOrFeeTypeNK = "TST";
			var tax1 = new RefCusTaxOrFee
			{
				ZZF_Maximum = 12223,
				ZZF_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
				ZZF_ZX0_NKTaxOrFeeType = taxOrFeeTypeNK
			};

			var safeObjs = new[] { tax1 };
			safe.Setup(x => x.Get<RefCusTaxOrFee>()).Returns(new[] { tax1 }.AsQueryable());
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTaxOrFee.ZZF_Maximum))).Returns(12);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTaxOrFee.ZZF_ZX0_NKTaxOrFeeType))).Returns(taxOrFeeTypeNK);
			var propertyNames = new KeyProperty[] { new KeyProperty
				{
					Name = nameof(Stage.RefCusTaxOrFee.ZZF_Maximum),
					Operation = Operations.StartsWith
				},
				new KeyProperty { Name = nameof(Stage.RefCusTaxOrFee.ZZF_ZX0_NKTaxOrFeeType) } };
			metadata.Setup(x => x.GetKeys(nameof(RefCusTaxOrFee))).Returns(propertyNames);

			Assert.Throws<InvalidOperationException>(() => provider.GetNewestObjectFromList<RefCusTaxOrFee>(safeObjs, wrapper.Object, metadata.Object));
		}

		[Test]
		public void GetIdenticalObjectFromList()
		{
			var tariffPK = Guid.NewGuid();
			var rateCodePK = Guid.NewGuid();
			var rate1 = new RefCusRate
			{
				ZZ2_ZZ1_Tariff = tariffPK,
				ZZ2_RateFormula = "0",
				ZZ2_ZY1_RateCode = rateCodePK,
				ZZ2_StartDate = new DateTime(2018, 07, 04).ToUTCDateTimeOffset(),
				ZZ2_EndDate = new DateTime(2079, 12, 31).ToUTCDateTimeOffset()
			};
			var rate2 = new RefCusRate
			{
				ZZ2_ZZ1_Tariff = tariffPK,
				ZZ2_RateFormula = "VFD * 0.1",
				ZZ2_ZY1_RateCode = rateCodePK,
				ZZ2_StartDate = new DateTime(2018, 07, 04).ToUTCDateTimeOffset(),
				ZZ2_EndDate = new DateTime(2079, 12, 31).ToUTCDateTimeOffset()
			};
			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusRate)))
				.Returns(new KeyProperty[] { new KeyProperty { Name = nameof(Stage.RefCusRate.ZZ2_ZY1_NKRateCode) },
				new KeyProperty { Name = nameof(Stage.RefCusRate.ZZ2_ZY1_ZZR_NKRateType) },
				new KeyProperty { Name = nameof(Stage.RefCusRate.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping) },
				new KeyProperty { Name = nameof(Stage.RefCusApplicability) } });
			metadata.Setup(x => x.GetProperties(nameof(Stage.RefCusRate)))
					.Returns(new[] { nameof(Stage.RefCusRate.ZZ2_ZY1_NKRateCode),
				nameof(Stage.RefCusRate.ZZ2_ZY1_ZZR_NKRateType),
				nameof(Stage.RefCusRate.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping),
				nameof(Stage.RefCusApplicability),
				nameof(Stage.RefCusRate.ZZ2_RateFormula) });

			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRate.ZZ2_ZZ1_Tariff))).Returns(tariffPK);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRate.ZZ2_ZY1_RateCode))).Returns(rateCodePK);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRate.ZZ2_StartDate))).Returns(new DateTime(2018, 07, 04));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRate.ZZ2_EndDate))).Returns(new DateTime(2079, 12, 31));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRate.ZZ2_RateFormula))).Returns("0");

			var result = provider.GetIdenticalObjectFromList<RefCusRate>(new object[] { rate1, rate2 }, wrapper.Object, metadata.Object);
			Assert.AreEqual(rate1, result);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRate.ZZ2_RateFormula))).Returns("VFD * 0.1");
			result = provider.GetIdenticalObjectFromList<RefCusRate>(new object[] { rate1, rate2 }, wrapper.Object, metadata.Object);
			Assert.AreEqual(rate2, result);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRate.ZZ2_RateFormula))).Returns("VFD * 0.2");
			result = provider.GetIdenticalObjectFromList<RefCusRate>(new object[] { rate1, rate2 }, wrapper.Object, metadata.Object);
			Assert.IsNull(result);
		}

		[Test]
		public void GetIdenticalObjectFromList_WithRelatedProperties()
		{
			var tariffPK = Guid.NewGuid();
			var rateCodePK = Guid.NewGuid();
			var condition1 = new RefCusCondition
			{
				ZX1_ZZ1_Tariff = tariffPK,
				ZX1_Source = "XXX",
				ZX1_Comment = "X",
				ZX1_StartDate = new DateTime(2018, 07, 04).ToUTCDateTimeOffset(),
				ZX1_EndDate = new DateTime(2079, 12, 31).ToUTCDateTimeOffset(),
			};

			var conditionvalue1 = new RefCusConditionValue
			{
				ZX3_ZX1_Condition = condition1.ZX1_PK,
				ZX3_Value = "0"
			};

			var condition2 = new RefCusCondition
			{
				ZX1_ZZ1_Tariff = tariffPK,
				ZX1_Source = "XXX",
				ZX1_Comment = "X",
				ZX1_StartDate = new DateTime(2018, 07, 04).ToUTCDateTimeOffset(),
				ZX1_EndDate = new DateTime(2079, 12, 31).ToUTCDateTimeOffset(),
			};

			var conditionvalue2 = new RefCusConditionValue
			{
				ZX3_ZX1_Condition = condition1.ZX1_PK,
				ZX3_Value = "1"
			};

			var valuesMock = new Mock<Microsoft.OData.Client.DataServiceCollection<RefCusConditionValue>>();
			var values = valuesMock.As<IEnumerable<RefCusConditionValue>>();
			values.Setup(x => x.GetEnumerator()).Returns(new[] { conditionvalue1 }.Cast<RefCusConditionValue>().GetEnumerator());
			condition1.RefCusConditionValues = valuesMock.Object;

			valuesMock = new Mock<Microsoft.OData.Client.DataServiceCollection<RefCusConditionValue>>();
			values = valuesMock.As<IEnumerable<RefCusConditionValue>>();
			values.Setup(x => x.GetEnumerator()).Returns(new[] { conditionvalue2 }.Cast<RefCusConditionValue>().GetEnumerator());
			condition2.RefCusConditionValues = valuesMock.Object;


			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusCondition)))
				.Returns(new KeyProperty[] { new KeyProperty { Name = nameof(Stage.RefCusCondition.ZX1_Source) },
				new KeyProperty { Name = nameof(Stage.RefCusConditionValue) } });
			metadata.Setup(x => x.GetProperties(nameof(Stage.RefCusCondition)))
					.Returns(new[] { nameof(Stage.RefCusCondition.ZX1_Comment),
				nameof(Stage.RefCusCondition.ZX1_Source),
				nameof(Stage.RefCusConditionValue) });

			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusCondition.ZX1_ZZ1_Tariff))).Returns(tariffPK);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusCondition.ZX1_Source))).Returns("XXX");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusCondition.ZX1_StartDate))).Returns(new DateTime(2018, 07, 04));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusCondition.ZX1_EndDate))).Returns(new DateTime(2079, 12, 31));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusCondition.ZX1_Comment))).Returns("X");

			var valueWrapper = new Mock<IStagingDataWrapper>();
			valueWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusConditionValue.ZX3_Value))).Returns("0");
			wrapper.Setup(x => x.GetRelatedEntities(nameof(RefCusConditionValue))).Returns(new[] { valueWrapper.Object });

			var result = provider.GetIdenticalObjectFromList<RefCusCondition>(new object[] { condition1, condition2 }, wrapper.Object, metadata.Object);
			Assert.AreEqual(condition1, result);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusConditionValue.ZX3_Value))).Returns("1");
			result = provider.GetIdenticalObjectFromList<RefCusCondition>(new object[] { condition1, condition2 }, wrapper.Object, metadata.Object);
			Assert.AreEqual(condition2, result);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusConditionValue.ZX3_Value))).Returns("2");
			result = provider.GetIdenticalObjectFromList<RefCusCondition>(new object[] { condition1, condition2 }, wrapper.Object, metadata.Object);
			Assert.Null(result);
		}

		[Test]
		public void GetIdenticalObjectFromList_WithRelatedProperties_CountNotMatch()
		{
			var tariffPK = Guid.NewGuid();
			var rateCodePK = Guid.NewGuid();
			var condition1 = new RefCusCondition
			{
				ZX1_ZZ1_Tariff = tariffPK,
				ZX1_Source = "XXX",
				ZX1_Comment = "X",
				ZX1_StartDate = new DateTime(2018, 07, 04).ToUTCDateTimeOffset(),
				ZX1_EndDate = new DateTime(2079, 12, 31).ToUTCDateTimeOffset(),
			};

			var conditionvalue1 = new RefCusConditionValue
			{
				ZX3_ZX1_Condition = condition1.ZX1_PK,
				ZX3_Value = "0"
			};

			var conditionvalue2 = new RefCusConditionValue
			{
				ZX3_ZX1_Condition = condition1.ZX1_PK,
				ZX3_Value = "1"
			};

			var valuesMock = new Mock<Microsoft.OData.Client.DataServiceCollection<RefCusConditionValue>>();
			var values = valuesMock.As<IEnumerable<RefCusConditionValue>>();
			values.Setup(x => x.GetEnumerator()).Returns(new[] { conditionvalue1, conditionvalue2 }.Cast<RefCusConditionValue>().GetEnumerator());
			condition1.RefCusConditionValues = valuesMock.Object;


			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			metadata.Setup(x => x.GetKeys(nameof(Stage.RefCusCondition)))
				.Returns(new KeyProperty[] { new KeyProperty { Name = nameof(Stage.RefCusCondition.ZX1_Source) },
				new KeyProperty { Name = nameof(Stage.RefCusConditionValue) } });
			metadata.Setup(x => x.GetProperties(nameof(Stage.RefCusCondition)))
					.Returns(new[] { nameof(Stage.RefCusCondition.ZX1_Comment),
				nameof(Stage.RefCusCondition.ZX1_Source),
				nameof(Stage.RefCusConditionValue) });

			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusCondition.ZX1_ZZ1_Tariff))).Returns(tariffPK);
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusCondition.ZX1_Source))).Returns("XXX");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusCondition.ZX1_StartDate))).Returns(new DateTime(2018, 07, 04));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusCondition.ZX1_EndDate))).Returns(new DateTime(2079, 12, 31));
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefCusCondition.ZX1_Comment))).Returns("X");

			var valueWrapper = new Mock<IStagingDataWrapper>();
			valueWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusConditionValue.ZX3_Value))).Returns("0");
			wrapper.Setup(x => x.GetRelatedEntities(nameof(RefCusConditionValue))).Returns(new[] { valueWrapper.Object });

			var result = provider.GetIdenticalObjectFromList<RefCusCondition>(new object[] { condition1 }, wrapper.Object, metadata.Object);
			Assert.Null(result);
		}

		[Test]
		public void GetNewestObjectFromList_WithConvertToSafeValue()
		{
			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var tarifTypePK = Guid.NewGuid();
			var rate = new RefExchangeRateZZ
			{
				ZZN_RX_NKExCurrency = "USD",
				ZZN_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
			};
			var safeObjs = new[] { rate };
			safe.Setup(x => x.Get<RefExchangeRateZZ>()).Returns(new[] { rate }.AsQueryable());
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefExchangeRateZZ.ZZN_RX_NKExCurrency))).Returns("USD");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefExchangeRateZZ.ZZN_StartDate))).Returns(new DateTime(2017, 01, 01));
			var propertyNames = new KeyProperty[] {
				new KeyProperty { Name = nameof(RefExchangeRateZZ.ZZN_RX_NKExCurrency) },
				new KeyProperty { Name = nameof(RefExchangeRateZZ.ZZN_StartDate) } };
			metadata.Setup(x => x.GetKeys(nameof(RefExchangeRateZZ))).Returns(propertyNames);

			var result = provider.GetNewestObjectFromList<RefExchangeRateZZ>(safeObjs, wrapper.Object, metadata.Object);
			Assert.That(result[0].Length == 1);
			Assert.That(result[0].First(), Is.EqualTo(rate));
		}

		[Test]
		public void GetNewestObjectFromList_WithRelatedProperties_WithConstantValue_MatchesParentWithUnmatchedChild()
		{
			var tariffNationalCodePK = Guid.NewGuid();
			var tariffPK = Guid.NewGuid();

			var tariff = new RefCusTariff
			{
				ZZ1_Description = "AB",
				ZZ1_TariffCode = "0103",
				ZZ1_PK = tariffPK
			};

			tariff.RefCusTariffAttributes.Add(new RefCusTariffAttribute { ZZ3_ZZ1_Tariff = tariff.ZZ1_PK, ZZ3_Name = "AnythingElse", ZZ3_ZZW_TariffNationalCode = tariffNationalCodePK, ZZ3_Value = "3" });

			var safeObjs = new[] { tariff };
			var relatedWrapper = new Mock<IStagingDataWrapper>();
			relatedWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariffAttribute.ZZ3_ZZW_TariffNationalCode))).Returns(tariffNationalCodePK);
			relatedWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariffAttribute.ZZ3_Name))).Returns("Another");
			relatedWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariffAttribute.ZZ3_Value))).Returns("3");

			var propertyNames = new KeyProperty[] {
				new KeyProperty { Name = nameof(Stage.RefCusTariff.ZZ1_Description) },
				new KeyProperty
				{
					Name = $"{nameof(Stage.RefCusTariffAttribute)}.{nameof(Stage.RefCusTariffAttribute.ZZ3_Name)}",
					ConstantValue = "CheckDigit"
				},
				new KeyProperty { Name = $"{nameof(Stage.RefCusTariffAttribute)}.{nameof(Stage.RefCusTariffAttribute.ZZ3_Value)}", }
			};

			var tariffWrapper = new Mock<IStagingDataWrapper>();
			tariffWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_Description))).Returns("AB");
			tariffWrapper.Setup(x => x.GetRelatedEntities(nameof(RefCusTariffAttribute))).Returns(new[] { relatedWrapper.Object });

			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetKeys(nameof(Stage.RefCusTariffAttribute))).Returns(new KeyProperty[] { new KeyProperty { Name = nameof(Stage.RefCusTariffAttribute.ZZ3_ZZW_TariffNationalCode) } });
			metadataProvider.Setup(x => x.GetKeys(nameof(Stage.RefCusTariff))).Returns(propertyNames);

			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			var result = provider.GetNewestObjectFromList<RefCusTariff>(safeObjs, tariffWrapper.Object, metadataProvider.Object);

			Assert.AreEqual(1, result[0].Length);
			Assert.AreEqual(tariff.ZZ1_TariffCode, result[0][0].ZZ1_TariffCode);
			Assert.AreEqual(1, result[0][0].RefCusTariffAttributes.Count);
		}

		[Test]
		public void GetNewestObjectFromList_WithRelatedProperties_CountNotMatch()
		{
			var tariffNationalCodePK = Guid.NewGuid();
			var tariffPK = Guid.NewGuid();

			var tariff = new RefCusTariff
			{
				ZZ1_Description = "AB",
				ZZ1_TariffCode = "0103",
				ZZ1_PK = tariffPK
			};

			tariff.RefCusTariffAttributes.Add(new RefCusTariffAttribute { ZZ3_ZZ1_Tariff = tariff.ZZ1_PK, ZZ3_Name = "CheckDigit", ZZ3_ZZW_TariffNationalCode = tariffNationalCodePK, ZZ3_Value = "1" });
			tariff.RefCusTariffAttributes.Add(new RefCusTariffAttribute { ZZ3_ZZ1_Tariff = tariff.ZZ1_PK, ZZ3_Name = "CheckDigit", ZZ3_ZZW_TariffNationalCode = tariffNationalCodePK, ZZ3_Value = "2" });

			var safeObjs = new[] { tariff };
			var relatedWrapper = new Mock<IStagingDataWrapper>();
			relatedWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariffAttribute.ZZ3_ZZW_TariffNationalCode))).Returns(tariffNationalCodePK);
			relatedWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariffAttribute.ZZ3_Name))).Returns("CheckDigit");
			relatedWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariffAttribute.ZZ3_Value))).Returns("1");

			var tariffWrapper = new Mock<IStagingDataWrapper>();
			tariffWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusTariff.ZZ1_Description))).Returns("AB");
			tariffWrapper.Setup(x => x.GetRelatedEntities(nameof(RefCusTariffAttribute))).Returns(new[] { relatedWrapper.Object });

			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetKeys(nameof(Stage.RefCusTariffAttribute))).Returns(new KeyProperty[] { new KeyProperty { Name = nameof(Stage.RefCusTariffAttribute.ZZ3_ZZW_TariffNationalCode) } });
			metadataProvider.Setup(x => x.GetKeys(nameof(Stage.RefCusTariff))).Returns(new KeyProperty[] { new KeyProperty { Name = nameof(RefCusTariffAttribute) } });

			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			var result = provider.GetNewestObjectFromList<RefCusTariff>(safeObjs, tariffWrapper.Object, metadataProvider.Object);

			Assert.AreEqual(0, result.Count);
		}

		[Test]
		public void GetNewestObjectFromList_WithOrder()
		{
			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var airlinePK1 = Guid.NewGuid();
			var airlinePK2 = Guid.NewGuid();
			var airline1 = new RefAirline
			{
				RM_PK = airlinePK1,
				RM_EagleAddedAirlinePrefixOrAccountingCode = "001",
				RM_ThreeLetterCode = "AA1",
				RM_AirlineName1 = "Name1"
			};
			var airline2 = new RefAirline
			{
				RM_PK = airlinePK2,
				RM_EagleAddedAirlinePrefixOrAccountingCode = "002",
				RM_ThreeLetterCode = "AA2",
				RM_AirlineName1 = "Name2"
			};
			var airline3 = new RefAirline
			{
				RM_EagleAddedAirlinePrefixOrAccountingCode = "",
				RM_ThreeLetterCode = "AA3",
				RM_AirlineName1 = "Name3"
			};
			var airline4 = new RefAirline
			{
				RM_EagleAddedAirlinePrefixOrAccountingCode = "",
				RM_ThreeLetterCode = "",
				RM_AirlineName1 = "Name4"
			};
			var safeObjs = new[] { airline1, airline2, airline3, airline4 };
			safe.Setup(x => x.Get<RefAirline>()).Returns(safeObjs.AsQueryable());

			var propertyNames = new KeyProperty[]
			{
				new KeyProperty { Name = nameof(RefAirline.RM_EagleAddedAirlinePrefixOrAccountingCode), Order = 0 },
				new KeyProperty { Name = nameof(RefAirline.RM_ThreeLetterCode), Order = 1 },
				new KeyProperty { Name = nameof(RefAirline.RM_AirlineName1), Order = 2 }
			};
			metadata.Setup(x => x.GetKeys(nameof(RefAirline))).Returns(propertyNames);
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefAirline.RM_EagleAddedAirlinePrefixOrAccountingCode))).Returns("001");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefAirline.RM_ThreeLetterCode))).Returns("AA2");
			var result = provider.GetNewestObjectFromList<RefAirline>(safeObjs, wrapper.Object, metadata.Object, false);
			Assert.AreEqual(2, result.Keys.Count);
			Assert.True(result.ContainsKey(0));
			Assert.True(result.ContainsKey(1));
			Assert.AreEqual(1, result[0].Length);
			Assert.AreEqual("001", result[0][0].RM_EagleAddedAirlinePrefixOrAccountingCode);

			wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefAirline.RM_EagleAddedAirlinePrefixOrAccountingCode))).Returns("");
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefAirline.RM_AirlineName1))).Returns("Name4");
			safe.Invocations.Clear();
			result = provider.GetNewestObjectFromList<RefAirline>(safeObjs, wrapper.Object, metadata.Object, false);
			Assert.True(result.ContainsKey(2));
			Assert.AreEqual(1, result[2].Length);

			wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefAirline.RM_ThreeLetterCode))).Returns("BBB");
			safe.Invocations.Clear();
			result = provider.GetNewestObjectFromList<RefAirline>(safeObjs, wrapper.Object, metadata.Object, false);
			Assert.AreEqual(0, result.Keys.Count);

			wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(RefAirline.RM_ThreeLetterCode))).Returns("");
			safe.Invocations.Clear();
			result = provider.GetNewestObjectFromList<RefAirline>(safeObjs, wrapper.Object, metadata.Object, false);
			Assert.AreEqual(0, result.Keys.Count);
		}

		[Test]
		public void GetNewestObjectFromList_EnableNullOrEmptyKeyMatching()
		{
			var rate1 = new RefCusRate { ZZ2_ZZZ_NKDataGrouping = "ZA", ZZ2_RateFormula = "AB" };
			rate1.RefCusApplicabilities.Add(new RefCusApplicability());
			var appWrapper = new Mock<IStagingDataWrapper>();
			appWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusApplicability.ZZT_ZZA_TradeGroup))).Returns(null);
			var rateWrapper = new Mock<IStagingDataWrapper>();
			rateWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRate.ZZ2_RateFormula))).Returns("AB");
			rateWrapper.Setup(x => x.GetWrapperValue(nameof(RefCusRate.ZZ2_ZZZ_NKDataGrouping))).Returns("ZA");
			rateWrapper.Setup(x => x.GetRelatedEntities(nameof(RefCusApplicability))).Returns(new[] { appWrapper.Object });

			var propertyNames = new KeyProperty[] {
				new KeyProperty { Name = nameof(Stage.RefCusRate.ZZ2_ZZZ_NKDataGrouping) },
				new KeyProperty { Name = nameof(Stage.RefCusRate.ZZ2_RateFormula) },
				new KeyProperty { Name = nameof(Stage.RefCusApplicability) }
			};

			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetKeys(nameof(Stage.RefCusRate))).Returns(propertyNames);
			metadataProvider.Setup(x => x.GetKeys(nameof(Stage.RefCusApplicability))).Returns(new KeyProperty[] { new KeyProperty { Name = nameof(Stage.RefCusApplicability.ZZT_ZZA_NKTradeGroup) } });

			metadataProvider.Setup(x => x.EnableNullOrEmptyKeyMatching(nameof(RefCusApplicability))).Returns(true);
			var provider = new SafeDataProvider(new Mock<ISafeRepository>().Object, cacheProvider, overlappingCalculator);
			var result = provider.GetNewestObjectFromList<RefCusRate>(new[] { rate1 }, rateWrapper.Object, metadataProvider.Object);
			Assert.Greater(result.Count, 0);
			Assert.AreEqual(1, result[0].Length);

			metadataProvider.Setup(x => x.EnableNullOrEmptyKeyMatching(nameof(RefCusApplicability))).Returns(false);
			result = provider.GetNewestObjectFromList<RefCusRate>(new[] { rate1 }, rateWrapper.Object, metadataProvider.Object);
			Assert.AreEqual(0, result.Count);
		}

		[Test]
		public void GetNewestObjectFromListEnableExpirableWithNullDate()
		{
			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var refCusTariffUOMTradeGroup = Guid.NewGuid();
			var refCusTariffUOM = new SafeDataClient.RefCusTariffUOM
			{
				ZZ8_PK = Guid.NewGuid(),
				ZZ8_StartDate = null,
				ZZ8_EndDate = null,
				ZZ8_Type = "CU2",
				ZZ8_UOM = "NAR",
				ZZ8_ZZZ_NKDataGrouping = "FR",
				ZZ8_ZZA_TradeGroup = refCusTariffUOMTradeGroup,
				ZZ8_ZZA_SecondTradeGroup = null
			};
			var safeObjs = new[] { refCusTariffUOM };
			safe.Setup(x => x.Get<SafeDataClient.RefCusTariffUOM>()).Returns(new[] { refCusTariffUOM }.AsQueryable());
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(SafeDataClient.RefCusTariffUOM.ZZ8_Type))).Returns("CU2");
			wrapper.Setup(x => x.GetWrapperValue(nameof(SafeDataClient.RefCusTariffUOM.ZZ8_UOM))).Returns("NAR");
			wrapper.Setup(x => x.GetWrapperValue(nameof(SafeDataClient.RefCusTariffUOM.ZZ8_ZZZ_NKDataGrouping))).Returns("FR");
			wrapper.Setup(x => x.GetWrapperValue(nameof(SafeDataClient.RefCusTariffUOM.ZZ8_ZZA_TradeGroup))).Returns(refCusTariffUOMTradeGroup);
			var propertyNames = new KeyProperty[] {
				new KeyProperty { Name = nameof(SafeDataClient.RefCusTariffUOM.ZZ8_Type) },
				new KeyProperty { Name = nameof(SafeDataClient.RefCusTariffUOM.ZZ8_UOM) },
				new KeyProperty { Name = nameof(SafeDataClient.RefCusTariffUOM.ZZ8_ZZZ_NKDataGrouping) },
				new KeyProperty { Name = nameof(SafeDataClient.RefCusTariffUOM.ZZ8_ZZA_TradeGroup) },
				new KeyProperty { Name = nameof(SafeDataClient.RefCusTariffUOM.ZZ8_ZZA_SecondTradeGroup) }
			};
			metadata.Setup(x => x.GetKeys(nameof(SafeDataClient.RefCusTariffUOM))).Returns(propertyNames);

			DataProviderHelper.SetEnableExpirable(true);
			var expectedMessage = $@"Property ZZ8_StartDate in {typeof(SafeDataClient.RefCusTariffUOM).Name} has to be type DateTimeOffset or Edm.Date and not null.";
			Assert.Throws(Is.TypeOf<RefDataProcessingException>().And.Message.StartsWith(expectedMessage), () => provider.GetNewestObjectFromList<SafeDataClient.RefCusTariffUOM>(safeObjs, wrapper.Object, metadata.Object));
		}


		[Test]
		public void GetNewestObjectFromListEnableExpirableWithMultipleDate()
		{
			var safe = new Mock<ISafeRepository>();
			var metadata = new Mock<IMetadataProvider>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var refCusTariffUOM1PK = Guid.NewGuid();
			var refCusTariffUOM1TradeGroup = Guid.NewGuid();
			var refCusTariffUOM2PK = Guid.NewGuid();
			var refCusTariffUOM1 = new SafeDataClient.RefCusTariffUOM
			{
				ZZ8_PK = refCusTariffUOM1PK,
				ZZ8_StartDate = null,
				ZZ8_EndDate = null,
				ZZ8_Type = "CU2",
				ZZ8_UOM = "NAR",
				ZZ8_ZZZ_NKDataGrouping = "US",
				ZZ8_ZZA_TradeGroup = Guid.NewGuid(),
				ZZ8_ZZA_SecondTradeGroup = null
			};
			var refCusTariffUOM2 = new SafeDataClient.RefCusTariffUOM
			{
				ZZ8_PK = refCusTariffUOM2PK,
				ZZ8_StartDate = new DateTime(2017, 01, 01).ToUTCDateTimeOffset(),
				ZZ8_EndDate = new DateTime(2018, 01, 01).ToUTCDateTimeOffset(),
				ZZ8_Type = "CU2",
				ZZ8_UOM = "NAR",
				ZZ8_ZZZ_NKDataGrouping = "FR",
				ZZ8_ZZA_TradeGroup = refCusTariffUOM1TradeGroup,
				ZZ8_ZZA_SecondTradeGroup = null
			};
			var safeObjs = new[] { refCusTariffUOM1, refCusTariffUOM2 };
			safe.Setup(x => x.Get<SafeDataClient.RefCusTariffUOM>()).Returns(new[] { refCusTariffUOM1, refCusTariffUOM2 }.AsQueryable());
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue(nameof(SafeDataClient.RefCusTariffUOM.ZZ8_Type))).Returns("CU2");
			wrapper.Setup(x => x.GetWrapperValue(nameof(SafeDataClient.RefCusTariffUOM.ZZ8_UOM))).Returns("NAR");
			wrapper.Setup(x => x.GetWrapperValue(nameof(SafeDataClient.RefCusTariffUOM.ZZ8_ZZZ_NKDataGrouping))).Returns("FR");
			wrapper.Setup(x => x.GetWrapperValue(nameof(SafeDataClient.RefCusTariffUOM.ZZ8_ZZA_TradeGroup))).Returns(refCusTariffUOM1TradeGroup);
			var propertyNames = new KeyProperty[] {
				new KeyProperty { Name = nameof(SafeDataClient.RefCusTariffUOM.ZZ8_Type) },
				new KeyProperty { Name = nameof(SafeDataClient.RefCusTariffUOM.ZZ8_UOM) },
				new KeyProperty { Name = nameof(SafeDataClient.RefCusTariffUOM.ZZ8_ZZZ_NKDataGrouping) },
				new KeyProperty { Name = nameof(SafeDataClient.RefCusTariffUOM.ZZ8_ZZA_TradeGroup) },
				new KeyProperty { Name = nameof(SafeDataClient.RefCusTariffUOM.ZZ8_ZZA_SecondTradeGroup) }
			};
			metadata.Setup(x => x.GetKeys(nameof(SafeDataClient.RefCusTariffUOM))).Returns(propertyNames);

			DataProviderHelper.SetEnableExpirable(true);
			var result = provider.GetNewestObjectFromList<SafeDataClient.RefCusTariffUOM>(safeObjs, wrapper.Object, metadata.Object);

			Assert.That(result[0], Contains.Item(refCusTariffUOM2));
		}

		[Test]
		public void Delete()
		{
			var safe = new Mock<ISafeRepository>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var safeObj = new RefCusTariff { ZZ1_TariffCode = "AAA" };
			provider.Delete(safeObj);
			safe.Verify(x => x.Delete(safeObj));
		}

		[Test]
		public void GetDateTimeRange_RefCusTariff()
		{
			var tariff = new RefCusTariff
			{
				ZZ1_StartDate = new DateTime(2016, 01, 01).ToUTCDateTimeOffset(),
				ZZ1_EndDate = new DateTime(2018, 01, 01).ToUTCDateTimeOffset()
			};
			var wrapper = new Mock<IStagingDataWrapper>().Object;
			var metadata = new Mock<IMetadataProvider>().Object;
			var safe = new Mock<ISafeRepository>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var result = provider.GetDateTimeRange(tariff);
			Assert.That(result.StartDate, Is.EqualTo(tariff.ZZ1_StartDate));
			Assert.That(result.EndDate, Is.EqualTo(tariff.ZZ1_EndDate));
		}

		[Test]
		public void GetDateTimeRange_RefCusTradeGroupCountry()
		{
			var edmStartDate = new Date(2024, 1, 1);
			var edmEndDate = new Date(2024, 12, 31);
			var country = new RefCusTradeGroupCountry
			{
				ZZB_StartDate = edmStartDate,
				ZZB_EndDate = edmEndDate
			};
			var wrapper = new Mock<IStagingDataWrapper>().Object;
			var metadata = new Mock<IMetadataProvider>().Object;
			var safe = new Mock<ISafeRepository>();
			var provider = new SafeDataProvider(safe.Object, cacheProvider, overlappingCalculator);
			var result = provider.GetDateTimeRange(country);
			Assert.That(result.StartDate, Is.EqualTo(((DateTime)edmStartDate).ToUTCDateTimeOffset()));
			Assert.That(result.EndDate, Is.EqualTo(((DateTime)edmEndDate).ToUTCDateTimeOffset()));
		}

		[Test]
		public async Task CloneExistingRecordChildrenIntoNewRecordAsync()
		{
			var cloneObjects = new[] { new CloneProcessObject { ExpiredRecordPk = Guid.Empty, NewRecordPk = Guid.Empty } };
			var safeRepo = new Mock<ISafeRepository>();
			var safeProvider = new SafeDataProvider(safeRepo.Object, cacheProvider, overlappingCalculator);
			await safeProvider.CloneExistingRecordChildrenIntoNewRecord<RefCusTariff>(cloneObjects);
			safeRepo.Verify(x => x.CloneExistingRecordChildrenIntoNewRecord<RefCusTariff>(cloneObjects), Times.Once);
		}

		[Test]
		public void DeleteConflictedRecordsByOrder()
		{
			var pk1 = Guid.NewGuid();
			var pk2 = Guid.NewGuid();
			var pk3 = Guid.NewGuid();
			var objectDictionary = new Dictionary<int, IEnumerable<object>>();
			objectDictionary.Add(0, new[] { new RefCusTariff { ZZ1_PK = pk1, ZZ1_TariffCode = "001" } });
			objectDictionary.Add(1, new[] { new RefCusTariff { ZZ1_PK = pk2, ZZ1_TariffCode = "002" } });
			objectDictionary.Add(2, new[] { new RefCusTariff { ZZ1_PK = pk3, ZZ1_TariffCode = "003" } });

			var safeRepo = new Mock<ISafeRepository>();
			var safeProvider = new SafeDataProvider(safeRepo.Object, cacheProvider, overlappingCalculator);
			var deletedPKs = safeProvider.DeleteConflictedRecordsByOrder<RefCusTariff>(0, objectDictionary);
			Assert.AreEqual(2, deletedPKs.Count());
			CollectionAssert.AreEquivalent(new[] { pk2, pk3 }, deletedPKs);
		}

		[Test]
		public void SavePersistentObjects()
		{
			var mockSafeRepo = new Mock<ISafeRepository>();
			var safeDataProvider = new SafeDataProvider(mockSafeRepo.Object, cacheProvider, overlappingCalculator);
			safeDataProvider.SavePersistentObjects();
			mockSafeRepo.Verify(x => x.SavePersistentObjects(), Times.Once());
		}

		[Test]
		public void GetAllPersistentObjects()
		{
			var mockSafeRepo = new Mock<ISafeRepository>();
			var safeDataProvider = new SafeDataProvider(mockSafeRepo.Object, cacheProvider, overlappingCalculator);
			safeDataProvider.GetAllPersistentObjects();
			mockSafeRepo.Verify(x => x.GetAllPersistentObjects(), Times.Once());
		}

		ICacheProvider cacheProvider;
		IOverlappingCalculator overlappingCalculator;

		[SetUp]
		public void SetUp()
		{
			var cacheProviderMock = new Mock<ICacheProvider>();
			cacheProviderMock.SetupGet(x => x.RelatedEntities).Returns(() => new ConcurrentDictionary<string, Lazy<object[]>>());
			cacheProviderMock.SetupGet(x => x.RelatedEntityCache).Returns(() => new ConcurrentDictionary<string, Lazy<object>>());
			cacheProviderMock.SetupGet(x => x.RelatedTypeAndNKPropertyNamesCache).Returns(() => new ConcurrentDictionary<string, Lazy<Tuple<Type, string, string[]>[]>>());
			cacheProviderMock.SetupGet(x => x.SafeKeysCache).Returns(() => new ConcurrentDictionary<string, Lazy<KeyProperty[]>>());
			cacheProviderMock.SetupGet(x => x.TableCodeAndTypeCache).Returns(() => new ConcurrentDictionary<string, Lazy<Type>>());
			cacheProviderMock.SetupGet(x => x.KeyExpirableTypeCache).Returns(() => new ConcurrentDictionary<string, Lazy<Type>>());
			cacheProvider = cacheProviderMock.Object;
			overlappingCalculator = new OverlappingCalculator(false);
		}
	}
}
