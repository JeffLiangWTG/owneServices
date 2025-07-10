using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Rating.Business.RatingConstants;

namespace Enterprise.Rating.Business.Testing
{
	class RelatedRateEntriesLoaderTest : RatingTestCase
	{
		#region RelatedRateEntriesLoader does not hit database excessively

		public void TestRelatedRateEntriesLoader_QuotationDoesNotCallDBExcessively()
		{
			#region Set up

			var zone1 = Helper.NewInternationalZone("CNZ1", null, "CNSHA", "CNCQI");
			var zone2 = Helper.NewInternationalZone("CNZ2", TransportProvider1, "CNSHA", "CNCQI");

			var quotation = Factory.NewWithValidTestData<Quote>();
			var mode = Constants.RateMode.LSE;
			var freightCategory = RatingConstants.RateCategory.AIR;
			quotation.AddRateEntryWithFlatRateLine(freightCategory, mode, "AU", "CNZ1", "FRT", 110);
			quotation.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUSYD", "CNZ1", "FRT", 120);
			quotation.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUMEL", "CNZ1", "FRT", 130);
			quotation.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUCAN", "CNZ1", "FRT", 140);
			quotation.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUPER", "CNZ1", "FRT", 150);

			quotation.AddRateEntryWithFlatRateLine(freightCategory, mode, "AU", "CNZ2", "FRT", 210);
			quotation.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUSYD", "CNZ2", "FRT", 220);
			quotation.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUMEL", "CNZ2", "FRT", 230);
			quotation.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUCAN", "CNZ2", "FRT", 240);
			quotation.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUPER", "CNZ2", "FRT", 250);

			var originCategory = RatingConstants.RateCategory.ORG;
			quotation.AddRateEntryWithFlatRateLine(originCategory, mode, "AU", "CNZ1", "OAQF", 480);
			quotation.AddRateEntryWithFlatRateLine(originCategory, mode, "AU", "CNZ2", "OAQF", 490);
			quotation.AddRateEntryWithFlatRateLine(originCategory, mode, "AUSYD", "CNSHA", "ODOC", 10);
			quotation.AddRateEntryWithFlatRateLine(originCategory, mode, "AUSYD", "CNCQI", "ODOC", 20);
			quotation.AddRateEntryWithFlatRateLine(originCategory, mode, "AUMEL", "CNSHA", "ODOC", 12);
			quotation.AddRateEntryWithFlatRateLine(originCategory, mode, "AUMEL", "CNCQI", "ODOC", 22);
			quotation.AddRateEntryWithFlatRateLine(originCategory, mode, "AUCAN", "CNSHA", "ODOC", 13);
			quotation.AddRateEntryWithFlatRateLine(originCategory, mode, "AUCAN", "CNCQI", "ODOC", 23);
			quotation.AddRateEntryWithFlatRateLine(originCategory, mode, "AUPER", "CNSHA", "ODOC", 14);
			quotation.AddRateEntryWithFlatRateLine(originCategory, mode, "AUPER", "CNCQI", "ODOC", 24);

			var destinationCategory = RatingConstants.RateCategory.DST;
			quotation.AddRateEntryWithFlatRateLine(destinationCategory, mode, "", "CNZ1", "DDOC", 10);
			quotation.AddRateEntryWithFlatRateLine(destinationCategory, mode, "AU", "CNZ1", "DDOC", 20);
			quotation.AddRateEntryWithFlatRateLine(destinationCategory, mode, "", "CNZ2", "DDOC", 12);
			quotation.AddRateEntryWithFlatRateLine(destinationCategory, mode, "AU", "CNZ2", "DDOC", 22);

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var reloadedQuotation = newFactory.Load<Quote>(quotation.PK);
			reloadedQuotation.ReloadCollectionsWithNoFilter();
			var reloadedEntries = reloadedQuotation.AllEntries.ToArray();

			#endregion

			//Calling AllEntries will load locations into the factory but since this is a unit test
			//we're only going to look at the calls from GetRelatedEntries
			newFactory.ResetDatabaseLoadCount();

			var pricingPage = new PricingPage(reloadedEntries[0], newFactory, PricingPageStyle.Landscape);
			var loader = new RelatedRateEntriesLoader(pricingPage, newFactory);
			foreach (var entry in reloadedEntries)
			{
				loader.GetRelatedEntries(entry);
			}

			var tableHitDictionary = new Dictionary<string, int>
			{
				{ RateEntrySchema.Constants.TableName, 29 },
				{ RefZonePivotSchema.Constants.TableName, 10 },
				{ RefZoneHeaderSchema.Constants.TableName, 10 },
				{ RefUNLOCOSchema.Constants.TableName, 3 },
				{ RatingHeaderSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgRateTariffLevelSchema.Constants.TableName, 1 },
			};

			AssertDbHits(tableHitDictionary, newFactory);
		}

		public void TestRelatedEntriesLoader_TariffDoesNotCallDBExcessively()
		{
			#region Set up

			var zone1 = Helper.NewInternationalZone("CNZ1", null, "CNSHA", "CNCQI");
			var zone2 = Helper.NewInternationalZone("CNZ2", TransportProvider1, "CNSHA", "CNCQI");

			var tariff = Factory.New<CompanyTariff>();
			var mode = Constants.RateMode.LSE;
			var freightCategory = RatingConstants.RateCategory.AIR;
			tariff.AddRateEntryWithFlatRateLine(freightCategory, mode, "AU", "CNZ1", "FRT", 110);
			tariff.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUSYD", "CNZ1", "FRT", 120);
			tariff.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUMEL", "CNZ1", "FRT", 130);
			tariff.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUCAN", "CNZ1", "FRT", 140);
			tariff.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUPER", "CNZ1", "FRT", 150);

			tariff.AddRateEntryWithFlatRateLine(freightCategory, mode, "AU", "CNZ2", "FRT", 210);
			tariff.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUSYD", "CNZ2", "FRT", 220);
			tariff.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUMEL", "CNZ2", "FRT", 230);
			tariff.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUCAN", "CNZ2", "FRT", 240);
			tariff.AddRateEntryWithFlatRateLine(freightCategory, mode, "AUPER", "CNZ2", "FRT", 250);

			var originCategory = RatingConstants.RateCategory.ORG;
			tariff.AddRateEntryWithFlatRateLine(originCategory, mode, "AU", "CNZ1", "OAQF", 480);
			tariff.AddRateEntryWithFlatRateLine(originCategory, mode, "AU", "CNZ2", "OAQF", 490);
			tariff.AddRateEntryWithFlatRateLine(originCategory, mode, "AUSYD", "CNSHA", "ODOC", 10);
			tariff.AddRateEntryWithFlatRateLine(originCategory, mode, "AUSYD", "CNCQI", "ODOC", 20);
			tariff.AddRateEntryWithFlatRateLine(originCategory, mode, "AUMEL", "CNSHA", "ODOC", 12);
			tariff.AddRateEntryWithFlatRateLine(originCategory, mode, "AUMEL", "CNCQI", "ODOC", 22);
			tariff.AddRateEntryWithFlatRateLine(originCategory, mode, "AUCAN", "CNSHA", "ODOC", 13);
			tariff.AddRateEntryWithFlatRateLine(originCategory, mode, "AUCAN", "CNCQI", "ODOC", 23);
			tariff.AddRateEntryWithFlatRateLine(originCategory, mode, "AUPER", "CNSHA", "ODOC", 14);
			tariff.AddRateEntryWithFlatRateLine(originCategory, mode, "AUPER", "CNCQI", "ODOC", 24);

			var destinationCategory = RatingConstants.RateCategory.DST;
			tariff.AddRateEntryWithFlatRateLine(destinationCategory, mode, "", "CNZ1", "DDOC", 10);
			tariff.AddRateEntryWithFlatRateLine(destinationCategory, mode, "AU", "CNZ1", "DDOC", 20);
			tariff.AddRateEntryWithFlatRateLine(destinationCategory, mode, "", "CNZ2", "DDOC", 12);
			tariff.AddRateEntryWithFlatRateLine(destinationCategory, mode, "AU", "CNZ2", "DDOC", 22);

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var reloadedTariff = newFactory.Load<CompanyTariff>(tariff.PK);
			reloadedTariff.ReloadCollectionsWithNoFilter();
			var reloadedEntries = reloadedTariff.AllEntries.ToArray();

			#endregion

			//Calling AllEntries will load locations into the factory but since this is a unit test
			//we're only going to look at the calls from GetRelatedEntries
			newFactory.ResetDatabaseLoadCount();

			var pricingPage = new PricingPage(reloadedEntries[0], newFactory, PricingPageStyle.Landscape);
			var loader = new RelatedRateEntriesLoader(pricingPage, newFactory);
			foreach (var entry in reloadedEntries)
			{
				loader.GetRelatedEntries(entry);
			}

			var tableHitDictionary = new Dictionary<string, int>
			{
				{ RateEntrySchema.Constants.TableName, 39 },
				{ RefZonePivotSchema.Constants.TableName, 10 },
				{ RefZoneHeaderSchema.Constants.TableName, 10 },
				{ RefUNLOCOSchema.Constants.TableName, 3 },
			};

			AssertDbHits(tableHitDictionary, newFactory);
		}

		#endregion

