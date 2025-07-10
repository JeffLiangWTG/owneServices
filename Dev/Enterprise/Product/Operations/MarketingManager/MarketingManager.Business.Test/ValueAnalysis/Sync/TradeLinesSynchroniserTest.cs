using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradeLinesSynchroniserTest : TestCaseWithFactory
	{
		public void TestCannotSetNegativeProperties()
		{
			var shipment = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var syncRange = new TradeLinesSynchronizationRange(new ZDate(2002, 1, 1), new ZDate(2002, 5, 1));
			var summary = new TradeLinesSummary(syncRange);
			var sales = Factory.NewWithValidTestData<OrgSales>();
			var tradeKey = new TradeLaneKey(sales);
			var tradeValue = new TradeLaneValue();
			var stubDecimal = 1.12m;
			var stubInt = 1;
			var period = new ZDate(2002, 2, 2);
			var weightVolume = -1m;
			var weight = -2m;
			var volume = -3m;
			var chargeable = -4m;

			var tradeLine = new TradeLineForTest(
				"AIR", "", ZGuid.Empty, 3,
				period,
				org.PK, org.PK, org.PK,
				period, stubInt, stubInt, stubInt, weightVolume, weight, volume, chargeable, "KG", stubDecimal,
				"AUD", Env.CurrentCompany.PK,
				stubDecimal, stubDecimal, stubDecimal, stubDecimal,
				ZGuid.Empty
			);
			tradeValue.AddData(
				org.PK,
				tradeLine
			);

			summary.ActualValues.Add(new KeyValuePair<TradeLaneKey, TradeLaneValue>(tradeKey, tradeValue));

			Factory.Save();
			AssertEquals(true, org.MiscServ.OM_CMClientCommenced.IsEmpty);

			var synchroniser = new TradeLinesSynchroniser(org.PK);
			synchroniser.Execute(summary);
			org.SalesCollection.Load();

			var actuals = org.SalesCollection.Where(x => x.IsActual).ToList();
			AssertEquals("Precondition: Should have added an actual", 1, actuals.Count);

			var addedSales = actuals[0];
			var addedTradeDetail = addedSales.TradeDetails[0];
			CombineAssertions(() =>
			{
				foreach (var addedTradePeriod in addedTradeDetail.TradedPeriods)
				{
					AssertEquals($"{addedTradePeriod}: PAS_Units: ", 0L, addedTradePeriod.PAS_Units);
					AssertEquals($"{addedTradePeriod}: PAS_Weight", 0m, addedTradePeriod.PAS_Weight);
					AssertEquals($"{addedTradePeriod}: PAS_Volume", 0m, addedTradePeriod.PAS_Volume);
					AssertEquals($"{addedTradePeriod}: PAS_Chargeable", 0m, addedTradePeriod.PAS_Chargeable);
				}
				AssertEquals("OM_CMClientCommenced", new ZDateTime(2002, 2, 2), new BusinessObjectFactory().Load<OrgHeader>(org.PK).MiscServ.OM_CMClientCommenced);
			});
		}

		[TestDate(2014, 6, 19)]
		public void TestTlsLogIsUpdatedWithWhenItWasProcessed()
		{
			Organisation.OH_Code = "TESTORGXXX";
			Organisation.OH_RL_NKClosestPort = "AUBNE";
			Factory.Save();

			var syncRange = new TradeLinesSynchronizationRange(new ZDate(2013, 12, 23), new ZDate(2016, 12, 23));
			var summary = new TradeLinesSummary(syncRange);

			var synch = new TradeLinesSynchroniser(Organisation.PK);
			synch.Execute(summary);

			var log = GetTlsLog(Organisation);
			log.Factory.Save();

			AssertEquals("PARTIAL 201312, Last run: 19-Jun-14, First run: 19-Jun-14", log.SL_Reference);

			TestDateAttribute.Date = new DateTime(2015, 12, 23);
			synch = new TradeLinesSynchroniser(Organisation.PK);
			synch.Execute(summary);
			log = LoadLog();

			AssertEquals("PARTIAL 201312, Last run: 23-Dec-15, First run: 19-Jun-14", log.SL_Reference);
		}

		static StmALog GetTlsLog(OrgHeader org)
		{
			return (StmALog)org.Logs.GetAllLogs()
				.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.SalesTradeLanesSynchronisedCode))
				.Single();
		}

		public void TestSynchronisingValuesWithHigherPrecisionThanDatabaseColumnsMultipleTimes()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var shipment = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var syncRange = new TradeLinesSynchronizationRange(new ZDate(2002, 1, 1), new ZDate(2002, 5, 1));
			var summary = new TradeLinesSummary(syncRange);
			var tradeValue = new TradeLaneValue();
			var period = new ZDate(2002, 2, 2);
			var alotOfDecimals = 1.123456789m;
			var stubInt = 1;
			var product = Factory.NewWithValidTestData<OrgSalesProduct>();

			var tradeLine = new TradeLineForTest(
				"AIR", "", ZGuid.Empty, 3,
				period,
				ZGuid.Empty, org.PK, org.PK,
				period, stubInt, stubInt, stubInt, alotOfDecimals, alotOfDecimals, alotOfDecimals, alotOfDecimals, "KG", alotOfDecimals,
				"AUD", Env.CurrentCompany.PK,
				alotOfDecimals, alotOfDecimals, alotOfDecimals, alotOfDecimals,
				ZGuid.Empty, product
				);

			var tradeKey = new TradeLaneKey(TradeLaneKey.TradeType.Actual, tradeLine);
			tradeValue.AddData(org.PK, tradeLine);

			Factory.Save();
			summary.ActualValues.Add(tradeKey, tradeValue);

			var synchroniser = new TradeLinesSynchroniser(org.PK);
			synchroniser.Execute(summary);
			org.SalesCollection.Load();

			var actuals = org.SalesCollection.Where(x => x.IsActual).ToList();
			AssertEquals("Precondition: Should have added an actual", 1, actuals.Count);
			var addedSales = actuals[0];
			var addedTradeDetail = addedSales.TradeDetails[0];

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var tradeDetailInOtherFactory = anotherFactory.Load<OrgTradeDetail>(addedTradeDetail.PK);
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(org.PK);
			var synchroniserInOtherFactory = new TradeLinesSynchroniser(orgInOtherFactory.PK);

			synchroniserInOtherFactory.Execute(summary);

			if (!tradeDetailInOtherFactory.HasChanges)
			{
				Assert(true);
			}
			else
			{
				var propertiesWithChanges = tradeDetailInOtherFactory.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.All).Cast<ZPropertyInfo>().Where(x => x.HasChanges);
				var msg = "Should not cause any changes when synching the exact same data as before. The following properties have changes:\r\n"
					+ string.Join(System.Environment.NewLine, propertiesWithChanges.Select(x => " - " + x.Name + ": " + x.OriginalValue.ToString() + " -> " + x.Value.ToString()));
				AssertEquals(msg, false, tradeDetailInOtherFactory.HasChanges);
			}

			var saleInOtherFactory = tradeDetailInOtherFactory.Sales;
			if (!saleInOtherFactory.HasChanges)
			{
				Assert(true);
			}
			else
			{
				var propertiesWithChanges = saleInOtherFactory.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.All).Cast<ZPropertyInfo>().Where(x => x.HasChanges);
				var msg = "Should not cause any changes when synching the exact same data as before. The following properties have changes:\r\n"
					+ string.Join(System.Environment.NewLine, propertiesWithChanges.Select(x => " - " + x.Name + ": " + x.OriginalValue.ToString() + " -> " + x.Value.ToString()));
				AssertEquals(msg, false, saleInOtherFactory.HasChanges);
			}
		}

		[TestDate(2015, 1, 1)]
		public void TestQuotationAndClientRate()
		{
			var anotherOrg = Factory.NewWithValidTestData<OrgHeader>();
			Organisation.OH_Code = "TESTORGXXX";
			Organisation.OH_RL_NKClosestPort = "AUBNE";

			var newQuote = Factory.New<Quote>();
			newQuote.TH_OH = Organisation.PK;
			newQuote.TH_QuoteDate = ZDate.Today.AddMonths(-6);
			CollectionHelper.AddRateEntry(newQuote, "AIR", "AUSYD", "USLAX", ZGuid.Empty, ZGuid.Empty);
			CollectionHelper.AddRateEntry(newQuote, "LCL", "AUSYD", "USLAX", ZGuid.Empty, ZGuid.Empty);
			CollectionHelper.AddRateEntry(newQuote, "FCL", "GBLON", "AUMEL", Organisation.PK, ZGuid.Empty);
			CollectionHelper.AddRateEntry(newQuote, "FCL", "", "AUMEL", ZGuid.Empty, Organisation.PK);

			CollectionHelper.AddRateEntry(newQuote, "SCO", "AUSYD", "USLAX", Organisation.PK, anotherOrg.PK);
			CollectionHelper.AddRateEntry(newQuote, "SNC", "AUSYD", "USLAX", Organisation.PK, anotherOrg.PK);

			var brokerageChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			brokerageChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;
			var orgEntry = newQuote.ORGRateEntriesForBinding.AddNew();
			orgEntry.TI_OriginLRC = "AUSYD";
			orgEntry.TI_Mode = "AIR";
			orgEntry.TI_OH_Consignee = Organisation.PK;
			orgEntry.TI_OH_Consignor = anotherOrg.PK;
			orgEntry.RateLines.AddNew().TL_AC = brokerageChargeCode.PK;
			var dstEntry = newQuote.DSTRateEntriesForBinding.AddNew();
			dstEntry.TI_DestinationLRC = "AUSYD";
			dstEntry.TI_Mode = "SEA";
			dstEntry.TI_OH_Consignee = Organisation.PK;
			dstEntry.TI_OH_Consignor = anotherOrg.PK;
			dstEntry.RateLines.AddNew().TL_AC = brokerageChargeCode.PK;

			Factory.Save();
			AssertEquals(true, Organisation.MiscServ.OM_CMClientCommenced.IsEmpty);

			var anotherFactory = new BusinessObjectFactory();
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(Organisation.PK);
			var collectionInOtherFactory = new OrgSalesCollection(orgInOtherFactory);

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryManager = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var tester = new TradeLinesSynchroniserTester(orgInOtherFactory.PK);
				var saveCount = 0;
				tester.OnSaved += (s, e) =>
				{
					if (saveCount == 2)
					{
						var localExpectedDbHits = new Dictionary<string, int>
						{
							{ OrgHeaderSchema.Constants.TableName, 1 },
							{ OrgSalesSchema.Constants.TableName, 1 },
							{ OrgSalesProductSchema.Constants.TableName, 1 }, // Checking product AllowedAssociationTargets
							{ ProcessTasksSchema.Constants.TableName, 1 }, // Adding Log
							{ ProcessTaskTemplateSchema.Constants.TableName, 1 }, // Adding Log
							{ StmALogSchema.Constants.TableName, 2 }, // Adding Log
							{ ViewLocationSchema.Constants.TableName, 4 }, // Adding Log
							{ OrgMiscServSchema.Constants.TableName, 1 } // Set Client Commenced Date
						};
						AssertDbHits(localExpectedDbHits, tester.Factory);
					}
					saveCount++;
				};
				tester.Execute(summaryManager, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}
			collectionInOtherFactory.Load();

			AssertEquals(5, collectionInOtherFactory.Count);

			collectionHelper = new OrgSalesCollectionTestHelper(collectionInOtherFactory, anotherFactory);
			var sales1 = CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", Guid.Empty, Guid.Empty, isActual: false).Single();
			AssertEquals(2, sales1.TradeDetails.Count);

			var sales2 = CollectionHelper.FindOrgSales("SHP", "GBLON", "AUMEL", Organisation.PK.ToGuid(), Guid.Empty, isActual: false).Single();
			AssertEquals(1, sales2.TradeDetails.Count);

			var sales3 = CollectionHelper.FindOrgSales("SHP", "", "AUMEL", Guid.Empty, Organisation.PK.ToGuid(), isActual: false).Single();
			AssertEquals(1, sales3.TradeDetails.Count);

			var sales4 = CollectionHelper.FindOrgSales("LGY", "AUSYD", "USLAX", Organisation.PK.ToGuid(), anotherOrg.PK.ToGuid(), isActual: false).Single();
			AssertEquals(2, sales4.TradeDetails.Count); //BOL FCL & BOL BBK
			var sales4BBK = sales4.TradeDetails.FirstOrDefault(d => ((OrgTradeDetail)d).PA_TradeType == "BBK");
			AssertNotNull(sales4BBK);

			var sales5 = CollectionHelper.FindOrgSales("BRK", "AUSYD", "", Organisation.PK.ToGuid(), anotherOrg.PK.ToGuid(), isActual: false).Single();
			AssertEquals(2, sales5.TradeDetails.Count);

			var query = new ZQuery();
			query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_ActivityTableCode, RateEntrySchema.Constants.Prefix);
			query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode, OrgSalesSchema.Constants.Prefix);
			var pivots = anotherFactory.Load<OrgSalesValueAssociationPivot>(query);
			AssertEquals(8, pivots.Length);
			AssertEquals(true, orgInOtherFactory.MiscServ.OM_CMClientCommenced.IsEmpty);

			// client rate should turn status to CNF-Confirmed
			var clientRate = anotherFactory.New(ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().RatingHeaderType);
			clientRate[RatingHeaderSchema.TH_OH.Name] = Organisation.PK;
			clientRate[RatingHeaderSchema.TH_RateType.Name] = "SAL";
			clientRate[RatingHeaderSchema.TH_QuoteDate.Name] = ZDateTime.Today.AddMonths(-1);
			clientRate[RatingHeaderSchema.TH_QuoteEndDate.Name] = ZDateTime.Today.AddMonths(1);
			CollectionHelper.AddRateEntry(clientRate, "AIR", "AUSYD", "USLAX", ZGuid.Empty, ZGuid.Empty);

			anotherFactory.Save();

			var anotherFactory2 = new BusinessObjectFactory();
			var orgInOtherFactory2 = anotherFactory2.Load<OrgHeader>(Organisation.PK);
			var collectionInOtherFactory2 = new OrgSalesCollection(orgInOtherFactory2);
			collectionInOtherFactory2.Load();

			using (var summary = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var tester = new TradeLinesSynchroniserTester(orgInOtherFactory.PK);
				var saveCount = 0;
				tester.OnSaved += (s, e) =>
				{
					if (saveCount == 2)
					{
						var localExpectedDbHits = new Dictionary<string, int>
						{
							{ OrgHeaderSchema.Constants.TableName, 1 },
							{ OrgSalesSchema.Constants.TableName, 1 },
							{ OrgTradeDetailSchema.Constants.TableName, 1 },
							{ OrgTradePeriodSchema.Constants.TableName, 1 },
							{ OrgTradeProspectSchema.Constants.TableName, 1 },
							{ OrgSalesProductSchema.Constants.TableName, 1 }, // Checking product AllowedAssociationTargets
							{ StmALogSchema.Constants.TableName, 1 }, // Adding Log
							{ OrgMiscServSchema.Constants.TableName, 1 } // Set Client Commenced Date
						};
						AssertDbHits(localExpectedDbHits, tester.Factory);
					}
					saveCount++;
				};
				tester.Execute(summary, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}

			anotherFactory2.ResetDatabaseLoadCount();
			anotherFactory2.Save();
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(RateEntrySchema.Constants.TableName, 0); // Check if parent is OrgOpportunity to set OW_LatestProspectDate, use OrgSalesValueAssociationPivot table code so no more
			AssertDbHits(expectedDbHits, anotherFactory2);

			query = new ZQuery();
			query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_ActivityTableCode, RateEntrySchema.Constants.Prefix);
			query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode, OrgSalesSchema.Constants.Prefix);
			pivots = anotherFactory2.Load<OrgSalesValueAssociationPivot>(query);
			AssertEquals(9, pivots.Length);
			AssertEquals(true, orgInOtherFactory.MiscServ.OM_CMClientCommenced.IsEmpty);
		}

		[TestDate(2015, 1, 1)]
		public void TestQuotationAndClientRate_WithExistingTradeLanes()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var orgA = Factory.NewWithValidTestData<OrgHeader>();
			var orgB = Factory.NewWithValidTestData<OrgHeader>();
			var orgC = Factory.NewWithValidTestData<OrgHeader>();

			Organisation.OH_Code = "TESTORGXXX";
			Organisation.OH_RL_NKClosestPort = "AUBNE";

			var salesNoBuyerSupplier = Organisation.SalesCollection.AddNew();
			using (salesNoBuyerSupplier.GetDefaultPropertySuspender())
			{
				salesNoBuyerSupplier.OW_MP_Product = shipmentProduct.Identifier;
				salesNoBuyerSupplier.OW_OriginID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD").PK;
				salesNoBuyerSupplier.OW_DestinationID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX").PK;
				salesNoBuyerSupplier.OW_OH_Buyer = ZGuid.Empty;
				salesNoBuyerSupplier.OW_OH_Supplier = ZGuid.Empty;
				var tradeDetailNoBuyerSupplierAir = salesNoBuyerSupplier.TradeDetails.AddNew();
				tradeDetailNoBuyerSupplierAir.PA_Status = OrgTradeDetail.TradeLaneStatus.Quoted;
				tradeDetailNoBuyerSupplierAir.PA_TradeMode = "AIR";
				tradeDetailNoBuyerSupplierAir.PA_TradeType = "LSE";
				tradeDetailNoBuyerSupplierAir.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
				tradeDetailNoBuyerSupplierAir.CurrentProspectPeriod.PAS_Weight = 100m;
			}

			Factory.Save();

			TestDateAttribute.AddMinutes(1);
			var salesOrgAOrgB = Organisation.SalesCollection.AddNew();
			using (salesOrgAOrgB.GetDefaultPropertySuspender())
			{
				salesOrgAOrgB.OW_MP_Product = shipmentProduct.Identifier;
				salesOrgAOrgB.OW_OriginID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD").PK;
				salesOrgAOrgB.OW_DestinationID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX").PK;
				salesOrgAOrgB.OW_OH_Buyer = orgA.PK;
				salesOrgAOrgB.OW_OH_Supplier = orgB.PK;
				var tradeDetailOrgAOrgBAir = salesOrgAOrgB.TradeDetails.AddNew();
				tradeDetailOrgAOrgBAir.PA_Status = OrgTradeDetail.TradeLaneStatus.Confirmed;
				tradeDetailOrgAOrgBAir.PA_TradeMode = "AIR";
				tradeDetailOrgAOrgBAir.PA_TradeType = "LSE";
				tradeDetailOrgAOrgBAir.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
				tradeDetailOrgAOrgBAir.CurrentProspectPeriod.PAS_Weight = 150m;
			}

			var newQuote = Factory.New(ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().QuoteType);
			newQuote[RatingHeaderSchema.TH_OH.Name] = Organisation.PK;
			newQuote[RatingHeaderSchema.TH_QuoteDate.Name] = ZDate.Today.AddMonths(-6);
			var rate00 = CollectionHelper.AddRateEntry(newQuote, "AIR", "AUSYD", "USLAX", ZGuid.Empty, ZGuid.Empty);
			var rateA0 = CollectionHelper.AddRateEntry(newQuote, "AIR", "AUSYD", "USLAX", orgA.PK, ZGuid.Empty);
			var rate0B = CollectionHelper.AddRateEntry(newQuote, "AIR", "AUSYD", "USLAX", ZGuid.Empty, orgB.PK);
			var rateAB = CollectionHelper.AddRateEntry(newQuote, "AIR", "AUSYD", "USLAX", orgA.PK, orgB.PK);
			var rateC0 = CollectionHelper.AddRateEntry(newQuote, "AIR", "AUSYD", "USLAX", orgC.PK, ZGuid.Empty);
			var rate0C = CollectionHelper.AddRateEntry(newQuote, "AIR", "AUSYD", "USLAX", ZGuid.Empty, orgC.PK);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(Organisation.PK);
			var collectionInOtherFactory = new OrgSalesCollection(orgInOtherFactory);
			collectionInOtherFactory.Load();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var tester = new TradeLinesSynchroniserTester(orgInOtherFactory.PK);
				var saveCount = 0;
				tester.OnSaved += (s, e) =>
				{
					if (saveCount == 2)
					{
						var expectedDbHits = new Dictionary<string, int>
						{
							{ OrgSalesSchema.Constants.TableName, 1 },
							{ OrgHeaderSchema.Constants.TableName, 1 }
						};
						AssertDbHits(expectedDbHits, anotherFactory);
					}
					saveCount++;
				};
				tester.Execute(summaryProvider, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}

			collectionInOtherFactory.Load();

			collectionHelper = new OrgSalesCollectionTestHelper(collectionInOtherFactory, anotherFactory);
			salesNoBuyerSupplier = CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", Guid.Empty, Guid.Empty, isActual: false).Single();
			AssertContainsExactElementsInAnyOrder("If trade lanes have no buyer/supplier entered, only quotes with no consignee/consignor should be matched", BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new[] { rate00 },
				salesNoBuyerSupplier.SalesAssociationPivotCollectionGlobal.Select(x => (BusinessObject)x.AssociatedEntity));
			AssertEquals(1, salesNoBuyerSupplier.TradeDetails.Count);
			AssertEquals(100m, salesNoBuyerSupplier.TradeDetails[0].CurrentProspectPeriod.PAS_Weight);

			salesOrgAOrgB = CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", orgA.PK.ToGuid(), orgB.PK.ToGuid(), isActual: false).Single();
			AssertContainsExactElementsInAnyOrder("If trade lanes have buyer/supplier entered, quotes with same consignee/consignor OR no consignee/consignor should be matched", BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new[]
			{
				rateA0,
				rate0B,
				rateAB
			},
				salesOrgAOrgB.SalesAssociationPivotCollectionGlobal.Select(x => (BusinessObject)x.AssociatedEntity));
			AssertEquals(1, salesOrgAOrgB.TradeDetails.Count);
			AssertEquals(150m, salesOrgAOrgB.TradeDetails[0].CurrentProspectPeriod.PAS_Weight);

			var salesOrgCOrg0 = CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", orgC.PK.ToGuid(), Guid.Empty, isActual: false).Single();
			AssertContainsExactElementsInAnyOrder("Quotes with consignee/consignor that don't match any existing trade lanes should create a new trade lane", BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new[] { rateC0 },
				salesOrgCOrg0.SalesAssociationPivotCollectionGlobal.Select(x => (BusinessObject)x.AssociatedEntity));
			AssertEquals(1, salesOrgCOrg0.TradeDetails.Count);
			AssertEquals(0m, salesOrgCOrg0.TradeDetails[0].CurrentProspectPeriod.PAS_Weight);
			AssertEquals("AUD", salesOrgCOrg0.TradeDetails[0].CurrentProspectPeriod.PAS_RX_NKCurrency);

			var salesOrg0OrgC = CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", Guid.Empty, orgC.PK.ToGuid(), isActual: false).Single();
			AssertContainsExactElementsInAnyOrder("Quotes with consignee/consignor that don't match any existing trade lanes should create a new trade lane", BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new[] { rate0C },
				salesOrg0OrgC.SalesAssociationPivotCollectionGlobal.Select(x => (BusinessObject)x.AssociatedEntity));
			AssertEquals(1, salesOrg0OrgC.TradeDetails.Count);
			AssertEquals(0m, salesOrg0OrgC.TradeDetails[0].CurrentProspectPeriod.PAS_Weight);
			AssertEquals("AUD", salesOrgCOrg0.TradeDetails[0].CurrentProspectPeriod.PAS_RX_NKCurrency);

			AssertEquals(4, collectionInOtherFactory.Count);
		}

		[TestDate(2015, 1, 1)]
		public void TestQuotationAndClientRate_WithExistingOpportunityRelatedTradeDetails()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var orgA = Factory.NewWithValidTestData<OrgHeader>();
			var orgB = Factory.NewWithValidTestData<OrgHeader>();

			Organisation.OH_Code = "TESTORGXXX";
			Organisation.OH_RL_NKClosestPort = "AUBNE";

			var opportunity = Organisation.SalesOpportunities.AddNew();

			var salesOrgAOrgB = Organisation.SalesCollection.AddNew();
			using (salesOrgAOrgB.GetDefaultPropertySuspender())
			{
				salesOrgAOrgB.OW_MP_Product = shipmentProduct.Identifier;
				salesOrgAOrgB.OW_OriginID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD").PK;
				salesOrgAOrgB.OW_DestinationID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX").PK;
				salesOrgAOrgB.OW_OH_Buyer = orgA.PK;
				salesOrgAOrgB.OW_OH_Supplier = orgB.PK;

				var tradeDetailOrgAOrgBAir = salesOrgAOrgB.TradeDetails.AddNew();
				tradeDetailOrgAOrgBAir.PA_TradeMode = "AIR";
				tradeDetailOrgAOrgBAir.PA_TradeType = "LSE";
				tradeDetailOrgAOrgBAir.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
				tradeDetailOrgAOrgBAir.CurrentProspectPeriod.PAS_Weight = 150m;
				tradeDetailOrgAOrgBAir.PA_Status = OpportunityTradeStatus.Codes.Successful;

				opportunity.AssociatedTradeLanesPivots.AddPivotFor(salesOrgAOrgB);
				opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeDetailOrgAOrgBAir);
			}

			var newQuote = Factory.New(ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().QuoteType);
			newQuote[RatingHeaderSchema.TH_OH.Name] = Organisation.PK;
			newQuote[RatingHeaderSchema.TH_QuoteDate.Name] = ZDate.Today.AddMonths(-6);
			var rateAB = CollectionHelper.AddRateEntry(newQuote, "AIR", "AUSYD", "USLAX", orgA.PK, orgB.PK);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(Organisation.PK);
			var collectionInOtherFactory = new OrgSalesCollection(orgInOtherFactory);
			collectionInOtherFactory.Load();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var tester = new TradeLinesSynchroniserTester(orgInOtherFactory.PK);
				tester.Execute(summaryProvider, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}

			collectionInOtherFactory.Load();

			collectionHelper = new OrgSalesCollectionTestHelper(collectionInOtherFactory, anotherFactory);
			salesOrgAOrgB = collectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", orgA.PK.ToGuid(), orgB.PK.ToGuid(), isActual: false).Single();
			AssertEquals("Should not create new trade details", 1, salesOrgAOrgB.TradeDetails.Count);
			var detail = salesOrgAOrgB.TradeDetails[0];
			AssertEquals("Detail should remain unchanged", 150m, detail.CurrentProspectPeriod.PAS_Weight);
			AssertEquals("Detail type should remain unchanged", OpportunityTradeStatus.Codes.Successful, detail.PA_Status);
		}

		[TestDate(2015, 1, 1)]
		public void TestShipment()
		{
			Organisation.OH_Code = "TESTORGXXX";
			Organisation.OH_RL_NKClosestPort = "AUBNE";

			var newOrg = CollectionHelper.GetNewOrg("TESTORG");
			for (int i = 0; i < 6; i++)
			{
				var airShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", Organisation.PK, newOrg.PK, ZDateTime.Empty);
				airShipment[JobShipmentSchema.JS_ActualWeight.Name] = i * 100m;
				var lCLShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "SEA", "LCL", Organisation.PK, newOrg.PK, ZDateTime.Empty);
				lCLShipment[JobShipmentSchema.JS_ActualVolume.Name] = i * 0.5m;
				var rORShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "SEA", "ROR", Organisation.PK, newOrg.PK, ZDateTime.Empty, true);
				rORShipment[JobShipmentSchema.JS_ActualVolume.Name] = i * 10m;
			}
			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summary = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser1 = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser1.Execute(summary, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
				var synchroniser2 = new TradeLinesSynchroniser(newOrg.PK);
				synchroniser2.Execute(summary, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}
			TestCollection.Load();

			AssertEquals(1, TestCollection.Count);

			var sales1 = CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", Organisation.PK.ToGuid(), newOrg.PK.ToGuid(), isActual: true).Single();
			AssertEquals(3, sales1.TradeDetails.Count);

			var loadFactory = new BusinessObjectFactory();

			var details1 = CollectionHelper.FindOrgTradeDetailByMode(sales1, "AIR");
			AssertShipmentTradeLaneJobCountWeightVolume(details1, Organisation.PK, newOrg.PK, 6, 1.5m, 0m);

			var details2 = CollectionHelper.FindOrgTradeDetailByModeAndType(sales1, "SEA", "LCL");
			AssertShipmentTradeLaneJobCountWeightVolume(details2, Organisation.PK, newOrg.PK, 6, 0m, 7.5m);

			var details3 = CollectionHelper.FindOrgTradeDetailByModeAndType(sales1, "SEA", "ROR");
			AssertShipmentTradeLaneJobCountWeightVolume(details3, Organisation.PK, newOrg.PK, 6, 0m, 150m);

			for (int i = 0; i < 6; i++)
			{
				var airShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", Organisation.PK, newOrg.PK);
				airShipment[JobShipmentSchema.JS_ActualWeight.Name] = i * 100m;
				var lCLShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "SEA", "LCL", Organisation.PK, newOrg.PK);
				lCLShipment[JobShipmentSchema.JS_ActualVolume.Name] = i * 0.5m;
				var rORShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "SEA", "ROR", Organisation.PK, newOrg.PK, ZDateTime.Today, true);
				rORShipment[JobShipmentSchema.JS_ActualVolume.Name] = i * 10m;

				CollectionHelper.CreateShipment("GBLON", "AUMEL", "SEA", "FCL", Organisation.PK, newOrg.PK);
				CollectionHelper.CreateShipment("GBLON", "AUMEL", "SEA", "FCL", Organisation.PK, newOrg.PK);
			}
			Factory.Save();

			using (var summary = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser1 = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser1.Execute(summary, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
				var synchroniser2 = new TradeLinesSynchroniser(newOrg.PK);
				synchroniser2.Execute(summary, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}
			TestCollection.Load();
			details1.Reload();
			details2.Reload();
			details3.Reload();

			AssertEquals(2, TestCollection.Count);
			Assert(TestCollection[0].OW_OH_Primary.IsEmpty);
			Assert(TestCollection[1].OW_OH_Primary.IsEmpty);

			sales1 = CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", Organisation.PK.ToGuid(), newOrg.PK.ToGuid(), isActual: true).Single();
			AssertEquals(3, sales1.TradeDetails.Count);

			details1 = CollectionHelper.FindOrgTradeDetailByMode(sales1, Constants.TransportModes.Air);
			AssertShipmentTradeLaneJobCountWeightVolume(details1, Organisation.PK, newOrg.PK, 12, 3m, 0m);

			details2 = CollectionHelper.FindOrgTradeDetailByModeAndType(sales1, Constants.TransportModes.Sea, "LCL");
			AssertShipmentTradeLaneJobCountWeightVolume(details2, Organisation.PK, newOrg.PK, 12, 0m, 15m);

			details3 = CollectionHelper.FindOrgTradeDetailByModeAndType(sales1, "SEA", "ROR");
			AssertShipmentTradeLaneJobCountWeightVolume(details3, Organisation.PK, newOrg.PK, 12, 0m, 300m);

			var sales2 = CollectionHelper.FindOrgSales("SHP", "GBLON", "AUMEL", Organisation.PK.ToGuid(), newOrg.PK.ToGuid(), isActual: true).Single();
			AssertEquals(1, sales2.TradeDetails.Count);

			var details4 = CollectionHelper.FindOrgTradeDetailByMode(sales2, Constants.TransportModes.Sea);
			AssertShipmentTradeLaneJobCountWeightVolume(details4, Organisation.PK, newOrg.PK, 12, 0m, 0m);
		}

		void AssertShipmentTradeLaneJobCountWeightVolume(OrgTradeDetail detail, ZGuid buyerPk, ZGuid suppilerPk, int expectedJobCount, decimal expectedWeight, decimal expectedVolume)
		{
			var periods = detail.TradedPeriods;
			AssertEquals(4, periods.Count);

			var buyerJobPeriod = periods.Single(x => x.PAS_IsJobValue && x.PAS_OH_Client == buyerPk);
			if (buyerJobPeriod.IsInDatabase)
			{ buyerJobPeriod.Reload(); }
			AssertEquals(expectedJobCount, (int)buyerJobPeriod.PAS_RepeatsMnth);
			AssertEquals(expectedWeight, buyerJobPeriod.PAS_Weight);
			AssertEquals(expectedVolume, buyerJobPeriod.PAS_Volume);

			var supplierJobPeriod = periods.Single(x => x.PAS_IsJobValue && x.PAS_OH_Client == suppilerPk);
			if (supplierJobPeriod.IsInDatabase)
			{ supplierJobPeriod.Reload(); }
			AssertEquals(expectedJobCount, (int)supplierJobPeriod.PAS_RepeatsMnth);
			AssertEquals(expectedWeight, supplierJobPeriod.PAS_Weight);
			AssertEquals(expectedVolume, supplierJobPeriod.PAS_Volume);

			var buyerPeriod = periods.Single(x => !x.PAS_IsJobValue && x.PAS_OH_Client == buyerPk);
			if (buyerPeriod.IsInDatabase)
			{ buyerPeriod.Reload(); }
			AssertEquals(expectedJobCount, (int)buyerPeriod.PAS_RepeatsMnth);
			AssertEquals(expectedWeight, buyerPeriod.PAS_Weight);
			AssertEquals(expectedVolume, buyerPeriod.PAS_Volume);

			var supplierPeriod = periods.Single(x => !x.PAS_IsJobValue && x.PAS_OH_Client == suppilerPk);
			if (supplierPeriod.IsInDatabase)
			{ supplierPeriod.Reload(); }
			AssertEquals(expectedJobCount, (int)supplierPeriod.PAS_RepeatsMnth);
			AssertEquals(expectedWeight, supplierPeriod.PAS_Weight);
			AssertEquals(expectedVolume, supplierPeriod.PAS_Volume);
		}

		[TestDate(2015, 1, 1)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestShipment_WithRevenueAmounts()
		{
			Organisation.FillWithValidTestData();

			var auCompany = Factory.NewWithValidTestData<GlbCompany>();
			auCompany.GC_RN_NKCountryCode = "AU";
			auCompany.GC_RX_NKLocalCurrency = "AUD";

			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = "US";
			usCompany.GC_RX_NKLocalCurrency = "USD";

			var periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.SetupSinglePeriod(201903, ZDate.Today.AddYears(-6), ZDate.Today.AddYears(1), auCompany.PK);
			periodTestHelper.SetupSinglePeriod(201903, ZDate.Today.AddYears(-6), ZDate.Today.AddYears(1), usCompany.PK);

			var nonDsbChargeQuery = new ZQuery();
			nonDsbChargeQuery.AddToFilter(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.NotEqual, "DSB");
			nonDsbChargeQuery.AddToFilter(AccChargeCodeSchema.AC_AG_RevenueAccount, SQLComparisonOperator.NotEqual, null);
			nonDsbChargeQuery.OrderBy = AccChargeCodeSchema.Constants.AC_Code;
			var nonDsbCharge = Factory.LoadTop1<AccChargeCode>(nonDsbChargeQuery);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var audShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", Organisation.PK, org2.PK);
			var audJob = new JobHeader.Loader((IJobHeaderParent)audShipment).TryCreate();
			audJob.JH_GC = auCompany.PK;
			audJob.JH_A_JCL = ZDate.Today;
			var audHeaderAR = Factory.NewWithValidTestData<AccTransactionHeader>();
			audHeaderAR.AH_Ledger = LedgerTypes.AccountsReceivable;
			audHeaderAR.AH_TransactionType = TransactionTypes.Invoice;
			audHeaderAR.AH_JH = audJob.PK;
			audHeaderAR.AH_GC = auCompany.PK;
			audHeaderAR.AH_OH = org2.PK;
			var audHeaderAP = Factory.NewWithValidTestData<AccTransactionHeader>();
			audHeaderAP.AH_Ledger = LedgerTypes.AccountsPayable;
			audHeaderAP.AH_TransactionType = TransactionTypes.Invoice;
			audHeaderAP.AH_JH = audJob.PK;
			audHeaderAP.AH_GC = auCompany.PK;
			audHeaderAP.AH_OH = org2.PK;

			var audLine1 = Factory.NewWithValidTestData<AccTransactionLines>();
			audLine1.AL_AH = audHeaderAR.PK;
			audLine1.AL_JH = audJob.PK;
			audLine1.AL_AC = nonDsbCharge.PK;
			audLine1.AL_AG = nonDsbCharge.AC_AG_RevenueAccount;
			audLine1.AL_RX_NKTransactionCurrency = "AUD";
			audLine1.AL_LineType = "REV";
			audLine1.AL_LineAmount = audLine1.AL_OSAmount = 100;
			audLine1.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			audLine1.AL_PostDate = ZDate.Today;
			audLine1.AL_ReverseDate = ZDate.Today;
			audLine1.AL_GC = auCompany.PK;
			var audJobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			audJobCharge1.JR_JH = audJob.PK;
			audJobCharge1.JR_AL_ARLine = audLine1.PK;
			audJobCharge1.JR_LocalSellAmt = 100;
			audJobCharge1.JR_OSSellAmt = 100;

			var audLine2 = Factory.NewWithValidTestData<AccTransactionLines>();
			audLine2.AL_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			audLine2.AL_AH = audHeaderAP.PK;
			audLine2.AL_JH = audJob.PK;
			audLine2.AL_AC = nonDsbCharge.PK;
			audLine2.AL_AG = nonDsbCharge.AC_AG_CostAccount;
			audLine2.AL_RX_NKTransactionCurrency = "AUD";
			audLine2.AL_LineType = "CST";
			audLine2.AL_LineAmount = audLine2.AL_OSAmount = -20;
			audLine2.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			audLine2.AL_PostDate = ZDate.Today;
			audLine2.AL_ReverseDate = ZDate.Today;
			audLine2.AL_GC = auCompany.PK;
			var audJobCharge2 = Factory.NewWithValidTestData<JobCharge>();
			audJobCharge2.JR_JH = audJob.PK;
			audJobCharge2.JR_AL_APLine = audLine2.PK;
			audJobCharge2.JR_LocalCostAmt = 20;
			audJobCharge2.JR_OSCostAmt = 20;

			var usdShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", Organisation.PK, org2.PK);
			var usdJob = new JobHeader.Loader((IJobHeaderParent)usdShipment).TryCreate();
			usdJob.JH_GC = usCompany.PK;
			usdJob.JH_A_JCL = ZDate.Today;
			var usdHeaderAR = Factory.NewWithValidTestData<AccTransactionHeader>();
			usdHeaderAR.AH_Ledger = LedgerTypes.AccountsReceivable;
			usdHeaderAR.AH_TransactionType = TransactionTypes.Invoice;
			usdHeaderAR.AH_JH = usdJob.PK;
			usdHeaderAR.AH_GC = usCompany.PK;
			usdHeaderAR.AH_OH = org2.PK;

			var usdHeaderAP = Factory.NewWithValidTestData<AccTransactionHeader>();
			usdHeaderAP.AH_Ledger = LedgerTypes.AccountsPayable;
			usdHeaderAP.AH_TransactionType = TransactionTypes.Invoice;
			usdHeaderAP.AH_JH = usdJob.PK;
			usdHeaderAP.AH_GC = usCompany.PK;
			usdHeaderAP.AH_OH = org2.PK;

			var usdLine1 = Factory.NewWithValidTestData<AccTransactionLines>();
			usdLine1.AL_AH = usdHeaderAR.PK;
			usdLine1.AL_JH = usdJob.PK;
			usdLine1.AL_AC = nonDsbCharge.PK;
			usdLine1.AL_AG = nonDsbCharge.AC_AG_RevenueAccount;
			usdLine1.AL_RX_NKTransactionCurrency = "USD";
			usdLine1.AL_LineType = "REV";
			usdLine1.AL_LineAmount = usdLine1.AL_OSAmount = 200;
			usdLine1.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			usdLine1.AL_PostDate = ZDate.Today;
			usdLine1.AL_ReverseDate = ZDate.Today;
			usdLine1.AL_GC = usCompany.PK;
			var usdJobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			usdJobCharge1.JR_JH = usdJob.PK;
			usdJobCharge1.JR_AL_ARLine = usdLine1.PK;
			usdJobCharge1.JR_LocalSellAmt = 200;
			usdJobCharge1.JR_OSSellAmt = 200;

			var usdLine2 = Factory.NewWithValidTestData<AccTransactionLines>();
			usdLine2.AL_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			usdLine2.AL_AH = usdHeaderAP.PK;
			usdLine2.AL_JH = usdJob.PK;
			usdLine2.AL_AC = nonDsbCharge.PK;
			usdLine2.AL_AG = nonDsbCharge.AC_AG_CostAccount;
			usdLine2.AL_RX_NKTransactionCurrency = "USD";
			usdLine2.AL_LineType = "CST";
			usdLine2.AL_LineAmount = usdLine2.AL_OSAmount = -50;
			usdLine2.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			usdLine2.AL_PostDate = ZDate.Today;
			usdLine2.AL_ReverseDate = ZDate.Today;
			usdLine2.AL_GC = usCompany.PK;
			var usdJobCharge2 = Factory.NewWithValidTestData<JobCharge>();
			usdJobCharge2.JR_JH = usdJob.PK;
			usdJobCharge2.JR_AL_APLine = usdLine2.PK;
			usdJobCharge2.JR_LocalCostAmt = 50;
			usdJobCharge2.JR_OSCostAmt = 50;

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				//Sync twice to test duplicate value
				var synchroniser1 = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser1.Execute(summaryProvider, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
				synchroniser1.Execute(summaryProvider, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));

				var synchroniser2 = new TradeLinesSynchroniser(org2.PK);
				synchroniser2.Execute(summaryProvider, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
				synchroniser2.Execute(summaryProvider, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}
			TestCollection.Load();

			var matchedSales = CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", Organisation.PK.ToGuid(), org2.PK.ToGuid(), isActual: true);
			AssertEquals(1, matchedSales.Count());

			var sales = matchedSales.Single();

			var details = sales.TradeDetails.Cast<OrgTradeDetail>().First();

			var audJobValue = details.TradedPeriods.Single(x => x.PAS_IsJobValue && x.PAS_OH_Client == org2.PK).TradeValues.Single(x => x.PAV_RX_NKCurrency == "AUD");
			AssertEquals(100m, audJobValue.PAV_Revenue);
			AssertEquals(20m, audJobValue.PAV_Cost);

			var audOrgValue = details.TradedPeriods.Single(x => !x.PAS_IsJobValue && x.PAS_OH_Client == org2.PK).TradeValues.Single(x => x.PAV_RX_NKCurrency == "AUD");
			AssertEquals(auCompany.PK, audOrgValue.PAV_GC);
			AssertEquals(100m, audOrgValue.PAV_Revenue);
			AssertEquals(0m, audOrgValue.PAV_Cost);

			var usdJobValue = details.TradedPeriods.Single(x => x.PAS_IsJobValue && x.PAS_OH_Client == org2.PK).TradeValues.Single(x => x.PAV_RX_NKCurrency == "USD");
			AssertEquals(200m, usdJobValue.PAV_Revenue);
			AssertEquals(50m, usdJobValue.PAV_Cost);

			var usdOrgValue = details.TradedPeriods.Single(x => !x.PAS_IsJobValue && x.PAS_OH_Client == org2.PK).TradeValues.Single(x => x.PAV_RX_NKCurrency == "USD");
			AssertEquals(usCompany.PK, usdOrgValue.PAV_GC);
			AssertEquals(200m, usdOrgValue.PAV_Revenue);
			AssertEquals(0m, usdOrgValue.PAV_Cost);

			AssertEquals(1, TestCollection.Count);
			AssertEquals(1, sales.TradeDetails.Count);
			AssertEquals(4, sales.TradeDetails[0].TradedPeriods.Count);
			AssertEquals(2, details.TradedPeriods.Single(x => x.PAS_IsJobValue && x.PAS_OH_Client == org2.PK).TradeValues.Count);
			AssertEquals(2, details.TradedPeriods.Single(x => !x.PAS_IsJobValue && x.PAS_OH_Client == org2.PK).TradeValues.Count);
			AssertEquals(2, details.TradedPeriods.Single(x => x.PAS_IsJobValue && x.PAS_OH_Client == Organisation.PK).TradeValues.Count);
			AssertEquals(0, details.TradedPeriods.Single(x => !x.PAS_IsJobValue && x.PAS_OH_Client == Organisation.PK).TradeValues.Count);
		}

		[TestDate(2015, 1, 1)]
		public void TestShipment_WithPartitions()
		{
			Organisation.OH_Code = "TESTORGXXX";
			Organisation.OH_RL_NKClosestPort = "AUBNE";

			var newOrg = CollectionHelper.GetNewOrg("TESTORG");
			for (int i = 0; i < 6; i++)
			{
				CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", Organisation.PK, newOrg.PK, new ZDate(2014, 2, 1).AddMonths(i));
			}
			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summary = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var tester = new TradeLinesSynchroniserTester(Organisation.PK, 1);
				tester.Execute(summary, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}

			TestCollection.Load();
			AssertEquals("Should only create one trade lane", 1, TestCollection.Count);
		}

		[TestDate(2015, 1, 1)]
		public void TestValuesOverflow()
		{
			var registryValue = new MaximumAllowedTransactionAmount()
			{
				MaximumAllowedHeaderAmount = 1200000000000000M,
				MaximumAllowedLineAmount = 1200000000000000M
			};

			using (AccountingMasterFilesRegistry.Instance.SystemDefinedMaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			using (AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				Organisation.FillWithValidTestData();
				Organisation.OH_Code = "DDDAAASYD";
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.OH_Code = "DDDBBBSYD";

				var company = GlbCompany.CurrentCompany;
				company.GC_RN_NKCountryCode = "AU";
				company.GC_RX_NKLocalCurrency = "AUD";

				var periodTestHelper = new AccountingPeriodTestHelper(Factory);
				periodTestHelper.SetupSinglePeriod(201903, ZDate.Today.AddYears(-6), ZDate.Today.AddYears(1), company.PK);

				var nonDsbChargeQuery = new ZQuery();
				nonDsbChargeQuery.AddToFilter(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.NotEqual, "DSB");
				nonDsbChargeQuery.AddToFilter(AccChargeCodeSchema.AC_AG_RevenueAccount, SQLComparisonOperator.NotEqual, null);
				nonDsbChargeQuery.OrderBy = AccChargeCodeSchema.Constants.AC_Code;
				var nonDsbCharge = Factory.LoadTop1<AccChargeCode>(nonDsbChargeQuery);

				var shipment1 = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", Organisation.PK, org2.PK);
				shipment1[JobShipmentSchema.Constants.JS_ActualWeight] = 999999.999m;
				shipment1[JobShipmentSchema.Constants.JS_UnitOfWeight] = Constants.Weight.Kilotonnes;
				shipment1[JobShipmentSchema.Constants.JS_ActualVolume] = 999999.999m;
				shipment1[JobShipmentSchema.Constants.JS_UnitOfVolume] = Constants.Volume.CubicMetres;
				shipment1[JobShipmentSchema.Constants.JS_ActualChargeable] = 999999.999m;
				shipment1[JobShipmentSchema.Constants.JS_DocumentedChargeable] = 999999.999m;
				shipment1[JobShipmentSchema.Constants.JS_ManifestedChargeable] = 999999.999m;

				var job1 = new JobHeader.Loader((IJobHeaderParent)shipment1).TryCreate();
				var header1 = Factory.NewWithValidTestData<AccTransactionHeader>();
				header1.AH_Ledger = LedgerTypes.AccountsReceivable;
				header1.AH_TransactionType = TransactionTypes.Invoice;
				header1.AH_JH = job1.PK;
				header1.AH_GC = company.PK;
				var line1 = Factory.NewWithValidTestData<AccTransactionLines>();
				line1.AL_AH = header1.PK;
				line1.AL_JH = job1.PK;
				line1.AL_AC = nonDsbCharge.PK;
				line1.AL_AG = nonDsbCharge.AC_AG_RevenueAccount;
				line1.AL_RX_NKTransactionCurrency = "AUD";
				line1.AL_LineType = "REV";
				line1.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
				line1.AL_LineAmount = line1.AL_OSAmount = 599999999999999m;
				line1.AL_PostDate = ZDate.Today;
				line1.AL_ReverseDate = ZDate.Today;
				var jobCharge1 = Factory.NewWithValidTestData<JobCharge>();
				jobCharge1.JR_JH = job1.PK;
				jobCharge1.JR_AL_ARLine = line1.PK;
				jobCharge1.JR_LocalSellAmt = 599999999999999m;
				jobCharge1.JR_OSSellAmt = 599999999999999m;
				job1.JH_A_JCL = ZDate.Today;

				var shipment2 = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", Organisation.PK, org2.PK);
				shipment2[JobShipmentSchema.Constants.JS_ActualWeight] = 999999.999m;
				shipment2[JobShipmentSchema.Constants.JS_UnitOfWeight] = Constants.Weight.Kilotonnes;
				shipment2[JobShipmentSchema.Constants.JS_ActualVolume] = 999999.999m;
				shipment2[JobShipmentSchema.Constants.JS_UnitOfVolume] = Constants.Volume.CubicMetres;
				shipment2[JobShipmentSchema.Constants.JS_ActualChargeable] = 999999.999m;
				shipment2[JobShipmentSchema.Constants.JS_DocumentedChargeable] = 999999.999m;
				shipment2[JobShipmentSchema.Constants.JS_ManifestedChargeable] = 999999.999m;

				var job2 = new JobHeader.Loader((IJobHeaderParent)shipment2).TryCreate();
				var header2 = Factory.NewWithValidTestData<AccTransactionHeader>();
				header2.AH_Ledger = LedgerTypes.AccountsReceivable;
				header2.AH_TransactionType = TransactionTypes.Invoice;
				header2.AH_JH = job2.PK;
				header2.AH_GC = company.PK;
				var line2 = Factory.NewWithValidTestData<AccTransactionLines>();
				line2.AL_AH = header2.PK;
				line2.AL_JH = job2.PK;
				line2.AL_AC = nonDsbCharge.PK;
				line2.AL_AG = nonDsbCharge.AC_AG_RevenueAccount;
				line2.AL_RX_NKTransactionCurrency = "AUD";
				line2.AL_LineType = "REV";
				line2.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
				line2.AL_LineAmount = line2.AL_OSAmount = 599999999999999m;
				line2.AL_PostDate = ZDate.Today;
				line2.AL_ReverseDate = ZDate.Today;
				var jobCharge2 = Factory.NewWithValidTestData<JobCharge>();
				jobCharge2.JR_JH = job2.PK;
				jobCharge2.JR_AL_ARLine = line2.PK;
				jobCharge2.JR_LocalSellAmt = 599999999999999m;
				jobCharge2.JR_OSSellAmt = 599999999999999m;
				job2.JH_A_JCL = ZDate.Today;

				Factory.Save();

				var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
				jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

				using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
				{
					var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
					synchroniser.Execute(summaryProvider, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
				}
				TestCollection.Load();

				var sales = CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", Organisation.PK.ToGuid(), org2.PK.ToGuid(), isActual: true).Single();
				var details = sales.TradeDetails[0];
				AssertEquals(999999999.000m, details.TradedPeriods[0].PAS_Weight);
				AssertEquals(1999999.998m, details.TradedPeriods[0].PAS_Volume);
				AssertEquals(900000000000000.0000m, details.TradedPeriods.First(x => x.PAS_IsJobValue).TradeValues[0].PAV_Revenue);
			}
		}

		[TestDate(2018, 1, 1)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestZeroTradedValues()
		{
			Organisation.FillWithValidTestData();

			var auCompany = Factory.NewWithValidTestData<GlbCompany>();
			auCompany.GC_RN_NKCountryCode = "AU";
			auCompany.GC_RX_NKLocalCurrency = "AUD";

			var periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.SetupSinglePeriod(201903, ZDate.Today.AddYears(-6), ZDate.Today.AddYears(1), auCompany.PK);

			var nonDsbChargeQuery = new ZQuery();
			nonDsbChargeQuery.AddToFilter(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.NotEqual, "DSB");
			nonDsbChargeQuery.AddToFilter(AccChargeCodeSchema.AC_AG_RevenueAccount, SQLComparisonOperator.NotEqual, null);
			nonDsbChargeQuery.OrderBy = AccChargeCodeSchema.Constants.AC_Code;
			var nonDsbCharge = Factory.LoadTop1<AccChargeCode>(nonDsbChargeQuery);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var audShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", Organisation.PK, org2.PK);
			var audJob = new JobHeader.Loader((IJobHeaderParent)audShipment).TryCreate();
			audJob.JH_GC = auCompany.PK;
			var audHeaderAR = Factory.NewWithValidTestData<AccTransactionHeader>();
			audHeaderAR.AH_Ledger = LedgerTypes.AccountsReceivable;
			audHeaderAR.AH_TransactionType = TransactionTypes.Invoice;
			audHeaderAR.AH_JH = audJob.PK;
			audHeaderAR.AH_GC = auCompany.PK;
			audHeaderAR.AH_OH = org2.PK;
			audJob.JH_A_JCL = ZDate.Today;

			var audLine1 = Factory.NewWithValidTestData<AccTransactionLines>();
			audLine1.AL_AH = audHeaderAR.PK;
			audLine1.AL_JH = audJob.PK;
			audLine1.AL_AC = nonDsbCharge.PK;
			audLine1.AL_AG = nonDsbCharge.AC_AG_RevenueAccount;
			audLine1.AL_RX_NKTransactionCurrency = "AUD";
			audLine1.AL_LineType = "REV";
			audLine1.AL_LineAmount = audLine1.AL_OSAmount = 100;
			audLine1.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			audLine1.AL_PostDate = ZDate.Today;
			audLine1.AL_ReverseDate = ZDate.Today;
			audLine1.AL_GC = auCompany.PK;
			var audJobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			audJobCharge1.JR_JH = audJob.PK;
			audJobCharge1.JR_AL_ARLine = audLine1.PK;
			audJobCharge1.JR_LocalSellAmt = 100;
			audJobCharge1.JR_OSSellAmt = 100;

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2018, 1, 1), new ZDate(2018, 2, 1));
			}
			TestCollection.Load();

			var sales = CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", Organisation.PK.ToGuid(), org2.PK.ToGuid(), isActual: true).Single();
			var details = sales.TradeDetails.Cast<OrgTradeDetail>().First();
			var jobValue = details.TradedPeriods.Single(x => x.PAS_IsJobValue && x.PAS_OH_Client == Organisation.PK).TradeValues.Single();
			AssertEquals("Pre-condition", 100m, jobValue.PAV_Revenue);
			var orgValue = details.TradedPeriods.Single(x => !x.PAS_IsJobValue && x.PAS_OH_Client == Organisation.PK).TradeValues.FirstOrDefault();
			AssertNull("Zero value should not be created", orgValue);

			var audHeaderCredit = Factory.NewWithValidTestData<AccTransactionHeader>();
			audHeaderCredit.AH_Ledger = LedgerTypes.AccountsReceivable;
			audHeaderCredit.AH_TransactionType = TransactionTypes.CreditNote;
			audHeaderCredit.AH_JH = audJob.PK;
			audHeaderCredit.AH_GC = auCompany.PK;
			audHeaderCredit.AH_OH = org2.PK;

			var audLine2 = Factory.NewWithValidTestData<AccTransactionLines>();
			audLine2.AL_AH = audHeaderCredit.PK;
			audLine2.AL_JH = audJob.PK;
			audLine2.AL_AC = nonDsbCharge.PK;
			audLine2.AL_AG = nonDsbCharge.AC_AG_RevenueAccount;
			audLine2.AL_RX_NKTransactionCurrency = "AUD";
			audLine2.AL_LineType = "REV";
			audLine2.AL_LineAmount = audLine2.AL_OSAmount = -100;
			audLine2.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			audLine2.AL_PostDate = ZDate.Today;
			audLine2.AL_ReverseDate = ZDate.Today;
			audLine2.AL_GC = auCompany.PK;
			var audJobCharge2 = Factory.NewWithValidTestData<JobCharge>();
			audJobCharge2.JR_JH = audJob.PK;
			audJobCharge2.JR_AL_ARLine = audLine2.PK;
			audJobCharge2.JR_LocalSellAmt = -100;
			audJobCharge2.JR_OSSellAmt = -100;

			Factory.Save();

			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2018, 1, 1), new ZDate(2018, 2, 1));
			}

			var anotherFactory = new BusinessObjectFactory();
			var deletedJobValue = anotherFactory.Load<OrgTradeValue>(jobValue.PK);
			AssertNull("Should be deleted", deletedJobValue);

			var detailsInOtherFactory = anotherFactory.Load<OrgTradeDetail>(details.PK);
			jobValue = detailsInOtherFactory.TradedPeriods.Single(x => x.PAS_IsJobValue && x.PAS_OH_Client == Organisation.PK).TradeValues.FirstOrDefault();
			AssertNull("Zero value should be deleted and not re-created", jobValue);
			orgValue = detailsInOtherFactory.TradedPeriods.Single(x => !x.PAS_IsJobValue && x.PAS_OH_Client == Organisation.PK).TradeValues.FirstOrDefault();
			AssertNull("Zero value should not be created", orgValue);
		}

		[TestDate(2015, 1, 1)]
		public void TestWithSynchroisedActualsGreaterThan12MonthsAgo()
		{
			Organisation.FillWithValidTestData();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");
			var shipmentProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var actualSalesAuAu = TestCollection.AddNew();
			PopulateExistingLostTradeLane(actualSalesAuAu, shipmentProduct.Identifier, ausyd.PK, ausyd.PK, Organisation.PK, org2.PK);

			var actualSalesAuUs = TestCollection.AddNew();
			PopulateExistingLostTradeLane(actualSalesAuUs, shipmentProduct.Identifier, ausyd.PK, uslax.PK, Organisation.PK, org2.PK);

			var actualSalesUsUs = TestCollection.AddNew();
			PopulateExistingLostTradeLane(actualSalesUsUs, shipmentProduct.Identifier, uslax.PK, uslax.PK, Organisation.PK, org2.PK);

			var prospectSalesAuUs = TestCollection.AddNew();
			using (prospectSalesAuUs.GetDefaultPropertySuspender())
			{
				prospectSalesAuUs.OW_IsTraded = false;
				prospectSalesAuUs.OW_MP_Product = shipmentProduct.Identifier;
				prospectSalesAuUs.OW_OriginID = ausyd.PK;
				prospectSalesAuUs.OW_DestinationID = uslax.PK;
				prospectSalesAuUs.OW_OH_Buyer = Organisation.PK;
				prospectSalesAuUs.OW_OH_Supplier = org2.PK;
			}

			var prospectSalesAuNz = TestCollection.AddNew();
			using (prospectSalesAuNz.GetDefaultPropertySuspender())
			{
				prospectSalesAuNz.OW_IsTraded = false;
				prospectSalesAuNz.OW_MP_Product = shipmentProduct.Identifier;
				prospectSalesAuNz.OW_OriginID = ausyd.PK;
				prospectSalesAuNz.OW_DestinationID = nzakl.PK;
				prospectSalesAuNz.OW_OH_Buyer = Organisation.PK;
				prospectSalesAuNz.OW_OH_Supplier = org2.PK;
			}
			var prospectSalesAuNzPk = prospectSalesAuNz.PK;

			var legacyProspectSalesNzNz = TestCollection.AddNew();
			using (legacyProspectSalesNzNz.GetDefaultPropertySuspender())
			{
				legacyProspectSalesNzNz.OW_IsTraded = false;
				legacyProspectSalesNzNz.OW_MP_Product = shipmentProduct.Identifier;
				legacyProspectSalesNzNz.OW_OriginID = nzakl.PK;
				legacyProspectSalesNzNz.OW_DestinationID = nzakl.PK;
				legacyProspectSalesNzNz.OW_OH_Primary = ZGuid.Empty;
				legacyProspectSalesNzNz.OW_OH_Buyer = Organisation.PK;
				legacyProspectSalesNzNz.OW_OH_Supplier = org2.PK;
			}

			CollectionHelper.CreateShipment("USLAX", "USLAX", "AIR", "LSE", Organisation.PK, org2.PK);
			CollectionHelper.CreateShipment("AUSYD", "NZAKL", "AIR", "LSE", Organisation.PK, org2.PK);
			CollectionHelper.CreateShipment("NZAKL", "NZAKL", "AIR", "LSE", Organisation.PK, org2.PK);

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}
			TestCollection.Load();

			AssertEquals("Should have created prospect for old actuals data that is no longer being traded", 1, CollectionHelper.FindOrgSales("SHP", "AUSYD", "AUSYD", isActual: false).Count());
			AssertContainsExactElementsInAnyOrder("Should not have created a new prospect lane for AUSYD>USLAX since one already exists", new[] { prospectSalesAuUs }, CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", isActual: false));
			AssertContainsExactElementsInAnyOrder("Should not have created a new prospect lane for USLAX>USLAX since still trading", Enumerable.Empty<OrgSales>(), CollectionHelper.FindOrgSales("SHP", "USLAX", "USLAX", isActual: false));

			var prospectSalesAuNzDeleted = TestCollection.FirstOrDefault(x => x.PK == prospectSalesAuNzPk);
			AssertEquals("Should delete prospect without any associations if it is currently trading", null, prospectSalesAuNzDeleted);
			AssertContainsExactElementsInAnyOrder("Should delete prospect without any associations if it is currently trading", Enumerable.Empty<OrgSales>(), CollectionHelper.FindOrgSales("SHP", "AUSYD", "NZAKL", isActual: false));

			legacyProspectSalesNzNz.Reload();
			AssertEquals("Should just hide legacy prospect instead of deleting", false, legacyProspectSalesNzNz.IsDeleted);
			AssertEquals("Should just hide legacy prospect instead of deleting", OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation, legacyProspectSalesNzNz.OW_OH_Primary);

			var filter = new ZQuery(OrgSalesSchema.PK, legacyProspectSalesNzNz.PK);
			filter.ReLoadExistingRows = true;
			legacyProspectSalesNzNz = Factory.LoadTop1<OrgSales>(filter);
			AssertNotNull("Should just hide legacy prospect instead of deleting", legacyProspectSalesNzNz);
		}

		[TestDate(2023, 8, 10)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAddAndRemoveProspectsForCurrentActuals()
		{
			Organisation.FillWithValidTestData();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var frpar = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "FRPAR");
			var shipmentProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Eaglets Pty Ltd";
			company.GC_RN_NKCountryCode = country.Code;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = company.PK;

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "LOCALORG1";
			localClient.OH_RL_NKClosestPort = "AUBNE";

			var shipment1 = CollectionHelper.CreateShipment("AUSYD", "FRPAR", "SEA", "FCL", Organisation.PK, org1.PK, new ZDate(2023, 6, 7));
			shipment1[JobShipmentSchema.JS_ActualWeight] = 1000;
			shipment1[JobShipmentSchema.JS_UnitOfWeight] = "KG";

			var job1 = TradeLinesSummaryProviderCommonTest.CreateJobHeader((IJobHeaderParent)shipment1, localClient, company.PK);
			TradeLinesSummaryProviderCommonTest.CreateCharge(Factory, job1, localClient, chargeCode, TransactionLineTypes.Revenue, 1000);

			var shipment2 = CollectionHelper.CreateShipment("AUSYD", "USLAX", "SEA", "FCL", Organisation.PK, org1.PK, new ZDate(2023, 7, 14));
			shipment2[JobShipmentSchema.JS_ActualWeight] = 1000;
			shipment2[JobShipmentSchema.JS_UnitOfWeight] = "KG";

			var job2 = TradeLinesSummaryProviderCommonTest.CreateJobHeader((IJobHeaderParent)shipment2, localClient, company.PK);
			TradeLinesSummaryProviderCommonTest.CreateCharge(Factory, job2, localClient, chargeCode, TransactionLineTypes.Revenue, 2000);

			var prospectSalesAuUs = TestCollection.AddNew();
			using (prospectSalesAuUs.GetDefaultPropertySuspender())
			{
				prospectSalesAuUs.OW_IsTraded = false;
				prospectSalesAuUs.OW_MP_Product = shipmentProduct.Identifier;
				prospectSalesAuUs.OW_OriginID = ausyd.PK;
				prospectSalesAuUs.OW_DestinationID = uslax.PK;
				prospectSalesAuUs.OW_OH_Buyer = Organisation.PK;
				prospectSalesAuUs.OW_OH_Supplier = org1.PK;
			}

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Prospect is existing", new[] { prospectSalesAuUs }, CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", isActual: false));

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2023, 7, 1), new ZDate(2023, 8, 1));
			}
			TestCollection.Load();

			AssertContainsExactElementsInAnyOrder("Should delete prospect without any associations if it is currently trading", Enumerable.Empty<OrgSales>(), CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", isActual: false));

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2023, 6, 1), new ZDate(2023, 7, 1));
			}
			TestCollection.Load();

			AssertContainsExactElementsInAnyOrder("Should delete prospect without any associations if it is currently trading", Enumerable.Empty<OrgSales>(), CollectionHelper.FindOrgSales("SHP", "AUSYD", "FRPAR", isActual: false));
			AssertContainsExactElementsInAnyOrder("Should not create prospect as it is currently trading", Enumerable.Empty<OrgSales>(), CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", isActual: false));

			prospectSalesAuUs = TestCollection.AddNew();
			using (prospectSalesAuUs.GetDefaultPropertySuspender())
			{
				prospectSalesAuUs.OW_IsTraded = false;
				prospectSalesAuUs.OW_MP_Product = shipmentProduct.Identifier;
				prospectSalesAuUs.OW_OriginID = ausyd.PK;
				prospectSalesAuUs.OW_DestinationID = uslax.PK;
				prospectSalesAuUs.OW_OH_Buyer = Organisation.PK;
				prospectSalesAuUs.OW_OH_Supplier = org1.PK;
			}

			var prospectSalesAuFr = TestCollection.AddNew();
			using (prospectSalesAuFr.GetDefaultPropertySuspender())
			{
				prospectSalesAuFr.OW_IsTraded = false;
				prospectSalesAuFr.OW_MP_Product = shipmentProduct.Identifier;
				prospectSalesAuFr.OW_OriginID = ausyd.PK;
				prospectSalesAuFr.OW_DestinationID = frpar.PK;
				prospectSalesAuFr.OW_OH_Buyer = Organisation.PK;
				prospectSalesAuFr.OW_OH_Supplier = org1.PK;
			}

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Prospect is existing", new[] { prospectSalesAuUs }, CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", isActual: false));
			AssertContainsExactElementsInAnyOrder("Prospect is existing", new[] { prospectSalesAuFr }, CollectionHelper.FindOrgSales("SHP", "AUSYD", "FRPAR", isActual: false));

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2023, 7, 1), new ZDate(2023, 8, 1));
			}
			TestCollection.Load();

			AssertContainsExactElementsInAnyOrder("Should delete prospect without any associations if it is currently trading", Enumerable.Empty<OrgSales>(), CollectionHelper.FindOrgSales("SHP", "AUSYD", "FRPAR", isActual: false));
			AssertContainsExactElementsInAnyOrder("Should delete prospect without any associations if it is currently trading", Enumerable.Empty<OrgSales>(), CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", isActual: false));

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2023, 6, 1), new ZDate(2023, 7, 1));
			}
			TestCollection.Load();

			AssertContainsExactElementsInAnyOrder("No change to prospects", Enumerable.Empty<OrgSales>(), CollectionHelper.FindOrgSales("SHP", "AUSYD", "FRPAR", isActual: false));
			AssertContainsExactElementsInAnyOrder("No change to prospects", Enumerable.Empty<OrgSales>(), CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", isActual: false));
		}

		[TestDate(2015, 1, 1)]
		public void TestWithSynchroisedActualsGreaterThan12MonthsAgo_Brokerage()
		{
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");
			var gblon = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "GBLON");
			var brokerageProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);

			Organisation.OH_RL_NKClosestPort = "AUSYD";
			Organisation.FillWithValidTestData();

			var usOrg = Factory.NewWithValidTestData<OrgHeader>();
			usOrg.OH_RL_NKClosestPort = "USLAX";

			var gbOrg = Factory.NewWithValidTestData<OrgHeader>();
			gbOrg.OH_RL_NKClosestPort = "GBLON";

			var actualSalesAuUs = TestCollection.AddNew();
			PopulateExistingLostTradeLane(actualSalesAuUs, brokerageProduct.Identifier, ausyd.PK, uslax.PK, usOrg.PK, Organisation.PK);

			var actualSalesNzUs = TestCollection.AddNew();
			PopulateExistingLostTradeLane(actualSalesNzUs, brokerageProduct.Identifier, nzakl.PK, uslax.PK, Organisation.PK, usOrg.PK);

			var actualSalesGbAu = TestCollection.AddNew();
			PopulateExistingLostTradeLane(actualSalesGbAu, brokerageProduct.Identifier, gblon.PK, ausyd.PK, Organisation.PK, gbOrg.PK);

			var prospectSalesAu = TestCollection.AddNew();
			using (prospectSalesAu.GetDefaultPropertySuspender())
			{
				prospectSalesAu.OW_IsTraded = false;
				prospectSalesAu.OW_MP_Product = brokerageProduct.Identifier;
				prospectSalesAu.OW_OriginID = ausyd.PK;
				prospectSalesAu.OW_OH_Buyer = Organisation.PK;
				prospectSalesAu.OW_OH_Supplier = usOrg.PK;
			}

			var prospectSalesNz = TestCollection.AddNew();
			var prospectSalesNzPk = prospectSalesNz.PK;
			using (prospectSalesNz.GetDefaultPropertySuspender())
			{
				prospectSalesNz.OW_IsTraded = false;
				prospectSalesNz.OW_MP_Product = brokerageProduct.Identifier;
				prospectSalesNz.OW_OriginID = nzakl.PK;
				prospectSalesNz.OW_OH_Buyer = Organisation.PK;
				prospectSalesNz.OW_OH_Supplier = usOrg.PK;
			}

			CollectionHelper.CreateDeclaration("USLAX", "NZAKL", usOrg.PK, Organisation.PK, Core.Constants.TransportModes.Other);
			CollectionHelper.CreateDeclaration("USLAX", "USLAX", usOrg.PK, Organisation.PK, Core.Constants.TransportModes.Air);

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}
			TestCollection.Load();

			AssertEquals("Should have created prospect for old actuals data that is no longer being traded", 1, CollectionHelper.FindOrgSales("BRK", "GBLON", "", isActual: false).Count());
			AssertContainsExactElementsInAnyOrder("Should not have created a new prospect lane for AUSYD since one already exists", new[] { prospectSalesAu }, CollectionHelper.FindOrgSales("BRK", "AUSYD", "", isActual: false));
			AssertContainsExactElementsInAnyOrder("Should not have created a new prospect lane for USLAX since still trading", Enumerable.Empty<OrgSales>(), CollectionHelper.FindOrgSales("BRK", "USLAX", "", isActual: false));

			var prospectSalesNzDeleted = TestCollection.FirstOrDefault(x => x.PK == prospectSalesNzPk);
			AssertEquals("Should delete prospect without any associations if it is currently trading", null, prospectSalesNzDeleted);
			AssertContainsExactElementsInAnyOrder("Should delete prospect without any associations if it is currently trading", Enumerable.Empty<OrgSales>(), CollectionHelper.FindOrgSales("BRK", "NZAKL", "", isActual: false));
		}

		void PopulateExistingLostTradeLane(OrgSales existingSales, ZGuid productPk, ZGuid originPk, ZGuid destinationPk, ZGuid buyerPk, ZGuid supplierPk)
		{
			existingSales.OW_IsTraded = true;
			existingSales.OW_MP_Product = productPk;
			existingSales.OW_OriginID = originPk;
			existingSales.OW_DestinationID = destinationPk;
			existingSales.OW_OH_Buyer = buyerPk;
			existingSales.OW_OH_Supplier = supplierPk;
			var existingDetail = existingSales.TradeDetails.AddNew();
			existingDetail.PA_TradeMode = "SEA";
			existingDetail.PA_TradeType = "FCL";
			var existingPeriod = existingDetail.TradedPeriods.AddNew();
			existingPeriod.PAS_OH_Client = buyerPk;
			existingPeriod.PAS_Period = new ZDate(1900, 1, 1);
			existingPeriod.PAS_LastTraded = new ZDate(1900, 1, 1);
			existingPeriod.PAS_IsJobValue = true;
		}

		[TestDate(2015, 1, 1)]
		public void TestWithSynchroisedActualsGreaterThan12MonthsAgo_MultipleInDifferentPeriods()
		{
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var shipmentProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			Organisation.OH_RL_NKClosestPort = "AUSYD";
			Organisation.FillWithValidTestData();

			var usOrg = Factory.NewWithValidTestData<OrgHeader>();
			usOrg.OH_RL_NKClosestPort = "USLAX";

			var shipment_2001 = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", Organisation.PK, Organisation.PK, new ZDateTime(2001, 1, 1));
			var shipment_2002 = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", Organisation.PK, Organisation.PK, new ZDateTime(2002, 1, 1));

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2000, 2, 1), new ZDate(2015, 2, 1));
			}
			TestCollection.Load();

			AssertEquals("Should have created only 1 prospect for old actuals data that is no longer being traded", 1, CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", isActual: false).Count());
		}

		[TestDate(2015, 1, 1)]
		public void TestCancelledQuotesAreNotUsedForSynchronisation()
		{
			Organisation.OH_Code = "TESTORGXXX";
			Organisation.OH_RL_NKClosestPort = "AUBNE";

			BusinessObject newQuote = Factory.New(ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().QuoteType);
			newQuote[RatingHeaderSchema.TH_OH.Name] = Organisation.PK;
			CollectionHelper.AddRateEntry(newQuote, "AIR", "AUSYD", "USLAX", Guid.Empty, Guid.Empty);
			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}
			TestCollection.Load();
			AssertEquals(1, TestCollection.Count);

			TestCollection.RemoveAndDeleteAll();

			CollectionHelper.CancelQuote(newQuote);
			Factory.Save();

			Assert((ZBool)newQuote[RatingHeaderSchema.TH_IsCancelled]);
			AssertEquals(0, TestCollection.Count);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}
			TestCollection.Load();

			AssertEquals(0, TestCollection.Count);
		}

		[TestDate(2015, 1, 1)]
		public void TestCoLoadMasterShipmentShouldBeTakenIntoAccount()
		{
			Organisation.OH_Code = "TESTORGXXX";
			Organisation.OH_RL_NKClosestPort = "AUBNE";

			BusinessObject coLoadMasterShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", "CLD", Organisation.PK, Organisation.PK, ZDateTime.Today, false);

			for (int i = 0; i < 6; i++)
			{
				OrgHeader newOrg = CollectionHelper.GetNewOrg("TESTORG" + i + i);

				BusinessObject airShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", Organisation.PK, newOrg.PK);
				airShipment[JobShipmentSchema.JS_ActualWeight.Name] = i * 100m;
				airShipment[JobShipmentSchema.JS_JS_ColoadMasterShipment.Name] = coLoadMasterShipment.PK;

				BusinessObject lclShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "SEA", "LCL", Organisation.PK, newOrg.PK);
				lclShipment[JobShipmentSchema.JS_ActualVolume.Name] = i * 0.5m;
				lclShipment[JobShipmentSchema.JS_JS_ColoadMasterShipment.Name] = coLoadMasterShipment.PK;
			}
			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summary = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summary, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}
			TestCollection.Load();

			AssertEquals(6, TestCollection.Count);

			var airDetails = TestCollection.Cast<OrgSales>().SelectMany(x => x.TradeDetails.Cast<OrgTradeDetail>()).Where(x => x.PA_TradeMode == Constants.TransportModes.Air);
			var airJobPeriods = airDetails.SelectMany(x => x.TradedPeriods).Where(x => x.PAS_IsJobValue);
			AssertEquals((ZDecimal)6, airJobPeriods.Sum(x => x.PAS_RepeatsMnth));
			AssertEquals(1.5m, airJobPeriods.Sum(x => x.PAS_Weight));

			var lclDetails = TestCollection.Cast<OrgSales>().SelectMany(x => x.TradeDetails.Cast<OrgTradeDetail>()).Where(x => x.PA_TradeMode == Constants.TransportModes.Sea);
			var lclJobPeriods = lclDetails.SelectMany(x => x.TradedPeriods).Where(x => x.PAS_IsJobValue);
			AssertEquals((ZDecimal)6, lclJobPeriods.Sum(x => x.PAS_RepeatsMnth));
			AssertEquals(7.5m, lclJobPeriods.Sum(x => x.PAS_Volume));
		}

		[TestDate(2015, 1, 1)]
		public void TestSynchronisationWithProspectiveLines()
		{
			Organisation.OH_Code = "TSTORGDDD";
			Organisation.OH_RL_NKClosestPort = "AUBNE";

			var consignor = CollectionHelper.GetNewOrg("TSTORGEEE");
			consignor.OH_RL_NKClosestPort = "AUSYD";

			var airShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", "STD", Organisation.PK, consignor.PK, new ZDateTime(2014, 6, 22, 11, 30, 0), false);
			airShipment[JobShipmentSchema.JS_ActualWeight.Name] = 1000m;
			airShipment[JobShipmentSchema.JS_INCO.Name] = "FOB";

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = airShipment.PK;
			jobHeader.JH_ParentTableCode = "JS";
			jobHeader.JH_GC = Env.CurrentCompany.PK;
			jobHeader.JH_GB = Env.CurrentBranch.PK;
			jobHeader.JH_GE = Env.CurrentDepartment.PK;
			jobHeader.JH_JobNum = "Job1";
			var jobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge1.JR_JH = jobHeader.PK;
			jobCharge1.JR_LocalSellAmt = 1000m;
			jobCharge1.JR_OSSellAmt = 1000m;
			jobCharge1.JR_AC = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "ODOC")).First(x => x.AC_GC == Env.CurrentCompany.PK).PK;
			var jobCharge2 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge2.JR_JH = jobHeader.PK;
			jobCharge2.JR_LocalSellAmt = 200m;
			jobCharge2.JR_OSSellAmt = 200m;
			jobCharge2.JR_AC = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "DDOC")).First(x => x.AC_GC == Env.CurrentCompany.PK).PK;

			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");

			var sales = Factory.New<OrgSales>();
			sales.OW_IsTraded = false;
			sales.OW_OH_Primary = Organisation.PK;
			sales.OW_OH_Buyer = Organisation.PK;
			sales.OW_OriginID = ausyd.PK;
			sales.OW_OriginTableCode = "RL";
			sales.OW_DestinationID = uslax.PK;
			sales.OW_DestinationTableCode = "RL";
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			TestCollection.Add(sales);

			var tradeDetail1a = sales.TradeDetails.AddNew();
			tradeDetail1a.PA_TradeMode = Constants.TransportModes.Sea;
			tradeDetail1a.PA_TradeType = Constants.ContainerModes.LCL;
			var period1a = tradeDetail1a.CurrentProspectPeriod;
			period1a.PAS_RepeatsMnth = 1;
			period1a.PAS_Units = 200;
			period1a.PAS_OH_Client = Organisation.PK;

			var tradeDetail1b = sales.TradeDetails.AddNew();
			tradeDetail1b.PA_TradeMode = Constants.TransportModes.Air;
			tradeDetail1b.PA_TradeType = Constants.ContainerModes.ULD;
			var period1b = tradeDetail1b.CurrentProspectPeriod;
			period1b.PAS_RepeatsMnth = 4;
			period1b.PAS_Units = 500;
			period1b.PAS_OH_Client = Organisation.PK;

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection, true, false))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}
			TestCollection.Load();

			var salesList = TestCollection.Cast<OrgSales>().Where(x => x.OW_OriginID == ausyd.PK && x.OW_DestinationID == uslax.PK).ToList();
			AssertEquals(2, salesList.Count);

			var sales1 = salesList.Single(x => !x.OW_IsTraded);
			tradeDetail1a = sales1.TradeDetails.OfType<OrgTradeDetail>().First(x => x.PA_TradeMode == Constants.TransportModes.Sea);
			AssertEquals(1m, tradeDetail1a.CurrentProspectPeriod.PAS_RepeatsMnth);
			AssertEquals(200L, tradeDetail1a.CurrentProspectPeriod.PAS_Units);

			tradeDetail1b = sales1.TradeDetails.OfType<OrgTradeDetail>().First(x => x.PA_TradeMode == Constants.TransportModes.Air);
			AssertEquals(4m, tradeDetail1b.CurrentProspectPeriod.PAS_RepeatsMnth);
			AssertEquals(500L, tradeDetail1b.CurrentProspectPeriod.PAS_Units);

			var sales2 = salesList.Single(x => x.OW_IsTraded);
			var tradeDetail2a = sales2.TradeDetails.OfType<OrgTradeDetail>().First(x => x.PA_TradeMode == Constants.TransportModes.Air);
			var tradePeriod2a = tradeDetail2a.TradedPeriods.Single(x => x.PAS_IsJobValue);
			AssertEquals(new ZDate(2014, 6, 1), tradePeriod2a.PAS_Period);
			AssertEquals(1L, tradePeriod2a.PAS_Units);

			var collection = new OrgSalesCollection(consignor);
			using (var summary = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summary, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}
			collection.Reload(true);

			AssertEquals(1, collection.Count);

			var sales3 = salesList.Single(x => x.OW_IsTraded);
			var tradeDetail3a = CollectionHelper.FindOrgTradeDetailByMode(sales3, Constants.TransportModes.Air);
			var tradePeriod3a = tradeDetail3a.TradedPeriods.Single(x => x.PAS_IsJobValue);
			AssertEquals(new ZDate(2014, 6, 1), tradePeriod3a.PAS_Period);
			AssertEquals(1L, tradePeriod3a.PAS_Units);
		}

		public void TestSynchronisationForLinerAndAngency()
		{
			Organisation.OH_Code = "TSTORGDDD";
			Organisation.OH_RL_NKClosestPort = "AUBNE";

			var consignor = CollectionHelper.GetNewOrg("TSTORGEEE");
			consignor.OH_RL_NKClosestPort = "AUSYD";

			#region LGY Shipment1
			var lGYShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "BOL", "FCL", Organisation.PK, consignor.PK, new ZDateTime(2017, 08, 12, 10, 30, 0), true);
			lGYShipment[JobShipmentSchema.JS_ActualWeight.Name] = 1000m;
			lGYShipment[JobShipmentSchema.JS_INCO.Name] = "FOB";
			lGYShipment[JobShipmentSchema.JS_ShipmentStatus] = "ESI";
			lGYShipment[JobShipmentSchema.JS_IsShipping] = true;

			var lGYjobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			lGYjobHeader.JH_ParentID = lGYShipment.PK;
			lGYjobHeader.JH_ParentTableCode = "JS";
			lGYjobHeader.JH_GC = Env.CurrentCompany.PK;
			lGYjobHeader.JH_GB = Env.CurrentBranch.PK;
			lGYjobHeader.JH_GE = Env.CurrentDepartment.PK;
			lGYjobHeader.JH_JobNum = "Job1";

			var lGYjobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			lGYjobCharge1.JR_JH = lGYjobHeader.PK;
			lGYjobCharge1.JR_LocalSellAmt = 1000m;
			lGYjobCharge1.JR_OSSellAmt = 1000m;
			lGYjobCharge1.JR_AC = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "ODOC")).First(x => x.AC_GC == Env.CurrentCompany.PK).PK;

			var lGYjobCharge2 = Factory.NewWithValidTestData<JobCharge>();
			lGYjobCharge2.JR_JH = lGYjobHeader.PK;
			lGYjobCharge2.JR_LocalSellAmt = 200m;
			lGYjobCharge2.JR_OSSellAmt = 200m;
			lGYjobCharge2.JR_AC = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "DDOC")).First(x => x.AC_GC == Env.CurrentCompany.PK).PK;

			#endregion

			#region LGY Shipment2
			var lGYShipment2 = CollectionHelper.CreateShipment("AUBNE", "USLAX", "BRK", "LCL", Organisation.PK, consignor.PK, new ZDateTime(2017, 08, 13, 11, 30, 0), true);
			lGYShipment2[JobShipmentSchema.JS_ActualWeight.Name] = 1000m;
			lGYShipment2[JobShipmentSchema.JS_INCO.Name] = "FOB";
			lGYShipment2[JobShipmentSchema.JS_ShipmentStatus] = "BKD";
			lGYShipment2[JobShipmentSchema.JS_IsShipping] = true;

			var lGYjobHeader2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			lGYjobHeader2.JH_ParentID = lGYShipment2.PK;
			lGYjobHeader2.JH_ParentTableCode = "JS";
			lGYjobHeader2.JH_GC = Env.CurrentCompany.PK;
			lGYjobHeader2.JH_GB = Env.CurrentBranch.PK;
			lGYjobHeader2.JH_GE = Env.CurrentDepartment.PK;
			lGYjobHeader2.JH_JobNum = "Job2";

			var lGYjobCharge2_1 = Factory.NewWithValidTestData<JobCharge>();
			lGYjobCharge2_1.JR_JH = lGYjobHeader2.PK;
			lGYjobCharge2_1.JR_LocalSellAmt = 1000m;
			lGYjobCharge2_1.JR_OSSellAmt = 1000m;
			lGYjobCharge2_1.JR_AC = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "ODOC")).First(x => x.AC_GC == Env.CurrentCompany.PK).PK;

			var lGYjobCharge2_2 = Factory.NewWithValidTestData<JobCharge>();
			lGYjobCharge2_2.JR_JH = lGYjobHeader2.PK;
			lGYjobCharge2_2.JR_LocalSellAmt = 200m;
			lGYjobCharge2_2.JR_OSSellAmt = 200m;
			lGYjobCharge2_2.JR_AC = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "DDOC")).First(x => x.AC_GC == Env.CurrentCompany.PK).PK;

			#endregion

			#region Forwarding Shipment
			var forwardingShipment = CollectionHelper.CreateShipment("AUSYD", "DEBER", "SEA", "FCL", "STD", Organisation.PK, consignor.PK, new ZDateTime(2017, 08, 15, 10, 30, 0), false);
			forwardingShipment[JobShipmentSchema.JS_ActualWeight.Name] = 1000m;
			forwardingShipment[JobShipmentSchema.JS_INCO.Name] = "FOB";
			forwardingShipment[JobShipmentSchema.JS_IsShipping] = false;

			var forwardingjobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			forwardingjobHeader.JH_ParentID = forwardingShipment.PK;
			forwardingjobHeader.JH_ParentTableCode = "JS";
			forwardingjobHeader.JH_GC = Env.CurrentCompany.PK;
			forwardingjobHeader.JH_GB = Env.CurrentBranch.PK;
			forwardingjobHeader.JH_GE = Env.CurrentDepartment.PK;
			forwardingjobHeader.JH_JobNum = "Job3";

			var forwardingJobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			forwardingJobCharge1.JR_JH = forwardingjobHeader.PK;
			forwardingJobCharge1.JR_LocalSellAmt = 1000m;
			forwardingJobCharge1.JR_OSSellAmt = 1000m;
			forwardingJobCharge1.JR_AC = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "ODOC")).First(x => x.AC_GC == Env.CurrentCompany.PK).PK;

			var forwardingJobCharge2 = Factory.NewWithValidTestData<JobCharge>();
			forwardingJobCharge2.JR_JH = forwardingjobHeader.PK;
			forwardingJobCharge2.JR_LocalSellAmt = 500m;
			forwardingJobCharge2.JR_OSSellAmt = 500m;
			forwardingJobCharge2.JR_AC = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "DDOC")).First(x => x.AC_GC == Env.CurrentCompany.PK).PK;

			#endregion

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection, true, true))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2017, 08, 01), new ZDate(2017, 08, 29));
			}
			TestCollection.Load();

			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var aubne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
			var deber = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "DEBER");

			var salesList = TestCollection.Cast<OrgSales>().Where(x => x.OW_OriginID == ausyd.PK && x.OW_DestinationID == uslax.PK && x.IsActual).ToList();
			AssertEquals(1, salesList.Count);

			salesList = TestCollection.Cast<OrgSales>().Where(x => x.OW_OriginID == aubne.PK && x.OW_DestinationID == uslax.PK && x.IsActual).ToList();
			AssertEquals(1, salesList.Count);

			salesList = TestCollection.Cast<OrgSales>().Where(x => x.ProductCode == SystemDefinedSalesProductList.Codes.LinerAgency && x.IsActual).ToList();
			AssertEquals(2, salesList.Count);

			var tradeDetails = salesList.SelectMany(x => x.TradeDetails.Cast<OrgTradeDetail>());
			AssertEquals(2, tradeDetails.Count());

			var billOfLading = tradeDetails.FirstOrDefault(p => p.PA_TradeMode == "BOL");
			AssertEquals("FCL", billOfLading.PA_TradeType);

			var booking = tradeDetails.FirstOrDefault(p => p.PA_TradeMode == "BRK");
			AssertEquals("LCL", booking.PA_TradeType);

			salesList = TestCollection.Cast<OrgSales>().Where(x => x.OW_OriginID == ausyd.PK && x.OW_DestinationID == deber.PK && x.IsActual).ToList();
			AssertEquals(1, salesList.Count);

			salesList = TestCollection.Cast<OrgSales>().Where(x => x.ProductCode == SystemDefinedSalesProductList.Codes.ForwardingShipment && x.IsActual).ToList();
			AssertEquals(1, salesList.Count);

			tradeDetails = salesList.SelectMany(x => x.TradeDetails.Cast<OrgTradeDetail>());
			AssertEquals(1, tradeDetails.Count());

			var forwarding = CollectionHelper.FindOrgTradeDetailByMode(salesList[0], Constants.TransportModes.Sea);
			AssertEquals("FCL", forwarding.PA_TradeType);
		}

		[TestDate(2017, 12, 1)]
		public void TestSynchronisationWithProspectiveLinesAndQuotation()
		{
			Organisation.OH_Code = "TSTORGDDD";
			Organisation.OH_RL_NKClosestPort = "AUBNE";

			var consignor = CollectionHelper.GetNewOrg("TSTORGEEE");
			consignor.OH_RL_NKClosestPort = "AUSYD";

			#region CASE 1

			var shipment1 = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", "STD", Organisation.PK, consignor.PK, new ZDateTime(2017, 08, 01, 11, 30, 0), false);
			shipment1[JobShipmentSchema.JS_ActualWeight.Name] = 1000m;
			shipment1[JobShipmentSchema.JS_INCO.Name] = "FOB";

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment1.PK;
			jobHeader.JH_ParentTableCode = "JS";
			jobHeader.JH_GC = Env.CurrentCompany.PK;
			jobHeader.JH_GB = Env.CurrentBranch.PK;
			jobHeader.JH_GE = Env.CurrentDepartment.PK;
			jobHeader.JH_JobNum = "Job1";
			var jobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge1.JR_JH = jobHeader.PK;
			jobCharge1.JR_LocalSellAmt = 1000m;
			jobCharge1.JR_OSSellAmt = 1000m;
			jobCharge1.JR_AC = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "ODOC")).First(x => x.AC_GC == Env.CurrentCompany.PK).PK;
			var jobCharge2 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge2.JR_JH = jobHeader.PK;
			jobCharge2.JR_LocalSellAmt = 200m;
			jobCharge2.JR_OSSellAmt = 200m;
			jobCharge2.JR_AC = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "DDOC")).First(x => x.AC_GC == Env.CurrentCompany.PK).PK;

			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");

			var sale1 = Factory.New<OrgSales>();
			sale1.OW_IsTraded = false;
			sale1.OW_OH_Primary = Organisation.PK;
			sale1.OW_OH_Buyer = Organisation.PK;
			sale1.OW_OriginID = ausyd.PK;
			sale1.OW_OriginTableCode = "RL";
			sale1.OW_DestinationID = uslax.PK;
			sale1.OW_DestinationTableCode = "RL";
			sale1.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			TestCollection.Add(sale1);

			var tradeDetail1a = sale1.TradeDetails.AddNew();
			tradeDetail1a.PA_TradeMode = Constants.TransportModes.Sea;
			tradeDetail1a.PA_TradeType = Constants.ContainerModes.LCL;
			tradeDetail1a.CurrentProspectPeriod.PAS_RepeatsMnth = 1;
			tradeDetail1a.CurrentProspectPeriod.PAS_Units = 200;

			var tradeDetail1b = sale1.TradeDetails.AddNew();
			tradeDetail1b.PA_TradeMode = Constants.TransportModes.Air;
			tradeDetail1b.PA_TradeType = Constants.ContainerModes.Loose;
			tradeDetail1b.CurrentProspectPeriod.PAS_RepeatsMnth = 4;
			tradeDetail1b.CurrentProspectPeriod.PAS_Units = 500;

			var tradeDetail1c = sale1.TradeDetails.AddNew();
			tradeDetail1c.PA_TradeMode = Constants.TransportModes.Rail;
			tradeDetail1c.PA_TradeType = Constants.ContainerModes.LCL;
			tradeDetail1c.CurrentProspectPeriod.PAS_RepeatsMnth = 3;
			tradeDetail1c.CurrentProspectPeriod.PAS_Units = 700;

			#endregion

			#region CASE 2

			var shipment2 = CollectionHelper.CreateShipment("AUSYD", "DEBER", "SEA", "LCL", "STD", Organisation.PK, consignor.PK, new ZDateTime(2017, 08, 01, 11, 30, 0), false);
			shipment2[JobShipmentSchema.JS_ActualWeight.Name] = 1000m;
			shipment2[JobShipmentSchema.JS_INCO.Name] = "FOB";

			var jobHeader2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader2.JH_ParentID = shipment2.PK;
			jobHeader2.JH_ParentTableCode = "JS";
			jobHeader2.JH_GC = Env.CurrentCompany.PK;
			jobHeader2.JH_GB = Env.CurrentBranch.PK;
			jobHeader2.JH_GE = Env.CurrentDepartment.PK;
			jobHeader2.JH_JobNum = "Job2";

			var jobCharge2_1 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge2_1.JR_JH = jobHeader2.PK;
			jobCharge2_1.JR_LocalSellAmt = 1000m;
			jobCharge2_1.JR_OSSellAmt = 1000m;
			jobCharge2_1.JR_AC = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "ODOC")).First(x => x.AC_GC == Env.CurrentCompany.PK).PK;

			var jobCharge2_2 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge2_2.JR_JH = jobHeader2.PK;
			jobCharge2_2.JR_LocalSellAmt = 200m;
			jobCharge2_2.JR_OSSellAmt = 200m;
			jobCharge2_2.JR_AC = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "DDOC")).First(x => x.AC_GC == Env.CurrentCompany.PK).PK;

			var deber = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "DEBER");

			var sale2 = Factory.New<OrgSales>();
			sale2.OW_IsTraded = false;
			sale2.OW_OH_Primary = Organisation.PK;
			sale2.OW_OH_Buyer = Organisation.PK;
			sale2.OW_OriginID = ausyd.PK;
			sale2.OW_OriginTableCode = "RL";
			sale2.OW_DestinationID = deber.PK;
			sale2.OW_DestinationTableCode = "RL";
			sale2.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			TestCollection.Add(sale2);

			var tradeDetail2a = sale2.TradeDetails.AddNew();
			tradeDetail2a.PA_TradeMode = Constants.TransportModes.Sea;
			tradeDetail2a.PA_TradeType = Constants.ContainerModes.LCL;
			tradeDetail2a.CurrentProspectPeriod.PAS_RepeatsMnth = 1;
			tradeDetail2a.CurrentProspectPeriod.PAS_Units = 200;

			var tradeDetail2b = sale2.TradeDetails.AddNew();
			tradeDetail2b.PA_TradeMode = Constants.TransportModes.Air;
			tradeDetail2b.PA_TradeType = Constants.ContainerModes.ULD;
			tradeDetail2b.CurrentProspectPeriod.PAS_RepeatsMnth = 1;
			tradeDetail2b.CurrentProspectPeriod.PAS_Units = 100;

			#endregion

			#region CASE 3

			var shipment3 = CollectionHelper.CreateShipment("AUSYD", "DEBER", "SEA", "LCL", "STD", Organisation.PK, consignor.PK, new ZDateTime(2017, 08, 01, 11, 30, 0), false);
			shipment3[JobShipmentSchema.JS_ActualWeight.Name] = 1000m;
			shipment3[JobShipmentSchema.JS_INCO.Name] = "FOB";

			var jobHeader3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader3.JH_ParentID = shipment3.PK;
			jobHeader3.JH_ParentTableCode = "JS";
			jobHeader3.JH_GC = Env.CurrentCompany.PK;
			jobHeader3.JH_GB = Env.CurrentBranch.PK;
			jobHeader3.JH_GE = Env.CurrentDepartment.PK;
			jobHeader3.JH_JobNum = "Job3";

			var jobCharge3_1 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge3_1.JR_JH = jobHeader3.PK;
			jobCharge3_1.JR_LocalSellAmt = 1000m;
			jobCharge3_1.JR_OSSellAmt = 1000m;
			jobCharge3_1.JR_AC = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "ODOC")).First(x => x.AC_GC == Env.CurrentCompany.PK).PK;

			var jobCharge3_2 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge3_2.JR_JH = jobHeader3.PK;
			jobCharge3_2.JR_LocalSellAmt = 200m;
			jobCharge3_2.JR_OSSellAmt = 200m;
			jobCharge3_2.JR_AC = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "DDOC")).First(x => x.AC_GC == Env.CurrentCompany.PK).PK;

			deber = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "DEBER");

			var sale3 = Factory.New<OrgSales>();
			sale3.OW_IsTraded = false;
			sale3.OW_OH_Primary = Organisation.PK;
			sale3.OW_OH_Buyer = Organisation.PK;
			sale3.OW_OriginID = ausyd.PK;
			sale3.OW_OriginTableCode = "RL";
			sale3.OW_DestinationID = deber.PK;
			sale3.OW_DestinationTableCode = "RL";
			sale3.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			TestCollection.Add(sale3);

			var tradeDetail3a = sale3.TradeDetails.AddNew();
			tradeDetail3a.PA_TradeMode = Constants.TransportModes.Air;
			tradeDetail3a.PA_TradeType = Constants.ContainerModes.Loose;
			tradeDetail3a.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			tradeDetail3a.CurrentProspectPeriod.PAS_RepeatsMnth = 3;
			tradeDetail3a.CurrentProspectPeriod.PAS_Units = 300;

			var tradeDetail3b = sale3.TradeDetails.AddNew();
			tradeDetail3b.PA_TradeMode = Constants.TransportModes.Sea;
			tradeDetail3b.PA_TradeType = Constants.ContainerModes.LCL;
			tradeDetail3a.CurrentProspectPeriod.PAS_RX_NKCurrency = "EUR";
			tradeDetail3b.CurrentProspectPeriod.PAS_RepeatsMnth = 3;
			tradeDetail3b.CurrentProspectPeriod.PAS_Units = 300;

			var tradeDetail3c = sale3.TradeDetails.AddNew();
			tradeDetail3c.PA_TradeMode = Constants.TransportModes.Sea;
			tradeDetail3c.PA_TradeType = Constants.ContainerModes.BreakBulk;
			tradeDetail3c.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			tradeDetail3c.CurrentProspectPeriod.PAS_RepeatsMnth = 3;
			tradeDetail3c.CurrentProspectPeriod.PAS_Units = 300;

			Factory.Save();

			#endregion

			var testQuote = Factory.New<Quote>();
			foreach (var detail in new OrgTradeDetail[] { tradeDetail1a, tradeDetail1b, tradeDetail1c, tradeDetail2a, tradeDetail2b, tradeDetail3a, tradeDetail3b, tradeDetail3c })
			{
				testQuote.ImportTradeDetailData(detail);
			}

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection, true, true))
			{
				var synchroniser1 = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser1.Execute(summaryProvider, new ZDate(2017, 08, 01), new ZDate(2017, 08, 01));

				var synchroniser2 = new TradeLinesSynchroniser(consignor.PK);
				synchroniser2.Execute(summaryProvider, new ZDate(2017, 08, 01), new ZDate(2017, 08, 01));
			}

			Factory.Save();

			TestCollection.Load();

			AssertEquals(3, TestCollection.Count);

			var salesList = TestCollection.Cast<OrgSales>().Where(x => x.OW_OriginID == ausyd.PK && x.OW_DestinationID == uslax.PK).OrderBy(x => x.IsActual).ToList();
			AssertEquals(1, salesList.Count);

			var tradeDetail = CollectionHelper.FindOrgTradeDetailByMode(salesList[0], Constants.TransportModes.Sea);
			AssertEquals(1m, tradeDetail1a.CurrentProspectPeriod.PAS_RepeatsMnth);
			AssertEquals(200L, tradeDetail1a.CurrentProspectPeriod.PAS_Units);
			AssertEquals(true, tradeDetail1a.CurrentProspectPeriod.IsInDatabase);
			AssertEquals(true, tradeDetail1a.ProspectDetail.IsInDatabase);

			tradeDetail = CollectionHelper.FindOrgTradeDetailByMode(salesList[0], Constants.TransportModes.Air);
			AssertEquals(4m, tradeDetail1b.CurrentProspectPeriod.PAS_RepeatsMnth);
			AssertEquals(500L, tradeDetail1b.CurrentProspectPeriod.PAS_Units);
			AssertEquals(true, tradeDetail1b.CurrentProspectPeriod.IsInDatabase);
			AssertEquals(true, tradeDetail1b.ProspectDetail.IsInDatabase);

			salesList = TestCollection.Cast<OrgSales>().Where(x => x.OW_OriginID == ausyd.PK && x.OW_DestinationID == deber.PK).OrderBy(x => x.IsActual).ToList();
			AssertEquals(2, salesList.Count);

			var orgSale = CollectionHelper.FindOrgSales(SystemDefinedSalesProductList.Codes.ForwardingShipment, "AUSYD", "USLAX", false);
			AssertEquals(1, orgSale.Count());

			var tradeDetails = orgSale.Select(p => p.TradeDetails);
			AssertEquals(1, tradeDetails.Count());

			orgSale = CollectionHelper.FindOrgSales(SystemDefinedSalesProductList.Codes.ForwardingShipment, "AUSYD", "DEBER", false);
			AssertEquals(2, orgSale.Count());

			tradeDetails = orgSale.Select(p => p.TradeDetails);
			AssertEquals(2, tradeDetails.Count());
		}

		[TestDate(2015, 1, 1)]
		public void TestDeleteObsoleteActualTradePeriods()
		{
			Organisation.OH_Code = "TSTORGDDD";
			Organisation.OH_RL_NKClosestPort = "AUBNE";

			var consignor = CollectionHelper.GetNewOrg("TSTORGEEE");
			consignor.OH_RL_NKClosestPort = "AUSYD";

			var airShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", "STD", Organisation.PK, consignor.PK, new ZDateTime(2014, 4, 22, 11, 30, 0), false);
			airShipment[JobShipmentSchema.JS_ActualWeight.Name] = 1m;
			airShipment[JobShipmentSchema.JS_INCO.Name] = "FOB";
			var seaShipment = CollectionHelper.CreateShipment("USLAX", "AUSYD", "SEA", "LSE", "STD", Organisation.PK, consignor.PK, new ZDateTime(2014, 4, 22, 11, 30, 0), false);
			seaShipment[JobShipmentSchema.JS_ActualWeight.Name] = 1m;
			seaShipment[JobShipmentSchema.JS_INCO.Name] = "FOB";

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summary = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summary, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}
			TestCollection.Load();

			var airTradeLane = CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", true).Single();
			var seaTradeLane = CollectionHelper.FindOrgSales("SHP", "USLAX", "AUSYD", true).Single();
			AssertEquals(2, TestCollection.Count);

			airShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUMEL";
			seaShipment[JobShipmentSchema.JS_RL_NKOrigin] = "USLXZ";
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(Organisation.PK);
			var collectionInOtherFactory = new OrgSalesCollection(orgInOtherFactory);
			//jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summary = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var tester = new TradeLinesSynchroniserTester(orgInOtherFactory.PK);
				var saveCount = 0;
				tester.OnSaved += (s, e) =>
				{
					if (saveCount == 1)
					{
						var expectedDbHits = new Dictionary<string, int>
						{
							{ OrgTradePeriodSchema.Constants.TableName, 1 },
							{ OrgTradeValueSchema.Constants.TableName, 1 },
							{ StmUniversalCopySchema.Constants.TableName, 1 },
							{ StmDocDataOverrideSchema.Constants.TableName, 2 }
						};
						AssertDbHits(expectedDbHits, tester.Factory);
					}

					saveCount++;
				};
				tester.Execute(summary, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}

			collectionInOtherFactory.Load();

			var oldAirTradeLane = anotherFactory.Load<OrgSales>(airTradeLane.PK);
			AssertEquals("Old trade lane should be deleted and new trade lane is added", 0, oldAirTradeLane.TradeDetails.Cast<OrgTradeDetail>().SelectMany(x => x.TradedPeriods).Count());
			var oldSeaTradeLane = anotherFactory.Load<OrgSales>(seaTradeLane.PK);
			AssertEquals("Old trade lane should be deleted and new trade lane is added", 0, oldSeaTradeLane.TradeDetails.Cast<OrgTradeDetail>().SelectMany(x => x.TradedPeriods).Count());

			var collectionInOtherFactoryHelper = new OrgSalesCollectionTestHelper(collectionInOtherFactory, anotherFactory);
			var newAirTradeLane = collectionInOtherFactoryHelper.FindOrgSales("SHP", "AUMEL", "USLAX", true).Single();
			AssertNotEquals("Old trade lane should be deleted and new trade lane is added", 0, newAirTradeLane.TradeDetails.Cast<OrgTradeDetail>().SelectMany(x => x.TradedPeriods).Count());
			var newSeaTradeLane = collectionInOtherFactoryHelper.FindOrgSales("SHP", "USLXZ", "AUSYD", true).Single();
			AssertNotEquals("Old trade lane should be deleted and new trade lane is added", newSeaTradeLane.TradeDetails.Cast<OrgTradeDetail>().SelectMany(x => x.TradedPeriods).Count());
		}

		[TestDate(2015, 1, 1)]
		public void TestDoNotUpdateActualsIfSummaryOnlyForProspect()
		{
			Organisation.OH_Code = "TSTORGDDD";
			Organisation.OH_RL_NKClosestPort = "AUBNE";

			var consignor = CollectionHelper.GetNewOrg("TSTORGEEE");
			consignor.OH_RL_NKClosestPort = "AUSYD";

			var airShipment = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", "STD", Organisation.PK, consignor.PK, new ZDateTime(2014, 4, 22, 11, 30, 0), false);
			airShipment[JobShipmentSchema.JS_ActualWeight.Name] = 1m;
			airShipment[JobShipmentSchema.JS_INCO.Name] = "FOB";
			var seaShipment = CollectionHelper.CreateShipment("USLAX", "AUSYD", "SEA", "LSE", "STD", Organisation.PK, consignor.PK, new ZDateTime(2014, 4, 22, 11, 30, 0), false);
			seaShipment[JobShipmentSchema.JS_ActualWeight.Name] = 1m;
			seaShipment[JobShipmentSchema.JS_INCO.Name] = "FOB";

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summary = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summary, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}
			TestCollection.Load();

			var airTradeLane = CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", true).Single();
			var seaTradeLane = CollectionHelper.FindOrgSales("SHP", "USLAX", "AUSYD", true).Single();
			AssertEquals(2, TestCollection.Count);

			airShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUMEL";
			seaShipment[JobShipmentSchema.JS_RL_NKOrigin] = "USLXZ";
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(Organisation.PK);
			var collectionInOtherFactory = new OrgSalesCollection(orgInOtherFactory);
			collectionInOtherFactory.Load();

			using (var summary = new NonCachedTradeLinesSummaryProvider(Db.Connection, false, true))
			{
				var synchroniser = new TradeLinesSynchroniser(orgInOtherFactory.PK);
				synchroniser.Execute(summary, new ZDate(2014, 2, 1), new ZDate(2015, 2, 1));
			}
			collectionInOtherFactory.Load();

			AssertEquals("Old trade lane should not be deleted", 2, collectionInOtherFactory.Count);
			AssertNotNull("Old trade lane should not be deleted", anotherFactory.Load<OrgSales>(airTradeLane.PK));
			AssertNotNull("Old trade lane should not be deleted", anotherFactory.Load<OrgSales>(seaTradeLane.PK));

			var collectionInOtherFactoryHelper = new OrgSalesCollectionTestHelper(collectionInOtherFactory, anotherFactory);
			AssertEquals("New trade lanes not added", false, collectionInOtherFactoryHelper.FindOrgSales("SHP", "AUMEL", "USLAX", true).Any());
			AssertEquals("New trade lanes not added", false, collectionInOtherFactoryHelper.FindOrgSales("SHP", "USLXZ", "AUSYD", true).Any());
		}

		[TestDate(2017, 9, 1)]
		public void TestExistingTradeLaneMatching()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			Organisation.OH_Code = "TESTORGXXX";
			Organisation.OH_RL_NKClosestPort = "AUBNE";

			var org1 = CollectionHelper.GetNewOrg("TESTORG1");
			var org2 = CollectionHelper.GetNewOrg("TESTORG2");

			var shipment1 = CollectionHelper.CreateShipment("AUSYD", "NZAKL", "AIR", "LSE", org1.PK, org2.PK, ZDateTime.Empty);

			var job1 = new JobHeader.Loader((IJobHeaderParent)shipment1).TryCreate();
			job1.JH_GC = Env.CurrentCompanyPK;
			job1.JH_OA_LocalChargesAddr = Organisation.MainAddress.PK;

			var shipment2 = CollectionHelper.CreateShipment("AUSYD", "USLAX", "AIR", "LSE", org1.PK, org2.PK, ZDateTime.Empty);

			var job2 = new JobHeader.Loader((IJobHeaderParent)shipment2).TryCreate();
			job2.JH_GC = Env.CurrentCompanyPK;
			job2.JH_OA_LocalChargesAddr = Organisation.MainAddress.PK;

			var audHeaderAR = Factory.NewWithValidTestData<AccTransactionHeader>();
			audHeaderAR.AH_Ledger = LedgerTypes.AccountsReceivable;
			audHeaderAR.AH_TransactionType = TransactionTypes.Invoice;
			audHeaderAR.AH_JH = job2.PK;
			audHeaderAR.AH_GC = Env.CurrentCompanyPK;
			audHeaderAR.AH_OH = Organisation.PK;

			var nonDsbChargeQuery = new ZQuery();
			nonDsbChargeQuery.AddToFilter(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.NotEqual, "DSB");
			nonDsbChargeQuery.AddToFilter(AccChargeCodeSchema.AC_AG_RevenueAccount, SQLComparisonOperator.NotEqual, null);
			nonDsbChargeQuery.OrderBy = AccChargeCodeSchema.Constants.AC_Code;
			var nonDsbCharge = Factory.LoadTop1<AccChargeCode>(nonDsbChargeQuery);

			var audLine1 = Factory.NewWithValidTestData<AccTransactionLines>();
			audLine1.AL_AH = audHeaderAR.PK;
			audLine1.AL_JH = job2.PK;
			audLine1.AL_AC = nonDsbCharge.PK;
			audLine1.AL_AG = nonDsbCharge.AC_AG_RevenueAccount;
			audLine1.AL_RX_NKTransactionCurrency = "AUD";
			audLine1.AL_LineType = "REV";
			audLine1.AL_LineAmount = audLine1.AL_OSAmount = 100;
			audLine1.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			var audJobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			audJobCharge1.JR_JH = job2.PK;
			audJobCharge1.JR_AL_ARLine = audLine1.PK;
			audJobCharge1.JR_LocalSellAmt = 100;
			audJobCharge1.JR_OSSellAmt = 100;

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summary = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser1 = new TradeLinesSynchroniser(org1.PK);
				synchroniser1.Execute(summary, new ZDate(2017, 8, 1), new ZDate(2017, 10, 1));

				var synchroniser = new TradeLinesSynchroniser(org2.PK);
				synchroniser.Execute(summary, new ZDate(2017, 8, 1), new ZDate(2017, 10, 1));
			}
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var localOrg = anotherFactory.Load<OrgHeader>(Organisation.PK);

			using (var summary = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(localOrg.PK);
				synchroniser.Execute(summary, new ZDate(2017, 8, 1), new ZDate(2017, 10, 1));
			}
			anotherFactory.Save();

			org1.SalesCollection.Load();
			var helper1 = new OrgSalesCollectionTestHelper(org1.SalesCollection, Factory);
			var salesList1a = helper1.FindOrgSales("SHP", "AUSYD", "USLAX", org1.PK.ToGuid(), org2.PK.ToGuid(), isActual: true).ToList();
			AssertEquals("Trade lane should not duplicate", 1, salesList1a.Count);
			var salesList1b = helper1.FindOrgSales("SHP", "AUSYD", "NZAKL", org1.PK.ToGuid(), org2.PK.ToGuid(), isActual: true).ToList();
			AssertEquals("Trade lane should not duplicate", 1, salesList1b.Count);

			org2.SalesCollection.Load();
			var helper2 = new OrgSalesCollectionTestHelper(org2.SalesCollection, Factory);
			var salesList2a = helper1.FindOrgSales("SHP", "AUSYD", "USLAX", org1.PK.ToGuid(), org2.PK.ToGuid(), isActual: true).ToList();
			AssertEquals("Trade lane should not duplicate", 1, salesList2a.Count);
			var salesList2b = helper1.FindOrgSales("SHP", "AUSYD", "NZAKL", org1.PK.ToGuid(), org2.PK.ToGuid(), isActual: true).ToList();
			AssertEquals("Trade lane should not duplicate", 1, salesList2b.Count);

			TestCollection.Load();
			var salesList3a = CollectionHelper.FindOrgSales("SHP", "AUSYD", "USLAX", org1.PK.ToGuid(), org2.PK.ToGuid(), isActual: true).ToList();
			AssertEquals("Trade lane should not duplicate", 1, salesList3a.Count);
			var salesList3b = CollectionHelper.FindOrgSales("SHP", "AUSYD", "NZAKL", org1.PK.ToGuid(), org2.PK.ToGuid(), isActual: true).ToList();
			AssertEquals("Trade lane should not duplicate", 1, salesList3b.Count);
		}

		[TestDate(2017, 9, 1)]
		public void TestExistingTradeLaneMatching_WithBlankFields()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var existingSales = Factory.New<OrgSales>();
			existingSales.OW_IsTraded = true;
			existingSales.OW_MP_Product = shipmentProduct.Identifier;
			existingSales.OW_OriginID = ausyd.PK;
			existingSales.OW_OriginTableCode = "RL";

			var existingDetail = Factory.New<OrgTradeDetail>();
			existingDetail.PA_OW = existingSales.PK;
			existingDetail.PA_TradeMode = "AIR";
			existingDetail.PA_TradeType = "LSE";

			Factory.Save();

			Organisation.OH_Code = "TESTORGXXX";
			Organisation.OH_RL_NKClosestPort = "AUBNE";

			var shipment = CollectionHelper.CreateShipment("AUSYD", "", "AIR", "LSE", ZGuid.Empty, ZGuid.Empty, ZDateTime.Empty);

			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();
			job.JH_GC = Env.CurrentCompanyPK;
			job.JH_OA_LocalChargesAddr = Organisation.MainAddress.PK;

			var audHeaderAR = Factory.NewWithValidTestData<AccTransactionHeader>();
			audHeaderAR.AH_Ledger = LedgerTypes.AccountsReceivable;
			audHeaderAR.AH_TransactionType = TransactionTypes.Invoice;
			audHeaderAR.AH_JH = job.PK;
			audHeaderAR.AH_GC = Env.CurrentCompanyPK;
			audHeaderAR.AH_OH = Organisation.PK;

			var nonDsbChargeQuery = new ZQuery();
			nonDsbChargeQuery.AddToFilter(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.NotEqual, "DSB");
			nonDsbChargeQuery.AddToFilter(AccChargeCodeSchema.AC_AG_RevenueAccount, SQLComparisonOperator.NotEqual, null);
			nonDsbChargeQuery.OrderBy = AccChargeCodeSchema.Constants.AC_Code;
			var nonDsbCharge = Factory.LoadTop1<AccChargeCode>(nonDsbChargeQuery);

			var audLine1 = Factory.NewWithValidTestData<AccTransactionLines>();
			audLine1.AL_AH = audHeaderAR.PK;
			audLine1.AL_JH = job.PK;
			audLine1.AL_AC = nonDsbCharge.PK;
			audLine1.AL_AG = nonDsbCharge.AC_AG_RevenueAccount;
			audLine1.AL_RX_NKTransactionCurrency = "AUD";
			audLine1.AL_LineType = "REV";
			audLine1.AL_LineAmount = audLine1.AL_OSAmount = 100;
			audLine1.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			var audJobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			audJobCharge1.JR_JH = job.PK;
			audJobCharge1.JR_AL_ARLine = audLine1.PK;
			audJobCharge1.JR_LocalSellAmt = 100;
			audJobCharge1.JR_OSSellAmt = 100;

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summary = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summary, new ZDate(2017, 8, 1), new ZDate(2017, 10, 1));
			}
			Factory.Save();

			var salesQuery = new ZQuery(OrgSalesSchema.OW_IsTraded, ZBool.True);
			salesQuery.AddToFilter(OrgSalesSchema.OW_MP_Product, shipmentProduct.Identifier);
			salesQuery.AddToFilter(OrgSalesSchema.OW_OriginID, ausyd.PK);
			var salesList = Factory.Load<OrgSales>(salesQuery);
			AssertEquals("Trade lane should not duplicate", 1, salesList.Length);

			var details = salesList[0].TradeDetails;
			details.Load();
			AssertEquals("Trade detail should not duplicate", 1, details.Count);
		}

		[ExpectNoExceptions]
		public void TestSyncWithDeletedOrgHeader()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var syncRange = new TradeLinesSynchronizationRange(new ZDate(2002, 1, 1), new ZDate(2002, 5, 1));
			var summary = new TradeLinesSummary(syncRange);

			var tradeValue = new TradeLaneValue();
			var period = new ZDate(2002, 2, 2);
			var alotOfDecimals = 1.123456789m;
			var stubInt = 1;
			var shipmentProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var tradeLine = new TradeLineForTest(
				"SEA", "FCL", ZGuid.Empty, 3,
				period,
				ZGuid.Empty, org.PK, org.PK,
				period, stubInt, stubInt, stubInt, alotOfDecimals, alotOfDecimals, alotOfDecimals, alotOfDecimals, "KG", alotOfDecimals,
				"AUD", Env.CurrentCompany.PK,
				alotOfDecimals, alotOfDecimals, alotOfDecimals, alotOfDecimals,
				ZGuid.Empty, shipmentProduct
				);

			var tradeKey = new TradeLaneKey(TradeLaneKey.TradeType.Actual, tradeLine);
			tradeValue.AddData(org.PK, tradeLine);
			summary.ActualValues.Add(tradeKey, tradeValue);

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			var synchroniser = new TradeLinesSynchroniserTester(org.PK);
			synchroniser.OnSaving += (s, e) =>
			{
				org.Delete();
				Factory.Save();
			};

			synchroniser.Execute(summary);
		}

		public void TestUpdateClientCommencedDateAndDoesNotCreateEDTLogWhenRegistryIsSet()
		{
			#region Set Up

			var auSyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD").PK;
			var auBne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE").PK;
			var usLax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX").PK;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			var org5 = Factory.NewWithValidTestData<OrgHeader>();
			var org6 = Factory.NewWithValidTestData<OrgHeader>();
			var miscServ2 = org2.MiscServ;
			var miscServ4 = org4.MiscServ;
			var miscServ5 = org5.MiscServ;

			org1.MiscServ.OM_CMClientCommenced = new ZDateTime(2002, 2, 8);
			org3.MiscServ.OM_CMClientCommenced = new ZDateTime(2002, 2, 8);
			org6.MiscServ.OM_CMClientCommenced = new ZDateTime(2002, 2, 8);

			var summary1 = SetUpTradeLinesSummary(org1, new ZDate(2002, 2, 2), auSyd, usLax);
			var summary2 = SetUpTradeLinesSummary(org2, new ZDate(2002, 2, 3), auBne, usLax);
			var summary3 = SetUpTradeLinesSummary(org3, new ZDate(2002, 2, 9), auSyd, usLax);

			var syncRange = new TradeLinesSynchronizationRange(new ZDate(2002, 1, 1), new ZDate(2002, 5, 1));
			var summaryWithoutTradeLine = new TradeLinesSummary(syncRange);
			var summaryWithoutOriginAndDestination = SetUpTradeLinesSummary(org4, new ZDate(2002, 2, 2), ZGuid.Empty, ZGuid.Empty);
			var summaryWithoutDestination = SetUpTradeLinesSummary(org5, new ZDate(2002, 2, 1), auSyd, ZGuid.Empty);
			var summaryWithoutOrigin = SetUpTradeLinesSummary(org6, new ZDate(2002, 1, 1), ZGuid.Empty, usLax);

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			#endregion

			using (OrganisationRegistry.Instance.UpdateExistingClientCommenceDateFromTradeLaneSync.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				RunSynchroniser(org1, summary1);
				AssertEquals("OM_CMClientCommenced should be updated as there is a trade period earlier than existing CCD.", new ZDateTime(2002, 2, 2), factory.Load<OrgHeader>(org1.PK).MiscServ.OM_CMClientCommenced);
				AssertUpdatingCCDDoesNotCreateEDTLog(org1, "AUSYD -> USLAX");

				Assert("Pre-condition", miscServ2.OM_CMClientCommenced.IsEmpty);
				RunSynchroniser(org2, summary2);
				AssertEquals("OM_CMClientCommenced should be updated as CCD is empty.", new ZDateTime(2002, 2, 3), factory.Load<OrgHeader>(org2.PK).MiscServ.OM_CMClientCommenced);
				AssertUpdatingCCDDoesNotCreateEDTLog(org2, "AUBNE -> USLAX");

				RunSynchroniser(org3, summary3);
				AssertEquals("If the first traded period is later than the existing CCD, the CDD should not be updated", new ZDateTime(2002, 2, 8), factory.Load<OrgHeader>(org3.PK).MiscServ.OM_CMClientCommenced);
				AssertUpdatingCCDDoesNotCreateEDTLog(org3, " ->");

				Assert("Pre-condition", miscServ4.OM_CMClientCommenced.IsEmpty);
				RunSynchroniser(org4, summaryWithoutTradeLine);
				Assert("If there is no CCD or firstTradedPeriod, the CCD should remain empty", factory.Load<OrgHeader>(org4.PK).MiscServ.OM_CMClientCommenced.IsEmpty);
				AssertUpdatingCCDDoesNotCreateEDTLog(org4, " ->");

				RunSynchroniser(org4, summaryWithoutOriginAndDestination);
				AssertEquals("CCD Should be updated as there is a firstTradedPeriod", new ZDate(2002, 2, 2), factory2.Load<OrgHeader>(org4.PK).MiscServ.OM_CMClientCommenced);
				AssertUpdatingCCDDoesNotCreateEDTLog(org4, " ->");

				Assert("Pre-condition", miscServ5.OM_CMClientCommenced.IsEmpty);
				RunSynchroniser(org5, summaryWithoutDestination);
				AssertEquals("OM_CMClientCommenced should be updated as CCD is empty.", new ZDate(2002, 2, 1), factory.Load<OrgHeader>(org5.PK).MiscServ.OM_CMClientCommenced);
				AssertUpdatingCCDDoesNotCreateEDTLog(org5, "AUSYD ->");

				RunSynchroniser(org6, summaryWithoutOrigin);
				AssertEquals("OM_CMClientCommenced should be updated as there is a trade period earlier than existing CCD.", new ZDate(2002, 1, 1), factory.Load<OrgHeader>(org6.PK).MiscServ.OM_CMClientCommenced);
				AssertUpdatingCCDDoesNotCreateEDTLog(org6, " -> USLAX");
			}
		}

		public void TestUpdateClientCommencedDateAndDoesNotCreateEDTLogWhenRegistryIsNotSet()
		{
			#region Set Up
			var auSyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD").PK;
			var auBne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE").PK;
			var usLax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX").PK;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			var org5 = Factory.NewWithValidTestData<OrgHeader>();

			org2.MiscServ.OM_CMClientCommenced = new ZDateTime(2002, 2, 8);
			var miscServ1 = org1.MiscServ;
			var miscServ3 = org3.MiscServ;
			var miscServ4 = org4.MiscServ;
			var miscServ5 = org5.MiscServ;

			var summary1 = SetUpTradeLinesSummary(org1, new ZDate(2002, 2, 2), auSyd, usLax);
			var summary2 = SetUpTradeLinesSummary(org2, new ZDate(2002, 2, 3), auBne, usLax);
			var syncRange = new TradeLinesSynchronizationRange(new ZDate(2002, 1, 1), new ZDate(2002, 5, 1));
			var summaryWithoutTradeLine = new TradeLinesSummary(syncRange);
			var summaryWithoutOriginAndDestination = SetUpTradeLinesSummary(org3, new ZDate(2002, 2, 2), ZGuid.Empty, ZGuid.Empty);
			var summaryWithoutDestination = SetUpTradeLinesSummary(org4, new ZDate(2002, 2, 1), auSyd, ZGuid.Empty);
			var summaryWithoutOrigin = SetUpTradeLinesSummary(org5, new ZDate(2002, 1, 1), ZGuid.Empty, usLax);

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			#endregion

			using (OrganisationRegistry.Instance.UpdateExistingClientCommenceDateFromTradeLaneSync.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert("Pre-condition", miscServ1.OM_CMClientCommenced.IsEmpty);
				RunSynchroniser(org1, summary1);
				AssertEquals("OM_CMClientCommenced should be updated as there is a trade period and CCD is empty.", new ZDateTime(2002, 2, 2), factory.Load<OrgHeader>(org1.PK).MiscServ.OM_CMClientCommenced);
				AssertUpdatingCCDDoesNotCreateEDTLog(org1, "AUSYD -> USLAX");

				RunSynchroniser(org2, summary2);
				AssertEquals("OM_CMClientCommenced should not be updated as CCD exist even if there is an earlier trade period.", new ZDateTime(2002, 2, 8), factory.Load<OrgHeader>(org2.PK).MiscServ.OM_CMClientCommenced);
				AssertUpdatingCCDDoesNotCreateEDTLog(org2, " ->");

				Assert("Pre-condition", miscServ3.OM_CMClientCommenced.IsEmpty);
				RunSynchroniser(org3, summaryWithoutTradeLine);
				Assert("CCD is not updated as a trade period does not exist.", factory.Load<OrgHeader>(org3.PK).MiscServ.OM_CMClientCommenced.IsEmpty);
				AssertUpdatingCCDDoesNotCreateEDTLog(org3, " ->");

				RunSynchroniser(org3, summaryWithoutOriginAndDestination);
				AssertEquals("CCD Should be updated as there is a firstTradedPeriod", new ZDate(2002, 2, 2), factory2.Load<OrgHeader>(org3.PK).MiscServ.OM_CMClientCommenced);
				AssertUpdatingCCDDoesNotCreateEDTLog(org3, " ->");

				Assert("Pre-condition", miscServ4.OM_CMClientCommenced.IsEmpty);
				RunSynchroniser(org4, summaryWithoutDestination);
				AssertEquals("CCD Should be updated as there is a firstTradedPeriod", new ZDate(2002, 2, 1), factory.Load<OrgHeader>(org4.PK).MiscServ.OM_CMClientCommenced);
				AssertUpdatingCCDDoesNotCreateEDTLog(org4, "AUSYD ->");

				Assert("Pre-condition", miscServ5.OM_CMClientCommenced.IsEmpty);
				RunSynchroniser(org5, summaryWithoutOrigin);
				AssertEquals("CCD Should be updated as there is a firstTradedPeriod", new ZDate(2002, 1, 1), factory.Load<OrgHeader>(org5.PK).MiscServ.OM_CMClientCommenced);
				AssertUpdatingCCDDoesNotCreateEDTLog(org5, " -> USLAX");
			}
		}

		void AssertUpdatingCCDDoesNotCreateEDTLog(OrgHeader org, string tradeLane)
		{
			var reference = String.Format(Culture.Invariant, $"Client Commenced Date updated by TLS based on Trade Lane {tradeLane}");

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, org.MiscServ.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.EditedARecordCode);
			query.AddToFilter(StmALogSchema.SL_Reference, reference);
			query.OrderBy = StmALogSchema.Constants.SL_EventTime + OrderByClause.Descending;
			var logs = Factory.Load<StmALog>(query);

			AssertEquals(0, logs.Length);
		}

		TradeLinesSummary SetUpTradeLinesSummary(OrgHeader org, ZDate period, ZGuid originPK, ZGuid destinationPK)
		{
			var syncRange = new TradeLinesSynchronizationRange(new ZDate(2002, 1, 1), new ZDate(2002, 5, 1));
			var summary = new TradeLinesSummary(syncRange);
			var sales = Factory.NewWithValidTestData<OrgSales>();
			sales.OW_OriginID = originPK;
			sales.OW_DestinationID = destinationPK;
			var tradeKey = new TradeLaneKey(sales);
			var tradeValue = new TradeLaneValue();
			var stubDecimal = 1.12m;
			var stubInt = 1;
			var weightVolume = -1m;
			var weight = -2m;
			var volume = -3m;
			var chargeable = -4m;

			var tradeLine = new TradeLineForTest(
				"AIR", "", ZGuid.Empty, 3,
				period,
				org.PK, org.PK, org.PK,
				period, stubInt, stubInt, stubInt, weightVolume, weight, volume, chargeable, "KG", stubDecimal,
				"AUD", Env.CurrentCompany.PK,
				stubDecimal, stubDecimal, stubDecimal, stubDecimal,
				ZGuid.Empty,
				null,
				originPK,
				destinationPK
			);

			tradeValue.AddData(
				org.PK,
				tradeLine
			);

			summary.ActualValues.Add(new KeyValuePair<TradeLaneKey, TradeLaneValue>(tradeKey, tradeValue));

			return summary;
		}

		void RunSynchroniser(OrgHeader org, TradeLinesSummary summary)
		{
			var synchroniser = new TradeLinesSynchroniserTester(org.PK);
			synchroniser.Execute(summary);
		}

		#region TLS Log

		public void TestTLSLogIsAddedOnceAndUpdated()
		{
			Organisation.OH_Code = "TESTORGXXX";
			Organisation.OH_RL_NKClosestPort = "AUBNE";
			Factory.Save();
			var logs = Organisation.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.SalesTradeLanesSynchronisedCode));
			AssertEquals(0, logs.Length);

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2014, 1, 1), new ZDate(2015, 1, 1));
			}
			logs = Organisation.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.SalesTradeLanesSynchronisedCode));
			var log = LoadLog();
			AssertNotNull(log);

			ZDateTime dt1 = log.SL_EventTime;
			Assert(dt1.IsValid);
			ZGuid logPK = log.PK;

			System.Threading.Thread.Sleep(100);
			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var synchroniser = new TradeLinesSynchroniser(Organisation.PK);
				synchroniser.Execute(summaryProvider, new ZDate(2014, 1, 1), new ZDate(2015, 1, 1));
			}
			log = LoadLog();
			AssertNotNull(log);

			ZDateTime dt2 = log.SL_EventTime;
			Assert(dt2 > dt1);
			AssertEquals(logPK, log.PK);
		}

		StmALog LoadLog()
		{
			var filter = new ZQuery(StmALogSchema.SL_Parent, Organisation.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.SalesTradeLanesSynchronisedCode);
			filter.OrderBy = StmALogSchema.Constants.SL_EventTime + OrderByClause.Descending;
			filter.ReLoadExistingRows = true;
			var log = Factory.LoadTop1<StmALog>(filter);
			return log;
		}

		#endregion

		#region Implementation

		OrgHeader Organisation => organisation ?? (organisation = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader organisation;

		OrgSalesCollection TestCollection => testCollection ?? (testCollection = new OrgSalesCollection(Organisation));
		OrgSalesCollection testCollection;

		OrgSalesCollectionTestHelper CollectionHelper => collectionHelper ?? (collectionHelper = new OrgSalesCollectionTestHelper(TestCollection, Factory));
		OrgSalesCollectionTestHelper collectionHelper;

		class TradeLinesSynchroniserTester : TradeLinesSynchroniser
		{
			internal event EventHandler OnSaving;
			internal event EventHandler OnSaved;

			public TradeLinesSynchroniserTester(ZGuid orgHeaderPk) : base(orgHeaderPk)
			{
			}

			public TradeLinesSynchroniserTester(ZGuid orgHeaderPk, int partitionSize) : base(orgHeaderPk)
			{
				partitionSizeOverride = partitionSize;
			}
			readonly int partitionSizeOverride;

			public override void Save(BusinessObjectFactoryProvider provider)
			{
				OnSaving?.Invoke(this, EventArgs.Empty);
				Factory = provider.Current;
				base.Save(provider);
				OnSaved?.Invoke(this, EventArgs.Empty);
			}

			public BusinessObjectFactory Factory { get; private set; }

			protected override int PartitionSize => partitionSizeOverride > 0 ? partitionSizeOverride : base.PartitionSize;
		}

		#endregion
	}
}
