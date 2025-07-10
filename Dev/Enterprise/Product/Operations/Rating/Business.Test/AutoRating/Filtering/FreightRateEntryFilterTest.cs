using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class FreightRateEntryFilterTest : RatingTestCase
	{
		#region Possible Matches

		public void TestLocationIsSimilar()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, GP20, testRate.Header);
			var matcher = new FreightRateEntryFilter(testCriteria, false, Factory, new DummyLogger());

			AssertEquals(true, matcher.LocationIsSimilar("AUSYD", "AUMEL"));
			AssertEquals(false, matcher.LocationIsSimilar("AUSYD", "USLAX"));
			AssertEquals(true, matcher.LocationIsSimilar("AUSYD", "OUSYD"));
			AssertEquals(false, matcher.LocationIsSimilar("AUSYD", "OOSYD"));
			AssertEquals(false, matcher.LocationIsSimilar("AUSYD", "AUSY"));
			AssertEquals(true, matcher.LocationIsSimilar("AUSYD", "ausyd"));
		}

		public void TestLocationMatchingCaseInsensitive()
		{
			InsertRefUnloco("nzxxx", "nz");
			InsertRefUnloco("inyyy", "in");

			var collection = new List<IRatingHeader>();
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			collection.Add(testRate);

			var entry1 = testRate.AddRateEntry("AIR", "LSE", "NZXXX", "INYYY");
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

			var testCriteria = new TestRatingCriteria("nzxxx", "inyyy", FreightMode.LSE, 15M, 1M, testRate.Header);

			var results = Filter(collection, testCriteria, false);
			Assert(results.Any());
		}

		public void TestIsPossileMatchCaseInsensitive()
		{
			InsertRefUnloco("nzxxx", "nz");
			InsertRefUnloco("inyyy", "in");
			InsertRefUnloco("inzzz", "in");

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = testRate.AddRateEntry("AIR", "LSE", "NZXXX", "INZZZ");
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

			var testCriteria = new TestRatingCriteria("nzxxx", "inyyy", FreightMode.LSE, 15M, 1M, testRate.Header);
			var filter = new FreightRateEntryFilter(testCriteria, false, Factory, new DummyLogger());

			Assert(filter.IsPossibleMatch(entry1));
		}

		void InsertRefUnloco(string code, string countryCode)
		{
			Db.Connection.ExecuteNonQuery(string.Format("Insert into dbo.RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode) values ('{0}', '{1}', '{2}') ", Guid.NewGuid(), code, countryCode));
		}

		#endregion

		#region Cross-Trade

		public void TestCrossTradeRating()
		{
			Converter<RateEntry, string> displayTextProvider = x => x.TI_OriginLRC + "-" + x.TI_DestinationLRC;

			var collection = new List<IRatingHeader>();
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			collection.Add(testRate);

			var entry1 = testRate.AddRateEntry("AIR", "LSE", "NZAKL", "INBOM");
			var entry2 = testRate.AddRateEntry("AIR");
			entry2.TI_IsCrossTrade = true;
			var entry5 = testRate.AddRateEntry("AIR", "LSE", "USLAX", "INBOM");

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

			var testCriteria = new TestRatingCriteria("NZAKL", "INBOM", FreightMode.LSE, 15M, 1M, testRate.Header);
			testCriteria.JobDirection = Directions.CrossTrade;
			var results = Filter(collection, testCriteria, false, Factory);

			AssertContainsExactElementsInAnyOrder(displayTextProvider, new[] { entry1, entry2 }, results.Select(x => x as RateEntry));

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.NewZealand);

			testCriteria = new TestRatingCriteria("NZAKL", "INBOM", FreightMode.LSE, 15M, 1M, testRate.Header);
			testCriteria.JobDirection = Directions.CrossTrade;
			var logger = new ElementaryLogger();
			results = Filter(collection, testCriteria, false, logger: logger);

			AssertContainsExactElementsInAnyOrder("Cross-trade rate does not exist", displayTextProvider, new[] { entry1 }, results.Select(x => x as RateEntry));
			AssertCollectionContains(
				"RateEntry Filtered Client Rate TESTORG1 reason: Company EDI is in the same country with NZ or IN.",
				logger.GetAllLogs());
		}

		public void TestCrossTradeRating_InvalidJobLocation()
		{
			var collection = new List<IRatingHeader>();
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			collection.Add(testRate);

			var entry1 = testRate.AddRateEntry("AIR");
			entry1.TI_IsCrossTrade = true;

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.NewZealand);

			var testCriteria = new TestRatingCriteria(origin: null, destination: "INBOM", FreightMode.LSE, 15M, 1M, testRate.Header);
			testCriteria.JobDirection = Directions.CrossTrade;
			var logger = new ElementaryLogger();
			Filter(collection, testCriteria, false, logger: logger);

			AssertCollectionContains(
				"RateEntry Filtered Client Rate TESTORG1 reason: Invalid location is identified on job. Unable to match location.",
				logger.GetAllLogs());
		}

		#endregion

		#region Transhipment

		public void TestTranshipmentLegRating()
		{
			var collection = new List<IRatingHeader>();
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			collection.Add(testRate);

			var entry1 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "INBOM");
			entry1.TI_ViaLRC = "SGSIN";
			var entry2 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "INBOM");
			var entry3 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "SGSIN");
			var entry4 = testRate.AddRateEntry("AIR", "LSE", "SGSIN", "INBOM");

			var testCriteria = new TestRatingCriteria("AUSYD", "INBOM", FreightMode.LSE, 15M, 1M, testRate.Header);
			testCriteria.SetVia(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN"));

			// Transhipment journey match
			var results = Filter(collection, testCriteria, false, Factory);

			AssertEquals(2, results.Count);
			Assert(results.Contains(entry1));
			Assert(results.Contains(entry2));

			// Direct match
			entry1.Delete();
			results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("1 entry found", 1, results.Count);
			AssertEquals("Exact direct journey entry used", entry2, results[0]);

			// Transhipment Leg match
			entry2.Delete();
			results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("2 entries found - 1 for each leg", 2, results.Count);
			AssertEquals("Transhipment Leg 2 Entry", results[0], entry3);
			AssertEquals("Transhipment Leg 1 Entry", results[1], entry4);

			entry4.Delete();
			results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("No entries found - only 1 leg existed", 0, results.Count);
		}

		public void TestTranshipmentLegRatingWithCosts()
		{
			var collection = new List<IRatingHeader>();
			var testCost = Helper.NewCosting(Helper.NewOrgHeader());
			collection.Add(testCost);

			var entry1 = testCost.AddRateEntry("AIR", "LSE", "AUSYD", "INBOM");
			entry1.TI_ViaLRC = "SGSIN";
			var entry2 = testCost.AddRateEntry("AIR", "LSE", "AUSYD", "INBOM");
			var entry3 = testCost.AddRateEntry("AIR", "LSE", "AUSYD", "SGSIN");
			var entry4 = testCost.AddRateEntry("AIR", "LSE", "SGSIN", "INBOM");

			var testCriteria = new TestRatingCriteria("AUSYD", "INBOM", FreightMode.LSE, 15M, 1M, Helper.NewOrgHeader());
			testCriteria.SetVia(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN"));
			testCriteria.Carrier = testCost.Header;

			// Transhipment journey match
			var results = Filter(collection, testCriteria, true, Factory);
			AssertEquals(2, results.Count);
			AssertEquals(true, results.Contains(entry1));
			AssertEquals(true, results.Contains(entry2));

			// Direct match
			entry1.Delete();
			results = Filter(collection, testCriteria, true, Factory);
			AssertEquals("1 entry found", 1, results.Count);
			AssertEquals("Exact direct journey entry used", entry2, results[0]);

			// Transhipment Leg match
			entry2.Delete();
			results = Filter(collection, testCriteria, true, Factory);
			AssertEquals("2 entries found - 1 for each leg", 2, results.Count);
			AssertEquals("Transhipment Leg 2 Entry", results[0], entry3);
			AssertEquals("Transhipment Leg 1 Entry", results[1], entry4);

			entry4.Delete();
			results = Filter(collection, testCriteria, true, Factory);
			AssertEquals("No entries found - only 1 leg existed", 0, results.Count);
		}

		public void TestTranshipmentLegRatingWithCompanyTariff()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var line1 = testRate.AddRateEntry("AIR", "LSE", "SGSIN", "INBOM").RateLines[0];
			line1.TL_RateCalculator = UnitCalculator.Code;
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var tariff = Helper.NewCompanyTariff();
			var line2 = tariff.AddRateEntry("AIR", "LSE", "AUSYD", "SGSIN").RateLines[0];
			line2.TL_RateCalculator = UnitCalculator.Code;
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)4m;
			tariff.Factory.Save();

			Factory.Save();

			var testCriteria = new TestRatingCriteria("AUSYD", "INBOM", FreightMode.LSE, 200M, 1M, testRate.Header);
			testCriteria.SetVia(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN"));
			testCriteria.JobDirection = Directions.Export;

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(testCriteria, CostSell.Revenue);

			var expected = new[]
			{
#pragma warning disable 618
				new SimpleArInfo
#pragma warning restore 618
					{
						Amount = 800m,
						InvoiceLineDesc = "International Freight",
						CalculationSingleLineDescription = "FRT: 200 Kilogram(s) @ AUD 4.00/KG"
					},
#pragma warning disable 618
				new SimpleArInfo
#pragma warning restore 618
					{
						Amount = 1000m,
						InvoiceLineDesc = "International Freight",
						CalculationSingleLineDescription = "FRT: 200 Kilogram(s) @ SGD 5.00/KG"
					},
			};

			AssertRatingResults(expected, results);
		}

		#endregion

		#region Container IsNonOperatingReefer

		public void TestIsNonOperatingReeferRating()
		{
			var container1 = Factory.New<RefContainer>();
			container1.RC_FreightRateClass = "ZUB";
			container1.RC_Code = "Z1";

			var container2 = Factory.New<RefContainer>();
			container2.RC_FreightRateClass = "ZUB";
			container2.RC_Code = "Z2";

			var container3 = Factory.New<RefContainer>();
			container3.RC_Code = "Z3";

			Factory.Save();

			var collection = new List<IRatingHeader>();
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			collection.Add(testRate);

			var entry1 = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", container1.RC_Code);
			var entry2 = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", container2.RC_Code);
			testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", container3.RC_Code);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, container1, testRate.Header);
			var results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("1 matching entry", 1, results.Count);
			AssertEquals("Matched Entry", entry1, results[0]);

			entry2.TI_MatchContainerRateClass = true;
			testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, container1, testRate.Header);
			results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("2 matching entries", 2, results.Count);
			Assert("Matched Entry 1", results.Contains(entry1));
			Assert("Matched Entry 2", results.Contains(entry2));
		}

		#endregion

		#region Container Classes

		public void TestFindBestMatchSameContainerClass()
		{
			var container1 = Factory.New<RefContainer>();
			container1.RC_FreightRateClass = "ZUB";
			container1.RC_Code = "Z1";

			var container2 = Factory.New<RefContainer>();
			container2.RC_FreightRateClass = "ZUB";
			container2.RC_Code = "Z2";

			var container3 = Factory.New<RefContainer>();
			container3.RC_Code = "Z3";

			Factory.Save();

			var collection = new List<IRatingHeader>();
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			collection.Add(testRate);

			var entry1 = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", container1.RC_Code);
			var entry2 = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", container2.RC_Code);
			testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", container3.RC_Code);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, container1, testRate.Header);
			var results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("1 matching entry", 1, results.Count);
			AssertEquals("Matched Entry", entry1, results[0]);

			entry2.TI_MatchContainerRateClass = true;
			testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, container1, testRate.Header);
			results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("2 matching entries", 2, results.Count);
			Assert("Matched Entry 1", results.Contains(entry1));
			Assert("Matched Entry 2", results.Contains(entry2));
		}

		[ExpectNoExceptions]
		public void TestFindBestMatch_ContainerFilter_NoExceptionThrown()
		{
			var container1 = Factory.New<RefContainer>();
			container1.RC_FreightRateClass = "ZUB";
			container1.RC_Code = "Z1";

			var container2 = Factory.New<RefContainer>();
			container2.RC_FreightRateClass = "ZUB";
			container2.RC_Code = "Z2";

			var collection = new List<IRatingHeader>();
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			collection.Add(testRate);

			testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", container1.RC_Code);
			testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", container2.RC_Code);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, null, testRate.Header);
			Filter(collection, testCriteria, false, Factory);
		}

		public void TestGetMatchesWithCorrectContainerTypeNoExact()
		{
			var container1 = Factory.New<RefContainer>();
			container1.RC_FreightRateClass = "ZUB";
			container1.RC_Code = "Z1";

			var container2 = Factory.New<RefContainer>();
			container2.RC_FreightRateClass = "ZUB";
			container2.RC_Code = "Z2";

			var container3 = Factory.New<RefContainer>();
			container3.RC_Code = "Z3";

			Factory.Save();

			var collection = new List<IRatingHeader>();
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			collection.Add(testRate);

			var entry1 = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", container2.RC_Code);
			testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", container3.RC_Code);

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, container1, testRate.Header);
			var results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("No matching entries", 0, results.Count);

			entry1.TI_MatchContainerRateClass = true;
			testCriteria = new TestRatingCriteria("AUSYD", "USLAX", 1, container1, testRate.Header);
			results = Filter(collection, testCriteria, false, Factory);
			AssertEquals("1 matching entry", 1, results.Count);
			AssertEquals("Matched Entry due to same container class", entry1, results[0]);
		}

		#endregion

		#region FilterContractNumber

		class ContractNumberTestHelper
		{
			public bool PopulateForwardingConsolContractNumbers { get; set; }

			public bool IgnoreAndReplaceContractNumbers { get; set; }

			public List<RateInfo> RateEntries { get; set; }

			public List<string> CriteriaContractNumbers { get; set; }

			public List<RateInfo> ResultRates { get; set; }
		}

		class RateInfo
		{
			public OrgHeader Provider { get; set; }
			public string RateCategory { get; set; }
			public string ContractNumber { get; set; }
		}

		// Note: As of WI00700002 - ORG & DST Conso Cost fallback from Contract No. to NO Contract No. doesn’t work, we don't filter ORG/DST entries early.
		// ContractNumberComparer can resolve duplicate charge codes, but it is not covered in this test.
		public void TestFilterByContractNumberWithDifferentRatesAndContractNumberShouldFilterRatesBasedOnRegistryAndCriteriaContractNumbers()
		{
			var contractNumberHelper = new ContractNumberTestHelper();
			contractNumberHelper.RateEntries = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};

			//WI00263373 Different Scenarios 13-16
			contractNumberHelper.PopulateForwardingConsolContractNumbers = false;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = false;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 17-20
			contractNumberHelper.PopulateForwardingConsolContractNumbers = false;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = true;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 21-24
			contractNumberHelper.PopulateForwardingConsolContractNumbers = true;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = false;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber =  "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 25-28
			contractNumberHelper.PopulateForwardingConsolContractNumbers = true;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = true;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 40-43
			contractNumberHelper.PopulateForwardingConsolContractNumbers = false;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = false;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 44-47
			contractNumberHelper.PopulateForwardingConsolContractNumbers = false;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = true;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 48-51
			contractNumberHelper.PopulateForwardingConsolContractNumbers = true;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = false;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 52-55
			contractNumberHelper.PopulateForwardingConsolContractNumbers = true;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = true;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 68-71
			contractNumberHelper.RateEntries = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "B67890" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};

			contractNumberHelper.PopulateForwardingConsolContractNumbers = false;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = false;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" },
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "B67890" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 72-75
			contractNumberHelper.PopulateForwardingConsolContractNumbers = false;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = true;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "B67890" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 76-79
			contractNumberHelper.PopulateForwardingConsolContractNumbers = true;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = false;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "B67890" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 80-83
			contractNumberHelper.PopulateForwardingConsolContractNumbers = true;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = true;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "B67890" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 96-99
			contractNumberHelper.PopulateForwardingConsolContractNumbers = false;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = false;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "B67890" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "B67890" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "B67890" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 100-103
			contractNumberHelper.PopulateForwardingConsolContractNumbers = false;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = true;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "B67890" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 104-107
			contractNumberHelper.PopulateForwardingConsolContractNumbers = true;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = false;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "B67890" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "B67890" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "B67890" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//WI00263373 Different Scenarios 108-111
			contractNumberHelper.PopulateForwardingConsolContractNumbers = true;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = true;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890", "" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "B67890" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "" };
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			contractNumberHelper.CriteriaContractNumbers = new List<string>();
			AssertRatesFilteredByContractNumbers(contractNumberHelper);

			//the below cases are for when carrier contract number filtering should not apply.
			// For these cases, registry settings does not matter. but we will leave them to confirm they don't affect the expected result
			contractNumberHelper.PopulateForwardingConsolContractNumbers = true;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = false;
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper: contractNumberHelper, shouldApplyContractNumberFilter: false);

			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "B67890" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper, shouldApplyContractNumberFilter: false);

			contractNumberHelper.RateEntries = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "B67890", "A12345" };
			contractNumberHelper.ResultRates = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = ""  },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = ""  },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = ""  },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = ""  },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = ""  },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = ""  }
			};
			AssertRatesFilteredByContractNumbers(contractNumberHelper, shouldApplyContractNumberFilter: false);

			contractNumberHelper.PopulateForwardingConsolContractNumbers = false;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = false;
			AssertRatesFilteredByContractNumbers(contractNumberHelper, shouldApplyContractNumberFilter: false);
		}

		ContractNumberTestHelper GetContractNumberHelper()
		{
			var contractNumberHelper = new ContractNumberTestHelper();
			contractNumberHelper.RateEntries = new List<RateInfo>
			{
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new () { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new () { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new () { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};

			contractNumberHelper.PopulateForwardingConsolContractNumbers = false;
			contractNumberHelper.IgnoreAndReplaceContractNumbers = false;

			return contractNumberHelper;
		}

		public void TestWhenCarrierContractNumberIsNOTBlankAndReferenceNumberCONHasEntryWithBlankValue_ShouldBringCostWithContractNumberAndBlank()
		{
			var contractNumberHelper = GetContractNumberHelper();
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345", ZString.Empty };

			contractNumberHelper.ResultRates = new List<RateInfo>()
			{
				new RateInfo { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new RateInfo { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new RateInfo { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new RateInfo { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new RateInfo { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};

			AssertRatesFilteredByContractNumbers(contractNumberHelper);
		}

		public void TestWhenCarrierContractNumberIsBlankAndReferenceNumberCONHasEntryWithBlankValue_ShouldBringCostWithOnlyBlankContract()
		{
			var contractNumberHelper = GetContractNumberHelper();
			contractNumberHelper.CriteriaContractNumbers = new List<string> { ZString.Empty };

			contractNumberHelper.ResultRates = new List<RateInfo>()
			{
				new RateInfo { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new RateInfo { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new RateInfo { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};

			AssertRatesFilteredByContractNumbers(contractNumberHelper);
		}

		public void TestWhenCarrierContractNumberIsNOTBlankAndReferenceNumberCONHasNoEntryWithBlankValue_ShouldBringCostWithContractNumber()
		{
			var contractNumberHelper = GetContractNumberHelper();
			contractNumberHelper.CriteriaContractNumbers = new List<string> { "A12345" };

			contractNumberHelper.ResultRates = new List<RateInfo>()
			{
				new RateInfo { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new RateInfo { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new RateInfo { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new RateInfo { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" },
			};

			AssertRatesFilteredByContractNumbers(contractNumberHelper);
		}

		public void TestWhenCarrierContractNumberIsBlankAndReferenceNumberCONHasNoEntryWithBlankValue_ShouldBringCostWithAnyBlankContractNumber()
		{
			var contractNumberHelper = GetContractNumberHelper();
			contractNumberHelper.CriteriaContractNumbers = Enumerable.Empty<string>().ToList();

			contractNumberHelper.ResultRates = new List<RateInfo>()
			{
				new RateInfo { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "A12345" },
				new RateInfo { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "B67890" },
				new RateInfo { Provider = TransportProvider1, RateCategory = "LCL", ContractNumber = "" },
				new RateInfo { Provider = TransportProvider1, RateCategory = "ORG", ContractNumber = "" },
				new RateInfo { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "A12345" },
				new RateInfo { Provider = TransportProvider2, RateCategory = "DST", ContractNumber = "" }
			};

			AssertRatesFilteredByContractNumbers(contractNumberHelper);
		}

		void AssertRatesFilteredByContractNumbers(ContractNumberTestHelper contractNumberHelper, bool shouldApplyContractNumberFilter = true)
		{
			using (FreightConfigurationRegistry.Instance.PopulateForwardingConsolContractNumbersIfBlankDuringAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, contractNumberHelper.PopulateForwardingConsolContractNumbers))
			using (FreightConfigurationRegistry.Instance.IgnoreAndReplaceCarrierContractNumbersDuringAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, contractNumberHelper.IgnoreAndReplaceContractNumbers))
			{
				var ratingHeaders = new List<RatingHeader>();
				var rateEntries = new List<RateEntry>();

				foreach (var info in contractNumberHelper.RateEntries)
				{
					var ratingHeader = ratingHeaders.SingleOrDefault(x => x.TH_OH == info.Provider.PK);
					if (ratingHeader == null)
					{
						ratingHeader = Helper.NewCosting(info.Provider);
						ratingHeaders.Add(ratingHeader);
					}

					var entry = ratingHeader.AddRateEntry(info.RateCategory, "LCL", "AU", "");
					entry.TI_ContractNumber = info.ContractNumber;
					rateEntries.Add(entry);
				}

				var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LCL, 100m, 5, NewClient);
				testCriteria.ShouldAddContractNumberQueryFilter = !contractNumberHelper.IgnoreAndReplaceContractNumbers;
				testCriteria.ShouldApplySpecificAdapterContractNumberFilter = shouldApplyContractNumberFilter;
				testCriteria.ShouldIgnoreJobCarrierContractNumbers = contractNumberHelper.IgnoreAndReplaceContractNumbers;
				testCriteria.ShouldMatchJobBlankContractNumber = false;

				foreach (var criteriaContractNumber in contractNumberHelper.CriteriaContractNumbers)
				{
					testCriteria.SetCarrierContractNumber(criteriaContractNumber);
				}

				var actualRates = Filter(ratingHeaders, testCriteria, true, Factory);
				var expectedRates = rateEntries.Where(rate => contractNumberHelper.ResultRates.Any(r => r.Provider.PK == rate.Parent.TH_OH && r.RateCategory == rate.TI_RateCategory && r.ContractNumber == rate.TI_ContractNumber)).ToArray();

				AssertContainsExactElementsInAnyOrder("applied rates should be equal to expected rates.", expectedRates.Select(ToString), actualRates.Select(ToString));

				string ToString(IRateEntry entry)
				{
					var builder = new ZStringBuilder();
					builder
						.Append(entry.TI_RateCategory)
						.Append(entry.TI_ContractNumber);
					return builder.ToStringWithDelimiterBetweenAppends("|");
				}
			}
		}

		#endregion

		#region Filter by ShipmentConsolidationStatus

		public void TestFilterByShipmentConsolidationStatus_NonForwardingShipment()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var ratingHeader = Helper.NewCosting(null);
			var entryNormal = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, origin, destination, "BAF", 100);
			entryNormal.TI_ShipmentConsolidationStatus = "";
			var entrySTS = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, origin, destination, "CAF", 200);
			entrySTS.TI_ShipmentConsolidationStatus = "STS";
			var entryCNS = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, origin, destination, "WAR", 300);
			entryCNS.TI_ShipmentConsolidationStatus = "CNS";

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LCL, 100m, 5, NewClient);
			testCriteria.SetShipmentConsolidationStatus("");

			var actualRates = Filter(new[] { ratingHeader }, testCriteria, true, Factory);
			AssertContainsExactElementsInAnyOrder(new[] { entryNormal }, actualRates);
		}

		public void TestFilterByShipmentConsolidationStatus_StandaloneShipment()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var ratingHeader = Helper.NewCosting(null);
			var entryNormal = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, origin, destination, "BAF", 100);
			entryNormal.TI_ShipmentConsolidationStatus = "";
			var entrySTS = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, origin, destination, "CAF", 200);
			entrySTS.TI_ShipmentConsolidationStatus = "STS";
			var entryCNS = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, origin, destination, "WAR", 300);
			entryCNS.TI_ShipmentConsolidationStatus = "CNS";

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LCL, 100m, 5, NewClient);
			testCriteria.SetShipmentConsolidationStatus("STS");

			var actualRates = Filter(new[] { ratingHeader }, testCriteria, true, Factory);
			AssertContainsExactElementsInAnyOrder(new[] { entryNormal, entrySTS }, actualRates);
		}

		public void TestFilterByShipmentConsolidationStatus_ConsolidatedShipment()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var ratingHeader = Helper.NewCosting(null);
			var entryNormal = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, origin, destination, "BAF", 100);
			entryNormal.TI_ShipmentConsolidationStatus = "";
			var entrySTS = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, origin, destination, "CAF", 200);
			entrySTS.TI_ShipmentConsolidationStatus = "STS";
			var entryCNS = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, origin, destination, "WAR", 300);
			entryCNS.TI_ShipmentConsolidationStatus = "CNS";

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LCL, 100m, 5, NewClient);
			testCriteria.SetShipmentConsolidationStatus("CNS");

			var actualRates = Filter(new[] { ratingHeader }, testCriteria, true, Factory);
			AssertContainsExactElementsInAnyOrder(new[] { entryNormal, entryCNS }, actualRates);
		}

		#endregion

		#region FMCTariffID for CompanyTariffOnly

		public void TestCriteriaWithFMCTariffID_NotEmpty_FilterDisabled_RevenueAutorate()
		{
			var rateEntries = GetRatesForFMCTariffIDTests(
				entriesTariffIDs: new[] { "duck", "", "owls" },
				criteriaTariff: "duck",
				isCosting: false,
				criteriaTariffEnabled: false
			)
			.Select(x => $"{x.TI_FMCTariffID}|{x.IsCompanyTariff()}")
			.ToArray();

			var expectedRateEntries = new[]
			{
				// filter disabled, everything matches
				"duck|True",
				"owls|True",
				"|True",
				// client rate, matching disabled, all matches
				"duck|False",
				"owls|False",
				"|False",
				// quote rate, matching disabled, all matches
				"duck|False",
				"owls|False",
				"|False",
				// costing, matching disabled, all matches
				"duck|False",
				"owls|False",
				"|False",
			};

			AssertContainsExactElementsInAnyOrder(
				"Rate entries should match the expected collection",
				expectedRateEntries,
				rateEntries
			);
		}

		public void TestCriteriaWithFMCTariffID_NotEmpty_RevenueAutorate()
		{
			var rateEntries = GetRatesForFMCTariffIDTests(
				entriesTariffIDs: ["duck", "", "owls"],
				criteriaTariff: "duck",
				isCosting: false
			).Select(x => $"{x.TI_FMCTariffID}|{x.IsCompanyTariff()}")
			.ToArray();

			var expectedRateEntries = new[]
			{
				"duck|True",
				"|True",
				"duck|False",
				"|False",
				"duck|False",
				"|False",
				"duck|False",
				"owls|False",
				"|False"
			};

			AssertContainsExactElementsInAnyOrder(
				"Rate entries must match the expected collection",
				expectedRateEntries,
				rateEntries
			);
		}

		public void TestCriteriaWithFMCTariffID_Empty_RevenueAutorate()
		{
			var rateEntries = GetRatesForFMCTariffIDTests(
				entriesTariffIDs: new[] { "duck", "", "owls" },
				criteriaTariff: "",
				isCosting: false
			).Select(x => $"{x.TI_FMCTariffID}|{x.IsCompanyTariff()}")
			.ToArray();

			var expectedRateEntries = new[]
			{
				"|True", // criteria FMC is blank. company tariff: blank matches only blank
				"|False", // criteria FMC is blank. client rate: blank matches only blank
				"|False", // criteria FMC is blank. quote rate: blank matches only blank
				"duck|False", // costing, matching disabled, all matches
				"owls|False", // costing, matching disabled, all matches
				"|False", // costing, matching disabled, all matches
			};

			AssertContainsExactElementsInAnyOrder(
				"Company tariffs and client rates should match the expected rates.",
				expectedRateEntries,
				rateEntries
			);
		}

		public void TestCriteriaWithFMCTariffID_NotEmpty_CostAutorate()
		{
			var rateEntries = GetRatesForFMCTariffIDTests(
				entriesTariffIDs: new[] { "duck", "", "owls" },
				criteriaTariff: "duck",
				isCosting: true
			).Select(x => $"{x.TI_FMCTariffID}|{x.IsCompanyTariff()}")
			.ToArray();

			var expectedRateEntries = new[]
			{
				"duck|True",
				"owls|True",
				"|True",
				"duck|False",
				"owls|False",
				"|False",
				"duck|False",
				"owls|False",
				"|False",
				"duck|False",
				"owls|False",
				"|False"
			};

			AssertContainsExactElementsInAnyOrder(
				"Filtered rates should match the expected values when cost autorating.",
				expectedRateEntries,
				rateEntries
			);
		}

		public void TestCriteriaWithFMCTariffID_Empty_CostAutorate()
		{
			var rateEntries = GetRatesForFMCTariffIDTests(
				entriesTariffIDs: new[] { "duck", "", "owls" },
				criteriaTariff: "",
				isCosting: true
			).Select(x => $"{x.TI_FMCTariffID}|{x.IsCompanyTariff()}")
			.ToArray();

			var expectedEntries = new[]
			{
				"duck|True",
				"owls|True",
				"|True",
				"duck|False",
				"owls|False",
				"|False",
				"duck|False",
				"owls|False",
				"|False",
				"duck|False",
				"owls|False",
				"|False",
			};

			AssertContainsExactElementsInAnyOrder(
				"Rate entries should match the expected collection without filtering for FMCTariffID when cost-autorating.",
				expectedEntries,
				rateEntries
			);
		}

		List<IRateEntry> GetRatesForFMCTariffIDTests(string[] entriesTariffIDs, string criteriaTariff, bool isCosting, bool criteriaTariffEnabled = true)
		{
			var costingRate = Helper.NewCosting(null);
			var clientRate = Helper.NewClientRate(null);
			var quoteRate = Helper.NewQuote(null);
			var companyRate = Helper.NewCompanyTariff();

			CreateRateEntriesForTariffs(costingRate, entriesTariffIDs);
			CreateRateEntriesForTariffs(clientRate, entriesTariffIDs);
			CreateRateEntriesForTariffs(quoteRate, entriesTariffIDs);
			CreateRateEntriesForTariffs(companyRate, entriesTariffIDs);

			var testCriteria = new TestRatingCriteria("AUSYD", "NZAKL", FreightMode.LSE, 100m, 5, NewClient);
			testCriteria.FMCTariffID = criteriaTariff;
			testCriteria.FMCTariffIDMatchEnabled = criteriaTariffEnabled;

			var actualRates = Filter(
				new IRatingHeader[] { costingRate, companyRate, clientRate, quoteRate },
				testCriteria, isCosting: isCosting, Factory);

			return actualRates;
		}

		IEnumerable<RateEntry> CreateRateEntriesForTariffs(RatingHeader rate, string[] entriesTariffIDs)
		{
			var result = new List<RateEntry>();

			foreach (var tariffID in entriesTariffIDs)
			{
				var entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL");
				entry.TI_FMCTariffID = tariffID;
				result.Add(entry);
			}

			return result;
		}

		#endregion

		#region Filter by RateCategory

		void TestRateCategoryFilterBCNOrSCN(string message, bool isBCN, FreightMode criteriaFreightMode, Integration.RateType rateType, params string[] expectedRateCategories)
		{
			var testRate = SetupBCNOrSCNRateEntries(isBCN: isBCN);

			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue: isBCN))
			{
				var testCriteria = new TestRatingCriteria("AUSYD", "INBOM", criteriaFreightMode, 15M, 1M, testRate.Header);
				testCriteria.RateTypeToUse = rateType;

				var actualRateCategories = Filter(new[] { testRate }, testCriteria, false)
					.Select(x => x.TI_RateCategory.ToString())
					.ToArray();

				AssertContainsExactElementsInAnyOrder(message, expectedRateCategories, actualRateCategories);
			}
		}

		public void TestRateCategoryFilter_BCN_SEACategories_ShouldLoadSEAEntries()
		{
			TestRateCategoryFilterBCNOrSCN(
				message: "FCL mode should load FCL, LCL, and origin/destination entries",
				isBCN: true,
				criteriaFreightMode: FreightMode.FCL | FreightMode.BCN,
				rateType: Integration.RateType.Forwarding,
				expectedRateCategories: new[] { "FCL", "LCL", "DST", "ORG" });
			TestRateCategoryFilterBCNOrSCN(
				message: "LCL mode should load FCL, LCL, and origin/destination entries",
				isBCN: true,
				criteriaFreightMode: FreightMode.LCL | FreightMode.BCN,
				rateType: Integration.RateType.Forwarding,
				expectedRateCategories: new[] { "FCL", "LCL", "DST", "ORG" });
		}

		public void TestRateCategoryFilter_BCN_AIRCategory_ShouldLoadAIREntries()
		{
			TestRateCategoryFilterBCNOrSCN(
				message: "LSE mode should load AIR and origin/destination entries",
				isBCN: true,
				criteriaFreightMode: FreightMode.LSE | FreightMode.BCN,
				rateType: Integration.RateType.Forwarding,
				expectedRateCategories: new[] { "AIR", "DST", "ORG" });
		}

		public void TestRateCategoryFilter_BCN_LinerAndAgencyCategory_ShouldLoadLinerAndAgencyEntries()
		{
			TestRateCategoryFilterBCNOrSCN(
				message: "Containerized mode should load SCO, SNC, and origin/destination entries",
				isBCN: true,
				criteriaFreightMode: FreightMode.SEA | FreightMode.BCN,
				rateType: Integration.RateType.Shipping,
				expectedRateCategories: new[] { "SCO", "SNC", "SDE", "SOR" });

			TestRateCategoryFilterBCNOrSCN(
				message: "Non-containerized mode should load SCO, SNC, and origin/destination entries",
				isBCN: true,
				criteriaFreightMode: FreightMode.LCL | FreightMode.BCN,
				rateType: Integration.RateType.Shipping,
				expectedRateCategories: new[] { "SCO", "SNC", "SDE", "SOR" });
		}

		public void TestRateCategoryFilter_SCN_SEACategories_ShouldLoadSEAEntries()
		{
			TestRateCategoryFilterBCNOrSCN(
				message: "FCL mode should load FCL, LCL, and origin/destination entries",
				isBCN: false,
				criteriaFreightMode: FreightMode.FCL | FreightMode.SCN,
				rateType: Integration.RateType.Forwarding,
				expectedRateCategories: new[] { "FCL", "LCL", "DST", "ORG" });
			TestRateCategoryFilterBCNOrSCN(
				message: "LCL mode should load FCL, LCL, and origin/destination entries",
				isBCN: false,
				criteriaFreightMode: FreightMode.LCL | FreightMode.SCN,
				rateType: Integration.RateType.Forwarding,
				expectedRateCategories: new[] { "FCL", "LCL", "DST", "ORG" });
		}

		public void TestRateCategoryFilter_SCN_AIRCategory_ShouldLoadAIREntries()
		{
			TestRateCategoryFilterBCNOrSCN(
				message: "LSE mode should load AIR and origin/destination entries",
				isBCN: false,
				criteriaFreightMode: FreightMode.LSE | FreightMode.SCN,
				rateType: Integration.RateType.Forwarding,
				expectedRateCategories: new[] { "AIR", "DST", "ORG" });
		}

		public void TestRateCategoryFilter_SCN_LinerAndAgencyCategory_ShouldLoadLinerAndAgencyEntries()
		{
			TestRateCategoryFilterBCNOrSCN(
				message: "Containerized mode should load SCO, SNC, and origin/destination entries",
				isBCN: false,
				criteriaFreightMode: FreightMode.SEA | FreightMode.SCN,
				rateType: Integration.RateType.Shipping,
				expectedRateCategories: new[] { "SCO", "SNC", "SDE", "SOR" });

			TestRateCategoryFilterBCNOrSCN(
				message: "Non-containerized mode should load SCO, SNC, and origin/destination entries",
				isBCN: false,
				criteriaFreightMode: FreightMode.LCL | FreightMode.SCN,
				rateType: Integration.RateType.Shipping,
				expectedRateCategories: new[] { "SCO", "SNC", "SDE", "SOR" });
		}

		ClientRate SetupBCNOrSCNRateEntries(bool isBCN = true)
		{
			var client = Helper.NewOrgHeader();
			var testRate = Helper.NewClientRate(client);

			var testMode = isBCN ? "BCN" : "SCN";
			testRate.AddRateEntry("ORG", testMode, "AUSYD", "INBOM"); // Forwarding Origin
			testRate.AddRateEntry("DST", testMode, "AUSYD", "INBOM"); // Forwarding Destination
			testRate.AddRateEntry("AIR", testMode, "AUSYD", "INBOM"); // Forwarding Freight
			testRate.AddRateEntry("FCL", testMode, "AUSYD", "INBOM"); // Forwarding Freight
			testRate.AddRateEntry("LCL", testMode, "AUSYD", "INBOM"); // Forwarding Freight
			testRate.AddRateEntry("SCO", testMode, "AUSYD", "INBOM"); // Shipping Freight
			testRate.AddRateEntry("SNC", testMode, "AUSYD", "INBOM"); // Shipping Freight
			testRate.AddRateEntry("SOR", testMode, "AUSYD", "INBOM"); // Shipping Origin
			testRate.AddRateEntry("SDE", testMode, "AUSYD", "INBOM"); // Shipping Destination

			return testRate;
		}

		#endregion

		protected List<IRateEntry> Filter(
			IEnumerable<IRatingHeader> ratingHeaders,
			RatingCriteria criteria,
			bool isCosting,
			BusinessObjectFactory factory = null,
			ILogger logger = null,
			FreightAutoRater.RatesToFindEnum ratesToFind = FreightAutoRater.RatesToFindEnum.ActiveRates)
		{
			if (isCosting)
			{
				var loader = new CostRatesLoader(criteria.Factory, logger);
				var rates = loader.Load(criteria, ratingHeaders);
				return RateEntryFilter.Filter(criteria, true, rates, factory ?? Factory, logger ?? new DummyLogger(), ratesToFind).ToList();
			}
			else
			{
				var loader = new RevenueRatesLoader(criteria.Factory, logger);
				var rates = loader.Load(criteria, ratingHeaders);
				return RateEntryFilter.Filter(criteria, false, rates, factory ?? Factory, logger ?? new DummyLogger(), ratesToFind).ToList();
			}
		}
	}
}