		public void TestDistinguishingColumnsForDocumentGrouping_ListIsUpdatedIfNewRelevantColumnsAreAddedToRateEntry()
		{
			var columnsFilteredByDistinguishingColumnsForDocumentGrouping = PricingPageRateLineFactory.DistinguishingColumnsForDocumentGrouping.Select(c => c.Name).ToArray();

			var columnsFilteredByRelatedEntryLoaderFilters = new string[]
			{
				RateEntrySchema.Constants.TI_TH,							//RelatedEntryLoader.GetHeaders
				RateEntrySchema.Constants.TI_Mode,							//RelatedEntryLoader.GetOriginDestinationModeExclusiveFilter
				RateEntrySchema.Constants.TI_OriginLRC,						//RelatedEntryLoader.BuildLocationFilter
				RateEntrySchema.Constants.TI_RateOrigin,					//RelatedEntryLoader.BuildLocationFilter
				RateEntrySchema.Constants.TI_ViaLRC,						//RelatedEntryLoader.BuildLocationFilter
				RateEntrySchema.Constants.TI_DestinationLRC,				//RelatedEntryLoader.BuildLocationFilter
				RateEntrySchema.Constants.TI_RateDestination,				//RelatedEntryLoader.BuildLocationFilter
				RateEntrySchema.Constants.TI_AircraftType,					//RelatedEntryLoader.BuildLocationFilter
				RateEntrySchema.Constants.TI_PlannedLoadLRC,				//RelatedEntryLoader.BuildLocationFilter
				RateEntrySchema.Constants.TI_PlannedDischargeLRC,			//RelatedEntryLoader.BuildLocationFilter
				RateEntrySchema.Constants.TI_FirstLoadLRC,					//RelatedEntryLoader.BuildLocationFilter
				RateEntrySchema.Constants.TI_LastDischargeLRC,				//RelatedEntryLoader.BuildLocationFilter
				RateEntrySchema.Constants.TI_FirstRouteSetLoadPortLRC,		//RelatedEntryLoader.BuildLocationFilter
				RateEntrySchema.Constants.TI_LastRouteSetDischargePortLRC,	//RelatedEntryLoader.BuildLocationFilter
				RateEntrySchema.Constants.TI_IsCrossTrade,					//RelatedEntryLoader.BuildLocationFilter
				RateEntrySchema.Constants.TI_OH_Supplier,					//RelatedEntryLoader.BuildServiceProviderFilter
				RateEntrySchema.Constants.TI_RC,							//RelatedEntryLoader.BuildContainerFilter
				RateEntrySchema.Constants.TI_MatchContainerRateClass,		//RelatedEntryLoader.BuildContainerFilter
				RateEntrySchema.Constants.TI_IsNonOperatedReefer,			//RelatedEntryLoader.BuildContainerFilter
				RateEntrySchema.Constants.TI_RateEndDate,					//RelatedEntryLoader.BuildDateFilter
				RateEntrySchema.Constants.TI_RateStartDate,					//RelatedEntryLoader.BuildDateFilter
			};

			var columnsNotRelevantForDocumentGrouping = new string[]
			{
				RateEntrySchema.Constants.PK,
				RateEntrySchema.Constants.TI_ParentTableCode,
				RateEntrySchema.Constants.TI_LineOrder,
				RateEntrySchema.Constants.TI_IsTact,
				RateEntrySchema.Constants.TI_IsValid,
				RateEntrySchema.Constants.TI_DataChecked,
				RateEntrySchema.Constants.TI_SystemCreateTimeUtc,
				RateEntrySchema.Constants.TI_SystemCreateUser,
				RateEntrySchema.Constants.TI_SystemLastEditTimeUtc,
				RateEntrySchema.Constants.TI_SystemLastEditUser,

				RateEntrySchema.Constants.TI_PageHeading,
				RateEntrySchema.Constants.TI_PageOpeningText,
				RateEntrySchema.Constants.TI_PageClosingText,

				RateEntrySchema.Constants.TI_RateCategory,
				RateEntrySchema.Constants.TI_RX_NKCurrency,
				RateEntrySchema.Constants.TI_ContractNumberLinked,
				RateEntrySchema.Constants.TI_Frequency,
				RateEntrySchema.Constants.TI_FrequencyUnit,
				RateEntrySchema.Constants.TI_OH_AgentOverride,
				RateEntrySchema.Constants.TI_BuyersConsolRateMode,
				RateEntrySchema.Constants.TI_QuotePageIncoTerm,
				RateEntrySchema.Constants.TI_GC_Publisher,
				RateEntrySchema.Constants.TI_GatewayAgentType,
				RateEntrySchema.Constants.TI_RS_NKGatewayServiceLevel,
				RateEntrySchema.Constants.TI_RCC_ComponentCode,
				RateEntrySchema.Constants.TI_RMC_Material,
				RateEntrySchema.Constants.TI_RRC_RepairCode,
				RateEntrySchema.Constants.TI_ContainerUnitSection,
				RateEntrySchema.Constants.TI_EstimateType,
				RateEntrySchema.Constants.TI_REG_EquipmentGrade,
				RateEntrySchema.Constants.TI_MNRGroup,
				RateEntrySchema.Constants.TI_RS_NKShipmentGatewayServiceLevel,
				RateEntrySchema.Constants.TI_IsExcludedFromAutoRating,
				RateEntrySchema.Constants.TI_ShipmentConsolidationStatus,
				RateEntrySchema.Constants.TI_CreationSource,
				RateEntrySchema.Constants.TI_FMCTariffID,

				RateEntrySchema.Constants.TI_ContractNumber,
				RateEntrySchema.Constants.TI_PaymentTerm,
				RateEntrySchema.Constants.TI_HBLDeliveryMode,

				RateEntrySchema.Constants.TI_YardUnitType,
				RateEntrySchema.Constants.TI_YardUnitLoad,
				RateEntrySchema.Constants.TI_ProviderReferenceID,
			};

			var combinedResults = new List<string>();
			combinedResults.AddRange(columnsFilteredByDistinguishingColumnsForDocumentGrouping);
			combinedResults.AddRange(columnsFilteredByRelatedEntryLoaderFilters);
			combinedResults.AddRange(columnsNotRelevantForDocumentGrouping);

			var duplicateElementsInCombinedResults = combinedResults.GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
			AssertArrayEqualsByElements("Pre-condition", Array.Empty<string>(), duplicateElementsInCombinedResults);

			var rateEntryColumns = RateEntrySchema.All.Select(x => x.Name);

			var message = @"Hello future developer, you've added a new column to RateEntry.
Please confirm with product which of the three categories this new column falls in to for the purpose of Pricing Page Printing.
Generally, if it something that will be added to RateEntry.DuplicateSearchColumns, then it should be added to
PricingPageLineSetFactory.DistinguishingColumnsForDocumentGrouping";
			AssertContainsExactElementsInAnyOrder(message, rateEntryColumns, combinedResults.ToArray());
		}

