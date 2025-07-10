using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class CachedTradeLinesSummaryProviderTest : TestCaseWithFactory
	{
		void CreateRelatedItems(OrgHeader org)
		{
			var collection = new OrgSalesCollection(Factory);
			var collectionHelper = new OrgSalesCollectionTestHelper(collection, Factory);

			var newQuote = Factory.New(ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().QuoteType);
			newQuote[RatingHeaderSchema.TH_OH.Name] = org.PK;
			collectionHelper.AddRateEntry(newQuote, "AIR", "AUSYD", "USLAX", ZGuid.Empty, ZGuid.Empty);
			collectionHelper.AddRateEntry(newQuote, "LCL", "AUSYD", "USLAX", org.PK, ZGuid.Empty);
			collectionHelper.AddRateEntry(newQuote, "FCL", "GBLON", "AUMEL", ZGuid.Empty, org.PK);
			collectionHelper.AddRateEntry(newQuote, "FCL", "", "AUMEL", org.PK, org.PK);
			collectionHelper.AddRateEntry(newQuote, "SCO", "AUSYD", "USLAX", ZGuid.Empty, ZGuid.Empty);
			collectionHelper.AddRateEntry(newQuote, "SNC", "AUSYD", "USLAX", ZGuid.Empty, ZGuid.Empty);
			Factory.Save();
		}

		[TestDate(2014, 12, 10)]
		public void TestCacheIsUsed()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORGXXX";
			org.OH_RL_NKClosestPort = "AUBNE";
			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.RunJCDServiceTaskForTableCreation(Db.Connection);

			using (var summary = new CachedTradeLinesSummaryProvider(Db.Connection, new ZDate(2014, 1, 1), new ZDate(2015, 1, 1)))
			{
				Assert("PRE: Before creation, there is no related trade statuss", !summary.GetForOrg(org, new ZDate(2014, 1, 1), new ZDate(2015, 1, 1)).ProspectValues.Any());

				//Create some related stuff
				CreateRelatedItems(org);

				Assert("The cache is used, so should still see no related objects", !summary.GetForOrg(org, new ZDate(2014, 1, 1), new ZDate(2015, 1, 1)).ProspectValues.Any());
			}

			using (var summary = new CachedTradeLinesSummaryProvider(Db.Connection, new ZDate(2014, 1, 1), new ZDate(2015, 1, 1)))
			{
				Assert("Cache has been recreated, and should not be empty", summary.GetForOrg(org, new ZDate(2014, 1, 1), new ZDate(2015, 1, 1)).ProspectValues.Any());
			}
		}

		public void TestThereShouldBeIndexesOnOrgColumns()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORGXXX";
			org.OH_RL_NKClosestPort = "AUBNE";
			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.RunJCDServiceTaskForTableCreation(Db.Connection);

			using (var summary = new CachedTradeLinesSummaryProvider(Db.Connection, new ZDate(2014, 1, 1), new ZDate(2015, 1, 1)))
			{
				summary.GetForOrg(org, new ZDate(2014, 1, 1), new ZDate(2015, 1, 1));

				AssertArrayEqualsByElements(new[] { "MainOrg" }, GetTempTableIndexColumns(CachedTradeLinesSummaryProvider.TradeLineCacheTableName, "TradeLinesSummaryCache_MainOrg").ToArray());
			}
		}

		IEnumerable<string> GetTempTableIndexColumns(string tableName, string indexName)
		{
			var query = @"
SELECT col.name
FROM tempdb.sys.indexes ind
JOIN tempdb.sys.index_columns ind_col on ind.object_id = ind_col.object_id and ind.index_id = ind_col.index_id
JOIN tempdb.sys.columns col on ind_col.object_id = col.object_id and ind_col.column_id = col.column_id
WHERE
	ind.object_id = OBJECT_ID(@TableName)
	AND ind.name = @IndexName
ORDER BY
	ind_col.index_column_id ASC";

			using (var command = TestConnection.Command(query))
			{
				command.AddParameter("@TableName", SqlDbType.VarChar, "tempdb.." + tableName);
				command.AddParameter("@IndexName", SqlDbType.VarChar, indexName);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var columnName = (string)reader["name"];
						yield return columnName;
					}
				}
			}
		}

		public void TestFromAndToDatesDifferentFromCached()
		{
			var insertSql = @"
DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany)
DECLARE @Branch UNIQUEIDENTIFIER = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch WHERE GB_GC = @Company)
DECLARE @Department UNIQUEIDENTIFIER = (SELECT TOP 1 GE_PK FROM dbo.GlbDepartment)
DECLARE @ChargeCode UNIQUEIDENTIFIER = (SELECT TOP 1 AC_PK FROM dbo.AccChargeCode WHERE AC_ChargeType != 'DSB')
DECLARE @Org UNIQUEIDENTIFIER = 'E8C9E76C-C515-405B-A5AC-177170F2644B'
DECLARE @OrgAddress UNIQUEIDENTIFIER = 'BA83A4BE-6519-4675-8FA5-8BC4249174AE'

