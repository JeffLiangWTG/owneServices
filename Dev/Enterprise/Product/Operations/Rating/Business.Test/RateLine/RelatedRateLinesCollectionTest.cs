using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class RelatedRateLinesCollectionTest : RatingTestCase
	{
		#region Loading

		public void TestRetrieveLocationGenericRates()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			rate.AddRateEntry("LCL", "LCL", "AUSYD", "");
			Factory.Save();

			var quote = Helper.NewQuote(rate.Header);
			var quoteEntry = quote.AddRateEntry("LCL", "LCL", "AUSYD", "ZA");

			AssertEquals(1, quoteEntry.RelatedRateLines.Count);
		}

		public void TestRetrieveRelevantClientRates()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = testRate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			Factory.Save();

			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			var quoteEntry = testQuote.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");

			AssertEquals("No related lines displayed as org is different", 0, quoteEntry.RelatedRateLines.Count);

			testQuote.TH_OH = testRate.Header.PK;

			AssertEquals("1 related line displayed", 1, quoteEntry.RelatedRateLines.Count);
			AssertEquals("Client Rate displayed", "Client Rate", quoteEntry.RelatedRateLines[0].RateType);
			AssertEquals("No supplier present on a client rate", ZGuid.Empty, quoteEntry.RelatedRateLines[0].Supplier);

			Factory.Save();

			AssertEquals("No related lines displayed on the client rate (client rate itself is not displayed)", 0, entry.RelatedRateLines.Count);
		}

		public void TestRetrieveRelevantGlobalClientRates()
		{
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out _, out _, true, false, "FRT", "FRT");

			var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
			var rateEntry = globalClientRate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			Factory.Save();

			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_OH = globalClientRate.Header.PK;
			var quoteEntry = quote.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");

			AssertEquals("1 related line displayed", 1, quoteEntry.RelatedRateLines.Count);
			AssertEquals("Client Rate displayed", "Global Client Rate", quoteEntry.RelatedRateLines[0].RateType);
			AssertEquals("No supplier present on a client rate", ZGuid.Empty, quoteEntry.RelatedRateLines[0].Supplier);

			Factory.Save();

			AssertEquals("No related lines displayed on the client rate (client rate itself is not displayed)", 0, rateEntry.RelatedRateLines.Count);
		}

		public void TestContainerClass()
		{
			var gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			gP20.RC_FreightRateClass = "20RN";

			var hC20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20HC");
			hC20.RC_FreightRateClass = "20RN";

			var gP40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			Factory.Save();

			var cost = Helper.NewCosting(Helper.NewOrgHeader());
			var costEntry = cost.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			costEntry.TI_RC = gP20.PK;
			costEntry.TI_MatchContainerRateClass = true;
			Factory.Save();

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");

			AssertEquals("Related lines displayed", 1, entry.RelatedRateLines.Count);

			entry.TI_RC = gP20.PK;
			AssertEquals("Related lines displayed", 1, entry.RelatedRateLines.Count);

			entry.TI_RC = hC20.PK;
			AssertEquals("Related lines displayed", 1, entry.RelatedRateLines.Count);

			entry.TI_RC = gP40.PK;
			AssertEquals("Related lines NOT displayed", 0, entry.RelatedRateLines.Count);

			costEntry.TI_MatchContainerRateClass = false;
			Factory.Save();

			entry.TI_RC = hC20.PK;
			AssertEquals("Related lines NOT displayed", 0, entry.RelatedRateLines.Count);
		}

		public void TestRetrievingRelatedRateLinesNoOriginDestination()
		{
			PostFreightCosting();
			Factory.Save();

			var testRate = Factory.New<ClientRate>();
			var entry = testRate.AddRateEntry("LCL");
			AssertEquals("No costings displayed initially when no origin/destination is set", 0, entry.RelatedRateLines.Count);

			entry.TI_OriginLRC = "AUSYD";
			AssertEquals("Costings displayed", 6, entry.RelatedRateLines.Count);
		}

		public void TestRetrievingCostingFromGenericCosting()
		{
			var genericCost = PostFreightCosting();
			genericCost.TH_OH = ZGuid.Empty;
			Factory.Save();

			var testCost = Factory.New<Costing>();
			testCost.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry = testCost.AddRateEntry("LCL");
			AssertEquals("No costings displayed initially when no origin/destination is set", 0, entry.RelatedRateLines.Count);

			entry.TI_OriginLRC = "AUSYD";
			AssertEquals("Costings displayed", 6, entry.RelatedRateLines.Count);

			AssertEquals("No related costings displayed for generic cost", 0, genericCost.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL)[0].RelatedRateLines.Count);
		}

		public void TestRetrievingRelatedRateLinesForWarehouseRating_ProductWarehouse()
		{
			AssertRetrievingRelatedRateLinesForWarehouseRating(RatingConstants.RateCategory.WHS);
		}

		public void TestRetrievingRelatedRateLinesForWarehouseRating_TransitWarehouse()
		{
			AssertRetrievingRelatedRateLinesForWarehouseRating(RatingConstants.RateCategory.TRW);
		}

		public void TestRetrievingRelatedRateLinesForWarehouseRating_TransitWarehouseTransportationUnit()
		{
			AssertRetrievingRelatedRateLinesForWarehouseRating(RatingConstants.RateCategory.TWU);
		}

		void AssertRetrievingRelatedRateLinesForWarehouseRating(string ratingCategory)
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var tariffEntry1 = tariff1.AddRateEntry(ratingCategory, "ALL", "", "");
			tariffEntry1.AllWarehouses = true;
			tariffEntry1.AddRateLine("ODOC", FlatCalculator.Code);
			var tariffEntry2 = tariff1.AddRateEntry(ratingCategory, "ALL", "", "");
			tariffEntry2.TI_WW_Warehouse = Helper.NewWarehouse().PK;
			tariffEntry2.AddRateLine("ODOC", FlatCalculator.Code);
			var tariffEntry3 = tariff1.AddRateEntry(ratingCategory, "ALL", "", "");
			tariffEntry3.TI_WW_Warehouse = Helper.NewWarehouse().PK;
			tariffEntry3.AddRateLine("ODOC", FlatCalculator.Code);
			Factory.Save();

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var entry = testRate.AddRateEntry(ratingCategory);
			AssertEquals(0, entry.RelatedRateLines.Count);
			entry.AllWarehouses = true;
			AssertEquals(3, entry.RelatedRateLines.Count);
			entry.AllWarehouses = false;
			entry.TI_WW_Warehouse = tariffEntry2.TI_WW_Warehouse;
			AssertEquals(2, entry.RelatedRateLines.Count);
		}

		public void TestRetrievingRelatedRateLinesForContainerStorage()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var tariffEntry1 = tariff1.AddRateEntry("CST", "ALL", "", "");
			tariffEntry1.AddRateLine("ODOC", FlatCalculator.Code);
			var tariffEntry2 = tariff1.AddRateEntry("CST", "SEA", "", "");
			tariffEntry2.AddRateLine("ODOC", FlatCalculator.Code);
			var tariffEntry3 = tariff1.AddRateEntry("CST", "AIR", "", "");
			tariffEntry3.AddRateLine("ODOC", FlatCalculator.Code);
			Factory.Save();

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var entry = testRate.AddRateEntry("CST");
			AssertEquals(1, entry.RelatedRateLines.Count);
			entry.TI_Mode = "SEA";
			AssertEquals(2, entry.RelatedRateLines.Count);
		}

		public void TestRetrievingRelatedRateLinesForContractNumber()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var tariffEntry1 = tariff1.AddRateEntry("ORG", "ALL", "AUSYD", "NZAKL");
			tariffEntry1.TI_ContractNumber = ZString.Empty;
			tariffEntry1.AddRateLine("ODOC", FlatCalculator.Code);
			var tariffEntry2 = tariff1.AddRateEntry("ORG", "ALL", "AUSYD", "NZAKL");
			tariffEntry2.TI_ContractNumber = "123";
			tariffEntry2.AddRateLine("ODOC", FlatCalculator.Code);
			var tariffEntry3 = tariff1.AddRateEntry("ORG", "ALL", "AUSYD", "NZAKL");
			tariffEntry3.TI_ContractNumber = "456";
			tariffEntry3.AddRateLine("ODOC", FlatCalculator.Code);
			Factory.Save();

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var entry = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "NZAKL");
			AssertEquals(3, entry.RelatedRateLines.Count);
			entry.TI_ContractNumber = "123";
			AssertEquals(2, entry.RelatedRateLines.Count);
			entry.TI_ContractNumber = "789";
			AssertEquals(1, entry.RelatedRateLines.Count);
		}

		public void TestRetrievingRelatedRateLinesForIsNonOperatedReefer()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var tariffEntry1 = tariff1.AddRateEntry("ORG", "ALL", "AUSYD", "NZAKL");
			tariffEntry1.TI_IsNonOperatedReefer = ZString.Empty;
			tariffEntry1.AddRateLine("ODOC", FlatCalculator.Code);
			var tariffEntry2 = tariff1.AddRateEntry("ORG", "ALL", "AUSYD", "NZAKL");
			tariffEntry2.TI_IsNonOperatedReefer = "N";
			tariffEntry2.AddRateLine("ODOC", FlatCalculator.Code);
			var tariffEntry3 = tariff1.AddRateEntry("ORG", "ALL", "AUSYD", "NZAKL");
			tariffEntry3.TI_IsNonOperatedReefer = "Y";
			tariffEntry3.AddRateLine("ODOC", FlatCalculator.Code);
			Factory.Save();

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var entry = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "NZAKL");
			AssertEquals(3, entry.RelatedRateLines.Count);
			entry.TI_IsNonOperatedReefer = "N";
			AssertEquals(2, entry.RelatedRateLines.Count);
			entry.TI_IsNonOperatedReefer = "T";
			AssertEquals(1, entry.RelatedRateLines.Count);
		}

		public void TestRetrievingRelatedRateLinesFreight()
		{
			PostFreightCosting();
			Factory.Save();

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var testCollection = RetrieveGlobalAndCostingLines(testRate, RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AUSYD", "USLAX", CC01);
			AssertEquals("Count", 6, testCollection.Count);
		}

		public void TestRetrievingRelatedRateLinesSupplemental()
		{
			PostSupplementalCosting();
			Factory.Save();

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var testCollection = RetrieveGlobalAndCostingLines(testRate, RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AUSYD", ZString.Empty, CC01);
			AssertEquals("Count", 3, testCollection.Count);
		}

		public void TestRetrievingGlobalRateLinesFreight()
		{
			PostFreightGlobal();
			Factory.Save();

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var testCollection = RetrieveGlobalAndCostingLines(testRate, RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "USCA", "AUSYD", CC02);
			AssertEquals("Count", 3, testCollection.Count);
		}

		public void TestRetrievingGlobalRateLinesSupplemental()
		{
			PostSupplementalGlobal(RateType.Forwarding);
			Factory.Save();

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var testCollection = RetrieveGlobalAndCostingLines(testRate, RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AUSYD", ZString.Empty, CC03);
			AssertEquals("Count", 9, testCollection.Count);
		}

		public void TestRetrievingCFSGlobalRateLinesSupplemental()
		{
			PostSupplementalGlobal(RateType.CFS);
			Factory.Save();

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var testCollection = RetrieveGlobalAndCostingLines(testRate, RatingConstants.RateCategory.PAC, Core.Constants.RateMode.SEA, "AUSYD", ZString.Empty, CC03);
			AssertEquals("Count", 9, testCollection.Count);
		}

		public void TestRetrievingGlobalAndRelatedRateLines()
		{
			PostSupplementalCosting();
			PostSupplementalGlobal(RateType.Forwarding);
			Factory.Save();

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var rateLinesTableHitCountBeforeLoading = testRate.ShowCostingCompanyTariffFactory.GetTableHitCount(RateLinesSchema.Constants.TableName);

			var testCollection = RetrieveGlobalAndCostingLines(testRate, RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AUSYD", ZString.Empty, CC03);
			AssertEquals("Count", 9, testCollection.Count);

			var rateLinesTableHitCountAfterLoading = testRate.ShowCostingCompanyTariffFactory.GetTableHitCount(RateLinesSchema.Constants.TableName);
			AssertEquals("RateLines table should not be hit many times", 1, rateLinesTableHitCountAfterLoading - rateLinesTableHitCountBeforeLoading);

			var companyTariffs = new CompanyTariffCollection(Factory);
			companyTariffs.Load();
			var tariff1 = companyTariffs[0];
			testCollection = RetrieveGlobalAndCostingLines(tariff1, RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AUSYD", ZString.Empty, CC03, 1, false);
			AssertEquals("Count", 3, testCollection.Count);
			AssertEquals("RateType", "Costing", testCollection[0].RateType);

			testCollection = RetrieveGlobalAndCostingLines(testRate, RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AUSYD", ZString.Empty, CC03, 1, true);
			AssertEquals("Count", 9, testCollection.Count);
		}

		public void TestRetrievingRelatedRateLinesInDifferentGlbCompany()
		{
			var costsCompany = Factory.NewWithValidTestData<GlbCompany>();
			costsCompany.GC_Code = "NEW";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			costsCompany.Branches.Add(branch1);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				PostSupplementalCosting();
				Factory.Save();
			}

			var testRate = Factory.New<ClientRate>();
			var testCollection = RetrieveGlobalAndCostingLines(testRate, RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AUSYD", ZString.Empty, CC01);
			AssertEquals("Count", 0, testCollection.Count);

			testRate.TH_GC = costsCompany.PK;
			testCollection = RetrieveGlobalAndCostingLines(testRate, RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AUSYD", ZString.Empty, CC01);
			AssertEquals("Count", 3, testCollection.Count);
		}

		public void TestLoadingRelatedRateLinesFromGlobalCosting()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBCHRG");
			var globalCosting = Helper.NewGlobalCosting(Helper.NewOrgHeader());

			var entry1 = globalCosting.AddRateEntry("ORG", "ALL", "AUSYD", "");
			entry1.TI_RateStartDate = ZDate.Today.AddDays(-5);
			entry1.TI_RateEndDate = ZDate.Today.AddDays(5);
			AddGlobalRateLine(entry1, globalChargeCode.PK);

			Factory.Save();

			var testRate = Factory.New<ClientRate>();
			var testCollection = RetrieveGlobalAndCostingLines(testRate, RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AUSYD", ZString.Empty, globalChargeCode.PK);
			AssertEquals("Count", 1, testCollection.Count);
		}

		#region Company Tariff

		public void TestLoadingRelatedRateLinesFromGlobalCompanyTariffs()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var globalTariff = Factory.New<GlobalTariff>();
			var entry1 = globalTariff.AddRateEntry("AIR", "LSE", "USHOU", "AUPER");
			entry1.TI_RS_NKServiceLevel_NI = "STD";
			entry1.RateLines.RemoveAndDeleteAll();

			var line1 = entry1.AddRateLine(globalChargeCode, FlatCalculator.Code);
			((FlatCalculator)line1.Calculator).BaseRate = 200;

			var entry2 = globalTariff.AddRateEntry("AIR", "LSE", "USHOU", "AUPER");
			entry2.TI_RS_NKServiceLevel_NI = "LIV";
			entry2.RateLines.RemoveAndDeleteAll();

			var line2 = entry2.AddRateLine(globalChargeCode, FlatCalculator.Code);
			((FlatCalculator)line2.Calculator).BaseRate = 300;

			Factory.Save();

			var rate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var rateEntry = rate.AddRateEntry("AIR", "LSE", "USHOU", "AUPER");

			AssertContainsExactElementsInAnyOrder(new[] { line1.PK, line2.PK }, rateEntry.RelatedRateLines.Cast<RateLine>().Select(o => o.PK).ToArray());
		}

		public void TestLoadingRelatedRateLinesFromGlobalCostingAndTariff()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var globalTariff = Factory.New<CompanyTariff>();
			globalTariff.TH_GC = ZGuid.Empty;

			var entry1 = globalTariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry1.TI_RS_NKServiceLevel_NI = "STD";
			entry1.RateLines.RemoveAndDeleteAll();

			var line1 = entry1.AddRateLine(globalChargeCode, FlatCalculator.Code);
			((FlatCalculator)line1.Calculator).BaseRate = 200;

			var entry2 = globalTariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry2.TI_RS_NKServiceLevel_NI = "LIV";
			entry2.RateLines.RemoveAndDeleteAll();

			var line2 = entry2.AddRateLine(globalChargeCode, FlatCalculator.Code);
			((FlatCalculator)line2.Calculator).BaseRate = 300;

			var globalCosting = Helper.NewGlobalCosting(Helper.NewOrgHeader());

			var entryCosting = globalCosting.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entryCosting.TI_RateStartDate = ZDate.Today.AddDays(-5);
			entryCosting.TI_RateEndDate = ZDate.Today.AddDays(5);
			entryCosting.RateLines.RemoveAndDeleteAll();
			AddGlobalRateLine(entryCosting, globalChargeCode.PK);

			Factory.Save();

			var rate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var rateEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			AssertContainsExactElementsInAnyOrder(new[] { line1.PK, line2.PK, entryCosting.RateLines[0].PK }, rateEntry.RelatedRateLines.Cast<RateLine>().Select(o => o.PK).ToArray());
		}

		public void TestLoadingAllTariffLinesEvenIfCompanyTariffLevelIsZeroByMistake()
		{
			var tariff = Factory.New<CompanyTariff>();

			var entry1 = tariff.AddRateEntry("AIR", "LSE", "USHOU", "AUPER");
			entry1.TI_RS_NKServiceLevel_NI = "STD";

			entry1.RateLines.RemoveAndDeleteAll();

			var line1 = entry1.AddRateLine("FRT", FlatCalculator.Code);
			((FlatCalculator)line1.Calculator).BaseRate = 200;

			var entry2 = tariff.AddRateEntry("AIR", "LSE", "USHOU", "AUPER");
			entry2.TI_RS_NKServiceLevel_NI = "LIV";

			entry2.RateLines.RemoveAndDeleteAll();

			var line2 = entry2.AddRateLine("FRT", FlatCalculator.Code);
			((FlatCalculator)line2.Calculator).BaseRate = 300;
			line2.TL_CompanyTariffLevel = 0;

			Factory.Save();

			var rate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var rateEntry = rate.AddRateEntry("AIR", "LSE", "USHOU", "AUPER");
			AssertContainsExactElementsInAnyOrder(new[] { line1.PK, line2.PK }, rateEntry.RelatedRateLines.Cast<RateLine>().Select(o => o.PK).ToArray());
		}

		public void TestLoadTariffLinesWithinCompanyTariffLevel()
		{
			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry("AIR", "LSE", "USHOU", "AUPER");

			var line1 = tariffEntry.RateLines[0];
			line1.TL_CompanyTariffLevel = 1;

			var line2 = tariffEntry.AddRateLine("BAF", FlatCalculator.Code);
			line2.TL_CompanyTariffLevel = 2;

			Factory.Save();

			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader(2));
			var rateEntry = rate1.AddRateEntry("AIR", "LSE", "USHOU", "AUPER");
			AssertContainsExactElementsInAnyOrder(new[] { line1.PK, line2.PK }, rateEntry.RelatedRateLines.Cast<RateLine>().Select(o => o.PK));

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader(1));
			rateEntry = rate2.AddRateEntry("AIR", "LSE", "USHOU", "AUPER");
			AssertContainsExactElementsInAnyOrder(new[] { line1.PK }, rateEntry.RelatedRateLines.Cast<RateLine>().Select(o => o.PK));
		}

		public void TestRelatedRateLinesCompanyTariff_CrossTrade()
		{
			Env.Registry.GlobalTariffDefault = 2;

			var clientRate = Helper.NewClientRate(NewClient);

			Helper.AddRateEntryWithSingleRateLine(CompanyTariff1, RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "SGSIN", "FRT", 100m);
			Helper.SetNonLevel1CompanyTariff(CompanyTariff2, discountType: RatingConstants.RateCategory.FCL, discount: 10m);

			TestHelper.NewLevel(NewClient, tariffType: "FRT", mode: Core.Constants.RateMode.SEA, direction: nameof(OrgRateTariffLevel.Directions.IMP), level: 1);
			TestHelper.NewLevel(NewClient, tariffType: "FRT", mode: Core.Constants.RateMode.SEA, direction: nameof(OrgRateTariffLevel.Directions.EXP), level: 1);

			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "SGSIN", "BAF", 1000m);
			AssertRelatedRateLines
			(
				clientRateEntry,
				expectedBaseRates: new[] { "90" },
				$"GIVEN cross trade with NO company tariff levels ALL THEN should fall back on registry"
			);
		}

		public void TestRelatedRateLinesCompanyTariff_CrossTrade_CompanyTariffLevelWithALLDirection()
		{
			var clientRate = Helper.NewClientRate(NewClient);

			Helper.AddRateEntryWithSingleRateLine(CompanyTariff1, RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "SGSIN", "FRT", 100m);
			Helper.SetNonLevel1CompanyTariff(CompanyTariff2, discountType: RatingConstants.RateCategory.FCL, discount: 10m);

			TestHelper.NewLevel(NewClient, tariffType: "FRT", mode: Core.Constants.RateMode.SEA, direction: nameof(OrgRateTariffLevel.Directions.ALL), level: 2);

			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "SGSIN", "BAF", 1000m);
			AssertRelatedRateLines
			(
				clientRateEntry,
				expectedBaseRates: new[] { "90" },
				$"GIVEN cross trade with company tariff levels ALL THEN should have company tariff"
			);
		}

		public void TestRelatedRateLinesCompanyTariff_CompanyTariffLevel0()
		{
			var clientRate = Helper.NewClientRate(NewClient);

			Helper.AddRateEntryWithSingleRateLine(CompanyTariff1, RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "AUSYD", "FRT", 100m);
			Helper.SetNonLevel1CompanyTariff(CompanyTariff2, discountType: RatingConstants.RateCategory.FCL, discount: 10m);

			TestHelper.NewLevel(NewClient, tariffType: "FRT", mode: Core.Constants.RateMode.SEA, direction: nameof(OrgRateTariffLevel.Directions.IMP), level: 0);

			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "AUSYD", "BAF", 1000m);
			AssertRelatedRateLines
			(
				clientRateEntry,
				expectedBaseRates: System.Array.Empty<string>(),
				$"GIVEN company tariff levels are 0 THEN should not have company tariff"
			);
		}

		public void TestRelatedRateLinesCompanyTariff_Level()
		{
			var clientRate = Helper.NewClientRate(NewClient);

			Helper.AddRateEntryWithSingleRateLine(CompanyTariff1, RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "AUSYD", "FRT", 100m);
			Helper.SetNonLevel1CompanyTariff(CompanyTariff2, discountType: RatingConstants.RateCategory.FCL, discount: 10m);

			TestHelper.NewLevel(NewClient, tariffType: "FRT", mode: Core.Constants.RateMode.SEA, direction: nameof(OrgRateTariffLevel.Directions.IMP), level: 1);
			TestHelper.NewLevel(NewClient, tariffType: "FRT", mode: Core.Constants.RateMode.SEA, direction: nameof(OrgRateTariffLevel.Directions.EXP), level: 2);

			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "AUSYD", "BAF", 1000m);
			AssertRelatedRateLines
			(
				clientRateEntry,
				expectedBaseRates: new[] { "100" },
				$"GIVEN company tariff levels are 1 for IMP and 2 for EXP THEN IMPORT rate should have company tariff level 1"
			);
		}

		public void TestRelatedRateLinesCompanyTariff()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			CombineAssertions("GIVEN ClientRate with x OrgRateTariffLevel then RelatedRateLines should show Company Tariff Level x", () =>
			{
				AssertRelatedRateLinesCompanyTariff_TestCoverage();

				foreach (var testCase in RelatedRateLinesCompanyTariffTestCases)
				{
					AssertRelatedRateLinesCompanyTariff(clientRate, testCase);
				}
			});
		}

		readonly ReadOnlyCollection<RelatedRateLinesCompanyTariffTestCase> RelatedRateLinesCompanyTariffTestCases = new ReadOnlyCollection<RelatedRateLinesCompanyTariffTestCase>
		(
			new[]
			{
				//RateType.Forwarding
				//RateType.Forwarding > RateCategory.AIR
				new RelatedRateLinesCompanyTariffTestCase("AIR", "LSE", "FRT", "LSE", 1m, 10m, new[] { 1m }, new[] { 0.9m }),
				new RelatedRateLinesCompanyTariffTestCase("AIR", "ULD", "FRT", "ULD", 2m, 10m, new[] { 2m }, new[] { 1.8m }),
				new RelatedRateLinesCompanyTariffTestCase("AIR", "BCN", "FRT", "BCN", 2m, 10m, new[] { 2m }, new[] { 1.8m }),
				new RelatedRateLinesCompanyTariffTestCase("AIR", "SCN", "FRT", "SCN", 2m, 10m, new[] { 2m }, new[] { 1.8m }),
				//RateType.Forwarding > RateCategory.FCL
				new RelatedRateLinesCompanyTariffTestCase("FCL", "SEA", "FRT", "SEA", 3m, 10m, new[] { 3m }, new[] { 2.7m }), // Sea Freight FCL
				new RelatedRateLinesCompanyTariffTestCase("FCL", "ROA", "FRT", "ROA", 4m, 10m, new[] { 4m }, new[] { 3.6m }), // Road Freight FCL
				new RelatedRateLinesCompanyTariffTestCase("FCL", "RAI", "FRT", "RAI", 5m, 10m, new[] { 5m }, new[] { 4.5m }), // Rail Freight FCL
				new RelatedRateLinesCompanyTariffTestCase("FCL", "BCN", "FRT", "BCN", 50000m, 10m, new[] { 50000m }, new[] { 45000m }),
				new RelatedRateLinesCompanyTariffTestCase("FCL", "SCN", "FRT", "SCN", 50000m, 10m, new[] { 50000m }, new[] { 45000m }),
				//RateType.Forwarding > RateCategory.LCL
				new RelatedRateLinesCompanyTariffTestCase("LCL", "LCL", "FRT", "LCL", 6m, 10m, new[] { 6m }, new[] { 5.4m }), // Sea Freight LCL
				new RelatedRateLinesCompanyTariffTestCase("LCL", "LRO", "FRT", "LRO", 7m, 10m, new[] { 7m }, new[] { 6.3m }), // Road Freight LTL
				new RelatedRateLinesCompanyTariffTestCase("LCL", "FTL", "FRT", "FTL", 8m, 10m, new[] { 8m }, new[] { 7.2m }), // Road Freight FTL
				new RelatedRateLinesCompanyTariffTestCase("LCL", "LRA", "FRT", "LRA", 9m, 10m, new[] { 9m }, new[] { 8.1m }), // Rail Freight LWL
				new RelatedRateLinesCompanyTariffTestCase("LCL", "FWL", "FRT", "FWL", 10m, 10m, new[] { 10m }, new[] { 9m }), // Rail Freight FWL
				new RelatedRateLinesCompanyTariffTestCase("LCL", "BLK", "FRT", "BLK", 100000m, 10m, new[] { 100000m }, new[] { 90000m }),
				new RelatedRateLinesCompanyTariffTestCase("LCL", "OBC", "FRT", "OBC", 100000m, 10m, new[] { 100000m }, new[] { 90000m }),
				new RelatedRateLinesCompanyTariffTestCase("LCL", "UNA", "FRT", "UNA", 100000m, 10m, new[] { 100000m }, new[] { 90000m }),
				new RelatedRateLinesCompanyTariffTestCase("LCL", "BBK", "FRT", "BBK", 110000m, 10m, new[] { 110000m }, new[] { 99000m }),
				new RelatedRateLinesCompanyTariffTestCase("LCL", "ROR", "FRT", "ROR", 120000m, 10m, new[] { 120000m }, new[] { 108000m }),
				new RelatedRateLinesCompanyTariffTestCase("LCL", "BCN", "FRT", "BCN", 130000m, 10m, new[] { 130000m }, new[] { 117000m }),
				new RelatedRateLinesCompanyTariffTestCase("LCL", "SCN", "FRT", "SCN", 130000m, 10m, new[] { 130000m }, new[] { 117000m }),
				//RateType.Forwarding > RateCategory.ORG
				new RelatedRateLinesCompanyTariffTestCase("ORG", "OBC", "ORG", "OBC", 140000m, 10m, new[] { 140000m }, new[] { 126000m }),
				new RelatedRateLinesCompanyTariffTestCase("ORG", "UNA", "ORG", "UNA", 140000m, 10m, new[] { 140000m }, new[] { 126000m }),
				new RelatedRateLinesCompanyTariffTestCase("ORG", "COU", "ORG", "COU", 140000m, 10m, new[] { 140000m }, new[] { 126000m }),
				new RelatedRateLinesCompanyTariffTestCase("ORG", "ULD", "ORG", "ULD", 40m, 10m, new[] { 40m }, new[] { 36m }), // Air Freight (ULD)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "LSE", "ORG", "LSE", 50m, 10m, new[] { 50m }, new[] { 45m } ), // Air Freight (LSE)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "AIR", "ORG", "AIR", 30m, 10m, new[] { 30m }, new[] { 27m }), // Air Freight (ULD and LSE)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "LCL", "ORG", "LCL", 70m, 10m, new[] { 70m }, new[] { 63m }), // Sea Freight (LCL)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "FCL", "ORG", "FCL", 80m, 10m, new[] { 80m }, new[] { 72m }), // Sea Freight (FCL)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "BLK", "ORG", "BLK", 140000m, 10m, new[] { 140000m }, new[] { 126000m }),
				new RelatedRateLinesCompanyTariffTestCase("ORG", "BBK", "ORG", "BBK", 60m, 10m, new[] { 60m }, new[] { 54m }),
				new RelatedRateLinesCompanyTariffTestCase("ORG", "ROR", "ORG", "ROR", 60m, 10m, new[] { 60m }, new[] { 54m }),
				new RelatedRateLinesCompanyTariffTestCase("ORG", "SEA", "ORG", "SEA", 60m, 10m, new[] { 60m }, new[] { 54m }), // Sea Freight (LCL and FCL)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "BCN", "ORG", "BCN", 60m, 10m, new[] { 60m }, new[] { 54m }),
				new RelatedRateLinesCompanyTariffTestCase("ORG", "SCN", "ORG", "SCN", 60m, 10m, new[] { 60m }, new[] { 54m }),
				new RelatedRateLinesCompanyTariffTestCase("ORG", "LRO", "ORG", "LRO", 100m, 10m, new[] { 100m }, new[] { 90m }), // Road Freight (LCL)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "FRO", "ORG", "FRO", 110m, 10m, new[] { 110m }, new[] { 99m }), // Road Freight (FCL)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "FTL", "ORG", "FTL", 120m, 10m, new[] { 120m }, new[] { 108m }), // Road Freight (FTL)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "ROA", "ORG", "ROA", 90m, 10m, new[] { 90m }, new[] { 81m }), // Road Freight (LCL, FCL AND FTL)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "LRA", "ORG", "LRA", 140m, 10m,  new[] { 140m }, new[] { 126m }), // Rail Freight (LCL)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "FRA", "ORG", "FRA", 150m, 10m,  new[] { 150m }, new[] { 135m }), // Rail Freight (FCL)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "FWL", "ORG", "FWL", 160m, 10m, new[] { 160m }, new[] { 144m }), // Rail Freight (FWL)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "RAI", "ORG", "RAI", 130m, 10m, new[] { 130m }, new[] { 117m }), // Rail Freight (LCL, FCL, FWL)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "MAI", "ORG", "MAI", 170m, 10m, new[] { 170m }, new[] { 153m }),
				new RelatedRateLinesCompanyTariffTestCase("ORG", "ALL", "ORG", "ALL", 20m, 10m, new[] { 20m }, new[] { 18m }),

				////RateType.Forwarding > RateCategory.DST
				new RelatedRateLinesCompanyTariffTestCase("DST", "OBC", "DST", "OBC", 1400m, 10m, new[] { 1400m }, new[] { 1260m }),
				new RelatedRateLinesCompanyTariffTestCase("DST", "UNA", "DST", "UNA", 1400m, 10m, new[] { 1400m }, new[] { 1260m }),
				new RelatedRateLinesCompanyTariffTestCase("DST", "COU", "DST", "COU", 1400m, 10m, new[] { 1400m }, new[] { 1260m }),
				new RelatedRateLinesCompanyTariffTestCase("DST", "ULD", "DST", "ULD", 1200m, 10m, new[] { 1200m }, new[] { 1080m }), // Air Freight (ULD)
				new RelatedRateLinesCompanyTariffTestCase("DST", "LSE", "DST", "LSE", 1300m, 10m, new[] { 1300m }, new[] { 1170m } ), // Air Freight (LSE)
				new RelatedRateLinesCompanyTariffTestCase("DST", "AIR", "DST", "AIR", 1100m, 10m, new[] { 1100m }, new[] { 990m }), // Air Freight (ULD and LSE)
				new RelatedRateLinesCompanyTariffTestCase("DST", "LCL", "DST", "LCL", 1500m, 10m, new[] { 1500m }, new[] { 1350m }), // Sea Freight (LCL)
				new RelatedRateLinesCompanyTariffTestCase("DST", "FCL", "DST", "FCL", 1600m, 10m, new[] { 1600m }, new[] { 1440m }), // Sea Freight (FCL)
				new RelatedRateLinesCompanyTariffTestCase("DST", "BLK", "DST", "BLK", 1400m, 10m, new[] { 1400m }, new[] { 1260m }),
				new RelatedRateLinesCompanyTariffTestCase("DST", "BBK", "DST", "BBK", 1400m, 10m, new[] { 1400m }, new[] { 1260m }),
				new RelatedRateLinesCompanyTariffTestCase("DST", "ROR", "DST", "ROR", 1400m, 10m, new[] { 1400m }, new[] { 1260m }),
				new RelatedRateLinesCompanyTariffTestCase("DST", "SEA", "DST", "SEA", 1400m, 10m, new[] { 1400m }, new[] { 1260m }), // Sea Freight (LCL and FCL)
				new RelatedRateLinesCompanyTariffTestCase("DST", "BCN", "DST", "BCN", 1400m, 10m, new[] { 1400m }, new[] { 1260m }),
				new RelatedRateLinesCompanyTariffTestCase("DST", "SCN", "DST", "SCN", 1400m, 10m, new[] { 1400m }, new[] { 1260m }),
				new RelatedRateLinesCompanyTariffTestCase("DST", "LRO", "DST", "LRO", 1800m, 10m, new[] { 1800m }, new[] { 1620m }), // Road Freight (LCL)
				new RelatedRateLinesCompanyTariffTestCase("DST", "FRO", "DST", "FRO", 1900m, 10m, new[] { 1900m }, new[] { 1710m }), // Road Freight (FCL)
				new RelatedRateLinesCompanyTariffTestCase("DST", "FTL", "DST", "FTL", 2000m, 10m, new[] { 2000m }, new[] { 1800m }), // Road Freight (FTL)
				new RelatedRateLinesCompanyTariffTestCase("DST", "ROA", "DST", "ROA", 1700m, 10m, new[] { 1700m }, new[] { 1530m }), // Road Freight (LCL, FCL AND FTL)
				new RelatedRateLinesCompanyTariffTestCase("DST", "LRA", "DST", "LRA", 2200m, 10m, new[] { 2200m }, new[] { 1980m }), // Rail Freight (LCL)
				new RelatedRateLinesCompanyTariffTestCase("DST", "FRA", "DST", "FRA", 2300m, 10m, new[] { 2300m }, new[] { 2070m }), // Rail Freight (FCL)
				new RelatedRateLinesCompanyTariffTestCase("DST", "FWL", "DST", "FWL", 2400m, 10m, new[] { 2400m }, new[] { 2160m }), // Rail Freight (FWL)
				new RelatedRateLinesCompanyTariffTestCase("DST", "RAI", "DST", "RAI", 2100m, 10m, new[] { 2100m }, new[] { 1890m }), // Rail Freight (LCL, FCL, FWL)
				new RelatedRateLinesCompanyTariffTestCase("DST", "MAI", "DST", "MAI", 2500m, 10m, new[] { 2500m }, new[] { 2250m }),
				new RelatedRateLinesCompanyTariffTestCase("DST", "ALL", "DST", "ALL", 1000m, 10m, new[] { 1000m }, new[] { 900m }),

				//RateType.Shipping
				//RateType.Shipping > RateCategory.SCO
				new RelatedRateLinesCompanyTariffTestCase("SCO", "SEA", "SFR", "SEA", 10000m, 10m, new[] { 10000m }, new[] { 9000m }),
				//RateType.Shipping > RateCategory.SNC
				new RelatedRateLinesCompanyTariffTestCase("SNC", "LCL", "SFR", "LCL", 11000m, 10m, new[] { 11000m }, new[] { 9900m }),
				//RateType.Shipping > RateCategory.SOR
				new RelatedRateLinesCompanyTariffTestCase("SOR", "LCL", "SOR", "LCL", 13000m, 10m, new[] { 13000m }, new[] { 11700m }),
				new RelatedRateLinesCompanyTariffTestCase("SOR", "FCL", "SOR", "FCL", 14000m, 10m, new[] { 14000m }, new[] { 12600m }),
				new RelatedRateLinesCompanyTariffTestCase("SOR", "ALL", "SOR", "ALL", 12000m, 10m, new[] { 12000m }, new[] { 10800m }),
				//RateType.Shipping > RateCategory.SDE
				new RelatedRateLinesCompanyTariffTestCase("SDE", "LCL", "SDE", "LCL", 16000m, 10m, new[] { 16000m }, new[] { 14400m }),
				new RelatedRateLinesCompanyTariffTestCase("SDE", "FCL", "SDE", "FCL", 17000m, 10m, new[] { 17000m }, new[] { 15300m }),
				new RelatedRateLinesCompanyTariffTestCase("SDE", "ALL", "SDE", "ALL", 15000m, 10m, new[] { 15000m }, new[] { 13500m }),
			}
		);

		public void TestRelatedRateLinesCompanyTariff_OriginAndDestination_SEAMode()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			CombineAssertions("GIVEN ClientRate with x OrgRateTariffLevel then RelatedRateLines should show Company Tariff Level x", () =>
			{
				foreach (var testCase in RelatedRateLinesCompanyTariffForOriginAndDestinationSEAModeTestCases)
				{
					AssertRelatedRateLinesCompanyTariff(clientRate, testCase);
				}
			});
		}

		readonly ReadOnlyCollection<RelatedRateLinesCompanyTariffTestCase> RelatedRateLinesCompanyTariffForOriginAndDestinationSEAModeTestCases = new ReadOnlyCollection<RelatedRateLinesCompanyTariffTestCase>
		(
			new[]
			{
				//RateType.Forwarding > RateCategory.ORG
				new RelatedRateLinesCompanyTariffTestCase("ORG", "SEA", "ORG", "SEA", 10m, 10m, new[] { 10m }, new[] { 9m }), // Sea Freight (LCL and FCL)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "FCL", "ORG", "FCL", 20m, 10m, new[] { 9m, 20m }, new[] { 9m, 18m }), // Sea Freight (FCL)
				new RelatedRateLinesCompanyTariffTestCase("ORG", "BLK", "ORG", "BLK", 30m, 10m, new[] { 9m, 30m }, new[] { 9m, 27m }),
				new RelatedRateLinesCompanyTariffTestCase("ORG", "BBK", "ORG", "BBK", 40m, 10m, new[] { 9m, 40m }, new[] { 9m, 36m }),
				new RelatedRateLinesCompanyTariffTestCase("ORG", "ROR", "ORG", "ROR", 50m, 10m, new[] { 9m, 50m }, new[] { 9m, 45m }),

				////RateType.Forwarding > RateCategory.DST
				new RelatedRateLinesCompanyTariffTestCase("DST", "SEA", "DST", "SEA", 100m, 10m, new[] { 100m }, new[] { 90m }), // Sea Freight (LCL and FCL)
				new RelatedRateLinesCompanyTariffTestCase("DST", "FCL", "DST", "FCL", 110m, 10m, new[] { 90m, 110m }, new[] { 90m, 99m }), // Sea Freight (FCL)
				new RelatedRateLinesCompanyTariffTestCase("DST", "BLK", "DST", "BLK", 120m, 10m, new[] { 90m, 120m }, new[] { 90m, 108m }),
				new RelatedRateLinesCompanyTariffTestCase("DST", "BBK", "DST", "BBK", 130m, 10m, new[] { 90m, 130m }, new[] { 90m, 117m }),
				new RelatedRateLinesCompanyTariffTestCase("DST", "ROR", "DST", "ROR", 140m, 10m, new[] { 90m, 140m }, new[] { 90m, 126m }),
			}
		);

		void AssertRelatedRateLinesCompanyTariff_TestCoverage()
		{
			foreach (var rateType in new[] { RateType.Forwarding, RateType.Shipping })
			{
				var rateCategories = RatingConstants.RateCategory.GetRateCategories(rateType, RateCategoryGroup.All);
				foreach (var rateCategory in rateCategories)
				{
					var rateModes = RateEntryLookups.GetTransportModesByRateCategory(rateCategory);
					foreach (CodeDescriptionPair rateMode in rateModes)
					{
						var isFound = RelatedRateLinesCompanyTariffTestCases.Any(x => x.RateCategory == rateCategory && x.RateMode == rateMode.Code);
						if (!isFound)
						{
							Assert($"Please implement test case for rate category = '{rateCategory}' and rate mode = '{rateMode}'", false);
						}
					}
				}
			}
		}

		void AssertRelatedRateLinesCompanyTariff(ClientRate clientRate, RelatedRateLinesCompanyTariffTestCase testCase)
		{
			var rateCategory = testCase.RateCategory;
			var rateMode = testCase.RateMode;

			var levelInfoAndExpectedAmount = testCase.LevelInfoAndExpectedAmount;

			var companyTariffRateEntry = Helper.AddRateEntryWithSingleRateLine(CompanyTariff1, rateCategory, rateMode, "USLAX", "AUSYD", "FRT", levelInfoAndExpectedAmount.CompanyTariff1Amount);
			Helper.SetNonLevel1CompanyTariff(CompanyTariff2, discountType: rateCategory, discount: levelInfoAndExpectedAmount.CompanyTariff2Discount);

			var orgRateTariffLevel = GetOrCreateLevel(NewClient, tariffType: levelInfoAndExpectedAmount.TariffLevelType, mode: levelInfoAndExpectedAmount.TariffLevelMode, direction: nameof(OrgRateTariffLevel.Directions.IMP), level: 1);

			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(rateCategory, rateMode, "USLAX", "AUSYD", "BAF", 100);
			AssertRelatedRateLines
			(
				clientRateEntry,
				expectedBaseRates: levelInfoAndExpectedAmount.ExpectedCompanyTariff1Amounts.Select(x => x.ToString()).ToArray(),
				$"* RateCategory: {rateCategory}, RateMode: {rateMode}, LevelType: {levelInfoAndExpectedAmount.TariffLevelType}, LevelMode: {levelInfoAndExpectedAmount.TariffLevelMode} with Level 1 - {levelInfoAndExpectedAmount.Message}"
			);

			orgRateTariffLevel.P7_TariffLevel = 2;
			AssertRelatedRateLines
			(
				clientRateEntry,
				expectedBaseRates: levelInfoAndExpectedAmount.ExpectedCompanyTariff2Amounts.Select(x => x.ToString()).ToArray(),
				$"* RateCategory: {rateCategory}, RateMode: {rateMode}, LevelType: {levelInfoAndExpectedAmount.TariffLevelType}, LevelMode: {levelInfoAndExpectedAmount.TariffLevelMode} with Level 2 - {levelInfoAndExpectedAmount.Message}"
			);
		}

		OrgRateTariffLevel GetOrCreateLevel(OrgHeader orgHeader, ZString tariffType, ZString mode, ZString direction, ZByte level)
		{
			var orgRateTariffLevel = orgHeader.CompanyData.RateTariffLevels.Cast<OrgRateTariffLevel>().FirstOrDefault(x => x.P7_TariffType == tariffType && x.P7_Mode == mode);
			if (orgRateTariffLevel == null)
			{
				orgRateTariffLevel = orgHeader.CompanyData.RateTariffLevels.AddNew();
				orgRateTariffLevel.P7_TariffType = tariffType;
				orgRateTariffLevel.P7_Mode = mode;
			}

			orgRateTariffLevel.P7_Direction = direction;
			orgRateTariffLevel.P7_TariffLevel = level;
			return orgRateTariffLevel;
		}

		static void AssertRelatedRateLines(RateEntry rateEntry, string[] expectedBaseRates, string message)
		{
			var actual = rateEntry.RelatedRateLines.Cast<RateLine>().Select(rateLine => rateLine.GetCalculator<FlatCalculator>().BaseRate.ToStringTrimZeros());
			AssertContainsExactElementsInAnyOrder(message, expectedBaseRates, actual);
		}

		class RelatedRateLinesCompanyTariffTestCase
		{
			public RelatedRateLinesCompanyTariffTestCase(string rateCategory, string rateMode, string tariffLevelType, string tariffLevelMode, decimal companyTariff1Amount, decimal companyTariff2Discount, decimal[] expectedCompanyTariff1Amounts, decimal[] expectedCompanyTariff2Amounts, string message = default)
			{
				RateCategory = rateCategory;
				RateMode = rateMode;
				LevelInfoAndExpectedAmount = new LevelInfoAndExpectedAmount(tariffLevelType, tariffLevelMode, companyTariff1Amount, companyTariff2Discount, expectedCompanyTariff1Amounts, expectedCompanyTariff2Amounts, message);
			}

			public RelatedRateLinesCompanyTariffTestCase(string rateCategory, string rateMode, LevelInfoAndExpectedAmount levelInfoAndExpectedAmounts)
			{
				RateCategory = rateCategory;
				RateMode = rateMode;
				LevelInfoAndExpectedAmount = levelInfoAndExpectedAmounts;
			}

			public readonly string RateCategory;
			public readonly string RateMode;
			public readonly LevelInfoAndExpectedAmount LevelInfoAndExpectedAmount;
		}

		class LevelInfoAndExpectedAmount
		{
			public LevelInfoAndExpectedAmount(string tariffLevelType, string tariffLevelMode, decimal companyTariff1Amount, decimal companyTariff2Discount, decimal[] expectedCompanyTariff1Amounts, decimal[] expectedCompanyTariff2Amounts, string message = default)
			{
				TariffLevelType = tariffLevelType;
				TariffLevelMode = tariffLevelMode;
				CompanyTariff1Amount = companyTariff1Amount;
				CompanyTariff2Discount = companyTariff2Discount;
				ExpectedCompanyTariff1Amounts = expectedCompanyTariff1Amounts;
				ExpectedCompanyTariff2Amounts = expectedCompanyTariff2Amounts;
				Message = message;
			}

			public readonly string TariffLevelType;
			public readonly string TariffLevelMode;
			public readonly decimal CompanyTariff1Amount;
			public readonly decimal CompanyTariff2Discount;
			public readonly decimal[] ExpectedCompanyTariff1Amounts;
			public readonly decimal[] ExpectedCompanyTariff2Amounts;
			public readonly string Message;
		}

		CompanyTariff CompanyTariff1
		{
			get
			{
				if (companyTariff1 == null)
				{
					companyTariff1 = Helper.NewCompanyTariff();
					companyTariff1.TH_GlobalRateLevel = 1;
					companyTariff1.Factory.Save();
				}
				return companyTariff1;
			}
		}

		CompanyTariff companyTariff1;

		CompanyTariff CompanyTariff2
		{
			get
			{
				if (companyTariff2 == null)
				{
					companyTariff2 = Helper.NewCompanyTariff();
					companyTariff2.TH_GlobalRateLevel = 2;
					companyTariff2.Factory.Save();
				}
				return companyTariff2;
			}
		}

		CompanyTariff companyTariff2;

		#endregion

		#endregion

		#region Data Refresh Bus

		public void TestDataRefreshBusDisabled()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = testRate.AddRateEntry("ORG");

			Factory.Save();

			var factory2 = new BusinessObjectFactory();

			var entryInFirstFactory = Factory.Load<ClientRate>(testRate.PK).GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var entryInSecondFactory = factory2.Load<ClientRate>(testRate.PK).GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];

			AssertEquals("Precondition: No Lines in either rate", 0, entryInFirstFactory.RelatedRateLines.Count);
			AssertEquals("Precondition: No Lines in either rate", 0, entryInSecondFactory.RelatedRateLines.Count);

			var line = entryInFirstFactory.RelatedRateLines.AddNew();
			line.TL_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK)).PK;
			line.TL_TI = entry.PK;
			line.TL_RX_NKCurrency = "USD";
			Factory.Save();

			AssertEquals("Entry in first factory has 1 cost line", 1, entryInFirstFactory.RelatedRateLines.Count);
			AssertEquals("Entry in second factory has no cost lines as the collection is NOT published for datarefresh", 0, entryInSecondFactory.RelatedRateLines.Count);
		}

		#endregion

		#region Implementation

		RelatedRateLinesCollection RetrieveGlobalAndCostingLines(RatingHeader header, ZString entryType, ZString mode, ZString origin, ZString destination, ZGuid chargeCode)
		{
			return RetrieveGlobalAndCostingLines(header, entryType, mode, origin, destination, chargeCode, 0, false);
		}

		RelatedRateLinesCollection RetrieveGlobalAndCostingLines(RatingHeader header, ZString entryType, ZString mode, ZString origin, ZString destination, ZGuid chargeCode, ZByte globalRateLevel, bool setTransportProvider)
		{
			var entry = header.AddRateEntry(entryType, mode, origin, destination, "STD", "");
			if (setTransportProvider)
			{
				entry.TI_OH_TransportProvider = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "COSTING").PK;
			}

			var rateLine = entry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode;
			if (header.IsTariff())
			{
				rateLine.TL_CompanyTariffLevel = globalRateLevel;
			}

			var testCollection = new RelatedRateLinesCollection(Factory, entry);
			testCollection.Load();

			return testCollection;
		}

		RatingHeader PostFreightCosting()
		{
			var header = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = header.AddRateEntry("LCL", "LCL", "AU", "USLAX");
			entry1.TI_RateStartDate = ZDate.Today.AddDays(-5);
			entry1.TI_RateEndDate = ZDate.Today.AddDays(5);
			entry1.RateLines.RemoveAndDeleteAll();
			AddRateLines(entry1);

			var entry2 = header.AddRateEntry("LCL", "LCL", "AUSYD", "USCA");
			entry2.TI_RateStartDate = ZDate.Today.AddDays(-30);
			entry2.TI_RateEndDate = ZDate.Today.AddDays(-6);
			entry2.RateLines.RemoveAndDeleteAll();
			AddRateLines(entry2);

			var entry3 = header.AddRateEntry("LCL", "LCL", "AUEC", "USLAX");
			entry3.TI_RateStartDate = ZDate.Today.AddDays(6);
			entry3.TI_RateEndDate = ZDate.Today.AddDays(30);
			entry3.RateLines.RemoveAndDeleteAll();
			AddRateLines(entry3);

			var entry4 = header.AddRateEntry("LCL", "LCL", "US", "AUSYD");
			entry4.TI_RateStartDate = ZDate.Today.AddDays(-5);
			entry4.TI_RateEndDate = ZDate.Today.AddDays(5);
			entry4.RateLines.RemoveAndDeleteAll();
			AddRateLines(entry4);

			return header;
		}

		void PostSupplementalCosting()
		{
			var header = Helper.NewCosting(Helper.NewOrgHeader());
			header.Header.OH_Code = "COSTING";

			var entry1 = header.AddRateEntry("ORG", "ALL", "AUSYD", "");
			entry1.TI_RateStartDate = ZDate.Today.AddDays(-5);
			entry1.TI_RateEndDate = ZDate.Today.AddDays(5);
			AddRateLines(entry1);

			var entry2 = header.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry2.TI_RateStartDate = ZDate.Today.AddDays(-30);
			entry2.TI_RateEndDate = ZDate.Today.AddDays(-6);
			AddRateLines(entry2);

			var entry3 = header.AddRateEntry("DST", "ALL", "USLAX", "");
			entry3.TI_RateStartDate = ZDate.Today.AddDays(6);
			entry3.TI_RateEndDate = ZDate.Today.AddDays(30);
			AddRateLines(entry3);

			var entry4 = header.AddRateEntry("ORG", "LCL", "AUSYD", "");
			entry4.TI_RateStartDate = ZDate.Today.AddDays(6);
			entry4.TI_RateEndDate = ZDate.Today.AddDays(30);
			AddRateLines(entry4);
		}

		void PostFreightGlobal()
		{
			var header = Factory.New<CompanyTariff>();

			var entry1 = header.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry1.TI_RateStartDate = ZDate.Today.AddDays(-5);
			entry1.TI_RateEndDate = ZDate.Today.AddDays(5);

			entry1.RateLines.RemoveAndDeleteAll();
			AddRateLines(entry1);

			var entry2 = header.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry2.TI_RateStartDate = ZDate.Today.AddDays(-30);
			entry2.TI_RateEndDate = ZDate.Today.AddDays(-6);

			entry2.RateLines.RemoveAndDeleteAll();
			AddRateLines(entry2);

			var entry3 = header.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry3.TI_RateStartDate = ZDate.Today.AddDays(6);
			entry3.TI_RateEndDate = ZDate.Today.AddDays(30);

			entry3.RateLines.RemoveAndDeleteAll();
			AddRateLines(entry3);

			var entry4 = header.AddRateEntry("LCL", "LCL", "USLAX", "AUSYD");
			entry4.TI_RateStartDate = ZDate.Today.AddDays(-5);
			entry4.TI_RateEndDate = ZDate.Today.AddDays(5);
			entry4.RateLines.RemoveAndDeleteAll();
			AddRateLines(entry4);
		}

		void PostSupplementalGlobal(RateType rateType, bool isGlobal = false)
		{
			var header = Factory.New<CompanyTariff>();

			var originCategory = RatingConstants.RateCategory.GetRateCategories(rateType, RateCategoryGroup.Origin);
			var destinationCategory = RatingConstants.RateCategory.GetRateCategories(rateType, RateCategoryGroup.Destination);

			if (originCategory.Length > 0)
			{
				var entry1 = header.AddRateEntry(originCategory[0], "SEA", "AUSYD", "");
				entry1.TI_RateStartDate = ZDate.Today.AddDays(-5);
				entry1.TI_RateEndDate = ZDate.Today.AddDays(5);
				AddRateLines(entry1);

				var entry2 = header.AddRateEntry(originCategory[0], "AIR", "AUSYD", "");
				entry2.TI_RateStartDate = ZDate.Today.AddDays(-30);
				entry2.TI_RateEndDate = ZDate.Today.AddDays(-6);
				AddRateLines(entry2);
			}

			if (destinationCategory.Length > 0)
			{
				var entry3 = header.AddRateEntry(destinationCategory[0], "ALL", "USLAX", "");
				entry3.TI_RateStartDate = ZDate.Today.AddDays(6);
				entry3.TI_RateEndDate = ZDate.Today.AddDays(30);
				AddRateLines(entry3);
			}

			if (originCategory.Length > 0)
			{
				var entry4 = header.AddRateEntry(originCategory[0], "ALL", "AUSYD", "");
				entry4.TI_RateStartDate = ZDate.Today.AddDays(6);
				entry4.TI_RateEndDate = ZDate.Today.AddDays(30);
				AddRateLines(entry4);

				var entry5 = header.AddRateEntry(originCategory[0], "ALL", "AUSYD", "");
				entry5.TI_RateStartDate = ZDate.Today.AddDays(-5);
				entry5.TI_RateEndDate = ZDate.Today.AddDays(5);
				AddRateLines(entry5);
			}
		}

		void AddGlobalRateLine(RateEntry entry, ZGuid globalChargeCode)
		{
			var rateLine = entry.RateLines.AddNew();
			rateLine.TL_AC = globalChargeCode;
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.M3;
			rateLine.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
		}

		void AddRateLines(RateEntry entry)
		{
			var chargeCodes = new[] { CC01, CC02, CC03 };

			foreach (var chargeCode in chargeCodes)
			{
				var rateLine = entry.RateLines.AddNew();
				rateLine.TL_AC = chargeCode;
				rateLine.TL_RateCalculator = UnitCalculator.Code;
				rateLine.TL_WeightVolume = QuantityUnit.M3;
				rateLine.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			}
		}

		ZGuid fCC01;
		ZGuid CC01
		{
			get
			{
				if (fCC01.IsEmpty)
				{
					fCC01 = Helper.ChargeCodes.New("CC01", "Charge Code 01", "").PK;
				}

				return fCC01;
			}
		}

		ZGuid fCC02;
		ZGuid CC02
		{
			get
			{
				if (fCC02.IsEmpty)
				{
					fCC02 = Helper.ChargeCodes.New("CC02", "Charge Code 02", "").PK;
				}

				return fCC02;
			}
		}

		ZGuid fCC03;
		ZGuid CC03
		{
			get
			{
				if (fCC03.IsEmpty)
				{
					fCC03 = Helper.ChargeCodes.New("CC03", "Charge Code 03", "").PK;
				}

				return fCC03;
			}
		}

		#endregion
	}

	#region Business Object Collection Test

	[TestedType(typeof(RelatedRateLinesCollection))]
	public class RelatedRateLineBizObjCollectionTest : BizObjectCollectionAddDeleteTestCase
	{
		protected override BusinessObjectCollection GetCollection()
		{
			var ratingHeader = Factory.New<RatingHeader>();
			var rateEntry = Factory.New<RateEntry>();
			rateEntry.Parent = ratingHeader;
			return new RelatedRateLinesCollection(Factory, rateEntry);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Collection.AddNew();
		}
	}

	#endregion
}