		public void TestPricingPagesDoNotConsiderEntriesWithDifferentFromAndToSuburbsAsRelated()
		{
			var chargeCode = Helper.ChargeCodes.New("TBCSTD", "Transport Standard", FlatCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			var cityTown1 = Factory.NewWithValidTestData<RefCityTown>();
			var cityTown2 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown1.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			cityTown2.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			Factory.Save();

			var quotation = Helper.NewQuote(NewClient);
			var category = RatingConstants.RateCategory.TBC;
			var quoteEntry1 = quotation.AddRateEntryWithFlatRateLine(category, Constants.RateMode.FRO, "AU", "", "TBCSTD", 10m);
			quoteEntry1.OriginSuburbPK = cityTown1.PK;

			var quoteEntry2 = quotation.AddRateEntryWithFlatRateLine(category, Constants.RateMode.FRO, "AU", "", "TBCSTD", 20m);
			quoteEntry2.DestinationSuburbPK = cityTown2.PK;

			var quoteEntry3 = quotation.AddRateEntryWithFlatRateLine(category, Constants.RateMode.FRO, "AU", "", "TBCSTD", 30m);

			Factory.Save();

			var collection = new PricingPageCollection(quotation);
			collection.Load(PricingPaginationStrategy.LandscapeComplexStyle);

			var pricingPage = collection.Cast<PricingPage>().SingleOrDefault();
			AssertNotNull("Pre-condition: there should be a pricing page for the quote entries");

			var relatedEntriesLoader = new RelatedRateEntriesLoader(pricingPage, Factory);
			var results = relatedEntriesLoader.GetRelatedEntries(quoteEntry1);
			var message = "Because these entries have different from and to suburbs they're not related rates. Quote Entry 3 be included because it is less specific than quoteEntry1";
			AssertContainsExactElementsInAnyOrder(message, new[] { quoteEntry1.PK, quoteEntry3.PK }, results.OriginRateEntries.Select(e => e.PK));
		}

		public void TestLoadLandscapeSimpleWithFreightAndDestinationCharges()
		{
			var tariff = Factory.New<CompanyTariff>();
			var airFreightTariffEntry1 = tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "", "AUSYD", "FRT", 110m);
			var airFreightTariffEntry2 = tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "", "GBLON", "FRT", 100m);
			var destinationTariffEntry1 = tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.LSE, "", "AUSYD", "DDOC", 180m);
			var destinationTariffEntry2 = tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.LSE, "", "GBLON", "DDOC", 250m);

			Factory.Save();

			var page1 = new PricingPage(airFreightTariffEntry1, Factory, PricingPageStyle.Landscape);
			var loader1 = new RelatedRateEntriesLoader(page1, Factory);

			var results1 = loader1.GetRelatedEntries(airFreightTariffEntry1);

			AssertEquals("correct amount of related destination entries", 1, results1.DestinationRateEntries.Count);
			AssertEquals("related destination entry is the correct entry", destinationTariffEntry1.PK, results1.DestinationRateEntries[0].PK);

			var page2 = new PricingPage(airFreightTariffEntry2, Factory, PricingPageStyle.Landscape);
			var loader2 = new RelatedRateEntriesLoader(page2, Factory);

			var results2 = loader2.GetRelatedEntries(airFreightTariffEntry2);

			AssertEquals("correct amount of related destination entries", 1, results2.DestinationRateEntries.Count);
			AssertEquals("related destination entry is the correct entry", destinationTariffEntry2.PK, results2.DestinationRateEntries[0].PK);
		}

		public void TestLoadLandscapeSimpleWithFreightAndDestinationChargesFiltered()
		{
			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			var airFreightTariffEntry1 = tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "", "AUSYD", "FRT", 110m);
			var airFreightTariffEntry2 = tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "", "GBLON", "FRT", 100m);
			var destinationTariffEntry1 = tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.LSE, "", "AUSYD", "DDOC", 180m);
			var destinationTariffEntry2 = tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.LSE, "", "GBLON", "DDOC", 250m);

			var locationFilter = new ZQuery(RateEntrySchema.TI_DestinationLRC, "AUSYD");
			var filter = new RateEntryFilterStripBusinessObjectForTest(locationFilter);
			tariff.EntryCollections[RatingConstants.RateCategory.DST].LazyLoadingCollection.SetUserFilter(filter);

			Factory.Save();

			var page1 = new PricingPage(airFreightTariffEntry1, Factory, PricingPageStyle.Landscape);
			var loader1 = new RelatedRateEntriesLoader(page1, Factory);

			var results1 = loader1.GetRelatedEntries(airFreightTariffEntry1);

			AssertEquals("correct amount of related destination entries", 1, results1.DestinationRateEntries.Count);
			AssertEquals("related destination entry is the correct entry", destinationTariffEntry1.PK, results1.DestinationRateEntries[0].PK);

			var page2 = new PricingPage(airFreightTariffEntry2, Factory, PricingPageStyle.Landscape);
			var loader2 = new RelatedRateEntriesLoader(page2, Factory);

			var results2 = loader2.GetRelatedEntries(airFreightTariffEntry2);

			AssertEquals("no related entries because of filter", 0, results2.DestinationRateEntries.Count);
		}

		[TestDate(2010, 10, 10)]
		public void TestLoadLandscapeSimpleWithFreightAndDestinationCharges_DefaultUserFiltersExpired()
		{
			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			var freightEntry = tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "", "AUSYD", "FRT", 110m);
			var expiredEntry = tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.LSE, "", "AUSYD", "DDOC", 10m);
			expiredEntry.TI_RateStartDate = new ZDate(2010, 01, 01);
			expiredEntry.TI_RateEndDate = new ZDate(2010, 10, 05);

			var currentEntry = tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.LSE, "", "AUSYD", "DDOC", 20m);
			currentEntry.TI_RateStartDate = new ZDate(2010, 10, 06);
			currentEntry.TI_RateEndDate = ZDate.Empty;
			Factory.Save();

			var page = new PricingPage(freightEntry, Factory, PricingPageStyle.Landscape);
			var loader = new RelatedRateEntriesLoader(page, Factory);
			var results = loader.GetRelatedEntries(freightEntry);

			var message = "Should only include the current rate entry and not the expired one due to the Default User Filter on the DSTRateEntryCollection";
			AssertContainsExactElementsInAnyOrder(message, new[] { currentEntry.PK }, results.DestinationRateEntries.Select(x => x.PK));
		}

		public void TestModeFilterFromFreight_LCL()
		{
			const string expected = @"
[-[ LCL|AU->NL|SEA| (GLB) <FRT,FRT> ]-]
**Freight**
LCL|AU->NL|SEA| (GLB) <FRT,FRT>
**Origin**
ORG|AU->|ALL| (GLB) <ODOC>
**Destoniation**
DST|->NL|ALL| (GLB) <DDOC>
";

			var page = PricingPageTestHelper.SetupSampleRatesForSupplementaryModeFiltering(Factory, null)[RatingConstants.RateCategory.LCL];
			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestModeFilterFromFreight_FCL()
		{
			const string expected = @"
[-[ FCL|AU->NL|SEA| (GLB) <FRT,FRT> ]-]
**Freight**
FCL|AU->NL|SEA| (GLB) <FRT,FRT>
**Origin**
ORG|AU->|ALL| (GLB) <ODOC>
ORG|AU->|FCL| (GLB) <OLAB>
ORG|AU->|FCL| (GLB) <OPCH>
**Destoniation**
DST|->NL|ALL| (GLB) <DDOC>
DST|->NL|FCL| (GLB) <DLAB>
DST|->NL|FCL| (GLB) <DPCH>
";

			var page = PricingPageTestHelper.SetupSampleRatesForSupplementaryModeFiltering(Factory, null)[RatingConstants.RateCategory.FCL];
			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestModeFilterFromSupplementary_All()
		{
			const string destinationExpected = @"
[-[ DST|->NL|ALL| (GLB) <DDOC> ]-]
**Freight**
**Origin**
**Destoniation**
DST|->NL|ALL| (GLB) <DDOC>
";

			const string originExpected = @"
[-[ ORG|AU->|ALL| (GLB) <ODOC> ]-]
**Freight**
**Origin**
ORG|AU->|ALL| (GLB) <ODOC>
**Destoniation**
";

			var pages = PricingPageTestHelper.SetupSampleRatesForSupplementaryModeFiltering(Factory, null);

			var destinationPage = pages[RatingConstants.RateCategory.DST];
			var originPage = pages[RatingConstants.RateCategory.ORG];

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Destination", destinationExpected, Render(destinationPage));
				AssertMultilineASCIIEquals("Origin", originExpected, Render(originPage));
			});
		}

		public void TestModeFilterFromSupplementary_LCL()
		{
			const string destinationExpected = @"
[-[ DST|->NL|LCL| (GLB) <DPCH> ]-]
**Freight**
**Origin**
**Destoniation**
DST|->NL|ALL| (GLB) <DDOC>
DST|->NL|LCL| (GLB) <DPCH>
";

			const string originExpected = @"
[-[ ORG|AU->|LCL| (GLB) <OPCH> ]-]
**Freight**
**Origin**
ORG|AU->|ALL| (GLB) <ODOC>
ORG|AU->|LCL| (GLB) <OPCH>
**Destoniation**
";

			var pages = PricingPageTestHelper.SetupSampleRatesForSupplementaryModeFiltering(Factory, false);

			var destinationPage = pages[RatingConstants.RateCategory.DST];
			var originPage = pages[RatingConstants.RateCategory.ORG];

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Destination", destinationExpected, Render(destinationPage));
				AssertMultilineASCIIEquals("Origin", originExpected, Render(originPage));
			});
		}

		public void TestModeFilterFromSupplementary_FCL()
		{
			const string destinationExpected = @"
[-[ DST|->NL|FCL| (GLB) <DPCH> ]-]
**Freight**
**Origin**
**Destoniation**
DST|->NL|ALL| (GLB) <DDOC>
DST|->NL|FCL| (GLB) <DPCH>
";

			const string originExpected = @"
[-[ ORG|AU->|FCL| (GLB) <OPCH> ]-]
**Freight**
**Origin**
ORG|AU->|ALL| (GLB) <ODOC>
ORG|AU->|FCL| (GLB) <OPCH>
**Destoniation**
";

			var pages = PricingPageTestHelper.SetupSampleRatesForSupplementaryModeFiltering(Factory, true);

			var destinationPage = pages[RatingConstants.RateCategory.DST];
			var originPage = pages[RatingConstants.RateCategory.ORG];

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Destination", destinationExpected, Render(destinationPage));
				AssertMultilineASCIIEquals("Origin", originExpected, Render(originPage));
			});
		}

		public void TestPortFilterFromFreight()
		{
			const string expected = @"
[-[ FCL|AU->NL|SEA| (GLB) <FRT,FRT> ]-]
**Freight**
FCL|AU->NL|SEA| (GLB) <FRT,FRT>
**Origin**
ORG|AU->|FCL| (GLB) <ODOC>
ORG|AU->NL|FCL| (GLB) <OPCH>
ORG|AUSYD->|FCL| (GLB) <OPCH>
**Destoniation**
DST|->NL|FCL| (GLB) <DDOC>
DST|->NLAMS|FCL| (GLB) <DPCH>
DST|AU->NL|FCL| (GLB) <DPCH>
";

			var page = PricingPageTestHelper.SetupSampleRatesForSupplementaryPortFiltering(Factory, true)[RatingConstants.RateCategory.FCL];
			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestPortFilterFromSupplementary()
		{
			const string destinationExpected = @"
[-[ DST|->NL|FCL| (GLB) <DDOC> ]-]
**Freight**
**Origin**
**Destoniation**
DST|->NL|FCL| (GLB) <DDOC>
";

			const string originExpected = @"
[-[ ORG|AU->|FCL| (GLB) <ODOC> ]-]
**Freight**
**Origin**
ORG|AU->|FCL| (GLB) <ODOC>
**Destoniation**
";
			var pages = PricingPageTestHelper.SetupSampleRatesForSupplementaryPortFiltering(Factory, false);

			var destinationPage = pages[RatingConstants.RateCategory.DST];
			var originPage = pages[RatingConstants.RateCategory.ORG];

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Destination", destinationExpected, Render(destinationPage));
				AssertMultilineASCIIEquals("Origin", originExpected, Render(originPage));
			});
		}

		public void TestPortFilterFromSupplementaryWithBothPorts()
		{
			const string destinationExpected = @"
[-[ DST|AU->NL|FCL| (GLB) <DPCH> ]-]
**Freight**
**Origin**
**Destoniation**
DST|->NL|FCL| (GLB) <DDOC>
DST|AU->NL|FCL| (GLB) <DPCH>
";

			const string originExpected = @"
[-[ ORG|AU->NL|FCL| (GLB) <OPCH> ]-]
**Freight**
**Origin**
ORG|AU->|FCL| (GLB) <ODOC>
ORG|AU->NL|FCL| (GLB) <OPCH>
**Destoniation**
";

			var pages = PricingPageTestHelper.SetupSampleRatesForSupplementaryPortFiltering(Factory, true);

			var destinationPage = pages[RatingConstants.RateCategory.DST];
			var originPage = pages[RatingConstants.RateCategory.ORG];

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Destination", destinationExpected, Render(destinationPage));
				AssertMultilineASCIIEquals("Origin", originExpected, Render(originPage));
			});
		}

		public void TestVisibilityForDestination()
		{
			const string expected = @"
[-[ DST|AU->NL|FCL| (GLB) <CC1,CC1,CC2,CC2,CC3,CC3> ]-]
**Freight**
**Origin**
**Destoniation**
DST|AU->NL|FCL| (GLB) <CC1,CC1,CC2,CC2,CC3,CC3>
";

			var page = PricingPageTestHelper.SetupSampleRatesForVisibilityChecking(Factory, RatingConstants.RateCategory.DST);
			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestVisibilityForOrigin()
		{
			const string expected = @"
[-[ ORG|AU->NL|FCL| (GLB) <CC1,CC1,CC2,CC2,CC3,CC3> ]-]
**Freight**
**Origin**
ORG|AU->NL|FCL| (GLB) <CC1,CC1,CC2,CC2,CC3,CC3>
**Destoniation**
";

			var page = PricingPageTestHelper.SetupSampleRatesForVisibilityChecking(Factory, RatingConstants.RateCategory.ORG);
			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestVisibilityForFreight()
		{
			const string expected = @"
[-[ FCL|AU->NL|SEA| (GLB) <FRT,CC1,CC1,CC2,CC2,CC3,CC3> ]-]
**Freight**
FCL|AU->NL|SEA| (GLB) <FRT,CC1,CC1,CC2,CC2,CC3,CC3>
**Origin**
**Destoniation**
";

			var page = PricingPageTestHelper.SetupSampleRatesForVisibilityChecking(Factory, RatingConstants.RateCategory.FCL);
			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestVisibilityWhenBaseIsZero()
		{
			const string expected = @"
[-[ FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC2,FCC2> ]-]
**Freight**
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC1,FCC2>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC2,FCC2>
**Origin**
**Destoniation**
";

			var page = PricingPageTestHelper.SetupSampleRatesForVisibilityCheckingWhenBaseIsZero(Factory);
			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestVisibilityWhenBaseIsNotZeroAgencyCalculator()
		{
			const string expected = @"
[-[ FCL|AUBNE->NLAMS|SEA|STD (QTE) <CCC> ]-]
**Freight**
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC,CCC>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <CCC>
**Origin**
**Destoniation**
";

			var page = PricingPageTestHelper.SetupSampleRatesForVisibilityCheckingWhenBaseIsZeroAgencyCalculator(Factory, 10);

			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestVisibilityWhenBaseIsZeroAgencyCalculator()
		{
			const string expected = @"
[-[ FCL|AUBNE->NLAMS|SEA|STD (QTE) <CCC> ]-]
**Freight**
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC,CCC>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <CCC>
**Origin**
**Destoniation**
";

			var page = PricingPageTestHelper.SetupSampleRatesForVisibilityCheckingWhenBaseIsZeroAgencyCalculator(Factory, -100);
			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestVisibilityWithOverride()
		{
			const string expected = @"
[-[ FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC2> ]-]
**Freight**
FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC2>
FCL|AUBNE->NLAMS|SEA|STD (SAL) <FCC1,FCC2>
**Origin**
**Destoniation**
";

			var page = PricingPageTestHelper.SetupSampleRatesForVisibilityCheckingWithOverride(Factory);
			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestCombining()
		{
			const string expected = @"
[-[ LCL|AU->NL|LCL| (GLB) <FRT> ]-]
**Freight**
LCL|AU->NL|LCL| (GLB) <FRT>
**Origin**
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
**Destoniation**
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
";

			var page = PricingPageTestHelper.SetupSampleRatesForCombining(Factory);
			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestMatchingFromSupplementary_Destination()
		{
			const string expectedFromEmpty1 = @"
[-[ DST|AU->NL|ALL| (GLB) <DPCH> ]-]
**Freight**
**Origin**
**Destoniation**
DST|AU->NL|ALL| (GLB) <DPCH>
";

			const string expectedFromFull1 = @"
[-[ DST|AU->NL|ALL| (GLB) <DPCH> ]-]
**Freight**
**Origin**
**Destoniation**
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
";

			CombineAssertions(delegate
			{
				var emptyPage = PricingPageTestHelper.SetupSampleRatesForEmptyMatching(Factory, RatingConstants.RateCategory.DST, true);
				var fullPage = PricingPageTestHelper.SetupSampleRatesForEmptyMatching(Factory, RatingConstants.RateCategory.DST, false);

				AssertMultilineASCIIEquals("Empty", expectedFromEmpty1, Render(emptyPage));
				AssertMultilineASCIIEquals("Full", expectedFromFull1, Render(fullPage));
			});
		}

		public void TestMatchingFromSupplementary_Origin()
		{
			const string expectedFromEmpty2 = @"
[-[ ORG|AU->NL|ALL| (GLB) <OPCH> ]-]
**Freight**
**Origin**
ORG|AU->NL|ALL| (GLB) <OPCH>
**Destoniation**
";

			const string expectedFromFull2 = @"
[-[ ORG|AU->NL|ALL| (GLB) <OPCH> ]-]
**Freight**
**Origin**
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
**Destoniation**
";

			CombineAssertions(delegate
			{
				var emptyPage = PricingPageTestHelper.SetupSampleRatesForEmptyMatching(Factory, RatingConstants.RateCategory.ORG, true);
				var fullPage = PricingPageTestHelper.SetupSampleRatesForEmptyMatching(Factory, RatingConstants.RateCategory.ORG, false);

				AssertMultilineASCIIEquals("Empty", expectedFromEmpty2, Render(emptyPage));
				AssertMultilineASCIIEquals("Full", expectedFromFull2, Render(fullPage));
			});
		}

		public void TestMatchingFromSupplementary_DifferentServiceProviders()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			secondHelper.ChargeCodes.New("DCCT", "Destination Charge Code", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);
			secondHelper.ChargeCodes.New("DDDD", "Destination Document Processing Charge Code", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);
			secondHelper.ChargeCodes.New("OCCT", "Origin Charge Code", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);
			secondHelper.ChargeCodes.New("ODDD", "Origin Document Processing Charge Code", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);

			var serviceProvider = createFactory.NewWithValidTestData<OrgHeader>();
			var quote = secondHelper.NewQuote(secondHelper.NewOrgHeader());

			var destinationEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry1.TI_OH_Supplier = ZGuid.Empty;
			var destinationRateLine1A = destinationEntry1.AddRateLine("DCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.NewZealand);
			destinationRateLine1A.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(110);
			var destinationRateLine1B = destinationEntry1.AddRateLine("DDDD", FlatCalculator.Code, "", Constants.CurrencyCodes.NewZealand);
			destinationRateLine1B.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(20);

			var destinationEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry2.TI_OH_Supplier = serviceProvider.PK;
			var destinationRateLine2A = destinationEntry2.AddRateLine("DCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.NewZealand);
			destinationRateLine2A.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(160);

			var originEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "", "", "20GP");
			originEntry1.TI_OH_Supplier = ZGuid.Empty;
			var originRateLine1A = originEntry1.AddRateLine("OCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.NewZealand);
			originRateLine1A.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(150);
			var originRateLine1B = originEntry1.AddRateLine("ODDD", FlatCalculator.Code, "", Constants.CurrencyCodes.NewZealand);
			originRateLine1B.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(30);

			var originEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "", "", "20GP");
			originEntry2.TI_OH_Supplier = serviceProvider.PK;
			var originRateLine2A = originEntry2.AddRateLine("OCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.NewZealand);
			originRateLine2A.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(180);

			createFactory.Save();

			const string destinationExpected = @"
[-[ DST|->NZAKL|FCL| (QTE) <DCCT> ]-]
**Freight**
**Origin**
**Destoniation**
DST|->NZAKL|FCL| (QTE) <DCCT,DDDD>
DST|->NZAKL|FCL| (QTE) <DCCT>
";

			const string originExpected = @"
[-[ ORG|AUSYD->|FCL| (QTE) <OCCT> ]-]
**Freight**
**Origin**
ORG|AUSYD->|FCL| (QTE) <OCCT,ODDD>
ORG|AUSYD->|FCL| (QTE) <OCCT>
**Destoniation**
";

			var destinationPage = new PricingPage(destinationEntry2, Factory, PricingPageStyle.Standard);
			var originPage = new PricingPage(originEntry2, Factory, PricingPageStyle.Standard);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Destination", destinationExpected, Render(destinationPage));
				AssertMultilineASCIIEquals("Origin", originExpected, Render(originPage));
			});
		}

		public void TestMatchingFromSupplementary_NoDuplicateRates()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			secondHelper.ChargeCodes.New("DCCT", "Destination Charge Code", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);
			secondHelper.ChargeCodes.New("OCCT", "Origin Charge Code", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);

			var quote = secondHelper.NewQuote(secondHelper.NewOrgHeader());

			var destinationEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry1.TI_RS_NKServiceLevel_NI = ZString.Empty;
			var destinationRateLine1 = destinationEntry1.AddRateLine("DCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.NewZealand);
			destinationRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(20);

			var destinationEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry2.TI_RS_NKServiceLevel_NI = "STD";
			var destinationRateLine2 = destinationEntry2.AddRateLine("DCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.NewZealand);
			destinationRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(25);

			var destinationEntry3 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "40GP");
			destinationEntry3.TI_RS_NKServiceLevel_NI = ZString.Empty;
			var destinationRateLine3 = destinationEntry3.AddRateLine("DCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.HongKong);
			destinationRateLine3.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(40);

			var originEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "20GP");
			originEntry1.TI_RS_NKServiceLevel_NI = ZString.Empty;
			var originRateLine1 = originEntry1.AddRateLine("OCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.HongKong);
			originRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(20);

			var originEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "20GP");
			originEntry2.TI_RS_NKServiceLevel_NI = "STD";
			var originRateLine2 = originEntry2.AddRateLine("OCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.HongKong);
			originRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(25);

			var originEntry3 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "40GP");
			originEntry3.TI_RS_NKServiceLevel_NI = ZString.Empty;
			var originRateLine3 = originEntry3.AddRateLine("OCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.HongKong);
			originRateLine3.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(40);

			createFactory.Save();

			var destinationPage = new PricingPage(destinationEntry1, Factory, PricingPageStyle.Standard);
			destinationPage.AddRateEntry(destinationEntry2);
			destinationPage.AddRateEntry(destinationEntry3);

			var originPage = new PricingPage(originEntry1, Factory, PricingPageStyle.Standard);
			originPage.AddRateEntry(originEntry2);
			originPage.AddRateEntry(originEntry3);

			const string destinationExpected = @"
[-[ DST|->NZAKL|FCL| (QTE) <DCCT> ]-]
**Freight**
**Origin**
**Destoniation**
DST|->NZAKL|FCL| (QTE) <DCCT>
DST|->NZAKL|FCL| (QTE) <DCCT>

[-[ DST|->NZAKL|FCL|STD (QTE) <DCCT> ]-]
**Freight**
**Origin**
**Destoniation**
DST|->NZAKL|FCL| (QTE) <DCCT>
DST|->NZAKL|FCL| (QTE) <DCCT>
DST|->NZAKL|FCL|STD (QTE) <DCCT>
";

			const string originExpected = @"
[-[ ORG|HKHKG->|FCL| (QTE) <OCCT> ]-]
**Freight**
**Origin**
ORG|HKHKG->|FCL| (QTE) <OCCT>
ORG|HKHKG->|FCL| (QTE) <OCCT>
**Destoniation**

[-[ ORG|HKHKG->|FCL|STD (QTE) <OCCT> ]-]
**Freight**
**Origin**
ORG|HKHKG->|FCL| (QTE) <OCCT>
ORG|HKHKG->|FCL| (QTE) <OCCT>
ORG|HKHKG->|FCL|STD (QTE) <OCCT>
**Destoniation**
";

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Destination", destinationExpected, Render(destinationPage));
				AssertMultilineASCIIEquals("Origin", originExpected, Render(originPage));
			});
		}

		public void TestMatchingFromSupplementary_NoDuplicateRatesFromCompanyTariff()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			var client = createFactory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "Client";

			secondHelper.ChargeCodes.New("DCCT", "Destination Charge Code", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);
			secondHelper.ChargeCodes.New("OCCT", "Origin Charge Code", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);

			var quote = secondHelper.NewQuote(client);

			var tariff = createFactory.New<CompanyTariff>();
			client.CompanyData.RateTariffLevels.SetLevel("DEF", tariff.TH_GlobalRateLevel);

			var destinationTariffEntry1 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "40GP");
			destinationTariffEntry1.TI_RS_NKServiceLevel_NI = "STD";
			var destinationTariffRateLine1 = destinationTariffEntry1.AddRateLine("DCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.NewZealand);
			destinationTariffRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(45.10);

			var destinationTariffEntry2 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationTariffEntry2.TI_RS_NKServiceLevel_NI = "STD";
			var destinationTariffRateLine2 = destinationTariffEntry2.AddRateLine("DCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.NewZealand);
			destinationTariffRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(25.10);

			var destinationEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry1.TI_RS_NKServiceLevel_NI = ZString.Empty;
			var destinationRateLine1 = destinationEntry1.AddRateLine("DCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.NewZealand);
			destinationRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(20);

			var destinationEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry2.TI_RS_NKServiceLevel_NI = "STD";
			var destinationRateLine2 = destinationEntry2.AddRateLine("DCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.NewZealand);
			destinationRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(25);

			var destinationEntry3 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "40GP");
			destinationEntry3.TI_RS_NKServiceLevel_NI = "STD";
			destinationEntry3.TI_CartageDeliveryAddressPostCode = "2000";

			var originTariffEntry1 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "40GP");
			originTariffEntry1.TI_RS_NKServiceLevel_NI = "STD";
			var originTariffRateLine1 = originTariffEntry1.AddRateLine("OCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.HongKong);
			originTariffRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(45.10);

			var originTariffEntry2 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "20GP");
			originTariffEntry2.TI_RS_NKServiceLevel_NI = "STD";
			var originTariffRateLine2 = originTariffEntry2.AddRateLine("OCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.HongKong);
			originTariffRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(25.10);

			var originEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "20GP");
			originEntry1.TI_RS_NKServiceLevel_NI = ZString.Empty;
			var originRateLine1 = originEntry1.AddRateLine("OCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.HongKong);
			originRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(20);

			var originEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "20GP");
			originEntry2.TI_RS_NKServiceLevel_NI = "STD";
			var originRateLine2 = originEntry2.AddRateLine("OCCT", FlatCalculator.Code, "", Constants.CurrencyCodes.HongKong);
			originRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(25);

			var originEntry3 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "40GP");
			originEntry3.TI_RS_NKServiceLevel_NI = "STD";
			originEntry3.TI_CartagePickupAddressPostCode = "2000";

			createFactory.Save();

			var destinationPage = new PricingPage(destinationEntry1, Factory, PricingPageStyle.Standard);
			destinationPage.AddRateEntry(destinationEntry2);
			destinationPage.AddRateEntry(destinationEntry3);

			var originPage = new PricingPage(originEntry1, Factory, PricingPageStyle.Standard);
			originPage.AddRateEntry(originEntry2);
			originPage.AddRateEntry(originEntry3);

			const string destinationExpected = @"
[-[ DST|->NZAKL|FCL| (QTE) <DCCT> ]-]
**Freight**
**Origin**
**Destoniation**
DST|->NZAKL|FCL| (QTE) <DCCT>

[-[ DST|->NZAKL|FCL|STD (QTE) <DCCT> ]-]
**Freight**
**Origin**
**Destoniation**
DST|->NZAKL|FCL| (QTE) <DCCT>
DST|->NZAKL|FCL|STD (GLB) <DCCT>
DST|->NZAKL|FCL|STD (GLB) <DCCT>
DST|->NZAKL|FCL|STD (QTE) <DCCT>

[-[ DST|->NZAKL|FCL|STD (QTE) <> ]-]
**Freight**
**Origin**
**Destoniation**
DST|->NZAKL|FCL| (QTE) <DCCT>
DST|->NZAKL|FCL|STD (GLB) <DCCT>
DST|->NZAKL|FCL|STD (GLB) <DCCT>
DST|->NZAKL|FCL|STD (QTE) <>
DST|->NZAKL|FCL|STD (QTE) <DCCT>
";

			const string originExpected = @"
[-[ ORG|HKHKG->|FCL| (QTE) <OCCT> ]-]
**Freight**
**Origin**
ORG|HKHKG->|FCL| (QTE) <OCCT>
**Destoniation**

[-[ ORG|HKHKG->|FCL|STD (QTE) <OCCT> ]-]
**Freight**
**Origin**
ORG|HKHKG->|FCL| (QTE) <OCCT>
ORG|HKHKG->|FCL|STD (GLB) <OCCT>
ORG|HKHKG->|FCL|STD (GLB) <OCCT>
ORG|HKHKG->|FCL|STD (QTE) <OCCT>
**Destoniation**

[-[ ORG|HKHKG->|FCL|STD (QTE) <> ]-]
**Freight**
**Origin**
ORG|HKHKG->|FCL| (QTE) <OCCT>
ORG|HKHKG->|FCL|STD (GLB) <OCCT>
ORG|HKHKG->|FCL|STD (GLB) <OCCT>
ORG|HKHKG->|FCL|STD (QTE) <>
ORG|HKHKG->|FCL|STD (QTE) <OCCT>
**Destoniation**
";

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Destination", destinationExpected, Render(destinationPage));
				AssertMultilineASCIIEquals("Origin", originExpected, Render(originPage));
			});
		}

		public void TestMatchingFromFreight()
		{
			const string expectedFromEmpty = @"
[-[ LCL|AU->NL|LCL| (GLB) <FRT> ]-]
**Freight**
LCL|AU->NL|LCL| (GLB) <FRT>
**Origin**
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
**Destoniation**
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
";

			const string expectedFromFull = @"
[-[ LCL|AU->NL|LCL| (GLB) <FRT> ]-]
**Freight**
LCL|AU->NL|LCL| (GLB) <FRT>
LCL|AU->NL|LCL| (GLB) <FRT>
LCL|AU->NL|LCL| (GLB) <FRT>
LCL|AU->NL|LCL| (GLB) <FRT>
LCL|AU->NL|LCL| (GLB) <FRT>
LCL|AU->NL|LCL| (GLB) <FRT>
LCL|AU->NL|LCL| (GLB) <FRT>
LCL|AU->NL|LCL| (GLB) <FRT>
LCL|AU->NL|LCL| (GLB) <FRT>
LCL|AU->NL|LCL| (GLB) <FRT>
LCL|AU->NL|LCL| (GLB) <FRT>
**Origin**
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
ORG|AU->NL|ALL| (GLB) <OPCH>
**Destoniation**
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
DST|AU->NL|ALL| (GLB) <DPCH>
";

			CombineAssertions(delegate
			{
				var emptyPage = PricingPageTestHelper.SetupSampleRatesForEmptyMatching(Factory, RatingConstants.RateCategory.LCL, true);
				var fullPage = PricingPageTestHelper.SetupSampleRatesForEmptyMatching(Factory, RatingConstants.RateCategory.LCL, false);

				AssertMultilineASCIIEquals("Empty", expectedFromEmpty, Render(emptyPage));
				AssertMultilineASCIIEquals("Full", expectedFromFull, Render(fullPage));
			});
		}

		public void TestMatchingFromFreight_DifferentServiceProviders()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			secondHelper.ChargeCodes.New("FCCT", "Freight Charge Code", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);
			secondHelper.ChargeCodes.New("FCC2", "Freight Charge Code 2", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);
			secondHelper.ChargeCodes.New("FCC3", "Freight Charge Code 3", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);
			secondHelper.ChargeCodes.New("DCCT", "Destination Charge Code", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);
			secondHelper.ChargeCodes.New("OCCT", "Origin Charge Code", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);
			secondHelper.ChargeCodes.New("ODDD", "Origin Document Processing Charge Code", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);

			var serviceProvider1 = createFactory.NewWithValidTestData<OrgHeader>();
			var serviceProvider2 = createFactory.NewWithValidTestData<OrgHeader>();

			var quote = secondHelper.NewQuote(createFactory.NewWithValidTestData<OrgHeader>());

			var freightEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "NZAKL", "STD", "20GP");
			freightEntry1.TI_OH_Supplier = serviceProvider1.PK;
			freightEntry1.AddRateLine("FCCT").GetCalculator<FlatCalculator>().BaseRate = 1200;

			var freightEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "STD", "20GP");
			freightEntry2.AddRateLine("FCC2").GetCalculator<FlatCalculator>().BaseRate = 50;
			freightEntry2.AddRateLine("FCCT").GetCalculator<FlatCalculator>().BaseRate = 1500;

			var freightEntry3 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "STD", "20GP");
			freightEntry3.TI_OH_Supplier = serviceProvider2.PK;
			freightEntry3.AddRateLine("FCC3").GetCalculator<FlatCalculator>().BaseRate = 60;

			var destinationEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry1.TI_OH_Supplier = serviceProvider1.PK;
			destinationEntry1.AddRateLine("DCCT").GetCalculator<FlatCalculator>().BaseRate = 250;

			var destinationEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry2.TI_OH_Supplier = serviceProvider2.PK;
			destinationEntry2.AddRateLine("DCCT").GetCalculator<FlatCalculator>().BaseRate = 350;

			var destinationEntry3 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry3.TI_OH_Supplier = ZGuid.Empty;
			destinationEntry3.AddRateLine("DCCT").GetCalculator<FlatCalculator>().BaseRate = 200;

			var originEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "", "", "20GP");
			originEntry1.TI_RX_NKCurrency = Constants.CurrencyCodes.NewZealand;
			originEntry1.TI_OH_Supplier = serviceProvider1.PK;
			originEntry1.AddRateLine("OCCT").GetCalculator<FlatCalculator>().BaseRate = 150;
			originEntry1.AddRateLine("ODDD").GetCalculator<FlatCalculator>().BaseRate = 30;

			var originEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "", "", "20GP");
			originEntry2.TI_RX_NKCurrency = Constants.CurrencyCodes.NewZealand;
			originEntry2.TI_OH_Supplier = serviceProvider2.PK;
			originEntry2.AddRateLine("OCCT").GetCalculator<FlatCalculator>().BaseRate = 180;

			var originEntry3 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "", "", "20GP");
			originEntry3.TI_RX_NKCurrency = Constants.CurrencyCodes.NewZealand;
			originEntry3.TI_OH_Supplier = ZGuid.Empty;
			originEntry3.AddRateLine("OCCT").GetCalculator<FlatCalculator>().BaseRate = 200;

			createFactory.Save();

			var page = new PricingPage(freightEntry1, Factory, PricingPageStyle.Standard);

			const string expected = @"
