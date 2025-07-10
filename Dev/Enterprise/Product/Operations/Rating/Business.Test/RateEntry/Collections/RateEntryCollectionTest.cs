using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class RateEntryCollectionTest : RatingTestCase
	{
		public void TestRateLineDescription()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_LocalLanguageDescription = "Charge Local Description 1";

			Factory.Save();

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rate = Helper.NewClientRate(orgHeader);
				var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR);
				var rateLine = rateEntry.RateLines[0];

				AssertEquals("Charge Local Description 1", rateLine.GetRateDescOrRateDescLocal());
			}
		}

		public void TestIHaveZQueryForZGridExcelExport()
		{
			var tariff = Factory.New<CompanyTariff>();
			var tariff2 = Factory.New<CompanyTariff>();
			var entry1 = tariff.AddRateEntry("FCL", "LSE", "AU", "HK");
			var entry2 = tariff2.AddRateEntry("FCL", "LSE", "AU", "HK");

			var collection = tariff.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.FCL);
			var correctQuery = new ZQuery(RateEntrySchema.TI_TH, tariff.LevelOneTariff.PK);
			correctQuery.AddToFilter(RateEntrySchema.TI_RateCategory, "FCL");

			AssertEquals(Factory.Load<RateEntry>(correctQuery).Length, Factory.Load<RateEntry>(((IHaveZQueryForZGridExcelExport)collection).Query).Length);
			AssertEquals(Factory.LoadTop1<RateEntry>(correctQuery), Factory.LoadTop1<RateEntry>(((IHaveZQueryForZGridExcelExport)collection).Query));
		}

		public void TestDataRefreshBusDisabled()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			Factory.Save();

			var factory2 = new BusinessObjectFactory();

			var rateInFirstFactory = Factory.Load<ClientRate>(testRate.PK);
			var rateInSecondFactory = factory2.Load<ClientRate>(testRate.PK);

			AssertEquals("Precondition: No entries in either rate", 0, rateInFirstFactory.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Count);
			AssertEquals("Precondition: No entries in either rate", 0, rateInSecondFactory.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Count);

			rateInFirstFactory.AddRateEntry("ORG", "ALL", "", "");
			Factory.Save();

			AssertEquals("Rate in first factory has 1 entry", 1, rateInFirstFactory.EntryCollections[RatingConstants.RateCategory.ORG].LazyLoadingCollection.Count);
			AssertEquals("Rate in second factory has no entries as the collection is NOT published for datarefresh", 0, rateInSecondFactory.EntryCollections[RatingConstants.RateCategory.ORG].LazyLoadingCollection.Count);

			rateInSecondFactory.EntryCollections[RatingConstants.RateCategory.ORG].LazyLoadingCollection.Load();

			AssertEquals("Rate in first factory still has 1 entry", 1, rateInFirstFactory.EntryCollections[RatingConstants.RateCategory.ORG].LazyLoadingCollection.Count);
			AssertEquals("Rate in second factory also has 1 entry", 1, rateInSecondFactory.EntryCollections[RatingConstants.RateCategory.ORG].LazyLoadingCollection.Count);
		}

		public void TestDefaultCommodityCode()
		{
			var code1 = Factory.New<RefCommodityCode>();
			code1.RH_Code = "COD1";

			var code2 = Factory.New<RefCommodityCode>();
			code2.RH_Code = "COD2";

			Env.Registry.CommodityCode = code1.PK.ToGuid();

			var rate = Factory.New<ClientRate>();
			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.AIR);
			AssertEquals("Correct commodity code", "COD1", entry1.TI_RH_NKCommodityCode);

			var whsEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.WHS);
			AssertEquals("Blank for Product warehouse", "", whsEntry1.TI_RH_NKCommodityCode);

			var trwEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.TRW);
			AssertEquals("Blank for Transit warehouse", "", trwEntry1.TI_RH_NKCommodityCode);
			AssertEquals("Freight mode is all for transit entry", Core.Constants.RateMode.ALL, trwEntry1.TI_Mode);

			var twuEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.TWU);
			AssertEquals("Blank for Transit warehouse Transportation Unit", "", twuEntry1.TI_RH_NKCommodityCode);
			AssertEquals("Freight mode is all for transit transportation unit entry", Core.Constants.RateMode.ALL, twuEntry1.TI_Mode);

			var cstEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.CST);
			AssertEquals("Blank for container storage", "", cstEntry1.TI_RH_NKCommodityCode);

			var trnEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.TRN, "ALL", "", "");
			AssertEquals("Blank for transport", "", trnEntry1.TI_RH_NKCommodityCode);

			Env.Registry.CommodityCode = Guid.NewGuid();
			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.AIR);
			AssertEquals("Unknown commodity code", "", entry2.TI_RH_NKCommodityCode);

			Env.Registry.CommodityCode = code2.PK.ToGuid();
			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.AIR);
			AssertEquals("Correct commodity code", "COD2", entry3.TI_RH_NKCommodityCode);

			var cwdEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.CYD);
			AssertEquals("Blank for Container Yard", "", cwdEntry1.TI_RH_NKCommodityCode);
			AssertEquals("Freight mode is all for container yard entry", Core.Constants.RateMode.ALL, cwdEntry1.TI_Mode);
		}

		public void TestAdditionalCompanyTariffs()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			tariff1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			tariff1.AddRateEntry("AIR", "LSE", "AUMEL", "USLAX");

			Assert("1st Tariff created is Level One Tariff", tariff1.IsLevelOneTariff());
			AssertEquals("1st Tariff has correct reference to Level 1 Tariff (itself)", tariff1, tariff1.LevelOneTariff);

			var airRateEntries1 = tariff1.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR);
			Assert("Rate Entries are NOT readonly in Level 1 tariff", !airRateEntries1[0].TI_OriginLRCInfo.ReadOnly);
			Assert("Rate Entries are NOT readonly in Level 1 tariff", !airRateEntries1[1].TI_OriginLRCInfo.ReadOnly);
			Assert("Rate Entries collection allows new entries", airRateEntries1.AllowNew);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();

			var tariff2 = factory2.New<CompanyTariff>();
			Assert("2nd Tariff created is an Additional Tariff", tariff2.IsAdditionalTariff());
			AssertEquals("2nd Tariff has correct reference to Level 1 Tariff", tariff1.PK, tariff2.LevelOneTariff.PK);

			var airRateEntries2 = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR);
			AssertEquals("2 rate entries from level 1 are part of level 2 tariff's entry collection", 2, airRateEntries2.Count);
			Assert("Rate Entries are readonly in Level 2 tariff", airRateEntries2[0].TI_OriginLRCInfo.ReadOnly);
			Assert("Rate Entries are readonly in Level 2 tariff", airRateEntries2[1].TI_OriginLRCInfo.ReadOnly);
			Assert("Rate Entries collection DOESNT allow new entries", !airRateEntries2.AllowNew);
			Assert("Rate Lines collection does not allow new entries", !airRateEntries2[0].RateLines.AllowNew);
			Assert("Rate Lines collection does not allow new entries", !airRateEntries2[1].RateLines.AllowNew);
		}

		public void TestRateLineAreNotCreatedForNewlyAddedEntry()
		{
			var tariff = Factory.New<CompanyTariff>();

			var support = tariff as ISupportDataImporting;
			AssertEquals("Tariff 's Is importingData is false", false, support.IsImportingData);

			var entry1 = tariff.AddRateEntry("FCL", "LSE", "AU", "HK");
			AssertEquals("RateLine are created automatically for entry1", true, entry1.RateLines.Count > 0);

			support.IsImportingData = true;

			entry1 = tariff.AddRateEntry("FCL", "LSE", "AU", "HK");
			AssertEquals("RateLine are NOT created automatically for entry1 during data import", true, entry1.RateLines.Count == 0);
		}

		public void TestNoDuplicateEntries()
		{
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "9XX";
			airline.RM_ThreeLetterCode = "EKK";
			airline.RM_TwoCharacterCode = "EK";

			TransportProvider1.OH_IsAirLine = true;
			TransportProvider1.MiscServ.OM_RM_Airline = airline.PK;

			Factory.Save();

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var airRateEntry1 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON", "STD", "");
			airRateEntry1.TI_ViaLRC = "SGSIN";
			airRateEntry1.TI_OH_TransportProvider = TransportProvider1.PK;
			airRateEntry1.TI_RateStartDate = ZDate.Today;
			airRateEntry1.TI_RateEndDate = ZDate.Today.AddDays(180);
			var airRateEntry2 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON", "STD", "");
			airRateEntry2.TI_ViaLRC = "SGSIN";
			airRateEntry2.TI_OH_TransportProvider = TransportProvider1.PK;
			airRateEntry2.TI_RateStartDate = ZDate.Today;
			airRateEntry2.TI_RateEndDate = ZDate.Today.AddDays(180);

			testRate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR);

			AssertNoRowErrors(airRateEntry1);
			testRate.RunPreSaveValidation();
			AssertHasRowError(airRateEntry1, ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(airRateEntry2, ErrorMessages.OverlappingDatesOnRateEntry);

			airRateEntry2.TI_RateStartDate = ZDate.Today.AddDays(181);
			airRateEntry2.TI_RateEndDate = ZDate.Today.AddDays(360);

			testRate.RunPreSaveValidation();
			AssertNoRowErrors(airRateEntry1);
			AssertNoRowErrors(airRateEntry2);

			airRateEntry2.TI_RateStartDate = ZDate.Today;
			airRateEntry2.TI_RateEndDate = ZDate.Today.AddDays(180);

			testRate.RunPreSaveValidation();
			AssertHasRowError(airRateEntry1, ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(airRateEntry2, ErrorMessages.OverlappingDatesOnRateEntry);

			airRateEntry1.TI_OriginLRC = "AUMEL";
			testRate.RunPreSaveValidation();
			AssertNoRowErrors(airRateEntry1);
			AssertNoRowErrors(airRateEntry2);

			airRateEntry1.TI_OriginLRC = "AUSYD";
			airRateEntry2.TI_RateStartDate = ZDate.Today.AddDays(5);
			airRateEntry2.TI_RateEndDate = ZDate.Today.AddDays(185);

			testRate.RunPreSaveValidation();
			AssertHasRowError(airRateEntry1, ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(airRateEntry2, ErrorMessages.OverlappingDatesOnRateEntry);

			airRateEntry2.TI_RateStartDate = ZDate.Today.AddDays(-5);
			airRateEntry2.TI_RateEndDate = ZDate.Today.AddDays(175);

			testRate.RunPreSaveValidation();
			AssertHasRowError(airRateEntry1, ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(airRateEntry2, ErrorMessages.OverlappingDatesOnRateEntry);

			airRateEntry1.TI_PlannedLoadLRC = "AUSYD";
			testRate.RunPreSaveValidation();
			AssertNoRowErrors(airRateEntry1);
			AssertNoRowErrors(airRateEntry2);

			airRateEntry2.TI_PlannedDischargeLRC = "GBLON";
			testRate.RunPreSaveValidation();
			AssertNoRowErrors(airRateEntry1);
			AssertNoRowErrors(airRateEntry2);

			airRateEntry1.TI_PlannedLoadLRC = "AUSYD";
			airRateEntry1.TI_PlannedDischargeLRC = "GBLON";
			testRate.RunPreSaveValidation();
			AssertNoRowErrors(airRateEntry1);
			AssertNoRowErrors(airRateEntry2);

			airRateEntry2.TI_PlannedLoadLRC = "AUSYD";
			testRate.RunPreSaveValidation();
			AssertHasRowError(airRateEntry1, ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(airRateEntry2, ErrorMessages.OverlappingDatesOnRateEntry);

			airRateEntry2.TI_PlannedLoadLRC = "AUMEL";
			testRate.RunPreSaveValidation();
			AssertNoRowErrors(airRateEntry1);
			AssertNoRowErrors(airRateEntry2);

			airRateEntry2.TI_PlannedLoadLRC = "AUSYD";
			airRateEntry1.TI_FMCTariffID = "abcd";
			airRateEntry2.TI_FMCTariffID = "abcd";
			testRate.RunPreSaveValidation();
			AssertHasRowError(airRateEntry1, ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(airRateEntry2, ErrorMessages.OverlappingDatesOnRateEntry);

			airRateEntry2.TI_FMCTariffID = "";
			testRate.RunPreSaveValidation();
			AssertNoRowErrors(airRateEntry1);
			AssertNoRowErrors(airRateEntry2);
		}

		public void TestDuplicatingAnEntryCausesValidationError()
		{
			var baseTariff = Helper.NewCompanyTariff();
			var airRateEntry = baseTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "GB");
			airRateEntry.TI_RateStartDate = ZDate.Today;
			airRateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			var airRateEntryCollection = baseTariff.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR);
			AssertNoRowErrors(airRateEntry);

			var rateEntryClone = airRateEntry.Clone(airRateEntryCollection);
			baseTariff.RunPreSaveValidation();

			AssertHasRowError(airRateEntry, ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rateEntryClone, ErrorMessages.OverlappingDatesOnRateEntry);
			Assert("Tariff should have errors", baseTariff.HasErrors);

			rateEntryClone.TI_OriginLRC = "UA";
			baseTariff.RunPreSaveValidation();

			AssertNoRowError(airRateEntry, ErrorMessages.OverlappingDatesOnRateEntry);
			AssertNoRowError(rateEntryClone, ErrorMessages.OverlappingDatesOnRateEntry);
			Assert("Clone is no longer identical to original rate line", !baseTariff.HasErrors);
		}

		public void TestTypeOfElementErrorOnParentDelete()
		{
			var costing = Factory.New<Costing>();
			var collection = new DSTRateEntryCollection(costing, Factory);
			AssertEquals("TypeOfElements", typeof(RateEntry), collection.TypeOfElements);

			costing.Delete();
			AssertEquals("TypeOfElements", typeof(RateEntry), collection.TypeOfElements);
		}

		public void TestTypeOfElement()
		{
			var header = Factory.New<ClientRate>() as RatingHeader;
			var sellRateCollection = new DSTRateEntryCollection(header, Factory);
			AssertEquals("SellRateCollection.TypeOfElements", typeof(RateEntry), sellRateCollection.TypeOfElements);

			header = Factory.New<CompanyTariff>();
			var globalRateCollection = new DSTRateEntryCollection(header, Factory);
			AssertEquals("GlobalRateCollection.TypeOfElements", typeof(RateEntry), globalRateCollection.TypeOfElements);

			header = Factory.New<Costing>();
			var costRateCollection = new DSTRateEntryCollection(header, Factory);
			AssertEquals("CostRateCollection.TypeOfElements", typeof(RateEntry), costRateCollection.TypeOfElements);

			header = Factory.New<Quote>();
			var quoteEntryCollection = new DSTRateEntryCollection(header, Factory);
			AssertEquals("QuoteEntryCollection.TypeOfElements", typeof(QuoteEntry), quoteEntryCollection.TypeOfElements);
		}

		public void TestLineOrder()
		{
			var testQuote = Factory.New<Quote>();
			foreach (RateEntryCollection entryCollection in testQuote.EntryCollectionsExcludingSummary.Values)
			{
				for (short i = 0; i < 5; i++)
				{
					AssertEquals(i, entryCollection.AddNew().TI_LineOrder);
				}
			}
		}

		public void TestCostRateEndDateUsesRegistrySetting()
		{
			var header = Factory.New<Costing>();
			var entry = header.AddRateEntry("DST");

			var monthsTillExpiry = Env.Registry.Rating.CostRateValidityPeriod;
			var date = new ZDateTime(ZDateTime.Today.AddMonths(monthsTillExpiry));
			if (monthsTillExpiry == 0)
			{
				date = ZDateTime.Empty;
			}

			AssertEquals("Costing end date is the default specified in the registry", date, entry.TI_RateEndDate);
		}

		public void TestGlobalTariffEndDateUsesRegistrySetting()
		{
			var header = Factory.New<CompanyTariff>();
			var entry = header.AddRateEntry("DST");

			var monthsTillExpiry = Env.Registry.Rating.GlobalTariffValidityPeriod;
			var date = new ZDateTime(ZDateTime.Today.AddMonths(monthsTillExpiry));
			if (monthsTillExpiry == 0)
			{
				date = ZDateTime.Empty;
			}

			AssertEquals("Global Tariff end date is the default specified in the registry", date, entry.TI_RateEndDate);
		}

		public void TestRateStartAndEndDatesSetOnOneOffQuotesOrRatingHeaderTypeQuote()
		{
			var testQuote = Factory.NewWithValidTestData<RatingHeader>();
			testQuote.TH_QuoteDate = ZDate.Today;
			testQuote.TH_QuoteEndDate = testQuote.DefaultQuoteEndDate;
			testQuote.TH_QuoteNumber = ZString.Empty;
			testQuote.TH_OneTimeQuote = true;
			testQuote.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;
			foreach (RateEntryCollection collection in testQuote.EntryCollectionsExcludingSummary.Values)
			{
				var entry = collection.AddNew();
				AssertEquals("Rate Start Date", ZDateTime.Today, entry.TI_RateStartDate);
				AssertEquals("Rate End Date", entry.DefaultRateEndDate, entry.TI_RateEndDate);
			}
			Factory.Save();
			testQuote.TH_OneTimeQuote = false;
			testQuote.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;
			foreach (RateEntryCollection collection in testQuote.EntryCollectionsExcludingSummary.Values)
			{
				var entry = collection[0];
				AssertEquals("Rate Start Date", ZDateTime.Today, entry.TI_RateStartDate);
				AssertEquals("Rate End Date", entry.DefaultRateEndDate, entry.TI_RateEndDate);
			}
			Factory.Save();
			testQuote.TH_OneTimeQuote = false;
			testQuote.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;
			foreach (RateEntryCollection collection in testQuote.EntryCollectionsExcludingSummary.Values)
			{
				var entry = collection[0];
				AssertEquals("Rate Start Date", ZDateTime.Today, entry.TI_RateStartDate);
				AssertEquals("Rate End Date", entry.DefaultRateEndDate, entry.TI_RateEndDate);
			}
			Factory.Save();
			testQuote.TH_OneTimeQuote = false;
			testQuote.TH_RateType = RatingConstants.RatingHeaderTypes.Quote;
			testQuote.TH_QuoteNumber = "QTE00001";
			foreach (RateEntryCollection collection in testQuote.EntryCollectionsExcludingSummary.Values)
			{
				var entry = collection.AddNew();
				AssertEquals("Rate Start Date", ZDateTime.Today, entry.TI_RateStartDate);
				AssertEquals("Rate End Date", entry.DefaultRateEndDate, entry.TI_RateEndDate);
			}
		}

		public void TestDefaultEntryCurrency()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			company1.Branches.Add(branch1);
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "UA");
			company1.GC_RN_NKCountryCode = country.Code;
			company1.GC_RX_NKLocalCurrency = country.RN_RX_NKLocalCurrency;
			Factory.Save();

			ClientRate rate;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				rate = Helper.NewClientRate(Helper.NewOrgHeader());
			}

			AssertEquals("UAH", rate.AddRateEntry(RatingConstants.RateCategory.AIR).TI_RX_NKCurrency);
		}

		public void TestDefaultEntryCurrency_ForGlobalCosting()
		{
			var costing = Helper.NewGlobalCosting(Helper.NewOrgHeader());
			AssertEquals("USD", costing.AddRateEntry(RatingConstants.RateCategory.AIR).TI_RX_NKCurrency);
		}

		public void TestAddNew_DefaultsPublisher()
		{
			void AssertEntryPublisher(RatingHeader header, string category, ZGuid expectedCompanyPK)
			{
				var rateEntry = header.AddRateEntry(category);

				AssertEquals("Expected company", expectedCompanyPK, rateEntry.TI_GC_Publisher);
				AssertNoErrors("Should always default something valid", rateEntry.TI_GC_PublisherInfo);
			}

			var currentCompanyPK = Env.CurrentCompanyPK;
			var anotherCompanyPK = Factory.NewWithValidTestData<GlbCompany>().PK;
			Factory.Save();

			var localCosting = Helper.NewCosting(Helper.NewOrgHeader());
			AssertEntryPublisher(localCosting, RatingConstants.RateCategory.AIR, currentCompanyPK);
			AssertEntryPublisher(localCosting, RatingConstants.RateCategory.WHS, currentCompanyPK);
			AssertEntryPublisher(localCosting, RatingConstants.RateCategory.ORG, currentCompanyPK);

			localCosting.TH_GC = anotherCompanyPK;
			AssertEntryPublisher(localCosting, RatingConstants.RateCategory.AIR, anotherCompanyPK);
			AssertEntryPublisher(localCosting, RatingConstants.RateCategory.FCL, anotherCompanyPK);

			var globalCosting = Helper.NewGlobalCosting(Helper.NewOrgHeader());
			Assert("Pre-condition", globalCosting.TH_GC.IsEmpty);
			AssertEntryPublisher(globalCosting, RatingConstants.RateCategory.AIR, currentCompanyPK);
			AssertEntryPublisher(globalCosting, RatingConstants.RateCategory.DST, currentCompanyPK);

			var localClientRate = Helper.NewCosting(Helper.NewOrgHeader());
			AssertEntryPublisher(localClientRate, RatingConstants.RateCategory.AIR, currentCompanyPK);
			AssertEntryPublisher(localClientRate, RatingConstants.RateCategory.SCO, currentCompanyPK);
			AssertEntryPublisher(localClientRate, RatingConstants.RateCategory.TRN, currentCompanyPK);

			localClientRate.TH_GC = anotherCompanyPK;
			AssertEntryPublisher(localClientRate, RatingConstants.RateCategory.AIR, anotherCompanyPK);
			AssertEntryPublisher(localClientRate, RatingConstants.RateCategory.PAC, anotherCompanyPK);

			var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
			Assert("Pre-condition", globalClientRate.TH_GC.IsEmpty);
			AssertEntryPublisher(globalClientRate, RatingConstants.RateCategory.AIR, currentCompanyPK);

			var globalTariff = Helper.NewGlobalTariff();
			Assert("Pre-condition", globalTariff.TH_GC.IsEmpty);
			AssertEntryPublisher(globalTariff, RatingConstants.RateCategory.AIR, currentCompanyPK);
		}

		public void TestOnRemovedDoesntLoadAllEntries()
		{
			var rate = Factory.NewWithValidTestData<ClientRate>();
			rate.TH_OH = NewClient.PK;

			var rateEntry1 = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "FRT", 40m);
			var rateEntry2 = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "CNSHA", "FRT", 40m);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var newFactoryRate = newFactory.LoadTop1<ClientRate>(new ZQuery(RatingHeaderSchema.PK, rate.PK));

			newFactory.ResetDatabaseLoadCount();

			var rateEntryToRemove = newFactoryRate.FCLRateEntriesForBinding[0];

			newFactoryRate.FCLRateEntriesForBinding.Remove(rateEntryToRemove);

			var tableHitDictionary = new Dictionary<string, int>
			{
				{ RateEntrySchema.Constants.TableName, 1 },
				{ RatingHeaderSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
			};

			AssertDbHits(tableHitDictionary, newFactory);
		}

		public void TestRateEntryCollectionDefaultChargeCodes_SetsUnitWhenWritable()
		{
			var chargeCode = Factory.Load<AccChargeCode>(Helper.ChargeCodes["ODOC"].PK);
			chargeCode.AC_RateCalculator = UnitCalculator.Code;

			var locationCharges = new LocationsChargesCollection();
			Helper.AddLocationCharge(locationCharges, "US", chargeCode.PK);

			Factory.Save();

			using (RatingDataRegistry.Instance.AirOriginDefaultChargeCodes
				.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, locationCharges))
			{
				var rate = Factory.NewWithValidTestData<ClientRate>();
				var entry = Factory.New<RateEntry>();
				entry.TI_OriginLRC = "US";
				entry.TI_Mode = "ULD";
				rate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Add(entry);
				var line = entry.RateLines[0];

				AssertNotEquals("Rate entry unit is not empty", ZString.Empty, entry.Unit);
				AssertEquals("Charge code is ODOC", Helper.ChargeCodes["ODOC"], line.ChargeCode);
				AssertType<UnitCalculator>("Calculator is UNT", line.Calculator);

				AssertNotEquals("Rate line unit is not empty", ZString.Empty, line.TL_WeightVolume);
			}
		}

		public void TestRateEntryCollectionDefaultChargeCodes_DoesNotSetReadOnlyUnit()
		{
			var chargeCode = Factory.Load<AccChargeCode>(Helper.ChargeCodes["ODOC"].PK);
			chargeCode.AC_RateCalculator = FlatCalculator.Code;

			var locationCharges = new LocationsChargesCollection();
			Helper.AddLocationCharge(locationCharges, "US", chargeCode.PK);

			Factory.Save();

			using (RatingDataRegistry.Instance.AirOriginDefaultChargeCodes
				.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, locationCharges))
			{
				var rate = Factory.NewWithValidTestData<ClientRate>();
				var entry = Factory.New<RateEntry>();
				entry.TI_OriginLRC = "US";
				entry.TI_Mode = "ULD";
				rate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Add(entry);
				var line = entry.RateLines[0];

				AssertNotEquals("Rate entry unit is not empty", ZString.Empty, entry.Unit);
				AssertEquals("Charge code is ODOC", Helper.ChargeCodes["ODOC"], line.ChargeCode);
				AssertType<FlatCalculator>("Calculator is FLT", line.Calculator);

				AssertEquals("Unit is read-only", true, line.TL_WeightVolumeInfo.ReadOnly);
				AssertEquals("Rate line unit is not empty", ZString.Empty, line.TL_WeightVolume);
			}
		}

		public void TestRunPreSaveValidation_SomeEntriesHaveErrors_MoveEntriesWithErrorsOnTop()
		{
			var clientRate = Helper.NewClientRate(NewClient);

			var category = RatingConstants.RateCategory.AIR;
			var entry1 = clientRate.AddRateEntryWithFlatRateLine(category, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100);
			var entry2 = clientRate.AddRateEntryWithFlatRateLine(category, RateMode.LSE, "AUSYD", "-----", "FRT", 100);
			var entry3 = clientRate.AddRateEntryWithFlatRateLine(category, RateMode.LSE, "AUSYD", "NZAKL", "FRT", 100);
			var entry4 = clientRate.AddRateEntryWithFlatRateLine(category, RateMode.LSE, "AUSYD", "SGSIN", "FRT", 100);
			var entry5 = clientRate.AddRateEntryWithFlatRateLine(category, RateMode.LSE, "AUSYD", "*****", "FRT", 100);

			AssertSortedCollectionAfterValidation(
				ratingHeader: clientRate,
				entriesWithErrors: new[] { entry2, entry5 },
				entriesWithRowErrors: Array.Empty<RateEntry>(),
				entriesWithoutErrors: new[] { entry1, entry3, entry4 });
		}

		public void TestRunPreSaveValidation_SomeEntriesHaveRowErrors_MoveEntriesWithErrorsOnTop()
		{
			var clientRate = Helper.NewClientRate(NewClient);

			var category = RatingConstants.RateCategory.AIR;
			var entry1 = clientRate.AddRateEntryWithFlatRateLine(category, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100);
			var entry2 = clientRate.AddRateEntryWithFlatRateLine(category, RateMode.LSE, "AUSYD", "AUMEL", "FRT", 100);
			var entry3 = clientRate.AddRateEntryWithFlatRateLine(category, RateMode.LSE, "AUSYD", "NZAKL", "FRT", 100);
			var entry4 = clientRate.AddRateEntryWithFlatRateLine(category, RateMode.LSE, "AUSYD", "SGSIN", "FRT", 100);
			var entry5 = clientRate.AddRateEntryWithFlatRateLine(category, RateMode.LSE, "AUSYD", "AUMEL", "FRT", 100);

			AssertSortedCollectionAfterValidation(
				ratingHeader: clientRate,
				entriesWithErrors: Array.Empty<RateEntry>(),
				entriesWithRowErrors: new[] { entry2, entry5 },
				entriesWithoutErrors: new[] { entry1, entry3, entry4 });
		}

		void AssertSortedCollectionAfterValidation(
			RatingHeader ratingHeader,
			IEnumerable<RateEntry> entriesWithErrors,
			IEnumerable<RateEntry> entriesWithRowErrors,
			IEnumerable<RateEntry> entriesWithoutErrors,
			string category = RatingConstants.RateCategory.AIR)
		{
			ratingHeader.RunPreSaveValidation();

			foreach (var entry in entriesWithErrors)
			{
				Assert("Pre-condition: RateEntries with HasErrors", entry.HasErrors);
			}

			foreach (var entry in entriesWithRowErrors)
			{
				Assert("Pre-condition: RateEntries with RowErrors", entry.HasRowErrors);
			}

			foreach (var entry in entriesWithoutErrors)
			{
				Assert("Pre-condition: RateEntries without errors", !(entry.HasErrors || entry.HasRowErrors));
			}

			var collection = ratingHeader.EntryCollections[category].LazyLoadingCollection;

			var message = "Collection must be sorted after validation and RateEntries with errors must come first";
			var expected = entriesWithErrors.Union(entriesWithRowErrors).ToArray();
			var entriesWithErrorsCount = expected.Length;
			var actual = collection.Take(entriesWithErrorsCount).ToArray();
			AssertContainsExactElementsInAnyOrder(message, expected, actual);

			message = "The rest of the rates should have no errors and should appear after those with errors";
			expected = entriesWithoutErrors.ToArray();
			actual = collection.Skip(entriesWithErrorsCount).ToArray();
			AssertContainsExactElementsInAnyOrder(message, expected, actual);
		}

		public void TestIImportCollectionElementMatchingSupporter_Defaults()
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var fclCollection = clientRate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.FCL);

			var matcher = fclCollection as IImportCollectionElementMatchingSupporter;

			AssertEquals(string.Empty, matcher.MatchingColumnName);
			AssertEquals(true, matcher.IsGenericColumnMatchingAllowed);
			AssertNull(matcher.GetMatchingBizObject("whatever"));
			AssertNull(matcher.GetMatchingBizObject(string.Empty));
		}
	}
}
