using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Rating.Business.Testing
{
	public class RateCommodityFMCPairProviderTest : RatingTestCase
	{
		#region Commodity matching related tests

		public void TestGetMatch_ExactCommodityMatch()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);

			var companyTariffLevel = Helper.NewCompanyTariff().LevelOneTariff;
			CreateRate(companyTariffLevel, "BBB");

			var criteria = GetCriteria(NewClient, "BBB");
			var loadOptions = RevenueLoadOptions.Tariffs;
			var provider = new RateCommodityFMCPairProvider(Factory, FMCTestLogger);

			var matches = provider.GetMatches(criteria).Select(x => x.CommodityCode).ToArray();
			var expectedMatches = new[] { "BBB" };
			AssertContainsExactElementsInAnyOrder("The matched commodity should only contain 'BBB'", expectedMatches, matches);

			var commodityOnlyMatches = provider.GetCommodityOnlyMatch(criteria, loadOptions).Select(x => x.CommodityCode).ToArray();
			AssertContainsExactElementsInAnyOrder("The commodity-only match should only contain 'BBB'", expectedMatches, commodityOnlyMatches);

			var childCommodityOnlyMatches = provider.GetChildCommodityOnlyMatch(criteria, loadOptions).Select(x => x.CommodityCode).ToArray();
			AssertEquals("There should be no child commodity matches", 0, childCommodityOnlyMatches.Length);

			var siblingCommodityOnlyMatches = provider.GetSiblingCommodityOnlyMatch(criteria, loadOptions).Select(x => x.CommodityCode).ToArray();
			AssertEquals("There should be no sibling commodity matches", 0, siblingCommodityOnlyMatches.Length);
		}

		public void TestGetMatch_ChildCommodityMatch()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);

			var companyTariffLevel = Helper.NewCompanyTariff().LevelOneTariff;
			CreateRate(companyTariffLevel, "BBB1");

			var criteria = GetCriteria(NewClient, "BBB");
			var loadOptions = RevenueLoadOptions.Tariffs;

			var provider = new RateCommodityFMCPairProvider(Factory, FMCTestLogger);

			var getMatchesResult = provider
				.GetMatches(criteria)
				.Select(x => x.CommodityCode)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				new[] { "BBB1" },
				getMatchesResult
			);

			var getCommodityOnlyMatchResult = provider
				.GetCommodityOnlyMatch(criteria, loadOptions)
				.Select(x => x.CommodityCode)
				.ToArray();

			AssertEquals("Expected the collection to be empty.", 0, getCommodityOnlyMatchResult.Length);

			var getChildCommodityOnlyMatchResult = provider
				.GetChildCommodityOnlyMatch(criteria, loadOptions)
				.Select(x => x.CommodityCode)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				new[] { "BBB1" },
				getChildCommodityOnlyMatchResult
			);

			var getSiblingCommodityOnlyMatchResult = provider
				.GetSiblingCommodityOnlyMatch(criteria, loadOptions)
				.Select(x => x.CommodityCode)
				.ToArray();

			AssertEquals("Expected the collection to be empty.", 0, getSiblingCommodityOnlyMatchResult.Length);
		}

		public void TestGetMatch_SiblingCommodityMatch()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);

			var companyTariffLevel = Helper.NewCompanyTariff().LevelOneTariff;
			CreateRate(companyTariffLevel, "BBB1");

			var criteria = GetCriteria(NewClient, "BBB2");
			var loadOptions = RevenueLoadOptions.Tariffs;

			var provider = new RateCommodityFMCPairProvider(Factory, FMCTestLogger);

			var getMatchesResult = provider.GetMatches(criteria).Select(x => x.CommodityCode).ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { "BBB1" }, getMatchesResult);

			var commodityOnlyMatchResult = provider.GetCommodityOnlyMatch(criteria, loadOptions).Select(x => x.CommodityCode).ToArray();
			AssertEquals(0, commodityOnlyMatchResult.Length);

			var childCommodityOnlyMatchResult = provider.GetChildCommodityOnlyMatch(criteria, loadOptions).Select(x => x.CommodityCode).ToArray();
			AssertEquals(0, childCommodityOnlyMatchResult.Length);

			var siblingCommodityOnlyMatchResult = provider.GetSiblingCommodityOnlyMatch(criteria, loadOptions).Select(x => x.CommodityCode).ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { "BBB1" }, siblingCommodityOnlyMatchResult);
		}

		public void TestGetMatch_SiblingCommodityMatch_StepSibling()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } },
				new RatingCode { commodity = "CCC", childCommodities = new[] { "CCC1", "BBB2" } }
			);

			var companyTariffLevel = Helper.NewCompanyTariff().LevelOneTariff;
			CreateRate(companyTariffLevel, "CCC1");
			CreateRate(companyTariffLevel, "BBB1");

			var criteria = GetCriteria(NewClient, "BBB2");
			var loadOptions = RevenueLoadOptions.Tariffs;

			var provider = new RateCommodityFMCPairProvider(Factory, FMCTestLogger);

			var expectedMatches = new[] { "BBB1", "CCC1" };

			// Validate GetMatches
			var actualMatches = provider
				.GetMatches(criteria)
				.Select(x => x.CommodityCode)
				.ToArray();
			AssertContainsExactElementsInAnyOrder("GetMatches should return correct commodity codes", expectedMatches, actualMatches);

			// Validate GetCommodityOnlyMatch
			var actualCommodityOnlyMatch = provider
				.GetCommodityOnlyMatch(criteria, loadOptions)
				.Select(x => x.CommodityCode)
				.ToArray();
			AssertEquals("GetCommodityOnlyMatch should return an empty collection", 0, actualCommodityOnlyMatch.Length);

			// Validate GetChildCommodityOnlyMatch
			var actualChildCommodityOnlyMatch = provider
				.GetChildCommodityOnlyMatch(criteria, loadOptions)
				.Select(x => x.CommodityCode)
				.ToArray();
			AssertEquals("GetChildCommodityOnlyMatch should return an empty collection", 0, actualChildCommodityOnlyMatch.Length);

			// Validate GetSiblingCommodityOnlyMatch
			var actualSiblingCommodityOnlyMatch = provider
				.GetSiblingCommodityOnlyMatch(criteria, loadOptions)
				.Select(x => x.CommodityCode)
				.ToArray();
			AssertContainsExactElementsInAnyOrder("GetSiblingCommodityOnlyMatch should return correct commodity codes", expectedMatches, actualSiblingCommodityOnlyMatch);
		}

		public void TestGetMatch_CompanyTariff()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } },
				new RatingCode { commodity = "CCC", childCommodities = new[] { "CCC1", "BBB2" } }
			);

			var companyTariffLevel = Helper.NewCompanyTariff();
			var rateEntryCollection = companyTariffLevel.FCLRateEntriesForBinding;

			var ccc1 = CreateRate(companyTariffLevel, "CCC1");
			ccc1.TI_FMCTariffID = "1234";
			ccc1.TI_PL_NKCarrierServiceLevel = "STD";
			var ccc1b = ccc1.Clone(rateEntryCollection);
			ccc1b.TI_FMCTariffID = "1234";
			ccc1b.TI_RateDestination = "FR";
			var ccc1c = ccc1.Clone(rateEntryCollection);
			ccc1c.TI_FMCTariffID = "2222";
			ccc1b.TI_RateOrigin = "US";

			var bbb1 = CreateRate(companyTariffLevel, "BBB1");
			bbb1.TI_FMCTariffID = "1234";
			bbb1.TI_PL_NKCarrierServiceLevel = "STD";
			companyTariffLevel.Factory.Save();

			var criteria = GetCriteria(NewClient, "BBB2");
			var loadOptions = RevenueLoadOptions.Tariffs;
			var mockProvider = new Mock<RateCommodityFMCPairProvider>(Factory, FMCTestLogger);
			mockProvider
				.Setup(x =>
					x.GetRateEntries(
						It.IsAny<RatingCriteria>(),
						It.IsAny<ILogger>(),
						It.IsAny<RevenueLoadOptions>()))
				.Returns(new List<IRateEntry>() { ccc1, ccc1b, ccc1c, bbb1 });

			var provider = mockProvider.Object;

			var actualMatches = provider
				.GetMatches(criteria)
				.Select(x => $"{x.CommodityCode}|{x.FMCTariffID}|{x.RateSource}")
				.ToArray();
			var expectedMatches = new[]
			{
				"CCC1|1234|CTR",
				"CCC1|2222|CTR",
				"BBB1|1234|CTR"
			};
			AssertContainsExactElementsInAnyOrder("The 'GetMatches' result should match the expected collection.", expectedMatches, actualMatches);

			var actualSiblingMatches = provider
				.GetSiblingCommodityOnlyMatch(criteria, loadOptions)
				.Select(x => $"{x.CommodityCode}|{x.FMCTariffID}|{x.RateSource}")
				.ToArray();
			var expectedSiblingMatches = new[]
			{
				"CCC1|1234|CTR",
				"CCC1|1234|CTR",
				"CCC1|2222|CTR",
				"BBB1|1234|CTR"
			};
			AssertContainsExactElementsInAnyOrder("The 'GetSiblingCommodityOnlyMatch' result should match the expected collection.", expectedSiblingMatches, actualSiblingMatches);
		}

		public void TestSelectingRateCommodity_NoExceptionWhenNoBaseRate()
		{
			CreateCommodityAndRatingCodesForFMCTest(
				new RatingCode { commodity = "UMBR" }
			);

			var dependentChargeCode = Helper.ChargeCodes.New("DEP", "Dependent Charge Code", "");

			var charge = Factory.NewWithValidTestData<TestCharge>();
			charge.JR_AC = dependentChargeCode.PK;
			charge.JR_LocalSellAmt = 123M;

			var testCharges = new[] { charge };

			var criteria = GetCriteria(NewClient, "UMBR", testCharges: testCharges);

			NewClient.CompanyData.RateTariffLevels.RemoveAndDeleteAll();
			var rateTariffLevel = NewClient.CompanyData.RateTariffLevels.AddNew();
			rateTariffLevel.P7_Mode = "ALL";
			rateTariffLevel.P7_TariffType = "DEF";
			rateTariffLevel.P7_Direction = "IMP";
			rateTariffLevel.P7_TariffLevel = 1;

			var companyTariffLevel = Helper.NewCompanyTariff().LevelOneTariff;
			var rateEntry = CreateRate(companyTariffLevel, "UMBR", otherSetters: (entry) =>
			{
				var percChargeCode = Helper.ChargeCodes.New("TEST", "Test Percentage", PercentageCalculator.Code);
				Factory.Save();

				entry.TI_RX_NKCurrency = "GBP";
				entry.RateLines.RemoveAndDeleteAll();
				var rateLine = entry.AddRateLine(percChargeCode);
				rateLine.TL_RateCalculator = PercentageCalculator.Code;
				rateLine.TL_RX_NKCurrency = "GBP";

				var percentageCalculator = rateLine.GetCalculator<PercentageCalculator>();
				percentageCalculator.Percent = 10m;
				percentageCalculator.BaseRate = 0m;

				var rateLineItem = rateLine.RateLineItems.AddNew();
				rateLineItem.TM_Type = CalculatorConstants.Type.ApplyTo;
				rateLineItem.TM_Text = CalculatorConstants.Text.ChargeCode;
				rateLineItem.TM_AC = dependentChargeCode.PK;
			});

			companyTariffLevel.Factory.Save();

			var provider = new RateCommodityFMCPairProvider(Factory, FMCTestLogger);
			var actualCodes = provider
				.GetMatches(criteria)
				.Select(x => x.CommodityCode)
				.ToArray();

			var expectedCodes = new[] { "UMBR" };

			AssertContainsExactElementsInAnyOrder(
				"Expected matched commodity codes to contain the correct values.",
				expectedCodes,
				actualCodes
			);
		}

		public void TestGetMatch_ClientRate()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } },
				new RatingCode { commodity = "CCC", childCommodities = new[] { "CCC1", "BBB2" } }
			);

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var clientRateEntryCollection = clientRate.FCLRateEntriesForBinding;

			var ccc1 = CreateRate(clientRate, "CCC1");
			ccc1.TI_FMCTariffID = "1234";
			ccc1.TI_PL_NKCarrierServiceLevel = "STD";
			var ccc1b = ccc1.Clone(clientRateEntryCollection);
			ccc1b.TI_FMCTariffID = "1234";
			ccc1b.TI_RateDestination = "FR";
			var ccc1c = ccc1.Clone(clientRateEntryCollection);
			ccc1c.TI_FMCTariffID = "2222";
			ccc1b.TI_RateOrigin = "US";

			var bbb1 = CreateRate(clientRate, "BBB1");
			bbb1.TI_FMCTariffID = "1234";
			bbb1.TI_PL_NKCarrierServiceLevel = "STD";
			clientRate.Factory.Save();

			var criteria = GetCriteria(NewClient, "BBB2");
			var loadOptions = RevenueLoadOptions.ClientRates;

			var mockProvider = new Mock<RateCommodityFMCPairProvider>(Factory, FMCTestLogger);
			mockProvider
				.Setup(x =>
					x.GetRateEntries(
						It.IsAny<RatingCriteria>(),
						It.IsAny<ILogger>(),
						It.IsAny<RevenueLoadOptions>()))
				.Returns(new List<IRateEntry>() { ccc1, ccc1b, ccc1c, bbb1 });

			var provider = mockProvider.Object;

			var actualMatchResults = provider
				.GetMatches(criteria)
				.Select(x => $"{x.CommodityCode}|{x.FMCTariffID}|{x.RateSource}")
				.ToArray();

			var expectedMatchResults = new[]
			{
				"CCC1|1234|CLR",
				"CCC1|2222|CLR",
				"BBB1|1234|CLR"
			};

			AssertContainsExactElementsInAnyOrder(
				"GetMatches results should match the expected collection.",
				expectedMatchResults,
				actualMatchResults
			);

			var actualSiblingResults = provider
				.GetSiblingCommodityOnlyMatch(criteria, loadOptions)
				.Select(x => $"{x.CommodityCode}|{x.FMCTariffID}|{x.RateSource}")
				.ToArray();

			var expectedSiblingResults = new[]
			{
				"CCC1|1234|CLR",
				"CCC1|1234|CLR",
				"CCC1|2222|CLR",
				"BBB1|1234|CLR"
			};

			AssertContainsExactElementsInAnyOrder(
				"GetSiblingCommodityOnlyMatch results should match the expected collection.",
				expectedSiblingResults,
				actualSiblingResults
			);
		}

		public void TestGetMatch_BothRateSources()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);

			var companyTariffLevel = Helper.NewCompanyTariff();
			var companyTariffCollection = companyTariffLevel.FCLRateEntriesForBinding;
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var clientRateEntryCollection = clientRate.FCLRateEntriesForBinding;

			var bbb1CompanyTariff = CreateRate(companyTariffLevel, "BBB1");
			bbb1CompanyTariff.TI_FMCTariffID = "1234";
			bbb1CompanyTariff.TI_PL_NKCarrierServiceLevel = "STD";
			var bbb2CompanyTariff = CreateRate(companyTariffLevel, "BBB2");
			bbb2CompanyTariff.TI_FMCTariffID = "5678";
			bbb2CompanyTariff.TI_PL_NKCarrierServiceLevel = "STD";
			companyTariffLevel.Factory.Save();

			var bbb1ClientRate = CreateRate(clientRate, "BBB1");
			bbb1ClientRate.TI_FMCTariffID = "1234";
			bbb1ClientRate.TI_PL_NKCarrierServiceLevel = "STD";
			var bbb3ClientRate = CreateRate(clientRate, "BBB3");
			bbb3ClientRate.TI_FMCTariffID = "7777";
			bbb3ClientRate.TI_PL_NKCarrierServiceLevel = "STD";
			clientRate.Factory.Save();

			var criteria = GetCriteria(NewClient, "BBB");
			var mockProvider = new Mock<RateCommodityFMCPairProvider>(Factory, FMCTestLogger);
			mockProvider
				.Setup(x => x.GetRateEntries(
						It.IsAny<RatingCriteria>(),
						It.IsAny<ILogger>(),
						It.IsAny<RevenueLoadOptions>()))
				.Returns(new List<IRateEntry>() { bbb1CompanyTariff, bbb2CompanyTariff, bbb1ClientRate, bbb3ClientRate });

			var provider = mockProvider.Object;

			var actual = provider
				.GetMatches(criteria)
				.Select(x => $"{x.CommodityCode}|{x.FMCTariffID}|{x.RateSource}")
				.ToArray();

			var expected = new[]
			{
				"BBB1|1234|BTH",
				"BBB2|5678|CTR",
				"BBB3|7777|CLR",
			};

			AssertContainsExactElementsInAnyOrder("The returned matches should match the expected collection", expected, actual);
		}
		#endregion

		#region Logging related tests

		public void TestGetMatchesReturnsLogs()
		{
			var expectedOutput = @"===== Begin Search Commodity Code For Tariff ID =====
--- Begin Search for Client Rate Headers ---
--- Getting exact Commodity Match ---
Found 0 pairs:

	RatingHeader Found Client Rate NEWTESSYD Entries: 1
	RateEntry Filtered Client Rate NEWTESSYD reason: Commodity Code didn't match job BBB.
--- Getting child Commodity Only Match ---
Found 1 pairs:
Commodity Code: BBB2, FMC Tariff ID: 1234, Rate Source: CLR
	RatingHeader Found Client Rate NEWTESSYD Entries: 1
--- Getting sibling Commodity Only Match ---
Found 0 pairs:

	RatingHeader Found Client Rate NEWTESSYD Entries: 1
--- Found Matches in Client Rate Headers ---
Client Matches Found: 1
--- Begin Search for Company Tariff Headers ---
--- Getting exact Commodity Match ---
Found 1 pairs:
Commodity Code: BBB, FMC Tariff ID: 1111, Rate Source: CTR
	RatingHeader Found Base Company Tariff Entries: 3
	RateEntry Filtered Base Company Tariff reason: Commodity Code didn't match job BBB.
--- Getting child Commodity Only Match ---
Found 1 pairs:
Commodity Code: BBB1, FMC Tariff ID: 1234, Rate Source: CTR
	RatingHeader Found Base Company Tariff Entries: 3
	RateEntry Filtered Base Company Tariff reason: Commodity Code didn't match job BBB1,BBB2. (x2)
--- Getting sibling Commodity Only Match ---
Found 0 pairs:

	RatingHeader Found Base Company Tariff Entries: 3
	RateEntry Filtered Base Company Tariff reason: Commodity Code didn't match job BBB2. (x3)
--- Found Matches in Company Tariff Headers ---
Company Matches Found: 2
===== Found a total of 3 distinct pairs =====";

			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);

			var companyTariffLevel = Helper.NewCompanyTariff().LevelOneTariff;
			var rate1 = CreateRate(companyTariffLevel, "BBB");
			rate1.TI_FMCTariffID = "1234";
			rate1.TI_RH_NKCommodityCode = "BBB";

			var rate2 = companyTariffLevel.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", container: "20GP");
			rate2.TI_RH_NKCommodityCode = "BBB";
			rate2.TI_FMCTariffID = "1111";
			companyTariffLevel.Factory.Save();

			var rate3 = companyTariffLevel.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", container: "20GP");
			rate3.TI_RH_NKCommodityCode = "BBB1";
			rate3.TI_FMCTariffID = "1234";
			companyTariffLevel.Factory.Save();

			var clientRate = Helper.NewClientRate(NewClient);
			var clientRateEntry = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", container: "20GP");
			clientRateEntry.TI_RH_NKCommodityCode = "BBB2";
			clientRateEntry.TI_FMCTariffID = "1234";
			clientRate.Factory.Save();

			var criteria = GetCriteria(NewClient, "BBB");
			var provider = new RateCommodityFMCPairProvider(companyTariffLevel.Factory, FMCTestLogger);
			provider.GetMatches(criteria);

			AssertEquals(expectedOutput, FMCTestLogger.ToString());
		}

		#endregion

		#region Company tariff related tests

		public void TestGetMatch_AcrossCompanyTariffLevels()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "AAA", },
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);

			var companyTariffLevel1 = Helper.NewCompanyTariff();
			CreateRate(companyTariffLevel1, "BBB1");

			var criteria = GetCriteria(NewClient, "BBB", tariffLevel: 1);
			var provider = new RateCommodityFMCPairProvider(Factory, FMCTestLogger);
			var result1 = provider
				.GetMatches(criteria)
				.Select(x => x.CommodityCode)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"The matched commodities at tariff level 1 should be as expected.",
				new[] { "BBB1" },
				result1
			);

			var companyTariffLevel2 = Helper.NewCompanyTariff();
			companyTariffLevel2.TH_GlobalRateLevel = 2;
			companyTariffLevel2.Factory.Save();
			criteria = GetCriteria(NewClient, "BBB", tariffLevel: 2);
			var result2 = provider
				.GetMatches(criteria)
				.Select(x => x.CommodityCode)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"The matched commodities at tariff level 2 should be as expected.",
				new[] { "BBB1" },
				result2
			);

			criteria = GetCriteria(NewClient, "BBB", tariffLevel: 3);
			var result3 = provider
				.GetMatches(criteria)
				.Select(x => x.CommodityCode)
				.ToArray();

			AssertEquals(
				"There should be no matched commodities at tariff level 3.",
				0,
				result3.Length
			);
		}

		public void TestGetMatch_CompanyTariff_DifferentDirectionAndIncoterm_MatchFound()
		{
			CreateCommodityAndRatingCodesForFMCTest(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1" } }
			);

			var criteria = GetCriteria(NewClient, "BBB", origin: "NZAKL", destination: "AUSYD", tariffLevel: 1);
			AssertEquals("criteria.Direction", OrgRateTariffLevel.Directions.IMP, criteria.Direction);

			NewClient.CompanyData.RateTariffLevels.RemoveAndDeleteAll();
			var rateTariffLevel = NewClient.CompanyData.RateTariffLevels.AddNew();
			rateTariffLevel.P7_Mode = "ALL";
			rateTariffLevel.P7_TariffType = "DEF";
			rateTariffLevel.P7_Direction = "IMP";
			rateTariffLevel.P7_TariffLevel = 1;

			var companyTariffLevel = Helper.NewCompanyTariff().LevelOneTariff;
			var originRateEntry =
				CreateRate(companyTariffLevel, "BBB", origin: "NZ", destination: "", otherSetters: (x) =>
				{
					x.TI_RateCategory = RatingConstants.RateCategory.ORG;
					x.RateLines.RemoveAndDeleteAll();
					x.AddFlatRateLine("ODOC", 100);
				});
			var destinationRateEntry =
				CreateRate(companyTariffLevel, "BBB1", origin: "", destination: "AU", otherSetters: (x) =>
				{
					x.TI_RateCategory = RatingConstants.RateCategory.DST;
					x.RateLines.RemoveAndDeleteAll();
					x.AddFlatRateLine("DDOC", 100);
				});

			var provider = new RateCommodityFMCPairProvider(Factory, FMCTestLogger);
			var actualCommodities = provider
				.GetMatches(criteria)
				.Select(x => x.CommodityCode)
				.ToArray();

			var expectedCommodities = new[] { "BBB", "BBB1" };

			AssertContainsExactElementsInAnyOrder(
				"The returned commodity codes should match the expected set of codes.",
				expectedCommodities,
				actualCommodities
			);
		}

		public void TestGetMatch_CompanyTariff_DifferentPaymentTerm_MatchFound()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB" }
			);

			var criteria = GetCriteria(NewClient, "BBB", tariffLevel: 1);
			using (criteria.TemporarilyAllowCriteriaChanging())
			{
				criteria.PaymentTermOverride = "PPD";
			}

			var companyTariffLevel = Helper.NewCompanyTariff().LevelOneTariff;
			var rateEntry = CreateRate(companyTariffLevel, "BBB", otherSetters: (x) =>
			{
				x.TI_PaymentTerm = "CCX";
			});

			var provider = new RateCommodityFMCPairProvider(Factory, FMCTestLogger);
			var actual = provider
				.GetMatches(criteria)
				.Select(x => x.CommodityCode)
				.ToArray();

			var expected = new[] { "BBB" };

			AssertContainsExactElementsInAnyOrder(
				"The matched commodity codes should match the expected values.",
				expected,
				actual
			);
		}

		public void TestGetMatch_Incoterms()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "HAZ", childCommodities = new[] { "ALC", "BEER" } }
			);

			var companyTariffLevel = Helper.NewCompanyTariff();
			var companyTariffCollection = companyTariffLevel.FCLRateEntriesForBinding;
			var org1 = Helper.NewOrgHeader();

			var clientRate = Helper.NewClientRate(org1);

			// FCL
			var alcFCL = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", container: "20GP");
			alcFCL.TI_RH_NKCommodityCode = "ALC";
			alcFCL.TI_FMCTariffID = "";
			alcFCL.TI_PL_NKCarrierServiceLevel = "STD";

			var haz001FCL = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", container: "20GP");
			haz001FCL.TI_RH_NKCommodityCode = "HAZ";
			haz001FCL.TI_FMCTariffID = "001";
			haz001FCL.TI_PL_NKCarrierServiceLevel = "STD";

			var beerFCL = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", container: "20GP");
			beerFCL.TI_RH_NKCommodityCode = "BEER";
			beerFCL.TI_FMCTariffID = "";
			beerFCL.TI_PL_NKCarrierServiceLevel = "STD";

			// ORG
			var beerORG = clientRate.AddRateEntry("ORG", "SEA", "AUSYD", "NZAKL", "STD", container: "20GP");
			beerORG.AddRateLine("ODOC", FlatCalculator.Code);
			beerORG.TI_RH_NKCommodityCode = "BEER";
			beerORG.TI_FMCTariffID = "";
			beerORG.TI_PL_NKCarrierServiceLevel = "STD";

			var beer002ORG = clientRate.AddRateEntry("ORG", "SEA", "AUSYD", "NZAKL", "STD", container: "20GP");
			beer002ORG.AddRateLine("ODOC", FlatCalculator.Code);
			beer002ORG.TI_RH_NKCommodityCode = "BEER";
			beer002ORG.TI_FMCTariffID = "002";
			beer002ORG.TI_PL_NKCarrierServiceLevel = "STD";

			var alc003ORG = clientRate.AddRateEntry("ORG", "SEA", "AUSYD", "NZAKL", container: "20GP");
			alc003ORG.AddRateLine("ODOC", FlatCalculator.Code);
			alc003ORG.TI_RH_NKCommodityCode = "ALC";
			alc003ORG.TI_FMCTariffID = "003";
			alc003ORG.TI_PL_NKCarrierServiceLevel = "STD";

			clientRate.Factory.Save();

			// Different Client Rate FCL
			var org2 = Helper.NewOrgHeader(2);
			var clientRate2 = Helper.NewClientRate(org2);

			var beer002FCL = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", container: "20GP");
			beer002FCL.TI_RH_NKCommodityCode = "BEER";
			beer002FCL.TI_FMCTariffID = "003";
			beer002FCL.TI_PL_NKCarrierServiceLevel = "STD";

			clientRate2.Factory.Save();

			var criteriaCFR = GetCriteria(org1, "HAZ");
			var criteriaFOB = GetCriteriaFOB(org1, "HAZ");

			var provider = new RateCommodityFMCPairProvider(Factory, FMCTestLogger);

			var expectedResults = new[]
			{
				"ALC|003|CLR",
				"ALC||CLR",
				"BEER|002|CLR",
				"BEER|003|CLR",
				"HAZ|001|CLR"
			};

			var cfrResults = provider
				.GetMatches(criteriaCFR)
				.Select(x => $"{x.CommodityCode}|{x.FMCTariffID}|{x.RateSource}")
				.ToList();

			var fobResults = provider
				.GetMatches(criteriaFOB)
				.Select(x => $"{x.CommodityCode}|{x.FMCTariffID}|{x.RateSource}")
				.ToList();

			AssertContainsExactElementsInAnyOrder("The CFR results should match the expected collection", expectedResults, cfrResults);
			AssertContainsExactElementsInAnyOrder("The FOB results should match the expected collection", expectedResults, fobResults);
		}

		#endregion

		#region Helpers

		RatingCriteria GetCriteria(OrgHeader organisation, ZString commodity, string origin = "AUSYD", string destination = "NZAKL", int tariffLevel = 1, IAutoRatingChargeInfo[] testCharges = null)
		{
			var testObject = new AutoRatingObject(origin, destination, FreightMode.FCL, new TestContainers(Factory, "20GP", 3), 0m, 0m, organisation);
			var autoRatingProxy = new TestAutoratingProxy(testObject, tariffLevel, testCharges);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (criteria.TemporarilyAllowCriteriaChanging())
			{
				criteria.OverriddenCommodity = new[] { Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, commodity) };
				criteria.PaymentTerm = new PaymentTermInfos();
				criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "CFR"));
				criteria.RateableMeasures.SetVolumeWithCommodity(1, "CC", commodity);
			}

			return criteria;
		}

		RatingCriteria GetCriteriaFOB(OrgHeader organisation, ZString commodity, string origin = "AUSYD", string destination = "NZAKL", int tariffLevel = 1)
		{
			var testObject = new AutoRatingObject(origin, destination, FreightMode.FCL, new TestContainers(Factory, "20GP", 3), 0m, 0m, organisation);
			var autoRatingProxy = new TestAutoratingProxy(testObject, tariffLevel);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (criteria.TemporarilyAllowCriteriaChanging())
			{
				criteria.OverriddenCommodity = new[] { Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, commodity) };
				criteria.PaymentTerm = new PaymentTermInfos();
				criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "FOB"));
				criteria.RateableMeasures.SetVolumeWithCommodity(1, "CC", commodity);
			}

			return criteria;
		}

		void CreateCommodityAndRatingCodesForFMCTest(params RatingCode[] ratingCodes)
		{
			var createdCommodities = new HashSet<string>();
			foreach (var ratingCode in ratingCodes)
			{
				Helper.NewCommodity(ratingCode.commodity, "FRT");
				createdCommodities.Add(ratingCode.commodity);
				foreach (var child in ratingCode.childCommodities)
				{
					Helper.NewCommodity(child, "FRT");
					Helper.NewCommodityRatingCode(ratingCode.commodity, child);
				}
			}
			Factory.Save();
		}

		RateEntry CreateRate(RatingHeader ratingHeader, string commodity, string origin = "AU", string destination = "NZ", Action<RateEntry> otherSetters = null)
		{
			var entry = ratingHeader.AddRateEntry("FCL", "SEA", origin, destination, container: "20GP");
			entry.TI_RH_NKCommodityCode = commodity;
			otherSetters?.Invoke(entry);
			ratingHeader.Factory.Save();

			return entry;
		}

		RateCommodityFMCPairLogger FMCTestLogger { get; } = new RateCommodityFMCPairLogger();

		class TestAutoratingProxy : AutoRatingProxy
		{
			readonly int tariffLevel;
			readonly IAutoRatingChargeInfo[] testCharges;

			public TestAutoratingProxy(IAutoRating autoRating, int tariffLevel, IAutoRatingChargeInfo[] testCharges = null)
			: base(autoRating)
			{
				this.tariffLevel = tariffLevel;
				this.testCharges = testCharges ?? Array.Empty<IAutoRatingChargeInfo>();
			}

			public override int TariffLevel
			{
				get
				{
					return tariffLevel;
				}
			}

			public override IAutoRatingChargeInfo[] GetExistingCharges(bool fromAllCompanies = false)
			{
				return testCharges;
			}
		}

		class RatingCode
		{
			public string[] childCommodities = Array.Empty<string>();
			public string commodity;
		}

		#endregion
	}
}