[-[ FCL|AUSYD->NZAKL|SEA|STD (QTE) <FRT,FCCT> ]-]
**Freight**
FCL|AU->NZ|SEA|STD (QTE) <FRT,FCC2,FCCT>
FCL|AUSYD->NZAKL|SEA|STD (QTE) <FRT,FCCT>
**Origin**
ORG|AUSYD->|FCL| (QTE) <OCCT,ODDD>
ORG|AUSYD->|FCL| (QTE) <OCCT>
ORG|AUSYD->|FCL| (QTE) <OCCT>
**Destoniation**
DST|->NZAKL|FCL| (QTE) <DCCT>
DST|->NZAKL|FCL| (QTE) <DCCT>
DST|->NZAKL|FCL| (QTE) <DCCT>
";

			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestMatchingFromFreight_ChargesFromCompanyTariff()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USNYC";
			Helper.ChargeCodes.New("DSSS", "Destination Charge Code", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);
			Helper.ChargeCodes.New("OSSS", "Origin Charge Code", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);

			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;
			tariff.TH_GlobalRateDescription = "Level 1";

			var dstEntry1 = tariff.AddRateEntry("DST", "FCL", "", "US");
			var dstLine1 = dstEntry1.AddRateLine("DSSS");
			dstLine1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)120m;

			var dstEntry2 = tariff.AddRateEntry("DST", "AIR", "", "US");
			var dstLine2 = dstEntry2.AddRateLine("DSSS");
			dstLine2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)150m;

			var orgEntey1 = tariff.AddRateEntry("ORG", "FCL", "AU", "");
			var orgLine1 = orgEntey1.AddRateLine("OSSS");
			orgLine1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)120m;

			var orgEntry2 = tariff.AddRateEntry("ORG", "AIR", "AU", "");
			var orgLine2 = orgEntry2.AddRateLine("OSSS");
			orgLine2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)150m;

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var level1A = org.CompanyData.RateTariffLevels.AddNew();
			level1A.P7_TariffType = "DST";
			level1A.P7_Mode = "ALL";
			level1A.P7_Direction = "ALL";
			level1A.P7_TariffLevel = 1;

			var level1B = org.CompanyData.RateTariffLevels.AddNew();
			level1B.P7_TariffType = "ORG";
			level1B.P7_Mode = "ALL";
			level1B.P7_Direction = "ALL";
			level1B.P7_TariffLevel = 1;

			var quote = Helper.NewQuote(org);
			var quoteEntry1 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "USNYC");
			quoteEntry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 1200m;

			var page = new PricingPage(quoteEntry1, Factory, PricingPageStyle.Standard);

			var expected1 = @"
