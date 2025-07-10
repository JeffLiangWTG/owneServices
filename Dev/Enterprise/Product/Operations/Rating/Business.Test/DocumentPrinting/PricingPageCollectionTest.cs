using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class PricingPageCollectionTest : RatingTestCase
	{
		public void TestPricingPageHasFilterCategoryIsReset()
		{
			var quote = Helper.NewQuote(Consignee);
			quote.SelectedFilterCategory = RatingConstants.RateCategory.LCL;

			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LCL, "", "AUSYD", "ODOC", 100);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "", "CNSHA", "FRT", 100);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "", "GBSUN", "FRT", 100);

			var collection = new PricingPageCollection(quote);
			collection.Load(PricingPaginationStrategy.StandardStyle);

			AssertEquals(3, collection.Count);

			quote.SelectedFilterCategory = RatingConstants.RateCategory.WHS;
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.WHS, RateMode.ALL, "", "AUSYD", "FRT", 100);

			collection = new PricingPageCollection(quote);
			collection.Load(PricingPaginationStrategy.StandardStyle);

			AssertEquals(4, collection.Count);

			quote.SelectedFilterCategory = RatingConstants.RateCategory.TRW;
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.TRW, RateMode.ALL, "", "AUMEL", "FRT", 100);

			collection = new PricingPageCollection(quote);
			collection.Load(PricingPaginationStrategy.StandardStyle);

			AssertEquals(5, collection.Count);

			quote.SelectedFilterCategory = RatingConstants.RateCategory.TWU;
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.TWU, RateMode.ALL, "", "GBSUN", "FRT", 100);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.TWU, RateMode.AIR, "", "AUSYD", "FRT", 100);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.TWU, RateMode.SEA, "", "CNSHA", "FRT", 100);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.TWU, RateMode.ROA, "", "AUMEL", "FRT", 100);

			collection = new PricingPageCollection(quote);
			collection.Load(PricingPaginationStrategy.StandardStyle);

			AssertEquals(9, collection.Count);
		}

		public void TestNoDeletedRatesIncludedInCollection()
		{
			var quote = Helper.NewQuote(Consignee);
			quote.SelectedFilterCategory = RatingConstants.RateCategory.LCL;

			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LCL, "", "CNSHA", "ODOC", 100);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "", "GBSUN", "FRT", 100);
			var entry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LCL, "", "AUSYD", "FRT", 100);
			entry.Delete();

			var collection = new PricingPageCollection(quote);
			collection.Load(PricingPaginationStrategy.StandardStyle);

			AssertEquals(2, collection.Count);
		}

		public void TestRateEntriesWithoutRateLinesAreIncluded()
		{
			var quote = Helper.NewQuote(Consignee);
			var quoteEntryCN = quote.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "CN", "");
			var quoteEntryAU = quote.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AU", "");
			Factory.Save();

			var collection = new PricingPageCollection(quote);
			collection.Load(PricingPaginationStrategy.StandardStyle);

			var actualEntries = collection.Cast<PricingPage>().SelectMany(page => page.RateEntries).Select(e => e.PK);
			var expectedEntries = new ZGuid[]
			{
				quoteEntryCN.PK,
				quoteEntryAU.PK,
			};

			var message = "Should include both quote entries even though they have no rate lines.";
			AssertContainsExactElementsInAnyOrder(message, expectedEntries, actualEntries);
		}

		public void TestMultipleContainerTypesWithOriginDestinationOnly()
		{
			var refContainer1 = Helper.Containers["20GP"];
			var refContainer2 = Helper.Containers["40GP"];
			var refContainer3 = Helper.Containers["40HC"];

			var quote = Helper.NewQuote(Consignee);
			var entry1 = quote.AddRateEntryWithFlatRateLine("ORG", "FCL", "AUSYD", "", "ODOC", 200m);
			var entry2 = quote.AddRateEntryWithFlatRateLine("ORG", "FCL", "AUSYD", "", "ODOC", 400m);
			entry1.TI_RC = refContainer1.PK;
			entry2.TI_RC = refContainer2.PK;

			var collection = new PricingPageCollection(quote);
			collection.Load(PricingPaginationStrategy.LandscapeComplexStyle);

			var pricingPage = collection.Cast<PricingPage>().Single();
			AssertEquals("Collection[0].Count", 1, pricingPage.RateEntries.Count());

			var expected = new[] { refContainer1.RC_Code, refContainer2.RC_Code };
			var actual = pricingPage.ContainerSet.Select(c => c.RC_Code);
			AssertContainsExactElementsInAnyOrder("Collection[0].ContainerList", expected, actual);

			var entry3 = quote.AddRateEntryWithFlatRateLine("ORG", "FCL", "AUMEL", "", "ODOC", 200m);
			var entry4 = quote.AddRateEntryWithFlatRateLine("ORG", "FCL", "AUMEL", "", "ODOC", 440m);
			entry3.TI_RC = refContainer1.PK;
			entry4.TI_RC = refContainer3.PK;

			collection = new PricingPageCollection(quote);
			collection.Load(PricingPaginationStrategy.LandscapeComplexStyle);

			pricingPage = collection.Cast<PricingPage>().Single();
			AssertEquals("Collection[0].Count", 2, pricingPage.RateEntries.Count());

			expected = new[] { refContainer1.RC_Code, refContainer2.RC_Code, refContainer3.RC_Code };
			actual = pricingPage.ContainerSet.Select(c => c.RC_Code);
			AssertContainsExactElementsInAnyOrder("Collection[0].ContainerList", expected, actual);
		}

		[TestDate(2008, 1, 10)]
		public void TestDuplicateFCLDifferentValidityNoFreightChargeCode()
		{
			var clientRate = Helper.NewClientRate(Consignee);

			var entry1 = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "40GP");
			entry1.TI_RateStartDate = new ZDate(2008, 1, 1);
			entry1.TI_RateEndDate = new ZDate(2008, 1, 31);
			entry1.RateLines.RemoveAndDeleteAll();
			entry1.AddRateLine("FRT", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 120m;

			var entry2 = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "40GP");
			entry2.TI_RateStartDate = new ZDate(2008, 2, 1);
			entry2.TI_RateEndDate = new ZDate(2008, 2, 28);
			entry2.RateLines.RemoveAndDeleteAll();
			entry2.AddRateLine("BAF", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 25;

			var collection = new PricingPageCollection(clientRate);
			collection.Load(PricingPaginationStrategy.LandscapeComplexStyle);

			AssertEquals("Count", 1, collection.Count);
			AssertEquals("Should contain both items - even if the 2nd item doesn't have an explicit FRT charge code", 2, collection[0].RateEntries.Count());
		}

		public void TestLoadStandardLoadsAllFreightEntries_SameFactory()
		{
			TestLoadStandardLoadsAllFreightEntries(false);
		}

		public void TestLoadStandardLoadsAllFreightEntries_NewFactory()
		{
			TestLoadStandardLoadsAllFreightEntries(true);
		}

		void TestLoadStandardLoadsAllFreightEntries(bool fromNewFactory)
		{
			var chargeCode1 = Helper.ChargeCodes.New("CCO1", "Origin Charge Code", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var chargeCode2 = Helper.ChargeCodes.New("FRT2", "Another Freight Charge Code", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);

			var quote = Helper.NewQuote(Consignee);
			var entry1 = quote.AddRateEntry("AIR", "LSE", "AU", "US", "STD", "");
			var entry2 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "");
			var entry3 = quote.AddRateEntry("ORG", "ALL", "AU", "US", "STD", "");

			entry1.RateLines.RemoveAndDeleteAll();
			entry2.RateLines.RemoveAndDeleteAll();
			entry3.RateLines.RemoveAndDeleteAll();

			entry1.AddRateLine(chargeCode2).GetCalculator<FlatCalculator>().BaseRate = 10m;
			entry2.AddRateLine(chargeCode2).GetCalculator<FlatCalculator>().BaseRate = 20m;
			entry3.AddRateLine(chargeCode1).GetCalculator<FlatCalculator>().BaseRate = 30m;

			Factory.Save();

			var quoteToPrint = fromNewFactory ? Helper.LoadInNewFactory(quote) : quote;
			var collection = new PricingPageCollection(quoteToPrint);

			collection.Load(PricingPaginationStrategy.StandardStyle);
			AssertEquals(2, collection.Count);

			var actualEntries = collection.Cast<PricingPage>().SelectMany(page => page.RateEntries).Select(e => e.DisplayInfo());
			var expectedEntries = new ZString[]
			{
				entry1.DisplayInfo(),
				entry2.DisplayInfo(),
			};

			var message = "Origin should be rolled up";
			AssertContainsExactElementsInAnyOrder(message, expectedEntries, actualEntries);
		}

		[TestDate(2010, 10, 10)]
		public void TestLoadStandardLoadsAllFreightEntries_ExcludingThoseThatShouldBeFilteredOut()
		{
			var frtChargeCode = Helper.ChargeCodes["WAR"];
			var dstChargeCode = Helper.ChargeCodes["DDOC"];
			AssertEquals("Precondition", FlatCalculator.Code, frtChargeCode.AC_RateCalculator);
			AssertEquals("Precondition", FlatCalculator.Code, dstChargeCode.AC_RateCalculator);

			var clientRate = Helper.NewClientRate(NewClient);

			var expiredLCLEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "GB");
			expiredLCLEntry.TI_RateStartDate = ZDate.Today.AddDays(-31);
			expiredLCLEntry.TI_RateEndDate = ZDate.Today.AddDays(-1);
			expiredLCLEntry.RateLines.RemoveAndDeleteAll();
			expiredLCLEntry.AddRateLine(frtChargeCode).GetCalculator<FlatCalculator>().BaseRate = 10m;

			var currentLCLEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "CN");
			currentLCLEntry.TI_RateStartDate = ZDate.Today;
			currentLCLEntry.TI_RateEndDate = ZDate.Empty;
			currentLCLEntry.RateLines.RemoveAndDeleteAll();
			currentLCLEntry.AddRateLine(frtChargeCode).GetCalculator<FlatCalculator>().BaseRate = 20m;

			var perthDSTEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "", "AUPER");
			perthDSTEntry.AddRateLine(dstChargeCode).GetCalculator<FlatCalculator>().BaseRate = 30m;

			var sydneyDSTEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "", "AUSYD");
			sydneyDSTEntry.AddRateLine(dstChargeCode).GetCalculator<FlatCalculator>().BaseRate = 40m;

			var sydneyDSTEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "CNSHA", "AUSYD");
			sydneyDSTEntry2.AddRateLine(dstChargeCode).GetCalculator<FlatCalculator>().BaseRate = 50m;
			clientRate.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedClientRate = newFactory.Load<ClientRate>(clientRate.PK);
			var locationFilter = new ZQuery(RateEntrySchema.TI_DestinationLRC, "AUSYD");
			var filter = new RateEntryFilterStripBusinessObjectForTest(locationFilter);
			reloadedClientRate.EntryCollections[RatingConstants.RateCategory.DST].LazyLoadingCollection.SetUserFilter(filter);

			var collection = new PricingPageCollection(reloadedClientRate);
			collection.Load(PricingPaginationStrategy.StandardStyle);

			var expected = new[] { currentLCLEntry.PK, sydneyDSTEntry.PK, sydneyDSTEntry2.PK };
			var actual = collection.Cast<PricingPage>().SelectMany(x => x.RateEntries).Select(x => x.PK).ToArray();
			CombineAssertions(() =>
			{
				Assert("Should be filted by the default date filter", !actual.Contains(expiredLCLEntry.PK));
				Assert("Should be filted by DST Location filter", !actual.Contains(perthDSTEntry.PK));
				AssertContainsExactElementsInAnyOrder(expected, actual);
			});
		}

		public void TestNoFreightEntries()
		{
			var quote = Helper.NewQuote(Consignee);

			var chinaEntry = quote.AddRateEntry("DST", "LCL", "CN", "AU");
			chinaEntry.AddRateLine("DDOC").GetCalculator<FlatCalculator>().BaseRate = 10m;

			var thaiEntry = quote.AddRateEntry("DST", "LCL", "TH", "AU");
			thaiEntry.AddRateLine("DDOC").GetCalculator<FlatCalculator>().BaseRate = 11m;

			var blankEntry = quote.AddRateEntry("DST", "LCL", "", "AU");
			blankEntry.AddRateLine("DDOC").GetCalculator<FlatCalculator>().BaseRate = 12m;

			var collection = new PricingPageCollection(quote);
			collection.Load(PricingPaginationStrategy.StandardStyle);

			AssertEquals(3, collection.Count);
		}

		public void TestMoreSpecificEntriesAreAddedToAPage()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry1.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 10m;

			var entry2 = rate.AddRateEntry("FCL", "SEA", "AUBNE", "USNAY");
			entry2.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 11m;

			var entry3 = rate.AddRateEntry("ORG", "FCL", "AUSYD", "");
			entry3.AddRateLine("ODOC", "FLT").GetCalculator<FlatCalculator>().BaseRate = 12m;

			var entry4 = rate.AddRateEntry("ORG", "FCL", "AUBNE", "");
			entry4.AddRateLine("ODOC", "FLT").GetCalculator<FlatCalculator>().BaseRate = 13m;

			var entry5 = rate.AddRateEntry("ORG", "FCL", "AU", "");
			entry5.AddRateLine("ODOC", "FLT").GetCalculator<FlatCalculator>().BaseRate = 14m;
			entry5.AddRateLine("OFUMI", "FLT").GetCalculator<FlatCalculator>().BaseRate = 15m;

			var entry6 = rate.AddRateEntry("DST", "FCL", "", "USLAX");
			entry6.AddRateLine("DDOC", "FLT").GetCalculator<FlatCalculator>().BaseRate = 16m;

			var entry7 = rate.AddRateEntry("DST", "FCL", "", "USNAY");
			entry7.AddRateLine("DDOC", "FLT").GetCalculator<FlatCalculator>().BaseRate = 17m;

			var entry8 = rate.AddRateEntry("DST", "FCL", "", "US");
			entry8.AddRateLine("DDOC", "FLT").GetCalculator<FlatCalculator>().BaseRate = 18m;
			entry8.AddRateLine("DFUMI", "FLT").GetCalculator<FlatCalculator>().BaseRate = 19m;

			Factory.Save();

			var collection = new PricingPageCollection(rate);
			collection.Load(PricingPaginationStrategy.StandardStyle);

			var message = "Expected the most specific option that covers all Freight Rates";
			var actualEntries = collection.Cast<PricingPage>().SelectMany(page => page.RateEntries).Select(e => e.DisplayInfo());
			var expectedEntries = new ZString[]
			{
				entry1.DisplayInfo(),
				entry2.DisplayInfo(),
				entry5.DisplayInfo(),
				entry8.DisplayInfo(),
			};

			AssertEquals(4, collection.Count);
			AssertContainsExactElementsInAnyOrder(message, expectedEntries, actualEntries);
		}

		public void TestOriginAndDestinationEntriesSameAsAFreightEntryButIncludeViaPort()
		{
			var rate = Helper.NewClientRate(Consignee);

			var entry1 = rate.AddRateEntry("AIR", "LSE", "DE", "US");
			entry1.RateLines.RemoveAndDeleteAll();
			entry1.AddRateLine("FRT", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 100m;

			var entry2 = rate.AddRateEntry("ORG", "LSE", "DE", "US");
			entry2.TI_ViaLRC = "ADALV";
			entry2.AddRateLine("ODOC").GetCalculator<FlatCalculator>().BaseRate = 200m;

			var entry3 = rate.AddRateEntry("DST", "LSE", "DE", "US");
			entry3.TI_ViaLRC = "ADALV";
			entry3.AddRateLine("DDOC").GetCalculator<FlatCalculator>().BaseRate = 300m;

			Factory.Save();

			var collection = new PricingPageCollection(rate);
			collection.Load(PricingPaginationStrategy.LandscapeSimpleStyle);

			var actualEntries = collection.Cast<PricingPage>().SelectMany(page => page.RateEntries).Select(e => e.DisplayInfo());
			var expectedEntries = new ZString[]
			{
				entry1.DisplayInfo(),
				entry2.DisplayInfo(),
				entry3.DisplayInfo()
			};

			var message = "Air Freight Entry with no via should still include ORG/DST rates with one if otherwise matching";
			AssertEquals(message, 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(message, expectedEntries, actualEntries);
		}

		public void TestPricingPagesDoNotMergeEntriesWithDifferentFromAndToSuburbs()
		{
			var chargeCode = Helper.ChargeCodes.New("TBCSTD", "Transport Standard", FlatCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			var cityTown1 = Factory.NewWithValidTestData<RefCityTown>();
			var cityTown2 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown1.R9_RN_NKCountry = CountryCodes.Australia;
			cityTown2.R9_RN_NKCountry = CountryCodes.Australia;
			Factory.Save();

			var quotation = Helper.NewQuote(NewClient);
			var category = RatingConstants.RateCategory.TBC;
			var quoteEntry1 = quotation.AddRateEntryWithFlatRateLine(category, RateMode.FRO, "AU", "", "TBCSTD", 10m);
			quoteEntry1.OriginSuburbPK = cityTown1.PK;

			var quoteEntry2 = quotation.AddRateEntryWithFlatRateLine(category, RateMode.FRO, "AU", "", "TBCSTD", 20m);
			quoteEntry2.DestinationSuburbPK = cityTown2.PK;

			var quoteEntry3 = quotation.AddRateEntryWithFlatRateLine(category, RateMode.FRO, "AU", "", "TBCSTD", 30m);

			Factory.Save();

			var collection = new PricingPageCollection(quotation);
			collection.Load(PricingPaginationStrategy.LandscapeComplexStyle);

			var actualEntries = collection.Cast<PricingPage>().SelectMany(page => page.RateEntries).Select(e => e.DisplayInfo());
			var expectedEntries = new ZString[]
			{
				quoteEntry1.DisplayInfo(),
				quoteEntry2.DisplayInfo(),
				quoteEntry3.DisplayInfo()
			};

			var message = "All quote entries should be included in the pricing page's entries.";
			AssertEquals(message, 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(message, expectedEntries, actualEntries);
		}

		public void TestViewAgentRatesSetByPricingPaginationStrategy()
		{
			var quote = Helper.NewQuote(Consignee);
			var line1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "IN", "CNSHA", "FRT", 100).RateLines[0];
			var line2 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "IN", "AUSYD", "FRT", 180).RateLines[0];
			var line3 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LCL, "IN", "AUSYD", "ODOC", 20).RateLines[0];
			var line4 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LCL, "IN", "AUSYD", "DDOC", 20).RateLines[0];
			line1.ViewAgentRates = true;
			line2.ViewAgentRates = true;
			line3.ViewAgentRates = true;
			line4.ViewAgentRates = true;
			line1.GetCalculator<FlatCalculator>().BaseRate = 90;
			line2.GetCalculator<FlatCalculator>().BaseRate = 90;
			line3.GetCalculator<FlatCalculator>().BaseRate = 90;
			line4.GetCalculator<FlatCalculator>().BaseRate = 90;

			var collection = new PricingPageCollection(quote);
			var strategy = PricingPaginationStrategy.StandardStyle;

			AssertPageViewAgentRates(collection, strategy, false);

			strategy = PricingPaginationStrategy.StandardStyle | PricingPaginationStrategy.IsAgentPricingPage;

			AssertPageViewAgentRates(collection, strategy, true);

			strategy = PricingPaginationStrategy.LandscapeCompactStyle;

			AssertPageViewAgentRates(collection, strategy, false);

			strategy = PricingPaginationStrategy.LandscapeCompactStyle | PricingPaginationStrategy.IsAgentPricingPage;

			AssertPageViewAgentRates(collection, strategy, true);
		}

		void AssertPageViewAgentRates(PricingPageCollection collection, PricingPaginationStrategy strategy, bool expected)
		{
			collection.Load(strategy);
			Assert("Pre-condition: expected collection to produce results", collection.Any());

			foreach (PricingPage page in collection)
			{
				AssertEquals("PricingPage ViewAgentRates flag should be set by strategy", expected, page.ViewAgentRates);
			}
		}
	}

	//Do not add new tests in this style, they're a pain to maintain.
	internal sealed class PricingPageCollectionRenderingTest : QuoteFormatCollectionsTestCase
	{
		public void TestLoadLandscapeCompact()
		{
			var quote = SetupQuote();

			var collection = new PricingPageCollection(quote);
			collection.Load(PricingPaginationStrategy.LandscapeCompactStyle);

			AssertMultilineASCIIEquals("", ExpectedCompact, DisplayProvider(collection));
		}

		public void TestLoadLandscapeSimple()
		{
			var quote = SetupQuote();

			var collection = new PricingPageCollection(quote);
			collection.Load(PricingPaginationStrategy.LandscapeSimpleStyle);

			AssertMultilineASCIIEquals("", ExpectedLandscapeSimple, DisplayProvider(collection));
		}

		public void TestLoadLandscapeComplex()
		{
			var quote = SetupQuote();

			var collection = new PricingPageCollection(quote);
			collection.Load(PricingPaginationStrategy.LandscapeComplexStyle);

			AssertMultilineASCIIEquals("", ExpectedLandscapeComplex, DisplayProvider(collection));
		}

		public void TestLoadStandard()
		{
			var quote = SetupQuote();

			var collection = new PricingPageCollection(quote);
			collection.Load(PricingPaginationStrategy.StandardStyle);

			AssertMultilineASCIIEquals("", ExpectedStandard, DisplayProvider(collection));
		}

		#region Implementation

		#region Expected Landscape Complex

		const string ExpectedLandscapeComplex = @"
C: 20GP
C: 40GP
C: 40HC
E: FCL-SEA-STD, CNCAN => AUSYD
E: FCL-SEA-STD, CNSHA => AUSYD
E: FCL-SEA-STD, GBLON => AUSYD
E: FCL-SEA-STD, GBLON => AUSYD
E: FCL-SEA-STD, NLRTM => AUSYD
E: FCL-SEA-STD, TWKEL => AUSYD
E: LCL-LCL-STD, CNCAN => AUSYD
E: LCL-LCL-STD, CNSHA => AUSYD
E: LCL-LCL-STD, TWKEL => AUSYD

C: 20GP
C: 40GP
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON

C: 20GP
C: 40GP
E: SCO-SEA-STD, AUSYD => GBLON
E: SCO-SEA-STD, AUSYD => NLAMS
E: SNC-LCL-STD, AUSYD => GBLON

C: 40GP
E: FCL-SEA-STD, CNCAN => AUSYD
E: FCL-SEA-STD, DKCPH => AUSYD
E: FCL-SEA-STD, GBLON => AUSYD

C: AIR1
C: AIR2
C: AIR3
E: AIR-ULD-STD, AUSYD => USLAX
E: AIR-ULD-STD, AUSYD => USMEM

C: TTRK
E: LCL-FTL-STD, AUSYD => AUMEL

E: AIR-LSE-STD, CHZRH => AUSYD
E: AIR-LSE-STD, FIHEL => AUSYD
E: AIR-LSE-STD, USNYC => AUSYD

E: LCL-LCL-D2D, USNYC => AUSYD

E: ORG-AIR-STD, USLAX => AU
E: ORG-AIR-STD, USLAX => AUSYD

E: SNC-LCL-STD, NLAMS => AUSYD

";

		#endregion

		#region Expected Landscape Simple

		const string ExpectedLandscapeSimple = @"
C: 20GP
C: 40GP
C: 40HC
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, CNCAN => AUSYD
E: FCL-SEA-STD, CNCAN => AUSYD
E: FCL-SEA-STD, CNSHA => AUSYD
E: FCL-SEA-STD, DKCPH => AUSYD
E: FCL-SEA-STD, GBLON => AUSYD
E: FCL-SEA-STD, GBLON => AUSYD
E: FCL-SEA-STD, GBLON => AUSYD
E: FCL-SEA-STD, NLRTM => AUSYD
E: FCL-SEA-STD, TWKEL => AUSYD
E: LCL-LCL-D2D, USNYC => AUSYD
E: LCL-LCL-STD, CNCAN => AUSYD
E: LCL-LCL-STD, CNSHA => AUSYD
E: LCL-LCL-STD, TWKEL => AUSYD

C: 20GP
C: 40GP
E: SCO-SEA-STD, AUSYD => GBLON
E: SCO-SEA-STD, AUSYD => NLAMS
E: SNC-LCL-STD, AUSYD => GBLON
E: SNC-LCL-STD, NLAMS => AUSYD

C: AIR1
C: AIR2
C: AIR3
E: AIR-ULD-STD, AUSYD => USLAX
E: AIR-ULD-STD, AUSYD => USMEM

C: TTRK
E: LCL-FTL-STD, AUSYD => AUMEL

E: AIR-LSE-STD, CHZRH => AUSYD
E: AIR-LSE-STD, FIHEL => AUSYD
E: AIR-LSE-STD, USNYC => AUSYD
E: ORG-AIR-STD, USLAX => AU
E: ORG-AIR-STD, USLAX => AUSYD

";

		#endregion

		#region Expected Standard

		const string ExpectedStandard = @"
C: 20GP
C: 40GP
C: 40HC
E: FCL-SEA-STD, CNSHA => AUSYD

C: 20GP
C: 40GP
E: FCL-SEA-STD, AUSYD => GBLON

C: 20GP
C: 40GP
E: FCL-SEA-STD, GBLON => AUSYD

C: 20GP
C: 40GP
E: SCO-SEA-STD, AUSYD => NLAMS

C: 20GP
C: 40HC
E: FCL-SEA-STD, CNCAN => AUSYD

C: 20GP
E: FCL-SEA-STD, AUSYD => GBLON

C: 20GP
E: FCL-SEA-STD, AUSYD => GBLON

C: 20GP
E: FCL-SEA-STD, AUSYD => GBLON

C: 20GP
E: FCL-SEA-STD, TWKEL => AUSYD

C: 20GP
E: SCO-SEA-STD, AUSYD => GBLON

C: 40GP
E: FCL-SEA-STD, AUSYD => GBLON

C: 40GP
E: FCL-SEA-STD, AUSYD => GBLON

C: 40GP
E: FCL-SEA-STD, AUSYD => GBLON

C: 40GP
E: FCL-SEA-STD, AUSYD => GBLON

C: 40GP
E: FCL-SEA-STD, CNCAN => AUSYD

C: 40GP
E: FCL-SEA-STD, DKCPH => AUSYD

C: 40GP
E: FCL-SEA-STD, GBLON => AUSYD

C: 40HC
E: FCL-SEA-STD, GBLON => AUSYD

C: 40HC
E: FCL-SEA-STD, NLRTM => AUSYD

C: AIR1
C: AIR3
E: AIR-ULD-STD, AUSYD => USLAX

C: AIR2
E: AIR-ULD-STD, AUSYD => USMEM

C: TTRK
E: LCL-FTL-STD, AUSYD => AUMEL

E: AIR-LSE-STD, CHZRH => AUSYD

E: AIR-LSE-STD, FIHEL => AUSYD

E: AIR-LSE-STD, USNYC => AUSYD

E: LCL-LCL-D2D, USNYC => AUSYD

E: LCL-LCL-STD, CNCAN => AUSYD

E: LCL-LCL-STD, CNSHA => AUSYD

E: LCL-LCL-STD, TWKEL => AUSYD

E: ORG-AIR-STD, USLAX => AU

E: ORG-AIR-STD, USLAX => AUSYD

E: SNC-LCL-STD, AUSYD => GBLON

E: SNC-LCL-STD, NLAMS => AUSYD

";

		#endregion

		#region Expected Compact

		const string ExpectedCompact = @"
C: 20GP
C: 40GP
C: 40HC
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, AUSYD => GBLON
E: FCL-SEA-STD, CNCAN => AUSYD
E: FCL-SEA-STD, CNSHA => AUSYD
E: FCL-SEA-STD, NLRTM => AUSYD
E: FCL-SEA-STD, TWKEL => AUSYD
E: LCL-LCL-STD, CNCAN => AUSYD
E: LCL-LCL-STD, CNSHA => AUSYD
E: LCL-LCL-STD, TWKEL => AUSYD

C: 20GP
C: 40GP
E: FCL-SEA-STD, GBLON => AUSYD

C: 20GP
C: 40GP
E: SCO-SEA-STD, AUSYD => GBLON
E: SCO-SEA-STD, AUSYD => NLAMS
E: SNC-LCL-STD, AUSYD => GBLON
E: SNC-LCL-STD, NLAMS => AUSYD

C: 40GP
E: FCL-SEA-STD, CNCAN => AUSYD
E: FCL-SEA-STD, DKCPH => AUSYD

C: 40GP
E: FCL-SEA-STD, GBLON => AUSYD

C: 40HC
E: FCL-SEA-STD, GBLON => AUSYD

C: AIR1
C: AIR2
C: AIR3
E: AIR-ULD-STD, AUSYD => USLAX
E: AIR-ULD-STD, AUSYD => USMEM

C: TTRK
E: LCL-FTL-STD, AUSYD => AUMEL

E: AIR-LSE-STD, CHZRH => AUSYD
E: AIR-LSE-STD, FIHEL => AUSYD
E: AIR-LSE-STD, USNYC => AUSYD
E: ORG-AIR-STD, USLAX => AU
E: ORG-AIR-STD, USLAX => AUSYD

E: LCL-LCL-D2D, USNYC => AUSYD
";

		#endregion

		static string DisplayProvider(PricingPage page)
		{
			var lines = new List<string>();

			foreach (var entry in page.RateEntries)
			{
				lines.Add($"E: {entry.TI_RateCategory}-{entry.TI_Mode}-{entry.TI_RS_NKServiceLevel_NI}, {entry.TI_OriginLRC} => {entry.TI_DestinationLRC}");
			}

			foreach (var container in page.ContainerSet)
			{
				lines.Add($"C: {container.RC_Code}");
			}

			lines.Sort();

			var builder = new StringBuilder();

			foreach (var line in lines)
			{
				builder.AppendLine(line);
			}

			return builder.ToString();
		}

		static string DisplayProvider(PricingPageCollection collection)
		{
			var lines = new List<string>();

			foreach (PricingPage page in collection)
			{
				lines.Add(DisplayProvider(page));
			}

			lines.Sort();

			var builder = new StringBuilder();
			builder.AppendLine();

			foreach (var line in lines)
			{
				builder.AppendLine(line);
			}

			return builder.ToString();
		}

		#endregion
	}

	[TestedType(typeof(PricingPageCollection))]
	internal sealed class PricingPageBizObjCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PricingPageCollection>
	{
		#region Implementation

		protected override PricingPageCollection GetCollectionToTest()
		{
			return new PricingPageCollection(Factory.New<Quote>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry(RatingConstants.RateCategory.AIR);

			return new PricingPage(entry, Factory, PricingPageStyle.Landscape);
		}

		#endregion
	}
}