DECLARE @Shipment2013 UNIQUEIDENTIFIER = '481EE6B2-C9C4-4739-BAEA-E610A8F713A2';
DECLARE @Shipment2014 UNIQUEIDENTIFIER = 'DA7C9467-156D-4D3F-B6DF-325AD7235BAE';
DECLARE @Shipment2015 UNIQUEIDENTIFIER = '2A50D0C5-2041-46F9-BF43-DB2223B2E919';
DECLARE @Quotation UNIQUEIDENTIFIER = '43FB1BBE-8070-4376-8BB5-1006A18419F2';

INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code)
VALUES
	(@Org, 'XXXX')

INSERT INTO dbo.OrgAddress
	(OA_PK, OA_OH, OA_Address1)
VALUES
	(@OrgAddress, @Org, 'TEST ADDRESS')

INSERT INTO dbo.JobShipment
	(JS_PK, JS_UniqueConsignRef, JS_RL_NKOrigin, JS_RL_NKDestination, JS_TransportMode, JS_E_DEP, JS_IsCancelled)
VALUES
	(@Shipment2013, 'TST2013', 'AUSYD', 'AUSYD', 'AIR', '2013-1-1', 0),
	(@Shipment2014, 'TST2014', 'AUSYD', 'AUSYD', 'AIR', '2014-1-1', 0),
	(@Shipment2015, 'TST2015', 'AUSYD', 'AUSYD', 'SEA', '2015-1-1', 0)

INSERT INTO dbo.JobHeader
	(JH_PK, JH_ParentID, JH_ParentTableCode, JH_JobNum, JH_OA_LocalChargesAddr, JH_GB, JH_GE, JH_GC, JH_Status)
VALUES
	('7F440C32-7D13-49FE-A89C-38BCC3EB7661', @Shipment2013, 'JS', 'TST2013', @OrgAddress, @Branch, @Department, @Company, 'WRK'),
	('7F440C32-7D13-49FE-A89C-38BCC3EB7662', @Shipment2014, 'JS', 'TST2014', @OrgAddress, @Branch, @Department, @Company, 'WRK'),
	('7F440C32-7D13-49FE-A89C-38BCC3EB7663', @Shipment2015, 'JS', 'TST2015', @OrgAddress, @Branch, @Department, @Company, 'WRK')

INSERT INTO dbo.AccTransactionHeader
	(AH_PK, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType)
VALUES
	('D9F2C6F6-E0B7-4284-AABA-9CCF72001061', 'INV00001001', @Company, @Branch, @Department, '2005-1-1', 'AR', 'INV'),
	('D9F2C6F6-E0B7-4284-AABA-9CCF72001062', 'INV00001002', @Company, @Branch, @Department, '2005-1-1', 'AR', 'INV'),
	('D9F2C6F6-E0B7-4284-AABA-9CCF72001063', 'INV00001003', @Company, @Branch, @Department, '2005-1-1', 'AR', 'INV')

INSERT INTO dbo.AccTransactionLines
	(AL_PK, AL_AH, AL_JH, AL_AC, AL_LineType, AL_LineAmount, AL_OH, AL_GC, AL_GB, AL_GE, AL_ReverseDate)
VALUES
	(NEWID(), 'D9F2C6F6-E0B7-4284-AABA-9CCF72001061', '7F440C32-7D13-49FE-A89C-38BCC3EB7661', @ChargeCode, 'REV', 200, @Org, @Company, @Branch, @Department, '2013-1-1'),
	(NEWID(), 'D9F2C6F6-E0B7-4284-AABA-9CCF72001062', '7F440C32-7D13-49FE-A89C-38BCC3EB7662', @ChargeCode, 'REV', 200, @Org, @Company, @Branch, @Department, '2013-1-1'),
	(NEWID(), 'D9F2C6F6-E0B7-4284-AABA-9CCF72001063', '7F440C32-7D13-49FE-A89C-38BCC3EB7663', @ChargeCode, 'REV', 200, @Org, @Company, @Branch, @Department, '2013-1-1')

INSERT INTO dbo.RatingHeader
	(TH_PK, TH_OH, TH_RateType, TH_QuoteNumber, TH_QuoteDate, TH_IsCancelled, TH_GC, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser)