[-[ FCL|AUSYD->USNYC|SEA| (QTE) <FRT> ]-]
**Freight**
FCL|AUSYD->USNYC|SEA| (QTE) <FRT>
**Origin**
ORG|AU->|FCL| (GLB) <OSSS>
**Destoniation**
DST|->US|FCL| (GLB) <DSSS>
";
			AssertMultilineASCIIEquals("", expected1, Render(page));

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUMEL";
			var quoteEntry2 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USNYC");
			quoteEntry2.RateLines[0].GetCalculator<CombinedCalculator>().PerUnit = 2200m;

			page = new PricingPage(quoteEntry2, Factory, PricingPageStyle.Standard);
			var expected2 = @"
[-[ AIR|AUSYD->USNYC|LSE| (QTE) <FRT> ]-]
**Freight**
AIR|AUSYD->USNYC|LSE| (QTE) <FRT>
**Origin**
ORG|AU->|AIR| (GLB) <OSSS>
**Destoniation**
DST|->US|AIR| (GLB) <DSSS>
";
			AssertMultilineASCIIEquals("", expected2, Render(page));
		}

		public void TestInheritOtherChargesFromLessSpecificDestinationsAndOrigins()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			AccChargeCode[] destinationChargeCodes =
			{
				secondHelper.ChargeCodes.New("DCC1", "Destination Charge Code 1", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true),
				secondHelper.ChargeCodes.New("DCC2", "Destination Charge Code 2", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true),
			};

			AccChargeCode[] originChargeCodes =
			{
				secondHelper.ChargeCodes.New("OCC1", "Origin Charge Code 1", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true),
				secondHelper.ChargeCodes.New("OCC2", "Origin Charge Code 2", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true),
			};

			var tariff = createFactory.New<CompanyTariff>();

			var originTariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "");
			PricingPageTestHelper.AddChargeLines(originTariffEntry, 0, originChargeCodes);

			var destinationTariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "", "NLAMS");
			PricingPageTestHelper.AddChargeLines(destinationTariffEntry, 0, destinationChargeCodes);

			var quote = secondHelper.NewQuote(secondHelper.NewOrgHeader(1));
			var originQuoteEntry = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "", "", "");
			PricingPageTestHelper.AddChargeLines(originQuoteEntry, 1, originChargeCodes);

			var destinationQuoteEntry = quote.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "", "NLAMS", "", "");
			PricingPageTestHelper.AddChargeLines(destinationQuoteEntry, 1, destinationChargeCodes);

			createFactory.Save();

			var reloadedQuote = Factory.Load<Quote>(quote.PK);
			var pages = new PricingPageCollection(reloadedQuote);
			pages.LoadStandard();

			const string destinationExpected = @"
