using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Rating.Business.Testing
{
	public class RateCreatorTest : RatingTestCase
	{
		#region Create Globally Published Rates From Local Rates

		public void TestLoadOrCreateGlobalRatingHeader_CreateGlobalClientRate()
		{
			var isAllowed = Env.Security.GlobalClientRatesNew.IsAllowed;

			try
			{
				var chargeCodeCode = "GLBCHRG";
				Helper.ChargeCodes.CreateGlobalCharge(chargeCodeCode);
				Factory.Save();

				var client = Helper.NewOrgHeader();
				var clientRate = Helper.NewClientRate(client);
				var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "CN");
				rateEntry.RateLines.RemoveAndDeleteAll();
				var rateLine = rateEntry.AddRateLine(chargeCodeCode, UnitCalculator.Code, QuantityUnit.KG);
				rateLine.GetCalculator<UnitCalculator>().PerUnit = 40m;

				var rateCreator = new RateCreator(clientRate);

				Env.Security.GlobalClientRatesNew.IsAllowed = false;

				AssertNull(rateCreator.LoadOrCreateGlobalRatingHeader(Factory));

				Env.Security.GlobalClientRatesNew.IsAllowed = true;

				var globalClientRate = rateCreator.LoadOrCreateGlobalRatingHeader(Factory);

				Assert("Pre-condition", globalClientRate.IsGlobalClientRate());
				AssertEquals(clientRate.TH_OH, globalClientRate.TH_OH);
			}
			finally
			{
				Env.Security.GlobalClientRatesNew.IsAllowed = isAllowed;
			}
		}

		public void TestLoadOrCreateGlobalRatingHeader_DoesNotDefaultClientRate()
		{
			var chargeCodeCode = "GLBCHRG";
			Helper.ChargeCodes.CreateGlobalCharge(chargeCodeCode);
			Factory.Save();

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "CN");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine(chargeCodeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 40m;

			var rateCreator = new RateCreator(clientRate);
			AssertNull("Expected not to have created the rate", rateCreator.LoadGlobalRatingHeader(Factory));
		}

		public void TestLoadOrCreateGlobalRatingHeader_CreateGlobalCosting()
		{
			var isAllowed = Env.Security.GlobalCostingRatesNew.IsAllowed;

			try
			{
				var chargeCodeCode = "GLBCHRG";
				Helper.ChargeCodes.CreateGlobalCharge(chargeCodeCode);
				Factory.Save();

				var serviceProvider = Helper.NewOrgHeader();
				var costing = Helper.NewCosting(serviceProvider);
				var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "GB", "AU");
				costEntry.RateLines.RemoveAndDeleteAll();
				var costLine = costEntry.AddRateLine(chargeCodeCode, UnitCalculator.Code, QuantityUnit.M3);
				costLine.GetCalculator<UnitCalculator>().PerUnit = 1000m;

				var rateCreator = new RateCreator(costing);

				Env.Security.GlobalCostingRatesNew.IsAllowed = false;

				AssertNull(rateCreator.LoadOrCreateGlobalRatingHeader(Factory));

				Env.Security.GlobalCostingRatesNew.IsAllowed = true;

				var globalCosting = rateCreator.LoadOrCreateGlobalRatingHeader(Factory);

				Assert(globalCosting.IsGlobalCostRate());
				AssertEquals(serviceProvider.PK, globalCosting.TH_OH);
				AssertNotEquals(costing.PK, globalCosting.PK);
			}
			finally
			{
				Env.Security.GlobalCostingRatesNew.IsAllowed = isAllowed;
			}
		}

		public void TestLoadOrCreateGlobalRatingHeader_CreateGlobalTariff()
		{
			var isAllowed = Env.Security.GlobalTariffRatesNew.IsAllowed;

			try
			{
				var companyTariff = Factory.New<CompanyTariff>();

				Assert("Pre-condition", companyTariff.IsLevelOneTariff());

				var rateCreator = new RateCreator(companyTariff);

				Env.Security.GlobalTariffRatesNew.IsAllowed = false;

				AssertNull(rateCreator.LoadOrCreateGlobalRatingHeader(Factory));

				Env.Security.GlobalTariffRatesNew.IsAllowed = true;

				var globalTariff = rateCreator.LoadOrCreateGlobalRatingHeader(Factory);

				Assert(globalTariff.IsGlobalTariff());
				Assert(globalTariff.IsLevelOneTariff());
				AssertNotEquals(companyTariff.PK, globalTariff.PK);

				var additionalCompanyTariff = Factory.New<CompanyTariff>();
				Assert("Pre-condition", additionalCompanyTariff.IsAdditionalTariff());

				rateCreator = new RateCreator(additionalCompanyTariff);
				AssertNull("Additional Tariffs should not load or create global costings", rateCreator.LoadGlobalRatingHeader(Factory));
			}
			finally
			{
				Env.Security.GlobalTariffRatesNew.IsAllowed = isAllowed;
			}
		}

		public void TestLoadOrCreateGlobalRatingHeader_FindsExistingGlobalClientRate()
		{
			var client = Helper.NewOrgHeader();
			var globalClientRate = Helper.NewGlobalClientRate(client);
			Factory.Save();

			var clientRate = Helper.NewClientRate(client);
			var rateCreator = new RateCreator(clientRate);

			AssertEquals(globalClientRate.PK, rateCreator.LoadGlobalRatingHeader(Factory).PK);
		}

		public void TestLoadOrCreateGlobalRatingHeader_FindsExistingGlobalTariff()
		{
			var globalTariff = Factory.New<GlobalTariff>();
			Factory.Save();

			Assert("Pre-condition", globalTariff.IsLevelOneTariff());

			var companyTariff = Factory.New<CompanyTariff>();
			Assert("Pre-condition", companyTariff.IsLevelOneTariff());
			Assert("Pre-condition", globalTariff.IsLevelOneTariff());

			var rateCreator = new RateCreator(companyTariff);
			AssertEquals(globalTariff.PK, rateCreator.LoadGlobalRatingHeader(Factory).PK);
		}

		#endregion

		#region Create Quote From Existing Rate Entries

		public void TestCreateQuoteFromClientRate()
		{
			var rate = Factory.NewWithValidTestData<ClientRate>();
			var entry1 = rate.AddRateEntry("AIR", "LSE", "INBOM", "US");
			var rateLine1 = entry1.RateLines[0];
			rateLine1.TL_RateCalculator = UnitCalculator.Code;
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 13m;

			rate.AddRateEntry("AIR", "LSE", "AU", "USLAX");
			rate.AddRateEntry("ORG", "ALL", "INBOM", "US");
			rate.AddRateEntry("ORG", "LSE", "INBOM", "USLAX");
			var entry5 = rate.AddRateEntry("ORG", "LSE", "AUSYD", "USLAX");

			// Create a quote from a Freight entry
			var quote = GetRatingHeaderCreator(rate).CreateQuoteFromExistingRateEntries(new BusinessObject[] { entry1 }, true);
			var airRateEntries = quote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR);
			var orgRateEntries = quote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG);

			AssertEquals("Client rate still only has 2 Air Entries", 2, rate.AIRRateEntriesForBinding.Count);
			AssertEquals("1 Air Entry in Quote", 1, airRateEntries.Count);
			AssertEquals("Entry is of type QuoteEntry", typeof(QuoteEntry), airRateEntries[0].GetType());
			AssertEquals("Correct Air Entry in Quote", "INBOM", airRateEntries[0].TI_OriginLRC);
			AssertEquals("Rate Line copied with same charge code", "FRT", airRateEntries[0].RateLines[0].ChargeCode.AC_Code);
			AssertEquals("Same calc", UnitCalculator.Code, airRateEntries[0].RateLines[0].TL_RateCalculator);
			AssertEquals("1 item only", 1, airRateEntries[0].RateLines[0].RateLineItems.Count);
			AssertEquals("Correct value", 13m, ((UnitCalculator)airRateEntries[0].RateLines[0].Calculator).PerUnit);

			AssertEquals("2 related Origin Entries in Quote", 2, orgRateEntries.Count);

			// Create a quote from an Origin entry
			quote = GetRatingHeaderCreator(rate).CreateQuoteFromExistingRateEntries(new BusinessObject[] { entry5 }, true);
			airRateEntries = quote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR);
			orgRateEntries = quote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG);

			AssertEquals("1 Air Entry in Quote", 1, airRateEntries.Count);
			AssertEquals("1 related Air Entry in Quote", "AU", airRateEntries[0].TI_OriginLRC);

			AssertEquals("1 Origin Entries in Quote", 1, orgRateEntries.Count);
			AssertEquals("Correct Origin Entry in Quote", "AUSYD", orgRateEntries[0].TI_OriginLRC);
		}

		public void TestCreateQuoteFromClientRateWithMultipleFreightMatchingSingleOrigin()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry1 = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var rateEntry2 = clientRate.AddRateEntry("AIR", "LSE", "AUMEL", "USLAX");
			var rateEntry3 = clientRate.AddRateEntry("ORG", "ALL", "AU", "US");
			rateEntry3.AddRateLine("ODOC");

			var testQuote = GetRatingHeaderCreator(clientRate).CreateQuoteFromExistingRateEntries(new BusinessObject[] { rateEntry1, rateEntry2 }, true);

			AssertEquals("2 Air Entries in Quote", 2, testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR).Count);
			AssertEquals("Only 1 Origin Entry", 1, testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Count);
		}

		public void TestCreateQuoteFromClientRateWithNoRelatedEntries()
		{
			var rate = Factory.New<ClientRate>();
			var entry1 = rate.AddRateEntry("AIR", "LSE", "INBOM", "US");
			var entry2 = rate.AddRateEntry("AIR", "LSE", "AU", "USLAX");
			var entry3 = rate.AddRateEntry("ORG", "ALL", "INBOM", "US");
			var entry4 = rate.AddRateEntry("ORG", "LSE", "INBOM", "USLAX");
			var entry5 = rate.AddRateEntry("ORG", "LSE", "AUSYD", "USLAX");

			var testQuote = GetRatingHeaderCreator(rate).CreateQuoteFromExistingRateEntries(new BusinessObject[] { entry1, entry2 }, false);

			AssertEquals("2 Air entries in quote", 2, testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR).Count);
			AssertEquals("No origin entry", 0, testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Count);
		}

		public void TestCreateQuoteFromCompanyTariff()
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry1 = tariff.AddRateEntry("AIR", "LSE", "INBOM", "US");
			var entry2 = tariff.AddRateEntry("ORG", "ALL", "INBOM", "US");
			entry2.AddRateLine("ODOC");

			var testQuote = GetRatingHeaderCreator(tariff).CreateQuoteFromExistingRateEntries(new BusinessObject[] { entry1 }, true);

			AssertEquals("1 Air Entry in Quote", 1, testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR).Count);
			AssertEquals("Correct Air Entry in Quote", entry1.TI_OriginLRC, testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR)[0].TI_OriginLRC);
			AssertEquals("Rate Line copied but with Company Tariff based calc", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR)[0].RateLines[0].TL_RateCalculator);
			AssertEquals("1 related Origin Entry in Quote", 1, testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Count);
		}

		#endregion

		#region Implementation

		static RateCreator GetRatingHeaderCreator(RatingHeader ratingHeader)
		{
			return new RateCreator(ratingHeader);
		}

		#endregion
	}
}