VALUES
	(@Quotation, @Org, 'QTE', 'QTE00001', '2015-1-1', 0, @Company, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.RateEntry
	(TI_PK, TI_TH, TI_GC_Publisher, TI_OriginLRC, TI_RateCategory, TI_Mode, TI_RateStartDate, TI_RateEndDate, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
VALUES
	('650B489F-E809-43C9-816B-A61A6A021695', @Quotation, @Company, 'AUSYD', 'AIR', 'LSE', '2015-1-1', NULL, '2015-1-1', '~BP', GetUtcDate(), '~BP')
";

			TestConnection.ExecuteNonQuery(insertSql);

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(TestConnection);

			using (var provider = new CachedTradeLinesSummaryProvider(TestConnection, new ZDate(2014, 1, 1), new ZDate(2016, 1, 1)))
			{
				var shpPk = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP").PK;

				var org = Factory.Load<OrgHeader>(Guid.Parse("E8C9E76C-C515-405B-A5AC-177170F2644B"));
				{
					var summary = provider.GetForOrg(org, new ZDate(2014, 1, 1), new ZDate(2016, 1, 1));

					AssertContainsExactElementsInAnyOrder(
						new Tuple<ZDate, ZGuid, string>[]
						{
							Tuple.Create(new ZDate(2014, 1, 1), shpPk, "AIR"),
							Tuple.Create(new ZDate(2015, 1, 1), shpPk, "SEA"),
						},
						GetActualPeriods(summary));

					AssertContainsExactElementsInAnyOrder(
						new Tuple<ZDate, ZGuid, string>[]
						{
							Tuple.Create(ZDate.Empty, shpPk, "AIR"),
						},
						GetProspectPeriods(summary));
				}

				{
					var summary = provider.GetForOrg(org, new ZDate(2014, 1, 1), new ZDate(2014, 10, 1));
					AssertContainsExactElementsInAnyOrder(
						new Tuple<ZDate, ZGuid, string>[]
						{
							Tuple.Create(new ZDate(2014, 1, 1), shpPk, "AIR"),
						},
						GetActualPeriods(summary));

					AssertContainsExactElementsInAnyOrder(
						new Tuple<ZDate, ZGuid, string>[]
						{
							Tuple.Create(ZDate.Empty, shpPk, "AIR"),
						},
						GetProspectPeriods(summary));
				}

				{
					var summary = provider.GetForOrg(org, new ZDate(2015, 1, 1), new ZDate(2015, 10, 1));
					AssertContainsExactElementsInAnyOrder(
						new Tuple<ZDate, ZGuid, string>[]
						{
							Tuple.Create(new ZDate(2015, 1, 1), shpPk, "SEA"),
						},
						GetActualPeriods(summary));

					AssertContainsExactElementsInAnyOrder(
						new Tuple<ZDate, ZGuid, string>[]
						{
							Tuple.Create(ZDate.Empty, shpPk, "AIR"),
						},
						GetProspectPeriods(summary));
				}

				AssertExceptionThrown(typeof(ArgumentException),
						"'from' (01-Jan-2000) must be equal to or greater than 'CacheFrom' (01-Jan-2014)",
						() => provider.GetForOrg(org, new ZDate(2000, 1, 1), new ZDate(2015, 1, 1))
					);

				AssertExceptionThrown(typeof(ArgumentException),
						"'to' (01-Jan-2020) must be equal to or less than 'CacheTo' (01-Jan-2016)",
						() => provider.GetForOrg(org, new ZDate(2015, 1, 1), new ZDate(2020, 1, 1))
					);
			}
		}

		IEnumerable<Tuple<ZDate, ZGuid, string>> GetActualPeriods(TradeLinesSummary summary)
		{
			return GetPeriods(summary.ActualValues, isProspectPeriod: false);
		}

		IEnumerable<Tuple<ZDate, ZGuid, string>> GetProspectPeriods(TradeLinesSummary summary)
		{
			return GetPeriods(summary.ProspectValues, isProspectPeriod: true);
		}

		IEnumerable<Tuple<ZDate, ZGuid, string>> GetPeriods(IDictionary<TradeLaneKey, TradeLaneValue> summaryValues, bool isProspectPeriod)
		{
			var valueKeys = new List<Tuple<ZDate, ZGuid, string>>();
			foreach (var tradeLane in summaryValues)
			{
				foreach (var tradeDetail in tradeLane.Value.TradeDetails)
				{
					var tradePeriod = tradeDetail.Value.TradePeriods.Single(x => isProspectPeriod || x.Key.IsJobValue);
					valueKeys.Add(Tuple.Create<ZDate, ZGuid, string>(tradePeriod.Key.Period, tradeLane.Key.ProductPk, tradeDetail.Key.Mode));
				}
			}
			return valueKeys;
		}

		[UseSnapshotProtection]
		public void TestDisposeWhenTableDoesNotExistDoesntThrowException()
		{
			try
			{
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
					jcdServiceTaskHelper.RunJCDServiceTaskForTableCreation(connection);

					using (var summary = new CachedTradeLinesSummaryProvider(connection, new ZDate(2014, 1, 1), new ZDate(2015, 1, 1)))
					{
						//Ensure table is actually created
						summary.GetForOrg(Factory.NewWithValidTestData<OrgHeader>(), new ZDate(2014, 1, 1), new ZDate(2015, 1, 1));

						connection.CloseConnection();
						AssertNoExceptionThrown(summary.Dispose);
					}
				}
			}
			finally
			{
				DbCommitTracker.Ignore(CachedTradeLinesSummaryProvider.TradeLineCacheTableName);
			}
		}
	}
}