[-[ DST|->NLAMS|ALL| (QTE) <DCC2> ]-]
**Freight**
**Origin**
**Destoniation**
DST|->NLAMS|ALL| (GLB) <DCC1,DCC2>
DST|->NLAMS|ALL| (QTE) <DCC2>
";

			const string originExpected = @"
[-[ ORG|AUBNE->|ALL| (QTE) <OCC2> ]-]
**Freight**
**Origin**
ORG|AUBNE->|ALL| (GLB) <OCC1,OCC2>
ORG|AUBNE->|ALL| (QTE) <OCC2>
**Destoniation**
";

			CombineAssertions(delegate
			{
				AssertEquals("precondition: page count", 2, pages.Count);
				AssertMultilineASCIIEquals("Origin", originExpected, Render(pages[0]));
				AssertMultilineASCIIEquals("Destination", destinationExpected, Render(pages[1]));
			});
		}

		public void TestInherit()
		{
			const string expected = @"
[-[ FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3> ]-]
**Freight**
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC1,FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC1,FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC1,FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC1,FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (SAL) <FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (SAL) <FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (SAL) <FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (SAL) <FCC2,FCC3>
**Origin**
ORG|AUBNE->|ALL| (GLB) <OCC1,OCC2,OCC3>
ORG|AUBNE->|ALL| (QTE) <OCC3>
ORG|AUBNE->|ALL| (SAL) <OCC2,OCC3>
**Destoniation**
DST|->NLAMS|ALL| (GLB) <DCC1,DCC2,DCC3>
DST|->NLAMS|ALL| (QTE) <DCC3>
DST|->NLAMS|ALL| (SAL) <DCC2,DCC3>
";

			var page = PricingPageTestHelper.SetupRates(Factory);
			var firstOrDefault = page.RateEntries.FirstOrDefault();

			if (firstOrDefault != null)
			{
				var header = firstOrDefault.Parent;
				header.TH_PrintRateLevelDestinationCharges = true;
				header.TH_PrintInheritedDestinationCharges = true;
			}

			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestNoInherit()
		{
			const string expected = @"
[-[ FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3> ]-]
**Freight**
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC1,FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC1,FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC1,FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC1,FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (SAL) <FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (SAL) <FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (SAL) <FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (SAL) <FCC2,FCC3>
**Origin**
ORG|AUBNE->|ALL| (QTE) <OCC3>
**Destoniation**
DST|->NLAMS|ALL| (QTE) <DCC3>
";

			var page = PricingPageTestHelper.SetupRates(Factory);
			var firstOrDefault = page.RateEntries.FirstOrDefault();
			if (firstOrDefault != null)
			{
				var header = firstOrDefault.Parent;
				header.TH_PrintRateLevelDestinationCharges = true;
				header.TH_PrintInheritedDestinationCharges = false;
				header.TH_PrintRateLevelOriginCharges = true;
				header.TH_PrintInheritedOriginCharges = false;
			}

			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestNoLoad()
		{
			var page = PricingPageTestHelper.SetupRates(Factory);
			var firstOrDefault = page.RateEntries.FirstOrDefault();

			if (firstOrDefault != null)
			{
				var header = firstOrDefault.Parent;
				header.TH_PrintRateLevelDestinationCharges = false;
				header.TH_PrintRateLevelOriginCharges = false;
			}

			const string expected = @"
[-[ FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3> ]-]
**Freight**
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC1,FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC1,FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC1,FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (GLB) <FCC1,FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (QTE) <FCC3,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (SAL) <FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (SAL) <FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (SAL) <FCC2,FCC3>
FCL|AUBNE->NLAMS|SEA|STD (SAL) <FCC2,FCC3>
**Origin**
**Destoniation**
";

			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestViaFilter()
		{
			var createFactory = new BusinessObjectFactory();

			Helper.ChargeCodes.New("DCC1", "Destination Charge Code 1", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);
			Helper.ChargeCodes.New("DCC2", "Destination Charge Code 2", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);
			Helper.ChargeCodes.New("OCC1", "Origin Charge Code 1", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);
			Helper.ChargeCodes.New("OCC2", "Origin Charge Code 2", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);

			createFactory.Save();

			var client = Factory.NewWithValidTestData<OrgHeader>();
			var quote = Helper.NewQuote(client);

			var freightEntry1 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", "", "20GP");
			freightEntry1.TI_ViaLRC = "SGSIN";
			freightEntry1.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)540m;

			var freightEntry2 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", "", "20GP");
			freightEntry2.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)300m;

			var destinationEntry1 = quote.AddRateEntry("DST", "FCL", "", "NZAKL", "", "20GP");
			destinationEntry1.TI_ViaLRC = "SGSIN";
			var destinationLine1 = destinationEntry1.AddRateLine("DCC1", UnitCalculator.Code, QuantityUnit.CN);
			destinationLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)40m;

			var destinationEntry2 = quote.AddRateEntry("DST", "FCL", "", "NZAKL", "", "20GP");
			var destinationLine2 = destinationEntry2.AddRateLine("DCC2", FlatCalculator.Code);
			destinationLine2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)52m;

			var originEntry1 = quote.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");
			originEntry1.TI_ViaLRC = "SGSIN";
			var originLine1 = originEntry1.AddRateLine("OCC1", UnitCalculator.Code, QuantityUnit.CN);
			originLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)40m;

			var originEntry2 = quote.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");
			var originLine2 = originEntry2.AddRateLine("OCC2", FlatCalculator.Code);
			originLine2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)52m;

			Factory.Save();

			var destinationPage1 = new PricingPage(destinationEntry1, Factory, PricingPageStyle.Standard);
			var destinationPage2 = new PricingPage(destinationEntry2, Factory, PricingPageStyle.Standard);
			var originPage1 = new PricingPage(originEntry1, Factory, PricingPageStyle.Standard);
			var originPage2 = new PricingPage(originEntry2, Factory, PricingPageStyle.Standard);
			var freightPage1 = new PricingPage(freightEntry1, Factory, PricingPageStyle.Standard);
			var freightPage2 = new PricingPage(freightEntry2, Factory, PricingPageStyle.Standard);

			var destinationExpected1 = @"
[-[ DST|->SGSIN->NZAKL|FCL| (QTE) <DCC1> ]-]
**Freight**
**Origin**
**Destoniation**
DST|->NZAKL|FCL| (QTE) <DCC2>
DST|->SGSIN->NZAKL|FCL| (QTE) <DCC1>
";

			var destinationExpected2 = @"
[-[ DST|->NZAKL|FCL| (QTE) <DCC2> ]-]
**Freight**
**Origin**
**Destoniation**
DST|->NZAKL|FCL| (QTE) <DCC2>
";

			var originExpected1 = @"
[-[ ORG|AUSYD->SGSIN->|FCL| (QTE) <OCC1> ]-]
**Freight**
**Origin**
ORG|AUSYD->|FCL| (QTE) <OCC2>
ORG|AUSYD->SGSIN->|FCL| (QTE) <OCC1>
**Destoniation**
";

			var originExpected2 = @"
[-[ ORG|AUSYD->|FCL| (QTE) <OCC2> ]-]
**Freight**
**Origin**
ORG|AUSYD->|FCL| (QTE) <OCC2>
**Destoniation**
";

			var freightExpected1 = @"
[-[ FCL|AUSYD->SGSIN->NZAKL|SEA| (QTE) <FRT> ]-]
**Freight**
FCL|AUSYD->NZAKL|SEA| (QTE) <FRT>
FCL|AUSYD->SGSIN->NZAKL|SEA| (QTE) <FRT>
**Origin**
ORG|AUSYD->|FCL| (QTE) <OCC2>
ORG|AUSYD->SGSIN->|FCL| (QTE) <OCC1>
**Destoniation**
DST|->NZAKL|FCL| (QTE) <DCC2>
DST|->SGSIN->NZAKL|FCL| (QTE) <DCC1>
";

			var freightExpected2 = @"
[-[ FCL|AUSYD->NZAKL|SEA| (QTE) <FRT> ]-]
**Freight**
FCL|AUSYD->NZAKL|SEA| (QTE) <FRT>
**Origin**
ORG|AUSYD->|FCL| (QTE) <OCC2>
**Destoniation**
DST|->NZAKL|FCL| (QTE) <DCC2>
";

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Destination 1", destinationExpected1, Render(destinationPage1));
				AssertMultilineASCIIEquals("Destination 2", destinationExpected2, Render(destinationPage2));
				AssertMultilineASCIIEquals("Origin 1", originExpected1, Render(originPage1));
				AssertMultilineASCIIEquals("Origin 2", originExpected2, Render(originPage2));
				AssertMultilineASCIIEquals("Freight 1", freightExpected1, Render(freightPage1));
				AssertMultilineASCIIEquals("Freight 2", freightExpected2, Render(freightPage2));
			});
		}

		public void TestZoneRates()
		{
			var zone = Helper.NewInternationalZone("AUTS", null, "AUSYD", "AUMEL");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cost = Helper.NewCosting(org);

			var entry1 = cost.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", "", "20GP");
			entry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 1100m;

			var entry2 = cost.AddRateEntry("FCL", "SEA", "AUMEL", "NZAKL", "", "20GP");
			entry2.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 2200m;

			var entry3 = cost.AddRateEntry("FCL", "SEA", "AUTS", "NZAKL", "", "20GP");
			entry3.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 3300m;

			var page = new PricingPage(entry3, Factory, PricingPageStyle.Landscape);

			const string expected = @"
[-[ FCL|AUTS->NZAKL|SEA| (COS) <FRT> ]-]
**Freight**
FCL|AUTS->NZAKL|SEA| (COS) <FRT>
**Origin**
**Destoniation**
";

			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestNotMatchingByFrequency()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var costing = Factory.NewWithValidTestData<Costing>();
			costing.TH_OH = carrier.PK;

			var costingEntry1 = costing.AddRateEntry("FCL", "SEA", "DE", "NZ");
			costingEntry1.TI_Frequency = 1;
			costingEntry1.TI_FrequencyUnit = FrequencyList.Codes.Week;
			costingEntry1.TI_OH_TransportProvider = carrier.PK;
			costingEntry1.RateLines.RemoveAndDeleteAll();
			var costingRate1 = costingEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			costingRate1.Calculator[UnitCalculator.Items.Operator.UNT] = (ZDecimal)350m;

			var costingEntry2 = costing.AddRateEntry("FCL", "SEA", "DEHAM", "NZAKL");
			costingEntry2.TI_Frequency = 1;
			costingEntry2.TI_FrequencyUnit = FrequencyList.Codes.Week;
			costingEntry2.TI_OH_TransportProvider = carrier.PK;
			costingEntry2.RateLines.RemoveAndDeleteAll();
			var costingRate2 = costingEntry2.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			costingRate2.Calculator[UnitCalculator.Items.Operator.UNT] = (ZDecimal)785m;

			Factory.Save();

			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;
			tariff.TH_GlobalRateDescription = "Level 1";

			var tariffEntry1 = tariff.AddRateEntry("FCL", "SEA", "DE", "NZ");
			tariffEntry1.TI_OH_TransportProvider = carrier.PK;
			tariffEntry1.TI_Frequency = 1;
			tariffEntry1.TI_FrequencyUnit = FrequencyList.Codes.Week;
			tariffEntry1.RateLines.RemoveAndDeleteAll();
			var tariffRate1 = tariffEntry1.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			tariffRate1.Calculator[CompanyTariffOrCostBasedCalculator.Items.Operator.UNT] = (ZDecimal)150m;
			var tariffRate2 = tariffEntry1.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);

			var tariffEntry2 = tariff.AddRateEntry("FCL", "SEA", "", "NZ");
			tariffEntry2.RateLines.RemoveAndDeleteAll();
			var tariffRate3 = tariffEntry2.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			tariffRate3.Calculator[UnitCalculator.Items.Operator.UNT] = (ZDecimal)1111m;

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var level0 = org.CompanyData.RateTariffLevels.AddNew();
			level0.P7_TariffType = OrgRateTariffLevel.DefaultTariffType;
			level0.P7_Mode = "ALL";
			level0.P7_Direction = "ALL";
			level0.P7_TariffLevel = 0;
			var level1 = org.CompanyData.RateTariffLevels.AddNew();
			level1.P7_TariffType = "FRT";
			level1.P7_Mode = "SEA";
			level1.Validation.ValidateP7_Mode();
			AssertNoErrors(level1.P7_ModeInfo);
			level1.P7_Direction = "ALL";
			level1.P7_TariffLevel = 1;

			var quote = Helper.NewQuote(org);
			var quoteEntry = quote.AddRateEntry("FCL", "SEA", "DEHAM", "NZAKL");
			quoteEntry.TI_OH_TransportProvider = carrier.PK;
			quoteEntry.TI_RC = GP20.PK;
			var quoteRateLine = quoteEntry.RateLines[0];

			var page = new PricingPage(quoteEntry, Factory, PricingPageStyle.Standard);

			const string expected = @"
[-[ FCL|DEHAM->NZAKL|SEA| (QTE) <FRT> ]-]
**Freight**
FCL|->NZ|SEA| (GLB) <BAF>
FCL|DE->NZ|SEA| (GLB) <FRT,BAF>
FCL|DEHAM->NZAKL|SEA| (QTE) <FRT>
**Origin**
**Destoniation**
";

			AssertMultilineASCIIEquals("", expected, Render(page));
		}

		public void TestGetRelatedEntries_Cache()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry1 = clientRate.AddRateEntryWithFlatRateLine(RateCategory.LCL, Constants.RateMode.LCL, "AU", "NZ", "BAF", 11);
			rateEntry1.TI_ContractNumber = "CONTRACT1";
			var rateEntry2 = clientRate.AddRateEntryWithFlatRateLine(RateCategory.LCL, Constants.RateMode.LCL, "AU", "NZ", "BAF", 12);
			var rateEntry3 = clientRate.AddRateEntryWithFlatRateLine(RateCategory.LCL, Constants.RateMode.LCL, "AU", "NZ", "BAF", 13);
			rateEntry3.TI_ContractNumber = "CONTRACT3";

			var page = new PricingPage(rateEntry1, Factory, PricingPageStyle.Standard);
			var loader = new RelatedRateEntriesLoader(page, Factory);

			AssertContainsExactElementsInAnyOrder
			(
				(RateEntry rateEntry) => GetRateEntryDescription(rateEntry),
				new[] { rateEntry1 },
				loader.GetRelatedEntries(rateEntry1, exactMatch: true).FreightRateEntries
			);

			AssertContainsExactElementsInAnyOrder
			(
				(RateEntry rateEntry) => GetRateEntryDescription(rateEntry),
				new[] { rateEntry1, rateEntry2, rateEntry3 },
				loader.GetRelatedEntries(rateEntry1, exactMatch: false).FreightRateEntries
			);
		}

		static string GetRateEntryDescription(RateEntry rateEntry)
		{
			var result = new StringBuilder();
			result.Append($"{rateEntry.TI_RateCategory}-{rateEntry.TI_Mode}-{rateEntry.TI_ContractNumber}");

			var rateLine = (RateLine)rateEntry.RateLines.FirstOrDefault();
			if (rateLine != null && rateLine.Calculator is FlatCalculator flatCalculator)
			{
				result.Append($"-{rateLine.ChargeCode.AC_Code}-{flatCalculator.BaseRate}");
			}

			return result.ToString();
		}

		public void TestRateLinesAreNotCachedByParent_RateEntry()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.SEA, "AU", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = Helper.AddRateLineWithFlatCalculatorToRateEntry(rateEntry, "FRT", 200m);
			var rateLine2 = Helper.AddRateLineWithFlatCalculatorToRateEntry(rateEntry, "WAR", 100m);

			var page = new PricingPage(rateEntry, Factory, PricingPageStyle.Standard);
			var lineSetFactory = new PricingPageRateLineFactory(RatingEnums.EntryTypes.Freight);
			var pageLineSetList = lineSetFactory.LoadLineSets(page, page.RateEntries);

			var actualResult = pageLineSetList.SelectMany(set => set).Select(line => line.PK);
			AssertContainsExactElementsInAnyOrder(new[] { rateLine1.PK, rateLine2.PK }, actualResult);

			rateLine2.Delete();

			pageLineSetList = lineSetFactory.LoadLineSets(page, page.RateEntries);

			actualResult = pageLineSetList.SelectMany(set => set).Select(line => line.PK);
			AssertContainsExactElementsInAnyOrder(new[] { rateLine1.PK }, actualResult);

			var rateLine3 = Helper.AddRateLineWithFlatCalculatorToRateEntry(rateEntry, "CAF", 30m);
			pageLineSetList = lineSetFactory.LoadLineSets(page, page.RateEntries);

			var message = "Because it's the same rate entry instance and not a loaded copy, we don't have any issues with changes";
			actualResult = pageLineSetList.SelectMany(set => set).Select(line => line.PK);
			AssertContainsExactElementsInAnyOrder(message, new[] { rateLine1.PK, rateLine3.PK }, actualResult);
		}

		public void TestRateLinesAreNotCachedByParent_QuoteEntry()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			var quoteEntry = quote.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.SEA, "AU", "");
			quoteEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = Helper.AddRateLineWithFlatCalculatorToRateEntry(quoteEntry, "FRT", 200m);
			var rateLine2 = Helper.AddRateLineWithFlatCalculatorToRateEntry(quoteEntry, "WAR", 100m);

			var page = new PricingPage(quoteEntry, Factory, PricingPageStyle.Standard);
			var lineSetFactory = new PricingPageRateLineFactory(RatingEnums.EntryTypes.Freight);
			var pageLineSetList = lineSetFactory.LoadLineSets(page, page.RateEntries);

			var actualResult = pageLineSetList.SelectMany(set => set).Select(line => line.PK);
			AssertContainsExactElementsInAnyOrder(new[] { rateLine1.PK, rateLine2.PK }, actualResult);

			rateLine2.Delete();

			pageLineSetList = lineSetFactory.LoadLineSets(page, page.RateEntries);

			actualResult = pageLineSetList.SelectMany(set => set).Select(line => line.PK);
			AssertContainsExactElementsInAnyOrder(new[] { rateLine1.PK }, actualResult);

			var rateLine3 = Helper.AddRateLineWithFlatCalculatorToRateEntry(quoteEntry, "CAF", 30m);
			pageLineSetList = lineSetFactory.LoadLineSets(page, page.RateEntries);

			var message = "Because it's the same rate entry instance and not a loaded copy, we don't have any issues with changes";
			actualResult = pageLineSetList.SelectMany(set => set).Select(line => line.PK);
			AssertContainsExactElementsInAnyOrder(message, new[] { rateLine1.PK, rateLine3.PK }, actualResult);
		}

		public void TestGlobalTariffPricingPage_RateLevelMoreThanOne()
		{
			var globalTariffLevel1 = Factory.New<GlobalTariff>();
			var globalTariffLevel2 = Factory.New<GlobalTariff>();

			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", UnitCalculator.Code);

			var tariffEntry = globalTariffLevel2.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "", "AUSYD");
			tariffEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tariffEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			tariffLine.GetCalculator<UnitCalculator>().PerUnit = 150m;

			var page = new PricingPage(tariffEntry, Factory, PricingPageStyle.Standard);
			var lineSetFactory = new PricingPageRateLineFactory(RatingEnums.EntryTypes.Freight);
			AssertNoExceptionThrown(() => lineSetFactory.LoadLineSets(page, page.RateEntries));
		}

		public void TestPricingPage_GlobalRelatedEntriesAreNotFilteredByDefaultUserFilter()
		{
			Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", FlatCalculator.Code);
			Helper.ChargeCodes.CreateGlobalCharge("GLBORG", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var client = Helper.NewOrgHeader(1);
			client.OH_IsConsignee = true;
			Factory.Save();

			var companyTariff = Factory.New<CompanyTariff>();
			var companyTariffEntry1 = companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "AU", "NZ", "WAR", 950);
			var companyTariffEntry2 = companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.LCL, "AU", "NZ", "OAQF", 500);

			var globalClientRate = Helper.NewGlobalClientRate(client);
			var globalClientRateEntry1 = globalClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "AU", "NZ", "GLBFRT", 920);
			var globalClientRateEntry2 = globalClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.LCL, "AU", "NZ", "GLBORG", 200);

			var localClientRate = Helper.NewClientRate(client);
			var localClientRateEntry1 = localClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "AU", "NZ", "FRT", 820);
			var localClientRateEntry2 = localClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.LCL, "AU", "NZ", "ODOC", 820);

			var page = new PricingPage(localClientRateEntry1, Factory, PricingPageStyle.Standard);
			var loader = new RelatedRateEntriesLoader(page, Factory);

			var actualResults = loader.GetRelatedEntries(localClientRateEntry1);
			var actualFreightResults = actualResults.FreightRateEntries.Select(x => x.PK);

			var message = "Global Client Rate should be excluded by DefaultUserFilter but Company should still apply";
			var expected = new ZGuid[] { localClientRateEntry1.PK, companyTariffEntry1.PK };

			AssertContainsExactElementsInAnyOrder(message, expected, actualFreightResults);

			var actualOriginResults = actualResults.OriginRateEntries.Select(x => x.PK);
			expected = new ZGuid[] { localClientRateEntry2.PK, companyTariffEntry2.PK };

			AssertContainsExactElementsInAnyOrder(message, expected, actualOriginResults);
		}

		protected string Render(PricingPage page)
		{
			var pageHelper = new RelatedRateEntriesLoader(page, Factory);
			var builder = new ZStringBuilder();

			foreach (var entry in page.RateEntries)
			{
				builder.AppendLine();
				builder.Append("[-[ ");
				builder.Append(Render(entry));
				builder.Append(" ]-]");
				builder.AppendLine();

				var re = pageHelper.GetRelatedEntries(entry);
				builder.Append("**Freight**");
				builder.AppendLine();
				builder.Append(Render(re.FreightRateEntries));

				builder.Append("**Origin**");
				builder.AppendLine();
				builder.Append(Render(re.OriginRateEntries));

				builder.Append("**Destoniation**");
				builder.AppendLine();
				builder.Append(Render(re.DestinationRateEntries));
			}

			return builder.ToString();
		}

		protected string Render(List<RateEntry> entries)
		{
			var renderedEntities = new List<string>();

			foreach (var entry in entries)
			{
				renderedEntities.Add(Render(entry));
			}

			renderedEntities.Sort();

			var builder = new ZStringBuilder();

			foreach (var renderedEntity in renderedEntities)
			{
				builder.AppendLine(renderedEntity);
			}

			return builder.ToString();
		}

		protected string Render(RateEntry entry)
		{
			var builder = new ZStringBuilder();

			builder.Append(entry.TI_RateCategory);
			builder.Append("|");

			builder.Append(entry.TI_OriginLRC);
			builder.Append("->");

			if (!entry.TI_ViaLRC.IsEmpty)
			{
				builder.Append(entry.TI_ViaLRC);
				builder.Append("->");
			}

			builder.Append(entry.TI_DestinationLRC);
			builder.Append("|");
			builder.Append(entry.TI_Mode);
			builder.Append("|");
			builder.Append(entry.TI_RS_NKServiceLevel_NI);

			builder.Append(" (");
			builder.Append(entry.Parent.TH_RateType);
			builder.Append(")");

			builder.Append(" <");

			for (var i = 0; i < entry.RateLines.Count; i++)
			{
				if (i > 0)
				{
					builder.Append(",");
				}

				var line = entry.RateLines[i];
				builder.Append(line.ChargeCode.AC_Code);
			}

			builder.Append(">");

			return builder.ToString();
		}
	}

	class RelatedEntriesLoaderTest_GetRelatedEntries : RatingTestCase
	{
		#region Combination Tests

		public void TestRelatedEntries_ORG_NoNeedToLoadInheritedRates() => AssertRelatedEntries(RateCategory.ORG, isLoadInheritedRates: false, expecteEntries: new[] { "QTE-Local" });
		public void TestRelatedEntries_ORG_NeedToLoadInheritedRates() => AssertRelatedEntries(RateCategory.ORG, isLoadInheritedRates: true, expecteEntries: new[] { "QTE-Local", "SAL-Local", "SAL-Global", "GLB-Local", "GLB-Global" });
		public void TestRelatedEntries_DST_NoNeedToLoadInheritedRates() => AssertRelatedEntries(RateCategory.DST, isLoadInheritedRates: false, expecteEntries: new[] { "QTE-Local" });
		public void TestRelatedEntries_DST_NeedToLoadInheritedRates() => AssertRelatedEntries(RateCategory.DST, isLoadInheritedRates: true, expecteEntries: new[] { "QTE-Local", "SAL-Local", "SAL-Global", "GLB-Local", "GLB-Global" });
		public void TestRelatedEntries_AIR_NoNeedToLoadInheritedRates() => AssertRelatedEntries(RateCategory.AIR, isLoadInheritedRates: false, expecteEntries: new[] { "QTE-Local", "SAL-Local", "SAL-Global", "GLB-Local", "GLB-Global" }, "RateCategory.Air doesn't have isNeedToLoadInheritedRates flag, hence it includes all RatingHeader");
		public void TestRelatedEntries_AIR_NeedToLoadInheritedRates() => AssertRelatedEntries(RateCategory.AIR, isLoadInheritedRates: true, expecteEntries: new[] { "QTE-Local", "SAL-Local", "SAL-Global", "GLB-Local", "GLB-Global" });

		void AssertRelatedEntries(string rateCategory, bool isLoadInheritedRates, string[] expecteEntries, string message = default)
		{
			const string rateMode = Constants.RateMode.AIR;

			var globalRateEntry = Helper.NewGlobalClientRate(client).AddRateEntryWithFlatRateLine(rateCategory, rateMode, "AU", "NZ", "CH3", 500);
			var localRateEntry = Helper.NewClientRate(client).AddRateEntryWithFlatRateLine(rateCategory, rateMode, "AU", "NZ", "CH2", 500);
			var globalTariff = Factory.New<GlobalTariff>();
			var globalTariffEntry = globalTariff.AddRateEntryWithFlatRateLine(rateCategory, rateMode, "AU", "NZ", "CH3", 500);
			var localTariff = Factory.New<CompanyTariff>();
			var localTariffEntry = localTariff.AddRateEntryWithFlatRateLine(rateCategory, rateMode, "AU", "NZ", "CH2", 500);

			AssertGetRelatedEntries(rateCategory, rateMode, isLoadInheritedRates, expecteEntries, message);
		}

		#endregion

		#region Implementation

		void AssertGetRelatedEntries(string rateCategory, string rateMode, bool isLoadInheritedRates, string[] expecteEntries, string message = default)
		{
			var quote = Helper.NewQuote(client);
			quote.TH_PrintRateLevelOriginCharges = true;

			quote.TH_PrintInheritedOriginCharges = isLoadInheritedRates;
			quote.TH_PrintInheritedDestinationCharges = isLoadInheritedRates;

			var quoteRateEntry = quote.AddRateEntryWithFlatRateLine(rateCategory, rateMode, "AU", "NZ", "CH1", 500);

			var pricingPage = new PricingPage(quoteRateEntry, Factory, PricingPageStyle.Standard);
			var relatedEntriesLoader = new RelatedRateEntriesLoader(pricingPage, Factory);
			var relatedEntries = relatedEntriesLoader.GetRelatedEntries(quoteRateEntry);
			var rateCategoryRelatedEntries = GetRateCategoryRelatedEntries(rateCategory, relatedEntries);

			AssertContainsExactElementsInAnyOrder
			(
				message,
				expected: expecteEntries,
				actual: rateCategoryRelatedEntries.Select(entry => RateEntryDisplay(entry))
			);
		}

		static string RateEntryDisplay(RateEntry rateEntry)
		{
			var result = new StringBuilder();
			result.Append($"{rateEntry.Parent.TH_RateType}-");

			if (rateEntry.Parent.TH_GC.IsEmpty)
			{
				result.Append($"Global");
			}
			else
			{
				result.Append($"Local");
			}

			return result.ToString();
		}

		IEnumerable<RateEntry> GetRateCategoryRelatedEntries(string rateCategory, RelatedRateEntries relatedEntries)
		{
			switch (rateCategory)
			{
				case RatingConstants.RateCategory.ORG:
					return relatedEntries.OriginRateEntries;
				case RatingConstants.RateCategory.DST:
					return relatedEntries.DestinationRateEntries;
				case RatingConstants.RateCategory.AIR:
					return relatedEntries.FreightRateEntries;
				default:
					throw new NotImplementedException();
			}
		}

		protected override void SetUp()
		{
			client = Helper.NewOrgHeader(companyTariffDefault: 1);
			client.OH_IsConsignee = true;

			Factory.Save();
		}

		OrgHeader client;

		#endregion
	}
}
